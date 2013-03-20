using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.UI;
using RC.App.BizLogic.PublicInterfaces;
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
        /// <param name="map">Reference to a map view.</param>
        /// <param name="tilesetView">Reference to a tileset view.</param>
        public RCMapDisplayExtension(RCMapDisplay extendedControl, IMapView map)
            : base(extendedControl.Position, new RCIntVector(extendedControl.Range.Width, extendedControl.Range.Height), map)
        {
            if (extendedControl == null) { throw new ArgumentNullException("extendedControl"); }
            this.extendedControl = extendedControl;
            this.extendedCtrlReady = null;

            /// Propagate mouse events of this object to the extended object.
            this.extendedControl.MouseSensor.AttachTo(this.MouseSensor);
        }

        #region Overrides

        /// <see cref="RCMapDisplay.ScrollTo_i"/>
        protected sealed override void ScrollTo_i(RCIntVector where)
        {
            this.extendedControl.ScrollTo(where);
            base.ScrollTo_i(where);
            this.ScrollToExtension_i(where);
        }

        /// <see cref="RCMapDisplay.Start_i"/>
        protected sealed override void Start_i()
        {
            this.extendedControl.Start();
            this.StartExtension_i();
            this.extendedCtrlReady = new ManualResetEvent(false);
            this.extendedControl.Started += delegate(object sender, EventArgs args)
            {
                this.extendedCtrlReady.Set();
            };
        }

        /// <see cref="RCMapDisplay.Stop_i"/>
        protected sealed override void Stop_i()
        {
            this.extendedControl.Stop();
            this.StopExtension_i();
            this.extendedCtrlReady = new ManualResetEvent(false);
            this.extendedControl.Stopped += delegate(object sender, EventArgs args)
            {
                this.extendedCtrlReady.Set();
            };
        }

        /// <see cref="RCMapDisplay.StartProc_i"/>
        protected sealed override void StartProc_i(object parameter)
        {
            this.extendedCtrlReady.WaitOne();
            this.extendedCtrlReady.Close();
            this.extendedCtrlReady = null;
            this.StartExtensionProc_i();
        }

        /// <see cref="RCMapDisplay.StopProc_i"/>
        protected sealed override void StopProc_i(object parameter)
        {
            this.extendedCtrlReady.WaitOne();
            this.extendedCtrlReady.Close();
            this.extendedCtrlReady = null;
            this.StopExtensionProc_i();
        }

        /// <see cref="UIObject.Render_i"/>
        protected sealed override void Render_i(IUIRenderContext renderContext)
        {
            this.extendedControl.Render(renderContext);
            this.RenderExtension_i(renderContext);
        }

        #endregion Overrides

        #region Overridables

        /// <summary>
        /// The internal implementation RCMapDisplayExtension.ScrollToExtension_i that can be overriden by the derived classes.
        /// The default implementation does nothing.
        /// </summary>
        /// <param name="where">The top-left corner of the displayed area in pixels.</param>
        protected virtual void ScrollToExtension_i(RCIntVector where) { }

        /// <summary>
        /// The internal implementation of the starting procedure of this extension that can be overriden by the derived classes.
        /// Note that this method will be called from the UI-thread!
        /// The default implementation does nothing.
        /// </summary>
        /// <param name="parameter">Not used.</param>
        protected virtual void StartExtension_i() { }

        /// <summary>
        /// The internal implementation of the stopping procedure of this extension that can be overriden by the derived classes.
        /// Note that this method will be called from the UI-thread!
        /// The default implementation does nothing.
        /// </summary>
        /// <param name="parameter">Not used.</param>
        protected virtual void StopExtension_i() { }

        /// <summary>
        /// The internal implementation of the starting procedure of this extension that can be overriden by the derived classes.
        /// Note that this method will be called from a background thread!
        /// The default implementation does nothing.
        /// </summary>
        /// <param name="parameter">Not used.</param>
        protected virtual void StartExtensionProc_i() { }

        /// <summary>
        /// The internal implementation of the stopping procedure of this extension that can be overriden by the derived classes.
        /// Note that this method will be called from a background thread!
        /// The default implementation does nothing.
        /// </summary>
        /// <param name="parameter">Not used.</param>
        protected virtual void StopExtensionProc_i() { }

        /// <summary>
        /// The internal implementation of the rendering operation of this extension that can be overriden by the derived classes.
        /// The default implementation does nothing.
        /// </summary>
        /// <param name="renderContext">The context of the render operation.</param>
        protected virtual void RenderExtension_i(IUIRenderContext renderContext) { }

        #endregion Overridables

        /// <summary>
        /// Reference to the extended control.
        /// </summary>
        private RCMapDisplay extendedControl;

        /// <summary>
        /// Event for delaying the background starting/stopping procedure until the extended control finished.
        /// </summary>
        private ManualResetEvent extendedCtrlReady;
    }
}
