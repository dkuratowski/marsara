﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using RC.Common;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using RC.Common.Diagnostics;

namespace RC.UI.MonoGamePlugin
{
    /// <summary>
    /// This implementation of the UISprite uses XNA-textures and System.Drawing.Bitmaps for representing
    /// the image data.
    /// </summary>
    class MonoGameSprite : UISprite, IDisposable
    {
        /// <summary>
        /// Constructs an MonoGameSprite object.
        /// </summary>
        /// <param name="rawBitmap">The underlying System.Drawing.Bitmap of this MonoGameSprite.</param>
        /// <param name="pixelSize">The pixel size of this MonoGameSprite.</param>
        public MonoGameSprite(/*Bitmap rawBitmap, */RCIntVector pixelSize, MonoGameGraphicsPlatform platform)
            : base(/*rawBitmap.Width / pixelSize.X, rawBitmap.Height / pixelSize.Y*/10, 10, pixelSize)
        {
            this.isLocked = false;
            // this.rawBitmap = rawBitmap;
            // this.transparentBitmap = null;
            this.isUploaded = false;
            this.xnaTexture = null;
            this.platform = platform;
        }

        // /// <summary>
        // /// Gets the underlying System.Drawing.Bitmap of this MonoGameSprite.
        // /// </summary>
        // public Bitmap RawBitmap
        // {
        //     get
        //     {
        //         if (this.isLocked) { throw new UIException("Sprite is locked"); }
        //         return this.rawBitmap;
        //     }
        // }

        // /// <summary>
        // /// Gets the transparent version of the underlying System.Drawing.Bitmap or null if there is no
        // /// transparent color has been set.
        // /// </summary>
        // public Bitmap TransparentBitmap
        // {
        //     get
        //     {
        //         if (this.isLocked) { throw new UIException("Sprite is locked"); }
        //         return this.transparentBitmap;
        //     }
        // }

        /// <summary>
        /// Gets the 2D texture of the underlying System.Drawing.Bitmap uploaded to the video card.
        /// </summary>
        public Texture2D XnaTexture
        {
            get
            {
                if (this.isLocked) { throw new UIException("Sprite is locked"); }
                return this.xnaTexture;
            }
        }

        #region UISprite overrides

        /// <see cref="UISprite.IsUploaded"/>
        public override bool IsUploaded
        {
            get { return this.isUploaded; }
        }

        /// <see cref="UISprite.TransparentColor_set"/>
        protected override void TransparentColor_set(RCColor newColor)
        {
            if (this.isLocked) { throw new UIException("Sprite is locked"); }

            if (newColor == RCColor.Undefined)
            {
                // /// The transparent bitmap should be deleted and the original bitmap has to be loaded.
                // this.transparentBitmap.Dispose();
                // this.transparentBitmap = null;
                TraceManager.WriteAllTrace("MonoGameSprite: transparent bitmap destroyed", MonoGameTraceFilters.DETAILS);
            }
            else
            {
                /// The transparent bitmap should be replaced and has to be loaded.
                // if (this.transparentBitmap != null) { this.transparentBitmap.Dispose(); }
                // this.transparentBitmap = new Bitmap(this.rawBitmap.Width, this.rawBitmap.Height, PixelFormat.Format24bppRgb);
                // MonoGameBitmapUtils.CopyBitmapScaled(this.rawBitmap, this.transparentBitmap, this.PixelSize, this.PixelSize);
                // this.transparentBitmap.MakeTransparent(Color.FromArgb(newColor.R, newColor.G, newColor.B));
                TraceManager.WriteAllTrace("MonoGameSprite: transparent bitmap replaced", MonoGameTraceFilters.DETAILS);
            }
        }

        /// <see cref="UISprite.Upload_i"/>
        protected override void Upload_i()
        {
            if (this.isLocked) { throw new UIException("Sprite is locked"); }

            GraphicsDevice device = this.platform.Device;
            if (device != null)
            {
                // Bitmap bmpToUpload = this.transparentBitmap == null ? this.rawBitmap : this.transparentBitmap;
                MemoryStream stream = new MemoryStream();
                // bmpToUpload.Save(stream, ImageFormat.Png);

                lock (device)
                {
                    this.xnaTexture = Texture2D.FromStream(device, stream);
                }

                // stream.Close();
                // if (bmpToUpload == this.transparentBitmap)
                // {
                //     TraceManager.WriteAllTrace("MonoGameSprite: transparent bitmap uploaded", MonoGameTraceFilters.DETAILS);
                // }
                // else
                // {
                    TraceManager.WriteAllTrace("MonoGameSprite: raw bitmap uploaded", MonoGameTraceFilters.DETAILS);
                // }
            }
            else
            {
                TraceManager.WriteAllTrace("MonoGameSprite: failed to upload", MonoGameTraceFilters.DETAILS);
            }

            this.isUploaded = true;
        }

