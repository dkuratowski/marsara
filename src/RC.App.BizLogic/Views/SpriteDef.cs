using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.Views
{
    /// <summary>
    /// This structure contains image informations about a sprite that can be displayed on the UI.
    /// </summary>
    public struct SpriteDef
    {
        /// <summary>
        /// This flag indicates whether the image data can be masked or not.
        /// </summary>
        public bool IsMaskableSprite { get; set; }

        /// <summary>
        /// The byte-stream that contains the image data of this sprite definition.
        /// </summary>
        public byte[] ImageData { get; set; }

        /// <summary>
        /// The string that represents the transparent color of this sprite definition.
        /// </summary>
        public string TransparentColorStr { get; set; }

        /// <summary>
        /// The string that represents the mask color of this sprite definition.
        /// </summary>
        public string MaskColorStr { get; set; }
    }
}
