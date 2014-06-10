using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// Represents a scheduler that is triggered from a background task.
    /// </summary>
    class TriggeredScheduler : Scheduler
    {
        /// <summary>
        /// Constructs a TriggeredScheduler instance.
        /// </summary>
        /// <param name="minMsBetweenCalls">The minimum elapsed time between calls in milliseconds.</param>
        public TriggeredScheduler(int minMsBetweenCalls)
            : base(minMsBetweenCalls)
        {
            this.isTriggered = false;
            this.lockObject = new object();
            this.continueBackgroundTaskEvt = new AutoResetEvent(false);
        }

        /// <summary>
        /// Triggers this scheduler from the appropriate background task. The task will be locked while the
        /// next call is not finished.
        /// </summary>
        public void Trigger()
        {
            lock (this.lockObject) { this.isTriggered = true; }
            this.continueBackgroundTaskEvt.WaitOne();
        }

        #region Overriden methods

        /// <see cref="Scheduler.HasPermissionToCall"/>
        protected override bool HasPermissionToCall() { lock (this.lockObject) { return this.isTriggered; } }

        /// <see cref="Scheduler.CallFinished"/>
        protected override void CallFinished()
        {
            lock (this.lockObject)
            {
                this.isTriggered = false;
                this.continueBackgroundTaskEvt.Set();
            }
        }

        /// <see cref="Scheduler.DisposeImpl"/>
        protected override void DisposeImpl() { this.continueBackgroundTaskEvt.Close(); }
        
        #endregion Overriden methods

        /// <summary>
        /// This flag indicates if the scheduler is triggered or not.
        /// </summary>
        private bool isTriggered;

        /// <summary>
        /// The object that is used for locking.
        /// </summary>
        private object lockObject;

        /// <summary>
        /// This event is used to release the background task when the actual call has been finished.
        /// </summary>
        private AutoResetEvent continueBackgroundTaskEvt;
    }
}
