using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using RC.Common.Diagnostics;

namespace RC.UI.MonoGamePlugin
{
    /// <summary>
    /// The MonoGame implementation of the graphics platform.
    /// </summary>
    class MonoGameGraphicsPlatform : UIGraphicsPlatformBase
    {
        /// <summary>
        /// Constructs a MonoGameGraphicsPlatform object.
        /// </summary>
        public MonoGameGraphicsPlatform()
        {
        }

        /// <summary>
        /// Gets the current graphics device.
        /// </summary>
        public GraphicsDevice Device
        {
            get
            {
                if (this.ObjectDisposed) { throw new ObjectDisposedException("MonoGameGraphicsPlatform"); }
                try
                {
                    return this.renderLoop.Device;
                }
                catch (Exception ex)
                {
                    TraceManager.WriteExceptionAllTrace(ex, false);
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the MonoGame implementation of the sprite manager.
        /// </summary>
        public MonoGameSpriteManager SpriteManagerImpl
        {
            get { return this.spriteManager; }
        }

        /// <summary>
        /// Gets the wrapper object of the main window of the RC application.
        /// </summary>
        public MonoGameWindow Window
        {
            get
            {
                if (this.ObjectDisposed) { throw new ObjectDisposedException("MonoGameGraphicsPlatform"); }
                return this.renderLoop.Window;
            }
        }

        #region UIGraphicsPlatformBase members

        /// <see cref="UIGraphicsPlatformBase.CreateRenderLoop_i"/>
        protected override UIRenderLoopBase CreateRenderLoop_i()
        {
            this.renderLoop = new MonoGameRenderLoop(this);
            return this.renderLoop;
        }

        /// <see cref="UIGraphicsPlatformBase.CreateSpriteManager_i"/>
        protected override UISpriteManagerBase CreateSpriteManager_i()
        {
            this.spriteManager = new MonoGameSpriteManager(this);
            return this.spriteManager;
        }

        #endregion UIGraphicsPlatformBase members

        /// <summary>
        /// The sprite manager of this platform.
        /// </summary>
        private MonoGameSpriteManager spriteManager;

        /// <summary>
        /// The render loop of this platform.
        /// </summary>
        private MonoGameRenderLoop renderLoop;
    }
}
