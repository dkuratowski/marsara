using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.UI;
using RC.Common;
using RC.App.BizLogic;
using RC.Common.ComponentModel;
using RC.App.BizLogic.Views;

namespace RC.App.PresLogic.Controls
{
    /// <summary>
    /// Defines the interface of a map display control.
    /// </summary>
    public abstract class RCMapDisplay : UISensitiveObject, IMapDisplay, IGameConnector
    {
        /// <summary>
        /// Constructs a map display control at the given position with the given size.
        /// </summary>
        /// <param name="position">The position of the map display control.</param>
        /// <param name="size">The size of the map display control.</param>
        public RCMapDisplay(RCIntVector position, RCIntVector size)
            : base(position, new RCIntRectangle(0, 0, size.X, size.Y))
        {
            this.isConnected = false;
            this.backgroundTask = null;
            this.displayedArea = RCIntRectangle.Undefined;
            this.attachedMouseHandler = null;
        }

        #region IMapDisplay methods

        /// <see cref="IMapDisplay.DisplayedArea"/>
        public RCIntRectangle DisplayedArea { get { return this.displayedArea; } }

        /// <see cref="IMapDisplay.AttachMouseHandler"/>
        public void AttachMouseHandler(IMouseHandler handler)
        {
            if (handler == null) { throw new ArgumentNullException("handler"); }
            if (this.attachedMouseHandler != null) { throw new InvalidOperationException("Mouse handler already attached!"); }

            this.attachedMouseHandler = handler;
            UIRoot.Instance.GraphicsPlatform.RenderLoop.FrameUpdate += this.OnFrameUpdate;
            this.OnMouseHandlerAttached(handler);
        }

        /// <see cref="IMapDisplay.DetachMouseHandler"/>
        public void DetachMouseHandler()
        {
            if (this.attachedMouseHandler == null) { throw new InvalidOperationException("Mouse handler not attached!"); }

            this.OnMouseHandlerDetaching();
            UIRoot.Instance.GraphicsPlatform.RenderLoop.FrameUpdate -= this.OnFrameUpdate;
            this.attachedMouseHandler = null;
        }

        #endregion IMapDisplay methods

        #region IGameConnector members

        /// <see cref="IGameConnector.Connect"/>
        public void Connect()
        {
            if (this.isConnected || this.backgroundTask != null) { throw new InvalidOperationException("The map display has been connected or is currently being connected!"); }

            this.ConnectorOperationFinished += this.OnConnected;

            this.Connect_i();
            this.backgroundTask = UITaskManager.StartParallelTask(this.ConnectBackgroundProc_i, "RCMapDisplay.Connect");
            this.backgroundTask.Finished += this.OnBackgroundTaskFinished;
            this.backgroundTask.Failed += delegate(IUIBackgroundTask sender, object message)
            {
                throw (Exception)message;
            };
        }

        /// <see cref="IGameConnector.Disconnect"/>
        public void Disconnect()
        {
            if (!this.isConnected || this.backgroundTask != null) { throw new InvalidOperationException("The map display has been connected or is currently being connected!"); }

            this.displayedArea = RCIntRectangle.Undefined;

            this.Disconnect_i();
            this.backgroundTask = UITaskManager.StartParallelTask(this.DisconnectBackgroundProc_i, "RCMapDisplay.Disconnect");
            this.backgroundTask.Finished += this.OnBackgroundTaskFinished;
            this.backgroundTask.Failed += delegate(IUIBackgroundTask sender, object message)
            {
                throw (Exception)message;
            };
        }

        /// <see cref="IGameConnector.CurrentStatus"/>
        public ConnectionStatusEnum CurrentStatus
        {
            get
            {
                if (this.backgroundTask == null) { return this.isConnected ? ConnectionStatusEnum.Online : ConnectionStatusEnum.Offline; }
                else { return this.isConnected ? ConnectionStatusEnum.Disconnecting : ConnectionStatusEnum.Connecting; }
            }
        }

        /// <see cref="IGameConnector.ConnectorOperationFinished"/>
        public event Action<IGameConnector> ConnectorOperationFinished;

        #endregion IGameConnector members

        #region Protected methods

        /// <summary>
        /// Gets the currently attached mouse handler or null if no mouse handler currently attached.
        /// </summary>
        protected IMouseHandler MouseHandler { get { return this.attachedMouseHandler; } }

        #endregion Protected methods

        #region Overridables

        /// <summary>
        /// Gets the currently active map view.
        /// </summary>
        protected abstract IMapView MapView { get; }

        /// <summary>
        /// This method is called when the given mouse handler has been attached to this map display.
        /// </summary>
        /// <param name="handler">The attached handler.</param>
        /// <remarks>Can be overriden in the derived classes. The default implementation does nothing.</remarks>
        protected virtual void OnMouseHandlerAttached(IMouseHandler handler) { }

        /// <summary>
        /// This method is called when the currently attached mouse handler is being detached from this map display.
        /// </summary>
        /// <remarks>Can be overriden in the derived classes. The default implementation does nothing.</remarks>
        protected virtual void OnMouseHandlerDetaching() { }

        /// <summary>
        /// The internal implementation of the connecting procedure that can be overriden by the derived classes.
        /// Note that this method will be called from the UI-thread!
        /// The default implementation does nothing.
        /// </summary>
        protected virtual void Connect_i() { }

        /// <summary>
        /// The internal implementation of the disconnecting procedure that can be overriden by the derived classes.
        /// Note that this method will be called from the UI-thread!
        /// The default implementation does nothing.
        /// </summary>
        protected virtual void Disconnect_i() { }

