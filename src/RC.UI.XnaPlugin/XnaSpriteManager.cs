using System;
using System.Collections.Generic;
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
            this.sprites = new List<XnaSprite>();
            this.renderContexts = new Dictionary<XnaSprite, XnaSpriteRenderContext>();
            this.firstFreeIdx = 0;
        }

        #region UISpriteManagerBase overrides

        /// <see cref="IUISpriteManager.CreateSprite"/>
        public override UISprite CreateSprite(UIColor color, RCIntVector spriteSize)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("XnaSpriteManager"); }
            if (color == UIColor.Undefined) { throw new ArgumentNullException("color"); }
            if (spriteSize == RCIntVector.Undefined) { throw new ArgumentNullException("spriteSize"); }

            /// Create the empty bitmap and fill with the given color
            Bitmap emptyBitmap = new Bitmap(spriteSize.X, spriteSize.Y, PixelFormat.Format24bppRgb);
            Graphics gc = Graphics.FromImage(emptyBitmap);
            gc.Clear(Color.FromArgb(color.R, color.G, color.B));
            gc.Dispose();

            /// Create the XnaSprite object and register it to this sprite manager.
            XnaSprite newSprite = new XnaSprite(emptyBitmap, this.firstFreeIdx, new RCIntVector(1, 1), this.platform);
            this.RegisterNewSprite(newSprite);

            TraceManager.WriteAllTrace(string.Format("XnaSpriteManager.CreateSprite: Sprite({0}) created", newSprite.ResourceId),
                                       XnaTraceFilters.INFO);
            return newSprite;
        }

        /// <see cref="IUISpriteManager.CreateSprite"/>
        public override UISprite CreateSprite(UIColor color, RCIntVector spriteSize, RCIntVector pixelSize)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("XnaSpriteManager"); }
            if (color == UIColor.Undefined) { throw new ArgumentNullException("color"); }
            if (spriteSize == RCIntVector.Undefined) { throw new ArgumentNullException("spriteSize"); }
            if (pixelSize == RCIntVector.Undefined) { throw new ArgumentNullException("pixelSize"); }

            /// Create the empty bitmap and fill with the given color
            Bitmap emptyBitmap = new Bitmap(spriteSize.X * pixelSize.X, spriteSize.Y * pixelSize.Y, PixelFormat.Format24bppRgb);
            Graphics gc = Graphics.FromImage(emptyBitmap);
            gc.Clear(Color.FromArgb(color.R, color.G, color.B));
            gc.Dispose();

            /// Create the XnaSprite object and register it to this sprite manager.
            XnaSprite newSprite = new XnaSprite(emptyBitmap, this.firstFreeIdx, pixelSize, this.platform);
            this.RegisterNewSprite(newSprite);

            TraceManager.WriteAllTrace(string.Format("XnaSpriteManager.CreateSprite: Sprite({0}) created", newSprite.ResourceId),
                                       XnaTraceFilters.INFO);
            return newSprite;
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

            /// Create the XnaSprite object and register it to this sprite manager.
            XnaSprite newSprite = new XnaSprite(loadedBitmap, this.firstFreeIdx, new RCIntVector(1, 1), this.platform);
            this.RegisterNewSprite(newSprite);

            TraceManager.WriteAllTrace(string.Format("XnaSpriteManager.LoadSprite: Sprite({0}) created", newSprite.ResourceId),
                                       XnaTraceFilters.INFO);
            return newSprite;
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

            /// Create the XnaSprite object and register it to this sprite manager.
            XnaSprite newSprite = new XnaSprite(scaledBitmap, this.firstFreeIdx, pixelSize, this.platform);
            this.RegisterNewSprite(newSprite);

            TraceManager.WriteAllTrace(string.Format("XnaSpriteManager.LoadSprite: Sprite({0}) created", newSprite.ResourceId),
                                       XnaTraceFilters.INFO);
            return newSprite;
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

            /// Create the XnaSprite object and register it to this sprite manager.
            XnaSprite newSprite = new XnaSprite(loadedBitmap, this.firstFreeIdx, new RCIntVector(1, 1), this.platform);
            this.RegisterNewSprite(newSprite);

            TraceManager.WriteAllTrace(string.Format("XnaSpriteManager.LoadSprite: Sprite({0}) created", newSprite.ResourceId),
                                       XnaTraceFilters.INFO);
            return newSprite;
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

            /// Create the XnaSprite object and register it to this sprite manager.
            XnaSprite newSprite = new XnaSprite(scaledBitmap, this.firstFreeIdx, pixelSize, this.platform);
            this.RegisterNewSprite(newSprite);

            TraceManager.WriteAllTrace(string.Format("XnaSpriteManager.LoadSprite: Sprite({0}) created", newSprite.ResourceId),
                                       XnaTraceFilters.INFO);
            return newSprite;
        }

        /// <see cref="UISpriteManagerBase.ScaleSprite"/>
        public override UISprite ScaleSprite(UISprite sprite, RCIntVector pixelSize)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("XnaSpriteManager"); }
            if (pixelSize == RCIntVector.Undefined) { throw new ArgumentNullException("pixelSize"); }
            if (pixelSize.X <= 0 || pixelSize.Y <= 0) { throw new ArgumentOutOfRangeException("pixelSize"); }

            /// Search the sprite in the list.
            XnaSprite spriteToScale = this.GetXnaSprite(sprite);

            /// Create a new bitmap and copy the original to this new with the given pixel size.
            Bitmap scaledBitmap = new Bitmap(spriteToScale.Size.X * pixelSize.X,
                                             spriteToScale.Size.Y * pixelSize.Y,
                                             PixelFormat.Format24bppRgb);
            XnaBitmapUtils.CopyBitmapScaled(spriteToScale.RawBitmap, scaledBitmap, spriteToScale.PixelSize, pixelSize);

            /// Create the XnaSprite object and register it to this sprite manager.
            XnaSprite newSprite = new XnaSprite(scaledBitmap, this.firstFreeIdx, pixelSize, this.platform);
            newSprite.TransparentColor = sprite.TransparentColor;
            this.RegisterNewSprite(newSprite);

            TraceManager.WriteAllTrace(string.Format("XnaSpriteManager.ScaleSprite: Sprite({0}) created", newSprite.ResourceId),
                                       XnaTraceFilters.INFO);
            return newSprite;
        }

        /// <see cref="UISpriteManagerBase.CreateRenderContext"/>
        public override IUIRenderContext CreateRenderContext(UISprite sprite)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("XnaSpriteManager"); }

            XnaSprite target = this.GetXnaSprite(sprite);
            if (this.renderContexts.ContainsKey(target))
            {
                throw new UIException("The given sprite has already an active render context!");
            }
            XnaSpriteRenderContext targetContext = new XnaSpriteRenderContext(target, this);
            this.renderContexts.Add(target, targetContext);
            TraceManager.WriteAllTrace(string.Format("XnaSpriteManager.CreateRenderContext: Render context for sprite({0}) created", sprite.ResourceId),
                                       XnaTraceFilters.INFO);
            return targetContext;
        }

        /// <see cref="UISpriteManagerBase.CloseRenderContext"/>
        public override void CloseRenderContext(UISprite sprite)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("XnaSpriteManager"); }

            XnaSprite target = this.GetXnaSprite(sprite);
            if (!this.renderContexts.ContainsKey(target))
            {
                throw new UIException("The given sprite doesn't have active render context!");
            }
            XnaSpriteRenderContext targetContext = this.renderContexts[target];
            this.renderContexts.Remove(target);
            targetContext.Close();
            TraceManager.WriteAllTrace(string.Format("XnaSpriteManager.CloseRenderContext: Render context for sprite({0}) closed", sprite.ResourceId),
                                       XnaTraceFilters.INFO);
        }

        /// <see cref="UISpriteManagerBase.DestroySprite"/>
        public override void DestroySprite(UISprite sprite)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("XnaSpriteManager"); }

            /// Search the sprite in the list.
            XnaSprite spriteToDestroy = this.GetXnaSprite(sprite);

            /// Remove the sprite from the list and destroy it.
            this.sprites[sprite.ResourceId] = null;
            this.firstFreeIdx = sprite.ResourceId;
            spriteToDestroy.Dispose();
            TraceManager.WriteAllTrace(string.Format("XnaSpriteManager.DestroySprite: Sprite({0}) destroyed", sprite.ResourceId),
                                       XnaTraceFilters.INFO);
        }

        /// <see cref="UISpriteManagerBase.Dispose_i"/>
        protected override void Dispose_i()
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("XnaSpriteManager"); }

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
            this.firstFreeIdx = 0;
        }

        #endregion UISpriteManagerBase overrides

        /// <summary>
        /// Forces uploading the sprites to the graphics device.
        /// </summary>
        public void UploadSprites()
        {
            TraceManager.WriteAllTrace("Uploading sprites to the graphics device", XnaTraceFilters.INFO);
            foreach (XnaSprite sprite in this.sprites)
            {
                if (sprite != null) { sprite.Upload(); }
            }
        }

        /// <summary>
        /// Gets an XnaSprite reference to the given UISprite.
        /// </summary>
        /// <param name="sprite">The sprite to get.</param>
        /// <returns>An XnaSprite reference to the given sprite.</returns>
        /// <exception cref="UIException">
        /// In case of invalid resource ID.
        /// If the given UISprite was not created by this sprite manager.
        /// </exception>
        public XnaSprite GetXnaSprite(UISprite sprite)
        {
            if (sprite == null) { throw new ArgumentNullException("sprite"); }
            if (sprite.ResourceId < 0 || sprite.ResourceId >= this.sprites.Count) { throw new UIException("Invalid resourceID!"); }
            XnaSprite retSprite = this.sprites[sprite.ResourceId];
            if (sprite != retSprite) { throw new UIException("The given sprite was not created by this sprite manager!"); }
            return retSprite;
        }

        /// <summary>
        /// Registers the given XnaSprite to this sprite manager.
        /// </summary>
        /// <param name="newSprite">The sprite to register.</param>
        private void RegisterNewSprite(XnaSprite newSprite)
        {
            /// Put the sprite to the first free place or add it to the end of the list if there is no
            /// more free places.
            if (this.firstFreeIdx == this.sprites.Count)
            {
                this.sprites.Add(newSprite);
            }
            else
            {
                this.sprites[this.firstFreeIdx] = newSprite;
            }

            /// Find the next free place in the sprite list.
            if (this.firstFreeIdx == this.sprites.Count - 1)
            {
                /// If the new sprite has been added to the end of the list, then the next free place will be at
                /// the end of the list again.
                this.firstFreeIdx++;
            }
            else
            {
                /// Else search for the nearest free place.
                int i = this.firstFreeIdx + 1;
                for (; i < this.sprites.Count; i++)
                {
                    if (this.sprites[i] == null)
                    {
                        break;
                    }
                }
                this.firstFreeIdx = i;
            }
        }

        /// <summary>
        /// List of the sprites created by this sprite manager.
        /// </summary>
        private List<XnaSprite> sprites;

        /// <summary>
        /// List of the sprite render contexts created by this sprite manager.
        /// </summary>
        private Dictionary<XnaSprite, XnaSpriteRenderContext> renderContexts;

        /// <summary>
        /// The first free index in this.sprites or this.sprites.Count if there is no free index
        /// in this.sprite.
        /// </summary>
        private int firstFreeIdx;

        /// <summary>
        /// Reference to the graphics platform.
        /// </summary>
        private XnaGraphicsPlatform platform;
    }
}
