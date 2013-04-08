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
        /// <param name="isoTile">Parent isometric tile.</param>
        /// <param name="mapCoords">The map coordinates of this quadratic tile.</param>
        public QuadTile(MapStructure map, IsoTile isoTile, RCIntVector mapCoords)
        {
            if (map == null) { throw new ArgumentNullException("map"); }
            if (isoTile == null) { throw new ArgumentNullException("isoTile"); }
            if (mapCoords == RCIntVector.Undefined) { throw new ArgumentNullException("mapCoords"); }
            if (mapCoords.X < 0 || mapCoords.X >= MapStructure.MAX_MAPSIZE || mapCoords.Y < 0 || mapCoords.Y >= MapStructure.MAX_MAPSIZE) { throw new ArgumentOutOfRangeException("mapCoords"); }

            this.parentMap = map;
            this.parentIsoTile = isoTile;
            this.mapCoords = mapCoords;
            this.neighbours = new QuadTile[8];
            this.detachedNeighbours = new List<Tuple<QuadTile, MapDirection>>();

            this.cells = new Cell[MapStructure.NAVCELL_PER_QUAD, MapStructure.NAVCELL_PER_QUAD];
            for (int col = 0; col < MapStructure.NAVCELL_PER_QUAD; col++)
            {
                for (int row = 0; row < MapStructure.NAVCELL_PER_QUAD; row++)
                {
                    this.cells[col, row] = new Cell(this.parentMap, this, new RCIntVector(col, row));
                }
            }
        }

        #region IQuadTile methods

        /// <see cref="IQuadTile.ParentIsoTile"/>
        public IIsoTile IsoTile { get { return this.GetIsoTile(); } }

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

        #endregion IQuadTile methods

        #region ICellDataChangeSetTarget methods

        /// <see cref="ICellDataChangeSetTarget.NavCellDims"/>
        public RCIntVector CellSize { get { return new RCIntVector(MapStructure.NAVCELL_PER_QUAD, MapStructure.NAVCELL_PER_QUAD); } }

        /// <see cref="ICellDataChangeSetTarget.GetNavCell"/>
        public ICell GetCell(RCIntVector index) { return this.GetCellImpl(index); }

        #endregion ICellDataChangeSetTarget methods

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

        #region Internal map structure buildup methods

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
        #endregion Internal map structure buildup methods

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

        /// <summary>
        /// Gets the reference to the parent isometric tile.
        /// </summary>
        public IsoTile GetIsoTile() { return this.parentIsoTile; }

        /// <summary>
        /// The 2D array of the cells of this quadratic tile.
        /// </summary>
        private Cell[,] cells;

        /// <summary>
        /// Reference to the parent isometric tile.
        /// </summary>
        private IsoTile parentIsoTile;

        /// <summary>
        /// The map coordinates of this quadratic tile.
        /// </summary>
        private RCIntVector mapCoords;

        /// <summary>
        /// List of the neighbours of this quadratic tile.
        /// </summary>
        private QuadTile[] neighbours;

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
