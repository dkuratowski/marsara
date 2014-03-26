using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Common.ComponentModel;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// The public interface of the pathfinder component.
    /// </summary>
    [ComponentInterface]
    public interface IPathFinder
    {
        /// <summary>
        /// Initializes the pathfinder component with the given navmesh.
        /// </summary>
        /// <param name="navmesh">The navmesh to initialize with.</param>
        /// <param name="maxIterationsPerFrame">The maximum number of search iterations per frame.</param>
        void Initialize(INavMesh navmesh, int maxIterationsPerFrame);

        /// <summary>
        /// Continues searching the requested paths.
        /// </summary>
        void ContinueSearching();

        /// <summary>
        /// Starts searching a path from one cell to another on the map.
        /// </summary>
        /// <param name="fromCoords">The starting coordinates of the path.</param>
        /// <param name="toCoords">The target coordinates of the path.</param>
        /// <param name="iterationLimit">The maximum number of iterations to execute when searching the path.</param>
        /// <return>The interface of the path.</return>
        IPath StartPathSearching(RCNumVector fromCoords, RCNumVector toCoords, int iterationLimit);

        /// <summary>
        /// Starts searching a detour to the target from the given section on the given original path.
        /// </summary>
        /// <param name="originalPath">The original path.</param>
        /// <param name="abortedSectionIdx">The index of the aborted section on the original path.</param>
        /// <param name="iterationLimit">The maximum number of iterations to execute when searching the detour.</param>
        /// <returns>The interface of the detour.</returns>
        IPath StartDetourSearching(IPath originalPath, int abortedSectionIdx, int iterationLimit);
    }
}
