using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.Maps.PublicInterfaces
{
    /// <summary>
    /// Interface for retrieving informations about and edge from a navmesh node to another navmesh node. These informations can be
    /// used for calculating the preferred velocities during the motion control of entities.
    /// </summary>
    public interface INavMeshEdge
    {
        /// <summary>
        /// Gets the vector that is perpendicular to this edge and points out of the initial navmesh node of the edge. The length of
        /// this vector equals with the length of the edge.
        /// </summary>
        RCNumVector TransitionVector { get; }

        /// <summary>
        /// Gets the coordinates of the midpoint of this edge.
        /// </summary>
        RCNumVector Midpoint { get; }
    }
}
