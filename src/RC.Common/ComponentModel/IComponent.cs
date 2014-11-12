using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Common.ComponentModel
{
    /// <summary>
    /// This interface can be implemented by the components optionally. If a component implements this interface,
    /// the Start method will automatically be called by the ComponentManager after the instantiation of the
    /// registered components has been finished.
    /// </summary>
    public interface IComponent
    {
        /// <summary>
        /// Starts this component.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops this component.
        /// </summary>
        void Stop();
    }
}
