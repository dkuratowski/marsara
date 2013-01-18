using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using RC.Common.Diagnostics;
using System.Windows.Forms;

namespace RC.UI.XnaPlugin
{
    /// <summary>
    /// The XNA-implementation of the graphics platform.
    /// </summary>
    class XnaGraphicsPlatform : UIGraphicsPlatformBase
    {
        /// <summary>
        /// Constructs an XnaGraphicsPlatform object.
        /// </summary>
        public XnaGraphicsPlatform()
        {
        }

        /// <summary>
        /// Gets the current graphics device.
        /// </summary>
        public GraphicsDevice Device
        {
            get
            {
                if (this.ObjectDisposed) { throw new ObjectDisposedException("XnaGraphicsPlatform"); }
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
        /// Gets the XNA implementation of the sprite manager.
        /// </summary>
        public XnaSpriteManager SpriteManagerImpl
        {
            get { return this.spriteManager; }
        }

        /// <summary>
        /// Gets the window of the application.
        /// </summary>
        public Form Window
        {
            get
            {
                if (this.ObjectDisposed) { throw new ObjectDisposedException("XnaGraphicsPlatform"); }
                return this.renderLoop.Window;
            }
        }

        #region UIGraphicsPlatformBase members

        /// <see cref="UIGraphicsPlatformBase.CreateRenderLoop_i"/>
        protected override UIRenderLoopBase CreateRenderLoop_i()
        {
            this.renderLoop = new XnaRenderLoop(this);
            return this.renderLoop;
        }

        /// <see cref="UIGraphicsPlatformBase.CreateSpriteManager_i"/>
        protected override UISpriteManagerBase CreateSpriteManager_i()
        {
            this.spriteManager = new XnaSpriteManager(this);
            return this.spriteManager;
        }

        #endregion UIGraphicsPlatformBase members

        /// <summary>
        /// The sprite manager of this platform.
        /// </summary>
        private XnaSpriteManager spriteManager;

        /// <summary>
        /// The render loop of this platform.
        /// </summary>
        private XnaRenderLoop renderLoop;
    }
}
