using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.UI
{
    /// <summary>
    /// Interface of a mouse pointer.
    /// </summary>
    public interface IUIMousePointer
    {
        /// <summary>
        /// Gets or sets the position of the mouse pointer.
        /// </summary>
        RCIntVector Position { get; set; }

        /// <summary>
        /// Gets the offset vector of the mouse pointer.
        /// </summary>
        RCIntVector Offset { get; }

        /// <summary>
        /// Gets the icon of the mouse pointer that has to be displayed.
        /// </summary>
        UISprite Icon { get; }

        /// <summary>
        /// Call this function to reload the underlying image data with the given pixel size.
        /// </summary>
        /// <param name="pixelSize">The pixel size.</param>
        void NewPixelSize(RCIntVector pixelSize);
    }

    /// <summary>
    /// This is a basic implementation of the IUIMousePointer interface.
    /// </summary>
    public class UIBasicPointer : IUIMousePointer
    {
        /// <summary>
        /// Constructs a UIBasicPointer.
        /// </summary>
        /// <param name="ptrSprite">The icon of the mouse pointer.</param>
        /// <param name="offset">The offset of the mouse pointer.</param>
        public UIBasicPointer(UISprite icon, RCIntVector offset)
        {
            if (icon == null) { throw new ArgumentNullException("icon"); }
            if (offset == RCIntVector.Undefined) { throw new ArgumentNullException("offset"); }

            this.icon = icon;
            this.offset = offset;
            this.position = new RCIntVector(0, 0);
        }

        #region IUIMousePointer Members

        /// <see cref="IUIMousePointer.Position"/>
        public RCIntVector Position
        {
            get { return this.position; }
            set
            {
                if (value == RCIntVector.Undefined) { throw new ArgumentNullException("Position"); }
                this.position = value;
            }
        }

        /// <see cref="IUIMousePointer.Offset"/>
        public RCIntVector Offset { get { return this.offset; } }

        /// <see cref="IUIMousePointer.Icon"/>
        public UISprite Icon { get { return this.icon; } }

        /// <see cref="IUIMousePointer.NewPixelSize"/>
        public void NewPixelSize(RCIntVector pixelSize)
        {
            UISprite scaledIcon = UIRoot.Instance.GraphicsPlatform.SpriteManager.ScaleSprite(this.icon, pixelSize);
            UIRoot.Instance.GraphicsPlatform.SpriteManager.DestroySprite(this.icon);
            this.icon = scaledIcon;
        }

        #endregion

        /// <summary>
        /// The icon of the mouse pointer.
        /// </summary>
        private UISprite icon;

        /// <summary>
        /// The offset of the mouse pointer.
        /// </summary>
        private RCIntVector offset;

        /// <summary>
        /// The position of the mouse pointer.
        /// </summary>
        private RCIntVector position;
    }
}
