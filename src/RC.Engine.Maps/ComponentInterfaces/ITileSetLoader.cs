using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.ComponentModel;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Maps.ComponentInterfaces
{
    /// <summary>
    /// Component interface for load tilesets.
    /// </summary>
    [ComponentInterface]
    public interface ITileSetLoader
    {
        /// <summary>
        /// Loads a tileset from the given byte array.
        /// </summary>
        /// <param name="data">The bytes of the serialized tileset.</param>
        /// <returns>The interface of the loaded tileset.</returns>
        ITileSet LoadTileSet(byte[] data);
    }
}
