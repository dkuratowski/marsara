using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Common.ComponentModel
{
    /// <summary>
    /// This interface can be implemented by the components optionally. If a component implements this interface,
    /// the Start method will automatically be called by the ComponentManager after the instantiation of the
    /// registered components has been finished and all references between the components has been set.
    /// </summary>
    public interface IComponentStart
    {
        /// <summary>
        /// Starts the component. When this method is called, the references between the components are valid.
        /// </summary>
        void Start();
    }
}
