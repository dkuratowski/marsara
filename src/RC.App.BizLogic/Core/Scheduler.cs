using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.ComponentInterfaces;
using RC.Common.ComponentModel;

namespace RC.App.BizLogic.Core
{
    /// <summary>
    /// Represents a scheduler that calls its registered functions periodically.
    /// </summary>
    class Scheduler : IDisposable
    {
        /// <summary>
        /// Type of functions that can be called by this scheduler.
        /// </summary>
        public delegate void ScheduledFunction();

        /// <summary>
        /// Constructs a Scheduler instance.
        /// </summary>
        /// <param name="minMsBetweenCalls">The minimum elapsed time between calls in milliseconds.</param>
        public Scheduler(int minMsBetweenCalls)
        {
            if (minMsBetweenCalls < 0) { throw new ArgumentOutOfRangeException("minMsBetweenCalls"); }
            this.minMsBetweenCalls = minMsBetweenCalls;
            this.timeSinceLastCall = 0;
            this.registeredFunctions = new List<ScheduledFunction>();
            this.taskManager = ComponentManager.GetInterface<ITaskManager>();
            this.taskManager.SubscribeToSystemUpdate(this.OnSystemUpdate);
        }

        #region Public methods

        /// <summary>
        /// Adds a scheduled function to this scheduler.
        /// </summary>
        /// <param name="function">The function to be added.</param>
        /// <remarks>
        /// If the given function has already been added to this scheduler then this function has no effect.
        /// </remarks>
        public void AddScheduledFunction(ScheduledFunction function)
        {
            lock (this.registeredFunctions)
            {
                if (!this.registeredFunctions.Contains(function))
                {
                    this.registeredFunctions.Add(function);
                }
            }
        }

        /// <summary>
        /// Removes the given scheduled function from this scheduler.
        /// </summary>
        /// <param name="function">The function to be removed.</param>
        /// <remarks>
        /// If the given function has already been removed from this scheduler then this function has no effect.
        /// </remarks>
        public void RemoveScheduledFunction(ScheduledFunction function)
        {
            lock (this.registeredFunctions) { this.registeredFunctions.Remove(function); }
        }

        #endregion Public methods

        #region IDisposable methods

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            this.taskManager.UnsubscribeFromSystemUpdate(this.OnSystemUpdate);
            this.DisposeImpl();
        }

        #endregion IDisposable methods

        #region Overridables

        /// <summary>
        /// Special schedulers might have other requirements for calling the registered functions.
        /// This default implementation has no such requirements.
        /// </summary>
        /// <returns>True if the requirements are fulfilled, false otherwise.</returns>
        protected virtual bool HasPermissionToCall() { return true; }

        /// <summary>
        /// Special schedulers might have to do additional work after calling the registered functions.
        /// This default implementation has nothing to do.
        /// </summary>
        protected virtual void CallFinished() { }

        /// <summary>
        /// Special schedulers might have to do additional work when being disposed.
        /// This default implementation has nothing to do.
        /// </summary>
        protected virtual void DisposeImpl() { }

        #endregion Overridables

        #region Protected members

        /// <summary>
        /// Gets the reference to the task manager.
        /// </summary>
        protected ITaskManager TaskManager { get { return this.taskManager; } }

        #endregion Protected members

        /// <summary>
        /// Called on system updates.
        /// </summary>
        /// <param name="timeSinceLastUpdate">The elapsed time since last update.</param>
        /// <param name="timeSinceStart">The elapsed time since system start.</param>
        private void OnSystemUpdate(int timeSinceLastUpdate, int timeSinceStart)
        {
            this.timeSinceLastCall += timeSinceLastUpdate;
            if (this.timeSinceLastCall >= this.minMsBetweenCalls && this.HasPermissionToCall())
            {
                this.timeSinceLastCall = 0;
                List<ScheduledFunction> registeredFunctionsCopy = null;
                lock (this.registeredFunctions)
                {
                    registeredFunctionsCopy = new List<ScheduledFunction>(this.registeredFunctions);
                }
                foreach (ScheduledFunction func in registeredFunctionsCopy) { func(); }
                this.CallFinished();
            }
        }

        /// <summary>
        /// The elapsed time since the registered functions called last time.
        /// </summary>
        private int timeSinceLastCall;

        /// <summary>
        /// The list of the registered functions.
        /// </summary>
        private List<ScheduledFunction> registeredFunctions;

        /// <summary>
        /// The minimum elapsed time between calls in milliseconds.
        /// </summary>
        private readonly int minMsBetweenCalls;

        /// <summary>
        /// Reference to the task manager component.
        /// </summary>
        private ITaskManager taskManager;
    }
}
