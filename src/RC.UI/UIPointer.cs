using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.UI
{
    /// <summary>
    /// Represents a mouse pointer resource.
    /// </summary>
    public class UIPointer
    {
        /// <summary>
        /// Constructs a UIPointer.
        /// </summary>
        /// <param name="ptrSprite">The icon of the mouse pointer.</param>
        /// <param name="offset">The offset of the mouse pointer.</param>
        public UIPointer(UISprite icon, RCIntVector offset)
        {
            if (icon == null) { throw new ArgumentNullException("icon"); }
            if (offset == RCIntVector.Undefined) { throw new ArgumentNullException("offset"); }

            this.icon = icon;
            this.offset = offset;
        }

        /// <summary>
        /// Gets the offset vector of the mouse pointer.
        /// </summary>
        public RCIntVector Offset { get { return this.offset; } }

        /// <summary>
        /// Gets the icon of the mouse pointer that has to be displayed.
        /// </summary>
        public UISprite Icon { get { return this.icon; } }

        /// <summary>
        /// The icon of the mouse pointer.
        /// </summary>
        private UISprite icon;

        /// <summary>
        /// The offset of the mouse pointer.
        /// </summary>
        private RCIntVector offset;
    }
}
