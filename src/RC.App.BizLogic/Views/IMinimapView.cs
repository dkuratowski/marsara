using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.App.BizLogic.Views
{
    /// <summary>
    /// Interface definition of the minimap views.
    /// </summary>
    public interface IMinimapView
    {
        /// <summary>
        /// Gets the list of the sprites to render the terrain of the full map.
        /// </summary>
        /// <returns>The list of render informations of the sprites.</returns>
        List<SpriteRenderInfo> GetTerrainSprites();

        /// <summary>
        /// Refreshes the minimap pixel informations in the given rows.
        /// </summary>
        /// <param name="firstRowIndex">The index of the first scanned row.</param>
        /// <param name="rowsCount">The numbers of rows to scan.</param>
        /// <param name="pixelInfos">The array of the pixel informations to refresh.</param>
        void RefreshPixelInfos(int firstRowIndex, int rowsCount, MinimapPixelInfo[,] pixelInfos);

        /// <summary>
        /// Gets the location of the indicator of the current map window on the minimap display.
        /// </summary>
        RCIntRectangle WindowIndicator { get; }

        /// <summary>
        /// The position of the minimap inside the minimap control.
        /// </summary>
        RCIntRectangle MinimapPosition { get; }

        /// <summary>
        /// Gets the size of the full map in pixels.
        /// </summary>
        RCIntVector MapPixelSize { get; }
    }
}
