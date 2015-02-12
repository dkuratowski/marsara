using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Common.ComponentModel;

namespace RC.App.BizLogic.BusinessComponents
{
    /// <summary>
    /// Interface of the business component used for performing transformations between map-, minimap- and window-coordinates.
    /// </summary>
    [ComponentInterface]
    interface IMapWindowBC
    {
        /// <summary>
        /// Attaches a window with the given pixel size.
        /// </summary>
        /// <param name="windowPixelSize">The size of the window in pixels.</param>
        /// <exception cref="InvalidOperationException">
        /// If there is no active scenario or a window has already been attached.
        /// </exception>
        void AttachWindow(RCIntVector windowPixelSize);

        /// <summary>
        /// Attaches a minimap control with the given pixel size.
        /// </summary>
        /// <param name="minimapControlPixelSize">The size of the minimap control in pixels.</param>
        /// <exception cref="InvalidOperationException">
        /// If there is no active scenario or a window has not yet been attached or a minimap control has already been attached.
        /// </exception>
        void AttachMinimap(RCIntVector minimapControlPixelSize);

        /// <summary>
        /// Scrolls the center of the attached window to the given position on the map.
        /// </summary>
        /// <param name="targetPosition">The coordinates of the target position on the map.</param>
        /// <exception cref="InvalidOperationException">If there is no active scenario.</exception>
        void ScrollTo(RCNumVector targetPosition);

        /// <summary>
        /// Gets the currently attached window or null if there is no window attached.
        /// </summary>
        IMapWindow AttachedWindow { get; }

        /// <summary>
        /// Gets the current full window or null if there is no active scenario.
        /// </summary>
        IMapWindow FullWindow { get; }

        /// <summary>
        /// Gets the currently attached minimap or null if there is no minimap attached.
        /// </summary>
        IMinimap Minimap { get; }
    }
}
