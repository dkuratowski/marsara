using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.Diagnostics;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Maps.Core
{
    /// <summary>
    /// Represents a complex data structure behind a map.
    /// </summary>
    class MapStructure
    {
        /// <summary>
        /// Enumerates the possible states of a MapStructure object.
        /// </summary>
        public enum MapStatus
        {
            Initializing = 0,       /// The map structure is currently being initialized.
            Closed = 1,             /// There is no map opened on the map structure.
            Opening = 2,            /// A map is currently being opened on the map structure.
            Opened = 3,             /// A map has been successfully opened on the map structure and is ready to use.
            Closing = 4,            /// The map on the map structure is currently being closed.
            Finalized = 5,          /// The opened map has been finalized.
            ExchangingTiles = 6     /// There is a tile exchanging operation in progress on the map.
        }

        /// <summary>
        /// Constructs a new map data structure.
        /// </summary>
        public MapStructure()
        {
            this.status = MapStatus.Initializing;

            this.tileset = null;
            this.size = RCIntVector.Undefined;
            this.quadTiles = new QuadTile[MAX_MAPSIZE, MAX_MAPSIZE];
            this.cells = new Cell[MAX_MAPSIZE * MapStructure.NAVCELL_PER_QUAD, MAX_MAPSIZE * MapStructure.NAVCELL_PER_QUAD];
            this.isometricTiles = new Dictionary<RCIntVector, IsoTile>();
            this.tmpReplacedTiles = null;
            this.defaultTileType = null;
        }

        #region Methods at MapStatus.Initializing

        /// <summary>
        /// Initializes the structure of this map.
        /// </summary>
        public void Initialize()
        {
            if (this.status != MapStatus.Initializing) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.status)); }

            /// Create the quadratic and isometric tiles.
            for (int col = 0; col < MAX_MAPSIZE; col++)
            {
                for (int row = 0; row < MAX_MAPSIZE; row++)
                {
                    RCIntVector coords = new RCIntVector(col, row);
                    RCNumVector isoCoords = MapStructure.QuadIsoTransform.TransformAB(coords);
                    RCIntVector isoCoordsInt = isoCoords.Round();
                    if (this.isometricTiles.ContainsKey(isoCoordsInt))
                    {
                        /// Just create the quadratic tile.
                        QuadTile quadTile = new QuadTile(this, this.isometricTiles[isoCoordsInt], coords);
                        this.quadTiles[coords.X, coords.Y] = quadTile;
                    }
                    else
                    {
                        /// Create the quadratic tile and the corresponding isometric tile as well.
                        /// Variant will be selected randomly as it is a new map.
                        IsoTile isoTile = new IsoTile(this, isoCoordsInt);
                        QuadTile quadTile = new QuadTile(this, isoTile, coords);
                        this.quadTiles[coords.X, coords.Y] = quadTile;
                        this.isometricTiles.Add(isoCoordsInt, isoTile);
                    }
                }
            }

            /// Buildup the structure of the map.
            this.SetIsoNeighbours();
            this.SetQuadNeighbours();
            this.SetNavCellNeighbours();
            this.SetNavCellIsoIndices();
            this.status = MapStatus.Closed;
        }

        #endregion Methods at MapStatus.Initializing

        #region Methods at MapStatus.Closed

        /// <summary>
        /// Begins opening a new map.
        /// </summary>
        /// <param name="tileset">The tileset of the map.</param>
        /// <param name="size">The size of the map.</param>
        public void BeginOpen(ITileSet tileset, RCIntVector size)
        {
            this.BeginOpen(tileset, size, null);
        }

        /// <summary>
        /// Begins opening a new map with a given default terrain or null if the terrain will be loaded from a map-file.
        /// </summary>
        /// <param name="tileset">The tileset of the map.</param>
        /// <param name="size">The size of the map.</param>
        /// <param name="defaultTerrain">
        /// The name of the default terrain of the map or null if the terrain will be loaded from a map-file.
        /// </param>
        public void BeginOpen(ITileSet tileset, RCIntVector size, string defaultTerrain)
        {
            if (this.status != MapStatus.Closed) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.status)); }
            if (tileset == null) { throw new ArgumentNullException("tileset"); }
            if (size == RCIntVector.Undefined) { throw new ArgumentNullException("size"); }
            if (size.X % QUAD_PER_ISO_VERT != 0 || size.Y % QUAD_PER_ISO_HORZ != 0) { throw new ArgumentException(string.Format("Horizontal size of the map must be a multiple of {0}, vertical size of the map must be a multiple of {1}!", QUAD_PER_ISO_VERT, QUAD_PER_ISO_HORZ), "size"); }
            if (size.X <= 0 || size.X > MAX_MAPSIZE || size.Y <= 0 || size.Y > MAX_MAPSIZE) { throw new ArgumentOutOfRangeException("size"); }

            this.status = MapStatus.Opening;
            this.tileset = tileset;
            this.defaultTileType = defaultTerrain != null ? this.tileset.GetIsoTileType(defaultTerrain) : null;
            this.size = size;

            this.DetachAtSize();
        }

        #endregion Methods at MapStatus.Closed

        #region Methods at MapStatus.Opening

        /// <summary>
        /// Initializes the isometric tile at the given quadratic coordinates with the given tile type and variant.
        /// </summary>
        /// <param name="quadCoords">The quadratic coordinates of the isometric tile.</param>
        /// <param name="tileType">The tile type to initialize with.</param>
        /// <param name="variantIdx">The index of the tile variant to initialize with.</param>
        public void InitIsoTile(RCIntVector quadCoords, IIsoTileType tileType, int variantIdx)
        {
            if (this.status != MapStatus.Opening) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.status)); }
            if (quadCoords == RCIntVector.Undefined) { throw new ArgumentNullException("quadCoords"); }
            if (quadCoords.X < 0 || quadCoords.Y < 0 || quadCoords.X >= this.size.X || quadCoords.Y >= this.size.Y) { throw new ArgumentOutOfRangeException("quadCoords"); }

            this.quadTiles[quadCoords.X, quadCoords.Y].GetIsoTile().SetTileType(tileType, variantIdx);
        }

        /// <summary>
        /// Ends opening the new map. This method will also validate the map against the tileset and checks if everything is fine.
        /// </summary>
        /// <exception cref="RCEngineException">If the validation fails.</exception>
        public void EndOpen()
        {
            if (this.status != MapStatus.Opening) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.status)); }

            for (int col = 0; col < this.size.X; col++)
            {
                for (int row = 0; row < this.size.Y; row++)
                {
                    this.quadTiles[col, row].GetIsoTile().SetDefaultTileType();
                }
            }

            for (int col = 0; col < this.size.X * MapStructure.NAVCELL_PER_QUAD; col++)
            {
                for (int row = 0; row < this.size.Y * MapStructure.NAVCELL_PER_QUAD; row++)
                {
                    this.cells[col, row].InitializeFields();
                }
            }

            for (int col = 0; col < this.size.X; col++)
            {
                for (int row = 0; row < this.size.Y; row++)
                {
                    this.quadTiles[col, row].GetIsoTile().CheckAndFinalize();
                }
            }

            this.status = MapStatus.Opened;
        }

        #endregion Methods at MapStatus.Opening

        #region Methods at MapStatus.Opened

        /// <summary>
        /// Closes the map. The data structure can be reused for loading another map into it.
        /// </summary>
        public void Close()
        {
            if (this.status != MapStatus.Opening && this.status != MapStatus.Opened) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.status)); }

            this.status = MapStatus.Closing;
            TraceManager.WriteAllTrace("Closing map", TraceFilters.INFO);

            for (int col = 0; col < this.size.X; col++)
            {
                for (int row = 0; row < this.size.Y; row++)
                {
                    this.quadTiles[col, row].GetIsoTile().Cleanup();
                }
            }

            for (int col = 0; col < this.size.X * MapStructure.NAVCELL_PER_QUAD; col++)
            {
                for (int row = 0; row < this.size.Y * MapStructure.NAVCELL_PER_QUAD; row++)
                {
                    this.cells[col, row].UninitializeFields();
                }
            }

            this.AttachAtSize();

            this.tileset = null;
            this.defaultTileType = null;
            this.size = RCIntVector.Undefined;

            this.status = MapStatus.Closed;
            TraceManager.WriteAllTrace("Map closed", TraceFilters.INFO);
        }

        /// <summary>
        /// Begins a tile exchanging operation on the map.
        /// </summary>
        public void BeginExchangingTiles()
        {
            if (this.status != MapStatus.Opened) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.status)); }

            this.tmpReplacedTiles = new HashSet<IsoTile>();
            this.status = MapStatus.ExchangingTiles;
        }

        /// <summary>
        /// Finalize the opened map. After calling this method the map no longer can be edited.
        /// </summary>
        public void FinalizeMap()
        {
            if (this.status != MapStatus.Opened) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.status)); }
            
            /// TODO: Implement this method!
            throw new NotImplementedException();
        }

        #endregion Methods at MapStatus.Opened

        #region Methods at MapStatus.ExchangingTiles

        /// <summary>
        /// Using this method the isometric tiles notify the MapStructure object if they type has been changed during a
        /// tile exchanging operation.
        /// </summary>
        /// <param name="tile">The isometric tile whose type has been changed.</param>
        public void OnIsoTileExchanged(IsoTile tile)
        {
            if (this.status != MapStatus.ExchangingTiles) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.status)); }
            if (tile == null) { throw new ArgumentNullException("tile"); }

            this.tmpReplacedTiles.Add(tile);
        }

        /// <summary>
        /// Indicates that the tile exchanging operation is finished.
        /// </summary>
        public IEnumerable<IIsoTile> EndExchangingTiles()
        {
            if (this.status != MapStatus.ExchangingTiles) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.status)); }

            foreach (IsoTile replacedTile in this.tmpReplacedTiles)
            {
                replacedTile.ReplaceVariant();
                replacedTile.Validate();
            }

            IEnumerable<IIsoTile> retList = this.tmpReplacedTiles;
            this.tmpReplacedTiles = null;
            this.status = MapStatus.Opened;

            return retList;
        }

        #endregion Methods at MapStatus.ExchangingTiles

        #region Internal public methods

        /// <summary>
        /// Gets the current status of the map structure.
        /// </summary>
        public MapStatus Status { get { return this.status; } }

        /// <summary>
        /// Gets the default tile type or null if no default tile type was given.
        /// </summary>
        public IIsoTileType DefaultTileType { get { return this.defaultTileType; } }

        /// <summary>
        /// Gets the tileset of the opened map.
        /// </summary>
        public ITileSet Tileset { get { return this.tileset; } }

        /// <summary>
        /// Gets the size of the opened map in quadratic tiles.
        /// </summary>
        public RCIntVector Size { get { return this.size; } }

        /// <summary>
        /// Gets the size of this changeset target in cells.
        /// </summary>
        public RCIntVector CellSize { get { return this.size * NAVCELL_PER_QUAD; } }

        /// <summary>
        /// Gets the quadratic tile of the opened map at the given coordinates.
        /// </summary>
        /// <param name="coords">The quadratic coordinates of the tile to get.</param>
        /// <returns>The quadratic tile at the given coordinates.</returns>
        public QuadTile GetQuadTile(RCIntVector coords)
        {
            if (this.status != MapStatus.Opened) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.status)); }
            if (coords == RCIntVector.Undefined) { throw new ArgumentNullException("coords"); }
            if (coords.X < 0 || coords.X >= this.size.X || coords.Y < 0 || coords.Y >= this.size.Y) { throw new ArgumentOutOfRangeException("coords"); }

            return this.quadTiles[coords.X, coords.Y];
        }

        /// <summary>
        /// Gets the isometric tile of the opened map at the given coordinates.
        /// </summary>
        /// <param name="coords">The isometric coordinates of the tile to get.</param>
        /// <returns>The isometric tile at the given coordinates.</returns>
        public IsoTile GetIsoTile(RCIntVector coords)
        {
            if (this.status != MapStatus.Opened && this.status != MapStatus.ExchangingTiles) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.status)); }
            if (coords == RCIntVector.Undefined) { throw new ArgumentNullException("coords"); }

            return this.isometricTiles.ContainsKey(coords) && !this.isometricTiles[coords].IsDetached ?
                this.isometricTiles[coords] :
                null;
        }

        /// <summary>Gets the cell of this map structure at the given index.</summary>
        /// <param name="index">The index of the cell to get.</param>
        /// <returns>The cell at the given index.</returns>
        public Cell GetCell(RCIntVector index)
        {
            if (this.status != MapStatus.Opened) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.status)); }
            if (index == RCIntVector.Undefined) { throw new ArgumentNullException("index"); }
            if (index.X < 0 || index.X >= this.size.X * NAVCELL_PER_QUAD || index.Y < 0 || index.Y >= this.size.Y * NAVCELL_PER_QUAD) { throw new ArgumentOutOfRangeException("coords"); }

            return this.cells[index.X, index.Y];
        }

        /// <summary>
        /// Converts a rectangle of quadratic tiles to a rectangle of cells.
        /// </summary>
        /// <param name="quadRect">The quadratic rectangle to convert.</param>
        /// <returns>The cell rectangle.</returns>
        public RCIntRectangle QuadToCellRect(RCIntRectangle quadRect)
        {
            if (quadRect == RCIntRectangle.Undefined) { throw new ArgumentNullException("quadRect"); }
            return quadRect * new RCIntVector(MapStructure.NAVCELL_PER_QUAD, MapStructure.NAVCELL_PER_QUAD);
        }

        #endregion Internal public methods

        #region Static constants

        /// <summary>
        /// Transformation between quadratic and isometric coordinates.
        /// </summary>
        public static readonly RCCoordTransformation QuadIsoTransform =
                new RCCoordTransformation(new RCNumVector(((RCNumber)QUAD_PER_ISO_VERT - 1) / 2, ((RCNumber)QUAD_PER_ISO_HORZ - 1) / 2),
                                          new RCNumVector(QUAD_PER_ISO_VERT_HALF, QUAD_PER_ISO_HORZ_HALF),
                                          new RCNumVector(-QUAD_PER_ISO_VERT_HALF, QUAD_PER_ISO_HORZ_HALF));

        /// <summary>
        /// Transformation between cell and isometric coordinates.
        /// </summary>
        public static readonly RCCoordTransformation NavCellIsoTransform =
                new RCCoordTransformation(new RCNumVector(((RCNumber)(QUAD_PER_ISO_VERT * NAVCELL_PER_QUAD) - 1) / 2, ((RCNumber)(QUAD_PER_ISO_HORZ * NAVCELL_PER_QUAD) - 1) / 2),
                                          new RCNumVector(QUAD_PER_ISO_VERT_HALF * NAVCELL_PER_QUAD, QUAD_PER_ISO_HORZ_HALF * NAVCELL_PER_QUAD),
                                          new RCNumVector(-QUAD_PER_ISO_VERT_HALF * NAVCELL_PER_QUAD, QUAD_PER_ISO_HORZ_HALF * NAVCELL_PER_QUAD));

        /// <summary>
        /// The maximum size of a map in quadratic tiles.
        /// </summary>
        public const int MAX_MAPSIZE = 64; // TODO: change back to 256

        /// <summary>
        /// Number of quadratic tile rows intersecting horizontally the half of an isometric tile.
        /// </summary>
        public const int QUAD_PER_ISO_HORZ_HALF = 1;

        /// <summary>
        /// Number of cells per quadratic tile in both horizontal and vertical direction.
        /// </summary>
        public const int NAVCELL_PER_QUAD = 4;

        /// <summary>
        /// Number of quadratic tile columns intersecting vertically the half of an isometric tile.
        /// </summary>
        public const int QUAD_PER_ISO_VERT_HALF = QUAD_PER_ISO_HORZ_HALF * 2;

        /// <summary>
        /// Number of quadratic tile rows intersecting an isometric tile.
        /// </summary>
        public const int QUAD_PER_ISO_HORZ = QUAD_PER_ISO_HORZ_HALF * 2;

        /// <summary>
        /// Number of quadratic tile columns intersecting an isometric tile.
        /// </summary>
        public const int QUAD_PER_ISO_VERT = QUAD_PER_ISO_VERT_HALF * 2;

        #endregion Static constants

        #region Internal structure buildup methods

        /// <summary>
        /// Builds up the neighbourhood relationships between the isometric tiles.
        /// </summary>
        private void SetIsoNeighbours()
        {
            foreach (IsoTile tile in this.isometricTiles.Values)
            {
                RCIntVector[] neighbourCoords = new RCIntVector[8];
                neighbourCoords[(int)MapDirection.North] = tile.MapCoords + new RCIntVector(-1, -1);
                neighbourCoords[(int)MapDirection.NorthEast] = tile.MapCoords + new RCIntVector(0, -1);
                neighbourCoords[(int)MapDirection.East] = tile.MapCoords + new RCIntVector(1, -1);
                neighbourCoords[(int)MapDirection.SouthEast] = tile.MapCoords + new RCIntVector(1, 0);
                neighbourCoords[(int)MapDirection.South] = tile.MapCoords + new RCIntVector(1, 1);
                neighbourCoords[(int)MapDirection.SouthWest] = tile.MapCoords + new RCIntVector(0, 1);
                neighbourCoords[(int)MapDirection.West] = tile.MapCoords + new RCIntVector(-1, 1);
                neighbourCoords[(int)MapDirection.NorthWest] = tile.MapCoords + new RCIntVector(-1, 0);

                for (int i = 0; i < neighbourCoords.Length; i++)
                {
                    if (this.isometricTiles.ContainsKey(neighbourCoords[i]))
                    {
                        tile.SetNeighbour(this.isometricTiles[neighbourCoords[i]], (MapDirection)i);
                    }
                }
            }
        }

        /// <summary>
        /// Builds up the neighbourhood relationships between the quadratic tiles.
        /// </summary>
        private void SetQuadNeighbours()
        {
            foreach (QuadTile tile in this.quadTiles)
            {
                RCIntVector[] neighbourCoords = new RCIntVector[8];
                neighbourCoords[(int)MapDirection.North] = tile.MapCoords + new RCIntVector(0, -1);
                neighbourCoords[(int)MapDirection.NorthEast] = tile.MapCoords + new RCIntVector(1, -1);
                neighbourCoords[(int)MapDirection.East] = tile.MapCoords + new RCIntVector(1, 0);
                neighbourCoords[(int)MapDirection.SouthEast] = tile.MapCoords + new RCIntVector(1, 1);
                neighbourCoords[(int)MapDirection.South] = tile.MapCoords + new RCIntVector(0, 1);
                neighbourCoords[(int)MapDirection.SouthWest] = tile.MapCoords + new RCIntVector(-1, 1);
                neighbourCoords[(int)MapDirection.West] = tile.MapCoords + new RCIntVector(-1, 0);
                neighbourCoords[(int)MapDirection.NorthWest] = tile.MapCoords + new RCIntVector(-1, -1);

                for (int i = 0; i < neighbourCoords.Length; i++)
                {
                    if (neighbourCoords[i].X >= 0 && neighbourCoords[i].X < MAX_MAPSIZE &&
                        neighbourCoords[i].Y >= 0 && neighbourCoords[i].Y < MAX_MAPSIZE)
                    {
                        tile.SetNeighbour(this.quadTiles[neighbourCoords[i].X, neighbourCoords[i].Y], (MapDirection)i);
                    }
                }
            }
        }

        /// <summary>
        /// Builds up the neighbourhood relationships between the cells.
        /// </summary>
        private void SetNavCellNeighbours()
        {
            foreach (QuadTile tile in this.quadTiles)
            {
                for (int x = 0; x < MapStructure.NAVCELL_PER_QUAD; x++)
                {
                    for (int y = 0; y < MapStructure.NAVCELL_PER_QUAD; y++)
                    {
                        Cell currentCell = tile.GetCellImpl(new RCIntVector(x, y));
                        this.cells[currentCell.MapCoords.X, currentCell.MapCoords.Y] = currentCell;
                    }
                }
            }

            foreach (Cell cell in this.cells)
            {
                RCIntVector[] neighbourCoords = new RCIntVector[8];
                neighbourCoords[(int)MapDirection.North] = cell.MapCoords + new RCIntVector(0, -1);
                neighbourCoords[(int)MapDirection.NorthEast] = cell.MapCoords + new RCIntVector(1, -1);
                neighbourCoords[(int)MapDirection.East] = cell.MapCoords + new RCIntVector(1, 0);
                neighbourCoords[(int)MapDirection.SouthEast] = cell.MapCoords + new RCIntVector(1, 1);
                neighbourCoords[(int)MapDirection.South] = cell.MapCoords + new RCIntVector(0, 1);
                neighbourCoords[(int)MapDirection.SouthWest] = cell.MapCoords + new RCIntVector(-1, 1);
                neighbourCoords[(int)MapDirection.West] = cell.MapCoords + new RCIntVector(-1, 0);
                neighbourCoords[(int)MapDirection.NorthWest] = cell.MapCoords + new RCIntVector(-1, -1);

                for (int i = 0; i < neighbourCoords.Length; i++)
                {
                    if (neighbourCoords[i].X >= 0 && neighbourCoords[i].X < MAX_MAPSIZE * MapStructure.NAVCELL_PER_QUAD &&
                        neighbourCoords[i].Y >= 0 && neighbourCoords[i].Y < MAX_MAPSIZE * MapStructure.NAVCELL_PER_QUAD)
                    {
                        cell.SetNeighbour(this.cells[neighbourCoords[i].X, neighbourCoords[i].Y], (MapDirection)i);
                    }
                }
            }
        }

        /// <summary>
        /// Builds up the relationships between the cells and isometric tiles.
        /// </summary>
        private void SetNavCellIsoIndices()
        {
            foreach (Cell cell in this.cells)
            {
                RCNumVector isoCoords = MapStructure.NavCellIsoTransform.TransformAB(cell.MapCoords);
                RCIntVector isoCoordsInt = isoCoords.Round();
                IsoTile isoTile = this.isometricTiles[isoCoordsInt];

                RCNumVector isoCoordsRel = isoCoords - isoTile.MapCoords;
                RCNumVector isoIndices = MapStructure.NavCellIsoTransform.TransformBA(isoCoordsRel);
                RCIntVector isoIndicesInt = isoIndices.Round();

                cell.SetIsoTile(isoTile, isoIndicesInt);
                isoTile.SetCell(cell, isoIndicesInt);
            }
        }

        #endregion Internal structure buildup methods

        #region Internal attach and detach methods

        /// <summary>
        /// Detaches the part of the map structure that is out of its size.
        /// </summary>
        private void DetachAtSize()
        {
            /// Detach the cells along the edge of the map.
            for (int row = 0; row < this.size.Y * NAVCELL_PER_QUAD; row++) { this.cells[this.size.X * NAVCELL_PER_QUAD - 1, row].DetachNeighboursAtSize(); }
            for (int col = 0; col < this.size.X * NAVCELL_PER_QUAD; col++) { this.cells[col, this.size.Y * NAVCELL_PER_QUAD - 1].DetachNeighboursAtSize(); }

            /// Detach the quadratic tiles along the edge of the map.
            for (int row = 0; row < this.size.Y; row++) { this.quadTiles[this.size.X - 1, row].DetachNeighboursAtSize(); }
            for (int col = 0; col < this.size.X; col++) { this.quadTiles[col, this.size.Y - 1].DetachNeighboursAtSize(); }

            RCNumVector outerEdges = new RCNumVector((RCNumber)(this.size.X * 2 - 1) / 2, (RCNumber)(this.size.Y * 2 - 1) / 2);
            RCNumVector innerEdges = new RCNumVector(((RCNumber)(this.size.X * 2 - 1) / 2) - MapStructure.QUAD_PER_ISO_VERT_HALF,
                                                     ((RCNumber)(this.size.Y * 2 - 1) / 2) - MapStructure.QUAD_PER_ISO_HORZ_HALF);

            /// Detach isometric tiles along the inner vertical edge.
            for (RCNumber innerVertical = MapStructure.QUAD_PER_ISO_HORZ_HALF - (RCNumber)1 / (RCNumber)2;
                 innerVertical <= innerEdges.Y;
                 innerVertical += MapStructure.QUAD_PER_ISO_HORZ)
            {
                RCNumVector quadCoords = new RCNumVector(innerEdges.X, innerVertical);
                RCIntVector isoCoords = MapStructure.QuadIsoTransform.TransformAB(quadCoords).Round();
                IsoTile tile = this.isometricTiles[isoCoords];
                tile.DetachAtSize();
            }

            /// Detach isometric tiles along the outer vertical edge.
            for (RCNumber outerVertical = -((RCNumber)1 / (RCNumber)2);
                 outerVertical <= outerEdges.Y;
                 outerVertical += MapStructure.QUAD_PER_ISO_HORZ)
            {
                RCNumVector quadCoords = new RCNumVector(outerEdges.X, outerVertical);
                RCIntVector isoCoords = MapStructure.QuadIsoTransform.TransformAB(quadCoords).Round();
                IsoTile tile = this.isometricTiles[isoCoords];
                tile.DetachAtSize();
            }

            /// Detach isometric tiles along the inner horizontal edge.
            for (RCNumber innerHorizontal = MapStructure.QUAD_PER_ISO_VERT_HALF - ((RCNumber)1 / (RCNumber)2);
                 innerHorizontal <= innerEdges.X;
                 innerHorizontal += MapStructure.QUAD_PER_ISO_VERT)
            {
                RCNumVector quadCoords = new RCNumVector(innerHorizontal, innerEdges.Y);
                RCIntVector isoCoords = MapStructure.QuadIsoTransform.TransformAB(quadCoords).Round();
                IsoTile tile = this.isometricTiles[isoCoords];
                tile.DetachAtSize();
            }

            /// Detach isometric tiles along the outer horizontal edge.
            for (RCNumber outerHorizontal = -((RCNumber)1 / (RCNumber)2);
                 outerHorizontal <= outerEdges.X;
                 outerHorizontal += MapStructure.QUAD_PER_ISO_VERT)
            {
                RCNumVector quadCoords = new RCNumVector(outerHorizontal, outerEdges.Y);
                RCIntVector isoCoords = MapStructure.QuadIsoTransform.TransformAB(quadCoords).Round();
                IsoTile tile = this.isometricTiles[isoCoords];
                tile.DetachAtSize();
            }
        }

        /// <summary>
        /// Attaches the part of the map structure that is out of its size.
        /// </summary>
        private void AttachAtSize()
        {
            /// Attach the cells along the edge of the map.
            for (int row = 0; row < this.size.Y * NAVCELL_PER_QUAD; row++) { this.cells[this.size.X * NAVCELL_PER_QUAD - 1, row].AttachNeighboursAtSize(); }
            for (int col = 0; col < this.size.X * NAVCELL_PER_QUAD; col++) { this.cells[col, this.size.Y * NAVCELL_PER_QUAD - 1].AttachNeighboursAtSize(); }

            /// Attach the quadratic tiles along the edge of the map.
            for (int row = 0; row < this.size.Y; row++) { this.quadTiles[this.size.X - 1, row].AttachNeighboursAtSize(); }
            for (int col = 0; col < this.size.X; col++) { this.quadTiles[col, this.size.Y - 1].AttachNeighboursAtSize(); }

            RCNumVector outerEdges = new RCNumVector((RCNumber)(this.size.X * 2 - 1) / 2, (RCNumber)(this.size.Y * 2 - 1) / 2);
            RCNumVector innerEdges = new RCNumVector(((RCNumber)(this.size.X * 2 - 1) / 2) - MapStructure.QUAD_PER_ISO_VERT_HALF,
                                                     ((RCNumber)(this.size.Y * 2 - 1) / 2) - MapStructure.QUAD_PER_ISO_HORZ_HALF);

            /// Attach isometric tiles along the inner vertical edge.
            for (RCNumber innerVertical = MapStructure.QUAD_PER_ISO_HORZ_HALF - (RCNumber)1 / (RCNumber)2;
                 innerVertical <= innerEdges.Y;
                 innerVertical += MapStructure.QUAD_PER_ISO_HORZ)
            {
                RCNumVector quadCoords = new RCNumVector(innerEdges.X, innerVertical);
                RCIntVector isoCoords = MapStructure.QuadIsoTransform.TransformAB(quadCoords).Round();
                IsoTile tile = this.isometricTiles[isoCoords];
                tile.AttachAtSize();
            }

            /// Attach isometric tiles along the outer vertical edge.
            for (RCNumber outerVertical = -((RCNumber)1 / (RCNumber)2);
                 outerVertical <= outerEdges.Y;
                 outerVertical += MapStructure.QUAD_PER_ISO_HORZ)
            {
                RCNumVector quadCoords = new RCNumVector(outerEdges.X, outerVertical);
                RCIntVector isoCoords = MapStructure.QuadIsoTransform.TransformAB(quadCoords).Round();
                IsoTile tile = this.isometricTiles[isoCoords];
                tile.AttachAtSize();
            }

            /// Attach isometric tiles along the inner horizontal edge.
            for (RCNumber innerHorizontal = MapStructure.QUAD_PER_ISO_VERT_HALF - ((RCNumber)1 / (RCNumber)2);
                 innerHorizontal <= innerEdges.X;
                 innerHorizontal += MapStructure.QUAD_PER_ISO_VERT)
            {
                RCNumVector quadCoords = new RCNumVector(innerHorizontal, innerEdges.Y);
                RCIntVector isoCoords = MapStructure.QuadIsoTransform.TransformAB(quadCoords).Round();
                IsoTile tile = this.isometricTiles[isoCoords];
                tile.AttachAtSize();
            }

            /// Attach isometric tiles along the outer horizontal edge.
            for (RCNumber outerHorizontal = -((RCNumber)1 / (RCNumber)2);
                 outerHorizontal <= outerEdges.X;
                 outerHorizontal += MapStructure.QUAD_PER_ISO_VERT)
            {
                RCNumVector quadCoords = new RCNumVector(outerHorizontal, outerEdges.Y);
                RCIntVector isoCoords = MapStructure.QuadIsoTransform.TransformAB(quadCoords).Round();
                IsoTile tile = this.isometricTiles[isoCoords];
                tile.AttachAtSize();
            }
        }

        #endregion Internal attach and detach methods

        /// TODO: only for debugging!
        public IEnumerable<IIsoTile> IsometricTiles
        {
            get
            {
                List<IIsoTile> retList = new List<IIsoTile>();
                foreach (IsoTile tile in this.isometricTiles.Values)
                {
                    if (tile.Type != null)
                    {
                        retList.Add(tile);
                    }
                }
                return retList;
            }
        }

        /// <summary>
        /// The 2D array of the quadratic tiles of this map.
        /// </summary>
        private QuadTile[,] quadTiles;

        /// <summary>
        /// The 2D array of the cells of this map.
        /// </summary>
        private Cell[,] cells;

        /// <summary>
        /// List of the isometric tiles mapped by their isometric coordinates.
        /// </summary>
        private Dictionary<RCIntVector, IsoTile> isometricTiles;

        /// <summary>
        /// The current status of the map.
        /// </summary>
        private MapStatus status;

        /// <summary>
        /// Reference to the tileset of this map.
        /// </summary>
        private ITileSet tileset;

        /// <summary>
        /// The size of this map.
        /// </summary>
        private RCIntVector size;

        /// <summary>
        /// Reference to the default tile type or null if no default tile type was given.
        /// </summary>
        private IIsoTileType defaultTileType;

        /// <summary>
        /// Temporary list of isometric tiles whose type has been exchanged during a tile exchanging operation.
        /// </summary>
        private HashSet<IsoTile> tmpReplacedTiles;
    }
}
