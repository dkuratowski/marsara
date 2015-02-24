using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.App.BizLogic.BusinessComponents
{
    /// <summary>
    /// Contains informations about a pixel on the minimap.
    /// </summary>
    interface IMinimapPixel
    {
        /// <summary>
        /// Gets the coordinates of this minimap pixel.
        /// </summary>
        RCIntVector PixelCoords { get; }

        /// <summary>
        /// Gets the rectangle of the quadratic tiles covered by this pixel.
        /// </summary>
        RCIntRectangle CoveredQuadTiles { get; }

        /// <summary>
        /// Gets the area on the map covered by this pixel.
        /// </summary>
        RCNumRectangle CoveredArea { get; }
    }
}
