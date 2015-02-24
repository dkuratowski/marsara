using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.Views;
using RC.Common;

namespace RC.App.BizLogic.BusinessComponents
{
    /// <summary>
    /// The interface of a minimap.
    /// </summary>
    interface IMinimap
    {
        /// <summary>
        /// Gets the informations about the given minimap pixel.
        /// </summary>
        /// <param name="minimapPixel">The coordinates of the minimap pixel on the minimap image.</param>
        /// <returns>The informations about the given minimap pixel.</returns>
        IMinimapPixel GetMinimapPixel(RCIntVector minimapPixel);

        /// <summary>
        /// Gets the minimap pixel that covers the given quadratic tile.
        /// </summary>
        /// <param name="quadTile">The coordinates of the quadratic tile.</param>
        /// <returns>The minimap pixel that covers the given quadratic tile.</returns>
        IMinimapPixel GetMinimapPixelAtQuadTile(RCIntVector quadTile);

        /// <summary>
        /// Gets the location of the indicator of the current map window in minimap control coordinates.
        /// </summary>
        RCIntRectangle WindowIndicator { get; }

        /// <summary>
        /// The position of the minimap in minimap control coordinates.
        /// </summary>
        RCIntRectangle MinimapPosition { get; }
    }
}
