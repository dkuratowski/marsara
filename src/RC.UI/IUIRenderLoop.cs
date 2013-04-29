using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.Diagnostics;
using RC.Common;
using System.Diagnostics;

namespace RC.UI
{
    /// <summary>
    /// Interface for accessing the render loop of the graphics platform.
    /// </summary>
    public interface IUIRenderLoop
    {
        /// <summary>
        /// Starts the render loop. The render loop is running while IUIRenderLoop.Stop is not called.
        /// </summary>
        /// <param name="screenSize">The size of the screen in pixels.</param>
        /// <remarks>If the render loop is already running then this function has no effect.</remarks>
        void Start(RCIntVector screenSize);

        /// <summary>
        /// Stops the render loop.
        /// </summary>
        /// <remarks>If the render loop is not running then this function has no effect.</remarks>
        void Stop();

        /// <summary>
        /// Gets the elapsed time since update in the previous frame in milliseconds.
        /// </summary>
        int TimeSinceLastUpdate { get; }

        /// <summary>
        /// Gets the elapsed time since the start of the render loop in milliseconds.
        /// </summary>
        int TimeSinceStart { get; }

        /// <summary>
        /// Gets whether rendering is in progress or not.
        /// </summary>
        bool IsRendering { get; }
    }

    /// <summary>
    /// The abstract base class of the implementation of the render loop.
    /// </summary>
    public abstract class UIRenderLoopBase : IUIRenderLoop, IUIRenderContext, IDisposable
    {
        /// <summary>
        /// Constructs a UIRenderLoopBase object.
        /// </summary>
        public UIRenderLoopBase()
        {
            this.state = UIRenderLoopState.Stopped;
            this.objectDisposed = false;
            this.isRendering = false;
            this.clipRect = RCIntRectangle.Undefined;
            this.renderLoopTimer = new Stopwatch();
            this.intermediateTime0 = 0;
            this.intermediateTime1 = 0;
        }

        #region IUIRenderLoop members

        /// <see cref="IUIRenderLoop.Start"/>
        public void Start(RCIntVector screenSize)
        {
            if (this.objectDisposed) { throw new ObjectDisposedException("UIRenderLoopBase"); }
            if (this.state == UIRenderLoopState.Stopped)
            {
                if (screenSize == RCIntVector.Undefined) { throw new ArgumentNullException("screenSize"); }
                if (screenSize.X <= 0 || screenSize.Y <= 0) { throw new ArgumentOutOfRangeException("screenSize"); }

                this.state = UIRenderLoopState.Running;
                this.renderLoopTimer.Start();
                this.Start_i(screenSize);
                this.renderLoopTimer.Stop();
                this.renderLoopTimer.Reset();
                this.intermediateTime0 = 0;
                this.intermediateTime1 = 0;
                this.state = UIRenderLoopState.Stopped;
            }
        }

        /// <see cref="IUIRenderLoop.Stop"/>
        public void Stop()
        {
            if (this.objectDisposed) { throw new ObjectDisposedException("UIRenderLoopBase"); }
            if (this.state == UIRenderLoopState.Running)
            {
                this.state = UIRenderLoopState.Stopping;
                this.Stop_i();
            }
        }

        /// <see cref="IUIRenderLoop.TimeSinceLastUpdate"/>
        public int TimeSinceLastUpdate
        {
            get
            {
                if (this.objectDisposed) { throw new ObjectDisposedException("UIRenderLoopBase"); }
                if (this.state == UIRenderLoopState.Stopped) { throw new InvalidOperationException("UIRenderLoopBase is stopped!"); }

                return this.intermediateTime0 <= this.intermediateTime1 ?
                       this.intermediateTime1 - this.intermediateTime0 :
                       this.intermediateTime0 - this.intermediateTime1;
            }
        }

        /// <see cref="IUIRenderLoop.TimeSinceStart"/>
        public int TimeSinceStart
        {
            get
            {
                if (this.objectDisposed) { throw new ObjectDisposedException("UIRenderLoopBase"); }
                if (this.state == UIRenderLoopState.Stopped) { throw new InvalidOperationException("UIRenderLoopBase is stopped!"); }
                                
                return this.intermediateTime0 <= this.intermediateTime1 ? this.intermediateTime1 : this.intermediateTime0;
            }
        }

        /// <see cref="IUIRenderLoop.IsRendering"/>
        public bool IsRendering
        {
            get
            {
                if (this.objectDisposed) { throw new ObjectDisposedException("UIRenderLoopBase"); }
                return this.isRendering;
            }
        }

