using RC.Common;
using System;
using RC.Common.Diagnostics;

namespace RC.UI
{
    /// <summary>
    /// The graphics platform is responsible for implementing any graphics related functionality.
    /// This should be the interface of any graphics platform implementation.
    /// </summary>
    public interface IUIGraphicsPlatform : IDisposable
    {
        /// <summary>
        /// Gets the sprite manager of the platform.
        /// </summary>
        IUISpriteManager SpriteManager { get; }

        /// <summary>
        /// Gets the render loop of the platform.
        /// </summary>
        IUIRenderLoop RenderLoop { get; }

        /// <summary>
        /// Gets the active render manager of the graphics platform. If there is no registered render manager
        /// then the default will be the active.
        /// </summary>
        IUIRenderManager RenderManager { get; }

        /// <summary>
        /// Registers the given render manager to the graphics platform. Use this function if you want to
        /// replace the default render manager.
        /// </summary>
        /// <param name="renderMgr">The new render manager to register.</param>
        void RegisterCustomRenderManager(UIRenderManagerBase renderMgr);

        /// <summary>
        /// Unregisters the currently registered render manager from the graphics platform. Use this function
        /// if you want to use the default render manager again.
        /// </summary>
        void UnregisterCustomRenderManager();
    }

    /// <summary>
    /// The abstract base class of the implementation of the graphics platform.
    /// </summary>
    public abstract class UIGraphicsPlatformBase : IUIGraphicsPlatform
    {
        /// <summary>
        /// Constructs a UIGraphicsPlatformBase object.
        /// </summary>
        public UIGraphicsPlatformBase()
        {
            this.objectDisposed = false;
            TraceManager.WriteAllTrace("Creating DefaultRenderManager", UITraceFilters.INFO);
            this.defaultRenderMgr = new DefaultRenderManager();
            TraceManager.WriteAllTrace("Creating sprite manager", UITraceFilters.INFO);
            this.spriteManager = this.CreateSpriteManager_i();
            TraceManager.WriteAllTrace("Creating render loop", UITraceFilters.INFO);
            this.renderLoop = this.CreateRenderLoop_i();
        }

        #region IUIGraphicsPlatform members

        /// <see cref="IUIGraphicsPlatform.SpriteManager"/>
        public IUISpriteManager SpriteManager
        {
            get
            {
                if (this.objectDisposed) { throw new ObjectDisposedException("UIGraphicsPlatformBase"); }
                return this.spriteManager;
            }
        }

        /// <see cref="IUIGraphicsPlatform.RenderLoop"/>
        public IUIRenderLoop RenderLoop
        {
            get
            {
                if (this.objectDisposed) { throw new ObjectDisposedException("UIGraphicsPlatformBase"); }
                return this.renderLoop;
            }
        }

        /// <see cref="IUIGraphicsPlatform.RenderManager"/>
        public IUIRenderManager RenderManager
        {
            get
            {
                if (this.objectDisposed) { throw new ObjectDisposedException("UIGraphicsPlatformBase"); }
                return this.customRenderMgr != null ? this.customRenderMgr : this.defaultRenderMgr;
            }
        }

        /// <see cref="IUIGraphicsPlatform.RegisterCustomRenderManager"/>
        public void RegisterCustomRenderManager(UIRenderManagerBase renderMgr)
        {
            if (this.objectDisposed) { throw new ObjectDisposedException("UIGraphicsPlatformBase"); }
            if (renderMgr == null) { throw new ArgumentNullException("renderMgr"); }
            if (this.customRenderMgr != null) { throw new UIException("A render manager already registered at the graphics platform!"); }
            if (this.customRenderMgr == this.defaultRenderMgr) { throw new UIException("Unable to register the default render manager at the graphics platform!"); }

            this.customRenderMgr = renderMgr;
            TraceManager.WriteAllTrace("Custom render manager registered", UITraceFilters.INFO);
        }

        /// <see cref="IUIGraphicsPlatform.UnregisterCustomRenderManager"/>
        public void UnregisterCustomRenderManager()
        {
            if (this.objectDisposed) { throw new ObjectDisposedException("UIGraphicsPlatformBase"); }
            if (this.customRenderMgr == null) { throw new UIException("There is no registered render manager at the graphics platform!"); }

            this.customRenderMgr = null;
            TraceManager.WriteAllTrace("Custom render manager unregistered", UITraceFilters.INFO);
        }

        #endregion IUIGraphicsPlatform members

        #region IDisposable members

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            if (this.objectDisposed) { throw new ObjectDisposedException("UIGraphicsPlatformBase"); }

            TraceManager.WriteAllTrace("Destroying graphics platform", UITraceFilters.INFO);
            this.Dispose_i();
            this.spriteManager.Dispose();
            this.renderLoop.Dispose();
            this.objectDisposed = true;
            TraceManager.WriteAllTrace("Graphics platform destroyed", UITraceFilters.INFO);
        }

        #endregion IDisposable members

        /// <summary>
        /// Internal function for creating the sprite manager of this platform.
        /// </summary>
        /// <returns>The created sprite manager.</returns>
        protected abstract UISpriteManagerBase CreateSpriteManager_i();

        /// <summary>
        /// Internal function for creating the render loop of this platform.
        /// </summary>
        /// <returns>The created render loop.</returns>
        protected abstract UIRenderLoopBase CreateRenderLoop_i();

        /// <summary>
        /// Internal function for performing dispose operations in the derived classes. The default implementation is empty.
        /// </summary>
        protected virtual void Dispose_i() { }

        /// <summary>
        /// Gets whether this object has been disposed or not.
        /// </summary>
        protected bool ObjectDisposed { get { return this.objectDisposed; } }

        /// <summary>
        /// The sprite manager of this platform.
        /// </summary>
        private UISpriteManagerBase spriteManager;

        /// <summary>
        /// The render loop of this platform.
        /// </summary>
        private UIRenderLoopBase renderLoop;

        /// <summary>
        /// Reference to the default render manager.
        /// </summary>
        private DefaultRenderManager defaultRenderMgr;

        /// <summary>
        /// Reference to the registered custom render manager or null if there is no custom render manager registered.
        /// </summary>
        private UIRenderManagerBase customRenderMgr;

        /// <summary>
        /// This flag indicates whether this object has been disposed or not.
        /// </summary>
        private bool objectDisposed;
    }
}
