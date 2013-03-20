using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.UI;

namespace RC.App.PresLogic.Controls
{
    /// <summary>
    /// Represents a scrollbar in the RC application.
    /// </summary>
    public class RCScrollBar : UIScrollBar
    {
        #region Type definitions

        /// <summary>
        /// Enumerates the possible alignments of an RCScrollBar control.
        /// </summary>
        public new enum Alignment
        {
            Horizontal = 0,     /// Horizontal alignment
            Vertical = 1        /// Vertical alignment
        }

        /// <summary>
        /// The static settings of an RCScrollBar.
        /// </summary>
        public new struct Settings
        {
            /// <summary>
            /// Defines the length of the interval that this RCScrollBar can select values from. If IntervalLength is N then
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
            /// The alignment of this RCScrollBar control (horizontal or vertical).
            /// </summary>
            public Alignment Alignment;
        }

        #endregion Type definitions

        #region Constructors

        /// <summary>
        /// Creates a vertical scrollbar with the default settings.
        /// </summary>
        /// <param name="position">The position of the upper-left corner of the scrollbar.</param>
        /// <param name="length">The length of the scrollbar.</param>
        /// <param name="interval">
        /// Defines the length of the interval that this RCScrollBar can select values from. If interval is N then
        /// the possible values are between 0 and N-1 inclusive.
        /// </param>
        public RCScrollBar(RCIntVector position, int length, int interval) : this(position, length, interval, false)
        {
        }

        /// <summary>
        /// Creates a vertical or horizontal scrollbar with the default settings.
        /// </summary>
        /// <param name="position">The position of the upper-left corner of the scrollbar.</param>
        /// <param name="length">The length of the scrollbar.</param>
        /// <param name="interval">
        /// Defines the length of the interval that this RCScrollBar can select values from. If interval is N then
        /// the possible values are between 0 and N-1 inclusive.
        /// </param>
        /// <param name="isHorizontal">Set it true if you want to create a horizontal scrollbar.</param>
        public RCScrollBar(RCIntVector position, int length, int interval, bool isHorizontal)
            : this(position,
                   length,
                   new Settings()
                   {
                       Alignment = isHorizontal ? Alignment.Horizontal : Alignment.Vertical,
                       IntervalLength = interval,
                       StepValueChange = RCScrollBar.STEP_VALUE_CHANGE_DEFAULT,
                       TrackingValueChange = RCScrollBar.TRACKING_VALUE_CHANGE_DEFAULT,
                       TimeBetweenSteps = RCScrollBar.TIME_BETWEEN_STEPS_DEFAULT,
                       TimeBetweenTrackings = RCScrollBar.TIME_BETWEEN_TRACKINGS_DEFAULT
                   })
        {
        }

        /// <summary>
        /// Creates a vertical or horizontal scrollbar with custom settings.
        /// </summary>
        /// <param name="position">The position of the upper-left corner of the scrollbar.</param>
        /// <param name="length">The length of the scrollbar.</param>
        /// <param name="settings">The settings of the scrollbar.</param>
        public RCScrollBar(RCIntVector position, int length, Settings settings)
            : base(position,
                   settings.Alignment == Alignment.Horizontal ? new RCIntVector(length, RCScrollBar.WIDTH) : new RCIntVector(RCScrollBar.WIDTH, length),
                   new UIScrollBar.Settings()
                   {
                       Alignment = settings.Alignment == Alignment.Horizontal ? UIScrollBar.Alignment.Horizontal : UIScrollBar.Alignment.Vertical,
                       ButtonExtension = RCScrollBar.BUTTON_SIZE,
                       IntervalLength = settings.IntervalLength,
                       SliderButtonRadius = RCScrollBar.WIDTH / 2,
                       StepValueChange = settings.StepValueChange,
                       TimeBetweenSteps = settings.TimeBetweenSteps,
                       TrackingValueChange = settings.TrackingValueChange,
                       TimeBetweenTrackings = settings.TimeBetweenTrackings
                   })
        {
            this.controlSprite = UIResourceManager.GetResource<UISprite>(settings.Alignment == Alignment.Horizontal ? "RC.App.Sprites.ScrollbarHorz" : "RC.App.Sprites.ScrollbarVert");
            this.alignment = settings.Alignment;
            this.sliderTrackLength = length - 2 * RCScrollBar.BUTTON_SIZE;
        }

