﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading;
using System.IO;
using RC.Common;

namespace RC.UI.MonoGamePlugin
{
    /// <summary>
    /// The MonoGame implementation of the render loop.
    /// </summary>
    class MonoGameRenderLoop : UIRenderLoopBase
    {
        /// <summary>
        /// Constructs an MonoGameRenderLoop object.
        /// </summary>
        /// <param name="platform">Reference to the platform.</param>
        public MonoGameRenderLoop(MonoGameGraphicsPlatform platform)
        {
            this.mouseInputDevice = new MonoGameMouseInputDevice(platform);
            this.keyboardInputDevice = new MonoGameKeyboardInputDevice();
            UIRoot.Instance.RegisterMouseInputDevice(this.mouseInputDevice);
            UIRoot.Instance.RegisterKeyboardInputDevice(this.keyboardInputDevice);

            List<MonoGameRenderLoopImpl.UpdateDlgt> updateFunctions = new List<MonoGameRenderLoopImpl.UpdateDlgt>();
            List<MonoGameRenderLoopImpl.RenderDlgt> renderFunctions = new List<MonoGameRenderLoopImpl.RenderDlgt>();
            List<MonoGameRenderLoopImpl.InitializeDlgt> initFunctions = new List<MonoGameRenderLoopImpl.InitializeDlgt>();
            updateFunctions.Add(this.mouseInputDevice.Update);
            updateFunctions.Add(this.keyboardInputDevice.Update);
            updateFunctions.Add(this.Update);
            renderFunctions.Add(this.Render);
            initFunctions.Add(this.Initialize);
            
            this.implementation = new MonoGameRenderLoopImpl(updateFunctions, renderFunctions, initFunctions);
            this.platform = platform;
        }

        /// <summary>
        /// Gets the current graphics device.
        /// </summary>
        public GraphicsDevice Device
        {
            get
            {
                if (this.ObjectDisposed) { throw new ObjectDisposedException("MonoGameRenderLoop"); }
                return this.implementation.GraphicsDevice;
            }
        }

        /// <summary>
        /// Gets the wrapper object of the main window of the RC application.
        /// </summary>
        public MonoGameWindow Window
        {
            get
            {
                if (this.ObjectDisposed) { throw new ObjectDisposedException("MonoGameRenderLoop"); }
                return this.implementation.Window;
            }
        }

        #region UIRenderLoopBase overrides

        /// <see cref="UIRenderLoopBase.Start_i"/>
        protected override void Start_i(RCIntVector screenSize)
        {
            this.mouseInputDevice.Reset(screenSize / 2);
            this.implementation.ScreenSize = screenSize;
            this.implementation.Run();
        }

        /// <see cref="UIRenderLoopBase.Stop_i"/>
        protected override void Stop_i()
        {
            this.implementation.Exit();
        }

        /// <see cref="UIRenderLoopBase.Dispose_i"/>
        protected override void Dispose_i()
        {
            UIRoot.Instance.UnregisterMouseInputDevice();
            UIRoot.Instance.UnregisterKeyboardInputDevice();
            this.implementation.Dispose();
        }

        #endregion UIRenderLoopBase overrides

        #region IUIRenderContext implementations

        /// <see cref="UIRenderLoopBase.RenderSprite_i"/>
        protected override void RenderSprite_i(UISprite sprite, RCIntVector position)
        {
            MonoGameSprite srcSprite = (MonoGameSprite)sprite;
            if (srcSprite.MonoGameTexture == null) { throw new InvalidOperationException("Sprite not uploaded to the graphics device!"); }

            if (this.Clip == RCIntRectangle.Undefined)
            {
                /// No clipping rectangle --> normal render
                this.implementation.SpriteBatch.Draw(
                    srcSprite.MonoGameTexture,
                    new Vector2((float)position.X, (float)position.Y),
                    Color.White
                );
            }
            else
            {
                /// Clipping rectangle exists --> render with clip
                RenderSpriteWithClip(
                    srcSprite,
                    position,
                    new RCIntRectangle(
                        0,
                        0,
                        srcSprite.Size.X * sprite.PixelSize.X,
                        srcSprite.Size.Y * sprite.PixelSize.Y
                    )
                );
            }
        }

