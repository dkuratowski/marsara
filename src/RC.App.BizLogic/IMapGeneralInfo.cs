using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.ComponentModel;
using RC.Common;

namespace RC.App.BizLogic
{
    /// <summary>
    /// This component interface provides general informations about the currently opened map.
    /// </summary>
    [ComponentInterface]
    public interface IMapGeneralInfo
    {
        /// <summary>
        /// Gets whether a map is currently opened in the system or not.
        /// </summary>
        bool IsMapOpened { get; }

        /// <summary>
        /// Gets the size of the currently opened map in quadratic tiles.
        /// </summary>
        RCIntVector Size { get; }

        /// <summary>
        /// Gets the size of the currently opened map in cells.
        /// </summary>
        RCIntVector NavSize { get; }

        /// <summary>
        /// Gets the name of the tileset of the currently opened map.
        /// </summary>
        string TilesetName { get; }
    }
}
