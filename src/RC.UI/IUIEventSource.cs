using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.Diagnostics;

namespace RC.UI
{
    /// <summary>
    /// Interface of every class that acts as an event source.
    /// </summary>
    public interface IUIEventSource
    {
        /// <summary>
        /// Activates the event source. If the event source is already activated then this function
        /// has no effect.
        /// </summary>
        void Activate();

        /// <summary>
        /// Deactivates the event source. If the event source is already deactivated then this
        /// function has no effect.
        /// </summary>
        void Deactivate();

        /// <summary>
        /// Gets the name of the event source. This name must be unique in the set of the registered
        /// event sources.
        /// </summary>
        string Name { get; }
    }

    /// <summary>
    /// This must be the base class of every class that acts as an event source.
    /// </summary>
    public abstract class UIEventSourceBase : IUIEventSource
    {
        /// <summary>
        /// Constructs an event source object.
        /// </summary>
        public UIEventSourceBase(string name)
        {
            this.isActive = false;
            this.name = name;
        }

        #region IUIEventSource members

        /// <see cref="IUIEventSource.Activate"/>
        public void Activate()
        {
            if (!this.isActive)
            {
                this.Activate_i();
                this.isActive = true;
                TraceManager.WriteAllTrace(string.Format("Event source {0} activated", this.name), UITraceFilters.INFO);
            }
        }

        /// <see cref="IUIEventSource.Deactivate"/>
        public void Deactivate()
        {
            if (this.isActive)
            {
                this.Deactivate_i();
                this.isActive = false;
                TraceManager.WriteAllTrace(string.Format("Event source {0} deactivated", this.name), UITraceFilters.INFO);
            }
        }

        /// <see cref="IUIEventSource.Name"/>
        public string Name { get { return this.name; } }

        #endregion IUIEventSource members

        /// <summary>
        /// Gets whether this event source is active or not.
        /// </summary>
        protected bool IsActive { get { return this.isActive; } }

        /// <summary>
        /// Internal method for performing activation procedure in the derived classes.
        /// </summary>
        protected abstract void Activate_i();

        /// <summary>
        /// Internal method for performing deactivation procedure in the derived classes.
        /// </summary>
        protected abstract void Deactivate_i();

        /// <summary>
        /// The name of the event source. This name must be unique in the set of the registered
        /// event sources.
        /// </summary>
        private string name;

        /// <summary>
        /// This flag indicates whether this event source is active or not.
        /// </summary>
        private bool isActive;
    }
}
