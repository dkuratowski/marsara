using RC.Common.ComponentModel;
using RC.Engine.Simulator.Scenarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.ComponentInterfaces
{
    /// <summary>
    /// This interface is used by the plugins of the scenario loader to install themselves.
    /// </summary>
    [PluginInstallInterface]
    public interface IScenarioLoaderPluginInstall
    {
        /// <summary>
        /// Registers the given placement constraint for the given entity type.
        /// </summary>
        /// <param name="entityType">The name of the entity type.</param>
        /// <param name="constraint">The constraint to register.</param>
        void RegisterEntityConstraint(string entityType, EntityConstraint constraint);
    }
}
