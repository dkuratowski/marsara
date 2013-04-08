using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.Diagnostics;
using RC.Common.ComponentModel;
using RC.Engine.Maps.ComponentInterfaces;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Maps.Core
{
    /// <summary>
    /// Implementation of the tileset loader component.
    /// </summary>
    [Component("RC.Engine.Maps.TileSetLoader")]
    class TileSetLoader : ITileSetLoader
    {
        /// <summary>
        /// Constructs a TileSetLoader object.
        /// </summary>
        public TileSetLoader()
        {
        }

        #region ITileSetLoader methods

        /// <see cref="ITileSetLoader.LoadTileSet"/>
        public ITileSet LoadTileSet(byte[] data)
        {
            if (data == null) { throw new ArgumentNullException("data"); }

            int parsedBytes;
            RCPackage package = RCPackage.Parse(data, 0, data.Length, out parsedBytes);
            if (!package.IsCommitted) { throw new ArgumentException("TileSet data package is not committed!", "data"); }
            if (package.PackageType != RCPackageType.CUSTOM_DATA_PACKAGE) { throw new ArgumentException("TileSet data package must be RCPackageType.CUSTOM_DATA_PACKAGE!", "data"); }
            if (package.PackageFormat.ID != PackageFormats.TILESET_FORMAT) { throw new ArgumentException("Format of TileSet data package must be RC.Engine.Maps.TileSetFormat!", "data"); }

            TileSet tileset = XmlTileSetReader.Read(package.ReadString(0), package.ReadString(1));
            return tileset;
        }

        #endregion ITileSetLoader methods
    }
}
