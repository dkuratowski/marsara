using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Common.Diagnostics;

namespace RC.UI
{
    /// <summary>
    /// Represents the starting methods of parallel background tasks.
    /// </summary>
    /// <param name="parameter">An optional parameter for the task.</param>
    public delegate void UIParallelTaskMethod(object parameter);

    /// <summary>
    /// Represents the executing method of time sharing background tasks. This method will be called by the framework on every
    /// frame update while the task is running.
    /// </summary>
    /// <param name="parameter">An optional parameter for the task.</param>
    /// <returns>The task should return true if it wants to continue or false if it wants to stop the execution.</returns>
    public delegate bool UITimeSharingTaskMethod(object parameter);

    /// <summary>
    /// Static methods for managing background tasks.
    /// </summary>
    public static class UITaskManager
    {
        /// <summary>
        /// Starts a background task that runs on a parallel thread. Shall be called from the UI-thread.
        /// </summary>
        /// <param name="task">The starting method of the task.</param>
        /// <param name="name">The name of the executing thread of the task.</param>
        /// <returns>A reference to the interface of the created task.</returns>
        public static IUIBackgroundTask StartParallelTask(UIParallelTaskMethod task, string name)
        {
            return StartParallelTask(task, name, null);
        }

        /// <summary>
        /// Starts a background task with the given parameter that runs on a parallel thread. Shall be called from the UI-thread.
        /// </summary>
        /// <param name="task">The starting method of the task.</param>
        /// <param name="name">The name of the executing thread of the task.</param>
        /// <param name="parameter">The starting parameter of the task.</param>
        /// <returns>A reference to the interface of the created task.</returns>
        public static IUIBackgroundTask StartParallelTask(UIParallelTaskMethod task, string name, object parameter)
        {
            if (uiThread != null && RCThread.CurrentThread != uiThread) { throw new InvalidOperationException("UITaskManager.StartParallelTask shall be called from the UI-thread!"); }
            if (calledFromUi) { throw new InvalidOperationException("Recursive call on UITaskManager!"); }
            if (task == null) { throw new ArgumentNullException("task"); }
            if (name == null) { throw new ArgumentNullException("name"); }

            calledFromUi = true;
            uiThread = RCThread.CurrentThread;
            RCThread taskThread = new RCThread(ParallelTaskProc, name);
            UIParallelTask taskData = new UIParallelTask(task, parameter);

            lock (runningParallelTasks)
            {
                runningParallelTasks.Add(taskThread, taskData);
            }

            taskThread.Start(taskData);
            calledFromUi = false;
            return taskData;
        }

        /// <summary>
        /// Starts a background task that runs on the UI-thread with time sharing. Shall be called from the UI-thread.
        /// </summary>
        /// <param name="task">The method of the task that will be executed on each frame update.</param>
        /// <returns>A reference to the interface of the created task.</returns>
        public static IUIBackgroundTask StartTimeSharingTask(UITimeSharingTaskMethod task)
        {
            return StartTimeSharingTask(task, null);
        }

        /// <summary>
        /// Starts a background task that runs on the UI-thread with time sharing. Shall be called from the UI-thread.
        /// </summary>
        /// <param name="task">The method of the task that will be executed on each frame update.</param>
        /// <param name="parameter">The parameter of the task method.</param>
        /// <returns>A reference to the interface of the created task.</returns>
        public static IUIBackgroundTask StartTimeSharingTask(UITimeSharingTaskMethod task, object parameter)
        {
            if (uiThread != null && RCThread.CurrentThread != uiThread) { throw new InvalidOperationException("UITaskManager.StartTimeSharingTask shall be called from the UI-thread!"); }
            if (calledFromUi) { throw new InvalidOperationException("Recursive call on UITaskManager!"); }
            if (task == null) { throw new ArgumentNullException("task"); }

            calledFromUi = true;
            uiThread = RCThread.CurrentThread;
            UITimeSharingTask taskData = new UITimeSharingTask(task, parameter);
            runningTimeSharingTasks.Add(taskData);
            calledFromUi = false;
            return taskData;
        }

