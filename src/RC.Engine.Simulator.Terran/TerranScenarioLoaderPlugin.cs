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

            Scenario scenario = player.StartLocation.Scenario;
            CommandCenter commandCenter = new CommandCenter(player.StartLocation.QuadCoords);
            scenario.AddEntity(commandCenter);
            player.AddBuilding(commandCenter);

            for (int i = 0; i < 5; i++)
            {
                SCV scv = new SCV();
                scenario.AddEntity(scv);
                player.AddUnit(scv);
                scv.AddToMap(new RCNumVector(commandCenter.Position.Left + i * (scv.ElementType.Area.Read().X + 2), commandCenter.Position.Bottom));
            }
        }
    }
}
