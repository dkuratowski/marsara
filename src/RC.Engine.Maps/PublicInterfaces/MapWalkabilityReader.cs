using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Maps.PublicInterfaces
{
    /// <summary>
    /// Helper class for reading walkability informations from an RC map.
    /// </summary>
    /// TODO: do we need this class in the RC.Engine.Maps module!
    public class MapWalkabilityReader : IWalkabilityGrid
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

        #region IWalkabilityGrid methods

        /// <see cref="IWalkabilityGrid.this[]"/>
        public bool this[RCIntVector position]
        {
            get
            {
                return position.X >= 0 && position.X < this.Width && position.Y >= 0 && position.Y < this.Height ?
                       this.map.GetCell(position).IsWalkable :
                       false;
            }
        }

        /// <see cref="IWalkabilityGrid.Width"/>
        public int Width { get { return this.map.CellSize.X; } }

        /// <see cref="IWalkabilityGrid.Height"/>
        public int Height { get { return this.map.CellSize.Y; } }

        #endregion IWalkabilityGrid methods

        /// <summary>
        /// Reference to the map to be read.
        /// </summary>
        private IMapAccess map;
    }
}
