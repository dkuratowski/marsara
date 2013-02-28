using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.Diagnostics;
using RC.Common.ComponentModel;
using RC.Engine.ComponentInterfaces;
using RC.Common;
using RC.Engine.PublicInterfaces;

namespace RC.Engine.Core
{
    /// <summary>
    /// Implementation of the tileset loader component.
    /// </summary>
    [Component("RC.Engine.TileSetLoader")]
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
        public ITileSet LoadTileSet(RCPackage data)
        {
            if (data == null) { throw new ArgumentNullException("data"); }
            if (!data.IsCommitted) { throw new ArgumentException("TileSet data package is not committed!", "data"); }
            if (data.PackageType != RCPackageType.CUSTOM_DATA_PACKAGE) { throw new ArgumentException("TileSet data package must be RCPackageType.CUSTOM_DATA_PACKAGE!", "data"); }
            if (data.PackageFormat.ID != RCEngineFormats.TILESET_FORMAT) { throw new ArgumentException("Format of TileSet data package must be RC.Engine.TileSetFormat!", "data"); }

            TileSet tileset = XmlTileSetReader.Read(data.ReadString(0), data.ReadString(1));
            return tileset;
        }

        #endregion ITileSetLoader methods
    }
}
