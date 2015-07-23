using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using RC.Common;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.Xna.Framework.Graphics;
using RC.Common.Diagnostics;
using System.IO;

namespace RC.UI.XnaPlugin
{
    /// <summary>
    /// The XNA-implementation of the sprite manager.
    /// </summary>
    class XnaSpriteManager : UISpriteManagerBase
    {
        /// <summary>
        /// Constructs an XnaSpriteManager object.
        /// </summary>
        /// <param name="platform">Reference to the platform.</param>
        public XnaSpriteManager(XnaGraphicsPlatform platform)
        {
            this.platform = platform;
            this.sprites = new RCSet<XnaSprite>();
            this.renderContexts = new Dictionary<XnaSprite, XnaSpriteRenderContext>();
        }

        #region UISpriteManagerBase overrides

        /// <see cref="IUISpriteManager.CreateSprite"/>
        public override UISprite CreateSprite(RCColor color, RCIntVector spriteSize)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("XnaSpriteManager"); }
            if (color == RCColor.Undefined) { throw new ArgumentNullException("color"); }
            if (spriteSize == RCIntVector.Undefined) { throw new ArgumentNullException("spriteSize"); }

            /// Create the empty bitmap and fill with the given color
            Bitmap emptyBitmap = new Bitmap(spriteSize.X, spriteSize.Y, PixelFormat.Format24bppRgb);
            Graphics gc = Graphics.FromImage(emptyBitmap);
            gc.Clear(Color.FromArgb(color.R, color.G, color.B));
            gc.Dispose();

