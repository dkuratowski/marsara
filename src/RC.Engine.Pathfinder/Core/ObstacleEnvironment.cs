using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Pathfinder.Core
{
    /// <summary>
    /// Provides informations about the environment of a rectangular obstacle.
    /// </summary>
    class ObstacleEnvironment
    {
        /// <summary>
        /// Constructs an ObstacleEnvironment instance.
        /// </summary>
        /// <param name="maxMovingSize">The maximum size of moving agents.</param>
        public ObstacleEnvironment(int maxMovingSize)
        {
            if (maxMovingSize <= 0) { throw new ArgumentOutOfRangeException("maxMovingSize", "The maximum size of moving agents shall be greater than 0!"); }

            this.maxMovingSize = maxMovingSize;
            this.cellDistances = new int[this.maxMovingSize, this.maxMovingSize];
            for (int row = 0; row < this.maxMovingSize; row++)
            {
                for (int col = 0; col < this.maxMovingSize; col++)
                {
                    this.cellDistances[col, row] = Math.Max(this.maxMovingSize - 1 - col, this.maxMovingSize - 1 - row);
                }
            }
        }

        /// <summary>
        /// Gets the cell-distance from the given cell to a rectangular obstacle relative to its top-left corner.
        /// </summary>
        /// <param name="relativeX">The X-coordinate of the given cell relative to the top-left corner of the obstacle.</param>
        /// <param name="relativeY">The Y-coordinate of the given cell relative to the top-left corner of the obstacle.</param>
        /// <returns>The cell-distance from the given cell to a rectangular obstacle relative to its top-left corner.</returns>
        public int this[int relativeX, int relativeY]
        {
            get
            {
                int col = relativeX + this.maxMovingSize - 1;
                int row = relativeY + this.maxMovingSize - 1;
                if (col < 0 || row < 0) { return this.maxMovingSize; }

                col = Math.Min(col, this.maxMovingSize - 1);
                row = Math.Min(row, this.maxMovingSize - 1);

                return this.cellDistances[col, row];
            }
        }

        /// <summary>
        /// Stores the cell-distances to the obstacle from the cells of the environment.
        /// </summary>
        private readonly int[,] cellDistances;

        /// <summary>
        /// The maximum size of moving agents.
        /// </summary>
        private readonly int maxMovingSize;
    }
}