        /// <summary>
        /// Raises all the events posted by the background tasks. Shall be called from the UI-thread.
        /// </summary>
        public static void OnUpdate()
        {
            if (uiThread != null && RCThread.CurrentThread != uiThread) { throw new InvalidOperationException("UITaskManager.OnUpdate shall be called from the UI-thread!"); }
            if (calledFromUi) { throw new InvalidOperationException("Recursive call on UITaskManager!"); }

            calledFromUi = true;
            uiThread = RCThread.CurrentThread;

            foreach (UITimeSharingTask taskItem in runningTimeSharingTasks)
            {
                try
                {
                    scheduledTask = taskItem;
                    if (!taskItem.TaskProc(taskItem.Parameter))
                    {
                        /// The task has been stopped
                        taskItem.PostFinish();
                        tmpStoppedTimeSharingTasks.Add(taskItem);
                    }
                }
                catch (Exception ex)
                {
                    /// Error occured in the task
                    taskItem.PostFailure(ex);
                    tmpStoppedTimeSharingTasks.Add(taskItem);
                }
                finally
                {
                    scheduledTask = null;
                }
            }

            foreach (UITimeSharingTask stoppedTask in tmpStoppedTimeSharingTasks)
            {
                runningTimeSharingTasks.Remove(stoppedTask);
            }

            lock (runningParallelTasks)
            {
                tmpStoppedThreads.Clear();
                foreach (KeyValuePair<RCThread, UIParallelTask> taskItem in runningParallelTasks)
                {
                    if (!taskItem.Value.RaiseEvents())
                    {
                        tmpStoppedThreads.Add(taskItem.Key);
                    }
                }

                foreach (RCThread stoppedThread in tmpStoppedThreads)
                {
                    stoppedThread.Join();
                    runningParallelTasks.Remove(stoppedThread);
                }
            }

            calledFromUi = false;
        }

        /// <summary>
        /// Posts a message back to the UI-thread. Shall not be called from the UI-thread in case of parallel tasks.
        /// </summary>
        /// <param name="message">The message to post.</param>
        public static void PostMessage(object message)
        {
            if (uiThread == null) { throw new InvalidOperationException("UITaskManager.PostMessage shall not be called before calling UITaskManager.StartParallelTask, UITaskManager.StartTimeSharingTask or UITaskManager.OnUpdate!"); }
            if (message == null) { throw new ArgumentNullException("message"); }

            if (uiThread == RCThread.CurrentThread)
            {
                /// Call from a time sharing task
                if (scheduledTask == null) { throw new InvalidOperationException("UITaskManager.PostMessage can be called from the UI-thread only if a time sharing task has been scheduled!"); }
                scheduledTask.PostMessage(message);
            }
            else
            {
                /// Call from a parallel task.
                lock (runningParallelTasks)
                {
                    UIParallelTask task = runningParallelTasks[RCThread.CurrentThread];
                    task.PostMessage(message);
                }
            }
        }

        /// <summary>
        /// The starting method of the parallel tasks.
        /// </summary>
        /// <param name="taskData">The UIParallelTask object itself.</param>
        private static void ParallelTaskProc(object taskData)
        {
            UIParallelTask task = (UIParallelTask)taskData;

            try
            {
                TraceManager.WriteAllTrace(string.Format("Starting task '{0}'", RCThread.CurrentThread.Name), UITraceFilters.INFO);
                task.TaskProc(task.Parameter);
                task.PostFinish();
                TraceManager.WriteAllTrace(string.Format("Task '{0}' finished", RCThread.CurrentThread.Name), UITraceFilters.INFO);
            }
            catch (Exception ex)
            {
                TraceManager.WriteAllTrace(string.Format("Task '{0}' failed. Exception: {1}", RCThread.CurrentThread.Name, ex.ToString()), UITraceFilters.ERROR);
                task.PostFailure(ex);
            }
        }

        /// <summary>
        /// List of the running parallel tasks mapped by their executing threads.
        /// </summary>
        private static Dictionary<RCThread, UIParallelTask> runningParallelTasks = new Dictionary<RCThread, UIParallelTask>();

        /// <summary>
        /// List of the running time sharing tasks.
        /// </summary>
        private static HashSet<UITimeSharingTask> runningTimeSharingTasks = new HashSet<UITimeSharingTask>();

        /// <summary>
        /// Reference to the UI-thread. The UI-thread is the first thread that calls UITaskManager.StartParallelTask,
        /// UITaskManager.StartTimeSharingTask or UITaskManager.OnUpdate.
        /// </summary>
        private static RCThread uiThread = null;

        /// <summary>
        /// This flag is used to avoid recursive calls from the UI-thread.
        /// </summary>
        private static bool calledFromUi = false;

        /// <summary>
        /// Reference to the currently scheduled time sharing task.
        /// </summary>
        private static UITimeSharingTask scheduledTask = null;

        /// <summary>
        /// Temporary list of the threads that stopped working due to a failure or completion.
        /// </summary>
        private static List<RCThread> tmpStoppedThreads = new List<RCThread>();

        /// <summary>
        /// Temporary list of the time sharing tasks that stopped working due to a failure or completion.
        /// </summary>
        private static List<UITimeSharingTask> tmpStoppedTimeSharingTasks = new List<UITimeSharingTask>();
    }
}
