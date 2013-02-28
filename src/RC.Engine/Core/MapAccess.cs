using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.PublicInterfaces;
using RC.Common;

namespace RC.Engine.Core
{
    /// <summary>
    /// Represents the main access point of a map.
    /// </summary>
    class MapAccess : IMapAccess
    {
        /// <summary>
        /// Constructs a MapAccess instance.
        /// </summary>
        /// <param name="mapStructure">Reference to the used map structure.</param>
        public MapAccess(MapStructure mapStructure)
        {
            if (mapStructure == null) { throw new ArgumentNullException("mapStructure"); }
            if (mapStructure.Status != MapStructure.MapStatus.Closed) { throw new InvalidOperationException("A map is already opened with this MapStructure!"); }

            this.mapStructure = mapStructure;
        }

        #region IMapAccess methods

        /// <see cref="IMapAccess.Size"/>
        public RCIntVector Size
        {
            get { return this.mapStructure.Size; }
        }

        /// <see cref="IMapAccess.Size"/>
        public RCIntVector CellSize
        {
            get { return this.mapStructure.CellSize; }
        }

        /// <see cref="IMapAccess.Size"/>
        public ITileSet Tileset
        {
            get { return this.mapStructure.Tileset; }
        }

        /// <see cref="IMapAccess.Size"/>
        public IQuadTile GetQuadTile(RCIntVector coords)
        {
            return this.mapStructure.GetQuadTile(coords);
        }

        /// <see cref="IMapAccess.Size"/>
        public IIsoTile GetIsoTile(RCIntVector coords)
        {
            return this.mapStructure.GetIsoTile(coords);
        }

        /// <see cref="IMapAccess.Size"/>
        public ICell GetCell(RCIntVector index)
        {
            return this.mapStructure.GetCell(index);
        }

        /// <see cref="IMapAccess.BeginExchangingTiles"/>
        public void BeginExchangingTiles()
        {
            this.mapStructure.BeginExchangingTiles();
        }

        /// <see cref="IMapAccess.EndExchangingTiles"/>
        public IEnumerable<IIsoTile> EndExchangingTiles()
        {
            return this.mapStructure.EndExchangingTiles();
        }

        /// <see cref="IMapAccess.Close"/>
        public void Close()
        {
            this.mapStructure.Close();
        }

        /// TODO: only for debugging!
        public IEnumerable<IIsoTile> IsometricTiles { get { return this.mapStructure.IsometricTiles; } }

        #endregion IMapAccess methods

        /// <summary>
        /// Reference to the used map structure.
        /// </summary>
        private MapStructure mapStructure;
    }
}
