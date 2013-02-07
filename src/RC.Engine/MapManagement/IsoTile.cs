using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine
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
        public IsoTile(Map map, RCIntVector mapCoords)
        {
            if (map == null) { throw new ArgumentNullException("map"); }
            if (mapCoords == RCIntVector.Undefined) { throw new ArgumentNullException("mapCoords"); }
            if (variantIdx < 0 && variantIdx != -1) { throw new ArgumentOutOfRangeException("variantIdx"); }

            this.parentMap = map;
            this.mapCoords = mapCoords;
            this.type = null;
            this.variantIdx = -1;
            this.variant = null;
            this.isReady = false;
            this.neighbours = new IsoTile[8];
            this.detachedNeighbours = new List<Tuple<IsoTile, MapDirection>>();
            this.detachedNavCells = new List<NavCell>();

            this.navCells = new NavCell[Map.QUAD_PER_ISO_VERT * Map.NAVCELL_PER_QUAD,
                                        Map.QUAD_PER_ISO_HORZ * Map.NAVCELL_PER_QUAD];

            this.referenceNavCell = null;
        }

        #region IIsoTile methods

        /// <see cref="IIsoTile.MapCoords"/>
        public RCIntVector MapCoords { get { return this.mapCoords; } }

        /// <see cref="IIsoTile.ParentMap"/>
        public IMap ParentMap { get { return this.parentMap; } }

        /// <see cref="IIsoTile.GetNeighbour"/>
        public IIsoTile GetNeighbour(MapDirection direction)
        {
            return this.GetNeighbourImpl(direction);
        }

        /// <see cref="IIsoTile.Type"/>
        public TileType Type { get { return this.type; } }

        /// <see cref="IIsoTile.Variant"/>
        public TileVariant Variant { get { return this.variant; } }

        /// <see cref="IIsoTile.GetNavCell"/>
        public INavCell GetNavCell(RCIntVector index)
        {
            if (index == RCIntVector.Undefined) { throw new ArgumentNullException("index"); }
            if (index.X < 0 || index.Y < 0 || index.X >= Map.QUAD_PER_ISO_VERT * Map.NAVCELL_PER_QUAD || index.Y >= Map.QUAD_PER_ISO_HORZ * Map.NAVCELL_PER_QUAD) { throw new ArgumentOutOfRangeException("index"); }

            return this.navCells[index.X, index.Y];
        }

        /// <see cref="IIsoTile.GetNavCellCoords"/>
        public RCIntVector GetNavCellCoords(RCIntVector index)
        {
            if (this.referenceNavCell == null) { throw new InvalidOperationException("Reference navigation cell doesn't exist!"); }
            if (index == RCIntVector.Undefined) { throw new ArgumentNullException("index"); }
            if (index.X < 0 || index.Y < 0 || index.X >= Map.QUAD_PER_ISO_VERT * Map.NAVCELL_PER_QUAD || index.Y >= Map.QUAD_PER_ISO_HORZ * Map.NAVCELL_PER_QUAD) { throw new ArgumentOutOfRangeException("index"); }

            return this.referenceNavCell.MapCoords + index - this.referenceNavCell.IsoIndices;
        }

        #endregion IIsoTile methods

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
            if (this.parentMap.Status != MapStatus.InitStructure) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }
            if (this.neighbours[(int)direction] != null) { throw new InvalidOperationException(string.Format("Neighbour in direction {0} already set!", direction)); }

            this.neighbours[(int)direction] = neighbour;
        }

        /// <summary>
        /// Sets the given navigation cell as a child of this isometric tile at the given indices.
        /// </summary>
        /// <param name="cell">The navigation cell to set as a child.</param>
        /// <param name="indices">The indices of the child.</param>
        public void SetNavCell(NavCell cell, RCIntVector indices)
        {
            if (cell == null) { throw new ArgumentNullException("cell"); }
            if (indices == RCIntVector.Undefined) { throw new ArgumentNullException("indices"); }
            if (cell.ParentMap != this.parentMap) { throw new InvalidOperationException("Isometric tiles and their child navigation cells must have the same parent map!"); }
            if (this.parentMap.Status != MapStatus.InitStructure) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }
            if (this.navCells[indices.X, indices.Y] != null) { throw new InvalidOperationException(string.Format("Child navigation cell at {0} already set!", indices)); }

            this.navCells[indices.X, indices.Y] = cell;
            if (this.referenceNavCell == null) { this.referenceNavCell = cell; }
        }

        #endregion Internal map structure buildup methods

        #region Internal attach and detach methods

        /// <summary>
        /// Detaches the appropriate neighbours and navigation cells of this isometric tile depending on the size of the map.
        /// </summary>
        public void DetachAtSize()
        {
            if (this.parentMap.Status != MapStatus.Opening) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }

            RCNumVector edges = new RCNumVector((RCNumber)(this.parentMap.Size.X * 2 - 1) / 2,
                                                (RCNumber)(this.parentMap.Size.Y * 2 - 1) / 2);

            /// Detach the appropriate neighbours.
            for (int dir = 0; dir < this.neighbours.Length; dir++)
            {
                if (this.neighbours[dir] != null)
                {
                    IsoTile neighbour = this.neighbours[dir];
                    RCNumVector quadCoords = Map.QuadIsoTransform.TransformBA(neighbour.mapCoords);
                    if (quadCoords.X > edges.X || quadCoords.Y > edges.Y)
                    {
                        this.detachedNeighbours.Add(new Tuple<IsoTile, MapDirection>(this.neighbours[dir], (MapDirection)dir));
                        this.neighbours[dir] = null;
                    }
                }
            }

            /// Detach the appropriate navigation cells.
            for (int col = 0; col < Map.QUAD_PER_ISO_VERT * Map.NAVCELL_PER_QUAD; col++)
            {
                for (int row = 0; row < Map.QUAD_PER_ISO_HORZ * Map.NAVCELL_PER_QUAD; row++)
                {
                    if (this.navCells[col, row] != null &&
                        (this.navCells[col, row].MapCoords.X >= this.parentMap.Size.X ||
                         this.navCells[col, row].MapCoords.Y >= this.parentMap.Size.Y))
                    {
                        this.detachedNavCells.Add(this.navCells[col, row]);
                        this.navCells[col, row] = null;
                    }
                }
            }
        }

        /// <summary>
        /// Attaches the appropriate neighbours and navigation cells of this isometric tile depending on the size of the map.
        /// </summary>
        public void AttachAtSize()
        {
            if (this.parentMap.Status != MapStatus.Closing) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }

            /// Attach the detached neighbours.
            foreach (Tuple<IsoTile, MapDirection> item in this.detachedNeighbours)
            {
                this.neighbours[(int)item.Item2] = item.Item1;
            }
            this.detachedNeighbours.Clear();

            /// Attach the detached navigation cells.
            foreach (NavCell cell in this.detachedNavCells)
            {
                this.navCells[cell.IsoIndices.X, cell.IsoIndices.Y] = cell;
            }
            this.detachedNavCells.Clear();
        }

        #endregion Internal attach and detach methods

        #region Internal initializing and finalizing methods

        /// <summary>
        /// Sets the default tile type of the map for this isometric tile. If a tile type has already been set then this function has no effect.
        /// </summary>
        public void SetDefaultTileType()
        {
            if (this.parentMap.Status != MapStatus.Opening) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }

            if (this.type == null)
            {
                this.type = this.parentMap.DefaultTileType;
                this.variantIdx = -1;
            }
        }

        /// <summary>
        /// Sets the given tile type of this isometric tile.
        /// </summary>
        /// <param name="type">The tile type to be set.</param>
        /// <param name="variantIdx">The index of the variant to be set.</param>
        /// <exception cref="InvalidOperationException">If a tile type has already been set for this isometric tile.</exception>
        public void SetTileType(TileType type, int variantIdx)
        {
            if (this.parentMap.Status != MapStatus.Opening) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }
            if (type == null) { throw new ArgumentNullException("type"); }
            if (variantIdx < 0) { throw new ArgumentOutOfRangeException("variantIdx", "Variant index must be non-negative!"); }
            if (this.type != null) { throw new InvalidOperationException(string.Format("Tile type already set for isometric tile at {0}!", this.mapCoords)); }

            this.type = type;
            this.variantIdx = variantIdx;
        }

        /// <summary>
        /// Validates this isometric tile against the tileset, loads the appropriate tile variant and
        /// load the data into the navigation cells.
        /// </summary>
        public void CheckAndFinalize()
        {
            if (this.parentMap.Status != MapStatus.Opening) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }
            if (this.type == null) { throw new RCEngineException(string.Format("Tile type not set for isometric tile at {0}", this.mapCoords)); }

            /// Check and finalize only if necessary.
            if (this.isReady) { return; }
                        
            /// Check against the tileset.
            this.ValidateNeighbour(MapDirection.NorthEast);
            this.ValidateNeighbour(MapDirection.SouthEast);
            this.ValidateNeighbour(MapDirection.SouthWest);
            this.ValidateNeighbour(MapDirection.NorthWest);

            /// If the variant index has not yet been set, we have to randomly generate one.
            if (this.variantIdx == -1) { this.variantIdx = RandomService.DefaultGenerator.Next(this.type.GetNumOfVariants(this)); }

            /// Load the data from the appropriate variant into the navigation cells.
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
            if (this.parentMap.Status != MapStatus.Closing) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }

            /// Cleanup only if necessary.
            if (!this.isReady) { return; }

            this.type = null;
            this.variantIdx = -1;
            this.variant = null;

            this.isReady = false;
        }

        #endregion Internal cleanup methods

        #region Internal terrain draw helper methods

        /// <summary>
        /// Exchanges the type of this isometric tile.
        /// </summary>
        /// <param name="newType">The new type.</param>
        /// <remarks>
        /// This method can only be called in edit mode during a draw terrain operation.
        /// WARNING! You have to call IsoTile.GenerateVariant method on each tiles whose type has been
        /// changed during the draw terrain operation.
        /// </remarks>
        public void ExchangeTileType(TileType newType)
        {
            if (this.parentMap.Status != MapStatus.DrawingTerrain) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }
            if (newType == null) { throw new ArgumentNullException("newType"); }

            this.type = newType;
            this.variantIdx = -1;
            this.variant = null;
        }

        /// <summary>
        /// Generates a new variant index for this isometric tile.
        /// </summary>
        public void GenerateVariant()
        {
            if (this.parentMap.Status != MapStatus.DrawingTerrain) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.parentMap.Status)); }

            /// Generate the new variant.
            this.variantIdx = RandomService.DefaultGenerator.Next(this.type.GetNumOfVariants(this));
            this.variant = this.type.GetVariant(this, this.variantIdx);

            /// Reinitialize the fields of the navigation cells with the default values.
            for (int x = 0; x < Map.QUAD_PER_ISO_VERT * Map.NAVCELL_PER_QUAD; x++)
            {
                for (int y = 0; y < Map.QUAD_PER_ISO_HORZ * Map.NAVCELL_PER_QUAD; y++)
                {
                    if (this.navCells[x, y] != null) { this.navCells[x, y].ReinitializeFields(); }
                }
            }

            /// Apply the cell data changesets of the new variant on the navigation cells.
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
        /// Gets the list of the neighbours of this isometric tile.
        /// </summary>
        public IEnumerable<IsoTile> Neighbours
        {
            get
            {
                List<IsoTile> retList = new List<IsoTile>();
                for (int i = 0; i < this.neighbours.Length; i++)
                {
                    if (this.neighbours[i] != null) { retList.Add(this.neighbours[i]);}
                }
                return retList;
            }
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
                TerrainCombination thisCombRot = RCEngineHelper.RotateTerrainCombination(this.type.Combination, dir, MapDirection.NorthEast);
                TerrainCombination neighCombRot = RCEngineHelper.RotateTerrainCombination(neighbour.type.Combination, dir, MapDirection.NorthEast);

                /// Generate the terrain-NESW array for this tile and the neighbour.
                TerrainType[] thisNESWRot = RCEngineHelper.GetTerrainNESW(this.type.TerrainA, this.type.TerrainB, thisCombRot);
                TerrainType[] neighNESWRot = RCEngineHelper.GetTerrainNESW(neighbour.type.TerrainA, neighbour.type.TerrainB, neighCombRot);

                /// Check the generated terrain-NESW arrays.
                if (thisNESWRot[0] != neighNESWRot[3] || thisNESWRot[1] != neighNESWRot[2])
                {
                    throw new RCEngineException(string.Format("Invalid neighbours at {0} and {1}!", this.mapCoords, neighbour.mapCoords));
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
                        TerrainCombination currCombRot = RCEngineHelper.RotateTerrainCombination(currTile.type.Combination, dir, MapDirection.NorthEast);
                        TerrainType[] currNESWRot = RCEngineHelper.GetTerrainNESW(currTile.type.TerrainA, currTile.type.TerrainB, currCombRot);

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
                                throw new RCEngineException(string.Format("Invalid transition from {0} in direction {1}! Length must be at least {2}!", this.mapCoords, dir, this.type.TerrainB.TransitionLength));
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
        private TileType type;

        /// <summary>
        /// The index of the variant to be selected from the tile type at the end of the initialization process
        /// or -1 if it has to be selected randomly.
        /// </summary>
        private int variantIdx;

        /// <summary>
        /// Reference to the current variant of this tile.
        /// </summary>
        private TileVariant variant;

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
        /// List of the detached neighbours and their direction.
        /// </summary>
        private List<Tuple<IsoTile, MapDirection>> detachedNeighbours;

        /// <summary>
        /// List of the detached navigation cells and their coordinates.
        /// </summary>
        private List<NavCell> detachedNavCells;

        /// <summary>
        /// List of the navigation cells of this isometric tile.
        /// </summary>
        private NavCell[,] navCells;

        /// <summary>
        /// The reference navigation cell.
        /// </summary>
        private NavCell referenceNavCell;

        /// <summary>
        /// Reference to the map that this isometric tile belongs to.
        /// </summary>
        private Map parentMap;
    }
}
