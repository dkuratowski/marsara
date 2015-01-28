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
            this.attachedMouseHandler = null;
        }

        #region IMapDisplay methods

        /// <see cref="IMapDisplay.AttachMouseHandler"/>
        public void AttachMouseHandler(IMouseHandler handler)
        {
            if (handler == null) { throw new ArgumentNullException("handler"); }
            if (this.attachedMouseHandler != null) { throw new InvalidOperationException("Mouse handler already attached!"); }

            this.attachedMouseHandler = handler;
            this.OnMouseHandlerAttached(handler);
        }

        /// <see cref="IMapDisplay.DetachMouseHandler"/>
        public void DetachMouseHandler()
        {
            if (this.attachedMouseHandler == null) { throw new InvalidOperationException("Mouse handler not attached!"); }

            this.OnMouseHandlerDetaching();
            this.attachedMouseHandler = null;
        }

        /// <see cref="IMapDisplay.PixelSize"/>
        public RCIntVector PixelSize { get { return this.Range.Size; } }

        #endregion IMapDisplay methods

        #region IGameConnector members

        /// <see cref="IGameConnector.Connect"/>
        public void Connect()
        {
            if (this.isConnected || this.backgroundTask != null) { throw new InvalidOperationException("The map display has been connected or is currently being connected!"); }

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

            this.Disconnect_i();
            this.backgroundTask = UITaskManager.StartParallelTask(this.DisconnectBackgroundProc_i, "RCMapDisplay.Disconnect");
            this.backgroundTask.Finished += this.OnBackgroundTaskFinished;
            this.backgroundTask.Failed += delegate(IUIBackgroundTask sender, object message)
            {
                throw (Exception)message;
            };
        }

        /// <see cref="IGameConnector.ConnectionStatus"/>
        public ConnectionStatusEnum ConnectionStatus
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
        /// Reference to the currently attached mouse handler or null if no handler is currently attached.
        /// </summary>
        private IMouseHandler attachedMouseHandler;
    }
}
