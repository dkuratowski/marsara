using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.App.BizLogic.BusinessComponents
{
    /// <summary>
    /// The interface of a minimap.
    /// </summary>
    interface IMinimap
    {
        /// <summary>
        /// Gets the location of the indicator of the current map window in minimap coordinates.
        /// </summary>
        RCIntRectangle WindowIndicator { get; }

        /// <summary>
        /// The position of the minimap inside the minimap control.
        /// </summary>
        RCIntRectangle MinimapPosition { get; }
    }
}
