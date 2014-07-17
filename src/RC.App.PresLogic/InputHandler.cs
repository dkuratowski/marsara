using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.PresLogic
{
    /// <summary>
    /// The abstract base class of the input handlers.
    /// </summary>
    abstract class InputHandler
    {
        /// <summary>
        /// Constructs an InputHandler.
        /// </summary>
        public InputHandler()
        {
            this.state = StateEnum.Stopped;
            this.isDisposed = false;
        }

        /// <summary>
        /// Enumerates the possible states of an input handler.
        /// </summary>
        public enum StateEnum
        {
            Stopped = 0,        /// The input handler is stopped.
            StandBy = 1,        /// The input handler is monitoring the input events.
            Processing = 2      /// The input handler is processing input events so other handlers must be hibernated.
        }

        #region Public members

        /// <summary>
        /// This event is raised when processing of an input event has been started.
        /// </summary>
        public event EventHandler ProcessingStarted;

        /// <summary>
        /// This event is raised when processing of an output event has been finished.
        /// </summary>
        public event EventHandler ProcessingFinished;

        /// <summary>
        /// Stops this input handler.
        /// </summary>
        public void Stop()
        {
            if (this.isDisposed) { throw new ObjectDisposedException("InputHandler"); }
            if (this.state != StateEnum.StandBy) { throw new InvalidOperationException("Invalid state!"); }
            this.state = StateEnum.Stopped;
            this.StopImpl();
        }

        /// <summary>
        /// Starts this input handler.
        /// </summary>
        public void Start()
        {
            if (this.isDisposed) { throw new ObjectDisposedException("InputHandler"); }
            if (this.state != StateEnum.Stopped) { throw new InvalidOperationException("Invalid state!"); }
            this.state = StateEnum.StandBy;
            this.StartImpl();
        }

        /// <summary>
        /// Gets the current state of this input handler.
        /// </summary>
        public StateEnum State { get { return this.state; } }

        #endregion Public members

        #region Protected members for the derived classes

        /// <summary>
        /// Call this method from the derived classes when processing an input event has been started.
        /// </summary>
        protected void OnProcessingStarted()
        {
            if (this.isDisposed) { throw new ObjectDisposedException("InputHandler"); }
            if (this.state != StateEnum.StandBy) { throw new InvalidOperationException("Input event processing can only be started from stand-by state!"); }
            this.state = StateEnum.Processing;
            if (this.ProcessingStarted != null) { this.ProcessingStarted(this, new EventArgs()); }
        }

        /// <summary>
        /// Call this method from the derived classes when processing an input event has been finished.
        /// </summary>
        protected void OnProcessingFinished()
        {
            if (this.isDisposed) { throw new ObjectDisposedException("InputHandler"); }
            if (this.state != StateEnum.Processing) { throw new InvalidOperationException("Input event processing can only be finished from processing state!"); }
            this.state = StateEnum.StandBy;
            if (this.ProcessingFinished != null) { this.ProcessingFinished(this, new EventArgs()); }
        }

        #endregion Protected members for the derived classes

        #region Overridables

        /// <summary>
        /// Override this method in the derived classes and implement stopping procedures.
        /// </summary>
        protected abstract void StopImpl();

        /// <summary>
        /// Override this method in the derived classes and implement starting procedures.
        /// </summary>
        protected abstract void StartImpl();

        #endregion Overridables

        /// <summary>
        /// The current state of this input handler.
        /// </summary>
        private StateEnum state;

        /// <summary>
        /// This flag indicates whether this input handler has already been disposed or not.
        /// </summary>
        private bool isDisposed;
    }
}
