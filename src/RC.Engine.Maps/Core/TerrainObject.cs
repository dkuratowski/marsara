using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Maps.Core
{
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
        /// <param name="mapCoords">The coordinates of the top-left quadratic tile of this terrain object.</param>
        public TerrainObject(IMapAccess map, ITerrainObjectType type, RCIntVector mapCoords)
        {
            if (map == null) { throw new ArgumentNullException("map"); }
            if (type == null) { throw new ArgumentNullException("type"); }
            if (mapCoords == RCIntVector.Undefined) { throw new ArgumentNullException("type"); }
            if (type.Tileset != map.Tileset) { throw new MapException("Tileset of the TerrainObjectType and tileset of the map are not the same!"); }

            this.mapCoords = mapCoords;
            this.type = type;
            this.parentMap = map;

            this.cells = new Cell[this.type.QuadraticSize.X * MapStructure.NAVCELL_PER_QUAD,
                                  this.type.QuadraticSize.Y * MapStructure.NAVCELL_PER_QUAD];
            this.quadTiles = new QuadTile[this.type.QuadraticSize.X, this.type.QuadraticSize.Y];

            /// Set the references to the appropriate quadratic tiles and cells.
            for (int quadX = 0; quadX < this.type.QuadraticSize.X; quadX++)
            {
                for (int quadY = 0; quadY < this.type.QuadraticSize.Y; quadY++)
                {
                    RCIntVector relQuadCoords = new RCIntVector(quadX, quadY);
                    if (!this.type.IsExcluded(relQuadCoords))
                    {
                        IQuadTile currQuadTile = this.parentMap.GetQuadTile(this.mapCoords + relQuadCoords);
                        this.quadTiles[quadX, quadY] = currQuadTile;
                        for (int navX = 0; navX < MapStructure.NAVCELL_PER_QUAD; navX++)
                        {
                            for (int navY = 0; navY < MapStructure.NAVCELL_PER_QUAD; navY++)
                            {
                                RCIntVector relNavCoords = relQuadCoords * MapStructure.NAVCELL_PER_QUAD + new RCIntVector(navX, navY);
                                this.cells[relNavCoords.X, relNavCoords.Y] = currQuadTile.GetCell(new RCIntVector(navX, navY));
                            }
                        }
                    }
                }
            }
            /// TODO: Apply the cell data changesets!
            /// TODO: Attach this TerrainObject to the map!
        }

        #region ISearchTreeContent methods

        /// <see cref="ISearchTreeContent.BoundingBox"/>
        public RCNumRectangle BoundingBox
        {
            get
            {
                RCIntVector cellSize = this.CellSize;
                return new RCNumRectangle(this.mapCoords.X * MapStructure.NAVCELL_PER_QUAD - (RCNumber)1 / (RCNumber)2,
                                          this.mapCoords.Y * MapStructure.NAVCELL_PER_QUAD - (RCNumber)1 / (RCNumber)2,
                                          cellSize.X,
                                          cellSize.Y);
            }
        }

        /// <see cref="ISearchTreeContent.BoundingBoxChanging"/>
        public event ContentBoundingBoxChangeHdl BoundingBoxChanging;

        /// <see cref="ISearchTreeContent.BoundingBoxChanged"/>
        public event ContentBoundingBoxChangeHdl BoundingBoxChanged;

        #endregion ISearchTreeContent methods

        #region ITerrainObject methods

        /// <see cref="ITerrainObject.Type"/>
        public ITerrainObjectType Type { get { return this.type; } }

        /// <see cref="ITerrainObject.MapCoords"/>
        public RCIntVector MapCoords { get { return this.mapCoords; } }

        /// <see cref="ITerrainObject.GetQuadTile"/>
        public IQuadTile GetQuadTile(RCIntVector index)
        {
            if (index == RCIntVector.Undefined) { throw new ArgumentNullException("index"); }
            if (index.X < 0 || index.Y < 0 || index.X >= this.type.QuadraticSize.X || index.Y >= this.type.QuadraticSize.Y) { throw new ArgumentOutOfRangeException("index"); }

            return this.quadTiles[index.X, index.Y];
        }

        /// <see cref="ITerrainObject.ParentMap"/>
        public IMapAccess ParentMap { get { return this.parentMap; } }

        #endregion ITerrainObject methods

        #region ICellDataChangeSetTarget methods

        /// <see cref="ICellDataChangeSetTarget.GetCell"/>
        public ICell GetCell(RCIntVector index)
        {
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
        /// List of the quadratic tiles of this terrain object.
        /// </summary>
        private IQuadTile[,] quadTiles;

        /// <summary>
        /// List of the cells of this terrain object.
        /// </summary>
        private ICell[,] cells;

        /// <summary>
        /// The map coordinates of this terrain object.
        /// </summary>
        private RCIntVector mapCoords;

        /// <summary>
        /// Reference to the type of this terrain object.
        /// </summary>
        private ITerrainObjectType type;

        /// <summary>
        /// Reference to the map that this terrain object belongs to.
        /// </summary>
        private IMapAccess parentMap;
    }
}
