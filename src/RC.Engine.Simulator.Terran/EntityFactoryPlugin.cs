using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.ComponentModel;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.Terran.Buildings;
using RC.Engine.Simulator.Terran.Units;

namespace RC.Engine.Simulator.Terran
{
    /// <summary>
    /// This class represents the entity factory plugin for the Terran race.
    /// </summary>
    [Plugin(typeof(IEntityFactory))]
    class EntityFactoryPlugin : IPlugin<IEntityFactoryPluginInstall>
    {
        /// <summary>
        /// Constructs an EntityFactoryPlugin instance.
        /// </summary>
        public EntityFactoryPlugin()
        {
        }

        /// <see cref="IPlugin<T>.Install"/>
        public void Install(IEntityFactoryPluginInstall extendedComponent)
        {
            /// TODO: Write installation code here!
            extendedComponent.RegisterPlayerInitializer(RaceEnum.Terran, this.TerranInitializer);
            extendedComponent.RegisterEntityCreator(SCV.SCV_TYPE_NAME, this.CreateScv);
        }

        /// <see cref="IPlugin<T>.Uninstall"/>
        public void Uninstall(IEntityFactoryPluginInstall extendedComponent)
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
            CommandCenter commandCenter = new CommandCenter();
            scenario.AddElementToScenario(commandCenter);
            player.AddBuilding(commandCenter);
            commandCenter.AttachToMap(scenario.Map.GetQuadTile(player.QuadraticStartPosition.Location));
            commandCenter.MotionControl.Fix();

            /// Find place for the given number of SCVs using an EntityNeighbourhoodIterator.
            EntityNeighbourhoodIterator cellIterator = new EntityNeighbourhoodIterator(commandCenter);
            IEnumerator<ICell> cellEnumerator = cellIterator.GetEnumerator();
            for (int scvCount = 0; scvCount < NUM_OF_SCVS; scvCount++)
            {
                /// Create the next SCV
                SCV scv = new SCV();
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
        /// Creates an SCV for the given player.
        /// </summary>
        /// <param name="player">The owner player of the new SCV.</param>
        /// <param name="producer">The producer entity.</param>
        /// <returns>True if the SCV has been created successfully; otherwise false.</returns>
        private bool CreateScv(Player player, Entity producer)
        {
            if (player == null) { throw new ArgumentNullException("player"); }
            if (producer == null) { throw new ArgumentNullException("producer"); }
            if (producer.Scenario != player.Scenario) { throw new ArgumentException("Mismatch between the scenario of the player and the producer entity!");}

            Scenario scenario = player.Scenario;
            EntityNeighbourhoodIterator cellIterator = new EntityNeighbourhoodIterator(producer);
            IEnumerator<ICell> cellEnumerator = cellIterator.GetEnumerator();

            /// Create the SCV
            SCV scv = new SCV();
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

            /// Remove the SCV if there is no more place on the map.
            if (!scvPlacedSuccessfully)
            {
                player.RemoveUnit(scv);
                scenario.RemoveElementFromScenario(scv);
                scv.Dispose();
            }

            return scvPlacedSuccessfully;
        }

        private const int NUM_OF_SCVS = 5;
    }
}
