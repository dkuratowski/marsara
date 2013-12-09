using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Scenarios
{
    /// <summary>
    /// Represents a player of a scenario.
    /// </summary>
    public class Player // TODO: derive from HeapedObject
    {
        /// <summary>
        /// Constructs a new player instance.
        /// </summary>
        /// <param name="playerIndex">The index of the player.</param>
        internal Player(int playerIndex)
        {
            if (playerIndex < 0 || playerIndex >= Player.MAX_PLAYERS) { throw new ArgumentOutOfRangeException("playerIndex"); }
            this.playerIndex = playerIndex;
        }

        /// <summary>
        /// Gets the index of this player.
        /// </summary>
        public int PlayerIndex { get { return this.playerIndex; } }

        /// <summary>
        /// The index of the player.
        /// </summary>
        private int playerIndex;

        /// <summary>
        /// The maximum number of players.
        /// </summary>
        public const int MAX_PLAYERS = 8;
    }
}
