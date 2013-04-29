using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.UI;
using RC.Common;
using RC.App.BizLogic;
using RC.Common.ComponentModel;
using RC.App.BizLogic.PublicInterfaces;

namespace RC.App.PresLogic.Controls
{
    /// <summary>
    /// Defines the interface of a map display control.
    /// </summary>
    public abstract class RCMapDisplay : UISensitiveObject, IScrollableControl
    {
        /// <summary>
        /// Constructs a map display control at the given position with the given size.
        /// </summary>
        /// <param name="position">The position of the map display control.</param>
        /// <param name="size">The size of the map display control.</param>
        /// <param name="map">Reference to a map view.</param>
        /// <param name="tilesetView">Reference to a tileset view.</param>
        public RCMapDisplay(RCIntVector position, RCIntVector size, IMapView map)
            : base(position, new RCIntRectangle(0, 0, size.X, size.Y))
        {
            if (map == null) { throw new ArgumentNullException("map"); }

            this.isStarted = false;
            this.backgroundTask = null;
            this.displayedArea = RCIntRectangle.Undefined;
            this.map = map;
        }

        #region IScrollableControl methods

        /// <see cref="IScrollableControl.ScrollTo"/>
        public void ScrollTo(RCIntVector where)
        {
            if (!this.isStarted || this.backgroundTask != null) { throw new InvalidOperationException("The map display has been stopped or is currently being stopped!"); }
            if (where == RCIntVector.Undefined) { throw new ArgumentNullException("where"); }

            this.ScrollTo_i(where);
        }

        /// <see cref="IScrollableControl.DisplayedArea"/>
        public RCIntRectangle DisplayedArea { get { return this.displayedArea; } }

        #endregion IScrollableControl methods

        #region Public interface

        /// <summary>
        /// Starts displaying the map.
        /// </summary>
        public void Start()
        {
            if (this.isStarted || this.backgroundTask != null) { throw new InvalidOperationException("The map display has been started or is currently being started!"); }

            this.Started += this.OnStarted;

            this.Start_i();
            this.backgroundTask = UITaskManager.StartParallelTask(this.StartProc_i, "RCMapDisplay.Start");
            this.backgroundTask.Finished += this.OnBackgroundTaskFinished;
            this.backgroundTask.Failed += delegate(IUIBackgroundTask sender, object message)
            {
                throw (Exception)message;
            };
        }

        /// <summary>
        /// Stops displaying the map.
        /// </summary>
        public void Stop()
        {
            if (!this.isStarted || this.backgroundTask != null) { throw new InvalidOperationException("The map display has been stopped or is currently being stopped!"); }

            this.displayedArea = RCIntRectangle.Undefined;

            this.Stop_i();
            this.backgroundTask = UITaskManager.StartParallelTask(this.StopProc_i, "RCMapDisplay.Stop");
            this.backgroundTask.Finished += this.OnBackgroundTaskFinished;
            this.backgroundTask.Failed += delegate(IUIBackgroundTask sender, object message)
            {
                throw (Exception)message;
            };
        }

        /// <summary>
        /// Raised when this map display control has been started.
        /// </summary>
        public event EventHandler Started;

        /// <summary>
        /// Raised when this map display control has been stopped.
        /// </summary>
        public event EventHandler Stopped;

        #endregion Public interface

        #region Overridables

        /// <summary>
        /// The internal implementation RCMapDisplay.ScrollTo that can be overriden by the derived classes.
        /// The default implementation maintains the RCMapDisplay.DisplayedArea property.
        /// </summary>
        /// <param name="where">The top-left corner of the displayed area in pixels.</param>
        protected virtual void ScrollTo_i(RCIntVector where)
        {
            RCIntVector location =
               new RCIntVector(Math.Max(0, Math.Min(this.map.MapSize.X - this.Range.Width, where.X)),
                               Math.Max(0, Math.Min(this.map.MapSize.Y - this.Range.Height, where.Y)));
            this.displayedArea = new RCIntRectangle(location, this.Range.Size);
        }

        /// <summary>
        /// The internal implementation of the starting procedure that can be overriden by the derived classes.
        /// Note that this method will be called from the UI-thread!
        /// The default implementation does nothing.
        /// </summary>
        protected virtual void Start_i() { }

        /// <summary>
        /// The internal implementation of the stopping procedure that can be overriden by the derived classes.
        /// Note that this method will be called from the UI-thread!
        /// The default implementation does nothing.
        /// </summary>
        protected virtual void Stop_i() { }

        /// <summary>
        /// The internal implementation of the starting procedure that can be overriden by the derived classes.
        /// Note that this method will be called from a background thread!
        /// The default implementation does nothing.
        /// </summary>
        /// <param name="parameter">Not used.</param>
        protected virtual void StartProc_i(object parameter) { }

        /// <summary>
        /// The internal implementation of the stopping procedure that can be overriden by the derived classes.
        /// Note that this method will be called from a background thread!
        /// The default implementation does nothing.
        /// </summary>
        /// <param name="parameter">Not used.</param>
        protected virtual void StopProc_i(object parameter) { }

        #endregion Overridables

        #region Event handlers

        /// <summary>
        /// Called when the currently running background task has been finished.
        /// </summary>
        private void OnBackgroundTaskFinished(IUIBackgroundTask sender, object message)
        {
            this.backgroundTask = null;
            if (!this.isStarted)
            {
                this.isStarted = true;
                if (this.Started != null) { this.Started(this, new EventArgs()); }
            }
            else
            {
                this.isStarted = false;
                if (this.Stopped != null) { this.Stopped(this, new EventArgs()); }
            }
        }

        /// <summary>
        /// Called when this RCMapDisplay has been started.
        /// </summary>
        private void OnStarted(object sender, EventArgs args)
        {
            this.Started -= this.OnStarted;
            this.ScrollTo(new RCIntVector(0, 0));
        }

        #endregion Event handlers

        /// <summary>
        /// This flag indicates whether this map display control has been started or not.
        /// </summary>
        private bool isStarted;

        /// <summary>
        /// Reference to the currently executed starting/stopping task or null if no such a task is under execution.
        /// </summary>
        private IUIBackgroundTask backgroundTask;

        /// <summary>
        /// The displayed area of the map in pixels.
        /// </summary>
        private RCIntRectangle displayedArea;

        /// <summary>
        /// Reference to a generic view on the map.
        /// </summary>
        private IMapView map;
    }
}
