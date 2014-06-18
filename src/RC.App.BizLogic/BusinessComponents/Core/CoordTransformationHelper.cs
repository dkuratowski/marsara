using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// Helper class for calculating coordinate transformations.
    /// </summary>
    static class CoordTransformationHelper
    {
        /// <summary>
        /// Calculates the rectangle of visible cells on the map.
        /// </summary>
        /// <param name="displayedArea">The display area in pixels.</param>
        /// <param name="cellWindow">The calculated cell rectangle.</param>
        /// <param name="displayOffset">
        /// The difference between the top-left corner of the displayed area and the top-left corner of the
        /// top-left visible cell.
        /// </param>
        public static void CalculateCellWindow(RCIntRectangle displayedArea, out RCIntRectangle cellWindow, out RCIntVector displayOffset)
        {
            cellWindow = new RCIntRectangle(displayedArea.X / BizLogicConstants.PIXEL_PER_NAVCELL,
                                            displayedArea.Y / BizLogicConstants.PIXEL_PER_NAVCELL,
                                            (displayedArea.Right - 1) / BizLogicConstants.PIXEL_PER_NAVCELL - displayedArea.X / BizLogicConstants.PIXEL_PER_NAVCELL + 1,
                                            (displayedArea.Bottom - 1) / BizLogicConstants.PIXEL_PER_NAVCELL - displayedArea.Y / BizLogicConstants.PIXEL_PER_NAVCELL + 1);
            displayOffset = new RCIntVector(displayedArea.X % BizLogicConstants.PIXEL_PER_NAVCELL, displayedArea.Y % BizLogicConstants.PIXEL_PER_NAVCELL);
        }

        /// <summary>
        /// Constants for coordinate transformations.
        /// </summary>
        public static readonly RCIntVector PIXEL_PER_NAVCELL_VECT = new RCIntVector(BizLogicConstants.PIXEL_PER_NAVCELL, BizLogicConstants.PIXEL_PER_NAVCELL);
        public static readonly RCNumVector HALF_VECT = new RCNumVector(1, 1) / 2;
    }
}
