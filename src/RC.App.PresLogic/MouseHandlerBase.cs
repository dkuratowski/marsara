using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.UI;
using RC.App.PresLogic.Controls;
using RC.Common;
using RC.App.BizLogic.Views;
using RC.App.BizLogic.Services;
using RC.Common.ComponentModel;

namespace RC.App.PresLogic
{
    /// <summary>
    /// Provides basic scrolling functionalities for handling mouse events.
    /// </summary>
    class MouseHandlerBase : IMouseHandler
    {
        /// <summary>
        /// Constructs a MouseHandlerBase instance.
        /// </summary>
        /// <param name="scrollEventSource">The UISensitiveObject that will raise the events for scrolling.</param>
        /// <param name="mapDisplay">Reference to the target map display.</param>
        public MouseHandlerBase(UISensitiveObject scrollEventSource, IMapDisplay mapDisplay)
        {
            if (scrollEventSource == null) { throw new ArgumentNullException("scrollEventSource"); }
            if (mapDisplay == null) { throw new ArgumentNullException("mapDisplay"); }

            this.selectionService = ComponentManager.GetInterface<ISelectionService>();
            this.commandService = ComponentManager.GetInterface<ICommandService>();
            this.scrollService = ComponentManager.GetInterface<IScrollService>();
            this.commandView = ComponentManager.GetInterface<IViewService>().CreateView<ICommandView>();

            this.currentScrollDir = ScrollDirectionEnum.NoScroll;
            this.timeSinceLastScroll = 0;
            this.isScrollEnabled = true;
            this.mapDisplay = mapDisplay;
            this.scrollEventSource = scrollEventSource;
            this.scrollEventSource.MouseSensor.Move += this.OnMouseMove;
            UIRoot.Instance.GraphicsPlatform.RenderLoop.FrameUpdate += this.OnFrameUpdate;
            this.mapDisplay.AttachMouseHandler(this);
        }

        /// <summary>
        /// Inactivates this mouse handler and detaches it from its target map display.
        /// </summary>
        public void Inactivate()
        {
            this.mapDisplay.DetachMouseHandler();
            this.scrollEventSource.MouseSensor.Move -= this.OnMouseMove;
            UIRoot.Instance.GraphicsPlatform.RenderLoop.FrameUpdate -= this.OnFrameUpdate;
            this.Inactivate_i();
            if (this.Inactivated != null) { this.Inactivated(); }
        }

        /// <summary>
        /// This event is raised when this mouse handler has been inactivated.
        /// </summary>
        public event Action Inactivated;

        #region IMouseHandler members

        /// <see cref="IMouseHandler.SelectionBox"/>
        public virtual RCIntRectangle SelectionBox { get { return RCIntRectangle.Undefined; } }

        /// <see cref="IMouseHandler.DisplayCrosshairs"/>
        public virtual bool DisplayCrosshairs { get { return false; } }

        /// <see cref="IMouseHandler.ObjectPlacementInfo"/>
        public virtual ObjectPlacementInfo ObjectPlacementInfo { get { return null; } }

        #endregion IMouseHandler members

        #region Protected methods

        /// <summary>
        /// Gets a reference to the selection service.
        /// </summary>
        protected ISelectionService SelectionService { get { return this.selectionService; } }

        /// <summary>
        /// Gets a reference to the command service.
        /// </summary>
        protected ICommandService CommandService { get { return this.commandService; } }

        /// <summary>
        /// Gets a reference to a command view.
        /// </summary>
        protected ICommandView CommandView { get { return this.commandView; } }

        /// <summary>
        /// Gets a reference to the target map display.
        /// </summary>
        protected IMapDisplay MapDisplay { get { return this.mapDisplay; } }

        /// <summary>
        /// Enables scrolling the target control. If scrolling of the target control is already enabled then this method has no effect.
        /// </summary>
        protected void EnableScrolling()
        {
            if (this.isScrollEnabled) { return; }
            this.isScrollEnabled = true;
        }

        /// <summary>
        /// Disables scrolling the target control. If scrolling of the target control is already disabled then this method has no effect.
        /// </summary>
        protected void DisableScrolling()
        {
            if (!this.isScrollEnabled) { return; }

            this.isScrollEnabled = false;
            this.currentScrollDir = ScrollDirectionEnum.NoScroll;
        }

