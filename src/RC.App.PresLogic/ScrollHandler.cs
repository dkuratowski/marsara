using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.UI;
using RC.App.PresLogic.Controls;
using RC.Common;

namespace RC.App.PresLogic
{
    /// <summary>
    /// Internal class that scrolls a given control when the appropriate input events are coming.
    /// </summary>
    class ScrollHandler
    {
        /// <summary>
        /// Constructs a ScrollHandler instance.
        /// </summary>
        /// <param name="evtSource">The UISensitiveObject that will raise the input events.</param>
        /// <param name="scrolledControl">Reference to the control to be scrolled.</param>
        public ScrollHandler(UISensitiveObject evtSource, IScrollableControl scrolledControl)
        {
            if (evtSource == null) { throw new ArgumentNullException("evtSource"); }
            if (scrolledControl == null) { throw new ArgumentNullException("scrolledControl"); }

            this.currentScrollDir = ScrollDirection.NoScroll;
            this.timeSinceLastScroll = 0;
            this.eventSource = evtSource;
            this.scrolledControl = scrolledControl;
            this.isMouseHandlingActive = false;
        }

        /// <summary>
        /// This event is raised when a mouse handling activity has been started on this scroll handler.
        /// </summary>
        public event EventHandler MouseActivityStarted;

        /// <summary>
        /// This event is raised when a mouse handling activity has been finished on this scroll handler.
        /// </summary>
        public event EventHandler MouseActivityFinished;

        /// <summary>
        /// Activates the scroll handler. If it is currently active then this method has no effect.
        /// </summary>
        public void ActivateMouseHandling()
        {
            if (!this.isMouseHandlingActive)
            {
                this.eventSource.MouseSensor.Move += this.OnMouseMove;
                this.isMouseHandlingActive = true;
            }
        }

        /// <summary>
        /// Deactivates the scroll handler. If it is currently inactive then this method has no effect.
        /// </summary>
        public void DeactivateMouseHandling()
        {
            if (this.isMouseHandlingActive)
            {
                this.eventSource.MouseSensor.Move -= this.OnMouseMove;
                this.isMouseHandlingActive = false;
            }
        }

        /// <summary>
        /// Called when the mouse cursor is moved over the area of the page.
        /// </summary>
        private void OnMouseMove(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            ScrollDirection newScrollDir = ScrollDirection.NoScroll;

            if (evtArgs.Position.X == 0 || evtArgs.Position.X == this.eventSource.Range.Width - 1 ||
                evtArgs.Position.Y == 0 || evtArgs.Position.Y == this.eventSource.Range.Height - 1)
            {
                if (evtArgs.Position.X == 0 && evtArgs.Position.Y == 0) { newScrollDir = ScrollDirection.NorthWest; }
                else if (evtArgs.Position.X == this.eventSource.Range.Width - 1 && evtArgs.Position.Y == 0) { newScrollDir = ScrollDirection.NorthEast; }
                else if (evtArgs.Position.X == this.eventSource.Range.Width - 1 && evtArgs.Position.Y == this.eventSource.Range.Height - 1) { newScrollDir = ScrollDirection.SouthEast; }
                else if (evtArgs.Position.X == 0 && evtArgs.Position.Y == this.eventSource.Range.Height - 1) { newScrollDir = ScrollDirection.SouthWest; }
                else if (evtArgs.Position.X == 0) { newScrollDir = ScrollDirection.West; }
                else if (evtArgs.Position.X == this.eventSource.Range.Width - 1) { newScrollDir = ScrollDirection.East; }
                else if (evtArgs.Position.Y == 0) { newScrollDir = ScrollDirection.North; }
                else if (evtArgs.Position.Y == this.eventSource.Range.Height - 1) { newScrollDir = ScrollDirection.South; }

            }
            
            this.CurrentScrollDir = newScrollDir;
        }

