using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Common.Diagnostics;

namespace RC.UI
{
    /// <summary>
    /// Interface for loading/unloading and handling sprites.
    /// </summary>
    public interface IUISpriteManager
    {
        /// <summary>
        /// Creates an empty 2D sprite that is filled with the given color.
        /// </summary>
        /// <param name="color">The color to fill the created sprite.</param>
        /// <param name="spriteSize">The size of the sprite to create.</param>
        /// <returns>The created UISprite.</returns>
        UISprite CreateSprite(RCColor color, RCIntVector spriteSize);

        /// <summary>
        /// Creates an empty 2D sprite with the given pixel size that is filled with the given color.
        /// </summary>
        /// <param name="color">The color to fill the created sprite.</param>
        /// <param name="spriteSize">The size of the sprite to create.</param>
        /// <param name="pixelSize">The pixel size of the created sprite.</param>
        /// <returns>The created UISprite.</returns>
        UISprite CreateSprite(RCColor color, RCIntVector spriteSize, RCIntVector pixelSize);

        /// <summary>
        /// Loads a 2D sprite from the given file with the original pixel size.
        /// </summary>
        /// <param name="fileName">The name of the file to load from.</param>
        /// <returns>The loaded UISprite or null if failed to load the sprite.</returns>
        UISprite LoadSprite(string fileName);

        /// <summary>
        /// Loads a 2D sprite from the given file with the given pixel size.
        /// </summary>
        /// <param name="fileName">The name of the file to load from.</param>
        /// <param name="pixelSize">The pixel size of the sprite.</param>
        /// <returns>The loaded and scaled UISprite or null if failed to load the sprite.</returns>
        UISprite LoadSprite(string fileName, RCIntVector pixelSize);

        /// <summary>
        /// Loads a 2D sprite from the given image data with the original pixel size.
        /// </summary>
        /// <param name="imageData">The byte sequence that contains the image data.</param>
        /// <returns>The loaded UISprite or null if failed to load the sprite.</returns>
        UISprite LoadSprite(byte[] imageData);

        /// <summary>
        /// Loads a 2D sprite from the given image data with the given pixel size.
        /// </summary>
        /// <param name="imageData">The byte sequence that contains the image data.</param>
        /// <param name="pixelSize">The pixel size of the sprite.</param>
        /// <returns>The loaded and scaled UISprite or null if failed to load the sprite.</returns>
        UISprite LoadSprite(byte[] imageData, RCIntVector pixelSize);

        /// <summary>
        /// Makes a scaled copy of the given 2D sprite with the given pixel size.
        /// </summary>
        /// <param name="sprite">The original sprite to scale.</param>
        /// <param name="pixelSize">The pixel size of the returned sprite.</param>
        /// <returns>The scaled copy of the UISprite or null if failed to scale the sprite.</returns>
        UISprite ScaleSprite(UISprite sprite, RCIntVector pixelSize);

        /// <summary>
        /// Makes a copy of the given 2D sprite shrinked to the given sprite size. The pixel size of the shrinked sprite will be the
        /// same as the pixel size of the original sprite.
        /// </summary>
        /// <param name="sprite">The original sprite to shrink.</param>
        /// <param name="spriteSize">The size of the shrinked sprite.</param>
        /// <returns>The copy of the given UISprite shrinked to the given sprite size or null if failed to shrink the sprite.</returns>
        UISprite ShrinkSprite(UISprite sprite, RCIntVector spriteSize);

        /// <summary>
        /// Makes a copy of the given 2D sprite shrinked to the given sprite size.
        /// </summary>
        /// <param name="sprite">The original sprite to shrink.</param>
        /// <param name="spriteSize">The size of the shrinked sprite.</param>
        /// <param name="pixelSize">The pixel size of the shrinked sprite.</param>
        /// <returns>The copy of the given UISprite shrinked to the given sprite size or null if failed to shrink the sprite.</returns>
        UISprite ShrinkSprite(UISprite sprite, RCIntVector spriteSize, RCIntVector pixelSize);

