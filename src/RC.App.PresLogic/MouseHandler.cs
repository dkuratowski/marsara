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
    /// Internal class for handling mouse events during gameplay.
    /// </summary>
    class MouseHandler
    {
        /// <summary>
        /// Constructs a MouseHandler instance.
        /// </summary>
        /// <param name="evtSource">The UISensitiveObject that will raise the input events.</param>
        /// <param name="targetControl">Reference to the target control.</param>
        public MouseHandler(UISensitiveObject evtSource, IMapControl targetControl)
        {
            if (evtSource == null) { throw new ArgumentNullException("evtSource"); }
            if (targetControl == null) { throw new ArgumentNullException("targetControl"); }

            this.eventSource = evtSource;
            this.targetControl = targetControl;
            this.commandService = ComponentManager.GetInterface<ICommandService>();
            this.isMouseHandlingActive = false;
            this.eventSource.MouseSensor.StateReset += this.OnStateReset;
        }

        #region Public interface

        /// <summary>
        /// This event is raised when a mouse handling activity has been started on this mouse handler.
        /// </summary>
        public event EventHandler MouseActivityStarted;

        /// <summary>
        /// This event is raised when a mouse handling activity has been finished on this mouse handler.
        /// </summary>
        public event EventHandler MouseActivityFinished;

        /// <summary>
        /// Activates the scroll handler. If it is currently active then this method has no effect.
        /// </summary>
        public void ActivateMouseHandling()
        {
            if (!this.isMouseHandlingActive)
            {
                this.eventSource.MouseSensor.ButtonDown += this.OnMouseDown;
                this.eventSource.MouseSensor.Move += this.OnMouseMove;
                this.eventSource.MouseSensor.ButtonUp += this.OnMouseUp;
                this.eventSource.MouseSensor.DoubleClick += this.OnMouseDoubleClick;
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
                this.eventSource.MouseSensor.ButtonDown -= this.OnMouseDown;
                this.eventSource.MouseSensor.Move -= this.OnMouseMove;
                this.eventSource.MouseSensor.ButtonUp -= this.OnMouseUp;
                this.eventSource.MouseSensor.DoubleClick -= this.OnMouseDoubleClick;
                this.isMouseHandlingActive = false;
            }
        }

        #endregion Public interface

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
                    this.commandService.FastCommand(this.targetControl.DisplayedArea, evtArgs.Position);
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
                this.targetControl.SelectionBox = this.CalculateSelectionBox();
            }
            else if (this.CurrentMouseStatus == MouseStatus.Selecting)
            {
                /// Actualize the selection box
                this.selectionBoxCurrPosition = evtArgs.Position;
                this.targetControl.SelectionBox = this.CalculateSelectionBox();
            }
            else if (this.CurrentMouseStatus == MouseStatus.DoubleClicked)
            {
                this.CurrentMouseStatus = MouseStatus.Selecting;

                /// Actualize the selection box
                this.selectionBoxCurrPosition = evtArgs.Position;
                this.targetControl.SelectionBox = this.CalculateSelectionBox();
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
                this.commandService.Select(this.targetControl.DisplayedArea, evtArgs.Position);

                /// Selection box off.
                this.selectionBoxStartPosition = RCIntVector.Undefined;
                this.selectionBoxCurrPosition = RCIntVector.Undefined;
                this.targetControl.SelectionBox = this.CalculateSelectionBox();
            }
            else if (this.CurrentMouseStatus == MouseStatus.Selecting && evtArgs.Button == UIMouseButton.Left)
            {
                this.CurrentMouseStatus = MouseStatus.None;

                /// Handle the mouse event.
                TraceManager.WriteAllTrace(string.Format("SELECTION {0}", this.targetControl.SelectionBox), PresLogicTraceFilters.INFO);
                this.commandService.Select(this.targetControl.DisplayedArea, this.targetControl.SelectionBox);

                /// Selection box off.
                this.selectionBoxStartPosition = RCIntVector.Undefined;
                this.selectionBoxCurrPosition = RCIntVector.Undefined;
                this.targetControl.SelectionBox = this.CalculateSelectionBox();
            }
            else if (this.CurrentMouseStatus == MouseStatus.RightDown && evtArgs.Button == UIMouseButton.Right)
            {
                this.CurrentMouseStatus = MouseStatus.None;
            }
            else if (this.CurrentMouseStatus == MouseStatus.DoubleClicked && evtArgs.Button == UIMouseButton.Left)
            {
                /// Handle the mouse event.
                TraceManager.WriteAllTrace(string.Format("DOUBLE_CLICK {0}", evtArgs.Position), PresLogicTraceFilters.INFO);
                this.commandService.SelectType(this.targetControl.DisplayedArea, evtArgs.Position);

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
        }

        #endregion Mouse sensor event handling

        #region Internal methods

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
                    if (this.MouseActivityStarted != null) { this.MouseActivityStarted(this, new EventArgs()); }
                }
                else if (oldValue != MouseStatus.None && this.currentMouseStatus == MouseStatus.None)
                {
                    if (this.MouseActivityFinished != null) { this.MouseActivityFinished(this, new EventArgs()); }
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
        /// The event source for the scrolling.
        /// </summary>
        private UISensitiveObject eventSource;

        /// <summary>
        /// Reference to the target control.
        /// </summary>
        private IMapControl targetControl;

        /// <summary>
        /// This flag indicates whether the mouse handling is currently active or not.
        /// </summary>
        private bool isMouseHandlingActive;

        /// <summary>
        /// Reference to the command service.
        /// </summary>
        private ICommandService commandService;
    }
}
