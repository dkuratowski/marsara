using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using RC.Common.Diagnostics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace RC.UI.MonoGamePlugin
{
    /// <summary>
    /// This implementation of the UISprite uses MonoGame textures and SixLabors.ImageSharp.Images for representing
    /// the image data.
    /// </summary>
    class MonoGameSprite : UISprite, IDisposable
    {
        /// <summary>
        /// Constructs an MonoGameSprite object.
        /// </summary>
        /// <param name="rawImage">The underlying SixLabors.ImageSharp.Image of this MonoGameSprite.</param>
        /// <param name="pixelSize">The pixel size of this MonoGameSprite.</param>
        public MonoGameSprite(Image<Rgb24> rawImage, RCIntVector pixelSize, MonoGameGraphicsPlatform platform)
            : base(rawImage.Width / pixelSize.X, rawImage.Height / pixelSize.Y, pixelSize)
        {
            this.isLocked = false;
            this.rawImage = rawImage;
            this.transparentImage = null;
            this.isUploaded = false;
            this.xnaTexture = null;
            this.platform = platform;
        }

        /// <summary>
        /// Gets the underlying SixLabors.ImageSharp.Image of this MonoGameSprite.
        /// </summary>
        public Image<Rgb24> RawImage
        {
            get
            {
                if (this.isLocked) { throw new UIException("Sprite is locked"); }
                return this.rawImage;
            }
        }

        /// <summary>
        /// Gets the transparent version of the underlying SixLabors.ImageSharp.Image or null if there is no
        /// transparent color has been set.
        /// </summary>
        public Image<Rgba32> TransparentImage
        {
            get
            {
                if (this.isLocked) { throw new UIException("Sprite is locked"); }
                return this.transparentImage;
            }
        }

        /// <summary>
        /// Gets the 2D texture of the underlying SixLabors.ImageSharp.Image uploaded to the video card.
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
                /// The transparent image should be deleted and the original image has to be loaded.
                this.transparentImage.Dispose();
                this.transparentImage = null;
                TraceManager.WriteAllTrace("MonoGameSprite: transparent bitmap destroyed", MonoGameTraceFilters.DETAILS);
            }
            else
            {
                /// The transparent image should be replaced and has to be loaded.
                if (this.transparentImage != null) { this.transparentImage.Dispose(); }
                this.transparentImage = new Image<Rgba32>(
                    this.rawImage.Size().Width,
                    this.rawImage.Size().Height,
                    new Rgba32(0, 0, 0, 0)
                );
                MonoGameImageUtils.MakeImageTransparent(this.rawImage, this.transparentImage, newColor);
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
                Image imageToUpload = this.transparentImage == null ? this.rawImage : this.transparentImage;
                MemoryStream stream = new MemoryStream();
                imageToUpload.SaveAsPng(stream);

                lock (device)
                {
                    this.xnaTexture = Texture2D.FromStream(device, stream);
                }

                stream.Close();
                if (imageToUpload == this.transparentImage)
                {
                    TraceManager.WriteAllTrace("MonoGameSprite: transparent bitmap uploaded", MonoGameTraceFilters.DETAILS);
                }
                else
                {
                    TraceManager.WriteAllTrace("MonoGameSprite: raw bitmap uploaded", MonoGameTraceFilters.DETAILS);
                }
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
                    this.xnaTexture.Dispose();
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

            Image imageToSave = this.transparentImage == null ? this.rawImage : this.transparentImage;
            imageToSave.SaveAsPng(fileName);
        }

        /// <see cref="UISprite.Save"/>
        public override byte[] Save()
        {
            if (this.isLocked) { throw new UIException("Sprite is locked"); }

            Image imageToSave = this.transparentImage == null ? this.rawImage : this.transparentImage;
            using (MemoryStream outputStream = new MemoryStream())
            {
                imageToSave.SaveAsPng(outputStream);
                return outputStream.ToArray();
            }
        }

        #endregion UISprite overrides

        #region IDisposable Members

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            if (this.isLocked) { throw new UIException("Sprite is locked"); }

            this.rawImage.Dispose();
            this.rawImage = null;
            TraceManager.WriteAllTrace("MonoGameSprite: raw bitmap destroyed", MonoGameTraceFilters.DETAILS);

            if (this.transparentImage != null)
            {
                this.transparentImage.Dispose();
                this.transparentImage = null;
                TraceManager.WriteAllTrace("MonoGameSprite: transparent bitmap destroyed", MonoGameTraceFilters.DETAILS);
            }

            if (this.xnaTexture != null)
            {
                lock (this.platform.Device)
                {
                    this.xnaTexture.Dispose();
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

        /// <summary>
        /// The underlying SixLabors.ImageSharp.Image of this MonoGameSprite.
        /// </summary>
        private Image<Rgb24> rawImage;

        /// <summary>
        /// The transparent version of the underlying SixLabors.ImageSharp.Image or null if there is no
        /// transparent color has been set.
        /// </summary>
        private Image<Rgba32> transparentImage;

        /// <summary>
        /// The 2D texture of the underlying SixLabors.ImageSharp.Image uploaded to the video card.
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
