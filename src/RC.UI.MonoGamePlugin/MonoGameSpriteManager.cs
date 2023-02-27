using System;
using System.Collections.Generic;
// using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using RC.Common;
// using System.Drawing;
// using System.Drawing.Imaging;
using Microsoft.Xna.Framework.Graphics;
using RC.Common.Diagnostics;
using System.IO;

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
            //this.sprites = new RCSet<MonoGameSprite>();
            //this.renderContexts = new Dictionary<MonoGameSprite, MonoGameSpriteRenderContext>();
        }

        #region UISpriteManagerBase overrides

        /// <see cref="IUISpriteManager.CreateSprite"/>
        public override UISprite CreateSprite(RCColor color, RCIntVector spriteSize)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("MonoGameSpriteManager"); }
            if (color == RCColor.Undefined) { throw new ArgumentNullException("color"); }
            if (spriteSize == RCIntVector.Undefined) { throw new ArgumentNullException("spriteSize"); }

            // TODO: implement!
            throw new NotImplementedException();

            // /// Create the empty bitmap and fill with the given color
            // Bitmap emptyBitmap = new Bitmap(spriteSize.X, spriteSize.Y, PixelFormat.Format24bppRgb);
            // Graphics gc = Graphics.FromImage(emptyBitmap);
            // gc.Clear(Color.FromArgb(color.R, color.G, color.B));
            // gc.Dispose();

            // lock (this.lockObj)
            // {
            //     /// Create the MonoGameSprite object and register it to this sprite manager.
            //     MonoGameSprite newSprite = new MonoGameSprite(emptyBitmap, new RCIntVector(1, 1), this.platform);
            //     this.sprites.Add(newSprite);

            //     TraceManager.WriteAllTrace("MonoGameSpriteManager.CreateSprite: Sprite created", MonoGameTraceFilters.INFO);
            //     return newSprite;
            // }
        }

        /// <see cref="IUISpriteManager.CreateSprite"/>
        public override UISprite CreateSprite(RCColor color, RCIntVector spriteSize, RCIntVector pixelSize)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("MonoGameSpriteManager"); }
            if (color == RCColor.Undefined) { throw new ArgumentNullException("color"); }
            if (spriteSize == RCIntVector.Undefined) { throw new ArgumentNullException("spriteSize"); }
            if (pixelSize == RCIntVector.Undefined) { throw new ArgumentNullException("pixelSize"); }

            // TODO: implement!
            throw new NotImplementedException();

            // /// Create the empty bitmap and fill with the given color
            // Bitmap emptyBitmap = new Bitmap(spriteSize.X * pixelSize.X, spriteSize.Y * pixelSize.Y, PixelFormat.Format24bppRgb);
            // Graphics gc = Graphics.FromImage(emptyBitmap);
            // gc.Clear(Color.FromArgb(color.R, color.G, color.B));
            // gc.Dispose();

            // lock (this.lockObj)
            // {
            //     /// Create the MonoGameSprite object and register it to this sprite manager.
            //     MonoGameSprite newSprite = new MonoGameSprite(emptyBitmap, pixelSize, this.platform);
            //     this.sprites.Add(newSprite);

            //     TraceManager.WriteAllTrace("MonoGameSpriteManager.CreateSprite: Sprite created", MonoGameTraceFilters.INFO);
            //     return newSprite;
            // }
        }

        /// <see cref="UISpriteManagerBase.LoadSprite"/>
        public override UISprite LoadSprite(string fileName)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("MonoGameSpriteManager"); }
            if (null == fileName) { throw new ArgumentNullException("fileName"); }

            // TODO: implement!
            throw new NotImplementedException();

            // /// Load the bitmap from the given file.
            // Bitmap loadedBitmap = (Bitmap)Image.FromFile(fileName);
            // if (loadedBitmap.PixelFormat != PixelFormat.Format24bppRgb)
            // {
            //     throw new ArgumentException("Pixel format of the given Bitmap must be PixelFormat.Format24bppRgb",
            //                                 "originalBmp");
            // }

            // lock (this.lockObj)
            // {
            //     /// Create the MonoGameSprite object and register it to this sprite manager.
            //     MonoGameSprite newSprite = new MonoGameSprite(loadedBitmap, new RCIntVector(1, 1), this.platform);
            //     this.sprites.Add(newSprite);

            //     TraceManager.WriteAllTrace("MonoGameSpriteManager.LoadSprite: Sprite created", MonoGameTraceFilters.INFO);
            //     return newSprite;
            // }
        }

        /// <see cref="UISpriteManagerBase.LoadSprite"/>
        public override UISprite LoadSprite(string fileName, RCIntVector pixelSize)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("MonoGameSpriteManager"); }
            if (null == fileName) { throw new ArgumentNullException("fileName"); }
            if (pixelSize == RCIntVector.Undefined) { throw new ArgumentNullException("pixelSize"); }
            if (pixelSize.X <= 0 || pixelSize.Y <= 0) { throw new ArgumentOutOfRangeException("pixelSize"); }

            // TODO: implement!
            throw new NotImplementedException();

            // /// Load the sprite from the given file.
            // Bitmap loadedBitmap = (Bitmap)Image.FromFile(fileName);
            // if (loadedBitmap.PixelFormat != PixelFormat.Format24bppRgb)
            // {
            //     throw new ArgumentException("Pixel format of the given Bitmap must be PixelFormat.Format24bppRgb",
            //                                 "originalBmp");
            // }

            // /// Create a new bitmap and copy the original to this new with the given pixel size.
            // Bitmap scaledBitmap = new Bitmap(loadedBitmap.Width * pixelSize.X,
            //                                  loadedBitmap.Height * pixelSize.Y,
            //                                  PixelFormat.Format24bppRgb);
            // MonoGameBitmapUtils.CopyBitmapScaled(loadedBitmap, scaledBitmap, new RCIntVector(1, 1), pixelSize);
            // loadedBitmap.Dispose();

            // lock (this.lockObj)
            // {
            //     /// Create the MonoGameSprite object and register it to this sprite manager.
            //     MonoGameSprite newSprite = new MonoGameSprite(scaledBitmap, pixelSize, this.platform);
            //     this.sprites.Add(newSprite);

            //     TraceManager.WriteAllTrace("MonoGameSpriteManager.LoadSprite: Sprite created", MonoGameTraceFilters.INFO);
            //     return newSprite;
            // }
        }

        /// <see cref="UISpriteManagerBase.LoadSprite"/>
        public override UISprite LoadSprite(byte[] imageData)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("MonoGameSpriteManager"); }
            if (null == imageData) { throw new ArgumentNullException("imageData"); }

            // TODO: implement!
            throw new NotImplementedException();

            // /// Load the bitmap from the given byte array.
            // Stream byteStream = new MemoryStream(imageData);
            // Bitmap loadedBitmap = (Bitmap)Image.FromStream(byteStream);
            // byteStream.Close();
            // if (loadedBitmap.PixelFormat != PixelFormat.Format24bppRgb)
            // {
            //     throw new ArgumentException("Pixel format of the given Bitmap must be PixelFormat.Format24bppRgb",
            //                                 "originalBmp");
            // }

            // lock (this.lockObj)
            // {
            //     /// Create the MonoGameSprite object and register it to this sprite manager.
            //     MonoGameSprite newSprite = new MonoGameSprite(loadedBitmap, new RCIntVector(1, 1), this.platform);
            //     this.sprites.Add(newSprite);

            //     TraceManager.WriteAllTrace("MonoGameSpriteManager.LoadSprite: Sprite created", MonoGameTraceFilters.INFO);
            //     return newSprite;
            // }
        }

        /// <see cref="UISpriteManagerBase.LoadSprite"/>
        public override UISprite LoadSprite(byte[] imageData, RCIntVector pixelSize)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("MonoGameSpriteManager"); }
            if (null == imageData) { throw new ArgumentNullException("imageData"); }
            if (pixelSize == RCIntVector.Undefined) { throw new ArgumentNullException("pixelSize"); }
            if (pixelSize.X <= 0 || pixelSize.Y <= 0) { throw new ArgumentOutOfRangeException("pixelSize"); }

            // TODO: implement!
            throw new NotImplementedException();

            // /// Load the bitmap from the given byte array.
            // Stream byteStream = new MemoryStream(imageData);
            // Bitmap loadedBitmap = (Bitmap)Image.FromStream(byteStream);
            // byteStream.Close();
            // if (loadedBitmap.PixelFormat != PixelFormat.Format24bppRgb)
            // {
            //     throw new ArgumentException("Pixel format of the given Bitmap must be PixelFormat.Format24bppRgb",
            //                                 "originalBmp");
            // }

            // /// Create a new bitmap and copy the original to this new with the given pixel size.
            // Bitmap scaledBitmap = new Bitmap(loadedBitmap.Width * pixelSize.X,
            //                                  loadedBitmap.Height * pixelSize.Y,
            //                                  PixelFormat.Format24bppRgb);
            // MonoGameBitmapUtils.CopyBitmapScaled(loadedBitmap, scaledBitmap, new RCIntVector(1, 1), pixelSize);
            // loadedBitmap.Dispose();

            // lock (this.lockObj)
            // {
            //     /// Create the MonoGameSprite object and register it to this sprite manager.
            //     MonoGameSprite newSprite = new MonoGameSprite(scaledBitmap, pixelSize, this.platform);
            //     this.sprites.Add(newSprite);

            //     TraceManager.WriteAllTrace("MonoGameSpriteManager.LoadSprite: Sprite created", MonoGameTraceFilters.INFO);
            //     return newSprite;
            // }
        }

        /// <see cref="UISpriteManagerBase.ScaleSprite"/>
        public override UISprite ScaleSprite(UISprite sprite, RCIntVector pixelSize)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("MonoGameSpriteManager"); }
            if (pixelSize == RCIntVector.Undefined) { throw new ArgumentNullException("pixelSize"); }
            if (pixelSize.X <= 0 || pixelSize.Y <= 0) { throw new ArgumentOutOfRangeException("pixelSize"); }

            // TODO: implement!
            throw new NotImplementedException();

            // lock (this.lockObj)
            // {
            //     /// Search the sprite in the list.
            //     MonoGameSprite spriteToScale = (MonoGameSprite)sprite;

            //     /// Create a new bitmap and copy the original to this new with the given pixel size.
            //     Bitmap scaledBitmap = new Bitmap(spriteToScale.Size.X * pixelSize.X,
            //                                      spriteToScale.Size.Y * pixelSize.Y,
            //                                      PixelFormat.Format24bppRgb);
            //     MonoGameBitmapUtils.CopyBitmapScaled(spriteToScale.RawBitmap, scaledBitmap, spriteToScale.PixelSize, pixelSize);

            //     /// Create the MonoGameSprite object and register it to this sprite manager.
            //     MonoGameSprite newSprite = new MonoGameSprite(scaledBitmap, pixelSize, this.platform);
            //     newSprite.TransparentColor = sprite.TransparentColor;
            //     this.sprites.Add(newSprite);

            //     TraceManager.WriteAllTrace("MonoGameSpriteManager.ScaleSprite: Sprite created", MonoGameTraceFilters.INFO);
            //     return newSprite;
            // }
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

            // TODO: implement!
            throw new NotImplementedException();

            // lock (this.lockObj)
            // {
            //     /// Search the sprite in the list.
            //     MonoGameSprite spriteToShrink = (MonoGameSprite)sprite;

            //     /// Create a copy of the original bitmap with pixelsize (1, 1) if necessary.
            //     Bitmap bitmapToShrink;
            //     if (spriteToShrink.PixelSize != new RCIntVector(1, 1))
            //     {
            //         bitmapToShrink = new Bitmap(spriteToShrink.Size.X, spriteToShrink.Size.Y, PixelFormat.Format24bppRgb);
            //         MonoGameBitmapUtils.CopyBitmapScaled(spriteToShrink.RawBitmap, bitmapToShrink, spriteToShrink.PixelSize, new RCIntVector(1, 1));
            //     }
            //     else
            //     {
            //         bitmapToShrink = spriteToShrink.RawBitmap;
            //     }

            //     /// Create the shrinked bitmap with pixelsize (1, 1).
            //     Bitmap shrinkedBitmap = new Bitmap(spriteSize.X, spriteSize.Y, PixelFormat.Format24bppRgb);
            //     Graphics gc = Graphics.FromImage(shrinkedBitmap);
            //     gc.InterpolationMode = InterpolationMode.NearestNeighbor;
            //     gc.DrawImage(bitmapToShrink, new Rectangle(0, 0, spriteSize.X, spriteSize.Y), new Rectangle(0, 0, bitmapToShrink.Width, bitmapToShrink.Height), GraphicsUnit.Pixel);
            //     gc.Dispose();

            //     /// Scale the shrinked bitmap to the target pixel size if necessary.
            //     Bitmap scaledShrinkedBitmap;
            //     if (pixelSize != new RCIntVector(1, 1))
            //     {
            //         scaledShrinkedBitmap = new Bitmap(shrinkedBitmap.Width*pixelSize.X,
            //                                           shrinkedBitmap.Height*pixelSize.Y,
            //                                           PixelFormat.Format24bppRgb);
            //         MonoGameBitmapUtils.CopyBitmapScaled(shrinkedBitmap, scaledShrinkedBitmap, new RCIntVector(1, 1), pixelSize);
            //     }
            //     else
            //     {
            //         scaledShrinkedBitmap = shrinkedBitmap;
            //     }

            //     /// Create the MonoGameSprite object and register it to this sprite manager.
            //     MonoGameSprite shrinkedSprite = new MonoGameSprite(scaledShrinkedBitmap, pixelSize, this.platform);
            //     shrinkedSprite.TransparentColor = sprite.TransparentColor;
            //     this.sprites.Add(shrinkedSprite);

            //     /// Cleanup if necessary.
            //     if (bitmapToShrink != spriteToShrink.RawBitmap) { bitmapToShrink.Dispose(); }
            //     if (shrinkedBitmap != scaledShrinkedBitmap) { shrinkedBitmap.Dispose(); }

            //     TraceManager.WriteAllTrace("MonoGameSpriteManager.ShrinkedSprite: Sprite shrinked", MonoGameTraceFilters.INFO);
            //     return shrinkedSprite;
            // }
        }

        /// <see cref="UISpriteManagerBase.CreateRenderContext_i"/>
        protected override IUIRenderContext CreateRenderContext_i(UISprite sprite)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("MonoGameSpriteManager"); }

            // TODO: implement!
            throw new NotImplementedException();


            // lock (this.lockObj)
            // {
            //     MonoGameSprite target = (MonoGameSprite)sprite;
            //     if (this.renderContexts.ContainsKey(target))
            //     {
            //         throw new UIException("The given sprite has already an active render context!");
            //     }
            //     MonoGameSpriteRenderContext targetContext = new MonoGameSpriteRenderContext(target, this);
            //     this.renderContexts.Add(target, targetContext);
            //     TraceManager.WriteAllTrace("MonoGameSpriteManager.CreateRenderContext: Render context for sprite created", MonoGameTraceFilters.INFO);
            //     return targetContext;
            // }
        }

        /// <see cref="UISpriteManagerBase.CloseRenderContext"/>
        public override void CloseRenderContext(UISprite sprite)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("MonoGameSpriteManager"); }

            // TODO: implement!
            throw new NotImplementedException();

            // lock (this.lockObj)
            // {
            //     MonoGameSprite target = (MonoGameSprite)sprite;
            //     if (!this.renderContexts.ContainsKey(target))
            //     {
            //         throw new UIException("The given sprite doesn't have active render context!");
            //     }
            //     MonoGameSpriteRenderContext targetContext = this.renderContexts[target];
            //     this.renderContexts.Remove(target);
            //     targetContext.Close();
            //     TraceManager.WriteAllTrace("MonoGameSpriteManager.CloseRenderContext: Render context for sprite closed", MonoGameTraceFilters.INFO);
            // }
        }

        /// <see cref="UISpriteManagerBase.DestroySprite"/>
        public override void DestroySprite(UISprite sprite)
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("MonoGameSpriteManager"); }

            // TODO: implement!
            throw new NotImplementedException();

            // lock (this.lockObj)
            // {
            //     MonoGameSprite spriteToDestroy = (MonoGameSprite)sprite;
            //     if (this.renderContexts.ContainsKey(spriteToDestroy))
            //     {
            //         throw new UIException("The given sprite still has active render context!");
            //     }

            //     /// Remove the sprite from the list and destroy it.
            //     if (!this.sprites.Remove(spriteToDestroy)) { throw new UIException("The given sprite has already been disposed or has not been created by this sprite manager!"); }
            //     spriteToDestroy.Dispose();
            //     TraceManager.WriteAllTrace("MonoGameSpriteManager.DestroySprite: Sprite destroyed", MonoGameTraceFilters.INFO);
            // }
        }

        /// <see cref="UISpriteManagerBase.Dispose_i"/>
        protected override void Dispose_i()
        {
            if (this.ObjectDisposed) { throw new ObjectDisposedException("MonoGameSpriteManager"); }

            // TODO: implement!
            throw new NotImplementedException();

            // lock (this.lockObj)
            // {
            //     /// Close every opened sprite render contexts
            //     TraceManager.WriteAllTrace("Closing sprite render contexts", MonoGameTraceFilters.INFO);
            //     foreach (KeyValuePair<MonoGameSprite, MonoGameSpriteRenderContext> item in this.renderContexts)
            //     {
            //         item.Value.Close();
            //     }
            //     this.renderContexts.Clear();

            //     /// Destroy every created sprites
            //     TraceManager.WriteAllTrace("Destroying sprites", MonoGameTraceFilters.INFO);
            //     foreach (MonoGameSprite sprite in this.sprites)
            //     {
            //         if (sprite != null) { sprite.Dispose(); }
            //     }
            //     this.sprites.Clear();
            // }
        }

        #endregion UISpriteManagerBase overrides

        /// <summary>
        /// Forces uploading the sprites to the graphics device.
        /// </summary>
        public void SecondChanceUploadSprites()
        {
            // TODO: implement!
            throw new NotImplementedException();

            // TraceManager.WriteAllTrace("Uploading sprites to the graphics device", MonoGameTraceFilters.INFO);
            // lock (this.lockObj)
            // {
            //     foreach (MonoGameSprite sprite in this.sprites)
            //     {
            //         if (sprite.IsUploaded && sprite.XnaTexture == null) { sprite.SecondChanceUpload(); }
            //     }
            // }
        }

        // /// <summary>
        // /// List of the sprites created by this sprite manager.
        // /// </summary>
        // private RCSet<MonoGameSprite> sprites;

        // /// <summary>
        // /// List of the sprite render contexts created by this sprite manager.
        // /// </summary>
        // private Dictionary<MonoGameSprite, MonoGameSpriteRenderContext> renderContexts;

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
