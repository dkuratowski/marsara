using System;
using System.Collections.Generic;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Commands;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.Terran.Buildings;
using RC.Engine.Simulator.Terran.Units;
using RC.Engine.Simulator.Terran.Addons;

namespace RC.Engine.Simulator.Terran
{
    /// <summary>
    /// This class represents the command executor plugin for the Terran race.
    /// </summary>
    [Plugin(typeof(ICommandExecutor))]
    class CommandExecutorPlugin : IPlugin<ICommandExecutorPluginInstall>
    {
        /// <summary>
        /// Constructs a CommandExecutorPlugin instance.
        /// </summary>
        public CommandExecutorPlugin()
        {
        }

        /// <see cref="IPlugin<T>.Install"/>
        public void Install(ICommandExecutorPluginInstall extendedComponent)
        {
            /// Terran SCV
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Move, SCV.SCV_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Stop, SCV.SCV_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Attack, SCV.SCV_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Patrol, SCV.SCV_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Hold, SCV.SCV_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Undefined, SCV.SCV_TYPE_NAME));

            /// Terran Marine
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Move, Marine.MARINE_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Stop, Marine.MARINE_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Attack, Marine.MARINE_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Patrol, Marine.MARINE_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Hold, Marine.MARINE_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Undefined, Marine.MARINE_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new SpecialAbilityExecutionFactory<Marine>(TerranAbilities.STIMPACKS, Marine.MARINE_TYPE_NAME,
                (recipientMarines, targetPosition, targetEntityID, parameter) =>
                    TerranAbilities.MarineStimPacksFactoryMethod(recipientMarines)));

            /// Terran Goliath
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Move, Goliath.GOLIATH_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Stop, Goliath.GOLIATH_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Attack, Goliath.GOLIATH_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Patrol, Goliath.GOLIATH_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Hold, Goliath.GOLIATH_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Undefined, Goliath.GOLIATH_TYPE_NAME));

            /// Terran Command Center
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Move, CommandCenter.COMMANDCENTER_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Stop, CommandCenter.COMMANDCENTER_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Undefined, CommandCenter.COMMANDCENTER_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new ProductionExecutionFactory(CommandCenter.COMMANDCENTER_TYPE_NAME, SCV.SCV_TYPE_NAME, ComsatStation.COMSATSTATION_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new ProductionCancelExecutionFactory(CommandCenter.COMMANDCENTER_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new LiftOffExecutionFactory(CommandCenter.COMMANDCENTER_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new LandExecutionFactory(CommandCenter.COMMANDCENTER_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new ConstructionCancelExecutionFactory(CommandCenter.COMMANDCENTER_TYPE_NAME));

            /// Terran Comsat Station
            extendedComponent.RegisterCommandExecutionFactory(new ConstructionCancelExecutionFactory(ComsatStation.COMSATSTATION_TYPE_NAME));

            /// Terran Barracks
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Move, Barracks.BARRACKS_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Stop, Barracks.BARRACKS_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Undefined, Barracks.BARRACKS_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new ProductionExecutionFactory(Barracks.BARRACKS_TYPE_NAME, Marine.MARINE_TYPE_NAME, Goliath.GOLIATH_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new ProductionCancelExecutionFactory(Barracks.BARRACKS_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new LiftOffExecutionFactory(Barracks.BARRACKS_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new LandExecutionFactory(Barracks.BARRACKS_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new ConstructionCancelExecutionFactory(Barracks.BARRACKS_TYPE_NAME));

            /// TEST:
            extendedComponent.RegisterCommandExecutionFactory(new ProductionExecutionFactory(ComsatStation.COMSATSTATION_TYPE_NAME,
                TerranUpgrades.INFANTRY_WEAPONS_1,
                TerranUpgrades.INFANTRY_WEAPONS_2,
                TerranUpgrades.INFANTRY_WEAPONS_3,
                TerranUpgrades.INFANTRY_ARMOR_1,
                TerranUpgrades.INFANTRY_ARMOR_2,
                TerranUpgrades.INFANTRY_ARMOR_3,
                TerranAbilities.STIMPACKS));
            extendedComponent.RegisterCommandExecutionFactory(new ProductionCancelExecutionFactory(ComsatStation.COMSATSTATION_TYPE_NAME));
        }

        /// <see cref="IPlugin<T>.Uninstall"/>
        public void Uninstall(ICommandExecutorPluginInstall extendedComponent)
        {
            /// TODO: Write uninstallation code here!
        }
    }
}
