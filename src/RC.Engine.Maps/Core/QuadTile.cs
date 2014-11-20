using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Maps.Core
{
    /// <summary>
    /// Represents a quadratic tile on a map.
    /// </summary>
    class QuadTile : IQuadTile
    {
        /// <summary>
        /// Constructs a quadratic tile.
        /// </summary>
        /// <param name="map">The map that this quadratic tile belongs to.</param>
        /// <param name="primaryIsoTile">Primary isometric tile.</param>
        /// <param name="secondaryIsoTile">Secondary isometric tile or null if doesn't exist.</param>
        /// <param name="mapCoords">The map coordinates of this quadratic tile.</param>
        public QuadTile(MapStructure map, IsoTile primaryIsoTile, IsoTile secondaryIsoTile, RCIntVector mapCoords)
        {
            if (map == null) { throw new ArgumentNullException("map"); }
            if (primaryIsoTile == null) { throw new ArgumentNullException("isoTile"); }
            if (mapCoords == RCIntVector.Undefined) { throw new ArgumentNullException("mapCoords"); }
            if (mapCoords.X < 0 || mapCoords.X >= MapStructure.MAX_MAPSIZE || mapCoords.Y < 0 || mapCoords.Y >= MapStructure.MAX_MAPSIZE) { throw new ArgumentOutOfRangeException("mapCoords"); }

            this.parentMap = map;
            this.primaryIsoTile = primaryIsoTile;
            this.secondaryIsoTile = secondaryIsoTile;
            this.terrainObject = null;
            this.mapCoords = mapCoords;
            this.neighbours = new QuadTile[8];
            this.detachedNeighbours = new List<Tuple<QuadTile, MapDirection>>();
            this.isBuildableCache = new CachedValue<bool>(this.CalculateBuildabilityFlag);
            this.groundLevelCache = new CachedValue<int>(this.CalculateGroundLevel);

            this.cells = new Cell[MapStructure.NAVCELL_PER_QUAD, MapStructure.NAVCELL_PER_QUAD];
            for (int col = 0; col < MapStructure.NAVCELL_PER_QUAD; col++)
            {
                for (int row = 0; row < MapStructure.NAVCELL_PER_QUAD; row++)
                {
                    this.cells[col, row] = new Cell(this.parentMap, this, new RCIntVector(col, row));
                }
            }

            this.primaryIsoTile.SetCuttingQuadTile(this);
            if (this.secondaryIsoTile != null) { this.secondaryIsoTile.SetCuttingQuadTile(this); }
        }

        #region IQuadTile methods

        /// <see cref="IQuadTile.ParentIsoTile"/>
        public IIsoTile PrimaryIsoTile { get { return this.GetPrimaryIsoTile(); } }

        /// <see cref="IQuadTile.SecondaryIsoTile"/>
        public IIsoTile SecondaryIsoTile { get { return this.secondaryIsoTile; } }

        /// <see cref="IQuadTile.TerrainObject"/>
        public ITerrainObject TerrainObject { get { return this.terrainObject; } }

        /// <see cref="IQuadTile.MapCoords"/>
        public RCIntVector MapCoords { get { return this.mapCoords; } }

        /// <see cref="IQuadTile.GetNeighbour"/>
        public IQuadTile GetNeighbour(MapDirection direction)
        {
            return this.GetNeighbourImpl(direction);
        }

        /// <see cref="IQuadTile.Neighbours"/>
        public IEnumerable<IQuadTile> Neighbours
        {
            get
            {
                List<QuadTile> retList = new List<QuadTile>();
                for (int i = 0; i < this.neighbours.Length; i++)
                {
                    if (this.neighbours[i] != null) { retList.Add(this.neighbours[i]); }
                }
                return retList;
            }
        }

        /// <see cref="IQuadTile.IsBuildable"/>
        public bool IsBuildable { get { return this.parentMap.Status == MapStructure.MapStatus.Finalized ? this.isBuildableCache.Value : this.CalculateBuildabilityFlag(); } }

        /// <see cref="IQuadTile.GroundLevel"/>
        public int GroundLevel { get { return this.parentMap.Status == MapStructure.MapStatus.Finalized ? this.groundLevelCache.Value : this.CalculateGroundLevel(); } }

        #endregion IQuadTile methods

        #region ICellDataChangeSetTarget methods

        /// <see cref="ICellDataChangeSetTarget.NavCellDims"/>
        public RCIntVector CellSize { get { return new RCIntVector(MapStructure.NAVCELL_PER_QUAD, MapStructure.NAVCELL_PER_QUAD); } }

        /// <see cref="ICellDataChangeSetTarget.GetNavCell"/>
        public ICell GetCell(RCIntVector index) { return this.GetCellImpl(index); }

        #endregion ICellDataChangeSetTarget methods

        #region Internal public methods

        /// <summary>
        /// Internal implementation of IQuadTile.GetCell
        /// </summary>
        public Cell GetCellImpl(RCIntVector index) { return this.cells[index.X, index.Y]; }

        /// <summary>
        /// Internal implementation of the IQuadTile.GetNeighbour method.
        /// </summary>
        public QuadTile GetNeighbourImpl(MapDirection dir)
        {
            return this.neighbours[(int)dir];
        }

        /// <summary>
        /// Gets the reference to the primary isometric tile.
        /// </summary>
        public IsoTile GetPrimaryIsoTile() { return this.primaryIsoTile; }

        /// <summary>
        /// Finalizes this quadratic tile.
        /// </summary>
        public void FinalizeTile()
        {
            for (int col = 0; col < MapStructure.NAVCELL_PER_QUAD; col++)
            {
                for (int row = 0; row < MapStructure.NAVCELL_PER_QUAD; row++)
                {
                    this.cells[col, row].Lock();
                }
            }
        }

        /// <summary>
        /// Cleans up the caches of this quadratic tile.
        /// </summary>
        public void Cleanup()
        {
            for (int col = 0; col < MapStructure.NAVCELL_PER_QUAD; col++)
            {
                for (int row = 0; row < MapStructure.NAVCELL_PER_QUAD; row++)
                {
                    this.cells[col, row].UninitializeFields();
                }
            }

            this.isBuildableCache.Invalidate();
            this.groundLevelCache.Invalidate();
            this.terrainObject = null;
        }

        /// <summary>
        /// Sets the given quadratic tile as a neighbour of this quadratic tile in the given direction.
        /// </summary>
        /// <param name="neighbour">The quadratic tile to be set as a neighbour of this.</param>
        /// <param name="direction">The direction of the new neighbour.</param>
        public void SetNeighbour(QuadTile neighbour, MapDirection direction)
        {
            if (neighbour == null) { throw new ArgumentNullException("neighbour"); }
            if (neighbour.parentMap != this.parentMap) { throw new InvalidOperationException("Neighbour quadratic tiles must have the same parent map!"); }
            if (this.parentMap.Status != MapStructure.MapStatus.Initializing) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }
            if (this.neighbours[(int)direction] != null) { throw new InvalidOperationException(string.Format("Neighbour in direction {0} already set!", direction)); }

            this.neighbours[(int)direction] = neighbour;
        }

        /// <summary>
        /// Attaches the given terrain object to this quadratic tile.
        /// </summary>
        /// <param name="terrainObj">The terrain object to attach.</param>
        /// <exception cref="InvalidOperationException">If a terrain object has already been attached to this quadratic tile.</exception>
        public void AttachTerrainObject(ITerrainObject terrainObj)
        {
            if (terrainObj == null) { throw new ArgumentNullException("terrainObj"); }
            if (this.terrainObject != null) { throw new InvalidOperationException("A terrain object has already been attached to this quadratic tile!"); }
            this.terrainObject = terrainObj;
        }

        /// <summary>
        /// Detaches the currently attached terrain object from this quadratic tile.
        /// </summary>
        /// <exception cref="InvalidOperationException">If there is no terrain object attached to this quadratic tile.</exception>
        public void DetachTerrainObject()
        {
            if (this.terrainObject == null) { throw new InvalidOperationException("No terrain object attached to this quadratic tile!"); }
            this.terrainObject = null;
        }

        #endregion Internal public methods

        #region Internal attach and detach methods

        /// <summary>
        /// Detaches the appropriate neighbours of this quadratic tile depending on the size of the map.
        /// </summary>
        public void DetachNeighboursAtSize()
        {
            if (this.parentMap.Status != MapStructure.MapStatus.Opening) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }

            for (int dir = 0; dir < this.neighbours.Length; dir++)
            {
                if (this.neighbours[dir] != null &&
                    (this.neighbours[dir].mapCoords.X >= this.parentMap.Size.X ||
                     this.neighbours[dir].mapCoords.Y >= this.parentMap.Size.Y))
                {
                    this.detachedNeighbours.Add(new Tuple<QuadTile, MapDirection>(this.neighbours[dir], (MapDirection)dir));
                    this.neighbours[dir] = null;
                }
            }
        }

        /// <summary>
        /// Attaches the appropriate neighbours of this quadratic tile depending on the size of the map.
        /// </summary>
        public void AttachNeighboursAtSize()
        {
            if (this.parentMap.Status != MapStructure.MapStatus.Closing) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }

            foreach (Tuple<QuadTile, MapDirection> item in this.detachedNeighbours)
            {
                this.neighbours[(int)item.Item2] = item.Item1;
            }
            this.detachedNeighbours.Clear();
        }

        #endregion Internal attach and detach methods
        
        #region Private methods

        /// <summary>
        /// Internal method for calculating the buildability flag for this quadratic tile.
        /// </summary>
        /// <returns>The calculated buildability flag.</returns>
        private bool CalculateBuildabilityFlag()
        {
            bool isBuildable = true;
            for (int row = 0; row < MapStructure.NAVCELL_PER_QUAD; row++)
            {
                for (int col = 0; col < MapStructure.NAVCELL_PER_QUAD; col++)
                {
                    ICell checkedCell = this.cells[col, row];
                    if (!checkedCell.IsBuildable)
                    {
                        isBuildable = false;
                        break;
                    }
                }
                if (!isBuildable) { break; }
            }
            return isBuildable;
        }

        /// <summary>
        /// Internal method for calculating the ground level for this quadratic tile.
        /// </summary>
        /// <returns>The calculated ground level.</returns>
        private int CalculateGroundLevel()
        {
            RCNumber groundLevelSum = 0;
            for (int row = 0; row < MapStructure.NAVCELL_PER_QUAD; row++)
            {
                for (int col = 0; col < MapStructure.NAVCELL_PER_QUAD; col++)
                {
                    groundLevelSum += this.cells[col, row].GroundLevel;
                }
            }
            return (groundLevelSum / (MapStructure.NAVCELL_PER_QUAD * MapStructure.NAVCELL_PER_QUAD)).Round();
        }

        #endregion Private methods

        /// <summary>
        /// The 2D array of the cells of this quadratic tile.
        /// </summary>
        private Cell[,] cells;

        /// <summary>
        /// Reference to the primary isometric tile.
        /// </summary>
        private IsoTile primaryIsoTile;

        /// <summary>
        /// Reference to the secondary isometric tile or null if this quadratic tile has no secondary isometric tile.
        /// </summary>
        private IsoTile secondaryIsoTile;

        /// <summary>
        /// Reference to the terrain object that is present in this quadratic tile or null if there is no terrain object in this quadratic tile.
        /// </summary>
        private ITerrainObject terrainObject;

        /// <summary>
        /// The map coordinates of this quadratic tile.
        /// </summary>
        private RCIntVector mapCoords;

        /// <summary>
        /// List of the neighbours of this quadratic tile.
        /// </summary>
        private QuadTile[] neighbours;

        /// <summary>
        /// The cache that stores the buildability flag of this quadratic tile.
        /// </summary>
        private CachedValue<bool> isBuildableCache;

        /// <summary>
        /// The cache that stores the calculated ground level of this quadratic tile.
        /// </summary>
        private CachedValue<int> groundLevelCache;

        /// <summary>
        /// List of the detached neighbours and their direction.
        /// </summary>
        private List<Tuple<QuadTile, MapDirection>> detachedNeighbours;

        /// <summary>
        /// Reference to the map that this quadratic tile belongs to.
        /// </summary>
        private MapStructure parentMap;
    }
}
