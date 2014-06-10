using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.Views
{
    /// <summary>
    /// Interface of views for handling mouse events on the map display.
    /// </summary>
    public interface IMapObjectControlView : IMapView
    {
        /// <summary>
        /// Handles a left click event on the map display.
        /// </summary>
        /// <param name="displayedArea">The area of the map currently being displayed in pixels.</param>
        /// <param name="position">The position inside the map displayed area in pixels.</param>
        void LeftClick(RCIntRectangle displayedArea, RCIntVector position);

        /// <summary>
        /// Handles a right click event on the map display.
        /// </summary>
        /// <param name="displayedArea">The area of the map currently being displayed in pixels.</param>
        /// <param name="position">The position inside the map displayed area in pixels.</param>
        void RightClick(RCIntRectangle displayedArea, RCIntVector position);

        /// <summary>
        /// Handles a double click event on the map display.
        /// </summary>
        /// <param name="displayedArea">The area of the map currently being displayed in pixels.</param>
        /// <param name="position">The position inside the map displayed area in pixels.</param>
        void DoubleClick(RCIntRectangle displayedArea, RCIntVector position);

        /// <summary>
        /// Handles a selection box event on the map display.
        /// </summary>
        /// <param name="displayedArea">The area of the map currently being displayed in pixels.</param>
        /// <param name="selectionBox">The selection box inside the map displayed area in pixels.</param>
        void SelectionBox(RCIntRectangle displayedArea, RCIntRectangle selectionBox);
    }
}
