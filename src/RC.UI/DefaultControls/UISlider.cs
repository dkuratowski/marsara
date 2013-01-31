using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Common.Diagnostics;

namespace RC.UI
{
    /// <summary>
    /// Represents a control that has a slider part and a horizontal or vertical track. The slider part of the control can be
    /// dragged along the track to select one value from an interval.
    /// </summary>
    public class UISlider : UIControl
    {
        #region Type definitions

        /// <summary>
        /// Enumerates the possible alignments of a UISlider control.
        /// </summary>
        public enum Alignment
        {
            Horizontal = 0,     /// Horizontal alignment
            Vertical = 1        /// Vertical alignment
        }

        /// <summary>
        /// The static settings of a UISlider.
        /// </summary>
        public struct Settings
        {
            /// <summary>
            /// The position of the beginning of the track in the local coordinate system.
            /// </summary>
            public RCIntVector TrackPos;

            /// <summary>
            /// The size of the track. The X coordinate of this vector defines the length of the track and the Y coordinate
            /// defines the radius of the track.
            /// </summary>
            public RCIntVector TrackSize;

            /// <summary>
            /// Defines the distance of the left edge of the slider part in it's own coordinate system.
            /// </summary>
            public int SliderLeft;

            /// <summary>
            /// Defines the distance of the right edge of the slider part in it's own coordinate system.
            /// </summary>
            public int SliderRight;

            /// <summary>
            /// Defines the distance of the top of the slider part in it's own coordinate system.
            /// </summary>
            public int SliderTop;

            /// <summary>
            /// Defines the distance of the bottom of the slider part in it's own coordinate system.
            /// </summary>
            public int SliderBottom;

            /// <summary>
            /// Defines the length of the interval that this UISlider can select values from. If IntervalLength is N then
            /// the possible values are between 0 and N-1 inclusive.
            /// </summary>
            public int IntervalLength;

            /// <summary>
            /// Defines the minimum time should be elapsed between trackings in milliseconds.
            /// </summary>
            public int TimeBetweenTrackings;

            /// <summary>
            /// Defines the amount of change in the selected value during a tracking operation.
            /// </summary>
            public int TrackingValueChange;

            /// <summary>
            /// The alignment of this UISlider control (horizontal or vertical).
            /// </summary>
            public Alignment Alignment;
        }

        #endregion Type definitions

        #region Event and delegate definitions

        /// <summary>
        /// Occurs when the selected value of this UISlider has been changed by an input device.
        /// </summary>
        public event UIInputEventHdl SelectedValueChanged;

        #endregion Event and delegate definitions

