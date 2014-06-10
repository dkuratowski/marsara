using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.Views
{
    /// <summary>
    /// Interface of views on data of a map object.
    /// </summary>
    public interface IMapObjectDataView
    {
        /// <summary>
        /// Gets the ID of the map object that is being read by this view.
        /// </summary>
        int ObjectID { get; }

        /// <summary>
        /// The amount of vespene gas in the map object if it is a vespene geyser; otherwise -1.
        /// </summary>
        int VespeneGasAmount { get; }

        /// <summary>
        /// The amount of minerals in the map object if it is a mineral field; otherwise -1.
        /// </summary>
        int MineralsAmount { get; }
    }
}
