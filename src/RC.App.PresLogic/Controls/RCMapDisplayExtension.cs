using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.UI;
using System.Threading;

namespace RC.App.PresLogic.Controls
{
    /// <summary>
    /// Abstract base class of extensions to a map display control.
    /// </summary>
    public abstract class RCMapDisplayExtension : RCMapDisplay
    {
        /// <summary>
        /// Contructs an RCMapDisplayExtension instance.
        /// </summary>
        /// <param name="extendedControl">Reference to the extended control.</param>
        public RCMapDisplayExtension(RCMapDisplay extendedControl)
            : base(extendedControl.Position, new RCIntVector(extendedControl.Range.Width, extendedControl.Range.Height))
        {
            if (extendedControl == null) { throw new ArgumentNullException("extendedControl"); }
            this.extendedControl = extendedControl;
            this.extendedCtrlOpFinished = null;

            /// Propagate mouse events of this object to the extended object.
            this.extendedControl.MouseSensor.AttachTo(this.MouseSensor);
        }

        #region Overrides

        /// <see cref="RCMapDisplay.ScrollTo_i"/>
        protected sealed override void ScrollTo_i(RCIntVector where)
        {
            this.extendedControl.ScrollTo(where);
            base.ScrollTo_i(where);
            this.ScrollToEx_i(where);
        }

        /// <see cref="RCMapDisplay.Connect_i"/>
        protected sealed override void Connect_i()
        {
            this.extendedControl.Connect();
            this.ConnectEx_i();
            this.extendedCtrlOpFinished = new ManualResetEvent(false);
            this.extendedControl.ConnectorOperationFinished += this.OnExtendedControlConnected;
        }

        /// <see cref="RCMapDisplay.Disconnect_i"/>
        protected sealed override void Disconnect_i()
        {
            this.extendedControl.Disconnect();
            this.DisconnectEx_i();
            this.extendedCtrlOpFinished = new ManualResetEvent(false);
            this.extendedControl.ConnectorOperationFinished += this.OnExtendedControlDisconnected;
        }

        /// <see cref="RCMapDisplay.ConnectBackgroundProc_i"/>
        protected sealed override void ConnectBackgroundProc_i(object parameter)
        {
            this.extendedCtrlOpFinished.WaitOne();
            this.extendedCtrlOpFinished.Close();
            this.extendedCtrlOpFinished = null;
            this.ConnectExBackgroundProc_i();
        }

        /// <see cref="RCMapDisplay.DisconnectBackgroundProc_i"/>
        protected sealed override void DisconnectBackgroundProc_i(object parameter)
        {
            this.extendedCtrlOpFinished.WaitOne();
            this.extendedCtrlOpFinished.Close();
            this.extendedCtrlOpFinished = null;
            this.DisconnectExBackgroundProc_i();
        }

        /// <see cref="UIObject.Render_i"/>
        protected sealed override void Render_i(IUIRenderContext renderContext)
        {
            this.extendedControl.Render(renderContext);
            this.RenderEx_i(renderContext);
        }

        #endregion Overrides

        #region Overridables

        /// <summary>
        /// The internal implementation RCMapDisplayExtension.ScrollTo_i that can be overriden by the derived classes.
        /// The default implementation does nothing.
        /// </summary>
        /// <param name="where">The top-left corner of the displayed area in pixels.</param>
        protected virtual void ScrollToEx_i(RCIntVector where) { }

        /// <summary>
        /// The internal implementation of the connecting procedure of this extension that can be overriden by the derived classes.
        /// Note that this method will be called from the UI-thread!
        /// The default implementation does nothing.
        /// </summary>
        protected virtual void ConnectEx_i() { }

        /// <summary>
        /// The internal implementation of the disconnecting procedure of this extension that can be overriden by the derived classes.
        /// Note that this method will be called from the UI-thread!
        /// The default implementation does nothing.
        /// </summary>
        protected virtual void DisconnectEx_i() { }

        /// <summary>
        /// The internal implementation of the connecting procedure of this extension that can be overriden by the derived classes.
        /// Note that this method will be called from a background thread!
        /// The default implementation does nothing.
        /// </summary>
        protected virtual void ConnectExBackgroundProc_i() { }

        /// <summary>
        /// The internal implementation of the disconnecting procedure of this extension that can be overriden by the derived classes.
        /// Note that this method will be called from a background thread!
        /// The default implementation does nothing.
        /// </summary>
        protected virtual void DisconnectExBackgroundProc_i() { }

        /// <summary>
        /// The internal implementation of the rendering operation of this extension that can be overriden by the derived classes.
        /// The default implementation does nothing.
        /// </summary>
        /// <param name="renderContext">The context of the render operation.</param>
        protected virtual void RenderEx_i(IUIRenderContext renderContext) { }

        #endregion Overridables

        #region Event handlers

        /// <summary>
        /// This event handler is called when the extended control has been connected successfully.
        /// </summary>
        private void OnExtendedControlConnected(IGameConnector sender)
        {
            if (sender != this.extendedControl) { throw new InvalidOperationException("Unexpected connector!"); }
            if (sender.CurrentStatus != ConnectionStatusEnum.Online) { throw new InvalidOperationException("Extended control is not online!"); }

            this.extendedControl.ConnectorOperationFinished -= this.OnExtendedControlConnected;
            this.extendedCtrlOpFinished.Set();
        }

        /// <summary>
        /// This event handler is called when the extended control has been disconnected successfully.
        /// </summary>
        private void OnExtendedControlDisconnected(IGameConnector sender)
        {
            if (sender != this.extendedControl) { throw new InvalidOperationException("Unexpected connector!"); }
            if (sender.CurrentStatus != ConnectionStatusEnum.Offline) { throw new InvalidOperationException("Extended control is not offline!"); }

            this.extendedControl.ConnectorOperationFinished -= this.OnExtendedControlDisconnected;
            this.extendedCtrlOpFinished.Set();
        }

        #endregion Event handlers

        /// <summary>
        /// Reference to the extended control.
        /// </summary>
        private RCMapDisplay extendedControl;

        /// <summary>
        /// Event for delaying the background connecting/disconnecting procedure until the extended control's operation finished.
        /// </summary>
        private ManualResetEvent extendedCtrlOpFinished;
    }
}
