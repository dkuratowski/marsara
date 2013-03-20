using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.App.BizLogic.PublicInterfaces
{
    /// <summary>
    /// Interface of views on the terrain of the currently opened map.
    /// </summary>
    public interface IMapTerrainView : IMapView
    {
        /// <summary>
        /// Gets the list of the visible isometric tiles at the given area.
        /// </summary>
        /// <param name="displayedArea">The area of the map to be displayed in pixels.</param>
        /// <returns>The list of display informations of the visible isometric tiles.</returns>
        List<IsoTileDisplayInfo> GetVisibleIsoTiles(RCIntRectangle displayedArea);

        /// <summary>
        /// Gets the display coordinates of the isometric tile at the given position inside the given displayed area.
        /// The term "display coordinates" means the same as in the description of the IsoTileDisplayInfo.DisplayCoords
        /// property.
        /// </summary>
        /// <param name="displayedArea">The area of the map to be displayed in pixels.</param>
        /// <param name="position">The position inside the map displayed are in pixels.</param>
        /// <returns>The display coordinates of the isometric tile at the given position.</returns>
        RCIntVector GetIsoTileDisplayCoords(RCIntRectangle displayedArea, RCIntVector position);
    }
}