        #endregion Constructors

        /// <see cref="UIObject.Render_i"/>
        protected override void Render_i(IUIRenderContext renderContext)
        {
            /// Render the decreasing button.
            renderContext.RenderSprite(
                this.controlSprite,
                new RCIntVector(0, 0),
                this.alignment == Alignment.Horizontal ?
                    new RCIntRectangle(0, 0, RCScrollBar.BUTTON_SIZE, RCScrollBar.WIDTH) :
                    new RCIntRectangle(0, 0, RCScrollBar.WIDTH, RCScrollBar.BUTTON_SIZE));

            /// Render the track of the slider.
            for (int drawnSliderLength = 0; drawnSliderLength < this.sliderTrackLength; )
            {
                int toBeDrawnNow = Math.Min(this.sliderTrackLength - drawnSliderLength, RCScrollBar.SPRITE_SLIDERTRACK_LENGTH);

                renderContext.RenderSprite(
                    this.controlSprite,
                    this.alignment == Alignment.Horizontal ?
                        new RCIntVector(RCScrollBar.BUTTON_SIZE + drawnSliderLength, 0) :
                        new RCIntVector(0, RCScrollBar.BUTTON_SIZE + drawnSliderLength),
                    this.alignment == Alignment.Horizontal ?
                        new RCIntRectangle(2 * RCScrollBar.BUTTON_SIZE + RCScrollBar.WIDTH, 0, toBeDrawnNow, RCScrollBar.WIDTH) :
                        new RCIntRectangle(0, 2 * RCScrollBar.BUTTON_SIZE + RCScrollBar.WIDTH, RCScrollBar.WIDTH, toBeDrawnNow));

                drawnSliderLength += toBeDrawnNow;
            }

            /// Render the increasing button.
            renderContext.RenderSprite(
                this.controlSprite,
                this.alignment == Alignment.Horizontal ?
                    new RCIntVector(RCScrollBar.BUTTON_SIZE + this.sliderTrackLength, 0) :
                    new RCIntVector(0, RCScrollBar.BUTTON_SIZE + this.sliderTrackLength),
                this.alignment == Alignment.Horizontal ?
                    new RCIntRectangle(RCScrollBar.BUTTON_SIZE, 0, RCScrollBar.BUTTON_SIZE, RCScrollBar.WIDTH) :
                    new RCIntRectangle(0, RCScrollBar.BUTTON_SIZE, RCScrollBar.WIDTH, RCScrollBar.BUTTON_SIZE));


            /// Render the slider.
            renderContext.RenderSprite(
                this.controlSprite,
                this.SliderRectangle.Location,
                this.alignment == Alignment.Horizontal ?
                new RCIntRectangle(2 * RCScrollBar.BUTTON_SIZE, 0, RCScrollBar.WIDTH, RCScrollBar.WIDTH) :
                new RCIntRectangle(0, 2 * RCScrollBar.BUTTON_SIZE, RCScrollBar.WIDTH, RCScrollBar.WIDTH));
        }

        /// <summary>
        /// The image used for rendering the scrollbar.
        /// </summary>
        private UISprite controlSprite;

        /// <summary>
        /// The alignment of the scrollbar.
        /// </summary>
        private Alignment alignment;

        /// <summary>
        /// The length of the track of the slider.
        /// </summary>
        private int sliderTrackLength;

        /// <summary>
        /// Constants.
        /// </summary>
        private const int WIDTH = 9;
        private const int BUTTON_SIZE = 9;
        private const int SPRITE_SLIDERTRACK_LENGTH = 200;
        private const int STEP_VALUE_CHANGE_DEFAULT = 1;
        private const int TRACKING_VALUE_CHANGE_DEFAULT = 5;
        private const int TIME_BETWEEN_STEPS_DEFAULT = 100;
        private const int TIME_BETWEEN_TRACKINGS_DEFAULT = 300;
    }
}
