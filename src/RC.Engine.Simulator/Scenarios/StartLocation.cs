using RC.Common;
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
    public class StartLocation : Entity
    {
        /// <summary>
        /// Constructs a start location instance.
        /// </summary>
        /// <param name="initialPosition">The initial position of the start location.</param>
        /// <param name="playerIndex">The index of the player that this start location belongs to.</param>
        public StartLocation(RCNumVector initialPosition, int playerIndex)
            : base(ELEMENT_TYPE_NAME, initialPosition)
        {
            if (playerIndex < 0) { throw new ArgumentOutOfRangeException("playerIndex"); }
            this.playerIndex = this.ConstructField<int>("playerIndex");
            this.playerIndex.Write(playerIndex);
        }

        /// <summary>
        /// Gets the index of the player that this start location belongs to.
        /// </summary>
        public IValueRead<int> PlayerIndex { get { return this.playerIndex; } }

        #region Heaped members

        /// <summary>
        /// The index of the player that this start location belongs to.
        /// </summary>
        private HeapedValue<int> playerIndex;

        #endregion Heaped members

        /// <summary>
        /// The name of the StartLocation element type.
        /// </summary>
        public const string ELEMENT_TYPE_NAME = "StartLocation";
    }
}
