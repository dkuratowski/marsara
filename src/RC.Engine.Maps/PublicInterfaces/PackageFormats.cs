using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.Configuration;

namespace RC.Engine.Maps.PublicInterfaces
{
    /// <summary>
    /// Public package format definitions of the RC.Engine.Maps module.
    /// </summary>
    public static class PackageFormats
    {
        public static readonly int TILESET_FORMAT = RCPackageFormatMap.Get("RC.Engine.Maps.TileSetFormat");
    }
}
