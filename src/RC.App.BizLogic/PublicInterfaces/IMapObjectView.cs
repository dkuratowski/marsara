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
        /// Gets the list of the visible map objects at the given area.
        /// </summary>
        /// <param name="displayedArea">The area of the map to be displayed in pixels.</param>
        /// <returns>The list of display informations of the visible map objects.</returns>
        List<MapSpriteInstance> GetVisibleObjects(RCIntRectangle displayedArea);

        /// <summary>
        /// Gets the list of the visible selection indicators at the given area.
        /// </summary>
        /// <param name="displayedArea">The area of the map to be displayed in pixels.</param>
        /// <returns>The list of the visible selection indicators at the given area.</returns>
        List<RCIntRectangle> GetVisibleSelectionIndicators(RCIntRectangle displayedArea);
    }
}
