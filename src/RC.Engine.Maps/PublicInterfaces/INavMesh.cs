using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.Maps.PublicInterfaces
{
    /// <summary>
    /// The interface of the navigation mesh of a map.
    /// </summary>
    public interface INavMesh
    {
        /// <summary>
        /// Gets the hash value of the walkability grid that this navmesh is based on.
        /// </summary>
        int WalkabilityHash { get; }

        /// <summary>
        /// Gets the size of the walkability grid that this navmesh is based on.
        /// </summary>
        RCIntVector GridSize { get; }

        /// <summary>
        /// Gets the list of the nodes of this navmesh.
        /// </summary>
        IEnumerable<INavMeshNode> Nodes { get; }
    }
}
