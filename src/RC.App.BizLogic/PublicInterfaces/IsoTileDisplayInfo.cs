using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.App.BizLogic.PublicInterfaces
{
    /// <summary>
    /// This structure is used to provide informations for displaying an isometric tile on the map.
    /// </summary>
    public struct IsoTileDisplayInfo
    {
        /// <summary>
        /// The index of the isometric tile type to be displayed. This index is aligned with the order of isometric tile type
        /// information list returned by the appropriate ITileSetView.GetIsoTileTypes method.
        /// </summary>
        public int IsoTileTypeIndex { get; set; }

        /// <summary>
        /// Suppose that we have a minimum size bounding box around the displayed isometric tile. This vector contains the coordinates
        /// of the upper-left pixel of this bounding box in the coordinate system of the appropriate display area.
        /// </summary>
        public RCIntVector DisplayCoords { get; set; }
    }
}