        /// <summary>
        /// Constructs a UISlider object.
        /// </summary>
        /// <param name="position">The position of the UISlider control.</param>
        /// <param name="size">The size of the UISlider control.</param>
        /// <param name="settings">The settings of this UISlider control.</param>
        public UISlider(RCIntVector position,
                        RCIntVector size,
                        Settings settings) : base(position, size)
        {
            /// Parameter checking.
            if (settings.TrackPos == RCIntVector.Undefined) { throw new ArgumentNullException("settings.TrackPos"); }
            if (settings.TrackSize == RCIntVector.Undefined) { throw new ArgumentNullException("settings.TrackSize"); }
            if (settings.TrackSize.X <= 0 || settings.TrackSize.Y <= 0) { throw new ArgumentOutOfRangeException("settings.TrackSize"); }
            if (settings.SliderLeft < (settings.Alignment == Alignment.Vertical ? settings.TrackSize.Y : 0)) { throw new ArgumentOutOfRangeException("settings.SliderLeft"); }
            if (settings.SliderRight < (settings.Alignment == Alignment.Vertical ? settings.TrackSize.Y : 0)) { throw new ArgumentOutOfRangeException("settings.SliderRight"); }
            if (settings.SliderTop < (settings.Alignment == Alignment.Horizontal ? settings.TrackSize.Y : 0)) { throw new ArgumentOutOfRangeException("settings.SliderTop"); }
            if (settings.SliderBottom < (settings.Alignment == Alignment.Horizontal ? settings.TrackSize.Y : 0)) { throw new ArgumentOutOfRangeException("settings.SliderBottom"); }
            if (settings.IntervalLength <= 0) { throw new ArgumentOutOfRangeException("settings.IntervalLength"); }
            if (settings.TimeBetweenTrackings <= 0) { throw new ArgumentOutOfRangeException("settings.TimeBetweenTrackings"); }
            if (settings.TrackingValueChange <= 0) { throw new ArgumentOutOfRangeException("settings.TrackingValueChange"); }

            /// Set the static settings of this UISlider.
            this.alignment = settings.Alignment;
            this.trackLength = settings.TrackSize.X;
            this.trackRectangle = this.alignment == Alignment.Horizontal
                                ? new RCIntRectangle(0, -settings.TrackSize.Y, settings.TrackSize.X, 2 * settings.TrackSize.Y + 1)
                                : new RCIntRectangle(-settings.TrackSize.Y, 0, 2 * settings.TrackSize.Y + 1, settings.TrackSize.X);
            this.sliderRectangle = new RCIntRectangle(-settings.SliderLeft,
                                                   -settings.SliderTop,
                                                   settings.SliderLeft + settings.SliderRight + 1,
                                                   settings.SliderTop + settings.SliderBottom + 1);
            this.trackPosition = settings.TrackPos;
            this.intervalLength = settings.IntervalLength;
            this.timeBetweenTrackings = settings.TimeBetweenTrackings;
            this.trackingValueChange = settings.TrackingValueChange;

            this.activatorBtn = UIMouseButton.Undefined;
            this.scaling = (float)(this.intervalLength - 1) / (float)(this.alignment == Alignment.Horizontal ?
                                                                                   this.trackRectangle.Width :
                                                                                   this.trackRectangle.Height);
            this.timeSinceLastTracking = 0;
            this.lastKnownMousePosition = new RCIntVector(0, 0);

            /// Set the initial status of this UISlider.
            this.sliderPosition = 0;
            this.selectedValue = 0;
            this.currentStatus = Status.Normal;
            this.trackingDirection = TrackingDirection.Decreasing;
            this.internalValueChange = false;

            this.trackRectCache = new CachedValue<RCIntRectangle>(this.ComputeTrackRectangle);
            this.sliderRectCache = new CachedValue<RCIntRectangle>(this.ComputeSliderRectangle);

            this.MouseSensor.Move += this.OnMouseMove;
            this.MouseSensor.ButtonDown += this.OnMouseDown;
            this.MouseSensor.ButtonUp += this.OnMouseUp;
        }

        /// <summary>
        /// Constructs a UISlider object.
        /// </summary>
        /// <param name="position">The position of the UISlider control.</param>
        /// <param name="size">The size of the UISlider control.</param>
        /// <param name="settings">The settings of this UISlider control.</param>
        /// <param name="scrollbarStyle">
        /// The flag that indicates whether this is a scrollbar-style UISlider or not.
        /// </param>
        /// <remarks>WARNING! This internal constructor is a hack only for implementing UIScrollBar!</remarks>
        internal UISlider(RCIntVector position, RCIntVector size, Settings settings, bool scrollbarStyle)
            : this(position, size, settings)
        {
            this.scrollbarStyle = scrollbarStyle;
            if (this.scrollbarStyle)
            {
                /// Compute the extended track-rectangle
                this.extendedTrackRect = this.alignment == Alignment.Horizontal
                                       ? new RCIntRectangle(-settings.SliderLeft, -settings.TrackSize.Y, settings.TrackSize.X + settings.SliderLeft + settings.SliderRight, 2 * settings.TrackSize.Y + 1)
                                       : new RCIntRectangle(-settings.TrackSize.Y, -settings.SliderTop, 2 * settings.TrackSize.Y + 1, settings.TrackSize.X + settings.SliderTop + settings.SliderBottom);
            }
        }
        private bool scrollbarStyle;
        private RCIntRectangle extendedTrackRect;

        /// <summary>
        /// Gets or sets the selected value from the interval.
        /// </summary>
        public int SelectedValue
        {
            get { return this.selectedValue; }
            set
            {
                if (value < 0 || value >= this.intervalLength) { throw new ArgumentOutOfRangeException("SelectedValue"); }

                int prevIdx = this.selectedValue;
                this.selectedValue = value;
                if (!this.internalValueChange)
                {
                    this.sliderPosition = this.ComputePositionFromValue();
                    this.sliderRectCache.Invalidate();
                }
                if (this.SelectedValueChanged != null && this.selectedValue != prevIdx)
                {
                    this.SelectedValueChanged(this);
                }
            }
        }
        private bool internalValueChange;

