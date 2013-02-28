using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.PublicInterfaces;

namespace RC.Engine.Core
{
    /// <summary>
    /// Represents a cell on a map.
    /// </summary>
    class Cell : ICell
    {
        /// <summary>
        /// Constructs a cell.
        /// </summary>
        /// <param name="map">The map that this cell belongs to.</param>
        /// <param name="quadCoords">The coordinates of this cell inside it's parent quadratic tile.</param>
        public Cell(MapStructure map, QuadTile quadTile, RCIntVector quadCoords)
        {
            if (map == null) { throw new ArgumentNullException("map"); }
            if (quadTile == null) { throw new ArgumentNullException("quadTile"); }
            if (quadCoords == RCIntVector.Undefined) { throw new ArgumentNullException("quadCoords"); }
            if (quadCoords.X < 0 || quadCoords.X >= MapStructure.NAVCELL_PER_QUAD || quadCoords.Y < 0 || quadCoords.Y >= MapStructure.NAVCELL_PER_QUAD) { throw new ArgumentOutOfRangeException("mapCoords"); }

            this.parentMap = map;
            this.parentQuadTile = quadTile;
            this.quadCoords = quadCoords;
            this.data = null;
            this.mapCoords = this.parentQuadTile.MapCoords * (new RCIntVector(MapStructure.NAVCELL_PER_QUAD, MapStructure.NAVCELL_PER_QUAD)) + this.quadCoords;
            this.neighbours = new Cell[8];
            this.detachedNeighbours = new List<Tuple<Cell, MapDirection>>();
            this.parentIsoTile = null;                  /// will be set later
            this.isoIndices = RCIntVector.Undefined;     /// will be set later
        }

        #region ICell methods

        /// <see cref="ICell.Data"/>
        public ICellData Data { get { return this.data; } }

        /// <see cref="ICell.ParentQuadTile"/>
        public IQuadTile ParentQuadTile { get { return this.parentQuadTile; } }

        /// <see cref="ICell.ParentIsoTile"/>
        public IIsoTile ParentIsoTile { get { return this.parentIsoTile; } }

        #endregion ICell methods

        #region Internal map structure buildup methods

        /// <summary>
        /// Sets the given cell as a neighbour of this cell in the given direction.
        /// </summary>
        /// <param name="neighbour">The cell to be set as a neighbour of this.</param>
        /// <param name="direction">The direction of the new neighbour.</param>
        public void SetNeighbour(Cell neighbour, MapDirection direction)
        {
            if (neighbour == null) { throw new ArgumentNullException("neighbour"); }
            if (neighbour.parentMap != this.parentMap) { throw new InvalidOperationException("Neighbour cells must have the same parent map!"); }
            if (this.parentMap.Status != MapStructure.MapStatus.Initializing) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }
            if (this.neighbours[(int)direction] != null) { throw new InvalidOperationException(string.Format("Neighbour in direction {0} already set!", direction)); }

            this.neighbours[(int)direction] = neighbour;
        }

        /// <summary>
        /// Sets the given isometric tile as the parent of this cell.
        /// </summary>
        /// <param name="tile">The isometric tile to set as the parent of this cell.</param>
        /// <param name="isoIndices">The indices of this cell inside the parent isometric tile.</param>
        public void SetIsoTile(IsoTile tile, RCIntVector isoIndices)
        {
            if (tile == null) { throw new ArgumentNullException("tile"); }
            if (isoIndices == RCIntVector.Undefined) { throw new ArgumentNullException("isoIndices"); }
            if (tile.ParentMap != this.parentMap) { throw new InvalidOperationException("cells and their parent isometric tile must have the same parent map!"); }
            if (this.parentMap.Status != MapStructure.MapStatus.Initializing) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }
            if (this.parentIsoTile != null) { throw new InvalidOperationException("Parent isometric tile already set!"); }

            this.parentIsoTile = tile;
            this.isoIndices = isoIndices;
        }

        #endregion Internal map structure buildup methods

        #region Internal attach and detach methods

        /// <summary>
        /// Detaches the appropriate neighbours of this cell depending on the size of the map.
        /// </summary>
        public void DetachNeighboursAtSize()
        {
            if (this.parentMap.Status != MapStructure.MapStatus.Opening) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }

            for (int dir = 0; dir < this.neighbours.Length; dir++)
            {
                if (this.neighbours[dir] != null &&
                    (this.neighbours[dir].mapCoords.X >= this.parentMap.Size.X * MapStructure.NAVCELL_PER_QUAD ||
                     this.neighbours[dir].mapCoords.Y >= this.parentMap.Size.Y * MapStructure.NAVCELL_PER_QUAD))
                {
                    this.detachedNeighbours.Add(new Tuple<Cell, MapDirection>(this.neighbours[dir], (MapDirection)dir));
                    this.neighbours[dir] = null;
                }
            }
        }

        /// <summary>
        /// Initializes the fields of this cell.
        /// </summary>
        public void InitializeFields()
        {
            if (this.parentMap.Status != MapStructure.MapStatus.Opening) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }
            this.data = this.parentMap.Tileset.DefaultCellData.Clone();
        }

        /// <summary>
        /// Uninitializes the fields of this cell.
        /// </summary>
        public void UninitializeFields()
        {
            if (this.parentMap.Status != MapStructure.MapStatus.Closing) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }
            this.data = null;
        }

        /// <summary>
        /// Attaches the appropriate neighbours of this cell depending on the size of the map.
        /// </summary>
        public void AttachNeighboursAtSize()
        {
            if (this.parentMap.Status != MapStructure.MapStatus.Closing) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }

            foreach (Tuple<Cell, MapDirection> item in this.detachedNeighbours)
            {
                this.neighbours[(int)item.Item2] = item.Item1;
            }
            this.detachedNeighbours.Clear();
        }

        #endregion Internal attach and detach methods

        /// <summary>
        /// Gets the map coordinates of this cell.
        /// </summary>
        public RCIntVector MapCoords { get { return this.mapCoords; } }

        /// <summary>
        /// Gets the coordinates of this cell inside its parent quadratic tile.
        /// </summary>
        public RCIntVector QuadCoords { get { return this.quadCoords; } }

        /// <summary>
        /// Gets the indices of this cell inside its parent isometric tile.
        /// </summary>
        public RCIntVector IsoIndices { get { return this.isoIndices; } }

        /// <summary>
        /// Gets the parent map of this cell.
        /// </summary>
        public MapStructure ParentMap { get { return this.parentMap; } }

        /// <summary>
        /// Reference to the map that this cell belongs to.
        /// </summary>
        private MapStructure parentMap;

        /// <summary>
        /// The map coordinates of this cell.
        /// </summary>
        private RCIntVector mapCoords;

        /// <summary>
        /// The coordinates of this cell inside its parent quadratic tile.
        /// </summary>
        private RCIntVector quadCoords;

        /// <summary>
        /// The coordinates of this cell inside its parent isometric tile.
        /// </summary>
        private RCIntVector isoIndices;

        /// <summary>
        /// Reference to the data attached to this cell.
        /// </summary>
        private ICellData data;

        /// <summary>
        /// List of the neighbours of this cell.
        /// </summary>
        private Cell[] neighbours;

        /// <summary>
        /// List of the detached neighbours and their direction.
        /// </summary>
        private List<Tuple<Cell, MapDirection>> detachedNeighbours;

        /// <summary>
        /// Reference to the quadratic tile that this cell belongs to.
        /// </summary>
        private QuadTile parentQuadTile;

        /// <summary>
        /// Reference to the isometric tile that this cell belongs to.
        /// </summary>
        private IsoTile parentIsoTile;
    }
}
