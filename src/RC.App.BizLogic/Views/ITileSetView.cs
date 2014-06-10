using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.Views
{
    /// <summary>
    /// Interface of views on tilesets.
    /// </summary>
    public interface ITileSetView
    {
        /// <summary>
        /// Gets the map sprite types for each isometric tile types defined by the tileset of this view.
        /// </summary>
        /// <returns>The list of the map sprite types for isometric tile types.</returns>
        List<SpriteDef> GetIsoTileTypes();

        /// <summary>
        /// Gets the map sprite types for each terrain object types defined by the tileset of this view.
        /// </summary>
        /// <returns>The list of the map sprite types for terrain object types.</returns>
        List<SpriteDef> GetTerrainObjectTypes();

        /// <summary>
        /// Gets the list of the names of the terrain object types defined by the tileset of this view.
        /// </summary>
        /// <returns>The list that contains the name of the terrain object types.</returns>
        List<string> GetTerrainObjectTypeNames();

        /// <summary>
        /// Gets the list of the names of the terrain types defined by the tileset of this view.
        /// </summary>
        /// <returns>The list that contains the name of the terrain types.</returns>
        List<string> GetTerrainTypeNames();
    }
}
