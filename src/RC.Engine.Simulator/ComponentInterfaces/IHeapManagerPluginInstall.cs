using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.ComponentModel;
using System.Reflection;

namespace RC.Engine.Simulator.ComponentInterfaces
{
    /// <summary>
    /// This interface is used by the plugins of the heap manager to install themselves.
    /// </summary>
    [PluginInstallInterface]
    public interface IHeapManagerPluginInstall
    {
        /// <summary>
        /// Registers the given assembly as a container of heap types.
        /// </summary>
        /// <param name="assembly">The assembly to register. The assembly must be loaded.</param>
        void RegisterHeapTypeContainer(Assembly assembly);
    }
}
