using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.UI
{
    /// <summary>
    /// Represents a background task that is executed on a parallel thread.
    /// </summary>
    class UIParallelTask : IUIBackgroundTask
    {
        /// <summary>
        /// Constructs a UIParallelTask instance.
        /// </summary>
        public UIParallelTask(UIParallelTaskMethod taskProc, object parameter)
        {
            this.taskProc = taskProc;
            this.parameter = parameter;
            this.postedMessages = new List<object>();
            this.postedFailure = null;
            this.isFinished = false;
        }

        /// <summary>
        /// Gets the starting method of the task.
        /// </summary>
        public UIParallelTaskMethod TaskProc { get { return this.taskProc; } }

        /// <summary>
        /// Gets the starting parameter of the task.
        /// </summary>
        public object Parameter { get { return this.parameter; } }

        /// <summary>
        /// Raises the events of this background task posted by it's executing thread.
        /// </summary>
        /// <returns>True if the task is still executing, false if it has been finished or failed.</returns>
        public bool RaiseEvents()
        {
            lock (this.lockObject)
            {
                foreach (object msg in this.postedMessages)
                {
                    if (this.Message != null) { this.Message(this, msg); }
                }
                this.postedMessages.Clear();

                if (this.isFinished == true)
                {
                    if (this.postedFailure == null)
                    {
                        if (this.Finished != null) { this.Finished(this, null); }
                    }
                    else
                    {
                        if (this.Failed != null) { this.Failed(this, this.postedFailure); }
                    }
                }

                return !this.isFinished;
            }
        }

        /// <summary>
        /// Posts a message back to the UI-thread from this background task.
        /// </summary>
        /// <param name="message">The message to post.</param>
        public void PostMessage(object message)
        {
            lock (this.lockObject)
            {
                this.postedMessages.Add(message);
            }
        }

        /// <summary>
        /// Posts a failure to this UIParallelTask.
        /// </summary>
        /// <param name="ex">The exception caught by the executing thread.</param>
        public void PostFailure(Exception ex)
        {
            if (ex == null) { throw new ArgumentNullException("ex"); }

            lock (this.lockObject)
            {
                this.postedFailure = ex;
                this.isFinished = true;
            }
        }

        /// <summary>
        /// Posts finish message to this UIParallelTask.
        /// </summary>
        public void PostFinish()
        {
            lock (this.lockObject)
            {
                this.isFinished = true;
            }
        }

        #region IUIBackgroundTask members

        /// <see cref="IUIBackgroundTask.Finished"/>
        public event UITaskEventHdl Finished;

        /// <see cref="IUIBackgroundTask.Message"/>
        public event UITaskEventHdl Message;

        /// <see cref="IUIBackgroundTask.Failed"/>
        public event UITaskEventHdl Failed;

        #endregion IUIBackgroundTask members

        /// <summary>
        /// Reference to the starting method of the task.
        /// </summary>
        private UIParallelTaskMethod taskProc;

        /// <summary>
        /// Reference to the starting parameter of the task.
        /// </summary>
        private object parameter;

        /// <summary>
        /// List of the posted but non-raised messages.
        /// </summary>
        private List<object> postedMessages;

        /// <summary>
        /// The posted exception (if any).
        /// </summary>
        private Exception postedFailure;

        /// <summary>
        /// This flag indicates whether this task has been finished or still running.
        /// </summary>
        private bool isFinished;

        /// <summary>
        /// Object used for thread synchronization.
        /// </summary>
        private object lockObject = new object();
    }
}
