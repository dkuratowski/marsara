using System;
using System.Collections.Generic;
using System.Threading;
using RC.Common.Diagnostics;

namespace RC.Common
{
    /// <summary>
    /// Stores a number of worker threads that are waiting for tasks to execute concurrently.
    /// This class is a simplified version of System.Threading.ThreadPool class.
    /// </summary>
    public class WorkerNest : IDisposable
    {
        /// <summary>
        /// The prototype of non-parametrized functions that workers can execute.
        /// </summary>
        public delegate void WorkerTask();

        /// <summary>
        /// The prototype of parametrized functions that workers can execute.
        /// </summary>
        public delegate void ParametrizedWorkerTask(object[] parameters);

        /// <summary>
        /// Constructs a WorkerNest object.
        /// </summary>
        /// <param name="numOfWorkers">
        /// The number of worker threads created in the WorkerNest.
        /// </param>
        /// <param name="maxNumOfTasks">
        /// The maximum number of tasks that can be waiting for execution.
        /// </param>
        public WorkerNest(int numOfWorkers, int maxNumOfTasks)
        {
            /// Check the arguments
            if (numOfWorkers < 1)
            {
                throw new ArgumentException("Number of worker threads must be at least 1.", "numOfWorkers");
            }
            if (maxNumOfTasks < 1)
            {
                throw new ArgumentException("Maximum number of tasks must be at least 1.", "maxNumOfTasks");
            }

            /// Create and initialize the administrative objects
            this.caughtExceptionList = new List<Exception>();
            this.nonParamTaskList = new WorkerTask[maxNumOfTasks];
            this.paramTaskList = new ParametrizedWorkerTask[maxNumOfTasks];
            this.taskParameterList = new object[maxNumOfTasks][];
            for (int i = 0; i < maxNumOfTasks; ++i)
            {
                this.nonParamTaskList[i] = null;
                this.paramTaskList[i] = null;
                this.taskParameterList[i] = null;
            }
            this.nextTaskToCheck = 0;

            /// Create and initialize the synchronization objects
            this.clientSemaphore = new Semaphore(maxNumOfTasks, maxNumOfTasks);
            this.workerSemaphore = new Semaphore(0, maxNumOfTasks);
            this.terminateEvent = new ManualResetEvent(false);
            this.administrationMutex = new Semaphore(1, 1);
            this.everyCurrentTaskCompletedEvent = new ManualResetEvent(true);

            /// Create and start the worker threads
            this.workerList = new RCThread[numOfWorkers];
            for (int i = 0; i < numOfWorkers; ++i)
            {
                this.workerList[i] = new RCThread(this.WorkerProc, "Worker");
                this.workerList[i].Start();
            }
        }

        /// <summary>
        /// Executes a non-parametrized task with one of the free worker threads.
        /// </summary>
        /// <param name="task">The task you want to be executed.</param>
        /// <remarks>
        /// This function returns immediately as soon as a free worker thread has been
        /// found and it has started to execute the given task.
        /// </remarks>
        public void ExecuteTask(WorkerTask task)
        {
            /// wait until a new task can be added
            this.clientSemaphore.WaitOne();
            /// send a signal that a new task is being added, so the WaitForCurrentTasks()
            /// function will block the caller from now.
            this.everyCurrentTaskCompletedEvent.Reset();

            /// add the new task
            this.administrationMutex.WaitOne();
            while (null != this.nonParamTaskList[this.nextTaskToCheck] ||
                   null != this.paramTaskList[this.nextTaskToCheck])
            {
                this.nextTaskToCheck++;
                if (this.nextTaskToCheck == this.nonParamTaskList.Length)
                {
                    this.nextTaskToCheck = 0;
                }
            }
            this.nonParamTaskList[this.nextTaskToCheck] = task;
            this.administrationMutex.Release();

            /// release a worker thread
            this.workerSemaphore.Release();
        }

        /// <summary>
        /// Executes a parametrized task with one of the free worker threads.
        /// </summary>
        /// <param name="task">The task you want to be executed.</param>
        /// <param name="parameters">The parameters of the task.</param>
        /// <remarks>
        /// This function returns immediately as soon as a free worker thread has been
        /// found and it has started to execute the given task.
        /// </remarks>
        public void ExecuteTask(ParametrizedWorkerTask task, object[] parameters)
        {
            /// wait until a new task can be added
            this.clientSemaphore.WaitOne();
            /// send a signal that a new task is being added, so the WaitForCurrentTasks()
            /// function will block the caller from now.
            this.everyCurrentTaskCompletedEvent.Reset();

            /// add the new task
            this.administrationMutex.WaitOne();
            while (null != this.nonParamTaskList[this.nextTaskToCheck] ||
                   null != this.paramTaskList[this.nextTaskToCheck])
            {
                this.nextTaskToCheck++;
                if (this.nextTaskToCheck == this.nonParamTaskList.Length)
                {
                    this.nextTaskToCheck = 0;
                }
            }
            this.paramTaskList[this.nextTaskToCheck] = task;
            this.taskParameterList[this.nextTaskToCheck] = parameters;
            this.administrationMutex.Release();

            /// release a worker thread
            this.workerSemaphore.Release();
        }

