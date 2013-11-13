using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Common.ComponentModel
{
    /// <summary>
    /// This interface can be implemented by the plugins optionally. If a plugin implements this interface,
    /// the Install method will automatically be called by the ComponentManager just after every registered
    /// component is ready to use. Similarly if a plugin implements this interface, the Uninstall method will
    /// automatically be called by the ComponentManager just before any of the registered components is being
    /// stopped.
    /// </summary>
    /// <typeparam name="T">The plugin install interface of the component that this plugin extends.</typeparam>
    public interface IPlugin<T>
    {
        /// <summary>
        /// Installs this plugin.
        /// </summary>
        /// <param name="extendedComponent">Reference to the extended component.</param>
        void Install(T extendedComponent);

        /// <summary>
        /// Uninstalls this plugin.
        /// </summary>
        /// <param name="extendedComponent">Reference to the extended component.</param>
        void Uninstall(T extendedComponent);
    }
}
