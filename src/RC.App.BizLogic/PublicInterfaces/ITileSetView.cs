using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.PublicInterfaces
{
    /// <summary>
    /// Interface of views on tilesets.
    /// </summary>
    public interface ITileSetView
    {
        /// <summary>
        /// Gets the display informations about each tiles defined by the given tileset.
        /// </summary>
        /// <param name="tilesetName">The name of the tileset.</param>
        /// <returns>The list of the tile display informations.</returns>
        List<IsoTileTypeInfo> GetIsoTileTypes();

        /// <summary>
        /// Gets the list of the terrain types defined in the tileset.
        /// </summary>
        /// <returns>The list of the terrain types defined in the tileset.</returns>
        List<string> GetTerrainTypes();
    }
}