        /// <summary>
        /// This function blocks the caller thread until every running task completes.
        /// </summary>
        /// <param name="caughtExceptions">
        /// List of the exceptions caught by the workers since the last call to this function.
        /// If there are no exceptions caught, this parameter will be null.
        /// </param>
        /// <returns>
        /// True if there are no exceptions caught by the worker threads, false otherwise. In that
        /// second case you can get those exceptions from the parameter caughtExceptions.
        /// </returns>
        public bool WaitForCurrentTasks(out List<Exception> caughtExceptions)
        {
            caughtExceptions = null;
            this.everyCurrentTaskCompletedEvent.WaitOne();
            lock (this.caughtExceptionList)
            {
                if (0 == this.caughtExceptionList.Count)
                {
                    caughtExceptions = null;
                    return true;
                }
                else
                {
                    caughtExceptions = new List<Exception>();
                    foreach (Exception e in this.caughtExceptionList)
                    {
                        caughtExceptions.Add(e);
                    }
                    this.caughtExceptionList.Clear();
                    return false;
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            /// Signal the terminate event, then wait all worker threads to complete
            this.terminateEvent.Set();
            foreach (RCThread worker in this.workerList)
            {
                worker.Join();
            }

            /// Close the synchronization objects
            this.terminateEvent.Close();
            this.clientSemaphore.Close();
            this.workerSemaphore.Close();
            this.administrationMutex.Close();
            this.everyCurrentTaskCompletedEvent.Close();

            /// Set the synchronization objects to null
            this.terminateEvent = null;
            this.clientSemaphore = null;
            this.workerSemaphore = null;
            this.administrationMutex = null;
            this.everyCurrentTaskCompletedEvent = null;
        }

        #endregion

        /// <summary>
        /// Every worker thread starts executing this function.
        /// </summary>
        private void WorkerProc()
        {
            WaitHandle[] objectsToWait = new WaitHandle[] { this.workerSemaphore, this.terminateEvent };

            /// execute this while until the this.terminateEvent is signaled
            while (1 != WaitHandle.WaitAny(objectsToWait))
            {
                this.administrationMutex.WaitOne();
                /// select a task to execute
                WorkerTask selectedWorkerTask = null;
                ParametrizedWorkerTask selectedParamTask = null;
                object[] selectedTaskParamList = null;
                while (null == this.nonParamTaskList[this.nextTaskToCheck] &&
                       null == this.paramTaskList[this.nextTaskToCheck])
                {
                    this.nextTaskToCheck++;
                    if (this.nextTaskToCheck == this.nonParamTaskList.Length)
                    {
                        this.nextTaskToCheck = 0;
                    }
                }
                selectedWorkerTask = this.nonParamTaskList[this.nextTaskToCheck];
                selectedParamTask = this.paramTaskList[this.nextTaskToCheck];
                selectedTaskParamList = this.taskParameterList[this.nextTaskToCheck];

                this.nonParamTaskList[this.nextTaskToCheck] = null;
                this.paramTaskList[this.nextTaskToCheck] = null;
                this.taskParameterList[this.nextTaskToCheck] = null;
                this.administrationMutex.Release();

                try
                {
                    /// execute the selected task
                    if (null != selectedWorkerTask)
                    {
                        /// a non-parametrized task has been found
                        selectedWorkerTask();
                    }
                    else
                    {
                        /// a parametrized task has been found
                        selectedParamTask(selectedTaskParamList);
                    }
                }
                catch (Exception e)
                {
                    TraceManager.WriteExceptionAllTrace(e, false);
                    lock (this.caughtExceptionList)
                    {
                        this.caughtExceptionList.Add(e);
                    }
                }

                /// release a client
                if (this.clientSemaphore.Release() == this.nonParamTaskList.Length - 1)
                {
                    /// send a signal if there is no more task currently waiting.
                    this.everyCurrentTaskCompletedEvent.Set();
                }
            }
        }

        /// <summary>
        /// List of the worker threads.
        /// </summary>
        private RCThread[] workerList;

        /// <summary>
        /// List of the non-parametrized tasks that the workers has to execute.
        /// </summary>
        /// <remarks>
        /// The following expression is always true for any 'n' index:
        ///     this.nonParamTaskList[n] == null || this.paramTaskList[n] == null
        /// </remarks>
        private WorkerTask[] nonParamTaskList;

        /// <summary>
        /// List of the parametrized tasks that the workers has to execute.
        /// </summary>
        private ParametrizedWorkerTask[] paramTaskList;

        /// <summary>
        /// The nth element in this list is the array of the parameters of the nth task
        /// if that task is a ParametrizedWorkerTask. If it is a simple WorkerTask or if
        /// the nth task doesn't exist then the nth element in this list is null.
        /// </summary>
        private object[][] taskParameterList;

        /// <summary>
        /// List of the exceptions that are caught by the worker threads. This list is
        /// cleared at every call to WaitForCurrentTasks().
        /// </summary>
        private List<Exception> caughtExceptionList;

        /// <summary>
        /// A round-trip pointer on the tasklist.
        /// </summary>
        private int nextTaskToCheck;

        /// <summary>
        /// This semaphore is used by the clients who want to send tasks for execution.
        /// </summary>
        private Semaphore clientSemaphore;

        /// <summary>
        /// This semaphore is used by the workers who are waiting for incoming tasks.
        /// </summary>
        private Semaphore workerSemaphore;

        /// <summary>
        /// This event is fired when the worker threads has to be stopped.
        /// </summary>
        private ManualResetEvent terminateEvent;

        /// <summary>
        /// Mutual exclusion for the internal administrative operations.
        /// </summary>
        private Semaphore administrationMutex;

        /// <summary>
        /// This event is fired when there is no task to execute.
        /// </summary>
        private ManualResetEvent everyCurrentTaskCompletedEvent;
    }
}
