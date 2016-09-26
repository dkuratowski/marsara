using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.Terran.Addons;
using RC.Engine.Simulator.Terran.Buildings;
using RC.Engine.Simulator.Terran.Units;

namespace RC.Engine.Simulator.Terran
{
    /// <summary>
    /// The element factory plugin for the Terran race.
    /// </summary>
    [Plugin(typeof(IElementFactory))]
    class ElementFactoryPlugin : IPlugin<IElementFactoryPluginInstall>
    {
        /// <summary>
        /// Constructs an ElementFactoryPlugin instance.
        /// </summary>
        public ElementFactoryPlugin()
        {
        }

        /// <see cref="IPlugin.Install"/>
        public void Install(IElementFactoryPluginInstall extendedComponent)
        {
            /// Player initializer method for Terran.
            extendedComponent.RegisterPlayerInitializer(RaceEnum.Terran, this.TerranInitializer);

            /// Element factory methods for Terran units.
            extendedComponent.RegisterElementFactory<Building>(SCV.SCV_TYPE_NAME, this.CreateUnit<SCV>);
            extendedComponent.RegisterElementFactory<Building>(Marine.MARINE_TYPE_NAME, this.CreateUnit<Marine>);
            extendedComponent.RegisterElementFactory<Building>(Goliath.GOLIATH_TYPE_NAME, this.CreateUnit<Goliath>);
            extendedComponent.RegisterElementFactory<Building>(Wraith.WRAITH_TYPE_NAME, this.CreateUnit<Wraith>);
            extendedComponent.RegisterElementFactory<Building>("Dropship", building => true);

            /// Element factory methods for Terran addons.
            extendedComponent.RegisterElementFactory<Building>(ComsatStation.COMSATSTATION_TYPE_NAME, this.CreateAddon<ComsatStation>);
            extendedComponent.RegisterElementFactory<Building>(ControlTower.CONTROLTOWER_TYPE_NAME, this.CreateAddon<ControlTower>);
            extendedComponent.RegisterElementFactory<Building>(MachineShop.MACHINESHOP_TYPE_NAME, this.CreateAddon<MachineShop>);

            /// Element factory methods for Terran buildings.
            extendedComponent.RegisterElementFactory<Player, RCIntVector, SCV>(Academy.ACADEMY_TYPE_NAME, this.CreateBuilding<Academy>);
            extendedComponent.RegisterElementFactory<Player, RCIntVector, SCV>(Armory.ARMORY_TYPE_NAME, this.CreateBuilding<Armory>);
            extendedComponent.RegisterElementFactory<Player, RCIntVector, SCV>(Barracks.BARRACKS_TYPE_NAME, this.CreateBuilding<Barracks>);
            extendedComponent.RegisterElementFactory<Player, RCIntVector, SCV>(CommandCenter.COMMANDCENTER_TYPE_NAME, this.CreateBuilding<CommandCenter>);
            extendedComponent.RegisterElementFactory<Player, RCIntVector, SCV>(EngineeringBay.ENGINEERINGBAY_TYPE_NAME, this.CreateBuilding<EngineeringBay>);
            extendedComponent.RegisterElementFactory<Player, RCIntVector, SCV>(Factory.FACTORY_TYPE_NAME, this.CreateBuilding<Factory>);
            extendedComponent.RegisterElementFactory<Player, RCIntVector, SCV>(MissileTurret.MISSILETURRET_TYPE_NAME, this.CreateBuilding<MissileTurret>);
            extendedComponent.RegisterElementFactory<Player, RCIntVector, SCV>(Refinery.REFINERY_TYPE_NAME, this.CreateBuilding<Refinery>);
            extendedComponent.RegisterElementFactory<Player, RCIntVector, SCV>(ScienceFacility.SCIENCEFACILITY_TYPE_NAME, this.CreateBuilding<ScienceFacility>);
            extendedComponent.RegisterElementFactory<Player, RCIntVector, SCV>(Starport.STARPORT_TYPE_NAME, this.CreateBuilding<Starport>);
            extendedComponent.RegisterElementFactory<Player, RCIntVector, SCV>(SupplyDepot.SUPPLYDEPOT_TYPE_NAME, this.CreateBuilding<SupplyDepot>);
        }

