using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.PublicInterfaces
{
    /// <summary>
    /// Interface for constraints of terrain objects.
    /// </summary>
    public interface ITerrainObjectConstraint
    {
        /// <summary>
        /// Checks whether this constraint allows attaching the given terrain object to the map at it's
        /// current position and collects all the violating quadratic coordinates.
        /// </summary>
        /// <param name="terrainObj">The terrain object to be checked.</param>
        /// <returns>
        /// The list of the quadratic coordinates (relative to the top-left corner) violating the constraint.
        /// </returns>
        HashSet<RCIntVector> Check(IMapAccess map, RCIntVector position);

        /// <summary>
        /// Gets the tileset that this constraint belongs to.
        /// </summary>
        ITileSet Tileset { get; }
    }
}
