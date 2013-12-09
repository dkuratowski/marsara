using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.PublicInterfaces
{
    /// <summary>
    /// Interface of views on data of a map object.
    /// </summary>
    public interface IMapObjectDataView
    {
        /// <summary>
        /// Starts reading the data of the map object with the given ID.
        /// </summary>
        /// <param name="objectID">The ID of the map object to be read.</param>
        /// <remarks>If no map object exists with this ID then this function has no effect.</remarks>
        void StartReadingMapObject(int objectID);

        /// <summary>
        /// Stops reading the data of the map object currently being read.
        /// If there is no map object currently being read by this view then this function has no effect.
        /// </summary>
        void StopReadingMapObject();

        /// <summary>
        /// The ID of the map object being read by this view or -1 if there is no map object being read by this view
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
