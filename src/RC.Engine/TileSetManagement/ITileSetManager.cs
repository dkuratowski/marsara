using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.ComponentModel;

namespace RC.Engine
{
    /// <summary>
    /// Defines the interface of the tileset manager component.
    /// </summary>
    [ComponentInterface]
    public interface ITileSetManager
    {
        /// <summary>
        /// Loads a tileset from the given file.
        /// </summary>
        /// <param name="filename">The file to load from.</param>
        /// <returns>The name of the loaded tileset.</returns>
        string LoadTileSet(string filename);

        /// <summary>
        /// Gets the tileset with the given name.
        /// </summary>
        /// <param name="name">The name of the tileset.</param>
        /// <returns>The tileset with the given name.</returns>
        TileSet GetTileSet(string name);
    }
}
