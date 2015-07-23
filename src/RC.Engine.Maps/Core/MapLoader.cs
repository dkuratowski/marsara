using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.Diagnostics;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Maps.ComponentInterfaces;
using RC.Engine.Maps.PublicInterfaces;
using RC.Common.Configuration;

namespace RC.Engine.Maps.Core
{
    /// <summary>
    /// Implementation of the map loader component.
    /// </summary>
    [Component("RC.Engine.Maps.MapLoader")]
    class MapLoader : IMapLoader, IComponent
    {
        /// <summary>
        /// Constructs a MapLoader object.
        /// </summary>
        public MapLoader()
        {
            this.mapStructure = null;
            this.initThread = new RCThread(this.InitThreadProc, "RC.Engine.Maps.MapLoader.InitThread");
            this.initThreadStarted = false;
        }

        #region IComponent methods

        /// <see cref="IComponent.Start"/>
        public void Start()
        {
            if (!this.initThreadStarted)
            {
                this.initThreadStarted = true;
                this.initThread.Start();
            }
        }

        /// <see cref="IComponent.Stop"/>
        public void Stop()
        {
            /// Do nothing
        }

        /// <summary>
        /// Internal method executed by the background initializer thread.
        /// </summary>
        private void InitThreadProc()
        {
            TraceManager.WriteAllTrace("RC.Engine.Maps.MapLoader initializing...", TraceFilters.INFO);

            this.mapStructure = new MapStructure();
            this.mapStructure.Initialize();

            TraceManager.WriteAllTrace("RC.Engine.Maps.MapLoader initialization finished.", TraceFilters.INFO);
        }

        #endregion IComponent methods

        #region IMapLoader methods

        /// <see cref="IMapLoader.NewMap"/>
        public IMapAccess NewMap(string mapName, ITileSet tileset, string defaultTerrain, RCIntVector size)
        {
            if (!this.initThreadStarted) { throw new InvalidOperationException("Component has not yet been started!"); }
            this.initThread.Join();

            if (mapName == null) { throw new ArgumentNullException("mapName"); }
            if (tileset == null) { throw new ArgumentNullException("tileset"); }
            if (defaultTerrain == null) { throw new ArgumentNullException("defaultTerrain"); }
            if (size == RCIntVector.Undefined) { throw new ArgumentNullException("size"); }

            MapAccess retObj = new MapAccess(mapName, this.mapStructure);
            this.mapStructure.BeginOpen(tileset, size, defaultTerrain);
            this.mapStructure.EndOpen();
            return retObj;
        }

        /// <see cref="IMapLoader.LoadMap"/>
        public IMapAccess LoadMap(ITileSet tileset, byte[] data)
        {
            if (!this.initThreadStarted) { throw new InvalidOperationException("Component has not yet been started!"); }
            this.initThread.Join();

            if (tileset == null) { throw new ArgumentNullException("tileset"); }
            if (data == null) { throw new ArgumentNullException("data"); }

            /// Load the packages from the byte array.
            RCPackage mapHeaderPackage = null;
            RCPackage isotileListPackage = null;
            RCPackage terrainObjListPackage = null;
            int offset = 0;
            while (offset < data.Length)
            {
                int parsedBytes;
                RCPackage package = RCPackage.Parse(data, offset, data.Length - offset, out parsedBytes);
                if (package == null || !package.IsCommitted) { throw new MapException("Syntax error!"); }
                offset += parsedBytes;
                if (package.PackageFormat.ID == MapFileFormat.MAP_HEADER) { mapHeaderPackage = package; }
                else if (package.PackageFormat.ID == MapFileFormat.ISOTILE_LIST) { isotileListPackage = package; }
                else if (package.PackageFormat.ID == MapFileFormat.TERRAINOBJ_LIST) { terrainObjListPackage = package; }
            }

            /// Validate the packages.
            if (mapHeaderPackage == null) { throw new MapException("Syntax error: map header is missing!"); }
            if (isotileListPackage == null) { throw new MapException("Syntax error: isometric-tile-list is missing!"); }
            if (terrainObjListPackage == null) { throw new MapException("Syntax error: terrain-object-list is missing!"); }

            /// Validate the map header.
            MapHeader mapHeader = MapHeader.FromPackage(mapHeaderPackage);
            if (mapHeader.AppVersion > new Version(ConstantsTable.Get<string>("RC.App.Version"))) { throw new MapException(string.Format("Incompatible map version: {0}!", mapHeader.AppVersion)); }
            if (mapHeader.TilesetName != tileset.Name) { throw new ArgumentException(string.Format("The given tileset '{0}' has to equal with the map tileset '{1}'!", tileset.Name, mapHeader.TilesetName), "tileset"); }
            if (mapHeader.MapSize.X > MapStructure.MAX_MAPSIZE || mapHeader.MapSize.Y > MapStructure.MAX_MAPSIZE) { throw new MapException(string.Format("Map size exceeds the limits: {0}x{0}!", MapStructure.MAX_MAPSIZE)); }

            MapAccess retObj = new MapAccess(mapHeader.MapName, this.mapStructure);
            this.mapStructure.BeginOpen(tileset, mapHeader.MapSize);
            this.LoadIsoTiles(isotileListPackage);
            this.mapStructure.EndOpen();

            this.LoadTerrainObjects(terrainObjListPackage, retObj);

            // TODO: validate MapHeader.MaxPlayers!
            // TODO: validate the MapHeader checksums!
            return retObj;
        }

