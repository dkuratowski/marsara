using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.PublicInterfaces
{
    /// <summary>
    /// This structure contains image informations about a map sprite type that can be displayed on the map.
    /// </summary>
    public struct MapSpriteType
    {
        /// <summary>
        /// This flag indicates whether the image data might belong to a player (true) or is always neutral.
        /// </summary>
        public bool HasOwner { get; set; }

        /// <summary>
        /// The byte-stream that contains the image data of this map sprite type.
        /// </summary>
        public byte[] ImageData { get; set; }

        /// <summary>
        /// The string that represents the transparent color of this map sprite type.
        /// </summary>
        public string TransparentColorStr { get; set; }

        /// <summary>
        /// The string that represents the owner mask color of this map sprite type.
        /// </summary>
        public string OwnerMaskColorStr { get; set; }
    }
}
