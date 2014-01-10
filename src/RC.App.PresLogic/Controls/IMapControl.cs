using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.App.PresLogic.Controls
{
    /// <summary>
    /// Interface of a map control.
    /// </summary>
    public interface IMapControl
    {
        /// <summary>
        /// Gets the displayed area of the control.
        /// </summary>
        RCIntRectangle DisplayedArea { get; }

        /// <summary>
        /// Gets or sets the selection box to be displayed. Set this property to RCIntRectangle.Undefined to turn
        /// off selection box.
        /// </summary>
        RCIntRectangle SelectionBox { get; set; }
    }
}