        /// <see cref="UISensitiveObject.ResetState"/>
        public override void ResetState()
        {
            base.ResetState();
            this.activatorBtn = UIMouseButton.Undefined;
            this.timeSinceLastTracking = 0;
            this.lastKnownMousePosition = new RCIntVector(0, 0);
            this.sliderPosition = this.ComputePositionFromValue();
            this.sliderRectCache.Invalidate();
            this.currentStatus = Status.Normal;
            this.trackingDirection = TrackingDirection.Decreasing;
            UIRoot.Instance.SystemEventQueue.Unsubscribe<UIUpdateSystemEventArgs>(this.OnFrameUpdate);
        }

        #region Status data

        /// <summary>
        /// Enumerates the possible states of a UISlider control.
        /// </summary>
        protected enum Status
        {
            Normal = 0,
            DraggingSlider = 1,
            Tracking = 2,
            TrackingPaused = 3
        }

        /// <summary>
        /// Enumerates the possible directions of a tracking operation on a UISlider control.
        /// </summary>
        protected enum TrackingDirection
        {
            Decreasing = 0,
            Increasing = 1
        }

        /// <summary>
        /// Gets the current status of this UISlider.
        /// </summary>
        protected Status CurrentStatus { get { return this.currentStatus; } }

        /// <summary>
        /// Gets the current tracking direction of this UISlider. Available only in Status.Tracking and
        /// Status.TrackingPaused states.
        /// </summary>
        protected TrackingDirection TrackingDir
        {
            get
            {
                if (this.currentStatus != Status.Tracking && this.currentStatus != Status.TrackingPaused)
                {
                    throw new InvalidOperationException("TrackingDirection is only available in Status.Tracking or Status.TrackingPaused states!");
                }
                return this.trackingDirection;
            }
        }

        /// <summary>
        /// Gets the track rectangle in the coordinate-system of this UISlider control.
        /// </summary>
        public RCIntRectangle TrackRectangle { get { return this.trackRectCache.Value; } }

        /// <summary>
        /// Gets the slider rectangle in the coordinate-system of this UISlider control.
        /// </summary>
        public RCIntRectangle SliderRectangle { get { return this.sliderRectCache.Value; } }

        #endregion Status data

        #region Mouse sensor event handlers

        /// <summary>
        /// Called when the mouse pointer moved over the range of this UISlider.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="evtArgs">The details of the event.</param>
        private void OnMouseMove(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            this.lastKnownMousePosition = evtArgs.Position;

            if (this.IsEnabled)
            {
                if (this.currentStatus == Status.DraggingSlider)
                {
                    /// Shift the slider and modify the selected value if necessary.
                    if (this.ShiftSlider(evtArgs.Position))
                    {
                        this.internalValueChange = true;
                        this.SelectedValue = this.ComputeValueFromPosition();
                        this.internalValueChange = false;
                    }
                }
                else if (this.currentStatus == Status.Tracking)
                {
                    /// Change the state of the control
                    if (this.IsInsideSlider(evtArgs.Position) ||
                        !this.IsInsideTrack(evtArgs.Position) ||
                        this.ComputeTrackingDirection(evtArgs.Position) != this.trackingDirection)
                    {
                        this.currentStatus = Status.TrackingPaused;
                        UIRoot.Instance.SystemEventQueue.Unsubscribe<UIUpdateSystemEventArgs>(this.OnFrameUpdate);
                    }
                }
                else if (this.currentStatus == Status.TrackingPaused)
                {
                    /// Change the state of the control
                    if (!this.IsInsideSlider(evtArgs.Position) &&
                        this.IsInsideTrack(evtArgs.Position) &&
                        this.ComputeTrackingDirection(evtArgs.Position) == this.trackingDirection)
                    {
                        this.timeSinceLastTracking = 0;
                        this.currentStatus = Status.Tracking;
                        UIRoot.Instance.SystemEventQueue.Subscribe<UIUpdateSystemEventArgs>(this.OnFrameUpdate);
                    }
                }
            }
        }

