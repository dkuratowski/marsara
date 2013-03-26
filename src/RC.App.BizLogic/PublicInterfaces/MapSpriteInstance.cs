using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.App.BizLogic.PublicInterfaces
{
    /// <summary>
    /// This structure contains informations for displaying an instance of a map sprite type on the map.
    /// </summary>
    public struct MapSpriteInstance
    {
        /// <summary>
        /// The index of the map sprite instance to be displayed.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Suppose that we have a minimum size bounding box around the displayed isometric tile. This vector contains the coordinates
        /// of the upper-left pixel of this bounding box in the coordinate system of the appropriate display area.
        /// </summary>
        public RCIntVector DisplayCoords { get; set; }
    }
}
