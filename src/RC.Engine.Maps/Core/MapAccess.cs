using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Maps.PublicInterfaces;
using RC.Common;
using RC.Common.Configuration;

namespace RC.Engine.Maps.Core
{
    /// <summary>
    /// Represents the main access point of a map.
    /// </summary>
    class MapAccess : IMapAccess
    {
        /// <summary>
        /// Constructs a MapAccess instance.
        /// </summary>
        /// <param name="mapStructure">Reference to the used map structure.</param>
        public MapAccess(string mapName, MapStructure mapStructure)
        {
            if (mapName == null) { throw new ArgumentNullException("mapName"); }
            if (mapStructure == null) { throw new ArgumentNullException("mapStructure"); }
            if (mapStructure.Status != MapStructure.MapStatus.Closed) { throw new InvalidOperationException("A map is already opened with this MapStructure!"); }

            this.mapName = mapName;
            this.mapStructure = mapStructure;
            this.terrainObjects = new RCSet<ITerrainObject>();
        }

        #region IMapAccess methods

        /// <see cref="IMapAccess.MapName"/>
        public string MapName { get { return this.mapName; } }

        /// <see cref="IMapAccess.Size"/>
        public RCIntVector Size { get { return this.mapStructure.Size; } }

        /// <see cref="IMapAccess.CellSize"/>
        public RCIntVector CellSize { get { return this.mapStructure.CellSize; } }

        /// <see cref="IMapAccess.Tileset"/>
        public ITileSet Tileset { get { return this.mapStructure.Tileset; } }

        /// <see cref="IMapAccess.IsFinalized"/>
        public bool IsFinalized { get { return this.mapStructure.Status == MapStructure.MapStatus.Finalized; } }

        /// <see cref="IMapAccess.GetQuadTile"/>
        public IQuadTile GetQuadTile(RCIntVector coords) { return this.mapStructure.GetQuadTile(coords); }

        /// <see cref="IMapAccess.GetIsoTile"/>
        public IIsoTile GetIsoTile(RCIntVector coords) { return this.mapStructure.GetIsoTile(coords); }

        /// <see cref="IMapAccess.GetCell"/>
        public ICell GetCell(RCIntVector coords) { return this.mapStructure.GetCell(coords); }

        /// <see cref="IMapAccess.QuadToCellRect"/>
        public RCIntRectangle QuadToCellRect(RCIntRectangle quadRect) { return this.mapStructure.QuadToCellRect(quadRect); }

        /// <see cref="IMapAccess.CellToQuadRect"/>
        public RCIntRectangle CellToQuadRect(RCIntRectangle cellRect) { return this.mapStructure.CellToQuadRect(cellRect); }

        /// <see cref="IMapAccess.CellToQuadSize"/>
        public RCIntVector CellToQuadSize(RCNumVector cellSize) { return this.mapStructure.CellToQuadSize(cellSize); }

        /// <see cref="IMapAccess.BeginExchangingTiles"/>
        public void BeginExchangingTiles() { this.mapStructure.BeginExchangingTiles(); }

        /// <see cref="IMapAccess.EndExchangingTiles"/>
        public IEnumerable<IIsoTile> EndExchangingTiles() { return this.mapStructure.EndExchangingTiles(); }

        /// <see cref="IMapAccess.FinalizeMap"/>
        public void FinalizeMap() { this.mapStructure.FinalizeMap(); }

        /// <see cref="IMapAccess.Close"/>
        public void Close() { this.mapStructure.Close(); }

        /// TODO: only for debugging!
        public IEnumerable<IIsoTile> IsometricTiles { get { return this.mapStructure.IsometricTiles; } }

        /// <see cref="IMapAccess.TerrainObjects"/>
        public IEnumerable<ITerrainObject> TerrainObjects { get { return this.terrainObjects; } }

        #endregion IMapAccess methods

        /// <summary>
        /// Attaches the given terrain object to the map structure.
        /// </summary>
        /// <param name="terrainObj">The terrain object to be attached.</param>
        /// <exception cref="InvalidOperationException">If the given terrain object has already been attached to this map.</exception>
        public void AttachTerrainObject(ITerrainObject terrainObj)
        {
            if (!this.terrainObjects.Add(terrainObj)) { throw new InvalidOperationException("The given terrain object has already been attached to this map!"); }
            for (int x = 0; x < terrainObj.Type.QuadraticSize.X; x++)
            {
                for (int y = 0; y < terrainObj.Type.QuadraticSize.Y; y++)
                {
                    IQuadTile quadTile = terrainObj.GetQuadTile(new RCIntVector(x, y));
                    if (quadTile != null)
                    {
                        QuadTile quadTileObj = this.mapStructure.GetQuadTile(quadTile.MapCoords);
                        quadTileObj.AttachTerrainObject(terrainObj);
                    }
                }
            }
        }

        /// <summary>
        /// Detaches the given terrain object from the map structure.
        /// </summary>
        /// <param name="terrainObj">The terrain object to be detached.</param>
        /// <exception cref="InvalidOperationException">If the given terrain object was not attached to this map.</exception>
        public void DetachTerrainObject(ITerrainObject terrainObj)
        {
            if (!this.terrainObjects.Remove(terrainObj)) { throw new InvalidOperationException("The given terrain object was not attached to this map!"); }
            for (int x = 0; x < terrainObj.Type.QuadraticSize.X; x++)
            {
                for (int y = 0; y < terrainObj.Type.QuadraticSize.Y; y++)
                {
                    IQuadTile quadTile = terrainObj.GetQuadTile(new RCIntVector(x, y));
                    if (quadTile != null)
                    {
                        QuadTile quadTileObj = this.mapStructure.GetQuadTile(quadTile.MapCoords);
                        quadTileObj.DetachTerrainObject();
                    }
                }
            }
        }

        /// <summary>
        /// The name of the map.
        /// </summary>
        private string mapName;

        /// <summary>
        /// Reference to the used map structure.
        /// </summary>
        private MapStructure mapStructure;

        /// <summary>
        /// The list of the terrain objects attached to this map.
        /// </summary>
        private RCSet<ITerrainObject> terrainObjects;
    }
}