        /// <see cref="IMapLoader.LoadMapHeader"/>
        public MapHeader LoadMapHeader(byte[] data)
        {
            if (!this.initThreadStarted) { throw new InvalidOperationException("Component has not yet been started!"); }
            this.initThread.Join();

            if (data == null) { throw new ArgumentNullException("data"); }

            int offset = 0;
            while (offset < data.Length)
            {
                int parsedBytes;
                RCPackage package = RCPackage.Parse(data, offset, data.Length - offset, out parsedBytes);
                if (package == null || !package.IsCommitted) { throw new MapException("Syntax error!"); }
                offset += parsedBytes;
                if (package.PackageFormat.ID == MapFileFormat.MAP_HEADER)
                {
                    return MapHeader.FromPackage(package);
                }
            }

            throw new MapException("Map header information not found!");
        }

        /// <see cref="IMapLoader.SaveMap"/>
        public byte[] SaveMap(IMapAccess map)
        {
            if (!this.initThreadStarted) { throw new InvalidOperationException("Component has not yet been started!"); }
            this.initThread.Join();

            if (map == null) { throw new ArgumentNullException("map"); }

            RCPackage mapHeader = this.CreateMapHeaderPackage(map);
            RCPackage isotileList = this.CreateIsoTileListPackage(map);
            RCPackage terrainObjList = this.CreateTerrainObjListPackage(map);

            byte[] retArray = new byte[mapHeader.PackageLength + isotileList.PackageLength + terrainObjList.PackageLength];
            int offset = 0;
            offset += mapHeader.WritePackageToBuffer(retArray, offset);
            offset += isotileList.WritePackageToBuffer(retArray, offset);
            offset += terrainObjList.WritePackageToBuffer(retArray, offset);

            return retArray;
        }

        #endregion IMapLoader methods

        #region Internal load methods

        /// <summary>
        /// Initializes the isometric tiles of the map structure.
        /// </summary>
        /// <param name="isotileListPackage">The package that contains the isometric tile informations.</param>
        private void LoadIsoTiles(RCPackage isotileListPackage)
        {
            string[] terrainIndexTable = isotileListPackage.ReadStringArray(0);
            byte[] isotileInfoBytes = isotileListPackage.ReadByteArray(1);

            int offset = 0;
            while (offset < isotileInfoBytes.Length)
            {
                int parsedBytes;
                RCPackage package = RCPackage.Parse(isotileInfoBytes, offset, isotileInfoBytes.Length - offset, out parsedBytes);
                if (package == null || !package.IsCommitted) { throw new MapException("Syntax error!"); }
                offset += parsedBytes;
                if (package.PackageFormat.ID == MapFileFormat.ISOTILE)
                {
                    RCIntVector quadCoords = new RCIntVector(package.ReadShort(0), package.ReadShort(1));
                    TerrainCombination terrainCombo = (TerrainCombination)package.ReadByte(4);
                    string terrainA = terrainIndexTable[package.ReadByte(2)];
                    string terrainB = terrainCombo != TerrainCombination.Simple ? terrainIndexTable[package.ReadByte(3)] : null;
                    int variantIdx = package.ReadByte(5);

                    this.mapStructure.InitIsoTile(quadCoords,
                                                  terrainCombo == TerrainCombination.Simple ?
                                                  this.mapStructure.Tileset.GetIsoTileType(terrainA) :
                                                  this.mapStructure.Tileset.GetIsoTileType(terrainA, terrainB, terrainCombo),
                                                  variantIdx);
                }
            }
        }
        
