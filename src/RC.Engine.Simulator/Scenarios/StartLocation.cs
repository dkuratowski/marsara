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
        /// <param name="quadCoords">The quadratic coordinates of the start location.</param>
        /// <param name="playerIndex">The index of the player that this start location belongs to.</param>
        public StartLocation(RCIntVector quadCoords, int playerIndex)
            : base(STARTLOCATION_TYPE_NAME, quadCoords)
        {
            if (playerIndex < 0 || playerIndex >= Player.MAX_PLAYERS) { throw new ArgumentOutOfRangeException("playerIndex"); }
            if (quadCoords == RCIntVector.Undefined) { throw new ArgumentNullException("quadCoords"); }
            this.playerIndex = this.ConstructField<int>("playerIndex");
            this.playerIndex.Write(playerIndex);
        }

        /// <summary>
        /// Gets the index of the player that this start location belongs to.
        /// </summary>
        public IValueRead<int> PlayerIndex { get { return this.playerIndex; } }

        /// <see cref="Entity.OnAddedToScenarioImpl"/>
        protected override void OnAddedToScenarioImpl()
        {
            base.OnAddedToScenarioImpl();
            this.SetCurrentAnimation(ANIMATION_NAME);
        }

        #region Heaped members

        /// <summary>
        /// The index of the player that this start location belongs to.
        /// </summary>
        private HeapedValue<int> playerIndex;

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
