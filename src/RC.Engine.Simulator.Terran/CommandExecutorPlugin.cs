using System;
using System.Collections.Generic;
using RC.Common.ComponentModel;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Commands;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.Terran.Buildings;
using RC.Engine.Simulator.Terran.Units;

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
            /// TODO: Write installation code here!
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Move, SCV.SCV_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Stop, SCV.SCV_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Attack, SCV.SCV_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Patrol, SCV.SCV_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Hold, SCV.SCV_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Undefined, SCV.SCV_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new ProductionExecutionFactory(CommandCenter.COMMANDCENTER_TYPE_NAME, SCV.SCV_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new ProductionCancelExecutionFactory(CommandCenter.COMMANDCENTER_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new LiftOffExecutionFactory(CommandCenter.COMMANDCENTER_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new LandExecutionFactory(CommandCenter.COMMANDCENTER_TYPE_NAME));
        }

        /// <see cref="IPlugin<T>.Uninstall"/>
        public void Uninstall(ICommandExecutorPluginInstall extendedComponent)
        {
            /// TODO: Write uninstallation code here!
        }
    }
}
