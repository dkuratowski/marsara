using RC.Common;
using RC.Engine.Pathfinder.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Engine.Pathfinder.Core
{
    /// <summary>
    /// Represents a walkability reader with constant walkability for all cells.
    /// </summary>
    class ConstantWalkabilityReader : IWalkabilityReader
    {
        /// <summary>
        /// Constructs a ConstantWalkabilityReader instance.
        /// </summary>
        /// <param name="walkability">The walkability to be returned for all cells.</param>
        /// <param name="width">The width of the walkability grid.</param>
        /// <param name="height">The height of the walkability grid.</param>
        public ConstantWalkabilityReader(bool walkability, int width, int height)
        {
            if (width < 1) { throw new ArgumentOutOfRangeException("width", "The width of a walkability grid shall be greater than 0!"); }
            if (height < 1) { throw new ArgumentOutOfRangeException("height", "The height of a walkability grid shall be greater than 0!"); }

            this.width = width;
            this.height = height;
            this.walkability = walkability;
        }

        #region IWalkabilityReader members

        /// <see cref="IWalkabilityReader.Item"/>
        public bool this[int x, int y] { get { return x >= 0 && x < this.width && y >= 0 && y < this.height ? this.walkability : false; } }

        /// <see cref="IWalkabilityReader.Width"/>
        public int Width { get { return this.width; } }

        /// <see cref="IWalkabilityReader.Height"/>
        public int Height { get { return this.height; } }

        #endregion IWalkabilityReader members

        /// <summary>
        /// The width of the walkability grid.
        /// </summary>
        private readonly int width;

        /// <summary>
        /// The height of the walkability grid.
        /// </summary>
        private readonly int height;

        /// <summary>
        /// The walkability to be returned for all cells.
        /// </summary>
        private readonly bool walkability;
    }
}