        /// <summary>
        /// This method is called on every frame update.
        /// </summary>
        /// <param name="evtArgs">Contains timing informations.</param>
        private void OnFrameUpdate(UIUpdateSystemEventArgs evtArgs)
        {
            this.timeSinceLastScroll += evtArgs.TimeSinceLastUpdate;
            if (this.timeSinceLastScroll > TIME_BETWEEN_MAP_SCROLLS)
            {
                this.timeSinceLastScroll = 0;
                RCIntRectangle oldDisplayedArea = this.scrolledControl.DisplayedArea;
                if (this.CurrentScrollDir == ScrollDirection.North) { this.scrolledControl.ScrollTo(this.scrolledControl.DisplayedArea.Location + new RCIntVector(0, -PIXELS_PER_SCROLLS)); }
                if (this.CurrentScrollDir == ScrollDirection.NorthEast) { this.scrolledControl.ScrollTo(this.scrolledControl.DisplayedArea.Location + new RCIntVector(PIXELS_PER_SCROLLS, -PIXELS_PER_SCROLLS)); }
                if (this.CurrentScrollDir == ScrollDirection.East) { this.scrolledControl.ScrollTo(this.scrolledControl.DisplayedArea.Location + new RCIntVector(PIXELS_PER_SCROLLS, 0)); }
                if (this.CurrentScrollDir == ScrollDirection.SouthEast) { this.scrolledControl.ScrollTo(this.scrolledControl.DisplayedArea.Location + new RCIntVector(PIXELS_PER_SCROLLS, PIXELS_PER_SCROLLS)); }
                if (this.CurrentScrollDir == ScrollDirection.South) { this.scrolledControl.ScrollTo(this.scrolledControl.DisplayedArea.Location + new RCIntVector(0, PIXELS_PER_SCROLLS)); }
                if (this.CurrentScrollDir == ScrollDirection.SouthWest) { this.scrolledControl.ScrollTo(this.scrolledControl.DisplayedArea.Location + new RCIntVector(-PIXELS_PER_SCROLLS, PIXELS_PER_SCROLLS)); }
                if (this.CurrentScrollDir == ScrollDirection.West) { this.scrolledControl.ScrollTo(this.scrolledControl.DisplayedArea.Location + new RCIntVector(-PIXELS_PER_SCROLLS, 0)); }
                if (this.CurrentScrollDir == ScrollDirection.NorthWest) { this.scrolledControl.ScrollTo(this.scrolledControl.DisplayedArea.Location + new RCIntVector(-PIXELS_PER_SCROLLS, -PIXELS_PER_SCROLLS)); }

                if (oldDisplayedArea == this.scrolledControl.DisplayedArea) { this.CurrentScrollDir = ScrollDirection.NoScroll; }
            }
        }

        /// <summary>
        /// Gets or sets the current scroll direction.
        /// </summary>
        private ScrollDirection CurrentScrollDir
        {
            get { return this.currentScrollDir; }
            set
            {
                ScrollDirection oldScrollDir = this.currentScrollDir;
                this.currentScrollDir = value;
                if (this.currentScrollDir != oldScrollDir)
                {
                    if (this.currentScrollDir != ScrollDirection.NoScroll)
                    {
                        UIRoot.Instance.SystemEventQueue.Subscribe<UIUpdateSystemEventArgs>(this.OnFrameUpdate);
                        if (this.MouseActivityStarted != null) { this.MouseActivityStarted(this, new EventArgs()); }
                    }
                    else
                    {
                        UIRoot.Instance.SystemEventQueue.Unsubscribe<UIUpdateSystemEventArgs>(this.OnFrameUpdate);
                        if (this.MouseActivityFinished != null) { this.MouseActivityFinished(this, new EventArgs()); }
                    }
                }
            }
        }

        /// <summary>
        /// The current scrolling direction.
        /// </summary>
        private ScrollDirection currentScrollDir;

        /// <summary>
        /// Elapsed time since last scroll in milliseconds.
        /// </summary>
        private int timeSinceLastScroll;

        /// <summary>
        /// The event source for the scrolling.
        /// </summary>
        private UISensitiveObject eventSource;

        /// <summary>
        /// Reference to the scrolled control.
        /// </summary>
        private IScrollableControl scrolledControl;

        /// <summary>
        /// This flag indicates whether the mouse handling is currently active or not.
        /// </summary>
        private bool isMouseHandlingActive;

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
