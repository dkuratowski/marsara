using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.Diagnostics;
using RC.Common;

namespace RC.Engine
{
    /// <summary>
    /// Enumerates the possible map states.
    /// </summary>
    enum MapStatus
    {
        InitStructure = 0,      /// The map is currently initializing its data structure.
        Closed = 1,             /// The map is currently closed.
        Opening = 2,            /// The map is currently being opened.
        ReadyToEdit = 3,        /// The map has been initialized and ready to edit.
        DrawingTerrain = 4,     /// There is a draw terrain operation in progress on the map.
        ReadyToUse = 5,         /// The map has been initialized and ready to use.
        Closing = 6,            /// The map is currently being closed.
        Disposed = 7            /// The map object has been disposed.
    }

    /// <summary>
    /// Implementation of the map data structure.
    /// </summary>
    class Map : IMapEdit, IDisposable
    {
        /// <summary>
        /// Constructs a new map data structure.
        /// </summary>
        public Map()
        {
            this.status = MapStatus.InitStructure;

            this.tileset = null;
            this.size = RCIntVector.Undefined;
            this.quadTiles = new QuadTile[MAX_MAPSIZE, MAX_MAPSIZE];
            this.navCells = new NavCell[MAX_MAPSIZE * Map.NAVCELL_PER_QUAD, MAX_MAPSIZE * Map.NAVCELL_PER_QUAD];
            this.isometricTiles = new Dictionary<RCIntVector, IsoTile>();
            this.defaultTileType = null;
        }

        #region Methods at MapStatus.InitStructure

