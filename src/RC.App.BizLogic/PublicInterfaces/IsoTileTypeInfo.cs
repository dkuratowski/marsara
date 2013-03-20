using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.PublicInterfaces
{
    /// <summary>
    /// This structure is used to provide informations that are necessary to display an isometric tile type.
    /// </summary>
    public struct IsoTileTypeInfo
    {
        /// <summary>
        /// The byte-stream that contains the image data of this isometric tile type.
        /// </summary>
        public byte[] ImageData { get; set; }

        /// <summary>
        /// The string that represents the transparent color of this isometric tile type.
        /// </summary>
        public string TransparentColorStr { get; set; }
    }
}
