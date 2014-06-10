using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.App.BizLogic.Views
{
    /// <summary>
    /// This structure contains informations for displaying an object placement box on the map.
    /// </summary>
    public struct ObjectPlacementBox
    {
        /// <summary>
        /// The sprites of the object to be displayed.
        /// </summary>
        public List<SpriteInst> Sprites { get; set; }

        /// <summary>
        /// The illegal parts of the object placement box.
        /// </summary>
        public List<RCIntRectangle> IllegalParts { get; set; }

        /// <summary>
        /// The legal parts of the object placement box.
        /// </summary>
        public List<RCIntRectangle> LegalParts { get; set; }
    }
}
