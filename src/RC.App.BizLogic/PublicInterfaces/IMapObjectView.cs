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
        /// Selects the objects inside the given selection box.
        /// </summary>
        /// <param name="displayedArea">The area of the map currently being displayed in pixels.</param>
        /// <param name="selectionBox">The selection box inside the map displayed area in pixels.</param>
        void SelectObjects(RCIntRectangle displayedArea, RCIntRectangle selectionBox);

        /// <summary>
        /// Selects the object at the given point.
        /// </summary>
        /// <param name="displayedArea">The area of the map currently being displayed in pixels.</param>
        /// <param name="selectionBox">The point inside the map displayed area in pixels.</param>
        void SelectObject(RCIntRectangle displayedArea, RCIntVector selectionPoint);

        /// <summary>
        /// Orders the currently selected units to execute a command on a given point as the target of the command.
        /// </summary>
        /// <param name="displayedArea">The area of the map currently being displayed in pixels.</param>
        /// <param name="targetPoint">The target point of the command inside the map displayed area in pixels.</param>
        void SendCommand(RCIntRectangle displayedArea, RCIntVector targetPoint);
    }
}
