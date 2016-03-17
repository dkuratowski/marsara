using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Pathfinder.PublicInterfaces;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// Helper class for reading walkability informations from an RC map.
    /// </summary>
    class MapWalkabilityReader : IWalkabilityReader
    {
        /// <summary>
        /// Constructs a walkability reader instance for the given map.
        /// </summary>
        /// <param name="map">The map to read.</param>
        public MapWalkabilityReader(IMapAccess map)
        {
            if (map == null) { throw new ArgumentNullException("map"); }
            this.map = map;
        }

        #region IWalkabilityReader methods

        /// <see cref="IWalkabilityReader.this[]"/>
        public bool this[int x, int y]
        {
            get
            {
                return x >= 0 && x < this.Width && y >= 0 && y < this.Height ?
                    this.map.GetCell(new RCIntVector(x, y)).IsWalkable :
                    false;
            }
        }

        /// <see cref="IWalkabilityReader.Width"/>
        public int Width { get { return this.map.CellSize.X; } }

        /// <see cref="IWalkabilityReader.Height"/>
        public int Height { get { return this.map.CellSize.Y; } }

        #endregion IWalkabilityReader methods

        /// <summary>
        /// Reference to the map to be read.
        /// </summary>
        private IMapAccess map;
    }
}
