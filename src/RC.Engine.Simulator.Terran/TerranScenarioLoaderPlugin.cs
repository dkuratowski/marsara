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
            extendedComponent.RegisterPlacementSuggestionProvider(CommandCenter.COMMANDCENTER_TYPE_NAME, new CorrespondingAddonSuggestion());

            /// Terran Barracks
            extendedComponent.RegisterEntityConstraint(Barracks.BARRACKS_TYPE_NAME, new BuildableAreaConstraint());
            extendedComponent.RegisterEntityConstraint(Barracks.BARRACKS_TYPE_NAME, new MinimumDistanceConstraint<Entity>(new RCIntVector(0, 0)));

            /// Terran Engineering Bay
            extendedComponent.RegisterEntityConstraint(EngineeringBay.ENGINEERINGBAY_TYPE_NAME, new BuildableAreaConstraint());
            extendedComponent.RegisterEntityConstraint(EngineeringBay.ENGINEERINGBAY_TYPE_NAME, new MinimumDistanceConstraint<Entity>(new RCIntVector(0, 0)));

            /// Terran Factory
            extendedComponent.RegisterEntityConstraint(Factory.FACTORY_TYPE_NAME, new BuildableAreaConstraint());
            extendedComponent.RegisterEntityConstraint(Factory.FACTORY_TYPE_NAME, new MinimumDistanceConstraint<Entity>(new RCIntVector(0, 0)));
            extendedComponent.RegisterPlacementSuggestionProvider(Factory.FACTORY_TYPE_NAME, new CorrespondingAddonSuggestion());

            /// Terran Starport
            extendedComponent.RegisterEntityConstraint(Starport.STARPORT_TYPE_NAME, new BuildableAreaConstraint());
            extendedComponent.RegisterEntityConstraint(Starport.STARPORT_TYPE_NAME, new MinimumDistanceConstraint<Entity>(new RCIntVector(0, 0)));
            extendedComponent.RegisterPlacementSuggestionProvider(Starport.STARPORT_TYPE_NAME, new CorrespondingAddonSuggestion());

            /// Terran Science Facility
            extendedComponent.RegisterEntityConstraint(ScienceFacility.SCIENCEFACILITY_TYPE_NAME, new BuildableAreaConstraint());
            extendedComponent.RegisterEntityConstraint(ScienceFacility.SCIENCEFACILITY_TYPE_NAME, new MinimumDistanceConstraint<Entity>(new RCIntVector(0, 0)));
            extendedComponent.RegisterPlacementSuggestionProvider(ScienceFacility.SCIENCEFACILITY_TYPE_NAME, new CorrespondingAddonSuggestion());

            /// Terran Academy
            extendedComponent.RegisterEntityConstraint(Academy.ACADEMY_TYPE_NAME, new BuildableAreaConstraint());
            extendedComponent.RegisterEntityConstraint(Academy.ACADEMY_TYPE_NAME, new MinimumDistanceConstraint<Entity>(new RCIntVector(0, 0)));

            /// Terran Armory
            extendedComponent.RegisterEntityConstraint(Armory.ARMORY_TYPE_NAME, new BuildableAreaConstraint());
            extendedComponent.RegisterEntityConstraint(Armory.ARMORY_TYPE_NAME, new MinimumDistanceConstraint<Entity>(new RCIntVector(0, 0)));

            /// Terran Missile Turret
            extendedComponent.RegisterEntityConstraint(MissileTurret.MISSILETURRET_TYPE_NAME, new BuildableAreaConstraint());
            extendedComponent.RegisterEntityConstraint(MissileTurret.MISSILETURRET_TYPE_NAME, new MinimumDistanceConstraint<Entity>(new RCIntVector(0, 0)));

            /// Terran Supply Depot
            extendedComponent.RegisterEntityConstraint(SupplyDepot.SUPPLYDEPOT_TYPE_NAME, new BuildableAreaConstraint());
            extendedComponent.RegisterEntityConstraint(SupplyDepot.SUPPLYDEPOT_TYPE_NAME, new MinimumDistanceConstraint<Entity>(new RCIntVector(0, 0)));

            /// Terran Comsat Station
            extendedComponent.RegisterEntityConstraint(ComsatStation.COMSATSTATION_TYPE_NAME, new BuildableAreaConstraint());
            extendedComponent.RegisterEntityConstraint(ComsatStation.COMSATSTATION_TYPE_NAME, new MinimumDistanceConstraint<Entity>(new RCIntVector(0, 0)));

            /// Terran Control Tower
            extendedComponent.RegisterEntityConstraint(ControlTower.CONTROLTOWER_TYPE_NAME, new BuildableAreaConstraint());
            extendedComponent.RegisterEntityConstraint(ControlTower.CONTROLTOWER_TYPE_NAME, new MinimumDistanceConstraint<Entity>(new RCIntVector(0, 0)));
        }

        /// <see cref="IPlugin<T>.Uninstall"/>
        public void Uninstall(IScenarioLoaderPluginInstall extendedComponent)
        {
            /// TODO: Write uninstallation code here!
        }
    }
}
