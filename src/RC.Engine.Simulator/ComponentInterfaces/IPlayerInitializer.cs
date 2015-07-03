using RC.Common.ComponentModel;
using RC.Engine.Simulator.Engine;

namespace RC.Engine.Simulator.ComponentInterfaces
{
    /// <summary>
    /// Interface of the player initializer component.
    /// </summary>
    [ComponentInterface]
    interface IPlayerInitializer
    {
        /// <summary>
        /// Initializes the given player with the given race.
        /// </summary>
        /// <param name="player">The player to initialize.</param>
        /// <param name="race">The race of the player.</param>
        void Initialize(Player player, RaceEnum race);
    }
}
