using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.PublicInterfaces;
using RC.Common;
using RC.Engine.Simulator.Scenarios;

namespace RC.App.BizLogic.Core
{
    /// <summary>
    /// This class is responsible for handling the players of the active scenario.
    /// </summary>
    class PlayerManager : IPlayerManager
    {
        /// <summary>
        /// Constructs a PlayerManager for the given scenario.
        /// </summary>
        /// <param name="scenario">The scenario for which this PlayerManager is responsible for.</param>
        public PlayerManager(Scenario scenario)
        {
            if (scenario == null) { throw new ArgumentNullException("scenario"); }
            if (scenario.HasPlayers) { throw new ArgumentException("The given scenario already has players!", "scenario"); }

            this.targetScenario = scenario;
            this.isLocked = false;
            this.freeStartLocations = new HashSet<StartLocation>(this.targetScenario.GetAllEntities<StartLocation>());
            this.freePlayerIndices = new HashSet<PlayerEnum>()
            {
                PlayerEnum.Player0, PlayerEnum.Player1, PlayerEnum.Player2, PlayerEnum.Player3,
                PlayerEnum.Player4, PlayerEnum.Player5, PlayerEnum.Player6, PlayerEnum.Player7,
            };
            this.slots = new PlayerSlot[this.freeStartLocations.Count];
            for (int slotIdx = 0; slotIdx < this.slots.Length; slotIdx++)
            {
                this.slots[slotIdx] = new PlayerSlot(this.CreatePlayerRandomly,
                                                     this.CreatePlayerManually,
                                                     this.DeletePlayer);
            }
        }

        /// <summary>
        /// Locks this player manager so that no more manipulation on players of the underlying scenario is possible.
        /// </summary>
        public void Lock() { this.targetScenario.FinalizePlayers(); }

        #region IPlayerManager members

        /// <see cref="IPlayerManager.Item"/>
        public IPlayerSlot this[int index] { get { return this.slots[index]; } }

        /// <see cref="IPlayerManager.NumberOfSlots"/>
        public int NumberOfSlots { get { return this.slots.Length; } }

        #endregion IPlayerManager members

        /// <summary>
        /// Internal method that creates and adds a new player to the target scenario.
        /// The index and the start location of the created player are determined randomly.
        /// </summary>
        /// <param name="race">The race of the created player.</param>
        /// <returns>The index and the start location of the created player.</returns>
        private Player CreatePlayerRandomly(RaceEnum race)
        {
            if (this.freePlayerIndices.Count == 0) { throw new InvalidOperationException("No more free player indices!"); }
            if (this.freeStartLocations.Count == 0) { throw new InvalidOperationException("No more free start locations!"); }

            /// Determine the player index and start location randomly.
            PlayerEnum index = this.freePlayerIndices.ElementAt(RandomService.DefaultGenerator.Next(this.freePlayerIndices.Count));
            StartLocation startLocation = this.freeStartLocations.ElementAt(RandomService.DefaultGenerator.Next(this.freeStartLocations.Count));

            /// Create and add the new player to the target scenario.
            this.targetScenario.CreatePlayer((int)index, startLocation, race);
            Player createdPlayer = this.targetScenario.GetPlayer((int)index);

            /// Remove the determined player index and start location from the appropriate sets.
            this.freePlayerIndices.Remove(index);
            this.freeStartLocations.Remove(startLocation);
            return createdPlayer;
        }

        /// <summary>
        /// Internal method that creates and adds a new player to the underlying scenario.
        /// The index and the start location of the created player are determined manually.
        /// </summary>
        /// <param name="race">The race of the created player.</param>
        /// <param name="index">The index of the created player.</param>
        /// <param name="startLocation">The start location of the created player.</param>
        private Player CreatePlayerManually(RaceEnum race, PlayerEnum index, StartLocation startLocation)
        {
            if (index == PlayerEnum.Neutral) { throw new ArgumentException("index cannot be PlayerEnum.Neutral!", "index"); }
            if (startLocation == null) { throw new ArgumentNullException("startLocation"); }
            if (startLocation.Scenario != this.targetScenario) { throw new ArgumentException("The given start location belongs to another scenario!", "startLocation"); }

            /// Check if the incoming player index and start location are not assigned to another player slot.
            if (!this.freePlayerIndices.Contains(index)) { throw new InvalidOperationException("The given player index has already been assigned to another slot!"); }
            if (!this.freeStartLocations.Contains(startLocation)) { throw new InvalidOperationException("The given start location has already been assigned to another slot!"); }

            /// Create and add the new player to the target scenario.
            this.targetScenario.CreatePlayer((int)index, startLocation, race);
            Player createdPlayer = this.targetScenario.GetPlayer((int)index);

            /// Remove the determined player index and start location from the appropriate sets.
            this.freePlayerIndices.Remove(index);
            this.freeStartLocations.Remove(startLocation);
            return createdPlayer;
        }

        /// <summary>
        /// Internal method that deletes the given player from the underlying scenario.
        /// </summary>
        /// <param name="player">The player to delete.</param>
        private void DeletePlayer(Player player)
        {
            if (player == null) { throw new ArgumentNullException("player"); }
            if (this.targetScenario.GetPlayer(player.PlayerIndex) != player) { throw new ArgumentException("The given player belongs to another scenario!", "player"); }

            /// Check if the incoming player has not yet been deleted.
            if (this.freePlayerIndices.Contains((PlayerEnum)player.PlayerIndex) || this.freeStartLocations.Contains(player.StartLocation))
            {
                throw new InvalidOperationException("The given player has already been deleted from the target scenario!");
            }

            /// Delete the player from the target scenario.
            this.targetScenario.DeletePlayer(player.PlayerIndex);

            /// Put the player index and the start location back to the appropriate sets.
            this.freePlayerIndices.Add((PlayerEnum)player.PlayerIndex);
            this.freeStartLocations.Add(player.StartLocation);
        }

        /// <summary>
        /// Reference to the target scenario that this PlayerManager is responsible for.
        /// </summary>
        private Scenario targetScenario;

        /// <summary>
        /// Set of the free start locations of the target scenario.
        /// </summary>
        private HashSet<StartLocation> freeStartLocations;

        /// <summary>
        /// Set of the free player indices.
        /// </summary>
        private HashSet<PlayerEnum> freePlayerIndices;

        /// <summary>
        /// The list of the player slots.
        /// </summary>
        private PlayerSlot[] slots;

        /// <summary>
        /// This flag indicates whether this player manager has already been locked.
        /// </summary>
        private bool isLocked;
    }
}
