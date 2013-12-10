using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Maps.Core
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
            this.walkabilityFlag = new Stack<bool>();
            this.buildabilityFlag = new Stack<bool>();
            this.groundLevel = new Stack<int>();
            this.isLocked = false;
            this.mapCoords = this.parentQuadTile.MapCoords * (new RCIntVector(MapStructure.NAVCELL_PER_QUAD, MapStructure.NAVCELL_PER_QUAD)) + this.quadCoords;
            this.neighbours = new Cell[8];
            this.detachedNeighbours = new List<Tuple<Cell, MapDirection>>();
            this.parentIsoTile = null;                  /// will be set later
            this.isoIndices = RCIntVector.Undefined;     /// will be set later
        }

        #region ICell methods

        /// <see cref="ICell.ParentQuadTile"/>
        public IQuadTile ParentQuadTile { get { return this.parentQuadTile; } }

        /// <see cref="ICell.ParentIsoTile"/>
        public IIsoTile ParentIsoTile { get { return this.parentIsoTile; } }

        /// <see cref="ICell.MapCoords"/>
        public RCIntVector MapCoords { get { return this.mapCoords; } }

        /// <see cref="ICell.IsWalkable"/>
        public bool IsWalkable { get { return this.walkabilityFlag.Peek(); } }

        /// <see cref="ICell.IsBuildable"/>
        public bool IsBuildable { get { return this.buildabilityFlag.Peek(); } }

        /// <see cref="ICell.GroundLevel"/>
        public int GroundLevel { get { return this.groundLevel.Peek(); } }

        /// <see cref="ICell.GroundLevel"/>
        public void Lock()
        {
            if (this.parentMap.Status != MapStructure.MapStatus.Opened) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }
            this.isLocked = true;
        }

        /// <see cref="ICell.ChangeWalkability"/>
        public void ChangeWalkability(bool newVal)
        {
            if (this.isLocked) { throw new InvalidOperationException("Cell data locked!"); }
            this.walkabilityFlag.Push(newVal);
        }

        /// <see cref="ICell.ChangeBuildability"/>
        public void ChangeBuildability(bool newVal)
        {
            if (this.isLocked) { throw new InvalidOperationException("Cell data locked!"); }
            this.buildabilityFlag.Push(newVal);
        }

        /// <see cref="ICell.ChangeGroundLevel"/>
        public void ChangeGroundLevel(int newVal)
        {
            if (this.isLocked) { throw new InvalidOperationException("Cell data locked!"); }
            this.groundLevel.Push(newVal);
        }

        /// <see cref="ICell.UndoWalkabilityChange"/>
        public void UndoWalkabilityChange()
        {
            if (this.isLocked) { throw new InvalidOperationException("Cell data locked!"); }
            if (this.walkabilityFlag.Count == 0) { throw new InvalidOperationException("The very first walkability change has already been undone!"); }
            this.walkabilityFlag.Pop();
        }

        /// <see cref="ICell.UndoBuildabilityChange"/>
        public void UndoBuildabilityChange()
        {
            if (this.isLocked) { throw new InvalidOperationException("Cell data locked!"); }
            if (this.buildabilityFlag.Count == 0) { throw new InvalidOperationException("The very first buildability change has already been undone!"); }
            this.buildabilityFlag.Pop();
        }

        /// <see cref="ICell.UndoGroundLevelChange"/>
        public void UndoGroundLevelChange()
        {
            if (this.isLocked) { throw new InvalidOperationException("Cell data locked!"); }
            if (this.groundLevel.Count == 0) { throw new InvalidOperationException("The very first ground level change has already been undone!"); }
            this.groundLevel.Pop();
        }

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

            this.walkabilityFlag.Clear();
            this.buildabilityFlag.Clear();
            this.groundLevel.Clear();
            this.walkabilityFlag.Push(DEFAULT_WALKABILITY);
            this.buildabilityFlag.Push(DEFAULT_BUILDABILITY);
            this.groundLevel.Push(DEFAULT_GROUNDLEVEL);
        }

        /// <summary>
        /// Uninitializes the fields of this cell.
        /// </summary>
        public void UninitializeFields()
        {
            if (this.parentMap.Status != MapStructure.MapStatus.Closing) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }

            this.walkabilityFlag.Clear();
            this.buildabilityFlag.Clear();
            this.groundLevel.Clear();
            this.isLocked = false;
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
        /// Stores the current and the previous values of the walkability flag of this cell.
        /// </summary>
        private Stack<bool> walkabilityFlag;

        /// <summary>
        /// Stores the current and the previous values of the buildability flag of this cell.
        /// </summary>
        private Stack<bool> buildabilityFlag;

        /// <summary>
        /// Stores the current and the previous values of the ground level of this cell.
        /// </summary>
        private Stack<int> groundLevel;

        /// <summary>
        /// This flag indicates whether this Cell object has been locked or not. Writing data of locked cells is not possible.
        /// </summary>
        private bool isLocked;

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

        /// <summary>
        /// The default values of the cell data.
        /// </summary>
        private const bool DEFAULT_WALKABILITY = true;
        private const bool DEFAULT_BUILDABILITY = true;
        private const int DEFAULT_GROUNDLEVEL = 0;
    }
}