        /// <summary>
        /// Initializes the terrain objects of the map.
        /// </summary>
        /// <param name="terrainObjListPackage">The package that contains the terrain object informations.</param>
        /// <param name="map">Reference to the map.</param>
        private void LoadTerrainObjects(RCPackage terrainObjListPackage, IMapAccess map)
        {
            /// TODO: Avoid this downcast!
            MapAccess mapObj = map as MapAccess;
            if (mapObj == null) { throw new ArgumentException("The given map cannot be handled by the MapEditor!", "map"); }

            string[] terrainObjIndexTable = terrainObjListPackage.ReadStringArray(0);
            byte[] terrainObjInfoBytes = terrainObjListPackage.ReadByteArray(1);

            int offset = 0;
            while (offset < terrainObjInfoBytes.Length)
            {
                int parsedBytes;
                RCPackage package = RCPackage.Parse(terrainObjInfoBytes, offset, terrainObjInfoBytes.Length - offset, out parsedBytes);
                if (package == null || !package.IsCommitted) { throw new MapException("Syntax error!"); }
                offset += parsedBytes;
                if (package.PackageFormat.ID == MapFileFormat.TERRAINOBJ)
                {
                    RCIntVector quadCoords = new RCIntVector(package.ReadShort(0), package.ReadShort(1));
                    ITerrainObjectType terrainObjType = this.mapStructure.Tileset.GetTerrainObjectType(terrainObjIndexTable[package.ReadByte(2)]);

                    /// TODO: Might be better to create the TerrainObject with a factory?
                    ITerrainObject newObj = new TerrainObject(map, terrainObjType, quadCoords);
                    foreach (ICellDataChangeSet changeset in newObj.Type.CellDataChangesets)
                    {
                        changeset.Apply(newObj);
                    }
                    mapObj.AttachTerrainObject(newObj);
                }
            }

            /// Check the constraints of the terrain objects.
            List<ITerrainObject> terrainObjects = new List<ITerrainObject>(map.TerrainObjects);
            foreach (ITerrainObject terrainObj in terrainObjects)
            {
                mapObj.DetachTerrainObject(terrainObj);
                if (terrainObj.Type.CheckConstraints(map, terrainObj.MapCoords).Count != 0) { throw new MapException(string.Format("Terrain object at {0} is voilating the tileset constraints!", terrainObj.MapCoords)); }
                if (terrainObj.Type.CheckTerrainObjectIntersections(map, terrainObj.MapCoords).Count != 0) { throw new MapException(string.Format("Terrain object at {0} intersects other terrain objects!", terrainObj.MapCoords)); }
                mapObj.AttachTerrainObject(terrainObj);
            }
        }

        #endregion Internal load methods

        #region Internal save methods

        /// <summary>
        /// Creates the header package of the given map.
        /// </summary>
        /// <param name="map">Reference to the map.</param>
        /// <returns>The data package that contains the header of the given map.</returns>
        private RCPackage CreateMapHeaderPackage(IMapAccess map)
        {
            RCPackage mapHeader = RCPackage.CreateCustomDataPackage(MapFileFormat.MAP_HEADER);
            Version appVersion = new Version(ConstantsTable.Get<string>("RC.App.Version"));
            mapHeader.WriteInt(0, appVersion.Major);
            mapHeader.WriteInt(1, appVersion.Minor);
            mapHeader.WriteInt(2, appVersion.Build);
            mapHeader.WriteInt(3, appVersion.Revision);
            mapHeader.WriteString(4, map.MapName);
            mapHeader.WriteString(5, map.Tileset.Name);
            mapHeader.WriteShort(6, (short)map.Size.X);
            mapHeader.WriteShort(7, (short)map.Size.Y);
            mapHeader.WriteByte(8, (byte)8); // TODO: get the maximum number of players
            mapHeader.WriteIntArray(9, new int[0] { }); // TODO: get checksum values of the map
            return mapHeader;
        }

