using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Common.ComponentModel;
using RC.App.BizLogic.Services;

namespace RC.App.BizLogic.BusinessComponents
{
    /// <summary>
    /// The interface of the host of a multiplayer game.
    /// </summary>
    [ComponentInterface]
    public interface IMultiplayerHostBC
    {
        /// <summary>
        /// Begins hosting a new multiplayer game.
        /// </summary>
        /// <param name="hostName">The name of the local player who is hosting the new game.</param>
        /// <param name="mapBytes">The contents of the map file.</param>
        /// <param name="gameType">The type of the game to be created.</param>
        /// <param name="gameSpeed">The speed of the game to be created.</param>
        /// <exception cref="InvalidOperationException">If a multiplayer game is already in progress.</exception>
        void BeginHosting(string hostName, byte[] mapBytes, GameTypeEnum gameType, GameSpeedEnum gameSpeed);
    }
}
