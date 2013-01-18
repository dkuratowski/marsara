using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.ComponentModel;

namespace RC.App.BizLogic
{
    /// <summary>
    /// This component interface is used to access informations about the available tilesets.
    /// </summary>
    [ComponentInterface]
    public interface ITileSetStore
    {
        /// <summary>
        /// Gets the list of the names of the available tilesets.
        /// </summary>
        IEnumerable<string> TileSets { get; }

        /// <summary>
        /// Gets the terrain types defined by the given tileset.
        /// </summary>
        /// <param name="tilesetName">The name of the tileset.</param>
        /// <returns>The list of the names of the terrain types.</returns>
        IEnumerable<string> GetTerrainTypes(string tilesetName);

        /// <summary>
        /// Gets the display informations about each tiles defined by the given tileset.
        /// </summary>
        /// <param name="tilesetName">The name of the tileset.</param>
        /// <returns>The list of the tile display informations.</returns>
        IEnumerable<TileTypeInfo> GetTileTypes(string tilesetName);
    }

    /// <summary>
    /// This structure is used to provide informations that are necessary to display a tile type.
    /// </summary>
    public struct TileTypeInfo
    {
        /// <summary>
        /// The byte-stream that contains the image data of this tile type.
        /// </summary>
        public byte[] ImageData { get; set; }

        /// <summary>
        /// List of the properties of this tile type mapped by their name.
        /// </summary>
        public Dictionary<string, string> Properties { get; set; }
    }
}
