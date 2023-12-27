using System;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.Commands;

namespace RC.App.BizLogic.Services
{
    /// <summary>
    /// Enumerates the possible game types.
    /// </summary>
    public enum GameTypeEnum
    {
        Melee = 0,
        OneOnOne = 1
    }

    /// <summary>
    /// Enumerates the possible game speeds in FPS.
    /// </summary>
    public enum GameSpeedEnum
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
    /// Interface of the multiplayer service that is responsible for the whole lifecycle of a multiplayer game.
    /// </summary>
    [ComponentInterface]
    public interface IMultiplayerService
    {
        /// <summary>
        /// Hosts a new multiplayer game and announces it on the network.
        /// </summary>
        /// <param name="hostName">The name of the local player who is hosting the new game.</param>
        /// <param name="mapFile">The file that contains the map of the new game.</param>
        /// <param name="gameType">The type of the game to be created.</param>
        /// <param name="gameSpeed">The speed of the game to be created.</param>
        void HostNewGame(string hostName, string mapFile, GameTypeEnum gameType, GameSpeedEnum gameSpeed);

        /// <summary>
        /// Starts the currently hosted multiplayer game.
        /// </summary>
        /// <exception cref="InvalidOperationException">If there is no hosted multiplayer game currently.</exception>
        void StartHostedGame();

        /// <summary>
        /// Joins to an existing multiplayer game created by the given host.
        /// </summary>
        /// <param name="hostName">The name of the remote player who is hosting the game.</param>
        /// <param name="guestName">The name of the local player who is joining to the game.</param>
        void JoinToExistingGame(string hostName, string guestName);

        /// <summary>
        /// Leaves the multiplayer game that this peer is currently connected to.
        /// </summary>
        void LeaveCurrentGame();

        /// <summary>
        /// Posts a command to the multiplayer game that this peer is currently connected to.
        /// </summary>
        /// <param name="cmd">The command to post.</param>
        void PostCommand(RCCommand cmd);

        /// <summary>
        /// This event is raised when the game that this peer is currently connected to has been updated.
        /// </summary>
        event Action GameUpdated;
    }
}
