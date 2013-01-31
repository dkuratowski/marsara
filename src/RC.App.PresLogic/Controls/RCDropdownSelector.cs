using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.UI;
using RC.Common;

namespace RC.App.PresLogic
{
    /// <summary>
    /// Represents a dropdown selector control in the RC application.
    /// </summary>
    public class RCDropdownSelector : UIDropdownSelector
    {
        /// <summary>
        /// Constructs a dropdown selector control.
        /// </summary>
        /// <param name="position">The position of the control.</param>
        /// <param name="width">The width of the control in pixels.</param>
        /// <param name="options">The list of the options available in the control.</param>
        public RCDropdownSelector(RCIntVector position, int width, string[] options)
            : base(position, new RCIntVector(width, HEIGHT), options.Length)
        {
            if (width <= TEXT_PADDING + ARROW_WIDTH) { throw new ArgumentOutOfRangeException("width", string.Format("Width must be at least {0}!", TEXT_PADDING + ARROW_WIDTH + 1)); }

            this.options = new UIString[options.Length];
            this.highlightedOptions = new UIString[options.Length];
            this.disabledOptions = new UIString[options.Length];
            this.textPartWidth = width - TEXT_PADDING - ARROW_WIDTH;

            for (int i = 0; i < options.Length; i++)
            {
                this.options[i] = new UIString(options[i], UIResourceManager.GetResource<UIFont>("RC.App.Fonts.Font5"), UIWorkspace.Instance.PixelScaling, UIColor.Green);
                this.highlightedOptions[i] = new UIString(options[i], UIResourceManager.GetResource<UIFont>("RC.App.Fonts.Font5"), UIWorkspace.Instance.PixelScaling, UIColor.LightGreen);
                this.disabledOptions[i] = new UIString(options[i], UIResourceManager.GetResource<UIFont>("RC.App.Fonts.Font5"), UIWorkspace.Instance.PixelScaling, UIColor.White);
            }

            this.controlSprite = UIResourceManager.GetResource<UISprite>("RC.App.Sprites.DropdownSelector");
        }

        /// <see cref="UIObject.Render_i"/>
        protected override void Render_i(IUIRenderContext renderContext)
        {
            /// Render the main part of the control.
            renderContext.RenderSprite(this.controlSprite, new RCIntVector(0, 0), new RCIntRectangle(0, 0, TEXT_PADDING, HEIGHT));
            for (int drawnWidth = 0; drawnWidth < this.textPartWidth; )
            {
                int toBeDrawnNow = Math.Min(this.textPartWidth - drawnWidth, SPRITE_BACKGROUND_WIDTH);

                renderContext.RenderSprite(
                    this.controlSprite,
                    new RCIntVector(TEXT_PADDING + drawnWidth, 0),
                    new RCIntRectangle(TEXT_PADDING, 0, toBeDrawnNow, HEIGHT));

                drawnWidth += toBeDrawnNow;
            }
            renderContext.RenderSprite(this.controlSprite, new RCIntVector(TEXT_PADDING + textPartWidth, 0), new RCIntRectangle(TEXT_PADDING + SPRITE_BACKGROUND_WIDTH, 0, ARROW_WIDTH, HEIGHT));

            if (this.IsEnabled)
            {
                renderContext.RenderString(this.CurrentStatus == Status.Normal ? this.options[this.SelectedIndex] : this.highlightedOptions[this.SelectedIndex],
                                           new RCIntVector(TEXT_PADDING, HEIGHT - BASELINE - 1),
                                           this.textPartWidth);

                /// Render the dropdown list part of the control.
                if (this.CurrentStatus == Status.DroppedDown || this.CurrentStatus == Status.Selecting)
                {
                    for (int i = 0; i < this.options.Length; i++)
                    {
                        renderContext.RenderSprite(this.controlSprite, new RCIntVector(0, (i + 1) * HEIGHT), new RCIntRectangle(0, i < this.options.Length - 1 ? HEIGHT : 2 * HEIGHT, TEXT_PADDING, HEIGHT));
                        for (int drawnWidth = 0; drawnWidth < this.textPartWidth; )
                        {
                            int toBeDrawnNow = Math.Min(this.textPartWidth - drawnWidth, SPRITE_BACKGROUND_WIDTH);

                            renderContext.RenderSprite(
                                this.controlSprite,
                                new RCIntVector(TEXT_PADDING + drawnWidth, (i + 1) * HEIGHT),
                                new RCIntRectangle(TEXT_PADDING, i < this.options.Length - 1 ? HEIGHT : 2 * HEIGHT, toBeDrawnNow, HEIGHT));

                            drawnWidth += toBeDrawnNow;
                        }
                        renderContext.RenderSprite(this.controlSprite, new RCIntVector(TEXT_PADDING + textPartWidth, (i + 1) * HEIGHT), new RCIntRectangle(TEXT_PADDING + SPRITE_BACKGROUND_WIDTH, i < this.options.Length - 1 ? HEIGHT : 2 * HEIGHT, ARROW_WIDTH, HEIGHT));
                        renderContext.RenderString(i == this.HighlightedIndex ? this.highlightedOptions[i] : this.options[i],
                                                   new RCIntVector(TEXT_PADDING, (i + 2) * HEIGHT - BASELINE - 1),
                                                   this.textPartWidth);
                    }
                }
            }
            else
            {
                renderContext.RenderString(this.disabledOptions[this.SelectedIndex],
                                           new RCIntVector(TEXT_PADDING, HEIGHT - BASELINE - 1),
                                           this.textPartWidth);
            }
        }

        /// <summary>
        /// List of the option strings.
        /// </summary>
        private UIString[] options;

        /// <summary>
        /// List of the highlighted option strings.
        /// </summary>
        private UIString[] highlightedOptions;

        /// <summary>
        /// List of the disabled option strings.
        /// </summary>
        private UIString[] disabledOptions;

        /// <summary>
        /// The sprite of the control.
        /// </summary>
        private UISprite controlSprite;

        /// <summary>
        /// The width of the text part of the control.
        /// </summary>
        private int textPartWidth;

        /// <summary>
        /// Constant definitions.
        /// </summary>
        private static int HEIGHT = 13;
        private static int TEXT_PADDING = 3;
        private static int BASELINE = 4;
        private static int ARROW_WIDTH = 16;
        private static int SPRITE_BACKGROUND_WIDTH = 203;
    }
}
