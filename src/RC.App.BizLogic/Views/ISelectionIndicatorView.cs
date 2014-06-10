using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.Views
{
    /// <summary>
    /// Interface of views on the selection indicators of selected objects of the currently running game.
    /// </summary>
    public interface ISelectionIndicatorView : IMapView
    {
        /// <summary>
        /// Gets the list of the visible selection indicators at the given area in the order as they shall be displayed.
        /// </summary>
        /// <param name="displayedArea">The area of the map currently being displayed in pixels.</param>
        /// <returns>The list of display informations of the visible selection indicators.</returns>
        List<SelIndicatorInst> GetVisibleSelIndicators(RCIntRectangle displayedArea);
    }
}
