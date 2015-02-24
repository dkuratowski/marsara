using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.Views;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Scenarios;

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
        /// Gets all the entities that are not entirely hidden by the Fog Of War inside the area of the attached window.
        /// </summary>
        /// <returns>All the entities that are not entirely hidden by the Fog Of War inside the area of the attached window.</returns>
        IEnumerable<Entity> GetEntitiesToUpdate();

        /// <summary>
        /// Gets all the entity snapshots that are not entirely hidden by the Fog Of War inside the given area.
        /// </summary>
        /// <param name="quadWindow">A rectangular area of the map given in quadratic tile coordinates.</param>
        /// <returns>All the entity snapshots that are not entirely hidden by the Fog Of War inside the given area.</returns>
        IEnumerable<EntitySnapshot> GetEntitySnapshotsInWindow(RCIntRectangle quadWindow);

        /// <summary>
        /// Gets all the entities that are not entirely hidden by the Fog Of War inside the given area.
        /// </summary>
        /// <param name="quadWindow">A rectangular area of the map given in quadratic tile coordinates.</param>
        /// <returns>All the entities that are not entirely hidden by the Fog Of War inside the given area.</returns>
        IEnumerable<Entity> GetEntitiesInWindow(RCIntRectangle quadWindow);

        /// <summary>
        /// Checks whether the given entity is not entirely hidden by the Fog Of War inside the area of the attached window.
        /// </summary>
        /// <param name="entity">The entity to check.</param>
        /// <returns>True if the given entity is not entirely hidden by the Fog Of War inside the area of the attached window; otherwise false.</returns>
        bool IsEntityVisible(Entity entity);

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
        /// Executes the next Fog Of War update iteration.
        /// </summary>
        void ExecuteUpdateIteration();
    }
}
