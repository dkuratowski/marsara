using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine
{
    /// <summary>
    /// Map editor interface for placing terrain objects onto the map.
    /// </summary>
    public interface ITerrainObjectEdit
    {
        /// <summary>
        /// Creates a terrain object.
        /// </summary>
        /// <param name="type">The type of the created terrain object.</param>
        /// <returns>Reference to the interface of the created terrain object.</returns>
        ITerrainObject CreateTerrainObject(TerrainObjectType type);

        /// <summary>
        /// Attaches the given terrain object to the map.
        /// </summary>
        /// <param name="terrainObject">The terrain object to be attached.</param>
        /// <param name="mapCoords">The coordinates of the top-left quadratic tile of the terrain object.</param>
        void AttachTerrainObject(ITerrainObject terrainObject, RCIntVector mapCoords);

        /// <summary>
        /// Detaches the given terrain object from the map.
        /// </summary>
        /// <param name="terrainObject">The terrain object to be detached.</param>
        void DetachTerrainObject(ITerrainObject terrainObject);

        /// <summary>
        /// Gets all the violating quadratic coordinates of the given terrain object if it were placed at the given position on the map.
        /// </summary>
        /// <param name="terrainObject">The terrain object to be checked.</param>
        /// <param name="mapCoords">The position to be checked.</param>
        /// <returns>The violating quadratic coordinates relative to the top-left corner of the terrain object.</returns>
        IEnumerable<RCIntVector> CheckConstraints(ITerrainObject terrainObject, RCIntVector mapCoords);
    }
}
