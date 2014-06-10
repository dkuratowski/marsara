using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.Views
{
    /// <summary>
    /// This structure contains informations for displaying a selection indicator on the map.
    /// </summary>
    public struct SelIndicatorInst
    {
        /// <summary>
        /// The type of the selection indicator to be displayed.
        /// </summary>
        public SelIndicatorTypeEnum SelIndicatorType { get; set; }

        /// <summary>
        /// The rectangle of the selection indicator in the coordinate system of the display area.
        /// </summary>
        public RCIntRectangle IndicatorRect { get; set; }

        /// <summary>
        /// The value of the shield of the object normalized between 0 and 1.
        /// </summary>
        /// <remarks>Set this property to 0 if the shield value shall not be displayed.</remarks>
        public RCNumber ShieldNorm { get; set; }

        /// <summary>
        /// The value of the energy of the object normalized between 0 and 1.
        /// </summary>
        /// <remarks>Set this property to 0 if the energy value shall not be displayed.</remarks>
        public RCNumber EnergyNorm { get; set; }

        /// <summary>
        /// The value of the HP of the object normalized between 0 and 1.
        /// </summary>
        /// <remarks>Set this property to 0 if the HP value shall not be displayed.</remarks>
        public RCNumber HpNorm { get; set; }
    }
}
