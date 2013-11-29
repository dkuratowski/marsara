using RC.Common.ComponentModel;
using RC.Common;
using RC.App.BizLogic.PublicInterfaces;

namespace RC.App.BizLogic.ComponentInterfaces
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
        /// <param name="mapName">The name of the new map.</param>
        /// <param name="tilesetName">The name of the tileset that the new map is based on.</param>
        /// <param name="defaultTerrain">The default terrain of the new map.</param>
        /// <param name="mapSize">
        /// The size of the new map in quadratic tiles. The first coordinate of the vector is the width, the
        /// second coordinate of the vector is the height of the new map. The constraints are the followings:
        /// the width of the map must be a multiple of 4, the height of the map must be a multiple of 2.
        /// </param>
        void NewMap(string mapName, string tilesetName, string defaultTerrain, RCIntVector mapSize);

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
        /// Creates a view on the game engine metadata.
        /// </summary>
        /// <returns>The view on the game engine metadata.</returns>
        IMetadataView CreateMetadataView();

        /// <summary>
        /// Creates a view on the currently opened scenario.
        /// </summary>
        /// <returns>The view on the currently opened scenario.</returns>
        /// <exception cref="InvalidOperationException">If there is no opened scenario.</exception>
        IMapObjectView CreateMapObjectView();

        /// <summary>
        /// Creates a terrain object placement view on the currently opened map.
        /// </summary>
        /// <param name="terrainObjectName">The name of the terrain object to be placed.</param>
        /// <returns>The terrain object placement view on the currently opened map.</returns>
        /// <exception cref="InvalidOperationException">If there is no opened map.</exception>
        IObjectPlacementView CreateTerrainObjectPlacementView(string terrainObjectName);

        /// <summary>
        /// Creates a map object placement view on the currently opened map.
        /// </summary>
        /// <param name="objectTypeName">The name of the object type to be placed.</param>
        /// <returns>The map object placement view on the currently opened map.</returns>
        /// <exception cref="InvalidOperationException">If there is no opened map.</exception>
        IObjectPlacementView CreateMapObjectPlacementView(string objectTypeName);

        /// <summary>
        /// Draws the given terrain type on the isometric tile at the given position.
        /// </summary>
        /// <param name="displayedArea">The displayed area in pixels.</param>
        /// <param name="position">The position inside the displayed area in pixels.</param>
        /// <param name="terrainName">The name of the terrain type to draw.</param>
        void DrawTerrain(RCIntRectangle displayedArea, RCIntVector position, string terrainType);

        /// <summary>
        /// Places a terrain object on the map at the given position.
        /// </summary>
        /// <param name="displayedArea">The displayed area in pixels.</param>
        /// <param name="position">The position inside the displayed area in pixels.</param>
        /// <param name="terrainObject">The name of the terrain object to place.</param>
        /// <returns>True if the terrain object could be placed to the given position, false otherwise.</returns>
        bool PlaceTerrainObject(RCIntRectangle displayedArea, RCIntVector position, string terrainObject);

        /// <summary>
        /// Removes a terrain object from the map at the given position.
        /// </summary>
        /// <param name="displayedArea">The displayed area in pixels.</param>
        /// <param name="position">The position inside the displayed area in pixels.</param>
        /// <returns>True if the terrain object at the given position was removed, false otherwise.</returns>
        bool RemoveTerrainObject(RCIntRectangle displayedArea, RCIntVector position);
    }
}
