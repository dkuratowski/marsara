using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.Scenarios;
using RC.App.BizLogic.PublicInterfaces;

namespace RC.App.BizLogic.ComponentInterfaces
{
    /// <summary>
    /// Enumerates the possible game types.
    /// </summary>
    enum GameTypeEnum
    {
        Melee = 0,
        OneOnOne = 1
    }

    /// <summary>
    /// Enumerates the possible game speeds in FPS.
    /// </summary>
    enum GameSpeedEnum
    {
        Slowest = 6,
        Slower = 9,
        Slow = 12,
        Normal = 15,
        Fast = 18,
        Faster = 21,
        Fastest = 24
    }

    /// <summary>
    /// Contains informations about existing multiplayer games on the network.
    /// </summary>
    struct GameInfo
    {
        /// <summary>
        /// The ID of the game.
        /// </summary>
        public Guid GameID;

        /// <summary>
        /// The name of the account of the user who created the game.
        /// </summary>
        public string CreatorAccount;

        /// <summary>
        /// The type of the game.
        /// </summary>
        public GameTypeEnum GameType;

        /// <summary>
        /// The name of the map of the game.
        /// </summary>
        public string MapName;

        /// <summary>
        /// The size of the map of the game.
        /// </summary>
        public RCIntVector MapSize;

        /// <summary>
        /// The speed of the game.
        /// </summary>
        public GameSpeedEnum GameSpeed;

        /// <summary>
        /// The maximum number of players being able to participate in the game.
        /// </summary>
        public int MaxPlayers;

        /// <summary>
        /// The actual number of players currently being joined to the game.
        /// </summary>
        public int ActualPlayers;
    }

    /// <summary>
    /// Handlers of the IMultiplayerOperation.Finished event.
    /// </summary>
    /// <param name="succeeded">True if the operation succeeded, otherwise false.</param>
    /// <param name="errorMessage">An optional error message in case of failure.</param>
    delegate void MultiplayerOperationFinishedHdl(bool succeeded, string errorMessage);

    /// <summary>
    /// Handlers of the IMultiplayerGameManager.GameCountdown event.
    /// </summary>
    /// <param name="remainingTime">The remaining time to the beginning of the multiplayer game.</param>
    delegate void GameCountdownHdl(int remainingTime);

    /// <summary>
    /// The interface of an asynchronous multiplayer operation.
    /// </summary>
    interface IMultiplayerOperation
    {
        /// <summary>
        /// This event is raised on the UI-thread when the multiplayer operation has been finished.
        /// </summary>
        event MultiplayerOperationFinishedHdl Finished;
    }

    /// <summary>
    /// This is the interface of the component that is responsible for the whole lifecycle of a multiplayer game.
    /// The responsibilities of the component are:
    ///     - Creating a new multiplayer game.
    ///     - Getting informations about existing multiplayer games on the network.
    ///     - Joining to an existing multiplayer game.
    ///     - Setup multiplayer game.
    ///     - Send commands to a multiplayer game.
    /// This is an internal interface that can be accessed indirectly from the PresLogic via the appropriate
    /// backend components and views.
    /// </summary>
    /// TODO: finalize the interface!
    [ComponentInterface]
    interface IMultiplayerGameManager
    {
        /// <summary>
        /// Starts creating a new multiplayer game an announcing it on the network.
        /// </summary>
        /// <param name="mapFile">The file that contains the map of the new game.</param>
        /// <param name="gameType">The type of the game to be created.</param>
        /// <param name="gameSpeed">The speed of the game to be created.</param>
        /// <returns>A reference to the started operation.</returns>
        IMultiplayerOperation CreateNewGame(string mapFile, GameTypeEnum gameType, GameSpeedEnum gameSpeed);

        /// <summary>
        /// Starts joining to an existing multiplayer game.
        /// </summary>
        /// <param name="gameID">The ID of the game to join.</param>
        /// <returns>A reference to the started operation.</returns>
        IMultiplayerOperation JoinExistingGame(Guid gameID);

        /// <summary>
        /// Starts leaving the multiplayer game that this peer is currently connected to.
        /// </summary>
        /// <returns>A reference to the started operation.</returns>
        IMultiplayerOperation LeaveCurrentGame();

        /// <summary>
        /// Starts the simulation of the multiplayer game that this peer is currently connected to.
        /// This method can only be called by the peer that is the host of the game.
        /// </summary>
        /// <returns>A reference to the started operation.</returns>
        IMultiplayerOperation StartCurrentGame();

        /// <summary>
        /// Gets the list of the multiplayer games currently available on the network.
        /// </summary>
        /// <returns>The list of the currently available multiplayer games on the network.</returns>
        List<GameInfo> GetAvailableGames();

        /// <summary>
        /// Posts a command to the multiplayer game that this peer is currently connected to.
        /// </summary>
        /// <param name="cmd">The command to post.</param>
        void PostCommand(RCCommand cmd);

        /// <summary>
        /// Gets a reference to the scenario of the multiplayer game that this peer is currently connected to.
        /// </summary>
        Scenario GameScenario { get; }

        /// <summary>
        /// This event is raised several times on every peer when the start of the multiplayer game that this peer is currently connected
        /// to has been initiated by the host. The event indicates the remaining time until the start of the game.
        /// </summary>
        event GameCountdownHdl GameCountdown;
    }
}
