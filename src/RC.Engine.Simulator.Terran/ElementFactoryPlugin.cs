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
            /// TODO: Write installation code here!
            extendedComponent.RegisterPlayerInitializer(RaceEnum.Terran, this.TerranInitializer);
            extendedComponent.RegisterElementFactory<Building>(SCV.SCV_TYPE_NAME, this.CreateUnit<SCV>);
            extendedComponent.RegisterElementFactory<Building>(Marine.MARINE_TYPE_NAME, this.CreateUnit<Marine>);
            extendedComponent.RegisterElementFactory<Building>(Goliath.GOLIATH_TYPE_NAME, this.CreateUnit<Goliath>);
            extendedComponent.RegisterElementFactory<Building>("Wraith", building => true);
            extendedComponent.RegisterElementFactory<Building>("Dropship", building => true);
            extendedComponent.RegisterElementFactory<Building>(ComsatStation.COMSATSTATION_TYPE_NAME, this.CreateAddon<ComsatStation>);
            extendedComponent.RegisterElementFactory<Building>(ControlTower.CONTROLTOWER_TYPE_NAME, this.CreateAddon<ControlTower>);
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
            Starport commandCenter = new Starport();
            //CommandCenter commandCenter = new CommandCenter();
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
                Goliath scv = new Goliath();
                //SCV scv = new SCV();
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

        private const int NUM_OF_SCVS = 4;
    }
}