            lock (this.lockObj)
            {
                /// Create the XnaSprite object and register it to this sprite manager.
                XnaSprite newSprite = new XnaSprite(emptyBitmap, new RCIntVector(1, 1), this.platform);
                this.sprites.Add(newSprite);

                TraceManager.WriteAllTrace("XnaSpriteManager.CreateSprite: Sprite created", XnaTraceFilters.INFO);
                return newSprite;
            }
        }

        /// <see cref="IUISpriteManager.CreateSprite"/>
        public override UISprite CreateSprite(RCColor color, RCIntVector spriteSize, RCIntVector pixelSize)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("XnaSpriteManager"); }
            if (color == RCColor.Undefined) { throw new ArgumentNullException("color"); }
            if (spriteSize == RCIntVector.Undefined) { throw new ArgumentNullException("spriteSize"); }
            if (pixelSize == RCIntVector.Undefined) { throw new ArgumentNullException("pixelSize"); }

            /// Create the empty bitmap and fill with the given color
            Bitmap emptyBitmap = new Bitmap(spriteSize.X * pixelSize.X, spriteSize.Y * pixelSize.Y, PixelFormat.Format24bppRgb);
            Graphics gc = Graphics.FromImage(emptyBitmap);
            gc.Clear(Color.FromArgb(color.R, color.G, color.B));
            gc.Dispose();

            lock (this.lockObj)
            {
                /// Create the XnaSprite object and register it to this sprite manager.
                XnaSprite newSprite = new XnaSprite(emptyBitmap, pixelSize, this.platform);
                this.sprites.Add(newSprite);

                TraceManager.WriteAllTrace("XnaSpriteManager.CreateSprite: Sprite created", XnaTraceFilters.INFO);
                return newSprite;
            }
        }

        /// <see cref="UISpriteManagerBase.LoadSprite"/>
        public override UISprite LoadSprite(string fileName)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("XnaSpriteManager"); }
            if (null == fileName) { throw new ArgumentNullException("fileName"); }

            /// Load the bitmap from the given file.
            Bitmap loadedBitmap = (Bitmap)Image.FromFile(fileName);
            if (loadedBitmap.PixelFormat != PixelFormat.Format24bppRgb)
            {
                throw new ArgumentException("Pixel format of the given Bitmap must be PixelFormat.Format24bppRgb",
                                            "originalBmp");
            }

            lock (this.lockObj)
            {
                /// Create the XnaSprite object and register it to this sprite manager.
                XnaSprite newSprite = new XnaSprite(loadedBitmap, new RCIntVector(1, 1), this.platform);
                this.sprites.Add(newSprite);

                TraceManager.WriteAllTrace("XnaSpriteManager.LoadSprite: Sprite created", XnaTraceFilters.INFO);
                return newSprite;
            }
        }

        /// <see cref="UISpriteManagerBase.LoadSprite"/>
        public override UISprite LoadSprite(string fileName, RCIntVector pixelSize)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("XnaSpriteManager"); }
            if (null == fileName) { throw new ArgumentNullException("fileName"); }
            if (pixelSize == RCIntVector.Undefined) { throw new ArgumentNullException("pixelSize"); }
            if (pixelSize.X <= 0 || pixelSize.Y <= 0) { throw new ArgumentOutOfRangeException("pixelSize"); }

            /// Load the sprite from the given file.
            Bitmap loadedBitmap = (Bitmap)Image.FromFile(fileName);
            if (loadedBitmap.PixelFormat != PixelFormat.Format24bppRgb)
            {
                throw new ArgumentException("Pixel format of the given Bitmap must be PixelFormat.Format24bppRgb",
                                            "originalBmp");
            }

            /// Create a new bitmap and copy the original to this new with the given pixel size.
            Bitmap scaledBitmap = new Bitmap(loadedBitmap.Width * pixelSize.X,
                                             loadedBitmap.Height * pixelSize.Y,
                                             PixelFormat.Format24bppRgb);
            XnaBitmapUtils.CopyBitmapScaled(loadedBitmap, scaledBitmap, new RCIntVector(1, 1), pixelSize);
            loadedBitmap.Dispose();

            lock (this.lockObj)
            {
                /// Create the XnaSprite object and register it to this sprite manager.
                XnaSprite newSprite = new XnaSprite(scaledBitmap, pixelSize, this.platform);
                this.sprites.Add(newSprite);

                TraceManager.WriteAllTrace("XnaSpriteManager.LoadSprite: Sprite created", XnaTraceFilters.INFO);
                return newSprite;
            }
        }

        /// <see cref="UISpriteManagerBase.LoadSprite"/>
        public override UISprite LoadSprite(byte[] imageData)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("XnaSpriteManager"); }
            if (null == imageData) { throw new ArgumentNullException("imageData"); }

            /// Load the bitmap from the given byte array.
            Stream byteStream = new MemoryStream(imageData);
            Bitmap loadedBitmap = (Bitmap)Image.FromStream(byteStream);
            byteStream.Close();
            if (loadedBitmap.PixelFormat != PixelFormat.Format24bppRgb)
            {
                throw new ArgumentException("Pixel format of the given Bitmap must be PixelFormat.Format24bppRgb",
                                            "originalBmp");
            }

            lock (this.lockObj)
            {
                /// Create the XnaSprite object and register it to this sprite manager.
                XnaSprite newSprite = new XnaSprite(loadedBitmap, new RCIntVector(1, 1), this.platform);
                this.sprites.Add(newSprite);

                TraceManager.WriteAllTrace("XnaSpriteManager.LoadSprite: Sprite created", XnaTraceFilters.INFO);
                return newSprite;
            }
        }

        /// <see cref="UISpriteManagerBase.LoadSprite"/>
        public override UISprite LoadSprite(byte[] imageData, RCIntVector pixelSize)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("XnaSpriteManager"); }
            if (null == imageData) { throw new ArgumentNullException("imageData"); }
            if (pixelSize == RCIntVector.Undefined) { throw new ArgumentNullException("pixelSize"); }
            if (pixelSize.X <= 0 || pixelSize.Y <= 0) { throw new ArgumentOutOfRangeException("pixelSize"); }

            /// Load the bitmap from the given byte array.
            Stream byteStream = new MemoryStream(imageData);
            Bitmap loadedBitmap = (Bitmap)Image.FromStream(byteStream);
            byteStream.Close();
            if (loadedBitmap.PixelFormat != PixelFormat.Format24bppRgb)
            {
                throw new ArgumentException("Pixel format of the given Bitmap must be PixelFormat.Format24bppRgb",
                                            "originalBmp");
            }

            /// Create a new bitmap and copy the original to this new with the given pixel size.
            Bitmap scaledBitmap = new Bitmap(loadedBitmap.Width * pixelSize.X,
                                             loadedBitmap.Height * pixelSize.Y,
                                             PixelFormat.Format24bppRgb);
            XnaBitmapUtils.CopyBitmapScaled(loadedBitmap, scaledBitmap, new RCIntVector(1, 1), pixelSize);
            loadedBitmap.Dispose();

            lock (this.lockObj)
            {
                /// Create the XnaSprite object and register it to this sprite manager.
                XnaSprite newSprite = new XnaSprite(scaledBitmap, pixelSize, this.platform);
                this.sprites.Add(newSprite);

                TraceManager.WriteAllTrace("XnaSpriteManager.LoadSprite: Sprite created", XnaTraceFilters.INFO);
                return newSprite;
            }
        }

        /// <see cref="UISpriteManagerBase.ScaleSprite"/>
        public override UISprite ScaleSprite(UISprite sprite, RCIntVector pixelSize)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("XnaSpriteManager"); }
            if (pixelSize == RCIntVector.Undefined) { throw new ArgumentNullException("pixelSize"); }
            if (pixelSize.X <= 0 || pixelSize.Y <= 0) { throw new ArgumentOutOfRangeException("pixelSize"); }

            lock (this.lockObj)
            {
                /// Search the sprite in the list.
                XnaSprite spriteToScale = (XnaSprite)sprite;

                /// Create a new bitmap and copy the original to this new with the given pixel size.
                Bitmap scaledBitmap = new Bitmap(spriteToScale.Size.X * pixelSize.X,
                                                 spriteToScale.Size.Y * pixelSize.Y,
                                                 PixelFormat.Format24bppRgb);
                XnaBitmapUtils.CopyBitmapScaled(spriteToScale.RawBitmap, scaledBitmap, spriteToScale.PixelSize, pixelSize);

                /// Create the XnaSprite object and register it to this sprite manager.
                XnaSprite newSprite = new XnaSprite(scaledBitmap, pixelSize, this.platform);
                newSprite.TransparentColor = sprite.TransparentColor;
                this.sprites.Add(newSprite);

                TraceManager.WriteAllTrace("XnaSpriteManager.ScaleSprite: Sprite created", XnaTraceFilters.INFO);
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
            if (this.ObjectDisposed) { throw new ObjectDisposedException("XnaSpriteManager"); }
            if (spriteSize == RCIntVector.Undefined) { throw new ArgumentNullException("spriteSize"); }
            if (pixelSize == RCIntVector.Undefined) { throw new ArgumentNullException("pixelSize"); }

            lock (this.lockObj)
            {
                /// Search the sprite in the list.
                XnaSprite spriteToShrink = (XnaSprite)sprite;

                /// Create a copy of the original bitmap with pixelsize (1, 1) if necessary.
                Bitmap bitmapToShrink;
                if (spriteToShrink.PixelSize != new RCIntVector(1, 1))
                {
                    bitmapToShrink = new Bitmap(spriteToShrink.Size.X, spriteToShrink.Size.Y, PixelFormat.Format24bppRgb);
                    XnaBitmapUtils.CopyBitmapScaled(spriteToShrink.RawBitmap, bitmapToShrink, spriteToShrink.PixelSize, new RCIntVector(1, 1));
                }
                else
                {
                    bitmapToShrink = spriteToShrink.RawBitmap;
                }

                /// Create the shrinked bitmap with pixelsize (1, 1).
                Bitmap shrinkedBitmap = new Bitmap(spriteSize.X, spriteSize.Y, PixelFormat.Format24bppRgb);
                Graphics gc = Graphics.FromImage(shrinkedBitmap);
                gc.InterpolationMode = InterpolationMode.NearestNeighbor;
                gc.DrawImage(bitmapToShrink, new Rectangle(0, 0, spriteSize.X, spriteSize.Y), new Rectangle(0, 0, bitmapToShrink.Width, bitmapToShrink.Height), GraphicsUnit.Pixel);
                gc.Dispose();

                /// Scale the shrinked bitmap to the target pixel size if necessary.
                Bitmap scaledShrinkedBitmap;
                if (pixelSize != new RCIntVector(1, 1))
                {
                    scaledShrinkedBitmap = new Bitmap(shrinkedBitmap.Width*pixelSize.X,
                                                      shrinkedBitmap.Height*pixelSize.Y,
                                                      PixelFormat.Format24bppRgb);
                    XnaBitmapUtils.CopyBitmapScaled(shrinkedBitmap, scaledShrinkedBitmap, new RCIntVector(1, 1), pixelSize);
                }
                else
                {
                    scaledShrinkedBitmap = shrinkedBitmap;
                }

                /// Create the XnaSprite object and register it to this sprite manager.
                XnaSprite shrinkedSprite = new XnaSprite(scaledShrinkedBitmap, pixelSize, this.platform);
                shrinkedSprite.TransparentColor = sprite.TransparentColor;
                this.sprites.Add(shrinkedSprite);

                /// Cleanup if necessary.
                if (bitmapToShrink != spriteToShrink.RawBitmap) { bitmapToShrink.Dispose(); }
                if (shrinkedBitmap != scaledShrinkedBitmap) { shrinkedBitmap.Dispose(); }

                TraceManager.WriteAllTrace("XnaSpriteManager.ShrinkedSprite: Sprite shrinked", XnaTraceFilters.INFO);
                return shrinkedSprite;
            }
        }

        /// <see cref="UISpriteManagerBase.CreateRenderContext_i"/>
        protected override IUIRenderContext CreateRenderContext_i(UISprite sprite)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("XnaSpriteManager"); }

            lock (this.lockObj)
            {
                XnaSprite target = (XnaSprite)sprite;
                if (this.renderContexts.ContainsKey(target))
                {
                    throw new UIException("The given sprite has already an active render context!");
                }
                XnaSpriteRenderContext targetContext = new XnaSpriteRenderContext(target, this);
                this.renderContexts.Add(target, targetContext);
                TraceManager.WriteAllTrace("XnaSpriteManager.CreateRenderContext: Render context for sprite created", XnaTraceFilters.INFO);
                return targetContext;
            }
        }

        /// <see cref="UISpriteManagerBase.CloseRenderContext"/>
        public override void CloseRenderContext(UISprite sprite)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("XnaSpriteManager"); }

            lock (this.lockObj)
            {
                XnaSprite target = (XnaSprite)sprite;
                if (!this.renderContexts.ContainsKey(target))
                {
                    throw new UIException("The given sprite doesn't have active render context!");
                }
                XnaSpriteRenderContext targetContext = this.renderContexts[target];
                this.renderContexts.Remove(target);
                targetContext.Close();
                TraceManager.WriteAllTrace("XnaSpriteManager.CloseRenderContext: Render context for sprite closed", XnaTraceFilters.INFO);
            }
        }

        /// <see cref="UISpriteManagerBase.DestroySprite"/>
        public override void DestroySprite(UISprite sprite)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("XnaSpriteManager"); }

            lock (this.lockObj)
            {
                XnaSprite spriteToDestroy = (XnaSprite)sprite;
                if (this.renderContexts.ContainsKey(spriteToDestroy))
                {
                    throw new UIException("The given sprite still has active render context!");
                }

                /// Remove the sprite from the list and destroy it.
                if (!this.sprites.Remove(spriteToDestroy)) { throw new UIException("The given sprite has already been disposed or has not been created by this sprite manager!"); }
                spriteToDestroy.Dispose();
                TraceManager.WriteAllTrace("XnaSpriteManager.DestroySprite: Sprite destroyed", XnaTraceFilters.INFO);
            }
        }

        /// <see cref="UISpriteManagerBase.Dispose_i"/>
        protected override void Dispose_i()
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("XnaSpriteManager"); }

            lock (this.lockObj)
            {
                /// Close every opened sprite render contexts
                TraceManager.WriteAllTrace("Closing sprite render contexts", XnaTraceFilters.INFO);
                foreach (KeyValuePair<XnaSprite, XnaSpriteRenderContext> item in this.renderContexts)
                {
                    item.Value.Close();
                }
                this.renderContexts.Clear();

                /// Destroy every created sprites
                TraceManager.WriteAllTrace("Destroying sprites", XnaTraceFilters.INFO);
                foreach (XnaSprite sprite in this.sprites)
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
            TraceManager.WriteAllTrace("Uploading sprites to the graphics device", XnaTraceFilters.INFO);
            lock (this.lockObj)
            {
                foreach (XnaSprite sprite in this.sprites)
                {
                    if (sprite.IsUploaded && sprite.XnaTexture == null) { sprite.SecondChanceUpload(); }
                }
            }
        }

        /// <summary>
        /// List of the sprites created by this sprite manager.
        /// </summary>
        private RCSet<XnaSprite> sprites;

        /// <summary>
        /// List of the sprite render contexts created by this sprite manager.
        /// </summary>
        private Dictionary<XnaSprite, XnaSpriteRenderContext> renderContexts;

        /// <summary>
        /// Object used as a mutex.
        /// </summary>
        private object lockObj = new object();

        /// <summary>
        /// Reference to the graphics platform.
        /// </summary>
        private XnaGraphicsPlatform platform;
    }
}
