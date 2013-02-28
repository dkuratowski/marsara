using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.PublicInterfaces;

namespace RC.Engine.Core
{
    /// <summary>
    /// Enumerates the possible terrain object states.
    /// </summary>
    enum TerrainObjectStatus
    {
        Detached = 0,   /// The terrain object is currently detached from the map.
        Attached = 1,   /// The terrain object is currently attached to the map.
        Disposed = 2    /// The terrain object has been disposed.
    }

    /// <summary>
    /// Represents a terrain object on the map.
    /// </summary>
    class TerrainObject : ITerrainObject
    {
        /// <summary>
        /// Constructs a terrain object.
        /// </summary>
        /// <param name="map">The map that this terrain object belongs to.</param>
        /// <param name="type">The type of this terrain object.</param>
        public TerrainObject(MapStructure map, TerrainObjectType type)
        {
            if (map == null) { throw new ArgumentNullException("map"); }
            if (type == null) { throw new ArgumentNullException("type"); }
            if (type.Tileset != map.Tileset) { throw new MapException("Tileset of the TerrainObjectType and tileset of the map are not the same!"); }

            this.mapCoords = new RCIntVector(0, 0);
            this.type = type;
            this.parentMap = map;
            this.status = TerrainObjectStatus.Detached;
            this.violatingQuadCoordsCache = null;
            this.violatingQuadCoordsCacheIsDirty = true;

            this.cells = new Cell[this.type.QuadraticSize.X * MapStructure.NAVCELL_PER_QUAD,
                                  this.type.QuadraticSize.Y * MapStructure.NAVCELL_PER_QUAD];
            this.quadTiles = new QuadTile[this.type.QuadraticSize.X, this.type.QuadraticSize.Y];
        }

        #region IMapContent methods

        /// <see cref="IMapContent.Position"/>
        public RCNumRectangle Position { get { throw new NotImplementedException(); } }

        /// <see cref="IMapContent.PositionChanging"/>
        public event MapContentPropertyChangeHdl PositionChanging;

        /// <see cref="IMapContent.PositionChanged"/>
        public event MapContentPropertyChangeHdl PositionChanged;

        #endregion IMapContent methods

        #region ITerrainObject methods

        /// <see cref="ITerrainObject.Type"/>
        public ITerrainObjectType Type
        {
            get
            {
                if (this.status == TerrainObjectStatus.Disposed) { throw new ObjectDisposedException("TerrainObject"); }
                return this.type;
            }
        }

        /// <see cref="ITerrainObject.MapCoords"/>
        public RCIntVector MapCoords
        {
            get
            {
                if (this.status == TerrainObjectStatus.Disposed) { throw new ObjectDisposedException("TerrainObject"); }
                return this.mapCoords;
            }

            set
            {
                if (this.status == TerrainObjectStatus.Disposed) { throw new ObjectDisposedException("TerrainObject"); }
                if (this.status != TerrainObjectStatus.Detached) { throw new InvalidOperationException("Invalid TerrainObject state!"); }
                if (mapCoords == RCIntVector.Undefined) { throw new ArgumentNullException("MapCoords"); }
                this.mapCoords = value;
                this.violatingQuadCoordsCacheIsDirty = true;
            }
        }

        /// <see cref="ITerrainObject.ViolatingQuadCoords"/>
        public IEnumerable<RCIntVector> ViolatingQuadCoords
        {
            get
            {
                if (this.status == TerrainObjectStatus.Disposed) { throw new ObjectDisposedException("TerrainObject"); }
                if (this.status != TerrainObjectStatus.Detached) { throw new InvalidOperationException("Invalid TerrainObject state!"); }
                return this.GetViolatingQuadCoordsImpl();
            }
        }

        /// <see cref="ITerrainObject.Attach"/>
        public void Attach()
        {
            if (this.status == TerrainObjectStatus.Disposed) { throw new ObjectDisposedException("TerrainObject"); }
            if (this.status != TerrainObjectStatus.Detached) { throw new InvalidOperationException("Invalid TerrainObject state!"); }
            if (this.GetViolatingQuadCoordsImpl().Count > 0) { throw new MapException("TerrainObject has quadratic tiles violating the constraints!"); }

            /// Set the references to the appropriate quadratic tiles and cells.
            for (int quadX = 0; quadX < this.type.QuadraticSize.X; quadX++)
            {
                for (int quadY = 0; quadY < this.type.QuadraticSize.Y; quadY++)
                {
                    RCIntVector relQuadCoords = new RCIntVector(quadX, quadY);
                    if (!this.type.IsExcluded(relQuadCoords))
                    {
                        QuadTile currQuadTile = this.parentMap.GetQuadTile(this.mapCoords + relQuadCoords);
                        this.quadTiles[quadX, quadY] = currQuadTile;
                        for (int navX = 0; navX < MapStructure.NAVCELL_PER_QUAD; navX++)
                        {
                            for (int navY = 0; navY < MapStructure.NAVCELL_PER_QUAD; navY++)
                            {
                                RCIntVector relNavCoords = relQuadCoords * MapStructure.NAVCELL_PER_QUAD + new RCIntVector(navX, navY);
                                this.cells[relNavCoords.X, relNavCoords.Y] = currQuadTile.GetCellImpl(new RCIntVector(navX, navY));
                            }
                        }
                    }
                }
            }

            /// TODO: Apply the cell data changesets!
            /// TODO: Attach this TerrainObject to the map!
            this.status = TerrainObjectStatus.Attached;
        }

