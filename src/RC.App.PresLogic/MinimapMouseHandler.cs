using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.Services;
using RC.App.BizLogic.Views;
using RC.App.PresLogic.Controls;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Common.Diagnostics;
using RC.UI;

namespace RC.App.PresLogic
{
    /// <summary>
    /// Internal class for handling mouse events of the minimap.
    /// </summary>
    class MinimapMouseHandler : IDisposable
    {
        /// <summary>
        /// Constructs a MinimapMouseHandler instance.
        /// </summary>
        /// <param name="mouseEventSource">The UISensitiveObject that will raise the mouse events.</param>
        public MinimapMouseHandler(UISensitiveObject mouseEventSource)
        {
            if (mouseEventSource == null) { throw new ArgumentNullException("mouseEventSource"); }

            this.commandService = ComponentManager.GetInterface<ICommandService>();
            this.scrollService = ComponentManager.GetInterface<IScrollService>();
            this.commandView = ComponentManager.GetInterface<IViewService>().CreateView<ICommandView>();

            this.currentMouseStatus = MouseStatus.None;
            this.pressedButton = UIMouseButton.Undefined;
            this.mouseEventSource = mouseEventSource;
            this.mouseEventSource.MouseSensor.StateReset += this.OnStateReset;
            this.mouseEventSource.MouseSensor.ButtonDown += this.OnMouseDown;
            this.mouseEventSource.MouseSensor.Move += this.OnMouseMove;
            this.mouseEventSource.MouseSensor.ButtonUp += this.OnMouseUp;
        }

        /// <summary>
        /// Gets whether crosshairs shall be displayed on the minimap or not.
        /// </summary>
        public bool DisplayCrosshairs
        {
            get { return this.commandView.IsWaitingForTargetPosition && this.commandView.SelectedBuildingType == null; }
        }


        #region IDisposable members

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            this.currentMouseStatus = MouseStatus.None;
            this.pressedButton = UIMouseButton.Undefined;
            this.mouseEventSource.MouseSensor.StateReset -= this.OnStateReset;
            this.mouseEventSource.MouseSensor.ButtonDown -= this.OnMouseDown;
            this.mouseEventSource.MouseSensor.Move -= this.OnMouseMove;
            this.mouseEventSource.MouseSensor.ButtonUp -= this.OnMouseUp;
            this.mouseEventSource = null;
        }

        #endregion IDisposable members

        #region Mouse event handlers

        /// <summary>
        /// Called when a mouse button has been pushed over the minimap display.
        /// </summary>
        private void OnMouseDown(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            if (this.currentMouseStatus == MouseStatus.None)
            {
                if (evtArgs.Button == UIMouseButton.Left && (!this.commandView.IsWaitingForTargetPosition || this.commandView.SelectedBuildingType != null) ||
                    evtArgs.Button == UIMouseButton.Right && this.commandView.IsWaitingForTargetPosition)
                {
                    this.currentMouseStatus = MouseStatus.MovingDisplay;
                    this.pressedButton = evtArgs.Button;
                    TraceManager.WriteAllTrace(string.Format("SCROLL_ON_MINIMAP {0}", evtArgs.Position), PresLogicTraceFilters.INFO);
                    this.scrollService.ScrollToMinimapPosition(evtArgs.Position);
                }
                else if (evtArgs.Button == UIMouseButton.Left && this.commandView.IsWaitingForTargetPosition && this.commandView.SelectedBuildingType == null)
                {
                    this.currentMouseStatus = MouseStatus.SelectingTarget;
                    this.pressedButton = evtArgs.Button;
                    TraceManager.WriteAllTrace(string.Format("SELECT_TARGET_ON_MINIMAP {0}", evtArgs.Position), PresLogicTraceFilters.INFO);
                    this.commandService.SelectTargetPositionOnMinimap(evtArgs.Position);
                }
                else if (evtArgs.Button == UIMouseButton.Right && !this.commandView.IsWaitingForTargetPosition)
                {
                    this.currentMouseStatus = MouseStatus.SelectingTarget;
                    this.pressedButton = evtArgs.Button;
                    TraceManager.WriteAllTrace(string.Format("SEND_FASTCOMMAND_ON_MINIMAP {0}", evtArgs.Position), PresLogicTraceFilters.INFO);
                    this.commandService.SendFastCommandOnMinimap(evtArgs.Position);
                }
            }
        }

        /// <summary>
        /// Called when the mouse pointer has been moved over the display.
        /// </summary>
        private void OnMouseMove(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            if (this.currentMouseStatus == MouseStatus.MovingDisplay)
            {
                TraceManager.WriteAllTrace(string.Format("SCROLL_ON_MINIMAP {0}", evtArgs.Position), PresLogicTraceFilters.INFO);
                this.scrollService.ScrollToMinimapPosition(evtArgs.Position);
            }
        }

        /// <summary>
        /// Called when a mouse button has been released over the display.
        /// </summary>
        private void OnMouseUp(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            if (this.currentMouseStatus != MouseStatus.None && evtArgs.Button == this.pressedButton)
            {
                this.pressedButton = UIMouseButton.Undefined;
                this.currentMouseStatus = MouseStatus.None;
            }
        }

        /// <summary>
        /// Called when the state of the mouse sensor has been reset.
        /// </summary>
        private void OnStateReset(object sender, EventArgs evtArgs)
        {
            this.pressedButton = UIMouseButton.Undefined;
            this.currentMouseStatus = MouseStatus.None;
        }

        #endregion Mouse event handlers

        /// <summary>
        /// Enumerates the possible mouse statuses of the minimap.
        /// </summary>
        private enum MouseStatus
        {
            None = 0,
            MovingDisplay = 1,
            SelectingTarget = 2,
        }

        /// <summary>
        /// The current mouse status of this handler.
        /// </summary>
        private MouseStatus currentMouseStatus;

        /// <summary>
        /// The mouse button that is currently pressed or UIMouseButton.Undefined if no mouse button is currently pressed.
        /// </summary>
        private UIMouseButton pressedButton;

        /// <summary>
        /// The event source of the mouse events.
        /// </summary>
        private UISensitiveObject mouseEventSource;

        /// <summary>
        /// Reference to the command service.
        /// </summary>
        private ICommandService commandService;

        /// <summary>
        /// Reference to the scroll service.
        /// </summary>
        private IScrollService scrollService;

        /// <summary>
        /// Reference to a command view.
        /// </summary>
        private ICommandView commandView;
    }
}