        /// <summary>
        /// The internal implementation of the connecting procedure that can be overriden by the derived classes.
        /// Note that this method will be called from a background thread!
        /// The default implementation does nothing.
        /// </summary>
        /// <param name="parameter">Not used.</param>
        protected virtual void ConnectBackgroundProc_i(object parameter) { }

        /// <summary>
        /// The internal implementation of the disconnecting procedure that can be overriden by the derived classes.
        /// Note that this method will be called from a background thread!
        /// The default implementation does nothing.
        /// </summary>
        /// <param name="parameter">Not used.</param>
        protected virtual void DisconnectBackgroundProc_i(object parameter) { }

        #endregion Overridables

        #region Event handlers

        /// <summary>
        /// Called when the currently running background task has been finished.
        /// </summary>
        private void OnBackgroundTaskFinished(IUIBackgroundTask sender, object message)
        {
            this.backgroundTask = null;
            if (!this.isConnected)
            {
                this.isConnected = true;
                if (this.ConnectorOperationFinished != null) { this.ConnectorOperationFinished(this); }
            }
            else
            {
                this.isConnected = false;
                if (this.ConnectorOperationFinished != null) { this.ConnectorOperationFinished(this); }
            }
        }

        /// <summary>
        /// Called when this RCMapDisplay has been connected.
        /// </summary>
        private void OnConnected(IGameConnector sender)
        {
            this.ConnectorOperationFinished -= this.OnConnected;
            this.OnScroll(new RCIntVector(0, 0));
        }

        /// <summary>
        /// This method is called on every frame update.
        /// </summary>
        private void OnFrameUpdate()
        {
            if (this.attachedMouseHandler == null || this.attachedMouseHandler.CurrentScrollDirection == ScrollDirection.NoScroll)
            {
                this.timeSinceLastScroll = 0;
                return;
            }

            this.timeSinceLastScroll += UIRoot.Instance.GraphicsPlatform.RenderLoop.TimeSinceLastUpdate;
            if (this.timeSinceLastScroll > TIME_BETWEEN_MAP_SCROLLS)
            {
                this.timeSinceLastScroll = 0;
                if (this.attachedMouseHandler.CurrentScrollDirection == ScrollDirection.North) { this.OnScroll(this.displayedArea.Location + new RCIntVector(0, -PIXELS_PER_SCROLLS)); }
                if (this.attachedMouseHandler.CurrentScrollDirection == ScrollDirection.NorthEast) { this.OnScroll(this.displayedArea.Location + new RCIntVector(PIXELS_PER_SCROLLS, -PIXELS_PER_SCROLLS)); }
                if (this.attachedMouseHandler.CurrentScrollDirection == ScrollDirection.East) { this.OnScroll(this.displayedArea.Location + new RCIntVector(PIXELS_PER_SCROLLS, 0)); }
                if (this.attachedMouseHandler.CurrentScrollDirection == ScrollDirection.SouthEast) { this.OnScroll(this.displayedArea.Location + new RCIntVector(PIXELS_PER_SCROLLS, PIXELS_PER_SCROLLS)); }
                if (this.attachedMouseHandler.CurrentScrollDirection == ScrollDirection.South) { this.OnScroll(this.displayedArea.Location + new RCIntVector(0, PIXELS_PER_SCROLLS)); }
                if (this.attachedMouseHandler.CurrentScrollDirection == ScrollDirection.SouthWest) { this.OnScroll(this.displayedArea.Location + new RCIntVector(-PIXELS_PER_SCROLLS, PIXELS_PER_SCROLLS)); }
                if (this.attachedMouseHandler.CurrentScrollDirection == ScrollDirection.West) { this.OnScroll(this.displayedArea.Location + new RCIntVector(-PIXELS_PER_SCROLLS, 0)); }
                if (this.attachedMouseHandler.CurrentScrollDirection == ScrollDirection.NorthWest) { this.OnScroll(this.displayedArea.Location + new RCIntVector(-PIXELS_PER_SCROLLS, -PIXELS_PER_SCROLLS)); }
            }
        }

        /// <summary>
        /// Scrolls this map display.
        /// </summary>
        /// <param name="where">The top-left corner of the new displayed area in pixels.</param>
        private void OnScroll(RCIntVector where)
        {
            RCIntVector location =
               new RCIntVector(Math.Max(0, Math.Min(this.MapView.MapSize.X - this.Range.Width, where.X)),
                               Math.Max(0, Math.Min(this.MapView.MapSize.Y - this.Range.Height, where.Y)));
            this.displayedArea = new RCIntRectangle(location, this.Range.Size);
        }

        #endregion Event handlers

        /// <summary>
        /// This flag indicates whether this map display control has been connected or not.
        /// </summary>
        private bool isConnected;

        /// <summary>
        /// Reference to the currently executed connecting/disconnecting task or null if no such a task is under execution.
        /// </summary>
        private IUIBackgroundTask backgroundTask;

        /// <summary>
        /// The displayed area of the map in pixels.
        /// </summary>
        private RCIntRectangle displayedArea;

        /// <summary>
        /// Reference to the currently attached mouse handler or null if no handler is currently attached.
        /// </summary>
        private IMouseHandler attachedMouseHandler;

        /// <summary>
        /// Elapsed time since last scroll in milliseconds.
        /// </summary>
        private int timeSinceLastScroll;

        /// <summary>
        /// The time between scrolling operations in milliseconds.
        /// </summary>
        private const int TIME_BETWEEN_MAP_SCROLLS = 20;

        /// <summary>
        /// The number of pixels to scroll per scrolling operations.
        /// </summary>
        private const int PIXELS_PER_SCROLLS = 5;
    }
}
