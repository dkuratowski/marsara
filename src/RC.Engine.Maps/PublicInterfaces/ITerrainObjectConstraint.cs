using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.Maps.PublicInterfaces
{
    /// <summary>
    /// Interface for constraints of terrain objects.
    /// </summary>
    public interface ITerrainObjectConstraint
    {
        /// <summary>
        /// Checks whether this constraint allows attaching the corresponding terrain object to the given map at the given
        /// quadratic position and collects all the violating quadratic coordinates relative to the top-left corner of the
		/// terrain object.
        /// </summary>
        /// <param name="map">Reference to the map.</param>
        /// <param name="position">The position to check.</param>
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
