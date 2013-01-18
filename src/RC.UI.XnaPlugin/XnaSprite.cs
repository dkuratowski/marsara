using System;
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

namespace RC.UI.XnaPlugin
{
    /// <summary>
    /// This implementation of the UISprite uses XNA-textures and System.Drawing.Bitmaps for representing
    /// the image data.
    /// </summary>
    class XnaSprite : UISprite, IDisposable
    {
        /// <summary>
        /// Constructs an XnaSprite object.
        /// </summary>
        /// <param name="rawBitmap">The underlying System.Drawing.Bitmap of this XnaSprite.</param>
        /// <param name="resourceId">The assigned resource ID of this XnaSprite.</param>
        /// <param name="pixelSize">The pixel size of this XnaSprite.</param>
        public XnaSprite(Bitmap rawBitmap, int resourceId, RCIntVector pixelSize, XnaGraphicsPlatform platform)
            : base(rawBitmap.Width / pixelSize.X, rawBitmap.Height / pixelSize.Y, pixelSize)
        {
            this.isLocked = false;
            this.rawBitmap = rawBitmap;
            this.resourceId = resourceId;
            this.transparentBitmap = null;
            this.platform = platform;
            this.Upload();
        }

        /// <summary>
        /// Gets the underlying System.Drawing.Bitmap of this XnaSprite.
        /// </summary>
        public Bitmap RawBitmap
        {
            get
            {
                if (this.isLocked) { throw new UIException("Sprite is locked"); }
                return this.rawBitmap;
            }
        }

        /// <summary>
        /// Gets the transparent version of the underlying System.Drawing.Bitmap or null if there is no
        /// transparent color has been set.
        /// </summary>
        public Bitmap TransparentBitmap
        {
            get
            {
                if (this.isLocked) { throw new UIException("Sprite is locked"); }
                return this.transparentBitmap;
            }
        }

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

        /// <see cref="UISprite.ResourceId"/>
        public override int ResourceId
        {
            get
            {
                //if (this.isLocked) { throw new UIException("Sprite is locked"); }
                return this.resourceId;
            }
        }

        /// <see cref="UISprite.TransparentColor_set"/>
        protected override void TransparentColor_set(UIColor newColor)
        {
            if (this.isLocked) { throw new UIException("Sprite is locked"); }

            if (newColor == UIColor.Undefined)
            {
                /// The transparent bitmap should be deleted and the original bitmap has to be loaded.
                this.transparentBitmap.Dispose();
                this.transparentBitmap = null;
                TraceManager.WriteAllTrace(string.Format("Sprite({0}): transparent bitmap destroyed", this.resourceId), XnaTraceFilters.DETAILS);
            }
            else
            {
                /// The transparent bitmap should be replaced and has to be loaded.
                if (this.transparentBitmap != null) { this.transparentBitmap.Dispose(); }
                this.transparentBitmap = new Bitmap(this.rawBitmap.Width, this.rawBitmap.Height, PixelFormat.Format24bppRgb);
                XnaBitmapUtils.CopyBitmapScaled(this.rawBitmap, this.transparentBitmap, this.PixelSize, this.PixelSize);
                this.transparentBitmap.MakeTransparent(Color.FromArgb(newColor.R, newColor.G, newColor.B));
                TraceManager.WriteAllTrace(string.Format("Sprite({0}): transparent bitmap replaced", this.resourceId), XnaTraceFilters.DETAILS);
            }

            /// Upload to the graphics device.
            this.Upload();
        }

        #endregion UISprite overrides

        #region IDisposable Members

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            if (this.isLocked) { throw new UIException("Sprite is locked"); }

            this.rawBitmap.Dispose();
            this.rawBitmap = null;
            TraceManager.WriteAllTrace(string.Format("Sprite({0}): raw bitmap destroyed", this.resourceId), XnaTraceFilters.DETAILS);

