using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.Configuration;

namespace RC.App.BizLogic.Core
{
    /// <summary>
    /// This static class is used to access the constants of the RC.App.BizLogic module.
    /// </summary>
    static class BizLogicConstants
    {
        /// <summary>
        /// The directory of the tilesets.
        /// </summary>
        public static readonly string TILESET_DIR = ConstantsTable.Get<string>("RC.App.BizLogic.TileSetDir");

        /// <summary>
        /// Name of the tile variant property that stores the transparent color.
        /// </summary>
        public const string TILEPROP_TRANSPARENTCOLOR = "TransparentColor";
    }
}
