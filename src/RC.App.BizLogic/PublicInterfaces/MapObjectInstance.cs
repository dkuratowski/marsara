using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.App.BizLogic.PublicInterfaces
{
    /// <summary>
    /// This structure contains informations for displaying a map object on the map.
    /// </summary>
    public struct MapObjectInstance
    {
        /// <summary>
        /// The sprite of the object to be displayed.
        /// </summary>
        public MapSpriteInstance Sprite { get; set; }

        /// <summary>
        /// The selection indicator of the object in the coordinate system of the display area or
        /// RCIntRectangle.Undefined if no selection indicator has to be displayed.
        /// </summary>
        public RCIntRectangle SelectionIndicator { get; set; }

        /// <summary>
        /// The color of the selection indicator to be displayed or -1 if no selection indicator has to
        /// be displayed.
        /// </summary>
        public int SelectionIndicatorColorIdx { get; set; }

        /// <summary>
        /// List of the values to be displayed when the object is selected. The items in this list are
        /// pairs of integers. The first number in this pair is the index of the value indicator's color
        /// and the second number is the value itself normalized between 0 and 1.
        /// </summary>
        public List<Tuple<int, RCNumber>> Values { get; set; }
    }
}
