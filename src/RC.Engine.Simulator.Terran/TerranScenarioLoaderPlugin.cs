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
            extendedComponent.RegisterPlayerInitializer(RaceEnum.Terran, this.TerranInitializer);
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

        /// <see cref="Player.Initializer"/>
        private void TerranInitializer(Player player)
        {
            if (player == null) { throw new ArgumentNullException("player"); }

            /// Add a Terran Command Center to the position of the start location.
            Scenario scenario = player.StartLocation.Scenario;
            CommandCenter commandCenter = new CommandCenter();
            scenario.AddEntityToScenario(commandCenter);
            player.AddBuilding(commandCenter);
            scenario.AttachEntityToMap(commandCenter, scenario.Map.GetQuadTile(player.StartLocation.LastKnownQuadCoords));

            /// Find place for the given number of SCVs using an EntityNeighbourhoodIterator.
            EntityNeighbourhoodIterator cellIterator = new EntityNeighbourhoodIterator(commandCenter);
            IEnumerator<ICell> cellEnumerator = cellIterator.GetEnumerator();
            for (int scvCount = 0; scvCount < NUM_OF_SCVS; scvCount++)
            {
                /// Create the next SCV
                SCV scv = new SCV();
                scenario.AddEntityToScenario(scv);
                player.AddUnit(scv);

                /// Search a place for the new SCV on the map.
                bool scvPlacedSuccessfully = false;
                while (cellEnumerator.MoveNext())
                {
                    if (scenario.AttachEntityToMap(scv, cellEnumerator.Current.MapCoords))
                    {
                        scvPlacedSuccessfully = true;
                        break;
                    }
                }

                /// Remove the SCV and stop initializing if there is no more place on the map.
                if (!scvPlacedSuccessfully)
                {
                    player.RemoveUnit(scv);
                    scenario.RemoveEntityFromScenario(scv);
                    scv.Dispose();
                    break;
                }
            }
        }

        private const int NUM_OF_SCVS = 5;
    }
}
