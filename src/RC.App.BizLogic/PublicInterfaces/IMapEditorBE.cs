using RC.Common.ComponentModel;
using RC.Common;

namespace RC.App.BizLogic.PublicInterfaces
{
    /// <summary>
    /// Interface to the backend component of the map editor. The UI of the map editor communicates directly with this interface.
    /// </summary>
    [ComponentInterface]
    public interface IMapEditorBE
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
        void NewMap(string tilesetName, string defaultTerrain, RCIntVector mapSize);

        /// <summary>
        /// Loads a map from the given file.
        /// </summary>
        /// <param name="filename">The name of the file to load from.</param>
        void LoadMap(string filename);

        /// <summary>
        /// Saves the currently opened map to the given file.
        /// </summary>
        /// <param name="filename">The name of the file to save.</param>
        void SaveMap(string filename);

        /// <summary>
        /// Closes the currently opened map. If there is no opened map, then this function has no effect.
        /// </summary>
        void CloseMap();

        /// <summary>
        /// Creates a view on the terrain of the currently opened map.
        /// </summary>
        /// <returns>The view on the terrain of the currently opened map.</returns>
        /// <exception cref="InvalidOperationException">If there is no opened map.</exception>
        IMapTerrainView CreateMapTerrainView();

        /// <summary>
        /// Creates a view on the tileset of the currently opened map.
        /// </summary>
        /// <returns>The view on the tileset of the currently opened map.</returns>
        /// <exception cref="InvalidOperationException">If there is no opened map.</exception>
        ITileSetView CreateTileSetView();

        /// <summary>
        /// Draws the given terrain type on the isometric tile at the given position.
        /// </summary>
        /// <param name="displayedArea">The displayed area in pixels.</param>
        /// <param name="position">The position inside the displayed are in pixels.</param>
        /// <param name="terrainName">The name of the terrain type to draw.</param>
        void DrawTerrain(RCIntRectangle displayedArea, RCIntVector position, string terrainType);
    }
}
