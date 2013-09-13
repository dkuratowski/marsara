using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.App.BizLogic.PublicInterfaces
{
    /// <summary>
    /// Interface of views on informations for debugging on the currently opened map.
    /// </summary>
    public interface IMapDebugView : IMapView
    {
        /// <summary>
        /// Gets the list of the visible pathfinder tree nodes at the given area.
        /// </summary>
        /// <param name="displayedArea">The area of the map currently being displayed in pixels.</param>
        /// <returns>The list of visible pathfinder tree nodes.</returns>
        List<RCIntRectangle> GetVisiblePathfinderTreeNodes(RCIntRectangle displayedArea);
    }
}
