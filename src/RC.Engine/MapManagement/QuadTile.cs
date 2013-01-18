using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine
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
        public QuadTile(Map map, IsoTile isoTile, RCIntVector mapCoords)
        {
            if (map == null) { throw new ArgumentNullException("map"); }
            if (isoTile == null) { throw new ArgumentNullException("isoTile"); }
            if (mapCoords == RCIntVector.Undefined) { throw new ArgumentNullException("mapCoords"); }
            if (mapCoords.X < 0 || mapCoords.X >= Map.MAX_MAPSIZE || mapCoords.Y < 0 || mapCoords.Y >= Map.MAX_MAPSIZE) { throw new ArgumentOutOfRangeException("mapCoords"); }

            this.parentMap = map;
            this.parentIsoTile = isoTile;
            this.mapCoords = mapCoords;
            this.neighbours = new QuadTile[8];
            this.detachedNeighbours = new List<Tuple<QuadTile, MapDirection>>();

            this.navCells = new NavCell[Map.NAVCELL_PER_QUAD, Map.NAVCELL_PER_QUAD];
            for (int col = 0; col < Map.NAVCELL_PER_QUAD; col++)
            {
                for (int row = 0; row < Map.NAVCELL_PER_QUAD; row++)
                {
                    this.navCells[col, row] = new NavCell(this.parentMap, this, new RCIntVector(col, row));
                }
            }
        }

        #region IQuadTile methods

        /// <see cref="IQuadTile.ParentIsoTile"/>
        public IIsoTile IsoTile { get { return this.GetIsoTile(); } }

        #endregion IQuadTile methods

        /// <summary>
        /// Gets the navigation cell of this quadratic tile at the given coordinates.
        /// </summary>
        /// <param name="x">The X coordinate of the navigation cell to get.</param>
        /// <param name="y">The Y coordinate of the navigation cell to get.</param>
        /// <returns>The navigation cell of this quadratic tile at the given coordinates.</returns>
        public NavCell this[int x, int y] { get { return this.navCells[x, y]; } }

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
            if (this.parentMap.Status != MapStatus.InitStructure) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }
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
            if (this.parentMap.Status != MapStatus.Opening) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }

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
            if (this.parentMap.Status != MapStatus.Closing) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }

            foreach (Tuple<QuadTile, MapDirection> item in this.detachedNeighbours)
            {
                this.neighbours[(int)item.Item2] = item.Item1;
            }
            this.detachedNeighbours.Clear();
        }

        #endregion Internal attach and detach methods

        /// <summary>
        /// Gets the map coordinates of this quadratic tile.
        /// </summary>
        public RCIntVector MapCoords { get { return this.mapCoords; } }

        /// <summary>
        /// Gets the reference to the parent isometric tile.
        /// </summary>
        public IsoTile GetIsoTile() { return this.parentIsoTile; }

        /// <summary>
        /// The 2D array of the navigation cells of this quadratic tile.
        /// </summary>
        private NavCell[,] navCells;

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
        private Map parentMap;
    }
}
