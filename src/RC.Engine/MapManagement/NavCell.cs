using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine
{
    /// <summary>
    /// Represents a navigation cell on a map.
    /// </summary>
    class NavCell : INavCell
    {
        /// <summary>
        /// Constructs a navigation cell.
        /// </summary>
        /// <param name="map">The map that this navigation cell belongs to.</param>
        /// <param name="quadCoords">The coordinates of this navigation cell inside it's parent quadratic tile.</param>
        public NavCell(Map map, QuadTile quadTile, RCIntVector quadCoords)
        {
            if (map == null) { throw new ArgumentNullException("map"); }
            if (quadTile == null) { throw new ArgumentNullException("quadTile"); }
            if (quadCoords == RCIntVector.Undefined) { throw new ArgumentNullException("quadCoords"); }
            if (quadCoords.X < 0 || quadCoords.X >= Map.NAVCELL_PER_QUAD || quadCoords.Y < 0 || quadCoords.Y >= Map.NAVCELL_PER_QUAD) { throw new ArgumentOutOfRangeException("mapCoords"); }

            this.parentMap = map;
            this.parentQuadTile = quadTile;
            this.quadCoords = quadCoords;
            this.data = null;
            this.mapCoords = this.parentQuadTile.MapCoords * (new RCIntVector(Map.NAVCELL_PER_QUAD, Map.NAVCELL_PER_QUAD)) + this.quadCoords;
            this.neighbours = new NavCell[8];
            this.detachedNeighbours = new List<Tuple<NavCell, MapDirection>>();
            this.parentIsoTile = null;                  /// will be set later
            this.isoIndices = RCIntVector.Undefined;     /// will be set later
        }

        #region INavCell methods

        /// <see cref="INavCell.Data"/>
        public CellData Data { get { return this.data; } }

        /// <see cref="INavCell.ParentQuadTile"/>
        public IQuadTile ParentQuadTile { get { return this.parentQuadTile; } }

        /// <see cref="INavCell.ParentIsoTile"/>
        public IIsoTile ParentIsoTile { get { return this.parentIsoTile; } }

        #endregion INavCell methods

        #region Internal map structure buildup methods

        /// <summary>
        /// Sets the given navigation cell as a neighbour of this navigation cell in the given direction.
        /// </summary>
        /// <param name="neighbour">The navigation cell to be set as a neighbour of this.</param>
        /// <param name="direction">The direction of the new neighbour.</param>
        public void SetNeighbour(NavCell neighbour, MapDirection direction)
        {
            if (neighbour == null) { throw new ArgumentNullException("neighbour"); }
            if (neighbour.parentMap != this.parentMap) { throw new InvalidOperationException("Neighbour navigation cells must have the same parent map!"); }
            if (this.parentMap.Status != MapStatus.InitStructure) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }
            if (this.neighbours[(int)direction] != null) { throw new InvalidOperationException(string.Format("Neighbour in direction {0} already set!", direction)); }

            this.neighbours[(int)direction] = neighbour;
        }

        /// <summary>
        /// Sets the given isometric tile as the parent of this navigation cell.
        /// </summary>
        /// <param name="tile">The isometric tile to set as the parent of this navigation cell.</param>
        /// <param name="isoIndices">The indices of this navigation inside the parent isometric tile.</param>
        public void SetIsoTile(IsoTile tile, RCIntVector isoIndices)
        {
            if (tile == null) { throw new ArgumentNullException("tile"); }
            if (isoIndices == RCIntVector.Undefined) { throw new ArgumentNullException("isoIndices"); }
            if (tile.ParentMap != this.parentMap) { throw new InvalidOperationException("Navigation cells and their parent isometric tile must have the same parent map!"); }
            if (this.parentMap.Status != MapStatus.InitStructure) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }
            if (this.parentIsoTile != null) { throw new InvalidOperationException("Parent isometric tile already set!"); }

            this.parentIsoTile = tile;
            this.isoIndices = isoIndices;
        }

        #endregion Internal map structure buildup methods

        #region Internal attach and detach methods

        /// <summary>
        /// Detaches the appropriate neighbours of this navigation cell depending on the size of the map.
        /// </summary>
        public void DetachNeighboursAtSize()
        {
            if (this.parentMap.Status != MapStatus.Opening) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }

            for (int dir = 0; dir < this.neighbours.Length; dir++)
            {
                if (this.neighbours[dir] != null &&
                    (this.neighbours[dir].mapCoords.X >= this.parentMap.Size.X * Map.NAVCELL_PER_QUAD ||
                     this.neighbours[dir].mapCoords.Y >= this.parentMap.Size.Y * Map.NAVCELL_PER_QUAD))
                {
                    this.detachedNeighbours.Add(new Tuple<NavCell, MapDirection>(this.neighbours[dir], (MapDirection)dir));
                    this.neighbours[dir] = null;
                }
            }
        }

        /// <summary>
        /// Initializes the fields of this navigation cell.
        /// </summary>
        public void InitializeFields()
        {
            if (this.parentMap.Status != MapStatus.Opening) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }
            this.data = new CellData(this.parentMap.Tileset.DefaultValues);
        }

        /// <summary>
        /// Reinitializes the fields of this navigation cell.
        /// </summary>
        public void ReinitializeFields()
        {
            if (this.parentMap.Status != MapStatus.DrawingTerrain) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }
            this.data = new CellData(this.parentMap.Tileset.DefaultValues);
        }

        /// <summary>
        /// Uninitializes the fields of this navigation cell.
        /// </summary>
        public void UninitializeFields()
        {
            if (this.parentMap.Status != MapStatus.Closing) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }
            this.data = null;
        }

        /// <summary>
        /// Attaches the appropriate neighbours of this navigation cell depending on the size of the map.
        /// </summary>
        public void AttachNeighboursAtSize()
        {
            if (this.parentMap.Status != MapStatus.Closing) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }

            foreach (Tuple<NavCell, MapDirection> item in this.detachedNeighbours)
            {
                this.neighbours[(int)item.Item2] = item.Item1;
            }
            this.detachedNeighbours.Clear();
        }

        #endregion Internal attach and detach methods

        /// <summary>
        /// Gets the map coordinates of this navigation cell.
        /// </summary>
        public RCIntVector MapCoords { get { return this.mapCoords; } }

        /// <summary>
        /// Gets the coordinates of this navigation cell inside its parent quadratic tile.
        /// </summary>
        public RCIntVector QuadCoords { get { return this.quadCoords; } }

        /// <summary>
        /// Gets the indices of this navigation cell inside its parent isometric tile.
        /// </summary>
        public RCIntVector IsoIndices { get { return this.isoIndices; } }

        /// <summary>
        /// Gets the parent map of this navigation cell.
        /// </summary>
        public IMap ParentMap { get { return this.parentMap; } }

        /// <summary>
        /// Reference to the map that this navigation cell belongs to.
        /// </summary>
        private Map parentMap;

        /// <summary>
        /// The map coordinates of this navigation cell.
        /// </summary>
        private RCIntVector mapCoords;

        /// <summary>
        /// The coordinates of this navigation cell inside its parent quadratic tile.
        /// </summary>
        private RCIntVector quadCoords;

        /// <summary>
        /// The coordinates of this navigation cell inside its parent isometric tile.
        /// </summary>
        private RCIntVector isoIndices;

        /// <summary>
        /// Reference to the data attached to this navigation cell.
        /// </summary>
        private CellData data;

        /// <summary>
        /// List of the neighbours of this navigation cell.
        /// </summary>
        private NavCell[] neighbours;

        /// <summary>
        /// List of the detached neighbours and their direction.
        /// </summary>
        private List<Tuple<NavCell, MapDirection>> detachedNeighbours;

        /// <summary>
        /// Reference to the quadratic tile that this navigation cell belongs to.
        /// </summary>
        private QuadTile parentQuadTile;

        /// <summary>
        /// Reference to the isometric tile that this navigation cell belongs to.
        /// </summary>
        private IsoTile parentIsoTile;
    }
}
