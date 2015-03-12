using RC.Common.ComponentModel;
using RC.Engine.Simulator.Scenarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Commands
{
    /// <summary>
    /// This interface is used by the plugins of the command executor to install themselves.
    /// </summary>
    [PluginInstallInterface]
    public interface ICommandExecutorPluginInstall
    {
        /// <summary>
        /// Registers a player initializer method for the given race.
        /// </summary>
        /// <param name="race">The race that the initializer belongs to.</param>
        /// <param name="initializer">The initializer method.</param>
        void RegisterPlayerInitializer(RaceEnum race, Action<Player> initializer);

        /// <summary>
        /// Registers the given command execution factory.
        /// </summary>
        /// <param name="factory">The registered factory.</param>
        void RegisterCommandExecutionFactory(ICommandExecutionFactory factory);
    }
}
