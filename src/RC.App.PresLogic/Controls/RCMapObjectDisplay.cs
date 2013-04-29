using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.PublicInterfaces;
using RC.UI;
using RC.Common;
using RC.Common.Diagnostics;

namespace RC.App.PresLogic.Controls
{
    /// <summary>
    /// Adds the following new functionalities to the map display control:
    ///     - displays the map objects
    ///     - indicates the selected map objects
    ///     - displays the selection box if a selection is currently in progress
    /// </summary>
    public class RCMapObjectDisplay : RCMapDisplayExtension
    {
        /// <summary>
        /// Constructs an RCMapObjectDisplay extension for the given map display control.
        /// </summary>
        /// <param name="extendedControl">The map display control to extend.</param>
        /// <param name="map">Reference to a map view.</param>
        public RCMapObjectDisplay(RCMapDisplay extendedControl, IMapView map)
            : base(extendedControl, map)
        {
            this.rectBrush = null;
            this.CurrentMouseStatus = MouseStatus.None;
            this.selectionBoxStartPosition = RCIntVector.Undefined;
            this.selectionBoxCurrPosition = RCIntVector.Undefined;
            this.isMouseHandlingActive = false;
        }

        /// <summary>
        /// This event is raised when a mouse handling activity has been started on this display.
        /// </summary>
        public event EventHandler MouseActivityStarted;

        /// <summary>
        /// This event is raised when a mouse handling activity has been finished on this display.
        /// </summary>
        public event EventHandler MouseActivityFinished;

        /// <summary>
        /// Activates the mouse handling of this display. If mouse handling is currently active then this method
        /// has no effect.
        /// </summary>
        public void ActivateMouseHandling()
        {
            if (!this.isMouseHandlingActive)
            {
                this.MouseSensor.ButtonDown += this.OnMouseDown;
                this.MouseSensor.Move += this.OnMouseMove;
                this.MouseSensor.ButtonUp += this.OnMouseUp;
                this.MouseSensor.DoubleClick += this.OnMouseDoubleClick;
                this.isMouseHandlingActive = true;
            }
        }

        /// <summary>
        /// Deactivates the mouse handling of this display. If mouse handling is currently inactive then this method
        /// has no effect.
        /// </summary>
        public void DeactivateMouseHandling()
        {
            if (this.isMouseHandlingActive)
            {
                this.MouseSensor.ButtonDown -= this.OnMouseDown;
                this.MouseSensor.Move -= this.OnMouseMove;
                this.MouseSensor.ButtonUp -= this.OnMouseUp;
                this.MouseSensor.DoubleClick -= this.OnMouseDoubleClick;
                this.isMouseHandlingActive = false;
            }
        }

        #region Overrides

        /// <see cref="UISensitiveObject.ResetState"/>
        public override void ResetState()
        {
            this.CurrentMouseStatus = MouseStatus.None;
            this.selectionBoxStartPosition = RCIntVector.Undefined;
            this.selectionBoxCurrPosition = RCIntVector.Undefined;
        }

        /// <see cref="RCMapDisplayExtension.StartExtension_i"/>
        protected override void StartExtension_i()
        {
        }

        /// <see cref="RCMapDisplayExtension.StopExtension_i"/>
        protected override void StopExtension_i()
        {
        }

        /// <see cref="RCMapDisplayExtension.StartExtensionProc_i"/>
        protected override void StartExtensionProc_i()
        {
            this.rectBrush = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(UIColor.LightGreen, new RCIntVector(1, 1), UIWorkspace.Instance.PixelScaling);
            this.rectBrush.Upload();
        }

        /// <see cref="RCMapDisplayExtension.StopExtensionProc_i"/>
        protected override void StopExtensionProc_i()
        {
            UIRoot.Instance.GraphicsPlatform.SpriteManager.DestroySprite(this.rectBrush);
            this.rectBrush = null;
        }

        /// <see cref="RCMapDisplayExtension.RenderExtension_i"/>
        protected override void RenderExtension_i(IUIRenderContext renderContext)
        {
            if (this.CurrentMouseStatus == MouseStatus.Selecting)
            {
                renderContext.RenderRectangle(this.rectBrush, this.CalculateSelectionBox());
            }
        }

        #endregion Overrides

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

        #region Mouse event handling

        /// <summary>
        /// Called when a mouse button has been pushed over the display.
        /// </summary>
        private void OnMouseDown(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            if (this.CurrentMouseStatus == MouseStatus.None)
            {
                if (evtArgs.Button == UIMouseButton.Right)
                {
                    /// TODO: send command to the selected units
                    TraceManager.WriteAllTrace("RIGHT_CLICK", PresLogicTraceFilters.INFO);

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
            }
            else if (this.CurrentMouseStatus == MouseStatus.Selecting)
            {
                /// Actualize the selection box
                this.selectionBoxCurrPosition = evtArgs.Position;
            }
            else if (this.CurrentMouseStatus == MouseStatus.DoubleClicked)
            {
                this.CurrentMouseStatus = MouseStatus.Selecting;

                /// Actualize the selection box
                this.selectionBoxCurrPosition = evtArgs.Position;
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

                /// TODO: handle single unit selection
                TraceManager.WriteAllTrace("LEFT_CLICK", PresLogicTraceFilters.INFO);

                /// Selection box off.
                this.selectionBoxStartPosition = RCIntVector.Undefined;
                this.selectionBoxCurrPosition = RCIntVector.Undefined;
            }
            else if (this.currentMouseStatus == MouseStatus.Selecting && evtArgs.Button == UIMouseButton.Left)
            {
                this.CurrentMouseStatus = MouseStatus.None;

                /// TODO: handle selection box
                TraceManager.WriteAllTrace(string.Format("SELECTION {0}", this.CalculateSelectionBox()), PresLogicTraceFilters.INFO);

                /// Selection box off.
                this.selectionBoxStartPosition = RCIntVector.Undefined;
                this.selectionBoxCurrPosition = RCIntVector.Undefined;
            }
            else if (this.CurrentMouseStatus == MouseStatus.RightDown && evtArgs.Button == UIMouseButton.Right)
            {
                this.CurrentMouseStatus = MouseStatus.None;
            }
            else if (this.CurrentMouseStatus == MouseStatus.DoubleClicked && evtArgs.Button == UIMouseButton.Left)
            {
                /// TODO: handle double click
                TraceManager.WriteAllTrace("DOUBLE_CLICK", PresLogicTraceFilters.INFO);

                this.CurrentMouseStatus = MouseStatus.None;
            }
        }

        #endregion Mouse event handling

        /// <summary>
        /// Calculates the current selection box in the coordinate-system of this display.
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
        /// Gets or sets the mouse status of this display.
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

        /// <summary>
        /// The brush for drawing the selection boxes.
        /// </summary>
        private UISprite rectBrush;

        /// <summary>
        /// The current mouse status of this display.
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
        /// This flag indicates whether the mouse handing of this display is active or not.
        /// </summary>
        private bool isMouseHandlingActive;
    }
}
