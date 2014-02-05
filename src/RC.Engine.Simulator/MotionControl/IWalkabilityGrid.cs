using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// The interface of the grid that contains the walkability information.
    /// </summary>
    interface IWalkabilityGrid
    {
        /// <summary>
        /// Gets the walkability information from the given cell of the grid.
        /// </summary>
        /// <param name="position">The position of the cell to read.</param>
        /// <returns>True if the given cell is walkable, false otherwise.</returns>
        /// <remarks>If the given coordinates are outside of the grid, then this indexer returns false.</remarks>
        bool this[RCIntVector position] { get; }

        /// <summary>
        /// Gets the width of the grid.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Gets the height of the grid.
        /// </summary>
        int Height { get; }
    }
}
