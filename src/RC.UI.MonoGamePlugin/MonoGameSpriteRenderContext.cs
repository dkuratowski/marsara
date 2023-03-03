using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace RC.UI.MonoGamePlugin
{
    /// <summary>
    /// This class represents the render context when rendering to a sprite.
    /// </summary>
    class MonoGameSpriteRenderContext : IUIRenderContext
    {
        /// <summary>
        /// Constructs an MonoGameSpriteRenderContext object for the given MonoGameSprite.
        /// </summary>
        /// <param name="targetSprite">The target MonoGameSprite.</param>
        public MonoGameSpriteRenderContext(MonoGameSprite targetSprite)
        {
            this.isClosed = false;
            this.targetSprite = targetSprite;
            this.targetImage = targetSprite.RawImage;
            this.targetSprite.Lock();
        }

        /// <summary>
        /// Closes this render context.
        /// </summary>
        public void Close()
        {
            if (!this.isClosed)
            {
                this.targetSprite.Unlock();

                // Enforce recreating the underlying transparent image of the target sprite.
                RCColor targetTransparentColor = this.targetSprite.TransparentColor;
                this.targetSprite.TransparentColor = RCColor.Undefined;
                this.targetSprite.TransparentColor = targetTransparentColor;

                this.targetSprite = null;
                this.targetImage = null;
                this.isClosed = true;
            }
        }

        #region IUIRenderContext members

        /// <see cref="IUIRenderContext.RenderSprite"/>
        public void RenderSprite(UISprite sprite, RCIntVector position)
        {
            this.RenderSprite(sprite, position, new RCIntRectangle(0, 0, sprite.Size.X, sprite.Size.Y));
        }

        /// <see cref="IUIRenderContext.RenderSprite"/>
        public void RenderSprite(UISprite sprite, RCIntVector position, RCIntRectangle section)
        {
            if (section == RCIntRectangle.Undefined)
            {
                this.RenderSprite(sprite, position, new RCIntRectangle(0, 0, sprite.Size.X, sprite.Size.Y));
                return;
            }

            if (this.isClosed) { throw new UIException("Render context unavailable!"); }
            if (sprite == null) { throw new ArgumentNullException("sprite"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }

            MonoGameSprite srcSprite = (MonoGameSprite)sprite;
            MonoGameImageUtils.CopyImageScaled(
                srcSprite.RawImage, this.targetImage,
                srcSprite.PixelSize, this.targetSprite.PixelSize,
                section, position,
                srcSprite.TransparentColor);
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
        private MonoGameSprite targetSprite;

        /// <summary>
        /// Reference to the target image that this render context is manipulating.
        /// </summary>
        private Image<Rgb24> targetImage;

        /// <summary>
        /// This flag indicates whether this render context is closed or not.
        /// </summary>
        private bool isClosed;
    }
}