        /// <see cref="IPlugin.Uninstall"/>
        public void Uninstall(IElementFactoryPluginInstall extendedComponent)
        {
            /// TODO: Write uninstallation code here!
        }

        /// <summary>
        /// Initializes a player of the Terran race.
        /// </summary>
        /// <param name="player">The player to be initialized.</param>
        private void TerranInitializer(Player player)
        {
            if (player == null) { throw new ArgumentNullException("player"); }

            /// Add a Terran Command Center to the position of the start location.
            Scenario scenario = player.StartLocation.Scenario;
            //Starport commandCenter = new Starport();
            CommandCenter commandCenter = new CommandCenter();
            scenario.AddElementToScenario(commandCenter);
            player.AddBuilding(commandCenter);
            commandCenter.AttachToMap(scenario.Map.GetQuadTile(player.QuadraticStartPosition.Location));

            /// TEST: Add a Terran Comsat Station
            //ComsatStation comsatStation = new ComsatStation();
            //scenario.AddElementToScenario(comsatStation);
            //player.AddAddon(comsatStation);
            //comsatStation.AttachToMap(scenario.Map.GetQuadTile(player.QuadraticStartPosition.Location + new RCIntVector(4, 1)));
            /// TEST END

            /// Find place for the given number of SCVs using an EntityNeighbourhoodIterator.
            EntityNeighbourhoodIterator cellIterator = new EntityNeighbourhoodIterator(commandCenter);
            IEnumerator<ICell> cellEnumerator = cellIterator.GetEnumerator();
            for (int scvCount = 0; scvCount < NUM_OF_SCVS; scvCount++)
            {
                /// Create the next SCV
                //Wraith scv = new Wraith();
                SCV scv = new SCV();
                //Unit scv = scvCount % 2 == 0 ? (Unit)new SCV() : (Unit)new Marine();
                scenario.AddElementToScenario(scv);
                player.AddUnit(scv);

                /// Search a place for the new SCV on the map.
                bool scvPlacedSuccessfully = false;
                while (cellEnumerator.MoveNext())
                {
                    if (scv.AttachToMap(cellEnumerator.Current.MapCoords))
                    {
                        scvPlacedSuccessfully = true;
                        break;
                    }
                }

                /// Remove the SCV and stop initializing if there is no more place on the map.
                if (!scvPlacedSuccessfully)
                {
                    player.RemoveUnit(scv);
                    scenario.RemoveElementFromScenario(scv);
                    scv.Dispose();
                    break;
                }
            }
        }

        /// <summary>
        /// Creates the given type of unit in the given factory building.
        /// </summary>
        /// <typeparam name="T">The type of the unit to create.</typeparam>
        /// <param name="factoryBuilding">The factory building for which the unit is created.</param>
        /// <returns>True if the unit has been created successfully; otherwise false.</returns>
        private bool CreateUnit<T>(Building factoryBuilding) where T : Unit, new()
        {
            if (factoryBuilding == null) { throw new ArgumentNullException("factoryBuilding"); }
            if (factoryBuilding.Scenario == null) { throw new ArgumentException("The factory building is not added to a scenario!"); }
            if (factoryBuilding.MapObject == null) { throw new ArgumentException("The factory building is detached from the map!", "factoryBuilding"); }

            EntityNeighbourhoodIterator cellIterator = new EntityNeighbourhoodIterator(factoryBuilding);
            IEnumerator<ICell> cellEnumerator = cellIterator.GetEnumerator();

            /// Create the unit.
            T unit = new T();
            factoryBuilding.Scenario.AddElementToScenario(unit);
            if (factoryBuilding.Owner != null) { factoryBuilding.Owner.AddUnit(unit); }

            /// Search a place for the new unit on the map.
            bool unitPlacedSuccessfully = false;
            while (cellEnumerator.MoveNext())
            {
                if (unit.AttachToMap(cellEnumerator.Current.MapCoords))
                {
                    unitPlacedSuccessfully = true;
                    break;
                }
            }

            /// Remove the unit if there is no more place on the map.
            if (!unitPlacedSuccessfully)
            {
                if (factoryBuilding.Owner != null) { factoryBuilding.Owner.RemoveUnit(unit); }
                factoryBuilding.Scenario.RemoveElementFromScenario(unit);
                unit.Dispose();
            }

            return unitPlacedSuccessfully;
        }

