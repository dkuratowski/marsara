using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.App.BizLogic.PublicInterfaces
{
    /// <summary>
    /// This structure contains informations for displaying an object on the map.
    /// </summary>
    public struct ObjectInst
    {
        /// <summary>
        /// The sprites instances of the object to be displayed.
        /// </summary>
        public List<SpriteInst> Sprites { get; set; }

        /// <summary>
        /// The player that owns the object to be displayed or Player.Neutral if this is a neutral object.
        /// </summary>
        public PlayerEnum Owner { get; set; }
    }
}
