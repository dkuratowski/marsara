using RC.Common.ComponentModel;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.Metadata;

namespace RC.Engine.Simulator.ComponentInterfaces
{
    /// <summary>
    /// Interface of the entity factory component that is responsible for instantiating entities and initializing players.
    /// </summary>
    [ComponentInterface]
    public interface IEntityFactory
    {
        /// <summary>
        /// Initializes the given player with the given race.
        /// </summary>
        /// <param name="player">The player to initialize.</param>
        /// <param name="race">The race of the player.</param>
        void InitializePlayer(Player player, RaceEnum race);

        /// <summary>
        /// Creates an entity of the given type for the given player using the given producer entity.
        /// </summary>
        /// <param name="entityType">The type of the entity to be created.</param>
        /// <param name="player">The player that will be the owner of the entity.</param>
        /// <param name="producer">The producer entity.</param>
        /// <returns>True if the entity has been successfully created.</returns>
        bool CreateEntity(IScenarioElementType entityType, Player player, Entity producer);
    }
}
