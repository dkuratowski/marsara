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
        /// Continues searching the enqueued paths until the search iteration limit in the current frame is not reached.
        /// </summary>
        /// <remarks>
        /// The maximum number of executed search iterations in a frame can be controlled by calling this method in every
        /// frames exactly once.
        /// </remarks>
        void Flush();

        /// <summary>
        /// Starts searching a path from one cell to another on the map.
        /// </summary>
        /// <param name="fromCoords">The starting coordinates of the path.</param>
        /// <param name="toCoords">The target coordinates of the path.</param>
        /// <param name="iterationLimit">The maximum number of iterations to execute when searching the path.</param>
        /// <return>The interface of the path.</return>
        /// <remarks>
        /// Use the IPath.IsReadyForUse property of the returned path to check if the pathfinding has already been finished or not.
        /// </remarks>
        IPath StartPathSearching(RCNumVector fromCoords, RCNumVector toCoords, int iterationLimit);

        /// <summary>
        /// Starts searching a detour to the target from the given section on the given original path.
        /// </summary>
        /// <param name="originalPath">The original path.</param>
        /// <param name="abortedSectionIdx">The index of the aborted section on the original path.</param>
        /// <param name="iterationLimit">The maximum number of iterations to execute when searching the detour.</param>
        /// <returns>The interface of the detour.</returns>
        /// <remarks>
        /// Use the IPath.IsReadyForUse property of the returned path to check if the pathfinding has already been finished or not.
        /// </remarks>
        IPath StartDetourSearching(IPath originalPath, int abortedSectionIdx, int iterationLimit);

        /// <summary>
        /// Checks whether the given position is on the walkable area of the loaded navmesh.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <returns>True if the given position is on the walkable area of the loaded navmesh.</returns>
        bool IsWalkable(RCNumVector position);
    }
}
