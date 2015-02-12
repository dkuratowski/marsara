using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.Views;
using RC.Common;
using RC.Common.ComponentModel;

namespace RC.App.BizLogic.Services
{
    /// <summary>
    /// Enumerates the possible scrolling directions of a map display.
    /// </summary>
    public enum ScrollDirectionEnum
    {
        NoScroll = -1,
        North = 0,
        NorthEast = 1,
        East = 2,
        SouthEast = 3,
        South = 4,
        SouthWest = 5,
        West = 6,
        NorthWest = 7
    }

    /// <summary>
    /// The interface definition of the scroll service.
    /// </summary>
    [ComponentInterface]
    public interface IScrollService
    {
        /// <summary>
        /// Attaches a window with the given pixel size.
        /// </summary>
        /// <param name="windowSize">The size of the window in pixels.</param>
        void AttachWindow(RCIntVector windowSize);

        /// <summary>
        /// Attaches a minimap with the given pixel size.
        /// </summary>
        /// <param name="minimapSize">The size of the minimap in pixels.</param>
        void AttachMinimap(RCIntVector minimapSize);

        /// <summary>
        /// Scrolls the map window towards the given direction.
        /// </summary>
        /// <param name="direction">The direction of the scroll.</param>
        void Scroll(ScrollDirectionEnum direction);
    }
}
