using RC.Common.ComponentModel;
using RC.Common;
using System;

namespace RC.App.BizLogic.Services
{
    /// <summary>
    /// Interface of the map editor backend service.
    /// </summary>
    [ComponentInterface]
    public interface IMapEditorService
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
        /// This event is raised when the animations of the currently opened map has been updated.
        /// </summary>
        event Action AnimationsUpdated;

        /// <summary>
        /// Draws the given terrain type on the isometric tile at the given position.
        /// </summary>
        /// <param name="position">The position inside the displayed area in pixels.</param>
        /// <param name="terrainType">The name of the terrain type to draw.</param>
        void DrawTerrain(RCIntVector position, string terrainType);

        /// <summary>
        /// Places a terrain object on the map at the given position.
        /// </summary>
        /// <param name="position">The position inside the displayed area in pixels.</param>
        /// <param name="terrainObject">The name of the terrain object to place.</param>
        /// <returns>True if the terrain object could be placed to the given position, false otherwise.</returns>
        bool PlaceTerrainObject(RCIntVector position, string terrainObject);

        /// <summary>
        /// Removes a terrain object from the map at the given position.
        /// </summary>
        /// <param name="position">The position inside the displayed area in pixels.</param>
        /// <returns>True if the terrain object at the given position was removed, false otherwise.</returns>
        bool RemoveTerrainObject(RCIntVector position);

        /// <summary>
        /// Places a start location on the map at the given position.
        /// </summary>
        /// <param name="position">The position inside the displayed area in pixels.</param>
        /// <param name="playerIndex">The index of the player that the start location belongs to.</param>
        /// <returns>True if the start location could be placed to the given position, false otherwise.</returns>
        bool PlaceStartLocation(RCIntVector position, int playerIndex);

        /// <summary>
        /// Places a mineral field on the map at the given position.
        /// </summary>
        /// <param name="position">The position inside the displayed area in pixels.</param>
        /// <returns>True if the mineral field could be placed to the given position, false otherwise.</returns>
        bool PlaceMineralField(RCIntVector position);

        /// <summary>
        /// Places a vespene geyser on the map at the given position.
        /// </summary>
        /// <param name="position">The position inside the displayed area in pixels.</param>
        /// <returns>True if the vespene geyser could be placed to the given position, false otherwise.</returns>
        bool PlaceVespeneGeyser(RCIntVector position);

        /// <summary>
        /// Removes an entity from the map at the given position.
        /// </summary>
        /// <param name="position">The position inside the displayed area in pixels.</param>
        /// <returns>True if the entity at the given position was removed, false otherwise.</returns>
        bool RemoveEntity(RCIntVector position);

        /// <summary>
        /// Changes the amount of resource in the given object if it is a resource object.
        /// </summary>
        /// <param name="objectID">The ID of the target object.</param>
        /// <param name="delta">The difference between the current and the new resource amount.</param>
        /// <returns>True if the resource amount has been changed, false otherwise.</returns>
        bool ChangeResourceAmount(int objectID, int delta);
    }
}
