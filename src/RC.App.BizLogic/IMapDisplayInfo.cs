using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.ComponentModel;
using RC.Common;

namespace RC.App.BizLogic
{
    /// <summary>
    /// This component interface provides informations for displaying a rectangular area of the currently opened map.
    /// The area to be displayed is designated by a window with a resolution aligned with the navigation cells.
    /// </summary>
    [ComponentInterface]
    public interface IMapDisplayInfo
    {
        /// <summary>
        /// Gets or sets the window that designates the displayed area of the map.
        /// </summary>
        RCIntRectangle Window { get; set; }

        /// <summary>
        /// Gets an enumerable list of data structures that provide informations for displaying the currently visible
        /// isometric tiles of the map.
        /// </summary>
        IEnumerable<IsoTileDisplayInfo> IsoTileDisplayInfos { get; }

        /// <summary>
        /// Gets the display coordinates of the isometric tile at the given position. The term "display coordinates"
        /// means the same as in the description of the IsoTileDisplayInfo.DisplayCoords property.
        /// </summary>
        /// <param name="position">The position inside the map display window in navigation cells.</param>
        /// <returns>The display coordinates of the isometric tile at the given position.</returns>
        RCIntVector GetIsoTileDisplayCoords(RCIntVector position);
    }

    /// <summary>
    /// This structure is used to provide informations for displaying an isometric tile.
    /// </summary>
    public struct IsoTileDisplayInfo
    {
        /// <summary>
        /// The index of the tile type to be displayed. This index is aligned with the order of tile type
        /// information list returned by the ITileSetStore.GetTileTypes method.
        /// </summary>
        public int TileTypeIndex { get; set; }

        /// <summary>
        /// Suppose that we have a minimum size bounding box around the isometric tile with the resolution
        /// of navigation cells. This vector is the coordinates of the upper-left corner of this bounding
        /// box in the coordinate-system of the map display window.
        /// </summary>
        public RCIntVector DisplayCoords { get; set; }
    }
}
