using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Pathfinder.Core
{
    /// <summary>
    /// The interface of pathfinding algorithms.
    /// </summary>
    interface IPathfindingAlgorithm
    {
        /// <summary>
        /// Executes this pathfinding algorithm.
        /// </summary>
        void Execute();
    }
}