        /// <see cref="UIRenderLoopBase.RenderSprite_i"/>
        protected override void RenderSprite_i(UISprite sprite, RCIntVector position, RCIntRectangle section)
        {
            MonoGameSprite srcSprite = (MonoGameSprite)sprite;
            if (srcSprite.MonoGameTexture == null) { throw new InvalidOperationException("Sprite not uploaded to the graphics device!"); }

            if (this.Clip == RCIntRectangle.Undefined)
            {
                /// No clipping rectangle --> normal render
                Rectangle srcRect = new Rectangle(
                    section.X * sprite.PixelSize.X,
                    section.Y * sprite.PixelSize.Y,
                    section.Width * sprite.PixelSize.X,
                    section.Height * sprite.PixelSize.Y
                );

                this.implementation.SpriteBatch.Draw(
                    srcSprite.MonoGameTexture,
                    new Vector2((float)position.X, (float)position.Y),
                    srcRect,
                    Color.White
                );
            }
            else
            {
                /// Clipping rectangle exists --> render with clip
                RenderSpriteWithClip(
                    srcSprite,
                    position,
                    new RCIntRectangle(
                        section.X * sprite.PixelSize.X,
                        section.Y * sprite.PixelSize.Y,
                        section.Width * sprite.PixelSize.X,
                        section.Height * sprite.PixelSize.Y
                    )
                );
            }
        }

        /// <see cref="UIRenderLoopBase.RenderString_i"/>
        protected override void RenderString_i(UIString str, RCIntVector position)
        {
            throw new NotImplementedException();
        }

        /// <see cref="UIRenderLoopBase.RenderString_i"/>
        protected override void RenderString_i(UIString str, RCIntVector position, int width)
        {
            throw new NotImplementedException();
        }

        /// <see cref="UIRenderLoopBase.RenderString_i"/>
        protected override void RenderString_i(UIString str, RCIntVector position, RCIntVector textboxSize, UIStringAlignment alignment)
        {
            throw new NotImplementedException();
        }

        /// <see cref="UIRenderLoopBase.RenderRectangle_i"/>
        protected override void RenderRectangle_i(UISprite brush, RCIntRectangle rect)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Internal method to render a sprite in order to clip it with the clip rectangle.
        /// </summary>
        /// <param name="sprite">The sprite to render.</param>
        /// <param name="position">The position where to render in screen coordinates.</param>
        /// <param name="absSection">
        /// The section of the sprite to render in the coordinate-system of the MonoGame texture.
        /// </param>
        private void RenderSpriteWithClip(MonoGameSprite sprite, RCIntVector position, RCIntRectangle absSection)
        {
            /// Compute the clipped section in the coordinate-system of the MonoGame texture.
            RCIntRectangle clippedSection = new RCIntRectangle(
                this.Clip.Location - position + absSection.Location,
                this.Clip.Size
            );
            clippedSection.Intersect(absSection);

            if (clippedSection != RCIntRectangle.Undefined)
            {
                Rectangle srcRect = new Rectangle(
                    clippedSection.X,
                    clippedSection.Y,
                    clippedSection.Width,
                    clippedSection.Height
                );

                this.implementation.SpriteBatch.Draw(
                    sprite.MonoGameTexture,
                    new Vector2(
                        (float)position.X + (float)clippedSection.X - (float)absSection.X,
                        (float)position.Y + (float)clippedSection.Y - (float)absSection.Y
                    ),
                    srcRect,
                    Color.White
                );
            }
        }

        #endregion IUIRenderContext implementations

        /// <summary>
        /// Initialization method that is called automatically after the graphics device has been created.
        /// </summary>
        private void Initialize()
        {
            this.platform.SpriteManagerImpl.SecondChanceUploadSprites();
        }

        /// <summary>
        /// Reference to the implementation.
        /// </summary>
        private MonoGameRenderLoopImpl implementation;

        /// <summary>
        /// Reference to the platform.
        /// </summary>
        private MonoGameGraphicsPlatform platform;

        /// <summary>
        /// Reference to the mouse input device.
        /// </summary>
        private MonoGameMouseInputDevice mouseInputDevice;

        /// <summary>
        /// Reference to the keyboard input device.
        /// </summary>
        private MonoGameKeyboardInputDevice keyboardInputDevice;
    }
}
