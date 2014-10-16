using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.Scenarios;

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
        /// Starts creating a new multiplayer game an announcing it on the network.
        /// </summary>
        /// <param name="mapFile">The file that contains the map of the new game.</param>
        /// <param name="gameType">The type of the game to be created.</param>
        /// <param name="gameSpeed">The speed of the game to be created.</param>
        /// <returns>A reference to the started operation.</returns>
        void CreateNewGame(string mapFile, GameTypeEnum gameType, GameSpeedEnum gameSpeed);

        /// <summary>
        /// Starts leaving the multiplayer game that this peer is currently connected to.
        /// </summary>
        /// <returns>A reference to the started operation.</returns>
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