            if (this.transparentBitmap != null)
            {
                this.transparentBitmap.Dispose();
                this.transparentBitmap = null;
                TraceManager.WriteAllTrace(string.Format("Sprite({0}): transparent bitmap destroyed", this.resourceId), XnaTraceFilters.DETAILS);
            }

            if (this.xnaTexture != null)
            {
                this.xnaTexture.Dispose();
                this.xnaTexture = null;
                TraceManager.WriteAllTrace(string.Format("Sprite({0}): XNA texture destroyed", this.resourceId), XnaTraceFilters.DETAILS);
            }
        }

        #endregion

        /// <summary>
        /// Uploads this sprite to the graphics device if the device is currently available.
        /// </summary>
        public void Upload()
        {
            if (this.isLocked) { throw new UIException("Sprite is locked"); }

            if (this.xnaTexture != null) { this.xnaTexture.Dispose(); }
            GraphicsDevice device = this.platform.Device;
            if (device != null)
            {
                Bitmap bmpToUpload = this.transparentBitmap == null ? this.rawBitmap : this.transparentBitmap;
                MemoryStream stream = new MemoryStream();
                bmpToUpload.Save(stream, ImageFormat.Png);
                this.xnaTexture = Texture2D.FromStream(device, stream);
                stream.Close();
                if (bmpToUpload == this.transparentBitmap)
                {
                    TraceManager.WriteAllTrace(string.Format("Sprite({0}): transparent bitmap uploaded", this.resourceId), XnaTraceFilters.DETAILS);
                }
                else
                {
                    TraceManager.WriteAllTrace(string.Format("Sprite({0}): raw bitmap uploaded", this.resourceId), XnaTraceFilters.DETAILS);
                }
            }
            else
            {
                TraceManager.WriteAllTrace(string.Format("Unable to upload sprite({0}). Device not found", this.resourceId), XnaTraceFilters.DETAILS);
            }
        }

        /// <see cref="UISprite.Save"/>
        public override void Save(string fileName)
        {
            if (this.isLocked) { throw new UIException("Sprite is locked"); }
            if (fileName == null || fileName.Length == 0) { throw new ArgumentNullException("fileName"); }

            Bitmap bmpToSave = this.transparentBitmap == null ? this.rawBitmap : this.transparentBitmap;
            bmpToSave.Save(fileName, ImageFormat.Png);
        }

        /// <summary>
        /// Lock function for the corresponding render context.
        /// </summary>
        public void Lock()
        {
            if (this.isLocked) { throw new UIException("Sprite already locked!"); }
            this.isLocked = true;
            TraceManager.WriteAllTrace(string.Format("Sprite({0}) locked", this.resourceId), XnaTraceFilters.DETAILS);
        }

        /// <summary>
        /// Unlock function for the corresponding render context.
        /// </summary>
        public void Unlock()
        {
            if (!this.isLocked) { throw new UIException("Sprite already unlocked!"); }
            this.isLocked = false;
            TraceManager.WriteAllTrace(string.Format("Sprite({0}) unlocked", this.resourceId), XnaTraceFilters.DETAILS);
        }

        /// <summary>
        /// Lock flag for render contexts.
        /// </summary>
        private bool isLocked;

        /// <summary>
        /// The underlying System.Drawing.Bitmap of this XnaSprite.
        /// </summary>
        private Bitmap rawBitmap;

        /// <summary>
        /// The transparent version of the underlying System.Drawing.Bitmap or null if there is no
        /// transparent color has been set.
        /// </summary>
        private Bitmap transparentBitmap;

        /// <summary>
        /// The 2D texture of the underlying System.Drawing.Bitmap uploaded to the video card.
        /// </summary>
        private Texture2D xnaTexture;

        /// <summary>
        /// The assigned resource ID of this XnaSprite.
        /// </summary>
        private int resourceId;

        /// <summary>
        /// Reference to the platform.
        /// </summary>
        private XnaGraphicsPlatform platform;
    }
}