        /// <summary>
        /// Initializes the structure of this map.
        /// </summary>
        public void InitStructure()
        {
            if (this.status == MapStatus.Disposed) { throw new ObjectDisposedException("MapManager"); }
            if (this.status != MapStatus.InitStructure) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.status)); }

            /// Create the quadratic and isometric tiles.
            for (int col = 0; col < MAX_MAPSIZE; col++)
            {
                for (int row = 0; row < MAX_MAPSIZE; row++)
                {
                    RCIntVector coords = new RCIntVector(col, row);
                    RCNumVector isoCoords = Map.QuadIsoTransform.TransformAB(coords);
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

        #endregion Methods at MapStatus.InitStructure

        #region Methods at MapStatus.Closed

        /// <summary>
        /// Begins opening a new map.
        /// </summary>
        /// <param name="tileset">The tileset of the map.</param>
        /// <param name="size">The size of the map.</param>
        public void BeginOpen(TileSet tileset, RCIntVector size)
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
        public void BeginOpen(TileSet tileset, RCIntVector size, string defaultTerrain)
        {
            if (this.status == MapStatus.Disposed) { throw new ObjectDisposedException("MapManager"); }
            if (this.status != MapStatus.Closed) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.status)); }
            if (tileset == null) { throw new ArgumentNullException("tileset"); }
            if (size == RCIntVector.Undefined) { throw new ArgumentNullException("size"); }
            if (size.X % QUAD_PER_ISO_VERT != 0 || size.Y % QUAD_PER_ISO_HORZ != 0) { throw new ArgumentException(string.Format("Horizontal size of the map must be a multiple of {0}, vertical size of the map must be a multiple of {1}!", QUAD_PER_ISO_VERT, QUAD_PER_ISO_HORZ), "size"); }
            if (size.X <= 0 || size.X > MAX_MAPSIZE || size.Y <= 0 || size.Y > MAX_MAPSIZE) { throw new ArgumentOutOfRangeException("size"); }

            this.status = MapStatus.Opening;
            this.tileset = tileset;
            this.defaultTileType = defaultTerrain != null ? this.tileset.GetSimpleTileType(defaultTerrain) : null;
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
        public void InitIsoTile(RCIntVector quadCoords, TileType tileType, int variantIdx)
        {
            if (this.status == MapStatus.Disposed) { throw new ObjectDisposedException("MapManager"); }
            if (this.status != MapStatus.Opening) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.status)); }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Ends opening the new map. This method will also validate the map against the tileset and checks if everything is fine.
        /// </summary>
        /// <exception cref="RCEngineException">If the validation fails.</exception>
        public void ReadyToEdit()
        {
            if (this.status == MapStatus.Disposed) { throw new ObjectDisposedException("MapManager"); }
            if (this.status != MapStatus.Opening) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.status)); }

            for (int col = 0; col < this.size.X; col++)
            {
                for (int row = 0; row < this.size.Y; row++)
                {
                    this.quadTiles[col, row].GetIsoTile().SetDefaultTileType();
                }
            }

            for (int col = 0; col < this.size.X * Map.NAVCELL_PER_QUAD; col++)
            {
                for (int row = 0; row < this.size.Y * Map.NAVCELL_PER_QUAD; row++)
                {
                    this.navCells[col, row].InitializeFields();
                }
            }

            for (int col = 0; col < this.size.X; col++)
            {
                for (int row = 0; row < this.size.Y; row++)
                {
                    this.quadTiles[col, row].GetIsoTile().CheckAndFinalize();
                }
            }

            this.status = MapStatus.ReadyToEdit;
        }

        /// <summary>
        /// Ends opening the new map. This method will also validate the map against the tileset and checks if everything is fine.
        /// </summary>
        /// <exception cref="RCEngineException">If the validation fails.</exception>
        public void ReadyToUse()
        {
            if (this.status == MapStatus.Disposed) { throw new ObjectDisposedException("MapManager"); }
            if (this.status != MapStatus.Opening) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.status)); }

            this.status = MapStatus.ReadyToUse;
            throw new NotImplementedException();
        }

        #endregion Methods at MapStatus.Opening

        #region IDisposable implementation and Close method

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            if (this.status == MapStatus.Disposed) { throw new ObjectDisposedException("Map"); }
            if (this.status != MapStatus.Closed) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.status)); }
            TraceManager.WriteAllTrace("Destroying map", EngineTraceFilters.INFO);

            /// TODO: implement disposing procedure here!

            this.status = MapStatus.Disposed;
            TraceManager.WriteAllTrace("Map destroyed", EngineTraceFilters.INFO);
        }

        /// <summary>
        /// Closes the map. The data structure can be reused for loading another map into it.
        /// </summary>
        public void Close()
        {
            if (this.status == MapStatus.Disposed) { throw new ObjectDisposedException("Map"); }
            if (this.status != MapStatus.Opening && this.status != MapStatus.ReadyToEdit && this.status != MapStatus.ReadyToUse) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.status)); }

            this.status = MapStatus.Closing;
            TraceManager.WriteAllTrace("Closing map", EngineTraceFilters.INFO);

            for (int col = 0; col < this.size.X; col++)
            {
                for (int row = 0; row < this.size.Y; row++)
                {
                    this.quadTiles[col, row].GetIsoTile().Cleanup();
                }
            }

            for (int col = 0; col < this.size.X * Map.NAVCELL_PER_QUAD; col++)
            {
                for (int row = 0; row < this.size.Y * Map.NAVCELL_PER_QUAD; row++)
                {
                    this.navCells[col, row].UninitializeFields();
                }
            }

            this.AttachAtSize();

            this.tileset = null;
            this.defaultTileType = null;
            this.size = RCIntVector.Undefined;

            this.status = MapStatus.Closed;
            TraceManager.WriteAllTrace("Map closed", EngineTraceFilters.INFO);
        }

        #endregion IDisposable implementation and Close method

        #region IMap methods

        /// <see cref="IMap.Tileset"/>
        public TileSet Tileset
        {
            get
            {
                if (this.status == MapStatus.Disposed) { throw new ObjectDisposedException("MapManager"); }
                return this.tileset;
            }
        }

        /// <see cref="IMap.Size"/>
        public RCIntVector Size
        {
            get
            {
                if (this.status == MapStatus.Disposed) { throw new ObjectDisposedException("MapManager"); }
                return this.size;
            }
        }

        /// <see cref="IMap.NavSize"/>
        public RCIntVector NavSize
        {
            get
            {
                if (this.status == MapStatus.Disposed) { throw new ObjectDisposedException("MapManager"); }
                return this.size * NAVCELL_PER_QUAD;
            }
        }

        /// <see cref="IMap.GetQuadTile"/>
        public IQuadTile GetQuadTile(RCIntVector coords)
        {
            return this.GetQuadTileImpl(coords);
        }

        /// <see cref="IMap.GetIsoTile"/>
        public IIsoTile GetIsoTile(RCIntVector coords)
        {
            return this.GetIsoTileImpl(coords);
        }

        /// <see cref="IMap.GetNavCell"/>
        public INavCell GetNavCell(RCIntVector coords)
        {
            return this.GetNavCellImpl(coords);
        }

        #endregion IMap methods

        #region IMapEdit methods

        /// <see cref="IMapEdit.DrawTerrain"/>
        public IEnumerable<IIsoTile> DrawTerrain(IIsoTile tile, TerrainType terrain) // TODO: make this method void
        {
            if (this.status == MapStatus.Disposed) { throw new ObjectDisposedException("MapManager"); }
            if (this.status != MapStatus.ReadyToEdit) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.status)); }
            if (tile == null) { throw new ArgumentNullException("tile"); }
            if (terrain == null) { throw new ArgumentNullException("terrain"); }
            if (this.tileset != terrain.Tileset) { throw new InvalidOperationException("The tileset of the new terrain type must be the same as the tileset of the map!"); }            
            if (tile.ParentMap != this) { throw new InvalidOperationException("The given tile must be on the map!"); }

            this.status = MapStatus.DrawingTerrain;

            /// First we have to search the basis layer of the draw operation.
            TerrainType baseLayer = terrain;
            FloodArea floodArea = new FloodArea();
            while (!this.CheckLayer(tile, baseLayer, floodArea))
            {
                floodArea.Enlarge(baseLayer.TransitionLength + 1);
                baseLayer = baseLayer.Parent;
                if (baseLayer == null) { throw new MapException("Basis-layer not found for draw terrain operation!"); }
            }

            /// Clear the appropriate areas of the map around the target tile of the draw operation.
            HashSet<IsoTile> replacedTiles = new HashSet<IsoTile>();
            foreach (string layerName in this.tileset.TerrainTypes)
            {
                TerrainType topmostLayer = this.tileset.GetTerrainType(layerName);
                if (topmostLayer.IsDescendantOf(baseLayer) && topmostLayer != terrain && !topmostLayer.HasChildren)
                {
                    TerrainType[] layersToClear = terrain.FindRoute(topmostLayer);
                    this.ClearLayers(tile, terrain, baseLayer, layersToClear, replacedTiles);
                }
            }

            /// Fill the appropriate areas of the map around the target tile of the draw operation.
            this.FillLayers(tile, terrain, baseLayer, replacedTiles);

            /// Force regenerating the variant of the draw operation center and its neighbours.
            if (this.isometricTiles.ContainsKey(tile.MapCoords) && !this.isometricTiles[tile.MapCoords].IsDetached)
            {
                replacedTiles.Add(this.isometricTiles[tile.MapCoords]);
                foreach (IsoTile neighbour in this.isometricTiles[tile.MapCoords].Neighbours) { replacedTiles.Add(neighbour); }
            }

            /// Regenerate the variants for each replaced tiles.
            foreach (IsoTile replacedTile in replacedTiles)
            {
                replacedTile.GenerateVariant();
            }

            // TODO: make this method void
            this.status = MapStatus.ReadyToEdit;
            return replacedTiles;
        }

        /// <see cref="IMapEdit.CreateTerrainObject"/> TODO: place to the implementation of ITerrainObjectEdit!!!
        //public ITerrainObject CreateTerrainObject(TerrainObjectType type)
        //{
        //    if (this.status == MapStatus.Disposed) { throw new ObjectDisposedException("MapManager"); }
        //    if (this.status != MapStatus.ReadyToEdit) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.status)); }
        //    if (type == null) { throw new ArgumentNullException("type"); }
        //    if (this.tileset != type.Tileset) { throw new ArgumentException("TerrainObjectType is in another tileset!", "type"); }

        //    return new TerrainObject(this, type);
        //}

        /// <see cref="IMapEdit.Save"/>
        public void Save(string fileName)
        {
            if (this.status == MapStatus.Disposed) { throw new ObjectDisposedException("MapManager"); }
            if (this.status != MapStatus.ReadyToEdit) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.status)); }

            throw new NotImplementedException();
        }

        /// <see cref="IMapEdit.TerrainObjectEditor"/>
        public ITerrainObjectEdit TerrainObjectEditor
        {
            get
            {
                return this.terrainObjectManager;
            }
        }

        #endregion IMapEdit methods

        #region Internal public methods

        /// <summary>
        /// Gets the current status of the map.
        /// </summary>
        public MapStatus Status
        {
            get
            {
                if (this.status == MapStatus.Disposed) { throw new ObjectDisposedException("MapManager"); }
                return this.status;
            }
        }

        /// <summary>
        /// Gets the default tile type or null if no default tile type was given.
        /// </summary>
        public TileType DefaultTileType
        {
            get
            {
                if (this.status == MapStatus.Disposed) { throw new ObjectDisposedException("MapManager"); }
                return this.defaultTileType;
            }
        }

        /// <summary>
        /// The internal implementation of IMap.GetQuadTile.
        /// </summary>
        public QuadTile GetQuadTileImpl(RCIntVector coords)
        {
            if (this.status == MapStatus.Disposed) { throw new ObjectDisposedException("MapManager"); }
            if (this.status != MapStatus.ReadyToEdit && this.status != MapStatus.ReadyToUse) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.status)); }
            if (coords == RCIntVector.Undefined) { throw new ArgumentNullException("coords"); }
            if (coords.X < 0 || coords.X >= this.size.X || coords.Y < 0 || coords.Y >= this.size.Y) { throw new ArgumentOutOfRangeException("coords"); }

            return this.quadTiles[coords.X, coords.Y];
        }

        /// <summary>
        /// The internal implementation of IMap.GetIsoTile.
        /// </summary>
        public IsoTile GetIsoTileImpl(RCIntVector coords)
        {
            if (this.status == MapStatus.Disposed) { throw new ObjectDisposedException("MapManager"); }
            if (this.status != MapStatus.ReadyToEdit && this.status != MapStatus.ReadyToUse) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.status)); }
            if (coords == RCIntVector.Undefined) { throw new ArgumentNullException("coords"); }

            return this.isometricTiles.ContainsKey(coords) && !this.isometricTiles[coords].IsDetached ?
                this.isometricTiles[coords] :
                null;
        }

        /// <summary>
        /// The internal implementation of IMap.GetNavCell.
        /// </summary>
        public NavCell GetNavCellImpl(RCIntVector coords)
        {
            if (this.status == MapStatus.Disposed) { throw new ObjectDisposedException("MapManager"); }
            if (this.status != MapStatus.ReadyToEdit && this.status != MapStatus.ReadyToUse) { throw new InvalidOperationException(string.Format("Invalid operation! Map status: {0}", this.status)); }
            if (coords == RCIntVector.Undefined) { throw new ArgumentNullException("coords"); }
            if (coords.X < 0 || coords.X >= this.size.X * NAVCELL_PER_QUAD || coords.Y < 0 || coords.Y >= this.size.Y * NAVCELL_PER_QUAD) { throw new ArgumentOutOfRangeException("coords"); }

            return this.navCells[coords.X, coords.Y];
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
        /// Transformation between navigation cell and isometric coordinates.
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
        /// Number of navigation cells per quadratic tile in both horizontal and vertical direction.
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
        /// Builds up the neighbourhood relationships between the navigation cells.
        /// </summary>
        private void SetNavCellNeighbours()
        {
            foreach (QuadTile tile in this.quadTiles)
            {
                for (int x = 0; x < Map.NAVCELL_PER_QUAD; x++)
                {
                    for (int y = 0; y < Map.NAVCELL_PER_QUAD; y++)
                    {
                        NavCell currentCell = tile.GetNavCellImpl(new RCIntVector(x, y));
                        this.navCells[currentCell.MapCoords.X, currentCell.MapCoords.Y] = currentCell;
                    }
                }
            }

            foreach (NavCell cell in this.navCells)
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
                    if (neighbourCoords[i].X >= 0 && neighbourCoords[i].X < MAX_MAPSIZE * Map.NAVCELL_PER_QUAD &&
                        neighbourCoords[i].Y >= 0 && neighbourCoords[i].Y < MAX_MAPSIZE * Map.NAVCELL_PER_QUAD)
                    {
                        cell.SetNeighbour(this.navCells[neighbourCoords[i].X, neighbourCoords[i].Y], (MapDirection)i);
                    }
                }
            }
        }

        /// <summary>
        /// Builds up the relationships between the navigation cells and isometric tiles.
        /// </summary>
        private void SetNavCellIsoIndices()
        {
            foreach (NavCell cell in this.navCells)
            {
                RCNumVector isoCoords = Map.NavCellIsoTransform.TransformAB(cell.MapCoords);
                RCIntVector isoCoordsInt = isoCoords.Round();
                IsoTile isoTile = this.isometricTiles[isoCoordsInt];

                RCNumVector isoCoordsRel = isoCoords - isoTile.MapCoords;
                RCNumVector isoIndices = Map.NavCellIsoTransform.TransformBA(isoCoordsRel);
                RCIntVector isoIndicesInt = isoIndices.Round();

                cell.SetIsoTile(isoTile, isoIndicesInt);
                isoTile.SetNavCell(cell, isoIndicesInt);
            }
        }

        #endregion Internal structure buildup methods

        #region Internal attach and detach methods

        /// <summary>
        /// Detaches the part of the map structure that is out of its size.
        /// </summary>
        private void DetachAtSize()
        {
            /// Detach the navigation cells along the edge of the map.
            for (int row = 0; row < this.size.Y * NAVCELL_PER_QUAD; row++) { this.navCells[this.size.X * NAVCELL_PER_QUAD - 1, row].DetachNeighboursAtSize(); }
            for (int col = 0; col < this.size.X * NAVCELL_PER_QUAD; col++) { this.navCells[col, this.size.Y * NAVCELL_PER_QUAD - 1].DetachNeighboursAtSize(); }

            /// Detach the quadratic tiles along the edge of the map.
            for (int row = 0; row < this.size.Y; row++) { this.quadTiles[this.size.X - 1, row].DetachNeighboursAtSize(); }
            for (int col = 0; col < this.size.X; col++) { this.quadTiles[col, this.size.Y - 1].DetachNeighboursAtSize(); }

            RCNumVector outerEdges = new RCNumVector((RCNumber)(this.size.X * 2 - 1) / 2, (RCNumber)(this.size.Y * 2 - 1) / 2);
            RCNumVector innerEdges = new RCNumVector(((RCNumber)(this.size.X * 2 - 1) / 2) - Map.QUAD_PER_ISO_VERT_HALF,
                                                     ((RCNumber)(this.size.Y * 2 - 1) / 2) - Map.QUAD_PER_ISO_HORZ_HALF);

            /// Detach isometric tiles along the inner vertical edge.
            for (RCNumber innerVertical = Map.QUAD_PER_ISO_HORZ_HALF - (RCNumber)1 / (RCNumber)2;
                 innerVertical <= innerEdges.Y;
                 innerVertical += Map.QUAD_PER_ISO_HORZ)
            {
                RCNumVector quadCoords = new RCNumVector(innerEdges.X, innerVertical);
                RCIntVector isoCoords = Map.QuadIsoTransform.TransformAB(quadCoords).Round();
                IsoTile tile = this.isometricTiles[isoCoords];
                tile.DetachAtSize();
            }

            /// Detach isometric tiles along the outer vertical edge.
            for (RCNumber outerVertical = -((RCNumber)1 / (RCNumber)2);
                 outerVertical <= outerEdges.Y;
                 outerVertical += Map.QUAD_PER_ISO_HORZ)
            {
                RCNumVector quadCoords = new RCNumVector(outerEdges.X, outerVertical);
                RCIntVector isoCoords = Map.QuadIsoTransform.TransformAB(quadCoords).Round();
                IsoTile tile = this.isometricTiles[isoCoords];
                tile.DetachAtSize();
            }

            /// Detach isometric tiles along the inner horizontal edge.
            for (RCNumber innerHorizontal = Map.QUAD_PER_ISO_VERT_HALF - ((RCNumber)1 / (RCNumber)2);
                 innerHorizontal <= innerEdges.X;
                 innerHorizontal += Map.QUAD_PER_ISO_VERT)
            {
                RCNumVector quadCoords = new RCNumVector(innerHorizontal, innerEdges.Y);
                RCIntVector isoCoords = Map.QuadIsoTransform.TransformAB(quadCoords).Round();
                IsoTile tile = this.isometricTiles[isoCoords];
                tile.DetachAtSize();
            }

            /// Detach isometric tiles along the outer horizontal edge.
            for (RCNumber outerHorizontal = -((RCNumber)1 / (RCNumber)2);
                 outerHorizontal <= outerEdges.X;
                 outerHorizontal += Map.QUAD_PER_ISO_VERT)
            {
                RCNumVector quadCoords = new RCNumVector(outerHorizontal, outerEdges.Y);
                RCIntVector isoCoords = Map.QuadIsoTransform.TransformAB(quadCoords).Round();
                IsoTile tile = this.isometricTiles[isoCoords];
                tile.DetachAtSize();
            }
        }

        /// <summary>
        /// Attaches the part of the map structure that is out of its size.
        /// </summary>
        private void AttachAtSize()
        {
            /// Attach the navigation cells along the edge of the map.
            for (int row = 0; row < this.size.Y * NAVCELL_PER_QUAD; row++) { this.navCells[this.size.X * NAVCELL_PER_QUAD - 1, row].AttachNeighboursAtSize(); }
            for (int col = 0; col < this.size.X * NAVCELL_PER_QUAD; col++) { this.navCells[col, this.size.Y * NAVCELL_PER_QUAD - 1].AttachNeighboursAtSize(); }

            /// Attach the quadratic tiles along the edge of the map.
            for (int row = 0; row < this.size.Y; row++) { this.quadTiles[this.size.X - 1, row].AttachNeighboursAtSize(); }
            for (int col = 0; col < this.size.X; col++) { this.quadTiles[col, this.size.Y - 1].AttachNeighboursAtSize(); }

            RCNumVector outerEdges = new RCNumVector((RCNumber)(this.size.X * 2 - 1) / 2, (RCNumber)(this.size.Y * 2 - 1) / 2);
            RCNumVector innerEdges = new RCNumVector(((RCNumber)(this.size.X * 2 - 1) / 2) - Map.QUAD_PER_ISO_VERT_HALF,
                                                     ((RCNumber)(this.size.Y * 2 - 1) / 2) - Map.QUAD_PER_ISO_HORZ_HALF);

            /// Attach isometric tiles along the inner vertical edge.
            for (RCNumber innerVertical = Map.QUAD_PER_ISO_HORZ_HALF - (RCNumber)1 / (RCNumber)2;
                 innerVertical <= innerEdges.Y;
                 innerVertical += Map.QUAD_PER_ISO_HORZ)
            {
                RCNumVector quadCoords = new RCNumVector(innerEdges.X, innerVertical);
                RCIntVector isoCoords = Map.QuadIsoTransform.TransformAB(quadCoords).Round();
                IsoTile tile = this.isometricTiles[isoCoords];
                tile.AttachAtSize();
            }

            /// Attach isometric tiles along the outer vertical edge.
            for (RCNumber outerVertical = -((RCNumber)1 / (RCNumber)2);
                 outerVertical <= outerEdges.Y;
                 outerVertical += Map.QUAD_PER_ISO_HORZ)
            {
                RCNumVector quadCoords = new RCNumVector(outerEdges.X, outerVertical);
                RCIntVector isoCoords = Map.QuadIsoTransform.TransformAB(quadCoords).Round();
                IsoTile tile = this.isometricTiles[isoCoords];
                tile.AttachAtSize();
            }

            /// Attach isometric tiles along the inner horizontal edge.
            for (RCNumber innerHorizontal = Map.QUAD_PER_ISO_VERT_HALF - ((RCNumber)1 / (RCNumber)2);
                 innerHorizontal <= innerEdges.X;
                 innerHorizontal += Map.QUAD_PER_ISO_VERT)
            {
                RCNumVector quadCoords = new RCNumVector(innerHorizontal, innerEdges.Y);
                RCIntVector isoCoords = Map.QuadIsoTransform.TransformAB(quadCoords).Round();
                IsoTile tile = this.isometricTiles[isoCoords];
                tile.AttachAtSize();
            }

            /// Attach isometric tiles along the outer horizontal edge.
            for (RCNumber outerHorizontal = -((RCNumber)1 / (RCNumber)2);
                 outerHorizontal <= outerEdges.X;
                 outerHorizontal += Map.QUAD_PER_ISO_VERT)
            {
                RCNumVector quadCoords = new RCNumVector(outerHorizontal, outerEdges.Y);
                RCIntVector isoCoords = Map.QuadIsoTransform.TransformAB(quadCoords).Round();
                IsoTile tile = this.isometricTiles[isoCoords];
                tile.AttachAtSize();
            }
        }

        #endregion Internal attach and detach methods

        #region Helper methods for drawing terrain

        /// <summary>
        /// Checks whether the given area with the given center tile is a subset of the given layer.
        /// </summary>
        /// <param name="center">The center of the area to check.</param>
        /// <param name="layer">The terrain type of the layer to check.</param>
        /// <param name="area">The area to check.</param>
        /// <returns>True if the area is a subset of the given layer, false otherwise.</returns>
        private bool CheckLayer(IIsoTile center, TerrainType layer, FloodArea area)
        {
            foreach (FloodItem floodItem in area)
            {
                RCIntVector mapCoords = center.MapCoords + floodItem.Coordinates;
                if (this.isometricTiles.ContainsKey(mapCoords) && !this.isometricTiles[mapCoords].IsDetached)
                {
                    IsoTile checkedTile = this.isometricTiles[mapCoords];
                    if (checkedTile.Type.TerrainA.IsDescendantOf(layer) || checkedTile.Type.TerrainA == layer) { continue; }

                    if (checkedTile.Type.Combination != TerrainCombination.Simple)
                    {
                        if (checkedTile.Type.TerrainB == layer)
                        {
                            /// We have to check the combinations
                            if (((int)floodItem.Combination & (int)checkedTile.Type.Combination) != (int)floodItem.Combination)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Clears the given layers for a draw operation.
        /// </summary>
        /// <param name="center">The center of the draw operation.</param>
        /// <param name="targetTerrain">The target terrain of the draw operation.</param>
        /// <param name="baseTerrain">The base layer of the draw operation.</param>
        /// <param name="layersToClear">The route from the target terrain up to a topmost layer in the terrain tree.</param>
        /// <param name="replacedTiles">Reference to the collection where the replaced tiles are being collected.</param>
        private void ClearLayers(IIsoTile center, TerrainType targetTerrain, TerrainType baseTerrain, TerrainType[] layersToClear, HashSet<IsoTile> replacedTiles)
        {
            /// Find the biggest flood area to be cleared.
            FloodArea areaToClear = new FloodArea();
            TerrainType lastUninjuredLayer = null;
            for (int routeIdx = 0; routeIdx < layersToClear.Length; routeIdx++)
            {
                TerrainType currTerrain = layersToClear[routeIdx];
                if (lastUninjuredLayer == null)
                {
                    /// We are going downstairs.
                    TerrainType nextTerrain = layersToClear[routeIdx + 1];
                    if (nextTerrain.Parent == currTerrain)
                    {
                        /// Last uninjured layer found, from now we go upstairs.
                        lastUninjuredLayer = currTerrain;

                        /// Enlarge the clear area by 1 if there was a previous layer along the way downstairs.
                        TerrainType prevTerrain = routeIdx - 1 >= 0 ? layersToClear[routeIdx - 1] : null;
                        if (prevTerrain != null) { areaToClear.Enlarge(1); }
                    }
                    else
                    {
                        /// Enlarge the clear area by the transition length of the previous layer if there
                        /// was a previous layer along the way downstairs.
                        TerrainType prevTerrain = routeIdx - 1 >= 0 ? layersToClear[routeIdx - 1] : null;
                        if (prevTerrain != null) { areaToClear.Enlarge(prevTerrain.TransitionLength + 1); }
                    }
                }
                else
                {
                    /// We are going upstairs.
                    TerrainType prevTerrain = layersToClear[routeIdx - 1];
                    if (prevTerrain != lastUninjuredLayer) { areaToClear.Enlarge(currTerrain.TransitionLength + 1); }
                }
            }

            /// Clear the appropriate layers.
            if (lastUninjuredLayer == null) { throw new MapException("Last uninjured layer not found for draw terrain operation!"); }
            for (int routeIdx = layersToClear.Length - 1; routeIdx >= 0; routeIdx--)
            {
                TerrainType currLayer = layersToClear[routeIdx];
                if (currLayer == lastUninjuredLayer) { break; }

                /// Clear the current layer at the appropriate area.
                foreach (FloodItem floodItem in areaToClear)
                {
                    RCIntVector mapCoords = center.MapCoords + floodItem.Coordinates;
                    if (this.isometricTiles.ContainsKey(mapCoords) && !this.isometricTiles[mapCoords].IsDetached)
                    {
                        IsoTile clearedTile = this.isometricTiles[mapCoords];
                        if (clearedTile.Type.Combination != TerrainCombination.Simple)
                        {
                            /// Mixed tile.
                            if (clearedTile.Type.TerrainB.IsDescendantOf(currLayer))
                            {
                                /// Check whether TerrainB will be cleared by another branch or this is an error.
                                if (!layersToClear.Contains(clearedTile.Type.TerrainB)) { continue; }
                                else { throw new MapException("Clearing non-topmost layer is not possible!"); }                                
                            }
                            if (clearedTile.Type.TerrainB == currLayer)
                            {
                                TerrainCombination newComb = (TerrainCombination)((int)clearedTile.Type.Combination & ~(floodItem.Combination != TerrainCombination.Simple ? (int)floodItem.Combination : 0xF));
                                if (newComb != clearedTile.Type.Combination)
                                {
                                    clearedTile.ExchangeTileType(
                                        newComb == TerrainCombination.Simple ?
                                        this.tileset.GetSimpleTileType(clearedTile.Type.TerrainA.Name) :
                                        this.tileset.GetMixedTileType(clearedTile.Type.TerrainA.Name, clearedTile.Type.TerrainB.Name, newComb));

                                    replacedTiles.Add(clearedTile);
                                    foreach (IsoTile neighbour in clearedTile.Neighbours) { replacedTiles.Add(neighbour); }
                                }
                            }
                        }
                        else
                        {
                            /// Simple tile.
                            if (clearedTile.Type.TerrainA.IsDescendantOf(currLayer))
                            {
                                /// Check whether TerrainA will be cleared by another branch or this is an error.
                                if (!layersToClear.Contains(clearedTile.Type.TerrainA)) { continue; }
                                else { throw new MapException("Clearing non-topmost layer is not possible!"); }
                            }
                            if (clearedTile.Type.TerrainA == currLayer)
                            {
                                TerrainCombination newComb = (TerrainCombination)(0xF & ~(floodItem.Combination != TerrainCombination.Simple ? (int)floodItem.Combination : 0xF));
                                clearedTile.ExchangeTileType(
                                    newComb == TerrainCombination.Simple ?
                                    this.tileset.GetSimpleTileType(clearedTile.Type.TerrainA.Parent.Name) :
                                    this.tileset.GetMixedTileType(clearedTile.Type.TerrainA.Parent.Name, clearedTile.Type.TerrainA.Name, newComb));

                                replacedTiles.Add(clearedTile);
                                foreach (IsoTile neighbour in clearedTile.Neighbours) { replacedTiles.Add(neighbour); }
                            }
                        }
                    }
                }

                if (routeIdx > 1) { areaToClear.Reduce(); }
            }
        }

        /// <summary>
        /// Fills up the layers from the base layer up to the target layer.
        /// </summary>
        /// <param name="center">The center of the draw operation.</param>
        /// <param name="targetTerrain">The target terrain of the draw operation.</param>
        /// <param name="baseTerrain">The base layer of the draw operation.</param>
        /// <param name="replacedTiles">Reference to the collection where the replaced tiles are being collected.</param>
        public void FillLayers(IIsoTile center, TerrainType targetTerrain, TerrainType baseTerrain, HashSet<IsoTile> replacedTiles)
        {
            /// Find the biggest flood area to be filled.
            FloodArea areaToFill = new FloodArea();
            TerrainType[] layersToFill = targetTerrain.FindRoute(baseTerrain);
            for (int routeIdx = 0; routeIdx < layersToFill.Length; routeIdx++)
            {
                TerrainType prevTerrain = routeIdx - 1 >= 0 ? layersToFill[routeIdx - 1] : null;
                if (prevTerrain != null) { areaToFill.Enlarge(prevTerrain.TransitionLength + 1); }
            }

            /// Fill the appropriate layers
            for (int routeIdx = layersToFill.Length - 1; routeIdx >= 0; routeIdx--)
            {
                /// Fill the current layer at the appropriate area.
                TerrainType currLayer = layersToFill[routeIdx];
                foreach (FloodItem floodItem in areaToFill)
                {
                    RCIntVector mapCoords = center.MapCoords + floodItem.Coordinates;
                    if (this.isometricTiles.ContainsKey(mapCoords) && !this.isometricTiles[mapCoords].IsDetached)
                    {
                        IsoTile filledTile = this.isometricTiles[mapCoords];
                        if (filledTile.Type.Combination != TerrainCombination.Simple)
                        {
                            /// Mixed tile.
                            if (filledTile.Type.TerrainB == currLayer)
                            {
                                int newCombInt = (int)filledTile.Type.Combination | (floodItem.Combination != TerrainCombination.Simple ? (int)floodItem.Combination : 0xF);
                                TerrainCombination newComb = newCombInt != 0xF ? (TerrainCombination)newCombInt : TerrainCombination.Simple;
                                if (newComb != filledTile.Type.Combination)
                                {
                                    filledTile.ExchangeTileType(
                                        newComb == TerrainCombination.Simple ?
                                        this.tileset.GetSimpleTileType(filledTile.Type.TerrainB.Name) :
                                        this.tileset.GetMixedTileType(filledTile.Type.TerrainA.Name, filledTile.Type.TerrainB.Name, newComb));

                                    replacedTiles.Add(filledTile);
                                    foreach (IsoTile neighbour in filledTile.Neighbours) { replacedTiles.Add(neighbour); }
                                }
                            }
                            else if (currLayer.IsDescendantOf(filledTile.Type.TerrainB))
                            {
                                throw new MapException("Filling over the topmost layer is not possible!");
                            }
                        }
                        else
                        {
                            /// Simple tile.
                            if (filledTile.Type.TerrainA == currLayer.Parent)
                            {
                                filledTile.ExchangeTileType(
                                    floodItem.Combination == TerrainCombination.Simple ?
                                    this.tileset.GetSimpleTileType(currLayer.Name) :
                                    this.tileset.GetMixedTileType(filledTile.Type.TerrainA.Name, currLayer.Name, floodItem.Combination));

                                replacedTiles.Add(filledTile);
                                foreach (IsoTile neighbour in filledTile.Neighbours) { replacedTiles.Add(neighbour); }
                            }
                            else if (currLayer.IsDescendantOf(filledTile.Type.TerrainA))
                            {
                                throw new MapException("Filling over the topmost layer is not possible!");
                            }
                        }
                    }
                }

                if (routeIdx > 0) { areaToFill.Reduce(); }
            }
        }

        #endregion Helper methods for drawing terrain

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
        /// The 2D array of the navigation cells of this map.
        /// </summary>
        private NavCell[,] navCells;

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
        private TileSet tileset;

        /// <summary>
        /// The size of this map.
        /// </summary>
        private RCIntVector size;

        /// <summary>
        /// Reference to the default tile type or null if no default tile type was given.
        /// </summary>
        private TileType defaultTileType;

        /// <summary>
        /// Reference to the terrain object manager part of the map.
        /// </summary>
        private TerrainObjectManager terrainObjectManager;
    }
}
