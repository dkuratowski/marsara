using System;
using System.Threading;
using RC.Common;

namespace RC.DssServices.Test
{
    /// <summary>
    /// This is the base class of every synchron call to the UI from another thread.
    /// </summary>
    public class SynchronUiCall
    {
        /// <summary>
        /// Class level constructor.
        /// </summary>
        static SynchronUiCall()
        {
            uiCallsRejected = new ManualResetEvent(false);
        }

        /// <summary>
        /// Rejects every pending UI-calls.
        /// </summary>
        public static void TurnOffBlocking()
        {
            uiCallsRejected.Set();
        }

        /// <summary>
        /// Stops rejecting every pending UI-calls.
        /// </summary>
        public static void TurnOnBlocking()
        {
            uiCallsRejected.Reset();
        }

        /// <summary>
        /// This event is signaled when the UI-thread rejects any UI-calls.
        /// </summary>
        private static ManualResetEvent uiCallsRejected;

        /// <summary>
        /// Creates a SynchronUiCall object.
        /// </summary>
        public SynchronUiCall()
        {
            this.finishedEvt = new ManualResetEvent(false);
        }

        /// <summary>
        /// Blocks the caller thread while this UI-call is being executed or rejected.
        /// </summary>
        public void Wait()
        {
            if (MainForm.UiThread == RCThread.CurrentThread) { throw new Exception("This call is not allowed from the UI-thread!"); }

            int finishedOrRejected = EventWaitHandle.WaitAny(new WaitHandle[2] { this.finishedEvt, uiCallsRejected });
            if (0 == finishedOrRejected)
            {
                /// Finished
                this.finishedEvt.Close();
                return;
            }
            else
            {
                /// Rejected
                //this.finishedEvt.Close();
                return;
            }
        }

        /// <summary>
        /// This function must be called by the UI-thread to execute the call.
        /// </summary>
        public void Execute()
        {
            if (MainForm.UiThread != RCThread.CurrentThread) { throw new Exception("This call is only allowed from the UI-thread!"); }

            Execute_i();
            if (!uiCallsRejected.WaitOne(0))
            {
                this.finishedEvt.Set();
            }
            else
            {
                this.finishedEvt.Close();
            }
        }

        /// <summary>
        /// The implementation of the UI-call. This function could be overriden by the derived classes.
        /// </summary>
        protected virtual void Execute_i()
        {
            /// Nothing to do in the default implementation.
        }

        /// <summary>
        /// This event is signaled when the UI-thread has executed this call.
        /// </summary>
        private ManualResetEvent finishedEvt;
    }
}
