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
    /// Internal class for handling mouse events in normal input mode.
    /// </summary>
    class NormalMouseHandler : MouseHandlerBase
    {
        /// <summary>
        /// Constructs a NormalMouseHandler instance.
        /// </summary>
        /// <param name="scrollEventSource">The UISensitiveObject that will raise the events for scrolling.</param>
        /// <param name="mapDisplay">Reference to the target map display.</param>
        /// <param name="normalMouseEventSource">The UISensitiveObject that will raise the additional mouse events.</param>
        public NormalMouseHandler(UISensitiveObject scrollEventSource, IMapDisplay mapDisplay,
                                  UISensitiveObject normalMouseEventSource)
            : base(scrollEventSource, mapDisplay)
        {
            if (normalMouseEventSource == null) { throw new ArgumentNullException("normalMouseEventSource"); }
            if (this.CommandView.IsWaitingForTargetPosition) { throw new InvalidOperationException("Normal mouse input is not possible currently!"); }

            this.multiplayerService = ComponentManager.GetInterface<IMultiplayerService>();
            this.normalMouseEventSource = normalMouseEventSource;

            this.currentMouseStatus = MouseStatus.None;
            this.selectionBoxStartPosition = RCIntVector.Undefined;
            this.selectionBoxCurrPosition = RCIntVector.Undefined;
            this.selectionBox = RCIntRectangle.Undefined;

            this.normalMouseEventSource.MouseSensor.StateReset += this.OnStateReset;
            this.normalMouseEventSource.MouseSensor.ButtonDown += this.OnMouseDown;
            this.normalMouseEventSource.MouseSensor.Move += this.OnMouseMove;
            this.normalMouseEventSource.MouseSensor.ButtonUp += this.OnMouseUp;
            this.normalMouseEventSource.MouseSensor.DoubleClick += this.OnMouseDoubleClick;

            this.multiplayerService.GameUpdated += this.OnGameUpdate;
        }

        #region Overrides from MouseHandlerBase

        /// <see cref="IMouseHandler.SelectionBox"/>
        public override RCIntRectangle SelectionBox { get { return this.selectionBox; } }

        /// <see cref="MouseHandlerBase.Inactivate_i"/>
        protected override void Inactivate_i()
        {
            this.multiplayerService.GameUpdated -= this.OnGameUpdate;

            this.normalMouseEventSource.MouseSensor.StateReset -= this.OnStateReset;
            this.normalMouseEventSource.MouseSensor.ButtonDown -= this.OnMouseDown;
            this.normalMouseEventSource.MouseSensor.Move -= this.OnMouseMove;
            this.normalMouseEventSource.MouseSensor.ButtonUp -= this.OnMouseUp;
            this.normalMouseEventSource.MouseSensor.DoubleClick -= this.OnMouseDoubleClick;
            this.selectionBox = RCIntRectangle.Undefined;
        }

        #endregion Overrides from MouseHandlerBase

        #region Mouse sensor event handling

        /// <summary>
        /// Called when a mouse button has been pushed over the display.
        /// </summary>
        private void OnMouseDown(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            if (this.CurrentMouseStatus == MouseStatus.None)
            {
                if (evtArgs.Button == UIMouseButton.Right)
                {
                    /// Handle the mouse event.
                    TraceManager.WriteAllTrace(string.Format("RIGHT_CLICK {0}", evtArgs.Position), PresLogicTraceFilters.INFO);
                    this.CommandService.SendFastCommand(this.MapDisplay.DisplayedArea, evtArgs.Position);
                    this.CurrentMouseStatus = MouseStatus.RightDown;
                }
                else if (evtArgs.Button == UIMouseButton.Left)
                {
                    this.CurrentMouseStatus = MouseStatus.LeftDown;

                    /// Start drawing the selection box.
                    this.selectionBoxStartPosition = evtArgs.Position;
                }
            }
        }

        /// <summary>
        /// Called when there was a double click happened over the display.
        /// </summary>
        private void OnMouseDoubleClick(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            if (evtArgs.Button == UIMouseButton.Left)
            {
                if (this.CurrentMouseStatus == MouseStatus.LeftDown)
                {
                    this.CurrentMouseStatus = MouseStatus.DoubleClicked;
                }
            }
        }

        /// <summary>
        /// Called when the mouse pointer has been moved over the display.
        /// </summary>
        private void OnMouseMove(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            if (this.CurrentMouseStatus == MouseStatus.LeftDown)
            {
                this.CurrentMouseStatus = MouseStatus.Selecting;

                /// Actualize the selection box
                this.selectionBoxCurrPosition = evtArgs.Position;
                this.selectionBox = this.CalculateSelectionBox();
            }
            else if (this.CurrentMouseStatus == MouseStatus.Selecting)
            {
                /// Actualize the selection box
                this.selectionBoxCurrPosition = evtArgs.Position;
                this.selectionBox = this.CalculateSelectionBox();
            }
            else if (this.CurrentMouseStatus == MouseStatus.DoubleClicked)
            {
                this.CurrentMouseStatus = MouseStatus.Selecting;

                /// Actualize the selection box
                this.selectionBoxCurrPosition = evtArgs.Position;
                this.selectionBox = this.CalculateSelectionBox();
            }
        }

        /// <summary>
        /// Called when a mouse button has been released over the display.
        /// </summary>
        private void OnMouseUp(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            if (this.CurrentMouseStatus == MouseStatus.LeftDown && evtArgs.Button == UIMouseButton.Left)
            {
                this.CurrentMouseStatus = MouseStatus.None;

                /// Handle the mouse event.
                TraceManager.WriteAllTrace(string.Format("LEFT_CLICK {0}", evtArgs.Position), PresLogicTraceFilters.INFO);
                this.CommandService.Select(this.MapDisplay.DisplayedArea, evtArgs.Position);

                /// Selection box off.
                this.selectionBoxStartPosition = RCIntVector.Undefined;
                this.selectionBoxCurrPosition = RCIntVector.Undefined;
                this.selectionBox = this.CalculateSelectionBox();
            }
            else if (this.CurrentMouseStatus == MouseStatus.Selecting && evtArgs.Button == UIMouseButton.Left)
            {
                this.CurrentMouseStatus = MouseStatus.None;

                /// Handle the mouse event.
                TraceManager.WriteAllTrace(string.Format("SELECTION {0}", this.selectionBox), PresLogicTraceFilters.INFO);
                this.CommandService.Select(this.MapDisplay.DisplayedArea, this.selectionBox);

                /// Selection box off.
                this.selectionBoxStartPosition = RCIntVector.Undefined;
                this.selectionBoxCurrPosition = RCIntVector.Undefined;
                this.selectionBox = this.CalculateSelectionBox();
            }
            else if (this.CurrentMouseStatus == MouseStatus.RightDown && evtArgs.Button == UIMouseButton.Right)
            {
                this.CurrentMouseStatus = MouseStatus.None;
            }
            else if (this.CurrentMouseStatus == MouseStatus.DoubleClicked && evtArgs.Button == UIMouseButton.Left)
            {
                /// Handle the mouse event.
                TraceManager.WriteAllTrace(string.Format("DOUBLE_CLICK {0}", evtArgs.Position), PresLogicTraceFilters.INFO);
                this.CommandService.SelectType(this.MapDisplay.DisplayedArea, evtArgs.Position);

                this.CurrentMouseStatus = MouseStatus.None;
            }
        }

        /// <summary>
        /// Called when the state of the mouse sensor has been reset.
        /// </summary>
        private void OnStateReset(object sender, EventArgs evtArgs)
        {
            this.CurrentMouseStatus = MouseStatus.None;
            this.selectionBoxStartPosition = RCIntVector.Undefined;
            this.selectionBoxCurrPosition = RCIntVector.Undefined;
            this.selectionBox = RCIntRectangle.Undefined;
        }

        #endregion Mouse sensor event handling

        #region Internal methods

        /// <summary>
        /// This method is called on every game updates and invalidates this mouse handler if necessary.
        /// </summary>
        private void OnGameUpdate()
        {
            if (this.CommandView.IsWaitingForTargetPosition) { this.Inactivate(); }
        }

        /// <summary>
        /// Calculates the current selection box in the coordinate-system of the map control.
        /// </summary>
        /// <returns>
        /// The calculated selection box or RCIntRectangle.Undefined if there is no active selection box.
        /// </returns>
        private RCIntRectangle CalculateSelectionBox()
        {
            if (this.selectionBoxStartPosition != RCIntVector.Undefined && this.selectionBoxCurrPosition != RCIntVector.Undefined)
            {
                RCIntVector topLeftCorner = new RCIntVector(Math.Min(this.selectionBoxStartPosition.X, this.selectionBoxCurrPosition.X),
                                                            Math.Min(this.selectionBoxStartPosition.Y, this.selectionBoxCurrPosition.Y));
                RCIntVector bottomRightCorner = new RCIntVector(Math.Max(this.selectionBoxStartPosition.X, this.selectionBoxCurrPosition.X),
                                                                Math.Max(this.selectionBoxStartPosition.Y, this.selectionBoxCurrPosition.Y));
                return new RCIntRectangle(topLeftCorner.X,
                                          topLeftCorner.Y,
                                          bottomRightCorner.X - topLeftCorner.X + 1,
                                          bottomRightCorner.Y - topLeftCorner.Y + 1);
            }
            else
            {
                return RCIntRectangle.Undefined;
            }
        }

        /// <summary>
        /// Gets or sets the mouse status of this handler.
        /// </summary>
        private MouseStatus CurrentMouseStatus
        {
            get { return this.currentMouseStatus; }
            set
            {
                MouseStatus oldValue = this.currentMouseStatus;
                this.currentMouseStatus = value;
                if (oldValue == MouseStatus.None && this.currentMouseStatus != MouseStatus.None)
                {
                    this.DisableScrolling();
                }
                else if (oldValue != MouseStatus.None && this.currentMouseStatus == MouseStatus.None)
                {
                    this.EnableScrolling();
                }
            }
        }

        #endregion Internal methods

        /// <summary>
        /// Enumerates the possible mouse statuses of the display.
        /// </summary>
        private enum MouseStatus
        {
            None = 0,
            LeftDown = 1,
            RightDown = 2,
            Selecting = 3,
            DoubleClicked = 4
        }

        /// <summary>
        /// The current mouse status of this handler.
        /// </summary>
        private MouseStatus currentMouseStatus;

        /// <summary>
        /// The starting position of the currently drawn selection box or RCIntVector.Undefined if there is no
        /// selection box currently being drawn.
        /// </summary>
        private RCIntVector selectionBoxStartPosition;

        /// <summary>
        /// The current position of the currently drawn selection box or RCIntVector.Undefined if there is no
        /// selection box currently being drawn.
        /// </summary>
        private RCIntVector selectionBoxCurrPosition;

        /// <summary>
        /// The current selection box.
        /// </summary>
        private RCIntRectangle selectionBox;

        /// <summary>
        /// The event source for additional mouse events.
        /// </summary>
        private UISensitiveObject normalMouseEventSource;

        /// <summary>
        /// Reference to the multiplayer service.
        /// </summary>
        private IMultiplayerService multiplayerService;
    }
}
