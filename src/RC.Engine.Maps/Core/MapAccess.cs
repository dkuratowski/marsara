using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Maps.PublicInterfaces;
using RC.Common;
using RC.Common.Configuration;

namespace RC.Engine.Maps.Core
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
        public MapAccess(string mapName, MapStructure mapStructure)
        {
            if (mapName == null) { throw new ArgumentNullException("mapName"); }
            if (mapStructure == null) { throw new ArgumentNullException("mapStructure"); }
            if (mapStructure.Status != MapStructure.MapStatus.Closed) { throw new InvalidOperationException("A map is already opened with this MapStructure!"); }

            this.mapName = mapName;
            this.mapStructure = mapStructure;
            this.terrainObjects = null; // Will be created later, when the map structure is opened
        }

        #region IMapAccess methods

        /// <see cref="IMapAccess.MapName"/>
        public string MapName
        {
            get { return this.mapName; }
        }

        /// <see cref="IMapAccess.Size"/>
        public RCIntVector Size
        {
            get { return this.mapStructure.Size; }
        }

        /// <see cref="IMapAccess.CellSize"/>
        public RCIntVector CellSize
        {
            get { return this.mapStructure.CellSize; }
        }

        /// <see cref="IMapAccess.Tileset"/>
        public ITileSet Tileset
        {
            get { return this.mapStructure.Tileset; }
        }

        /// <see cref="IMapAccess.GetQuadTile"/>
        public IQuadTile GetQuadTile(RCIntVector coords)
        {
            return this.mapStructure.GetQuadTile(coords);
        }

        /// <see cref="IMapAccess.GetIsoTile"/>
        public IIsoTile GetIsoTile(RCIntVector coords)
        {
            return this.mapStructure.GetIsoTile(coords);
        }

        /// <see cref="IMapAccess.GetCell"/>
        public ICell GetCell(RCIntVector index)
        {
            return this.mapStructure.GetCell(index);
        }

        /// <see cref="IMapAccess.QuadToCellRect"/>
        public RCIntRectangle QuadToCellRect(RCIntRectangle quadRect)
        {
            return this.mapStructure.QuadToCellRect(quadRect);
        }

        /// <see cref="IMapAccess.CellToQuadSize"/>
        public RCIntVector CellToQuadSize(RCNumVector cellSize)
        {
            return this.mapStructure.CellToQuadSize(cellSize);
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

        /// <see cref="IMapAccess.TerrainObjects"/>
        public ISearchTree<ITerrainObject> TerrainObjects
        {
            get
            {
                if (this.terrainObjects == null)
                {
                    this.terrainObjects = new BspSearchTree<ITerrainObject>(
                        new RCNumRectangle(-(RCNumber)1 / (RCNumber)2,
                                           -(RCNumber)1 / (RCNumber)2,
                                           this.CellSize.X,
                                           this.CellSize.Y),
                                           ConstantsTable.Get<int>("RC.Engine.Maps.BspNodeCapacity"),
                                           ConstantsTable.Get<int>("RC.Engine.Maps.BspMinNodeSize"));
                }
                return this.terrainObjects;
            }
        }

        #endregion IMapAccess methods

        /// <summary>
        /// The name of the map.
        /// </summary>
        private string mapName;

        /// <summary>
        /// Reference to the used map structure.
        /// </summary>
        private MapStructure mapStructure;

        /// <summary>
        /// The map content manager that contains the terrain objects of this map.
        /// </summary>
        private ISearchTree<ITerrainObject> terrainObjects;
    }
}
