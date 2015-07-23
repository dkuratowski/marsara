using System.Collections.Generic;
using RC.Common;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Engine;

namespace RC.Engine.Simulator.Commands
{
    /// <summary>
    /// Common interface of the command execution factories.
    /// </summary>
    public interface ICommandExecutionFactory
    {
        /// <summary>
        /// Gets the type of the commands for which this factory creates executions or null if this factory creates executions
        /// of unknown commands.
        /// </summary>
        string CommandType { get; }

        /// <summary>
        /// Gets the type of the entities for which this factory creates executions.
        /// </summary>
        string EntityType { get; }

        /// <summary>
        /// Gets the availability of the command from the point of view of this factory for the given entity set.
        /// </summary>
        /// <param name="entitySet">The entity set.</param>
        /// <returns>The availability of the command from the point of view of this factory for the given entity set.</returns>
        AvailabilityEnum GetCommandAvailability(RCSet<Entity> entitySet);

        /// <summary>
        /// Starts a command execution on the given entity set with the given parameters.
        /// </summary>
        /// <param name="entitySet">The entity set.</param>
        /// <param name="targetPosition">The target position.</param>
        /// <param name="targetEntityID">The ID of the target entity or -1 if not defined.</param>
        /// <param name="parameter">The optional parameter.</param>
        void StartCommandExecution(RCSet<Entity> entitySet, RCNumVector targetPosition, int targetEntityID, string parameter);
    }
}
