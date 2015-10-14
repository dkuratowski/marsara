using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.App.BizLogic.Views
{
    /// <summary>
    /// This structure contains informations for rendering a sprite on the UI.
    /// </summary>
    public struct SpriteRenderInfo
    {
        /// <summary>
        /// The sprite group of the sprite to be rendered.
        /// </summary>
        public SpriteGroupEnum SpriteGroup { get; set; }

        /// <summary>
        /// The index of the sprite in the sprite group to be rendered.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The coordinates of the upper-left pixel of the rendered part of the sprite in the coordinate system of
        /// the display area.
        /// </summary>
        public RCIntVector DisplayCoords { get; set; }

        /// <summary>
        /// The section of the sprite to render in the coordinate-system of the sprite or RCIntRectangle.Undefined if the
        /// whole sprite has to be rendered.
        /// </summary>
        public RCIntRectangle Section { get; set; }
    }
}
