using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Engine.Pathfinder.PublicInterfaces
{
    /// <summary>
    /// The interface of objects providing walkability informations about a grid.
    /// </summary>
    public interface IWalkabilityReader
    {
        /// <summary>
        /// Gets the walkability information from the given cell of the grid.
        /// </summary>
        /// <param name="x">The X-coordinate of the cell.</param>
        /// <param name="y">The Y-coordinate of the cell.</param>
        /// <returns>True if the given cell is walkable, false otherwise.</returns>
        /// <remarks>If the given coordinates are outside of the grid, then this indexer returns false.</remarks>
        bool this[int x, int y] { get; }

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
