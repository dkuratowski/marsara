using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.ComponentModel;
using RC.Common;
using RC.Engine.PublicInterfaces;

namespace RC.Engine.ComponentInterfaces
{
    /// <summary>
    /// Component interface for load tilesets.
    /// </summary>
    [ComponentInterface]
    public interface ITileSetLoader
    {
        /// <summary>
        /// Loads a tileset from the given data package.
        /// </summary>
        /// <param name="data">The package that contains the serialized tileset.</param>
        /// <returns>The interface of the loaded tileset.</returns>
        ITileSet LoadTileSet(RCPackage data);
    }
}
