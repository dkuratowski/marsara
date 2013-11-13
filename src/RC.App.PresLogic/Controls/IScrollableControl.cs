﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.App.PresLogic.Controls
{
    /// <summary>
    /// Common interface of scrollable controls. A scrollable control displays a part of a bigger area.
    /// </summary>
    public interface IScrollableControl
    {
        /// <summary>
        /// Gets the displayed area of the control.
        /// </summary>
        RCIntRectangle DisplayedArea { get; }

        /// <summary>
        /// Scrolls this control to the given position.
        /// </summary>
        /// <param name="where">The top-left corner of the displayed area in pixels.</param>
        /// <remarks>The displayed area will automatically be corrected not to exceed the borders of the whole area.</remarks>
        void ScrollTo(RCIntVector where);
    }
}