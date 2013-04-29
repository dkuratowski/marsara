using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using System.Drawing;
using System.Drawing.Imaging;

namespace RC.UI.XnaPlugin
{
    /// <summary>
    /// This class represents the render context when rendering to a sprite.
    /// </summary>
    class XnaSpriteRenderContext : IUIRenderContext
    {
        /// <summary>
        /// Constructs an XnaSpriteRenderContext object for the given XnaSprite.
        /// </summary>
        /// <param name="targetSprite">The target XnaSprite.</param>
        public XnaSpriteRenderContext(XnaSprite targetSprite, XnaSpriteManager spriteManager)
        {
            this.isClosed = false;
            this.targetSprite = targetSprite;
            this.spriteManager = spriteManager;
            this.targetBmp = this.targetSprite.RawBitmap;
            this.targetGC = Graphics.FromImage(this.targetBmp);
            this.targetTraspColor = this.targetSprite.TransparentColor;
            this.targetSprite.Lock();
        }

        /// <summary>
        /// Closes this render context.
        /// </summary>
        public void Close()
        {
            if (!this.isClosed)
            {
                this.targetGC.Dispose(); this.targetGC = null;
                this.targetBmp = null;
                this.targetSprite.Unlock();
                this.targetSprite.TransparentColor = UIColor.Undefined;
                this.targetSprite.TransparentColor = this.targetTraspColor;
                this.targetSprite = null;
                this.targetTraspColor = UIColor.Undefined;
                this.spriteManager = null;
                this.isClosed = true;
            }
        }

        #region IUIRenderContext members

        /// <see cref="IUIRenderContext.RenderSprite"/>
        public void RenderSprite(UISprite sprite, RCIntVector position)
        {
            if (this.isClosed) { throw new UIException("Render context unavailable!"); }
            if (sprite == null) { throw new ArgumentNullException("sprite"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }

            UISprite source = sprite.PixelSize == this.targetSprite.PixelSize
                            ? sprite
                            : this.spriteManager.ScaleSprite(sprite, this.targetSprite.PixelSize);

            XnaSprite srcSprite = (XnaSprite)source;
            Bitmap srcBitmap = srcSprite.TransparentBitmap == null ? srcSprite.RawBitmap : srcSprite.TransparentBitmap;
            this.targetGC.DrawImageUnscaled(srcBitmap,
                                            position.X * this.targetSprite.PixelSize.X,
                                            position.Y * this.targetSprite.PixelSize.Y);

            if (source != sprite) { this.spriteManager.DestroySprite(source); }
        }

        /// <see cref="IUIRenderContext.RenderSprite"/>
        public void RenderSprite(UISprite sprite, RCIntVector position, RCIntRectangle section)
        {
            if (this.isClosed) { throw new UIException("Render context unavailable!"); }
            if (sprite == null) { throw new ArgumentNullException("sprite"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }
            if (section == RCIntRectangle.Undefined) { throw new ArgumentNullException("section"); }

            XnaSprite srcSprite = (XnaSprite)sprite;
            Bitmap sectionBmp = new Bitmap(section.Width * this.targetSprite.PixelSize.X,
                                           section.Height * this.targetSprite.PixelSize.Y,
                                           PixelFormat.Format24bppRgb);
            XnaBitmapUtils.CopyBitmapScaled(srcSprite.RawBitmap, sectionBmp,
                                            sprite.PixelSize, this.targetSprite.PixelSize,
                                            section, new RCIntVector(0, 0));
            if (srcSprite.TransparentColor != UIColor.Undefined)
            {
                sectionBmp.MakeTransparent(Color.FromArgb(srcSprite.TransparentColor.R,
                                                          srcSprite.TransparentColor.G,
                                                          srcSprite.TransparentColor.B));
            }
            this.targetGC.DrawImageUnscaled(sectionBmp,
                                            position.X * this.targetSprite.PixelSize.X,
                                            position.Y * this.targetSprite.PixelSize.Y);
            sectionBmp.Dispose();
        }

        /// <see cref="IUIRenderContext.RenderString"/>
        public void RenderString(UIString str, RCIntVector position)
        {
            if (this.isClosed) { throw new UIException("Render context unavailable!"); }

            throw new NotImplementedException(); // TODO: implement this method
        }

        /// <see cref="IUIRenderContext.RenderString"/>
        public void RenderString(UIString str, RCIntVector position, int width)
        {
            if (this.isClosed) { throw new UIException("Render context unavailable!"); }

            throw new NotImplementedException(); // TODO: implement this method
        }

        /// <see cref="IUIRenderContext.RenderString"/>
        public void RenderString(UIString str, RCIntVector position, RCIntVector textboxSize, UIStringAlignment alignment)
        {
            if (this.isClosed) { throw new UIException("Render context unavailable!"); }

            throw new NotImplementedException(); // TODO: implement this method
        }

        /// <see cref="IUIRenderContext.RenderRectangle"/>
        public void RenderRectangle(UISprite brush, RCIntRectangle rect)
        {
            if (this.isClosed) { throw new UIException("Render context unavailable!"); }

            throw new NotImplementedException(); // TODO: implement this method
        }

        /// <see cref="IUIRenderContext.Clip"/>
        public RCIntRectangle Clip
        {
            get
            {
                if (this.isClosed) { throw new UIException("Render context unavailable!"); }
                return new RCIntRectangle(0, 0, this.targetSprite.Size.X, this.targetSprite.Size.Y);
            }

            set
            {
                /// TODO: implement this setter in the future if necessary.
                throw new NotImplementedException();
            }
        }

        #endregion IUIRenderContext members

        /// <summary>
        /// Reference to the target sprite of this render context.
        /// </summary>
        private XnaSprite targetSprite;

        /// <summary>
        /// The target bitmap to draw.
        /// </summary>
        private Bitmap targetBmp;

        /// <summary>
        /// Reference to the target GDI context.
        /// </summary>
        private Graphics targetGC;

        /// <summary>
        /// The transparent color of the target sprite.
        /// </summary>
        private UIColor targetTraspColor;

        /// <summary>
        /// Reference to the sprite manager that created this render context.
        /// </summary>
        private XnaSpriteManager spriteManager;

        /// <summary>
        /// This flag indicates whether this render context is closed or not.
        /// </summary>
        private bool isClosed;
    }
}
