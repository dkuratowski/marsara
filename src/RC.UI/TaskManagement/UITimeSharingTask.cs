using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.UI
{
    /// <summary>
    /// Represents a time sharing background task.
    /// </summary>
    class UITimeSharingTask : IUIBackgroundTask
    {
        /// <summary>
        /// Constructs a UIParallelTask instance.
        /// </summary>
        public UITimeSharingTask(UITimeSharingTaskMethod taskProc, object parameter)
        {
            this.taskProc = taskProc;
            this.parameter = parameter;
        }

        /// <summary>
        /// Gets the executing method of the task.
        /// </summary>
        public UITimeSharingTaskMethod TaskProc { get { return this.taskProc; } }

        /// <summary>
        /// Gets the parameter of the task.
        /// </summary>
        public object Parameter { get { return this.parameter; } }

        /// <summary>
        /// Posts a message from this background task.
        /// </summary>
        /// <param name="message">The message to post.</param>
        public void PostMessage(object message)
        {
            if (this.Message != null) { this.Message(this, message); }
        }

        /// <summary>
        /// Posts a failure to this UITimeSharingTask.
        /// </summary>
        /// <param name="ex">The exception caught by the task.</param>
        public void PostFailure(Exception ex)
        {
            if (ex == null) { throw new ArgumentNullException("ex"); }
            if (this.Failed != null) { this.Failed(this, ex); }
        }

        /// <summary>
        /// Posts finish message to this UITimeSharingTask.
        /// </summary>
        public void PostFinish()
        {
            if (this.Finished != null) { this.Finished(this, null); }
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
        /// Reference to the executing method of the task.
        /// </summary>
        private UITimeSharingTaskMethod taskProc;

        /// <summary>
        /// Reference to the starting parameter of the task.
        /// </summary>
        private object parameter;
    }
}