        /// <summary>
        /// Called when a mouse button has been pushed over the range of this UISlider.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="evtArgs">The details of the event.</param>
        private void OnMouseDown(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            this.lastKnownMousePosition = evtArgs.Position;

            if (this.IsEnabled && this.activatorBtn == UIMouseButton.Undefined &&
                evtArgs.Button == UIMouseButton.Left)
            {
                this.activatorBtn = evtArgs.Button;
                if (this.currentStatus == Status.Normal)
                {
                    if (this.IsInsideSlider(evtArgs.Position))
                    {
                        this.currentStatus = Status.DraggingSlider;

                        /// Shift the slider and modify the selected value if necessary.
                        if (this.ShiftSlider(evtArgs.Position))
                        {
                            this.internalValueChange = true;
                            this.SelectedValue = this.ComputeValueFromPosition();
                            this.internalValueChange = false;
                        }
                    }
                    else if (this.IsInsideTrack(evtArgs.Position))
                    {
                        this.trackingDirection = this.ComputeTrackingDirection(evtArgs.Position);
                        this.timeSinceLastTracking = 0;

                        /// Perform the tracking operation
                        if (this.trackingDirection == TrackingDirection.Decreasing)
                        {
                            this.SelectedValue = Math.Max(0, this.selectedValue - this.trackingValueChange);
                        }
                        else
                        {
                            this.SelectedValue =
                                Math.Min(this.intervalLength - 1, this.selectedValue + this.trackingValueChange);
                        }

                        /// Change the state of the control
                        if (!this.IsInsideSlider(evtArgs.Position) &&
                            this.IsInsideTrack(evtArgs.Position) &&
                            this.ComputeTrackingDirection(evtArgs.Position) == this.trackingDirection)
                        {
                            this.currentStatus = Status.Tracking;
                            UIRoot.Instance.SystemEventQueue.Subscribe<UIUpdateSystemEventArgs>(this.OnFrameUpdate);
                        }
                        else
                        {
                            this.currentStatus = Status.TrackingPaused;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Called when a mouse button has been released over the range of this UISlider.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="evtArgs">The details of the event.</param>
        private void OnMouseUp(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            this.lastKnownMousePosition = evtArgs.Position;

            if (this.IsEnabled && this.activatorBtn == UIMouseButton.Left &&
                evtArgs.Button == UIMouseButton.Left)
            {
                this.activatorBtn = UIMouseButton.Undefined;
                if (this.currentStatus == Status.DraggingSlider ||
                    this.currentStatus == Status.Tracking ||
                    this.currentStatus == Status.TrackingPaused)
                {
                    if (this.currentStatus == Status.DraggingSlider)
                    {
                        this.sliderPosition = this.ComputePositionFromValue();
                        this.sliderRectCache.Invalidate();
                    }
                    this.currentStatus = Status.Normal;
                    UIRoot.Instance.SystemEventQueue.Unsubscribe<UIUpdateSystemEventArgs>(this.OnFrameUpdate);
                }
            }
        }

        /// <summary>
        /// The UIMouseButton that activated the mouse sensor of this UISlider.
        /// </summary>
        private UIMouseButton activatorBtn;

        #endregion Mouse sensor event handlers

        /// <summary>
        /// Shifts the slider part of the control according to the given position.
        /// </summary>
        /// <param name="position">The target position in the local coordinate system of this UISlider control.</param>
        /// <returns>True if the position of the slider has changed, false otherwise.</returns>
        private bool ShiftSlider(RCIntVector position)
        {
            RCIntVector posInTrackSpace = position - this.trackPosition;
            int prevPosition = this.sliderPosition;
            this.sliderPosition = (this.alignment == Alignment.Horizontal)
                                ? Math.Min(Math.Max(0, posInTrackSpace.X), this.trackRectangle.Right - 1)
                                : Math.Min(Math.Max(0, posInTrackSpace.Y), this.trackRectangle.Bottom - 1);
            this.sliderRectCache.Invalidate();
            return prevPosition != this.sliderPosition;
        }

        /// <summary>
        /// Checks whether the given position is inside the slider part or not.
        /// </summary>
        /// <param name="position">The checked position in the local coordinate system of this UISlider control.</param>
        /// <returns>True if the checked position is inside the slider part, false otherwise.</returns>
        private bool IsInsideSlider(RCIntVector position)
        {
            RCIntVector posInTrackSpace = position - this.trackPosition;
            RCIntRectangle sliderInTrackSpace = this.sliderRectangle
                                           + (this.alignment == Alignment.Horizontal ?
                                                new RCIntVector(this.sliderPosition, 0) :
                                                new RCIntVector(0, this.sliderPosition));
            return sliderInTrackSpace.Contains(posInTrackSpace);
        }

        /// <summary>
        /// Checks whether the given position is inside the track rectangle or not.
        /// </summary>
        /// <param name="position">The checked position in the local coordinate system of this UISlider control.</param>
        /// <returns>True if the checked position is inside the track rectangle, false otherwise.</returns>
        private bool IsInsideTrack(RCIntVector position)
        {
            RCIntVector posInTrackSpace = position - this.trackPosition;
            if (!this.scrollbarStyle)
            {
                return this.trackRectangle.Contains(posInTrackSpace);
            }
            else
            {
                return this.extendedTrackRect.Contains(posInTrackSpace);
            }
        }

        /// <summary>
        /// Computes the selected value from the current position of the slider.
        /// </summary>
        private int ComputeValueFromPosition()
        {
            if (this.sliderPosition == 0)
            {
                return 0;
            }
            else if (this.sliderPosition == (this.alignment == Alignment.Horizontal ?
                                                          this.trackRectangle.Width :
                                                          this.trackRectangle.Height) - 1)
            {
                return this.intervalLength - 1;
            }
            else if (this.sliderPosition > 0 && this.sliderPosition < (this.alignment == Alignment.Horizontal ?
                                                                                    this.trackRectangle.Width :
                                                                                    this.trackRectangle.Height) - 1)
            {
                return Math.Min((int)Math.Round((float)this.sliderPosition * this.scaling), this.intervalLength - 1);
            }
            else
            {
                throw new UIException("Unexpected slider position!");
            }
        }

        /// <summary>
        /// Computes the position of the slider from the currently selected value.
        /// </summary>
        /// <returns></returns>
        private int ComputePositionFromValue()
        {
            if (this.selectedValue == 0)
            {
                return 0;
            }
            else if (this.selectedValue == this.intervalLength - 1)
            {
                return (this.alignment == Alignment.Horizontal ? this.trackRectangle.Width : this.trackRectangle.Height) - 1;
            }
            else if (this.selectedValue > 0 && this.selectedValue < this.intervalLength - 1)
            {
                return Math.Min((int)Math.Round((float)this.selectedValue / this.scaling), this.trackLength - 1);
            }
            else
            {
                throw new UIException("Unexpected selected value!");
            }
        }

        /// <summary>
        /// Computes the tracking direction from the given position.
        /// </summary>
        /// <param name="position">The checked position in the local coordinate system of this UISlider control.</param>
        private TrackingDirection ComputeTrackingDirection(RCIntVector position)
        {
            RCIntVector posInTrackSpace = position - this.trackPosition;
            return (this.alignment == Alignment.Horizontal ?
                                         posInTrackSpace.X :
                                         posInTrackSpace.Y) >= this.sliderPosition ?
                                                      TrackingDirection.Increasing :
                                                      TrackingDirection.Decreasing;
        }

        /// <summary>
        /// This method is called on every frame update.
        /// </summary>
        /// <param name="evtArgs">Contains timing informations.</param>
        private void OnFrameUpdate(UIUpdateSystemEventArgs evtArgs)
        {
            if (this.IsEnabled)
            {
                if (this.currentStatus == Status.Tracking)
                {
                    this.timeSinceLastTracking += evtArgs.TimeSinceLastUpdate;
                    if (this.timeSinceLastTracking > this.timeBetweenTrackings)
                    {
                        this.timeSinceLastTracking = 0;
                        /// Perform the tracking operation
                        if (this.trackingDirection == TrackingDirection.Decreasing)
                        {
                            this.SelectedValue =
                                Math.Max(0, this.selectedValue - this.trackingValueChange);
                        }
                        else
                        {
                            this.SelectedValue =
                                Math.Min(this.intervalLength - 1, this.selectedValue + this.trackingValueChange);
                        }

                        /// Change the state of the control
                        if (this.IsInsideSlider(this.lastKnownMousePosition) ||
                            !this.IsInsideTrack(this.lastKnownMousePosition) ||
                            this.ComputeTrackingDirection(this.lastKnownMousePosition) != this.trackingDirection)
                        {
                            this.currentStatus = Status.TrackingPaused;
                            UIRoot.Instance.SystemEventQueue.Unsubscribe<UIUpdateSystemEventArgs>(this.OnFrameUpdate);
                        }

                        /// WARNING: UIMouseManager.RaiseMovementEvents doesn't raise the OnMove event for the
                        ///          active UISensitiveObject in the same cycle the OnEnter or OnLeave event has
                        ///          been raised.
                    }                    
                }
            }
        }

        /// <summary>
        /// Computes the track rectangle in the local coordinate system of this UISlider.
        /// </summary>
        private RCIntRectangle ComputeTrackRectangle()
        {
            return this.alignment == Alignment.Horizontal
                ? new RCIntRectangle(this.trackPosition.X, this.trackPosition.Y + this.trackRectangle.Top,
                                  this.trackRectangle.Width, this.trackRectangle.Height)
                : new RCIntRectangle(this.trackPosition.X + this.trackRectangle.Left, this.trackPosition.Y,
                                  this.trackRectangle.Width, this.trackRectangle.Height);
        }

        /// <summary>
        /// Computes the slider rectangle in the local coordinate system of this UISlider.
        /// </summary>
        private RCIntRectangle ComputeSliderRectangle()
        {
            return this.alignment == Alignment.Horizontal
                ? new RCIntRectangle(this.trackPosition.X + this.sliderPosition + this.sliderRectangle.Left, this.trackPosition.Y + this.sliderRectangle.Top,
                                  this.sliderRectangle.Width, this.sliderRectangle.Height)
                : new RCIntRectangle(this.trackPosition.X + this.sliderRectangle.Left, this.trackPosition.Y + this.sliderPosition + this.sliderRectangle.Top,
                                  this.sliderRectangle.Width, this.sliderRectangle.Height);
        }

        /// <summary>
        /// The alignment of this UISlider control.
        /// </summary>
        private Alignment alignment;

        /// <summary>
        /// The area of the track in it's own coordinate system.
        /// </summary>
        private RCIntRectangle trackRectangle;

        /// <summary>
        /// The area of the slider in it's own coordinate system.
        /// </summary>
        private RCIntRectangle sliderRectangle;

        /// <summary>
        /// The length of the track in pixels.
        /// </summary>
        private int trackLength;

        /// <summary>
        /// The position of the beginning of the track in the local coordinate system of this UISlider control.
        /// </summary>
        private RCIntVector trackPosition;

        /// <summary>
        /// The length of the interval that this UISlider can select values from. If intervalLength is N then
        /// the possible values are between 0 and N-1 inclusive.
        /// </summary>
        private int intervalLength;

        /// <summary>
        /// The minimum time should be elapsed between trackings in milliseconds.
        /// </summary>
        private int timeBetweenTrackings;

        /// <summary>
        /// The amount of change in the selected value during a tracking operation.
        /// </summary>
        private int trackingValueChange;

        /// <summary>
        /// The scaling of this UISlider (this.intervalLength / this.trackRectangle.Width).
        /// </summary>
        private float scaling;

        /// <summary>
        /// The position of the slider in the coordinate system of the track.
        /// </summary>
        private int sliderPosition;

        /// <summary>
        /// The currently selected value from the interval.
        /// </summary>
        private int selectedValue;

        /// <summary>
        /// The current status of this UISlider.
        /// </summary>
        private Status currentStatus;

        /// <summary>
        /// The current tracking direction of this UISlider. Relevant only in Status.Tracking and
        /// Status.TrackingPaused states.
        /// </summary>
        private TrackingDirection trackingDirection;

        /// <summary>
        /// Temporary storage of the elapsed time since the last tracking.
        /// </summary>
        private int timeSinceLastTracking;

        /// <summary>
        /// Temporary storage of the last known position of the mouse.
        /// </summary>
        private RCIntVector lastKnownMousePosition;

        /// <summary>
        /// Cache of the track rectangle in the local coordinate system of this UISlider.
        /// </summary>
        private CachedValue<RCIntRectangle> trackRectCache;

        /// <summary>
        /// Cache of the slider rectangle in the local coordinate system of this UISlider.
        /// </summary>
        private CachedValue<RCIntRectangle> sliderRectCache;
    }
}