        /// <see cref="ITerrainObject.GetQuadTile"/>
        public IQuadTile GetQuadTile(RCIntVector index)
        {
            if (this.status == TerrainObjectStatus.Disposed) { throw new ObjectDisposedException("TerrainObject"); }
            if (this.status != TerrainObjectStatus.Attached) { throw new InvalidOperationException("Invalid TerrainObject state!"); }
            if (index == RCIntVector.Undefined) { throw new ArgumentNullException("index"); }
            if (index.X < 0 || index.Y < 0 || index.X >= this.type.QuadraticSize.X || index.Y >= this.type.QuadraticSize.Y) { throw new ArgumentOutOfRangeException("index"); }

            return this.quadTiles[index.X, index.Y];
        }

        #endregion ITerrainObject methods

        #region ICellDataChangeSetTarget methods

        /// <see cref="ICellDataChangeSetTarget.GetCell"/>
        public ICell GetCell(RCIntVector index)
        {
            if (this.status == TerrainObjectStatus.Disposed) { throw new ObjectDisposedException("TerrainObject"); }
            if (this.status != TerrainObjectStatus.Attached) { throw new InvalidOperationException("Invalid TerrainObject state!"); }
            if (index == RCIntVector.Undefined) { throw new ArgumentNullException("index"); }
            if (index.X < 0 || index.Y < 0 || index.X >= this.type.QuadraticSize.X * MapStructure.NAVCELL_PER_QUAD || index.Y >= this.type.QuadraticSize.Y * MapStructure.NAVCELL_PER_QUAD) { throw new ArgumentOutOfRangeException("index"); }

            return this.cells[index.X, index.Y];
        }

        /// <see cref="ICellDataChangeSetTarget.CellSize"/>
        public RCIntVector CellSize
        {
            get
            {
                return new RCIntVector(this.type.QuadraticSize.X * MapStructure.NAVCELL_PER_QUAD,
                                       this.type.QuadraticSize.Y * MapStructure.NAVCELL_PER_QUAD);
            }
        }

        #endregion ICellDataChangeSetTarget methods

        /// <summary>
        /// Gets the map structure that this terrain object belongs to.
        /// </summary>
        public MapStructure ParentMap
        {
            get
            {
                if (this.status == TerrainObjectStatus.Disposed) { throw new ObjectDisposedException("TerrainObject"); }
                return this.parentMap;
            }
        }

        /// <summary>
        /// Internal implementation of ITerrainObject.ViolatingQuadCoords_get.
        /// </summary>
        private HashSet<RCIntVector> GetViolatingQuadCoordsImpl()
        {
            /// Collect the violating quadratic tiles if necessary.
            if (this.violatingQuadCoordsCacheIsDirty)
            {
                this.violatingQuadCoordsCache = new HashSet<RCIntVector>();

                /// Check intersection with the boundaries of the map.
                for (int quadX = 0; quadX < this.type.QuadraticSize.X; quadX++)
                {
                    for (int quadY = 0; quadY < this.type.QuadraticSize.Y; quadY++)
                    {
                        RCIntVector relQuadCoords = new RCIntVector(quadX, quadY);
                        RCIntVector absQuadCoords = this.mapCoords + relQuadCoords;
                        if (absQuadCoords.X < 0 || absQuadCoords.X >= this.parentMap.Size.X ||
                            absQuadCoords.Y < 0 || absQuadCoords.Y >= this.parentMap.Size.Y)
                        {
                            this.violatingQuadCoordsCache.Add(relQuadCoords);
                        }
                    }
                }

                /// TODO: Check intersection with other terrain objects attached to the map!!!

                /// Check against the constraints defined by the type of this terrain object.
                this.violatingQuadCoordsCache.UnionWith(this.type.CheckConstraints(this));
                this.violatingQuadCoordsCacheIsDirty = false;
            }

            return this.violatingQuadCoordsCache;
        }

        /// <summary>
        /// List of the quadratic tiles of this terrain object.
        /// </summary>
        private QuadTile[,] quadTiles;

        /// <summary>
        /// List of the cells of this terrain object.
        /// </summary>
        private Cell[,] cells;

        /// <summary>
        /// This list is caching the currently violating quadratic coordinates.
        /// </summary>
        private HashSet<RCIntVector> violatingQuadCoordsCache;

        /// <summary>
        /// This flag indicates whether the violating quadratic coordinates cache is dirty or not.
        /// </summary>
        private bool violatingQuadCoordsCacheIsDirty;

        /// <summary>
        /// The map coordinates of this terrain object.
        /// </summary>
        private RCIntVector mapCoords;

        /// <summary>
        /// Reference to the type of this terrain object.
        /// </summary>
        private TerrainObjectType type;

        /// <summary>
        /// Reference to the map that this terrain object belongs to.
        /// </summary>
        private MapStructure parentMap;

        /// <summary>
        /// The current status of this terrain object.
        /// </summary>
        private TerrainObjectStatus status;
    }
}
