using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using Microsoft.Xna.Framework.Graphics;
using RC.Common.Diagnostics;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace RC.UI.MonoGamePlugin
{
    /// <summary>
    /// The MonoGame implementation of the sprite manager.
    /// </summary>
    class MonoGameSpriteManager : UISpriteManagerBase
    {
        /// <summary>
        /// Constructs an MonoGameSpriteManager object.
        /// </summary>
        /// <param name="platform">Reference to the platform.</param>
        public MonoGameSpriteManager(MonoGameGraphicsPlatform platform)
        {
            this.platform = platform;
            this.sprites = new RCSet<MonoGameSprite>();
            this.renderContexts = new Dictionary<MonoGameSprite, MonoGameSpriteRenderContext>();
        }

        #region UISpriteManagerBase overrides

        /// <see cref="IUISpriteManager.CreateSprite"/>
        public override UISprite CreateSprite(RCColor color, RCIntVector spriteSize)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("MonoGameSpriteManager"); }
            if (color == RCColor.Undefined) { throw new ArgumentNullException("color"); }
            if (spriteSize == RCIntVector.Undefined) { throw new ArgumentNullException("spriteSize"); }

            /// Create the empty image and fill with the given color
            Image<Rgb24> emptyImage = new Image<Rgb24>(spriteSize.X, spriteSize.Y, new Rgb24(color.R, color.G, color.B));

            lock (this.lockObj)
            {
                /// Create the MonoGameSprite object and register it to this sprite manager.
                MonoGameSprite newSprite = new MonoGameSprite(emptyImage, new RCIntVector(1, 1), this.platform);
                this.sprites.Add(newSprite);

                TraceManager.WriteAllTrace("MonoGameSpriteManager.CreateSprite: Sprite created", MonoGameTraceFilters.INFO);
                return newSprite;
            }
        }

        /// <see cref="IUISpriteManager.CreateSprite"/>
        public override UISprite CreateSprite(RCColor color, RCIntVector spriteSize, RCIntVector pixelSize)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("MonoGameSpriteManager"); }
            if (color == RCColor.Undefined) { throw new ArgumentNullException("color"); }
            if (spriteSize == RCIntVector.Undefined) { throw new ArgumentNullException("spriteSize"); }
            if (pixelSize == RCIntVector.Undefined) { throw new ArgumentNullException("pixelSize"); }

            /// Create the empty image and fill with the given color
            Image<Rgb24> emptyImage = new Image<Rgb24>(
                spriteSize.X * pixelSize.X,
                spriteSize.Y * pixelSize.Y,
                new Rgb24(color.R, color.G, color.B)
            );

            lock (this.lockObj)
            {
                /// Create the MonoGameSprite object and register it to this sprite manager.
                MonoGameSprite newSprite = new MonoGameSprite(emptyImage, pixelSize, this.platform);
                this.sprites.Add(newSprite);

                TraceManager.WriteAllTrace("MonoGameSpriteManager.CreateSprite: Sprite created", MonoGameTraceFilters.INFO);
                return newSprite;
            }
        }

        /// <see cref="UISpriteManagerBase.LoadSprite"/>
        public override UISprite LoadSprite(string fileName)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("MonoGameSpriteManager"); }
            if (null == fileName) { throw new ArgumentNullException("fileName"); }

            /// Load the image from the given file.
            Image<Rgb24> loadedImage = Image.Load<Rgb24>(fileName);

            lock (this.lockObj)
            {
                /// Create the MonoGameSprite object and register it to this sprite manager.
                MonoGameSprite newSprite = new MonoGameSprite(loadedImage, new RCIntVector(1, 1), this.platform);
                this.sprites.Add(newSprite);

                TraceManager.WriteAllTrace("MonoGameSpriteManager.LoadSprite: Sprite created", MonoGameTraceFilters.INFO);
                return newSprite;
            }
        }

        /// <see cref="UISpriteManagerBase.LoadSprite"/>
        public override UISprite LoadSprite(string fileName, RCIntVector pixelSize)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("MonoGameSpriteManager"); }
            if (null == fileName) { throw new ArgumentNullException("fileName"); }
            if (pixelSize == RCIntVector.Undefined) { throw new ArgumentNullException("pixelSize"); }
            if (pixelSize.X <= 0 || pixelSize.Y <= 0) { throw new ArgumentOutOfRangeException("pixelSize"); }

            /// Load the image from the given file.
            Image<Rgb24> loadedImage = Image.Load<Rgb24>(fileName);

            /// Create a new image and copy the original to this new with the given pixel size.
            Image<Rgb24> scaledImage = new Image<Rgb24>(
                loadedImage.Size().Width * pixelSize.X,
                loadedImage.Size().Height * pixelSize.Y,
                new Rgb24(0, 0, 0)
            );
            MonoGameImageUtils.CopyImageScaled(loadedImage, scaledImage, new RCIntVector(1, 1), pixelSize);
            loadedImage.Dispose();

            lock (this.lockObj)
            {
                /// Create the MonoGameSprite object and register it to this sprite manager.
                MonoGameSprite newSprite = new MonoGameSprite(scaledImage, pixelSize, this.platform);
                this.sprites.Add(newSprite);

                TraceManager.WriteAllTrace("MonoGameSpriteManager.LoadSprite: Sprite created", MonoGameTraceFilters.INFO);
                return newSprite;
            }
        }

        /// <see cref="UISpriteManagerBase.LoadSprite"/>
        public override UISprite LoadSprite(byte[] imageData)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("MonoGameSpriteManager"); }
            if (null == imageData) { throw new ArgumentNullException("imageData"); }

            /// Load the image from the given byte array.
            Image<Rgb24> loadedImage = Image.Load<Rgb24>(imageData);

            lock (this.lockObj)
            {
                /// Create the MonoGameSprite object and register it to this sprite manager.
                MonoGameSprite newSprite = new MonoGameSprite(loadedImage, new RCIntVector(1, 1), this.platform);
                this.sprites.Add(newSprite);

                TraceManager.WriteAllTrace("MonoGameSpriteManager.LoadSprite: Sprite created", MonoGameTraceFilters.INFO);
                return newSprite;
            }
        }

        /// <see cref="UISpriteManagerBase.LoadSprite"/>
        public override UISprite LoadSprite(byte[] imageData, RCIntVector pixelSize)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("MonoGameSpriteManager"); }
            if (null == imageData) { throw new ArgumentNullException("imageData"); }
            if (pixelSize == RCIntVector.Undefined) { throw new ArgumentNullException("pixelSize"); }
            if (pixelSize.X <= 0 || pixelSize.Y <= 0) { throw new ArgumentOutOfRangeException("pixelSize"); }

            /// Load the image from the given byte array.
            Image<Rgb24> loadedImage = Image.Load<Rgb24>(imageData);

            /// Create a new image and copy the original to this new with the given pixel size.
            Image<Rgb24> scaledImage = new Image<Rgb24>(
                loadedImage.Size().Width * pixelSize.X,
                loadedImage.Size().Height * pixelSize.Y,
                new Rgb24(0, 0, 0)
            );
            MonoGameImageUtils.CopyImageScaled(loadedImage, scaledImage, new RCIntVector(1, 1), pixelSize);
            loadedImage.Dispose();

            lock (this.lockObj)
            {
                /// Create the MonoGameSprite object and register it to this sprite manager.
                MonoGameSprite newSprite = new MonoGameSprite(scaledImage, pixelSize, this.platform);
                this.sprites.Add(newSprite);

                TraceManager.WriteAllTrace("MonoGameSpriteManager.LoadSprite: Sprite created", MonoGameTraceFilters.INFO);
                return newSprite;
            }
        }

        /// <see cref="UISpriteManagerBase.ScaleSprite"/>
        public override UISprite ScaleSprite(UISprite sprite, RCIntVector pixelSize)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("MonoGameSpriteManager"); }
            if (pixelSize == RCIntVector.Undefined) { throw new ArgumentNullException("pixelSize"); }
            if (pixelSize.X <= 0 || pixelSize.Y <= 0) { throw new ArgumentOutOfRangeException("pixelSize"); }

            lock (this.lockObj)
            {
                /// Search the sprite in the list.
                MonoGameSprite spriteToScale = (MonoGameSprite)sprite;

                /// Create a new image and copy the original to this new with the given pixel size.
                Image<Rgb24> scaledImage = new Image<Rgb24>(
                    spriteToScale.Size.X * pixelSize.X,
                    spriteToScale.Size.Y * pixelSize.Y,
                    new Rgb24(0, 0, 0)
                );
                MonoGameImageUtils.CopyImageScaled(spriteToScale.RawImage, scaledImage, spriteToScale.PixelSize, pixelSize);

                /// Create the MonoGameSprite object and register it to this sprite manager.
                MonoGameSprite newSprite = new MonoGameSprite(scaledImage, pixelSize, this.platform);
                newSprite.TransparentColor = sprite.TransparentColor;
                this.sprites.Add(newSprite);

                TraceManager.WriteAllTrace("MonoGameSpriteManager.ScaleSprite: Sprite created", MonoGameTraceFilters.INFO);
                return newSprite;
            }
        }

        /// <see cref="UISpriteManagerBase.ShrinkSprite"/>
        public override UISprite ShrinkSprite(UISprite sprite, RCIntVector spriteSize)
        {
            return this.ShrinkSprite(sprite, spriteSize, sprite.PixelSize);
        }

        /// <see cref="UISpriteManagerBase.ShrinkSprite"/>
        public override UISprite ShrinkSprite(UISprite sprite, RCIntVector spriteSize, RCIntVector pixelSize)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("MonoGameSpriteManager"); }
            if (spriteSize == RCIntVector.Undefined) { throw new ArgumentNullException("spriteSize"); }
            if (pixelSize == RCIntVector.Undefined) { throw new ArgumentNullException("pixelSize"); }

            lock (this.lockObj)
            {
                /// Search the sprite in the list.
                MonoGameSprite spriteToShrink = (MonoGameSprite)sprite;

                /// Create a copy of the original image with pixelsize (1, 1) and shrink it.
                Image<Rgb24> shrinkedImage = new Image<Rgb24>(
                    spriteToShrink.Size.X,
                    spriteToShrink.Size.Y,
                    new Rgb24(0, 0, 0)
                );
                MonoGameImageUtils.CopyImageScaled(
                    spriteToShrink.RawImage,
                    shrinkedImage,
                    spriteToShrink.PixelSize,
                    new RCIntVector(1, 1)
                );
                shrinkedImage.Mutate(x => x.Resize(spriteSize.X, spriteSize.Y, KnownResamplers.NearestNeighbor));

                /// Scale the shrinked image to the target pixel size if necessary.
                Image<Rgb24> scaledShrinkedImage;
                if (pixelSize != new RCIntVector(1, 1))
                {
                    scaledShrinkedImage = new Image<Rgb24>(
                        shrinkedImage.Size().Width * pixelSize.X,
                        shrinkedImage.Size().Height * pixelSize.Y,
                        new Rgb24(0, 0, 0)
                    );
                    MonoGameImageUtils.CopyImageScaled(shrinkedImage, scaledShrinkedImage, new RCIntVector(1, 1), pixelSize);
                }
                else
                {
                    scaledShrinkedImage = shrinkedImage;
                }

                /// Create the MonoGameSprite object and register it to this sprite manager.
                MonoGameSprite shrinkedSprite = new MonoGameSprite(scaledShrinkedImage, pixelSize, this.platform);
                shrinkedSprite.TransparentColor = sprite.TransparentColor;
                this.sprites.Add(shrinkedSprite);

                /// Cleanup if necessary.
                if (shrinkedImage != scaledShrinkedImage) { shrinkedImage.Dispose(); }

                TraceManager.WriteAllTrace("MonoGameSpriteManager.ShrinkSprite: Sprite shrinked", MonoGameTraceFilters.INFO);
                return shrinkedSprite;
            }
        }

        /// <see cref="UISpriteManagerBase.CreateRenderContext_i"/>
        protected override IUIRenderContext CreateRenderContext_i(UISprite sprite)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("MonoGameSpriteManager"); }

            lock (this.lockObj)
            {
                MonoGameSprite target = (MonoGameSprite)sprite;
                if (this.renderContexts.ContainsKey(target))
                {
                    throw new UIException("The given sprite has already an active render context!");
                }
                MonoGameSpriteRenderContext targetContext = new MonoGameSpriteRenderContext(target);
                this.renderContexts.Add(target, targetContext);
                TraceManager.WriteAllTrace("MonoGameSpriteManager.CreateRenderContext: Render context for sprite created", MonoGameTraceFilters.INFO);
                return targetContext;
            }
        }

        /// <see cref="UISpriteManagerBase.CloseRenderContext"/>
        public override void CloseRenderContext(UISprite sprite)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("MonoGameSpriteManager"); }

            lock (this.lockObj)
            {
                MonoGameSprite target = (MonoGameSprite)sprite;
                if (!this.renderContexts.ContainsKey(target))
                {
                    throw new UIException("The given sprite doesn't have active render context!");
                }
                MonoGameSpriteRenderContext targetContext = this.renderContexts[target];
                this.renderContexts.Remove(target);
                targetContext.Close();
                TraceManager.WriteAllTrace("MonoGameSpriteManager.CloseRenderContext: Render context for sprite closed", MonoGameTraceFilters.INFO);
            }
        }

        /// <see cref="UISpriteManagerBase.DestroySprite"/>
        public override void DestroySprite(UISprite sprite)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("MonoGameSpriteManager"); }

            lock (this.lockObj)
            {
                MonoGameSprite spriteToDestroy = (MonoGameSprite)sprite;
                if (this.renderContexts.ContainsKey(spriteToDestroy))
                {
                    throw new UIException("The given sprite still has active render context!");
                }

                /// Remove the sprite from the list and destroy it.
                if (!this.sprites.Remove(spriteToDestroy)) { throw new UIException("The given sprite has already been disposed or has not been created by this sprite manager!"); }
                spriteToDestroy.Dispose();
                TraceManager.WriteAllTrace("MonoGameSpriteManager.DestroySprite: Sprite destroyed", MonoGameTraceFilters.INFO);
            }
        }

        /// <see cref="UISpriteManagerBase.Dispose_i"/>
        protected override void Dispose_i()
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("MonoGameSpriteManager"); }

            lock (this.lockObj)
            {
                /// Close every opened sprite render contexts
                TraceManager.WriteAllTrace("Closing sprite render contexts", MonoGameTraceFilters.INFO);
                foreach (KeyValuePair<MonoGameSprite, MonoGameSpriteRenderContext> item in this.renderContexts)
                {
                    item.Value.Close();
                }
                this.renderContexts.Clear();

                /// Destroy every created sprites
                TraceManager.WriteAllTrace("Destroying sprites", MonoGameTraceFilters.INFO);
                foreach (MonoGameSprite sprite in this.sprites)
                {
                    if (sprite != null) { sprite.Dispose(); }
                }
                this.sprites.Clear();
            }
        }

        #endregion UISpriteManagerBase overrides

        /// <summary>
        /// Forces uploading the sprites to the graphics device.
        /// </summary>
        public void SecondChanceUploadSprites()
        {
            TraceManager.WriteAllTrace("Uploading sprites to the graphics device", MonoGameTraceFilters.INFO);
            lock (this.lockObj)
            {
                foreach (MonoGameSprite sprite in this.sprites)
                {
                    if (sprite.IsUploaded && sprite.MonoGameTexture == null) { sprite.SecondChanceUpload(); }
                }
            }
        }

        /// <summary>
        /// List of the sprites created by this sprite manager.
        /// </summary>
        private RCSet<MonoGameSprite> sprites;

        /// <summary>
        /// List of the sprite render contexts created by this sprite manager.
        /// </summary>
        private Dictionary<MonoGameSprite, MonoGameSpriteRenderContext> renderContexts;

        /// <summary>
        /// Object used as a mutex.
        /// </summary>
        private object lockObj = new object();

        /// <summary>
        /// Reference to the graphics platform.
        /// </summary>
        private MonoGameGraphicsPlatform platform;
    }
}