        #endregion IUIRenderLoop members
        
        #region IUIRenderContext Members

        /// <see cref="IUIRenderContext.RenderSprite"/>
        public void RenderSprite(UISprite sprite, RCIntVector position)
        {
            if (this.objectDisposed) { throw new ObjectDisposedException("UIRenderLoopBase"); }
            if (!this.isRendering) { throw new UIException("Access denied on screen render context!"); }
            if (sprite == null) { throw new ArgumentNullException("sprite"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }

            this.RenderSprite_i(sprite, position);
        }

        /// <see cref="IUIRenderContext.RenderSprite"/>
        public void RenderSprite(UISprite sprite, RCIntVector position, RCIntRectangle section)
        {
            if (this.objectDisposed) { throw new ObjectDisposedException("UIRenderLoopBase"); }
            if (!this.isRendering) { throw new UIException("Access denied on screen render context!"); }
            if (sprite == null) { throw new ArgumentNullException("sprite"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }
            if (section == RCIntRectangle.Undefined) { throw new ArgumentNullException("section"); }

            this.RenderSprite_i(sprite, position, section);
        }

        /// <see cref="IUIRenderContext.RenderString"/>
        public void RenderString(UIString str, RCIntVector position)
        {
            if (this.objectDisposed) { throw new ObjectDisposedException("UIRenderLoopBase"); }
            if (!this.isRendering) { throw new UIException("Access denied on screen render context!"); }
            if (str == null) { throw new ArgumentNullException("str"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }

            this.RenderString_i(str, position);
        }

        /// <see cref="IUIRenderContext.RenderString"/>
        public void RenderString(UIString str, RCIntVector position, int width)
        {
            if (this.objectDisposed) { throw new ObjectDisposedException("UIRenderLoopBase"); }
            if (!this.isRendering) { throw new UIException("Access denied on screen render context!"); }
            if (str == null) { throw new ArgumentNullException("str"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }
            if (width <= 0) { throw new ArgumentOutOfRangeException("width"); }

            this.RenderString_i(str, position, width);
        }

        /// <see cref="IUIRenderContext.RenderString"/>
        public void RenderString(UIString str, RCIntVector position, RCIntVector textboxSize, UIStringAlignment alignment)
        {
            if (this.objectDisposed) { throw new ObjectDisposedException("UIRenderLoopBase"); }
            if (!this.isRendering) { throw new UIException("Access denied on screen render context!"); }
            if (str == null) { throw new ArgumentNullException("str"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }
            if (textboxSize == RCIntVector.Undefined) { throw new ArgumentNullException("textboxSize"); }
            if (textboxSize.X <= 0 || textboxSize.Y <= 0) { throw new ArgumentOutOfRangeException("textboxSize"); }

            this.RenderString_i(str, position, textboxSize, alignment);
        }

        /// <see cref="IUIRenderContext.RenderRectangle"/>
        public void RenderRectangle(UISprite brush, RCIntRectangle rect)
        {
            if (this.objectDisposed) { throw new ObjectDisposedException("UIRenderLoopBase"); }
            if (!this.isRendering) { throw new UIException("Access denied on screen render context!"); }
            if (brush == null) { throw new ArgumentNullException("brush"); }
            if (rect == RCIntRectangle.Undefined) { throw new ArgumentNullException("rect"); }

            this.RenderRectangle_i(brush, rect);
        }

        /// <see cref="IUIRenderContext.Clip"/>
        public RCIntRectangle Clip
        {
            get
            {
                if (this.objectDisposed) { throw new ObjectDisposedException("UIRenderLoopBase"); }
                if (!this.isRendering) { throw new UIException("Access denied on screen render context!"); }
                return this.clipRect;
            }

            set
            {
                if (this.objectDisposed) { throw new ObjectDisposedException("UIRenderLoopBase"); }
                if (!this.isRendering) { throw new UIException("Access denied on screen render context!"); }
                this.clipRect = value;
            }
        }

        #region IUIRenderContext implementations

        /// <see cref="IUIRenderContext.RenderSprite"/>
        protected abstract void RenderSprite_i(UISprite sprite, RCIntVector position);

        /// <see cref="IUIRenderContext.RenderSprite"/>
        protected abstract void RenderSprite_i(UISprite sprite, RCIntVector position, RCIntRectangle section);

        /// <see cref="IUIRenderContext.RenderString"/>
        protected abstract void RenderString_i(UIString str, RCIntVector position);

        /// <see cref="IUIRenderContext.RenderString"/>
        protected abstract void RenderString_i(UIString str, RCIntVector position, int width);

        /// <see cref="IUIRenderContext.RenderString"/>
        protected abstract void RenderString_i(UIString str, RCIntVector position, RCIntVector textboxSize, UIStringAlignment alignment);

        /// <see cref="IUIRenderContext.RenderRectangle"/>
        protected abstract void RenderRectangle_i(UISprite brush, RCIntRectangle rect);

        #endregion IUIRenderContext implementations

        #endregion IUIRenderContext Members

        #region IDisposable members

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            if (this.objectDisposed) { throw new ObjectDisposedException("UIRenderLoopBase"); }
            if (this.state != UIRenderLoopState.Stopped) { throw new InvalidOperationException("UIRenderLoopBase can only be disposed in Stopped state!"); }

            TraceManager.WriteAllTrace("Destroying render loop", UITraceFilters.INFO);
            this.Dispose_i();
            this.objectDisposed = true;
        }

        #endregion IDisposable members

        /// <summary>
        /// After the render loop has been started, this function is automatically called at the beginning of every
        /// frame for updating the UIObjects.
        /// </summary>
        protected void Update()
        {
            if (this.intermediateTime0 <= this.intermediateTime1)
            {
                this.intermediateTime0 = (int)this.renderLoopTimer.ElapsedMilliseconds;
            }
            else
            {
                this.intermediateTime1 = (int)this.renderLoopTimer.ElapsedMilliseconds;
            }

            UIRoot.Instance.SystemEventQueue.EnqueueEvent<UIUpdateSystemEventArgs>(
                this.intermediateTime0 <= this.intermediateTime1 ?
                new UIUpdateSystemEventArgs(this.intermediateTime1 - this.intermediateTime0, this.intermediateTime1) :
                new UIUpdateSystemEventArgs(this.intermediateTime0 - this.intermediateTime1, this.intermediateTime0));
            UIRoot.Instance.SystemEventQueue.PostEvents();

            UITaskManager.OnUpdate();
        }

        /// <summary>
        /// After the render loop has been started, this function is automatically called at every frame for rendering
        /// the UIObjects.
        /// </summary>
        protected void Render()
        {
            this.isRendering = true;
            UIRoot.Instance.GraphicsPlatform.RenderManager.Render(this);
            this.isRendering = false;
        }

        /// <summary>
        /// Internal function for starting the render loop.
        /// </summary>
        /// <param name="screenSize">The size of the screen in pixels.</param>
        protected abstract void Start_i(RCIntVector screenSize);

        /// <summary>
        /// Internal function for stopping the render loop.
        /// </summary>
        protected abstract void Stop_i();

        /// <summary>
        /// Internal function for performing dispose operations in the derived classes. The default implementation is empty.
        /// </summary>
        protected virtual void Dispose_i() { }

        /// <summary>
        /// Gets whether this object has been disposed or not.
        /// </summary>
        protected bool ObjectDisposed { get { return this.objectDisposed; } }

        /// <summary>
        /// Gets whether rendering to the screen is enabled or not.
        /// </summary>
        protected bool RenderEnabled { get { return this.isRendering; } }

        /// <summary>
        /// Enumerates the possible states of a UIRenderLoopBase.
        /// </summary>
        private enum UIRenderLoopState
        {
            Stopped = 0,
            Running = 1,
            Stopping = 2
        }

        /// <summary>
        /// This flag indicates whether this object has been disposed or not.
        /// </summary>
        private bool objectDisposed;

        /// <summary>
        /// This flag indicates whether rendering to the screen is in progress or not.
        /// </summary>
        private bool isRendering;

        /// <summary>
        /// The screen coordinates of the clipping rectangle or RCIntRectangle.Undefined if there is
        /// no clipping rectangle refined.
        /// </summary>
        private RCIntRectangle clipRect;

        /// <summary>
        /// The current state of this UIRenderLoopBase.
        /// </summary>
        private UIRenderLoopState state;

        /// <summary>
        /// This timer measures the time between Updates.
        /// </summary>
        private Stopwatch renderLoopTimer;

        /// <summary>
        /// The intermediate measurements of time in Update calls.
        /// </summary>
        private int intermediateTime0;

        /// <summary>
        /// The intermediate measurements of time in Update calls.
        /// </summary>
        private int intermediateTime1;
    }

}
