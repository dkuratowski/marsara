using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Common.ComponentModel;

namespace RC.App.BizLogic.BusinessComponents
{
    /// <summary>
    /// Interface of the business component used for performing transformations between map-, window- and minimap-coordinates.
    /// </summary>
    [ComponentInterface]
    interface IMapWindowBC
    {
        /// <summary>
        /// Attaches a window with the given pixel size.
        /// </summary>
        /// <param name="windowPixelSize">The size of the window in pixels.</param>
        /// <exception cref="InvalidOperationException">If there is no active scenario.</exception>
        void AttachWindow(RCIntVector windowPixelSize);

        /// <summary>
        /// Gets the currently attached window or null if there is no window attached.
        /// </summary>
        IMapWindow AttachedWindow { get; }

        /// <summary>
        /// Gets the current full window or null if there is no active scenario.
        /// </summary>
        IMapWindow FullWindow { get; }
    }
}