        /// <summary>
        /// Overridable method for implementing additional inactivation procedures in the derived classes if necessary.
        /// The default implementation does nothing.
        /// </summary>
        protected virtual void Inactivate_i() { }

        #endregion Protected methods

        #region Event handling

        /// <summary>
        /// Called when the mouse cursor is moved over the area of the page.
        /// </summary>
        private void OnMouseMove(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            ScrollDirectionEnum newScrollDir = ScrollDirectionEnum.NoScroll;

            if (evtArgs.Position.X == 0 || evtArgs.Position.X == this.scrollEventSource.Range.Width - 1 ||
                evtArgs.Position.Y == 0 || evtArgs.Position.Y == this.scrollEventSource.Range.Height - 1)
            {
                if (evtArgs.Position.X == 0 && evtArgs.Position.Y == 0) { newScrollDir = ScrollDirectionEnum.NorthWest; }
                else if (evtArgs.Position.X == this.scrollEventSource.Range.Width - 1 && evtArgs.Position.Y == 0) { newScrollDir = ScrollDirectionEnum.NorthEast; }
                else if (evtArgs.Position.X == this.scrollEventSource.Range.Width - 1 && evtArgs.Position.Y == this.scrollEventSource.Range.Height - 1) { newScrollDir = ScrollDirectionEnum.SouthEast; }
                else if (evtArgs.Position.X == 0 && evtArgs.Position.Y == this.scrollEventSource.Range.Height - 1) { newScrollDir = ScrollDirectionEnum.SouthWest; }
                else if (evtArgs.Position.X == 0) { newScrollDir = ScrollDirectionEnum.West; }
                else if (evtArgs.Position.X == this.scrollEventSource.Range.Width - 1) { newScrollDir = ScrollDirectionEnum.East; }
                else if (evtArgs.Position.Y == 0) { newScrollDir = ScrollDirectionEnum.North; }
                else if (evtArgs.Position.Y == this.scrollEventSource.Range.Height - 1) { newScrollDir = ScrollDirectionEnum.South; }

            }
            
            this.currentScrollDir = newScrollDir;
        }

        /// <summary>
        /// This method is called on every frame update.
        /// </summary>
        private void OnFrameUpdate()
        {
            if (!this.isScrollEnabled || this.currentScrollDir == ScrollDirectionEnum.NoScroll)
            {
                this.timeSinceLastScroll = 0;
                return;
            }

            this.timeSinceLastScroll += UIRoot.Instance.GraphicsPlatform.RenderLoop.TimeSinceLastUpdate;
            if (this.timeSinceLastScroll > TIME_BETWEEN_MAP_SCROLLS)
            {
                this.timeSinceLastScroll = 0;
                this.scrollService.Scroll(this.currentScrollDir);
            }
        }

        #endregion Event handling

        /// <summary>
        /// The current scrolling direction.
        /// </summary>
        private ScrollDirectionEnum currentScrollDir;

        /// <summary>
        /// This flag indicates whether scrolling is currently enabled or not.
        /// </summary>
        private bool isScrollEnabled;

        /// <summary>
        /// The event source for the scrolling.
        /// </summary>
        private readonly UISensitiveObject scrollEventSource;

        /// <summary>
        /// Reference to the target map display.
        /// </summary>
        private readonly IMapDisplay mapDisplay;

        /// <summary>
        /// Reference to the selection service.
        /// </summary>
        private readonly ISelectionService selectionService;

        /// <summary>
        /// Reference to the command service.
        /// </summary>
        private readonly ICommandService commandService;

        /// <summary>
        /// Reference to the scroll service.
        /// </summary>
        private readonly IScrollService scrollService;

        /// <summary>
        /// Reference to a command view.
        /// </summary>
        private readonly ICommandView commandView;

        /// <summary>
        /// Elapsed time since last scroll in milliseconds.
        /// </summary>
        private int timeSinceLastScroll;

        /// <summary>
        /// The time between scrolling operations in milliseconds.
        /// </summary>
        private const int TIME_BETWEEN_MAP_SCROLLS = 20;
    }
}
