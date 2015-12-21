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
            extendedComponent.RegisterElementFactory<Building>(SCV.SCV_TYPE_NAME, this.CreateScv);
            extendedComponent.RegisterElementFactory<Building>(Marine.MARINE_TYPE_NAME, this.CreateMarine);
            extendedComponent.RegisterElementFactory<Building>(Goliath.GOLIATH_TYPE_NAME, this.CreateGoliath);
            extendedComponent.RegisterElementFactory<Building>(ComsatStation.COMSATSTATION_TYPE_NAME, this.CreateComsatStation);
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
            Barracks commandCenter = new Barracks();
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
        /// Creates an SCV in the given building.
        /// </summary>
        /// <param name="factoryBuilding">The building in which the SCV is created.</param>
        /// <returns>True if the SCV has been created successfully; otherwise false.</returns>
        private bool CreateScv(Building factoryBuilding)
        {
            if (factoryBuilding == null) { throw new ArgumentNullException("factoryBuilding"); }
            if (factoryBuilding.Scenario == null) { throw new ArgumentException("The factory building is not added to a scenario!"); }

            EntityNeighbourhoodIterator cellIterator = new EntityNeighbourhoodIterator(factoryBuilding);
            IEnumerator<ICell> cellEnumerator = cellIterator.GetEnumerator();

            /// Create the SCV
            SCV scv = new SCV();
            factoryBuilding.Scenario.AddElementToScenario(scv);
            if (factoryBuilding.Owner != null) { factoryBuilding.Owner.AddUnit(scv); }

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

            /// Remove the SCV if there is no more place on the map.
            if (!scvPlacedSuccessfully)
            {
                if (factoryBuilding.Owner != null) { factoryBuilding.Owner.RemoveUnit(scv); }
                factoryBuilding.Scenario.RemoveElementFromScenario(scv);
                scv.Dispose();
            }

            return scvPlacedSuccessfully;
        }

        /// <summary>
        /// Creates a Marine in the given building.
        /// </summary>
        /// <param name="factoryBuilding">The building in which the Marine is created.</param>
        /// <returns>True if the Marine has been created successfully; otherwise false.</returns>
        private bool CreateMarine(Building factoryBuilding)
        {
            if (factoryBuilding == null) { throw new ArgumentNullException("factoryBuilding"); }
            if (factoryBuilding.Scenario == null) { throw new ArgumentException("The factory building is not added to a scenario!"); }

            EntityNeighbourhoodIterator cellIterator = new EntityNeighbourhoodIterator(factoryBuilding);
            IEnumerator<ICell> cellEnumerator = cellIterator.GetEnumerator();

            /// Create the Marine
            Marine marine = new Marine();
            factoryBuilding.Scenario.AddElementToScenario(marine);
            if (factoryBuilding.Owner != null) { factoryBuilding.Owner.AddUnit(marine); }

            /// Search a place for the new Marine on the map.
            bool marinePlacedSuccessfully = false;
            while (cellEnumerator.MoveNext())
            {
                if (marine.AttachToMap(cellEnumerator.Current.MapCoords))
                {
                    marinePlacedSuccessfully = true;
                    break;
                }
            }

            /// Remove the Marine if there is no more place on the map.
            if (!marinePlacedSuccessfully)
            {
                if (factoryBuilding.Owner != null) { factoryBuilding.Owner.RemoveUnit(marine); }
                factoryBuilding.Scenario.RemoveElementFromScenario(marine);
                marine.Dispose();
            }

            return marinePlacedSuccessfully;
        }

        /// <summary>
        /// Creates a Goliath in the given building.
        /// </summary>
        /// <param name="factoryBuilding">The building in which the Goliath is created.</param>
        /// <returns>True if the Goliath has been created successfully; otherwise false.</returns>
        private bool CreateGoliath(Building factoryBuilding)
        {
            if (factoryBuilding == null) { throw new ArgumentNullException("factoryBuilding"); }
            if (factoryBuilding.Scenario == null) { throw new ArgumentException("The factory building is not added to a scenario!"); }

            EntityNeighbourhoodIterator cellIterator = new EntityNeighbourhoodIterator(factoryBuilding);
            IEnumerator<ICell> cellEnumerator = cellIterator.GetEnumerator();

            /// Create the Goliath
            Goliath goliath = new Goliath();
            factoryBuilding.Scenario.AddElementToScenario(goliath);
            if (factoryBuilding.Owner != null) { factoryBuilding.Owner.AddUnit(goliath); }

            /// Search a place for the new Goliath on the map.
            bool goliathPlacedSuccessfully = false;
            while (cellEnumerator.MoveNext())
            {
                if (goliath.AttachToMap(cellEnumerator.Current.MapCoords))
                {
                    goliathPlacedSuccessfully = true;
                    break;
                }
            }

            /// Remove the Goliath if there is no more place on the map.
            if (!goliathPlacedSuccessfully)
            {
                if (factoryBuilding.Owner != null) { factoryBuilding.Owner.RemoveUnit(goliath); }
                factoryBuilding.Scenario.RemoveElementFromScenario(goliath);
                goliath.Dispose();
            }

            return goliathPlacedSuccessfully;
        }

        /// <summary>
        /// Creates a ComsatStation addon for the given main building.
        /// </summary>
        /// <param name="mainBuilding">The main building for which the ComsatStation addon is created.</param>
        /// <returns>True if the ComsatStation addon has been created successfully; otherwise false.</returns>
        private bool CreateComsatStation(Building mainBuilding)
        {
            if (mainBuilding == null) { throw new ArgumentNullException("mainBuilding"); }
            if (mainBuilding.Scenario == null) { throw new ArgumentException("The main building is not added to a scenario!", "mainBuilding"); }
            if (mainBuilding.MapObject == null) { throw new ArgumentException("The main building is detached from the map!", "mainBuilding"); }

            /// Create the ComsatStation.
            ComsatStation comsatStation = new ComsatStation();
            mainBuilding.Scenario.AddElementToScenario(comsatStation);

            /// Try to attach the ComsatStation to the map.
            RCIntVector relativeAddonPosition = mainBuilding.BuildingType.GetRelativeAddonPosition(mainBuilding.Scenario.Map, comsatStation.AddonType);
            RCIntVector addonPosition = mainBuilding.MapObject.QuadraticPosition.Location + relativeAddonPosition;
            bool comsatStationPlacedSuccessfully = comsatStation.AttachToMap(mainBuilding.Scenario.Map.GetQuadTile(addonPosition));
            if (!comsatStationPlacedSuccessfully)
            {
                mainBuilding.Scenario.RemoveElementFromScenario(comsatStation);
                comsatStation.Dispose();
            }

            return comsatStationPlacedSuccessfully;
        }

        private const int NUM_OF_SCVS = 6;
    }
}
