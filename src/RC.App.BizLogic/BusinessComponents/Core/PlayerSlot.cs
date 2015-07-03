using System;
using RC.App.BizLogic.Views;
using RC.Common;
using RC.Engine.Simulator.Engine;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// Represents a player slot.
    /// </summary>
    class PlayerSlot : IPlayerSlot
    {
        /// <summary>
        /// Constructs a PlayerSlot instance with no connected player.
        /// </summary>
        /// <param name="randomConnector">Method used to connect a player randomly.</param>
        /// <param name="manualConnector">Method used to connect a player manually.</param>
        /// <param name="disconnector">Method used to disconnect a player.</param>
        public PlayerSlot(Func<RaceEnum, Player> randomConnector,
                          Func<RaceEnum, PlayerEnum, StartLocation, Player> manualConnector,
                          Action<Player> disconnector)
        {
            if (randomConnector == null) { throw new ArgumentNullException("randomConnector"); }
            if (manualConnector == null) { throw new ArgumentNullException("manualConnector"); }
            if (disconnector == null) { throw new ArgumentNullException("disconnector"); }

            this.state = PlayerSlotStateEnum.Opened;
            this.player = null;
            this.race = RaceEnum.Terran;

            this.randomConnector = randomConnector;
            this.manualConnector = manualConnector;
            this.disconnector = disconnector;
        }

        #region IPlayerSlot methods

        /// <see cref="IPlayerSlot.State"/>
        public PlayerSlotStateEnum State { get { return this.state; } }

        /// <see cref="IPlayerSlot.Player"/>
        public PlayerEnum Player
        {
            get
            {
                if (this.state != PlayerSlotStateEnum.Connected) { throw new InvalidOperationException("No connected player on slot!"); }
                return (PlayerEnum)this.player.PlayerIndex;
            }
        }

        /// <see cref="IPlayerSlot.Race"/>
        public RaceEnum Race
        {
            get
            {
                if (this.state != PlayerSlotStateEnum.Connected) { throw new InvalidOperationException("No connected player on slot!"); }
                return this.race;
            }
        }

        /// <see cref="IPlayerSlot.StartPosition"/>
        public RCNumVector StartPosition
        {
            get
            {
                if (this.state != PlayerSlotStateEnum.Connected) { throw new InvalidOperationException("No connected player on slot!"); }
                return this.player.StartPosition;
            }
        }

        /// <see cref="IPlayerSlot.StartLocation"/>
        public StartLocation StartLocation
        {
            get
            {
                if (this.state != PlayerSlotStateEnum.Connected) { throw new InvalidOperationException("No connected player on slot!"); }
                return this.player.StartLocation;
            }
        }

        /// <see cref="IPlayerSlot.ConnectRandomPlayer"/>
        public void ConnectRandomPlayer(RaceEnum race)
        {
            Player player = this.randomConnector(race);
            this.state = PlayerSlotStateEnum.Connected;
            this.player = player;
            this.race = race;
        }

        /// <see cref="IPlayerSlot.ConnectPlayer"/>
        public void ConnectPlayer(RaceEnum race, PlayerEnum index, StartLocation startLocation)
        {
            Player player = this.manualConnector(race, index, startLocation);
            this.state = PlayerSlotStateEnum.Connected;
            this.race = race;
            this.player = player;
        }

        /// <see cref="IPlayerSlot.DisconnectPlayer"/>
        public void DisconnectPlayer()
        {
            if (this.state != PlayerSlotStateEnum.Connected) { throw new InvalidOperationException("No connected player on slot!"); }

            this.disconnector(this.player);
            this.state = PlayerSlotStateEnum.Opened;
            this.player = null;
            this.race = RaceEnum.Terran;
        }

        #endregion IPlayerSlot methods

        /// <summary>
        /// The state of this slot.
        /// </summary>
        private PlayerSlotStateEnum state;

        /// <summary>
        /// The player that is connected to this slot.
        /// </summary>
        private Player player;

        /// <summary>
        /// The race of the player that is connected to this slot.
        /// </summary>
        private RaceEnum race;

        /// <summary>
        /// Method used to connect a player randomly.
        /// </summary>
        private readonly Func<RaceEnum, Player> randomConnector;

        /// <summary>
        /// Method used to connect a player manually.
        /// </summary>
        private readonly Func<RaceEnum, PlayerEnum, StartLocation, Player> manualConnector;

        /// <summary>
        /// Method used to disconnect a player manually.
        /// </summary>
        private readonly Action<Player> disconnector;
    }
}
