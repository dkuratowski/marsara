using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Scenarios
{
    /// <summary>
    /// Represents a start location of a scenario.
    /// </summary>
    public class StartLocation : QuadEntity
    {
        /// <summary>
        /// Constructs a start location instance.
        /// </summary>
        /// <param name="playerIndex">The index of the player that this start location belongs to.</param>
        public StartLocation(int playerIndex)
            : base(STARTLOCATION_TYPE_NAME)
        {
            if (playerIndex < 0 || playerIndex >= Player.MAX_PLAYERS) { throw new ArgumentOutOfRangeException("playerIndex"); }
            this.playerIndex = this.ConstructField<int>("playerIndex");
            this.playerIndex.Write(playerIndex);
            this.SetCurrentAnimation(ANIMATION_NAME);
        }

        /// <summary>
        /// Gets the index of the player that this start location belongs to.
        /// </summary>
        public int PlayerIndex { get { return this.playerIndex.Read(); } }

        #region Heaped members

        /// <summary>
        /// The index of the player that this start location belongs to.
        /// </summary>
        private readonly HeapedValue<int> playerIndex;

        #endregion Heaped members

        /// <summary>
        /// The name of the StartLocation element type.
        /// </summary>
        public const string STARTLOCATION_TYPE_NAME = "StartLocation";

        /// <summary>
        /// The name of the show animation of the start location element.
        /// </summary>
        public const string ANIMATION_NAME = "Show";
    }
}
