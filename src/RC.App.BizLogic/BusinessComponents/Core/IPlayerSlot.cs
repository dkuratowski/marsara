using System;
using RC.App.BizLogic.Views;
using RC.Common;
using RC.Engine.Simulator.Engine;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// Enumerates the possible states of a player slot.
    /// </summary>
    enum PlayerSlotStateEnum
    {
        Closed = 0,     // The player slot is closed (currently not used)
        Opened = 1,     // The player slot is opened but no connected player
        Connected = 2   // The player slot is opened and a player is connected to that slot
    }

    /// <summary>
    /// Interface of the player slots of a multiplayer game.
    /// </summary>
    interface IPlayerSlot
    {
        /// <summary>
        /// Gets the state of this slot.
        /// </summary>
        PlayerSlotStateEnum State { get; }

        /// <summary>
        /// Gets the player that is connected to this slot.
        /// </summary>
        /// <exception cref="InvalidOperationException">If there is no connected player.</exception>
        PlayerEnum Player { get; }

        /// <summary>
        /// Gets the race of the player that is connected to this slot.
        /// </summary>
        /// <exception cref="InvalidOperationException">If there is no connected player.</exception>
        RaceEnum Race { get; }

        /// <summary>
        /// Gets the start position of the player that is connected to this slot.
        /// </summary>
        /// <exception cref="InvalidOperationException">If there is no connected player.</exception>
        RCNumVector StartPosition { get; }

        /// <summary>
        /// Gets the start location of the player that is connected to this slot.
        /// </summary>
        /// <exception cref="InvalidOperationException">If there is no connected player.</exception>
        StartLocation StartLocation { get; }

        /// <summary>
        /// Connects a player to this slot by randomly selecting a start location and a player index.
        /// </summary>
        /// <param name="race">The race of the connected player.</param>
        void ConnectRandomPlayer(RaceEnum race);

        /// <summary>
        /// Connects a player to this slot by manually selecting a start location and a player index.
        /// </summary>
        /// <param name="race">The race of the connected player.</param>
        /// <param name="index">The selected index of the connected player.</param>
        /// <param name="startLocation">The selected start location of the connected player.</param>
        /// <exception cref="ArgumentException">If index is PlayerEnum.Neutral.</exception>
        /// <exception cref="InvalidOperationException">
        /// If at least one of the given index and start location has already been assigned to another slot.
        /// </exception>
        void ConnectPlayer(RaceEnum race, PlayerEnum index, StartLocation startLocation);

        /// <summary>
        /// Disconnect the currently connected player from this slot.
        /// </summary>
        /// <exception cref="InvalidOperationException">If there is no connected player.</exception>
        void DisconnectPlayer();
    }
}