        /// <summary>
        /// Creates the package that contains the description of the isometric tiles of the given map.
        /// </summary>
        /// <param name="map">Reference to the map.</param>
        /// <returns>The data package that contains the description of the isometric tiles of the given map.</returns>
        private RCPackage CreateIsoTileListPackage(IMapAccess map)
        {
            RCPackage isotileList = RCPackage.CreateCustomDataPackage(MapFileFormat.ISOTILE_LIST);

            /// Create the terrain type index table.
            List<string> terrainTypeList = new List<string>();
            Dictionary<ITerrainType, int> terrainTypeIndexTable = new Dictionary<ITerrainType, int>();
            int terrainTypeIndex = 0;
            foreach (ITerrainType terrainType in map.Tileset.TerrainTypes)
            {
                terrainTypeList.Add(terrainType.Name);
                terrainTypeIndexTable.Add(terrainType, terrainTypeIndex);
                terrainTypeIndex++;
            }
            isotileList.WriteStringArray(0, terrainTypeList.ToArray());

            /// Create the packages of the isometric tiles.
            RCSet<IIsoTile> processedIsoTiles = new RCSet<IIsoTile>();
            List<RCPackage> isotilePackages = new List<RCPackage>();
            int isotileInfoLength = 0;
            for (int row = 0; row < map.Size.Y; row++)
            {
                for (int column = 0; column < map.Size.X; column++)
                {
                    IIsoTile currIsoTile = map.GetQuadTile(new RCIntVector(column, row)).PrimaryIsoTile;
                    if (!processedIsoTiles.Contains(currIsoTile))
                    {
                        RCPackage isotilePackage = RCPackage.CreateCustomDataPackage(MapFileFormat.ISOTILE);
                        isotilePackage.WriteShort(0, (short)column);
                        isotilePackage.WriteShort(1, (short)row);
                        isotilePackage.WriteByte(2, (byte)terrainTypeIndexTable[currIsoTile.Type.TerrainA]);
                        isotilePackage.WriteByte(3, currIsoTile.Type.TerrainB != null ?
                                                    (byte)terrainTypeIndexTable[currIsoTile.Type.TerrainB] :
                                                    (byte)0);
                        isotilePackage.WriteByte(4, (byte)currIsoTile.Type.Combination);
                        isotilePackage.WriteByte(5, (byte)currIsoTile.VariantIdx);

                        isotilePackages.Add(isotilePackage);
                        processedIsoTiles.Add(currIsoTile);
                        isotileInfoLength += isotilePackage.PackageLength;
                    }
                }
            }

            /// Write the isometric tile packages into the final package
            byte[] isotileInfoBytes = new byte[isotileInfoLength];
            int offset = 0;
            foreach (RCPackage isotilePackage in isotilePackages)
            {
                offset += isotilePackage.WritePackageToBuffer(isotileInfoBytes, offset);
            }

            isotileList.WriteByteArray(1, isotileInfoBytes);
            return isotileList;
        }

        /// <summary>
        /// Creates the package that contains the description of the terrain objects of the given map.
        /// </summary>
        /// <param name="map">Reference to the map.</param>
        /// <returns>The data package that contains the description of the terrain objects of the given map.</returns>
        private RCPackage CreateTerrainObjListPackage(IMapAccess map)
        {
            RCPackage terrainObjList = RCPackage.CreateCustomDataPackage(MapFileFormat.TERRAINOBJ_LIST);

            /// Create the terrain object type index table.
            List<string> terrainObjTypeList = new List<string>();
            Dictionary<ITerrainObjectType, int> terrainObjTypeIndexTable = new Dictionary<ITerrainObjectType, int>();
            int terrainObjTypeIndex = 0;
            foreach (ITerrainObjectType terrainObjType in map.Tileset.TerrainObjectTypes)
            {
                terrainObjTypeList.Add(terrainObjType.Name);
                terrainObjTypeIndexTable.Add(terrainObjType, terrainObjTypeIndex);
                terrainObjTypeIndex++;
            }
            terrainObjList.WriteStringArray(0, terrainObjTypeList.ToArray());

            /// Create the packages of the terrain objects.
            List<RCPackage> terrainObjPackages = new List<RCPackage>();
            int terrainObjInfoLength = 0;
            foreach (ITerrainObject terrainObj in map.TerrainObjects)
            {
                RCPackage terrainObjPackage = RCPackage.CreateCustomDataPackage(MapFileFormat.TERRAINOBJ);
                terrainObjPackage.WriteShort(0, (short)terrainObj.MapCoords.X);
                terrainObjPackage.WriteShort(1, (short)terrainObj.MapCoords.Y);
                terrainObjPackage.WriteByte(2, (byte)terrainObjTypeIndexTable[terrainObj.Type]);

                terrainObjPackages.Add(terrainObjPackage);
                terrainObjInfoLength += terrainObjPackage.PackageLength;
            }

            /// Write the terrain object packages into the final package
            byte[] terrainObjInfoBytes = new byte[terrainObjInfoLength];
            int offset = 0;
            foreach (RCPackage terrainObjPackage in terrainObjPackages)
            {
                offset += terrainObjPackage.WritePackageToBuffer(terrainObjInfoBytes, offset);
            }

            terrainObjList.WriteByteArray(1, terrainObjInfoBytes);
            return terrainObjList;
        }

        #endregion Internal save methods

        /// <summary>
        /// Reference to the map structure.
        /// </summary>
        private MapStructure mapStructure;

        /// <summary>
        /// Reference to the initializer thread.
        /// </summary>
        private RCThread initThread;

        /// <summary>
        /// This flag indicates whether the initializer thread has been started or not.
        /// </summary>
        private bool initThreadStarted;
    }
}
