using RC.Common.ComponentModel;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.Terran.Addons;
using RC.Engine.Simulator.Terran.Buildings;
using RC.Common;

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
            /// Terran Command Center
            extendedComponent.RegisterEntityConstraint(CommandCenter.COMMANDCENTER_TYPE_NAME, new BuildableAreaConstraint());
            extendedComponent.RegisterEntityConstraint(CommandCenter.COMMANDCENTER_TYPE_NAME, new MinimumDistanceConstraint<ResourceObject>(new RCIntVector(3, 3)));
            extendedComponent.RegisterEntityConstraint(CommandCenter.COMMANDCENTER_TYPE_NAME, new MinimumDistanceConstraint<StartLocation>(new RCIntVector(3, 3)));
            extendedComponent.RegisterEntityConstraint(CommandCenter.COMMANDCENTER_TYPE_NAME, new MinimumDistanceConstraint<Entity>(new RCIntVector(0, 0)));

            /// Terran Comsat Station
            extendedComponent.RegisterEntityConstraint(ComsatStation.COMSATSTATION_TYPE_NAME, new BuildableAreaConstraint());
            extendedComponent.RegisterEntityConstraint(ComsatStation.COMSATSTATION_TYPE_NAME, new MinimumDistanceConstraint<Entity>(new RCIntVector(0, 0)));
        }

        /// <see cref="IPlugin<T>.Uninstall"/>
        public void Uninstall(IScenarioLoaderPluginInstall extendedComponent)
        {
            /// TODO: Write uninstallation code here!
        }
    }
}
