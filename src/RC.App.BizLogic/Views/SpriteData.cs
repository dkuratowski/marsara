using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.App.BizLogic.Views
{
    /// <summary>
    /// This structure contains sprite image data that can be displayed on the UI.
    /// </summary>
    public struct SpriteData
    {
        /// <summary>
        /// The byte-stream that contains the image data of this sprite.
        /// </summary>
        public byte[] ImageData { get; set; }

        /// <summary>
        /// The transparent color of this sprite.
        /// </summary>
        public RCColor TransparentColor { get; set; }

        /// <summary>
        /// The mask color of this sprite.
        /// </summary>
        public RCColor MaskColor { get; set; }

        /// <summary>
        /// This flag indicates whether the sprite can be masked or not.
        /// </summary>
        public bool IsMaskableSprite { get; set; }
    }
}
