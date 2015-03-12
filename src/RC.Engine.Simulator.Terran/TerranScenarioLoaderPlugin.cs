using RC.Common.ComponentModel;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Scenarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.Terran.Buildings;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.Terran.Units;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Common.Diagnostics;

namespace RC.Engine.Simulator.Terran
{
    /// <summary>
    /// This class represents the scenario loader plugin for the Terran race.
    /// </summary>
    [Plugin(typeof(IScenarioLoader))]
    class TerranScenarioLoaderPlugin : IPlugin<IScenarioLoaderPluginInstall>
    {
        /// <summary>
        /// Constructs a TerranScenarioLoaderPlugin instance.
        /// </summary>
        public TerranScenarioLoaderPlugin()
        {
        }

        /// <see cref="IPlugin<T>.Install"/>
        public void Install(IScenarioLoaderPluginInstall extendedComponent)
        {
            /// TODO: Write installation code here!
            extendedComponent.RegisterEntityConstraint(CommandCenter.COMMANDCENTER_TYPE_NAME, new BuildableAreaConstraint());
            extendedComponent.RegisterEntityConstraint(CommandCenter.COMMANDCENTER_TYPE_NAME, new MinimumDistanceConstraint<ResourceObject>(new RCIntVector(3, 3)));
            extendedComponent.RegisterEntityConstraint(CommandCenter.COMMANDCENTER_TYPE_NAME, new MinimumDistanceConstraint<StartLocation>(new RCIntVector(3, 3)));
            extendedComponent.RegisterEntityConstraint(CommandCenter.COMMANDCENTER_TYPE_NAME, new MinimumDistanceConstraint<Entity>(new RCIntVector(0, 0)));
        }

        /// <see cref="IPlugin<T>.Uninstall"/>
        public void Uninstall(IScenarioLoaderPluginInstall extendedComponent)
        {
            /// TODO: Write uninstallation code here!
        }
    }
}
