using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.App.BizLogic.Views
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
        List<SpriteInst> GetVisibleIsoTiles(RCIntRectangle displayedArea);

        /// <summary>
        /// Gets the display coordinates of the isometric tile at the given position inside the given displayed area.
        /// The term "display coordinates" means the same as in the description of the SpriteInst.DisplayCoords
        /// property.
        /// </summary>
        /// <param name="displayedArea">The area of the map to be displayed in pixels.</param>
        /// <param name="position">The position inside the map displayed area in pixels.</param>
        /// <returns>The display coordinates of the isometric tile at the given position.</returns>
        RCIntVector GetIsoTileDisplayCoords(RCIntRectangle displayedArea, RCIntVector position);

        /// <summary>
        /// Gets the list of the visible terrain objects at the given area.
        /// </summary>
        /// <param name="displayedArea">The area of the map to be displayed in pixels.</param>
        /// <returns>The list of display informations of the visible terrain objects.</returns>
        List<SpriteInst> GetVisibleTerrainObjects(RCIntRectangle displayedArea);

        /// <summary>
        /// Gets the display coordinates of the terrain object at the given position inside the given displayed area.
        /// The term "display coordinates" means the same as in the description of the SpriteInst.DisplayCoords
        /// property.
        /// </summary>
        /// <param name="displayedArea">The area of the map to be displayed in pixels.</param>
        /// <param name="position">The position inside the map displayed area in pixels.</param>
        /// <returns>
        /// The display coordinates of the terrain object at the given position or RCIntVector.Undefined if there is no
        /// terrain object at that position.
        /// </returns>
        RCIntVector GetTerrainObjectDisplayCoords(RCIntRectangle displayedArea, RCIntVector position);

        /// <summary>
        /// Gets the list of the walkable cells at the area of the map currently being displayed.
        /// </summary>
        /// <param name="displayedArea">The area of the map to be displayed in pixels.</param>
        /// <returns>The list of the walkable cells.</returns>
        List<RCIntRectangle> GetWalkableCells(RCIntRectangle displayedArea);
    }
}
