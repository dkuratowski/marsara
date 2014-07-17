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
    class ScrollHandler : InputHandler
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
        }

        #region Overrides from InputHandler

        /// <see cref="InputHandler.StartImpl"/>
        protected override void StartImpl()
        {
            this.eventSource.MouseSensor.Move += this.OnMouseMove;
        }

        /// <see cref="InputHandler.StopImpl"/>
        protected override void StopImpl()
        {
            this.eventSource.MouseSensor.Move -= this.OnMouseMove;
        }

        #endregion Overrides from InputHandler

        #region Event handling

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
        private void OnFrameUpdate()
        {
            this.timeSinceLastScroll += UIRoot.Instance.GraphicsPlatform.RenderLoop.TimeSinceLastUpdate;
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

        #endregion Event handling

        #region Internal methods

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
                if (oldScrollDir == ScrollDirection.NoScroll && this.currentScrollDir != ScrollDirection.NoScroll)
                {
                    UIRoot.Instance.GraphicsPlatform.RenderLoop.FrameUpdate += this.OnFrameUpdate;
                    this.OnProcessingStarted();
                }
                else if (oldScrollDir != ScrollDirection.NoScroll && this.currentScrollDir == ScrollDirection.NoScroll)
                {
                    UIRoot.Instance.GraphicsPlatform.RenderLoop.FrameUpdate -= this.OnFrameUpdate;
                    this.OnProcessingFinished();
                }
            }
        }

        #endregion Internal methods

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
        /// The time between scrolling operations in milliseconds.
        /// </summary>
        private const int TIME_BETWEEN_MAP_SCROLLS = 20;

        /// <summary>
        /// The number of pixels to scroll per scrolling operations.
        /// </summary>
        private const int PIXELS_PER_SCROLLS = 5;
    }
}
