using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.UI
{
    /// <summary>
    /// A scrollbar is the combination of a slider and 2 buttons. The slider part of the control can be dragged
    /// along the track to select one value from an interval. The user can also use the 2 buttons to move the
    /// slider up/down in case of vertical or left/right in case of horizontal scrollbars.
    /// </summary>
    public class UIScrollBar : UIControl
    {
        #region Type definitions

        /// <summary>
        /// Enumerates the possible alignments of a UIScrollBar control.
        /// </summary>
        public enum Alignment
        {
            Horizontal = 0,     /// Horizontal alignment
            Vertical = 1        /// Vertical alignment
        }

        /// <summary>
        /// The static settings of a UIScrollBar.
        /// </summary>
        public struct Settings
        {
            /// <summary>
            /// Defines the extension of the 2 buttons of the UIScrollBar along it's alignment.
            /// </summary>
            public int ButtonExtension;

            /// <summary>
            /// Defines the radius of the slider-button of the UIScrollBar along it's alignment.
            /// </summary>
            public int SliderButtonRadius;

            /// <summary>
            /// Defines the length of the interval that this UIScrollBar can select values from. If IntervalLength is N then
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
            /// Defines the minimum time should be elapsed between stepping the slider with the buttons.
            /// </summary>
            public int TimeBetweenSteps;

            /// <summary>
            /// Defines the amount of change in the selected value when stepping the slider with the buttons.
            /// </summary>
            public int StepValueChange;

            /// <summary>
            /// The alignment of this UIScrollBar control (horizontal or vertical).
            /// </summary>
            public Alignment Alignment;
        }

        #endregion Type definitions
        
        #region Event and delegate definitions

        /// <summary>
        /// Occurs when the selected value of this UIScrollBar has been changed by an input device.
        /// </summary>
        public event UIInputEventHdl SelectedValueChanged;

        #endregion Event and delegate definitions

        /// <summary>
        /// Constructs a UIScrollBar object.
        /// </summary>
        /// <param name="position">The position of the UIScrollBar control.</param>
        /// <param name="size">The size of the UIScrollBar control.</param>
        /// <param name="settings">The settings of this UIScrollBar control.</param>
        public UIScrollBar(RCIntVector position, RCIntVector size, Settings settings)
            : base(position, size)
        {
            /// Parameter checking.
            if (settings.ButtonExtension <= 0) { throw new ArgumentOutOfRangeException("settings.ButtonExtension"); }
            if (settings.SliderButtonRadius < 0) { throw new ArgumentOutOfRangeException("settings.SliderButtonRadius"); }
            if (settings.IntervalLength <= 0) { throw new ArgumentOutOfRangeException("settings.IntervalLength"); }
            if (settings.TimeBetweenTrackings <= 0) { throw new ArgumentOutOfRangeException("settings.TimeBetweenTrackings"); }
            if (settings.TrackingValueChange <= 0) { throw new ArgumentOutOfRangeException("settings.TrackingValueChange"); }
            if (settings.TimeBetweenSteps <= 0) { throw new ArgumentOutOfRangeException("settings.TimeBetweenSteps"); }
            if (settings.StepValueChange <= 0) { throw new ArgumentOutOfRangeException("settings.StepValueChange"); }
            if (2 * settings.ButtonExtension + 2 * settings.SliderButtonRadius + 1 >= (settings.Alignment == Alignment.Horizontal ? size.X : size.Y)) { throw new ArgumentOutOfRangeException("size", "Scrollbar is too short!"); }
            if (settings.Alignment == Alignment.Horizontal ? size.Y % 2 == 0 : size.X % 2 == 0) { throw new ArgumentException("Width of the scrollbar must be odd!", "size"); }

            /// Save settings.
            this.alignment = settings.Alignment;
            this.timeBetweenSteps = settings.TimeBetweenSteps;
            this.stepValueChange = settings.StepValueChange;
            this.intervalLength = settings.IntervalLength;
            this.timeBetweenTrackings = settings.TimeBetweenTrackings;

            /// Create the buttons of the scrollbar.
            this.increaseButton = new UIButton(
                settings.Alignment == Alignment.Horizontal ? new RCIntVector(size.X - settings.ButtonExtension, 0)
                                                           : new RCIntVector(0, size.Y - settings.ButtonExtension),
                settings.Alignment == Alignment.Horizontal ? new RCIntVector(settings.ButtonExtension, size.Y)
                                                           : new RCIntVector(size.X, settings.ButtonExtension));
            this.decreaseButton = new UIButton(
                new RCIntVector(0, 0),
                settings.Alignment == Alignment.Horizontal ? new RCIntVector(settings.ButtonExtension, size.Y)
                                                           : new RCIntVector(size.X, settings.ButtonExtension));

            this.pushedButton = null;
            this.timeSinceLastStep = 0;

            /// Create the slider part of the scrollbar.
            UISlider.Settings sliderSettings = new UISlider.Settings()
            {
                Alignment = settings.Alignment == Alignment.Horizontal ? UISlider.Alignment.Horizontal : UISlider.Alignment.Vertical,
                IntervalLength = settings.IntervalLength,
                SliderBottom = settings.Alignment == Alignment.Horizontal ? size.Y / 2 : settings.SliderButtonRadius,
                SliderLeft = settings.Alignment == Alignment.Horizontal ? settings.SliderButtonRadius : size.X / 2,
                SliderRight = settings.Alignment == Alignment.Horizontal ? settings.SliderButtonRadius : size.X / 2,
                SliderTop = settings.Alignment == Alignment.Horizontal ? size.Y / 2 : settings.SliderButtonRadius,
                TimeBetweenTrackings = settings.TimeBetweenTrackings,
                TrackingValueChange = settings.TrackingValueChange,
                TrackPos = settings.Alignment == Alignment.Horizontal ?
                           new RCIntVector(settings.SliderButtonRadius, size.Y / 2) :
                           new RCIntVector(size.X / 2, settings.SliderButtonRadius),
                TrackSize = settings.Alignment == Alignment.Horizontal ?
                           new RCIntVector(size.X - 2 * settings.ButtonExtension - 2 * settings.SliderButtonRadius, size.Y / 2) :
                           new RCIntVector(size.Y - 2 * settings.ButtonExtension - 2 * settings.SliderButtonRadius, size.X / 2)
            };

            this.sliderControl = new UISlider(
                settings.Alignment == Alignment.Horizontal ? new RCIntVector(settings.ButtonExtension, 0)
                                                           : new RCIntVector(0, settings.ButtonExtension),
                settings.Alignment == Alignment.Horizontal ? new RCIntVector(size.X - 2 * settings.ButtonExtension, size.Y)
                                                           : new RCIntVector(size.X, size.Y - 2 * settings.ButtonExtension),
                sliderSettings,
                true);  /// WARNING: calling this constructor is only a hack.

            /// Attach the buttons and the slider part.
            this.Attach(this.sliderControl);
            this.Attach(this.increaseButton);
            this.Attach(this.decreaseButton);
            this.AttachSensitive(this.sliderControl);
            this.AttachSensitive(this.increaseButton);
            this.AttachSensitive(this.decreaseButton);

            /// Subscribe to events
            this.decreaseButton.Pushed += this.OnButtonPushed;
            this.decreaseButton.Released += this.OnButtonReleased;
            this.increaseButton.Pushed += this.OnButtonPushed;
            this.increaseButton.Released += this.OnButtonReleased;
            this.sliderControl.SelectedValueChanged += this.OnSelectedValueChanged;
        }

        /// <summary>
        /// Gets the slider rectangle in the coordinate-system of this UIScrollBar control.
        /// </summary>
        public RCIntRectangle SliderRectangle { get { return this.sliderControl.SliderRectangle + this.sliderControl.Position; } }

        /// <summary>
        /// Gets or sets the selected value from the interval.
        /// </summary>
        public int SelectedValue
        {
            get { return this.sliderControl.SelectedValue; }
            set { this.sliderControl.SelectedValue = value; }
        }

        /// <see cref="UISensitiveObject.ResetState"/>
        public override void ResetState()
        {
            base.ResetState();
            this.pushedButton = null;
            this.timeSinceLastStep = 0;
            UIRoot.Instance.SystemEventQueue.Unsubscribe<UIUpdateSystemEventArgs>(this.OnFrameUpdate);
        }

        #region Event handlers

        /// <summary>
        /// Called when the selected value on the slider control has been changed.
        /// </summary>
        /// <param name="sender">Reference to the slider control.</param>
        private void OnSelectedValueChanged(UISensitiveObject sender)
        {
            if (this.SelectedValueChanged != null)
            {
                this.SelectedValueChanged(this);
            }
        }

        /// <summary>
        /// Called when the increase or decrease button has been pushed by an input device.
        /// </summary>
        /// <param name="sender">Reference to the sender button.</param>
        private void OnButtonPushed(UISensitiveObject sender)
        {
            this.pushedButton = (UIButton)sender;

            this.timeSinceLastStep = 0;
            if (this.pushedButton == this.decreaseButton)
            {
                this.sliderControl.SelectedValue =
                    Math.Max(0, this.sliderControl.SelectedValue - this.stepValueChange);
            }
            else if (this.pushedButton == this.increaseButton)
            {
                this.sliderControl.SelectedValue =
                    Math.Min(this.intervalLength - 1, this.sliderControl.SelectedValue + this.stepValueChange);
            }

            UIRoot.Instance.SystemEventQueue.Subscribe<UIUpdateSystemEventArgs>(this.OnFrameUpdate);
        }

        /// <summary>
        /// Called when the increase or decrease button has been released by an input device.
        /// </summary>
        /// <param name="sender">Reference to the sender button.</param>
        private void OnButtonReleased(UISensitiveObject sender)
        {
            this.pushedButton = null;
            this.timeSinceLastStep = 0;
            UIRoot.Instance.SystemEventQueue.Unsubscribe<UIUpdateSystemEventArgs>(this.OnFrameUpdate);
        }

        /// <summary>
        /// This method is called on every frame update.
        /// </summary>
        /// <param name="evtArgs">Contains timing informations.</param>
        private void OnFrameUpdate(UIUpdateSystemEventArgs evtArgs)
        {
            if (this.IsEnabled && this.pushedButton != null)
            {
                this.timeSinceLastStep += evtArgs.TimeSinceLastUpdate;
                if (this.timeSinceLastStep > this.timeBetweenSteps)
                {
                    this.timeSinceLastStep = 0;
                    if (this.pushedButton == this.decreaseButton)
                    {
                        this.sliderControl.SelectedValue =
                            Math.Max(0, this.sliderControl.SelectedValue - this.stepValueChange);
                    }
                    else if (this.pushedButton == this.increaseButton)
                    {
                        this.sliderControl.SelectedValue =
                            Math.Min(this.intervalLength - 1, this.sliderControl.SelectedValue + this.stepValueChange);
                    }
                }
            }
        }

        #endregion Event handlers

        /// <summary>
        /// The alignment of this UIScrollBar.
        /// </summary>
        private Alignment alignment;

        /// <summary>
        /// The slider part of this UIScrollBar.
        /// </summary>
        private UISlider sliderControl;

        /// <summary>
        /// The increasing button of this UIScrollBar.
        /// </summary>
        private UIButton increaseButton;

        /// <summary>
        /// The decreasing button of this UIScrollBar.
        /// </summary>
        private UIButton decreaseButton;

        /// <summary>
        /// Reference to the button that is currently pushed or null if none of the buttons are pushed.
        /// </summary>
        private UIButton pushedButton;

        /// <summary>
        /// The minimum time should be elapsed between stepping the scrollbar with the buttons in milliseconds.
        /// </summary>
        private int timeBetweenSteps;

        /// <summary>
        /// The minimum time should be elapsed between trackings in milliseconds. Needed for areas that doesn't
        /// belong to the underlying slider control but belongs to the slider part of the scrollbar.
        /// </summary>
        private int timeBetweenTrackings;

        /// <summary>
        /// The amount of change in the selected value when stepping the slider with the buttons.
        /// </summary>
        private int stepValueChange;

        /// <summary>
        /// The length of the interval that this UIScrollBar can select values from. If intervalLength is N then
        /// the possible values are between 0 and N-1 inclusive.
        /// </summary>
        private int intervalLength;

        /// <summary>
        /// Temporary storage of the elapsed time since the last stepping of the slider with the buttons.
        /// </summary>
        private int timeSinceLastStep;
    }
}
