using System.Collections.Generic;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.Commands;
using RC.Engine.Simulator.Engine;

namespace RC.Engine.Simulator.ComponentInterfaces
{
    /// <summary>
    /// Enumerates the possible availability states of a feature (e.g. command, building, research, upgrade...).
    /// </summary>
    public enum AvailabilityEnum
    {
        Unavailable = 0,        /// The feature is unavailable.
        Disabled = 1,           /// The feature is available but currently disabled.
        Enabled = 2,            /// The feature is available and currently enabled.
    }

    /// <summary>
    /// Interface for executing the incoming commands.
    /// </summary>
    [ComponentInterface]
    public interface ICommandExecutor
    {
        /// <summary>
        /// Gets the availability of the given command for the given set of entities based on the current state
        /// of the given scenario.
        /// </summary>
        /// <param name="scenario">The scenario of the entities.</param>
        /// <param name="commandType">The type of the command.</param>
        /// <param name="entityIDs">The IDs of the entities.</param>
        /// <returns>The availability of the given command for the given set of entities.</returns>
        AvailabilityEnum GetCommandAvailability(Scenario scenario, string commandType, IEnumerable<int> entityIDs);

        /// <summary>
        /// Starts the execution of the given command on the given scenario.
        /// </summary>
        /// <param name="scenario">The scenario on which to execute the command.</param>
        /// <param name="command">The command to be executed.</param>
        void StartExecution(Scenario scenario, RCCommand command);

        /// <summary>
        /// Gets the commands that are currently being executed by the given set of entities.
        /// </summary>
        /// <param name="scenario">The scenario of the entities</param>
        /// <param name="entityIDs">The IDs of the entities.</param>
        /// <returns>The commands that are currently being executed by the given set of entities.</returns>
        RCSet<string> GetCommandsBeingExecuted(Scenario scenario, IEnumerable<int> entityIDs);
    }
}
