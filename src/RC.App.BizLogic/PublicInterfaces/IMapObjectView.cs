using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.App.BizLogic.PublicInterfaces
{
    /// <summary>
    /// Interface of views on the objects of the map of the currently running game.
    /// </summary>
    public interface IMapObjectView : IMapView
    {
        /// <summary>
        /// Gets the list of the visible map objects at the given area in the order as they shall be displayed.
        /// </summary>
        /// <param name="displayedArea">The area of the map currently being displayed in pixels.</param>
        /// <returns>The list of display informations of the visible map objects.</returns>
        List<MapObjectInstance> GetVisibleMapObjects(RCIntRectangle displayedArea);

        /// <summary>
        /// Gets the display coordinates of the map object at the given position inside the given displayed area.
        /// The term "display coordinates" means the same as in the description of the MapSpriteInstance.DisplayCoords
        /// property.
        /// </summary>
        /// <param name="displayedArea">The area of the map to be displayed in pixels.</param>
        /// <param name="position">The position inside the map displayed area in pixels.</param>
        /// <returns>
        /// The display coordinates of the map object at the given position or RCIntVector.Undefined if there is no
        /// map object at that position.
        /// </returns>
        RCIntVector GetMapObjectDisplayCoords(RCIntRectangle displayedArea, RCIntVector position);

        /// <summary>
        /// Gets the ID of the map object at the given position inside the given displayed area.
        /// The term "display coordinates" means the same as in the description of the MapSpriteInstance.DisplayCoords
        /// property.
        /// </summary>
        /// <param name="displayedArea">The area of the map to be displayed in pixels.</param>
        /// <param name="position">The position inside the map displayed area in pixels.</param>
        /// <returns>
        /// The ID of the map object at the given position or -1 if there is no map object at that position.
        /// </returns>
        int GetMapObjectID(RCIntRectangle displayedArea, RCIntVector position);
    }
}
