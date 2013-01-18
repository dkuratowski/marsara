using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.ComponentModel;
using RC.Common;

namespace RC.App.BizLogic
{
    /// <summary>
    /// This component interface is used to access the functionalities of the map editor.
    /// </summary>
    [ComponentInterface]
    public interface IMapEditor
    {
        /// <summary>
        /// Creates a new map.
        /// </summary>
        /// <param name="tilesetName">The name of the tileset that the new map is based on.</param>
        /// <param name="defaultTerrain">The default terrain of the new map.</param>
        /// <param name="mapSize">
        /// The size of the new map in quadratic tiles. The first coordinate of the vector is the width, the
        /// second coordinate of the vector is the height of the new map. The constraints are the followings:
        /// the width of the map must be a multiple of 4, the height of the map must be a multiple of 2.
        /// </param>
        MapEditorErrorCode CreateMap(string tilesetName, string defaultTerrain, RCIntVector mapSize);

        /// <summary>
        /// Loads a map from the given file.
        /// </summary>
        /// <param name="filename">The name of the file to load from.</param>
        MapEditorErrorCode LoadMap(string filename);

        /// <summary>
        /// Saves the map to the given file.
        /// </summary>
        /// <param name="filename">The name of the file to save.</param>
        MapEditorErrorCode SaveMap(string filename);

        /// <summary>
        /// Draws the given terrain type on the isometric tile at the given position.
        /// </summary>
        /// <param name="position">The position inside the map display window in navigation cells.</param>
        /// <param name="terrainName">The name of the terrain to draw.</param>
        MapEditorErrorCode DrawTerrain(RCIntVector position, string terrainName);

        /// <summary>
        /// Gets the name of the tileset of the currently loaded map.
        /// </summary>
        string TilesetName { get; }
    }

    /// <summary>
    /// Enumerates the possible error codes of the IMapEditor component interface.
    /// </summary>
    public enum MapEditorErrorCode
    {
        Success = 0,            /// No error, operation succeeded.
        FileNotFound = 1,       /// The given map file not found.
        TileSetNotFound = 2,    /// The tileset defined in the given map file not found.
        FileFormatError = 3,    /// The given map file has a wrong file format.
        CannotOpenFile = 4,     /// Cannot open the given map file.
        CannotWriteFile = 5,    /// Cannot write the given file.

        UnknownError = -1,      /// Unknown error.
    }
}