        /// <summary>
        /// Creates a render context for the given sprite.
        /// </summary>
        /// <param name="sprite">The sprite to create render context for.</param>
        /// <returns>The created render context or null if failed.</returns>
        IUIRenderContext CreateRenderContext(UISprite sprite);

        /// <summary>
        /// Closes the render context of the given sprite.
        /// </summary>
        /// <param name="sprite">The sprite to close the render context for.</param>
        void CloseRenderContext(UISprite sprite);

        /// <summary>
        /// Destroys the given sprite.
        /// </summary>
        /// <param name="sprite">The sprite to destroy.</param>
        void DestroySprite(UISprite sprite);
    }

    /// <summary>
    /// The abstract base class of the implementation of the sprite manager.
    /// </summary>
    public abstract class UISpriteManagerBase : IUISpriteManager, IDisposable
    {
        /// <summary>
        /// Constructs a UISpriteManagerBase object.
        /// </summary>
        public UISpriteManagerBase()
        {
            this.objectDisposed = false;
        }

        #region IUISpriteManager members

        /// <see cref="IUISpriteManager.CreateSprite"/>
        public abstract UISprite CreateSprite(RCColor color, RCIntVector spriteSize);

        /// <see cref="IUISpriteManager.CreateSprite"/>
        public abstract UISprite CreateSprite(RCColor color, RCIntVector spriteSize, RCIntVector pixelSize);

        /// <see cref="IUISpriteManager.LoadSprite"/>
        public abstract UISprite LoadSprite(string fileName);

        /// <see cref="IUISpriteManager.LoadSprite"/>
        public abstract UISprite LoadSprite(string fileName, RCIntVector pixelSize);

        /// <see cref="IUISpriteManager.LoadSprite"/>
        public abstract UISprite LoadSprite(byte[] imageData);

        /// <see cref="IUISpriteManager.LoadSprite"/>
        public abstract UISprite LoadSprite(byte[] imageData, RCIntVector pixelSize);

        /// <see cref="IUISpriteManager.ScaleSprite"/>
        public abstract UISprite ScaleSprite(UISprite sprite, RCIntVector pixelSize);

        /// <see cref="IUISpriteManager.ShrinkSprite"/>
        public abstract UISprite ShrinkSprite(UISprite sprite, RCIntVector spriteSize);

        /// <see cref="IUISpriteManager.ShrinkSprite"/>
        public abstract UISprite ShrinkSprite(UISprite sprite, RCIntVector spriteSize, RCIntVector pixelSize);

        /// <see cref="IUISpriteManager.CreateRenderContext"/>
        public IUIRenderContext CreateRenderContext(UISprite sprite)
        {
            if (sprite.IsUploaded) { throw new InvalidOperationException("Unable to create render context on an uploaded sprite!"); }
            return this.CreateRenderContext_i(sprite);
        }
        protected abstract IUIRenderContext CreateRenderContext_i(UISprite sprite);

        /// <see cref="IUISpriteManager.CloseRenderContext"/>
        public abstract void CloseRenderContext(UISprite sprite);

        /// <see cref="IUISpriteManager.DestroySprite"/>
        public abstract void DestroySprite(UISprite sprite);

        #endregion IUISpriteManager members

        #region IDisposable members

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            if (this.objectDisposed) { throw new ObjectDisposedException("UISpriteManagerBase"); }

            TraceManager.WriteAllTrace("Destroying sprite manager", UITraceFilters.INFO);
            this.Dispose_i();
            this.objectDisposed = true;
        }

        #endregion IDisposable members

        /// <summary>
        /// Internal function for performing dispose operations in the derived classes. The default implementation is empty.
        /// </summary>
        protected virtual void Dispose_i() { }

        /// <summary>
        /// Gets whether this object has been disposed or not.
        /// </summary>
        protected bool ObjectDisposed { get { return this.objectDisposed; } }

        /// <summary>
        /// This flag indicates whether this object has been disposed or not.
        /// </summary>
        private bool objectDisposed;
    }
}
