using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.Core;

namespace RC.Engine.Maps.PublicInterfaces
{
    /// <summary>
    /// Contains header informations of a map.
    /// </summary>
    public class MapHeader
    {
        /// <summary>
        /// Creates a MapHeader structure from the given RCPackage.
        /// </summary>
        /// <param name="package">The RCPackage that contains the map header informations.</param>
        /// <returns>The created MapHeader structure.</returns>
        public static MapHeader FromPackage(RCPackage package)
        {
            if (package == null) { throw new ArgumentNullException("package"); }
            if (!package.IsCommitted) { throw new ArgumentException("The header package is not committed!", "package"); }
            if (package.PackageType != RCPackageType.CUSTOM_DATA_PACKAGE) { throw new ArgumentException("Invalid package type!", "package"); }
            if (package.PackageFormat.ID != MapFileFormat.MAP_HEADER) { throw new ArgumentException("Invalid package format!", "package"); }

            MapHeader header = new MapHeader();
            header.appVersion = new Version(package.ReadInt(0), package.ReadInt(1), package.ReadInt(2), package.ReadInt(3));
            header.mapName = package.ReadString(4);
            header.tilesetName = package.ReadString(5);
            header.mapSize = new RCIntVector(package.ReadShort(6), package.ReadShort(7));
            header.maxPlayers = package.ReadByte(8);
            header.checksumList = new List<int>(package.ReadIntArray(9));

            if (header.mapName == null) { throw new MapException("Map name information is missing!"); }
            if (header.tilesetName == null) { throw new MapException("Tileset name information is missing!"); }
            if (header.mapSize.X <= 0 || header.mapSize.Y <= 0) { throw new MapException("Map size cannot be negative or 0!"); }
            if (header.maxPlayers <= 0) { throw new MapException("Maximum number of players cannot be negative or 0!"); }
            return header;
        }

        /// <summary>
        /// Gets the version of the RC application that created the map.
        /// </summary>
        public Version AppVersion { get { return this.appVersion; } }

        /// <summary>
        /// Gets the name of the map.
        /// </summary>
        public string MapName { get { return this.mapName; } }

        /// <summary>
        /// Gets the name of the tileset of the map.
        /// </summary>
        public string TilesetName { get { return this.tilesetName; } }

        /// <summary>
        /// Gets the size of the map in quadratic tiles.
        /// </summary>
        public RCIntVector MapSize { get { return this.mapSize; } }

        /// <summary>
        /// Gets the maximum number of players of the map.
        /// </summary>
        public int MaxPlayers { get { return this.maxPlayers; } }

        /// <summary>
        /// Gets the checksum of the map at the given index.
        /// </summary>
        /// <param name="index">The index of the checksum to get.</param>
        /// <returns>The checksum of the map at the given index.</returns>
        public int GetChecksum(int index) { return this.checksumList[index]; }

        /// <summary>
        /// Private ctor.
        /// </summary>
        private MapHeader() { }

        /// <summary>
        /// The version of the RC application that created the map.
        /// </summary>
        private Version appVersion;

        /// <summary>
        /// The name of the map.
        /// </summary>
        private string mapName;

        /// <summary>
        /// The name of the tileset of the map.
        /// </summary>
        private string tilesetName;

        /// <summary>
        /// The size of the map in quadratic tiles.
        /// </summary>
        private RCIntVector mapSize;

        /// <summary>
        /// The maximum number of players of the map.
        /// </summary>
        private int maxPlayers;

        /// <summary>
        /// The list of the checksum values of the map.
        /// </summary>
        private List<int> checksumList;
    }
}
