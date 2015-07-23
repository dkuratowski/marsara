using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Maps.Core
{
    /// <summary>
    /// Represents an isometric tile on a map.
    /// </summary>
    class IsoTile : IIsoTile
    {
        /// <summary>
        /// Constructs an isometric tile.
        /// </summary>
        /// <param name="map">The map that this isometric tile belongs to.</param>
        /// <param name="mapCoords">The map coordinates of this isometric tile.</param>
        public IsoTile(MapStructure map, RCIntVector mapCoords)
        {
            if (map == null) { throw new ArgumentNullException("map"); }
            if (mapCoords == RCIntVector.Undefined) { throw new ArgumentNullException("mapCoords"); }

            this.parentMap = map;
            this.mapCoords = mapCoords;
            this.type = null;
            this.variantIdx = -1;
            this.variant = null;
            this.isReady = false;
            this.neighbours = new IsoTile[8];
            this.detachedNeighbours = new List<Tuple<IsoTile, MapDirection>>();
            this.detachedCells = new List<Cell>();
            this.cuttingQuadTiles = new RCSet<QuadTile>();
            this.detachedCuttingQuadTiles = new RCSet<QuadTile>();

            this.cells = new Cell[MapStructure.QUAD_PER_ISO_VERT * MapStructure.NAVCELL_PER_QUAD,
                                        MapStructure.QUAD_PER_ISO_HORZ * MapStructure.NAVCELL_PER_QUAD];

            this.referenceCell = null;
        }

        #region IIsoTile methods

        /// <see cref="IIsoTile.MapCoords"/>
        public RCIntVector MapCoords { get { return this.mapCoords; } }

        /// <see cref="IIsoTile.GetNeighbour"/>
        public IIsoTile GetNeighbour(MapDirection direction)
        {
            return this.GetNeighbourImpl(direction);
        }

        /// <see cref="IIsoTile.Type"/>
        public IIsoTileType Type { get { return this.type; } }

        /// <see cref="IIsoTile.Variant"/>
        public IIsoTileVariant Variant { get { return this.variant; } }

        /// <see cref="IIsoTile.VariantIdx"/>
        public int VariantIdx { get { return this.variantIdx; } }

        /// <see cref="IIsoTile.Neighbours"/>
        public IEnumerable<IIsoTile> Neighbours
        {
            get
            {
                for (int i = 0; i < this.neighbours.Length; i++)
                {
                    if (this.neighbours[i] != null) { yield return this.neighbours[i]; }
                }
            }
        }

        /// <see cref="IIsoTile.CuttingQuadTiles"/>
        public IEnumerable<IQuadTile> CuttingQuadTiles
        {
            get
            {
                foreach (QuadTile cuttingQuadTile in this.cuttingQuadTiles)
                {
                    if (!this.detachedCuttingQuadTiles.Contains(cuttingQuadTile)) { yield return cuttingQuadTile; }
                }
            }
        }

        /// <see cref="IIsoTile.ExchangeType"/>
        public void ExchangeType(IIsoTileType newType)
        {
            if (this.parentMap.Status != MapStructure.MapStatus.ExchangingTiles) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }
            if (newType == null) { throw new ArgumentNullException("newType"); }
            
            this.type = newType;

            this.parentMap.OnIsoTileExchanged(this);
            foreach (IsoTile neighbour in this.neighbours) { if (neighbour != null) { this.parentMap.OnIsoTileExchanged(neighbour); } }
        }

        /// <see cref="IIsoTile.GetCellMapCoords"/>
        public RCIntVector GetCellMapCoords(RCIntVector index)
        {
            if (this.referenceCell == null) { throw new InvalidOperationException("Reference cell doesn't exist!"); }
            if (index == RCIntVector.Undefined) { throw new ArgumentNullException("index"); }
            if (index.X < 0 || index.Y < 0 || index.X >= MapStructure.QUAD_PER_ISO_VERT * MapStructure.NAVCELL_PER_QUAD || index.Y >= MapStructure.QUAD_PER_ISO_HORZ * MapStructure.NAVCELL_PER_QUAD) { throw new ArgumentOutOfRangeException("index"); }

            return this.referenceCell.MapCoords + index - this.referenceCell.IsoIndices;
        }

        #endregion IIsoTile methods

        #region ICellDataChangeSetTarget methods

        /// <see cref="ICellDataChangeSetTarget.GetCell"/>
        public ICell GetCell(RCIntVector index)
        {
            if (index == RCIntVector.Undefined) { throw new ArgumentNullException("index"); }
            if (index.X < 0 || index.Y < 0 || index.X >= MapStructure.QUAD_PER_ISO_VERT * MapStructure.NAVCELL_PER_QUAD || index.Y >= MapStructure.QUAD_PER_ISO_HORZ * MapStructure.NAVCELL_PER_QUAD) { throw new ArgumentOutOfRangeException("index"); }

            return this.cells[index.X, index.Y];
        }

        /// <see cref="ICellDataChangeSetTarget.CellSize"/>
        public RCIntVector CellSize
        {
            get
            {
                return new RCIntVector(MapStructure.QUAD_PER_ISO_VERT * MapStructure.NAVCELL_PER_QUAD,
                                       MapStructure.QUAD_PER_ISO_HORZ * MapStructure.NAVCELL_PER_QUAD);
            }
        }

        #endregion ICellDataChangeSetTarget methods

        #region Internal public methods

        /// <summary>
        /// Gets the map that this isometric tile belongs to.
        /// </summary>
        public MapStructure ParentMap { get { return this.parentMap; } }

        #endregion Internal public methods

        #region Internal map structure buildup methods

        /// <summary>
        /// Sets the given isometric tile as a neighbour of this isometric tile in the given direction.
        /// </summary>
        /// <param name="neighbour">The isometric tile to be set as a neighbour of this.</param>
        /// <param name="direction">The direction of the new neighbour.</param>
        public void SetNeighbour(IsoTile neighbour, MapDirection direction)
        {
            if (neighbour == null) { throw new ArgumentNullException("neighbour"); }
            if (neighbour.parentMap != this.parentMap) { throw new InvalidOperationException("Neighbour isometric tiles must have the same parent map!"); }
            if (this.parentMap.Status != MapStructure.MapStatus.Initializing) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }
            if (this.neighbours[(int)direction] != null) { throw new InvalidOperationException(string.Format("Neighbour in direction {0} already set!", direction)); }

            this.neighbours[(int)direction] = neighbour;
        }

        /// <summary>
        /// Sets the given cell as a child of this isometric tile at the given indices.
        /// </summary>
        /// <param name="cell">The cell to set as a child.</param>
        /// <param name="indices">The indices of the child.</param>
        public void SetCell(Cell cell, RCIntVector indices)
        {
            if (cell == null) { throw new ArgumentNullException("cell"); }
            if (indices == RCIntVector.Undefined) { throw new ArgumentNullException("indices"); }
            if (cell.ParentMap != this.parentMap) { throw new InvalidOperationException("Isometric tiles and their child cells must have the same parent map!"); }
            if (this.parentMap.Status != MapStructure.MapStatus.Initializing) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }
            if (this.cells[indices.X, indices.Y] != null) { throw new InvalidOperationException(string.Format("Child cell at {0} already set!", indices)); }

            this.cells[indices.X, indices.Y] = cell;
            if (this.referenceCell == null) { this.referenceCell = cell; }
        }

        /// <summary>
        /// Sets the given quadratic tile as a cutting quadratic tile for this isometric tile.
        /// </summary>
        /// <param name="quadTile">The quadratic tile to set.</param>
        public void SetCuttingQuadTile(QuadTile quadTile)
        {
            if (quadTile == null) { throw new ArgumentNullException("quadTile"); }
            this.cuttingQuadTiles.Add(quadTile);
        }

        #endregion Internal map structure buildup methods

        #region Internal attach and detach methods

        /// <summary>
        /// Detaches the appropriate neighbours and cells of this isometric tile depending on the size of the map.
        /// </summary>
        public void DetachAtSize()
        {
            if (this.parentMap.Status != MapStructure.MapStatus.Opening) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }

            RCNumVector edges = new RCNumVector((RCNumber)(this.parentMap.Size.X * 2 - 1) / 2,
                                                (RCNumber)(this.parentMap.Size.Y * 2 - 1) / 2);

            /// Detach the appropriate neighbours.
            for (int dir = 0; dir < this.neighbours.Length; dir++)
            {
                if (this.neighbours[dir] != null)
                {
                    IsoTile neighbour = this.neighbours[dir];
                    RCNumVector quadCoords = MapStructure.QuadIsoTransform.TransformBA(neighbour.mapCoords);
                    if (quadCoords.X > edges.X || quadCoords.Y > edges.Y)
                    {
                        this.detachedNeighbours.Add(new Tuple<IsoTile, MapDirection>(this.neighbours[dir], (MapDirection)dir));
                        this.neighbours[dir] = null;
                    }
                }
            }

            /// Detach the appropriate cells.
            for (int col = 0; col < MapStructure.QUAD_PER_ISO_VERT * MapStructure.NAVCELL_PER_QUAD; col++)
            {
                for (int row = 0; row < MapStructure.QUAD_PER_ISO_HORZ * MapStructure.NAVCELL_PER_QUAD; row++)
                {
                    if (this.cells[col, row] != null &&
                        (this.cells[col, row].MapCoords.X >= this.parentMap.Size.X * MapStructure.NAVCELL_PER_QUAD ||
                         this.cells[col, row].MapCoords.Y >= this.parentMap.Size.Y * MapStructure.NAVCELL_PER_QUAD))
                    {
                        this.detachedCells.Add(this.cells[col, row]);
                        this.cells[col, row] = null;
                    }
                }
            }

            /// Detach the appropriate cutting quadratic tiles.
            foreach (QuadTile cuttingQuadTile in this.cuttingQuadTiles)
            {
                if (cuttingQuadTile.MapCoords.X >= this.parentMap.Size.X || cuttingQuadTile.MapCoords.Y >= this.parentMap.Size.Y)
                {
                    this.detachedCuttingQuadTiles.Add(cuttingQuadTile);
                }
            }
        }

        /// <summary>
        /// Attaches the appropriate neighbours and cells of this isometric tile depending on the size of the map.
        /// </summary>
        public void AttachAtSize()
        {
            if (this.parentMap.Status != MapStructure.MapStatus.Closing) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }

            /// Attach the detached neighbours.
            foreach (Tuple<IsoTile, MapDirection> item in this.detachedNeighbours)
            {
                this.neighbours[(int)item.Item2] = item.Item1;
            }
            this.detachedNeighbours.Clear();

            /// Attach the detached cells.
            foreach (Cell cell in this.detachedCells)
            {
                this.cells[cell.IsoIndices.X, cell.IsoIndices.Y] = cell;
            }
            this.detachedCells.Clear();

            /// Attach the detached cutting quadratic tiles.
            this.detachedCuttingQuadTiles.Clear();
        }

        #endregion Internal attach and detach methods

        #region Internal initializing and finalizing methods

        /// <summary>
        /// Sets the default tile type of the map for this isometric tile. If a tile type has already been set then this function has no effect.
        /// </summary>
        public void SetDefaultTileType()
        {
            if (this.parentMap.Status != MapStructure.MapStatus.Opening) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }

            if (this.type == null)
            {
                this.type = this.parentMap.DefaultTileType;
                this.variantIdx = -1;
            }
        }

        /// TODO: Check if necessary!
        /// <summary>
        /// Sets the given tile type of this isometric tile.
        /// </summary>
        /// <param name="type">The tile type to be set.</param>
        /// <param name="variantIdx">The index of the variant to be set.</param>
        /// <exception cref="InvalidOperationException">If a tile type has already been set for this isometric tile.</exception>
        public void SetTileType(IIsoTileType type, int variantIdx)
        {
            if (this.parentMap.Status != MapStructure.MapStatus.Opening) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }
            if (type == null) { throw new ArgumentNullException("type"); }
            if (variantIdx < 0) { throw new ArgumentOutOfRangeException("variantIdx", "Variant index must be non-negative!"); }
            if (this.type != null) { throw new InvalidOperationException(string.Format("Tile type already set for isometric tile at {0}!", this.mapCoords)); }

            this.type = type;
            this.variantIdx = variantIdx;
        }

        /// <summary>
        /// Validates this isometric tile against the tileset, loads the appropriate tile variant and
        /// load the data into the cells.
        /// </summary>
        public void CheckAndFinalize()
        {
            if (this.parentMap.Status != MapStructure.MapStatus.Opening) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }
            if (this.type == null) { throw new MapException(string.Format("Tile type not set for isometric tile at {0}", this.mapCoords)); }

            /// Check and finalize only if necessary.
            if (this.isReady) { return; }
                        
            /// Check against the tileset.
            this.ValidateNeighbour(MapDirection.NorthEast);
            this.ValidateNeighbour(MapDirection.SouthEast);
            this.ValidateNeighbour(MapDirection.SouthWest);
            this.ValidateNeighbour(MapDirection.NorthWest);

            /// If the variant index has not yet been set, we have to randomly generate one.
            if (this.variantIdx == -1) { this.variantIdx = RandomService.DefaultGenerator.Next(this.type.GetNumOfVariants(this)); }

            /// Load the data from the appropriate variant into the cells.
            this.variant = this.type.GetVariant(this, this.variantIdx);
            foreach (ICellDataChangeSet changeset in this.variant.CellDataChangesets)
            {
                changeset.Apply(this);
            }

            this.isReady = true;
        }

        #endregion Internal initializing and finalizing methods

        #region Internal cleanup methods

        public void Cleanup()
        {
            if (this.parentMap.Status != MapStructure.MapStatus.Closing) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }

            /// Cleanup only if necessary.
            if (!this.isReady) { return; }

            this.type = null;
            this.variantIdx = -1;
            this.variant = null;

            this.isReady = false;
        }

        #endregion Internal cleanup methods

        #region Internal tile exchanging helper methods

        /// <summary>
        /// Validates this isometric tile against the tileset.
        /// </summary>
        public void Validate()
        {
            this.ValidateNeighbour(MapDirection.NorthEast);
            this.ValidateNeighbour(MapDirection.SouthEast);
            this.ValidateNeighbour(MapDirection.SouthWest);
            this.ValidateNeighbour(MapDirection.NorthWest);
        }

        /// <summary>
        /// Generates a new variant index for this isometric tile.
        /// </summary>
        public void ReplaceVariant()
        {
            if (this.parentMap.Status != MapStructure.MapStatus.ExchangingTiles) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }

            /// Undo the changesets of the current variant.
            foreach (ICellDataChangeSet changeset in this.variant.CellDataChangesets.Reverse())
            {
                changeset.Undo(this);
            }

            /// Generate the new variant.
            this.variantIdx = RandomService.DefaultGenerator.Next(this.type.GetNumOfVariants(this));
            this.variant = this.type.GetVariant(this, this.variantIdx);

            /// Apply the changesets of the new variant.
            foreach (ICellDataChangeSet changeset in this.variant.CellDataChangesets)
            {
                changeset.Apply(this);
            }
        }

        #endregion Internal terrain draw helper methods

        /// <summary>
        /// True if this isometric tile is detached from the map, false otherwise.
        /// </summary>
        public bool IsDetached { get { return this.type == null; } }

        /// <summary>
        /// Internal implementation of the IIsoTile.GetNeighbour method.
        /// </summary>
        public IsoTile GetNeighbourImpl(MapDirection dir)
        {
            return this.neighbours[(int)dir];
        }

        /// <summary>
        /// Validates whether the neighbour of this isometric tile in the given direction satisfies the
        /// constraints of the tileset.
        /// </summary>
        /// <param name="dir">The direction of the neighbour to validate.</param>
        private void ValidateNeighbour(MapDirection dir)
        {
            IsoTile neighbour = this.neighbours[(int)dir];
            if (neighbour != null)
            {
                /// To simplify the algorithm, we use terrain combinations rotated to MapDirection.NorthEast.
                TerrainCombination thisCombRot = MapHelper.RotateTerrainCombination(this.type.Combination, dir, MapDirection.NorthEast);
                TerrainCombination neighCombRot = MapHelper.RotateTerrainCombination(neighbour.type.Combination, dir, MapDirection.NorthEast);

                /// Generate the terrain-NESW array for this tile and the neighbour.
                ITerrainType[] thisNESWRot = MapHelper.GetTerrainNESW(this.type.TerrainA, this.type.TerrainB, thisCombRot);
                ITerrainType[] neighNESWRot = MapHelper.GetTerrainNESW(neighbour.type.TerrainA, neighbour.type.TerrainB, neighCombRot);

                /// Check the generated terrain-NESW arrays.
                if (thisNESWRot[0] != neighNESWRot[3] || thisNESWRot[1] != neighNESWRot[2])
                {
                    throw new MapException(string.Format("Invalid neighbours at {0} and {1}!", this.mapCoords, neighbour.mapCoords));
                }

                /// Check whether the given direction satisfies the transition-length constraint.
                if (this.type.TerrainB != null && this.type.TerrainB.TransitionLength > 0 &&
                    thisNESWRot[0] == this.type.TerrainA && thisNESWRot[1] == this.type.TerrainA)
                {
                    int remaining = this.type.TerrainB.TransitionLength;
                    IsoTile currTile = neighbour;
                    while (currTile != null && remaining > 0)
                    {
                        /// Generate the terrain-NESW array of the currently checked tile.
                        TerrainCombination currCombRot = MapHelper.RotateTerrainCombination(currTile.type.Combination, dir, MapDirection.NorthEast);
                        ITerrainType[] currNESWRot = MapHelper.GetTerrainNESW(currTile.type.TerrainA, currTile.type.TerrainB, currCombRot);

                        /// Check if the currently checked tile is part of the transition.
                        if (currNESWRot[0] != this.type.TerrainA || currNESWRot[1] != this.type.TerrainA ||
                            currNESWRot[2] != this.type.TerrainA || currNESWRot[3] != this.type.TerrainA)
                        {
                            /// No, it's not part of the transition. We have to check whether the upcoming terrain type
                            /// is another child of TerrainA or not.
                            if (currNESWRot[2] == this.type.TerrainA && currNESWRot[3] == this.type.TerrainA &&
                                (currNESWRot[0] == this.type.TerrainA || currNESWRot[0].Parent == this.type.TerrainA) &&
                                (currNESWRot[1] == this.type.TerrainA || currNESWRot[1].Parent == this.type.TerrainA))
                            {
                                /// It's another child of TerrainA, no contradiction with the tileset -> OK.
                                break;
                            }
                            else
                            {
                                /// It's not a child of TerrainA -> Error.
                                throw new MapException(string.Format("Invalid transition from {0} in direction {1}! Length must be at least {2}!", this.mapCoords, dir, this.type.TerrainB.TransitionLength));
                            }
                        }

                        /// Yes, it's part of the transition. We can switch to the next tile in the same direction.
                        currTile = currTile.neighbours[(int)dir];
                        remaining--;
                    }
                }
            }
        }

        /// <summary>
        /// The type of this isometric tile.
        /// </summary>
        private IIsoTileType type;

        /// <summary>
        /// The index of the variant to be selected from the tile type at the end of the initialization process
        /// or -1 if it has to be selected randomly.
        /// </summary>
        private int variantIdx;

        /// <summary>
        /// Reference to the current variant of this tile.
        /// </summary>
        private IIsoTileVariant variant;

        /// <summary>
        /// This flag indicates whether this isometric tile is ready to use (true) or it is cleaned up (false).
        /// </summary>
        private bool isReady;

        /// <summary>
        /// The map coordinates of this isometric tile.
        /// </summary>
        private RCIntVector mapCoords;

        /// <summary>
        /// List of the neighbours of this isometric tile.
        /// </summary>
        private IsoTile[] neighbours;

        /// <summary>
        /// List of all quadratic tiles that cuts this isometric tile.
        /// </summary>
        private RCSet<QuadTile> cuttingQuadTiles;

        /// <summary>
        /// List of the detached quadratic tiles that cuts this isometric tile.
        /// </summary>
        private RCSet<QuadTile> detachedCuttingQuadTiles;

        /// <summary>
        /// List of the detached neighbours and their direction.
        /// </summary>
        private List<Tuple<IsoTile, MapDirection>> detachedNeighbours;

        /// <summary>
        /// List of the detached cells and their coordinates.
        /// </summary>
        private List<Cell> detachedCells;

        /// <summary>
        /// List of the cells of this isometric tile.
        /// </summary>
        private Cell[,] cells;

        /// <summary>
        /// The reference cell.
        /// </summary>
        private Cell referenceCell;

        /// <summary>
        /// Reference to the map that this isometric tile belongs to.
        /// </summary>
        private MapStructure parentMap;
    }
}
