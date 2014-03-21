using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.Configuration;

namespace RC.Engine.Maps.Core
{
    /// <summary>
    /// Package format definitions of the map files.
    /// </summary>
    static class MapFileFormat
    {
        public static readonly int MAP_HEADER = RCPackageFormatMap.Get("RC.Engine.Maps.MapHeader");
        public static readonly int ISOTILE_LIST = RCPackageFormatMap.Get("RC.Engine.Maps.IsometricTileList");
        public static readonly int TERRAINOBJ_LIST = RCPackageFormatMap.Get("RC.Engine.Maps.TerrainObjectList");
        public static readonly int ISOTILE = RCPackageFormatMap.Get("RC.Engine.Maps.IsometricTile");
        public static readonly int TERRAINOBJ = RCPackageFormatMap.Get("RC.Engine.Maps.TerrainObject");
        public static readonly int NAVMESH = RCPackageFormatMap.Get("RC.Engine.Maps.NavMesh");
        public static readonly int NAVMESH_VERTEX_LIST = RCPackageFormatMap.Get("RC.Engine.Maps.NavMeshVertexList");
        public static readonly int NAVMESH_NODE_LIST = RCPackageFormatMap.Get("RC.Engine.Maps.NavMeshNodeList");
    }
}
