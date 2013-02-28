using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.Configuration;

namespace RC.Engine.PublicInterfaces
{
    /// <summary>
    /// Internal package format definitions.
    /// </summary>
    public static class RCEngineFormats
    {
        public static readonly int TILESET_FORMAT = RCPackageFormatMap.Get("RC.Engine.TileSetFormat");
    }
}
