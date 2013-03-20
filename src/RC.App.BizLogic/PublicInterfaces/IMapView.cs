using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.App.BizLogic.PublicInterfaces
{
    /// <summary>
    /// Interface of generic views on the currently opened map.
    /// </summary>
    public interface IMapView
    {
        /// <summary>
        /// Gets the size of the map in pixels.
        /// </summary>
        RCIntVector MapSize { get; }
    }
}
