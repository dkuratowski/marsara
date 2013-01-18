using System;

namespace RC.UI
{
    /// <summary>
    /// Contains the basic implementation parts of the IRenderManager interface, and provides extension points to the derived
    /// classes for specialization.
    /// </summary>
    public abstract class UIRenderManagerBase : IUIRenderManager
    {
        /// <summary>
        /// Constructs a RenderManagerBase object.
        /// </summary>
        public UIRenderManagerBase()
        {
            this.attachedObject = null;
            this.renderCalled = false;
        }

        #region IRenderManager Members

        /// <see cref="IUIRenderManager.Attach"/>
        public void Attach(UIObject obj)
        {
            if (UIRoot.Instance.GraphicsPlatform.RenderLoop.IsRendering) { throw new InvalidOperationException("Rendering is in progress!"); }
            if (this.attachedObject != null) { throw new InvalidOperationException("Another UIObject is currently attached to the render manager!"); }
            if (obj == null) { throw new ArgumentNullException("obj"); }
            if (obj.Parent != null) { throw new InvalidOperationException("obj is not a root UIObject!"); }
            
            this.attachedObject = obj;
            this.attachedObject.Attached += this.OnRootObjectAttached;
            this.PostAttach_i();
        }

        /// <see cref="IUIRenderManager.Detach"/>
        public UIObject Detach()
        {
            if (UIRoot.Instance.GraphicsPlatform.RenderLoop.IsRendering) { throw new InvalidOperationException("Rendering is in progress!"); }
            if (this.attachedObject == null) { throw new InvalidOperationException("There is no UIObject currently attached to the render manager!"); }

            this.PreDetach_i();
            UIObject retObj = this.attachedObject;
            this.attachedObject.Attached -= this.OnRootObjectAttached;
            this.attachedObject = null;
            return retObj;
        }

        /// <see cref="IUIRenderManager.Render"/>
        public void Render(IUIRenderContext screenContext)
        {
            if (this.renderCalled) { throw new InvalidOperationException("Illegal recursive call!"); }

            if (this.attachedObject != null)
            {
                this.renderCalled = true;
                this.Render_i(screenContext);
                this.renderCalled = false;
            }
        }

        /// <see cref="IUIRenderManager.AttachedObject"/>
        public UIObject AttachedObject
        {
            get { return this.attachedObject; }
        }

        #endregion

        #region Overridables

        /// <summary>
        /// Internal method to execute post-attach tasks. Must be implemented by the derived classes.
        /// </summary>
        protected abstract void PostAttach_i();

        /// <summary>
        /// Internal method to execute pre-detach tasks. Must be implemented by the derived classes.
        /// </summary>        
        protected abstract void PreDetach_i();

        /// <summary>
        /// Internal method to execute the rendering. Must be implemented by the derived classes.
        /// </summary>
        /// <param name="screenContext">The render context of the screen.</param>
        protected abstract void Render_i(IUIRenderContext screenContext);

        #endregion Overridables

        /// <summary>
        /// This method is called when the UIObject that is attached to the render manager is attached
        /// to another UIObject as a child. The render manager can only work with root UIObjects so
        /// here we only have to throw an exception.
        /// </summary>
        /// <param name="parentObj">The parent UIObject.</param>
        /// <param name="childObj">The root UIObject.</param>
        private void OnRootObjectAttached(UIObject parentObj, UIObject childObj)
        {
            throw new InvalidOperationException("UIObject attached to the render manager has to be root object!");
        }

        /// <summary>
        /// Reference to the currently attached UIObject.
        /// </summary>
        private UIObject attachedObject;

        /// <summary>
        /// This flag is used to avoid recursive calls on UIRenderManagerBase.Render.
        /// </summary>
        private bool renderCalled;
    }
}