        /// <summary>
        /// Creates the given type of addon for the given main building.
        /// </summary>
        /// <typeparam name="T">The type of the addon to create.</typeparam>
        /// <param name="mainBuilding">The main building for which the addon is created.</param>
        /// <returns>True if the addon has been created successfully; otherwise false.</returns>
        private bool CreateAddon<T>(Building mainBuilding) where T : Addon, new()
        {
            if (mainBuilding == null) { throw new ArgumentNullException("mainBuilding"); }
            if (mainBuilding.Scenario == null) { throw new ArgumentException("The main building is not added to a scenario!", "mainBuilding"); }
            if (mainBuilding.MapObject == null) { throw new ArgumentException("The main building is detached from the map!", "mainBuilding"); }

            /// Create the addon.
            T addon = new T();
            mainBuilding.Scenario.AddElementToScenario(addon);

            /// Try to attach the addon to the map.
            RCIntVector relativeAddonPosition = mainBuilding.BuildingType.GetRelativeAddonPosition(mainBuilding.Scenario.Map, addon.AddonType);
            RCIntVector addonPosition = mainBuilding.MapObject.QuadraticPosition.Location + relativeAddonPosition;
            bool addonPlacedSuccessfully = addon.AttachToMap(mainBuilding.Scenario.Map.GetQuadTile(addonPosition));
            if (!addonPlacedSuccessfully)
            {
                mainBuilding.Scenario.RemoveElementFromScenario(addon);
                addon.Dispose();
            }

            return addonPlacedSuccessfully;
        }

        /// <summary>
        /// Creates a building for the given player, attaches it to the given position and starts its construction with the given SCV.
        /// </summary>
        /// <typeparam name="T">The type of the building to be created.</typeparam>
        /// <param name="owner">The owner player of the building.</param>
        /// <param name="topLeftQuadTile">The coordinates of the top-left quadratic tile of the building to be created.</param>
        /// <param name="constructorSCV">The SCV that starts the construction of the building.</param>
        /// <returns>True if the building has been created successfully; otherwise false.</returns>
        private bool CreateBuilding<T>(Player owner, RCIntVector topLeftQuadTile, SCV constructorSCV) where T : TerranBuilding, new()
        {
            if (owner == null) { throw new ArgumentNullException("owner"); }
            if (topLeftQuadTile == RCIntVector.Undefined) { throw new ArgumentNullException("topLeftQuadTile"); }
            if (constructorSCV == null) { throw new ArgumentNullException("constructorSCV"); }

            /// Create the building.
            T building = new T();
            owner.Scenario.AddElementToScenario(building);
            owner.AddBuilding(building);

            /// Try to attach the building onto the map and start its construction.
            bool buildingPlacedSuccessfully = building.AttachToMap(owner.Scenario.Map.GetQuadTile(topLeftQuadTile), constructorSCV);
            if (buildingPlacedSuccessfully)
            {
                building.Biometrics.Construct();
            }
            else
            {
                owner.RemoveBuilding(building);
                owner.Scenario.RemoveElementFromScenario(building);
                building.Dispose();
            }

            return buildingPlacedSuccessfully;
        }

        private const int NUM_OF_SCVS = 4;
    }
}
