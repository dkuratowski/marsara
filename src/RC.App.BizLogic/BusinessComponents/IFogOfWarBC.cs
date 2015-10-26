using System.Collections.Generic;
using RC.App.BizLogic.Views;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.Metadata;

namespace RC.App.BizLogic.BusinessComponents
{
    /// <summary>
    /// Interface of the business component used for retrieving Fog Of War informations from the currently active scenario.
    /// </summary>
    [ComponentInterface]
    interface IFogOfWarBC
    {
        /// <summary>
        /// Starts calculating the Fog Of War for the given player.
        /// </summary>
        /// <param name="owner">The owner player of the Fog Of War to start calculating.</param>
        void StartFogOfWar(PlayerEnum owner);

        /// <summary>
        /// Stops calculating the Fog Of War for the given player.
        /// </summary>
        /// <param name="owner">The owner player of the Fog Of War to stop calculating.</param>
        void StopFogOfWar(PlayerEnum owner);

        /// <summary>
        /// Gets all the isometric tiles that are not entirely hidden by the Fog Of War inside the area of the attached window.
        /// </summary>
        /// <returns>All the isometric tiles that are not entirely hidden by the Fog Of War inside the area of the attached window.</returns>
        IEnumerable<IIsoTile> GetIsoTilesToUpdate();

        /// <summary>
        /// Gets all the terrain objects that are not entirely hidden by the Fog Of War inside the area of the attached window.
        /// </summary>
        /// <returns>All the terrain objects that are not entirely hidden by the Fog Of War inside the area of the attached window.</returns>
        IEnumerable<ITerrainObject> GetTerrainObjectsToUpdate();

        /// <summary>
        /// Gets all the quadratic tiles on which the Fog Of War shall be updated inside the area of the attached window.
        /// </summary>
        /// <returns>All the quadratic tiles on which the Fog Of War shall be updated inside the area of the attached window.</returns>
        IEnumerable<IQuadTile> GetQuadTilesToUpdate();

        /// <summary>
        /// Gets all the entity snapshots that are not entirely hidden by the Fog Of War inside the area of the attached window.
        /// </summary>
        /// <returns>All the entity snapshots that are not entirely hidden by the Fog Of War inside the area of the attached window.</returns>
        IEnumerable<EntitySnapshot> GetEntitySnapshotsToUpdate();

        /// <summary>
        /// Gets all the map objects on the ground that are not entirely hidden by the Fog Of War inside the area of the attached window.
        /// </summary>
        /// <returns>All the map objects on the ground that are not entirely hidden by the Fog Of War inside the area of the attached window.</returns>
        IEnumerable<MapObject> GetGroundMapObjectsToUpdate();

        /// <summary>
        /// Gets all the map objects in the air that are not entirely hidden by the Fog Of War inside the area of the attached window.
        /// </summary>
        /// <returns>All the map objects in the air that are not entirely hidden by the Fog Of War inside the area of the attached window.</returns>
        IEnumerable<MapObject> GetAirMapObjectsToUpdate();

        /// <summary>
        /// Gets all the map objects that are not entirely hidden by the Fog Of War inside the area of the attached window.
        /// </summary>
        /// <returns>All the map objects that are not entirely hidden by the Fog Of War inside the area of the attached window.</returns>
        IEnumerable<MapObject> GetAllMapObjectsToUpdate();
        
        /// <summary>
        /// Gets all the entity snapshots that are not entirely hidden by the Fog Of War inside the given area.
        /// </summary>
        /// <param name="quadWindow">A rectangular area of the map given in quadratic tile coordinates.</param>
        /// <returns>All the entity snapshots that are not entirely hidden by the Fog Of War inside the given area.</returns>
        IEnumerable<EntitySnapshot> GetEntitySnapshotsInWindow(RCIntRectangle quadWindow);

        /// <summary>
        /// Gets all the map objects that are not entirely hidden by the Fog Of War inside the given area.
        /// </summary>
        /// <param name="quadWindow">A rectangular area of the map given in quadratic tile coordinates.</param>
        /// <returns>All the map objects that are not entirely hidden by the Fog Of War inside the given area.</returns>
        IEnumerable<MapObject> GetAllMapObjectsInWindow(RCIntRectangle quadWindow);

        /// <summary>
        /// Checks whether the given map object is not entirely hidden by the Fog Of War inside the area of the attached window.
        /// </summary>
        /// <param name="mapObject">The map object to check.</param>
        /// <returns>True if the given map object is not entirely hidden by the Fog Of War inside the area of the attached window; otherwise false.</returns>
        bool IsMapObjectVisible(MapObject mapObject);

        /// <summary>
        /// Gets the full FOW-flags at the given quadratic tile.
        /// </summary>
        /// <param name="quadCoords">The quadratic coordinates of the tile.</param>
        /// <returns>The full FOW-flags at the given quadratic tile.</returns>
        FOWTileFlagsEnum GetFullFowTileFlags(RCIntVector quadCoords);

        /// <summary>
        /// Gets the partial FOW-flags at the given quadratic tile.
        /// </summary>
        /// <param name="quadCoords">The quadratic coordinates of the tile.</param>
        /// <returns>The partial FOW-flags at the given quadratic tile.</returns>
        FOWTileFlagsEnum GetPartialFowTileFlags(RCIntVector quadCoords);

        /// <summary>
        /// Gets the current FOW-state at the given quadratic tile.
        /// </summary>
        /// <param name="quadCoords">The quadratic coordinates of the tile.</param>
        /// <returns>The current FOW-state at the given quadratic tile.</returns>
        FOWTypeEnum GetFowState(RCIntVector quadCoords);

        /// <summary>
        /// Checks whether the constraints of the given entity type allows placing an entity of this type to the active scenario at the given
        /// quadratic position and collects all the violating quadratic coordinates relative to the given position.
        /// </summary>
        /// <param name="elementType">The type to be checked.</param>
        /// <param name="position">The position to be checked.</param>
        /// <returns>
        /// The list of the quadratic coordinates (relative to the given position) violating the placement constraints of the given entity type.
        /// </returns>
        RCSet<RCIntVector> CheckPlacementConstraints(IScenarioElementType elementType, RCIntVector position);

        /// <summary>
        /// Checks whether the placement constraints of the given entity allows it to be placed at the given quadratic position and
        /// collects all the violating quadratic coordinates relative to the given position.
        /// </summary>
        /// <param name="entity">The entity to be checked.</param>
        /// <param name="position">The position to be checked.</param>
        /// <returns>
        /// The list of the quadratic coordinates (relative to the given position) violating the constraints of the given entity.
        /// </returns>
        RCSet<RCIntVector> CheckPlacementConstraints(Entity entity, RCIntVector position);
        
        /// <summary>
        /// Executes the next Fog Of War update iteration.
        /// </summary>
        void ExecuteUpdateIteration();
    }
}