        /// <see cref="UISprite.Download_i"/>
        protected override void Download_i()
        {
            if (this.xnaTexture != null)
            {
                lock (this.platform.Device)
                {
                    // this.xnaTexture.Dispose();
                }

                this.xnaTexture = null;
                TraceManager.WriteAllTrace("MonoGameSprite: XNA-texture destroyed", MonoGameTraceFilters.DETAILS);
            }

            this.isUploaded = false;
        }

        /// <see cref="UISprite.Save"/>
        public override void Save(string fileName)
        {
            if (this.isLocked) { throw new UIException("Sprite is locked"); }
            if (fileName == null || fileName.Length == 0) { throw new ArgumentNullException("fileName"); }

            // Bitmap bmpToSave = this.transparentBitmap == null ? this.rawBitmap : this.transparentBitmap;
            // bmpToSave.Save(fileName, ImageFormat.Png);
        }

        /// <see cref="UISprite.Save"/>
        public override byte[] Save()
        {
            if (this.isLocked) { throw new UIException("Sprite is locked"); }

            // Bitmap bmpToSave = this.transparentBitmap == null ? this.rawBitmap : this.transparentBitmap;
            // using (MemoryStream outputStream = new MemoryStream())
            // {
            //     bmpToSave.Save(outputStream, ImageFormat.Png);
            //     return outputStream.ToArray();
            // }
            return new byte[0];
        }

        #endregion UISprite overrides

        #region IDisposable Members

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            if (this.isLocked) { throw new UIException("Sprite is locked"); }

            // this.rawBitmap.Dispose();
            // this.rawBitmap = null;
            TraceManager.WriteAllTrace("MonoGameSprite: raw bitmap destroyed", MonoGameTraceFilters.DETAILS);

            // if (this.transparentBitmap != null)
            // {
                // this.transparentBitmap.Dispose();
                // this.transparentBitmap = null;
                TraceManager.WriteAllTrace("MonoGameSprite: transparent bitmap destroyed", MonoGameTraceFilters.DETAILS);
            // }

            if (this.xnaTexture != null)
            {
                lock (this.platform.Device)
                {
                    // this.xnaTexture.Dispose();
                }

                this.xnaTexture = null;
                TraceManager.WriteAllTrace("MonoGameSprite: XNA-texture destroyed", MonoGameTraceFilters.DETAILS);
            }
        }

        #endregion IDisposable Members

        /// <summary>
        /// Lock function for the corresponding render context.
        /// </summary>
        public void Lock()
        {
            if (this.isLocked) { throw new UIException("Sprite already locked!"); }
            this.isLocked = true;
            TraceManager.WriteAllTrace("MonoGameSprite: locked", MonoGameTraceFilters.DETAILS);
        }

        /// <summary>
        /// Unlock function for the corresponding render context.
        /// </summary>
        public void Unlock()
        {
            if (!this.isLocked) { throw new UIException("Sprite already unlocked!"); }
            this.isLocked = false;
            TraceManager.WriteAllTrace("MonoGameSprite: unlocked", MonoGameTraceFilters.DETAILS);
        }

        /// <summary>
        /// Gives a second chance to upload this sprite. If upload fails again, this method throws an exception.
        /// </summary>
        public void SecondChanceUpload()
        {
            if (!this.isUploaded) { throw new InvalidOperationException("This sprite was not uploaded to the device!"); }
            if (this.xnaTexture != null) { throw new InvalidOperationException("This sprite has already been uploaded to the device!"); }

            this.Upload_i();
            if (this.xnaTexture == null) { throw new UIException("MonoGameSprite: Second chance upload failed!"); }
        }

        /// <summary>
        /// Lock flag for render contexts.
        /// </summary>
        private bool isLocked;

        // /// <summary>
        // /// The underlying System.Drawing.Bitmap of this MonoGameSprite.
        // /// </summary>
        // private Bitmap rawBitmap;

        // /// <summary>
        // /// The transparent version of the underlying System.Drawing.Bitmap or null if there is no
        // /// transparent color has been set.
        // /// </summary>
        // private Bitmap transparentBitmap;

        /// <summary>
        /// The 2D texture of the underlying System.Drawing.Bitmap uploaded to the video card.
        /// </summary>
        private Texture2D xnaTexture;

        /// <summary>
        /// This flag indicates whether this MonoGameSprite has been uploaded to the graphics device or not.
        /// </summary>
        private bool isUploaded;

        /// <summary>
        /// Reference to the platform.
        /// </summary>
        private MonoGameGraphicsPlatform platform;
    }
}
