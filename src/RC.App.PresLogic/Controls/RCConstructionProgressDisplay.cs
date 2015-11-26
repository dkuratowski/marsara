using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.Views;
using RC.UI;
using RC.Common;
using RC.App.BizLogic.Services;
using RC.Common.ComponentModel;

namespace RC.App.PresLogic.Controls
{
    /// <summary>
    /// Represents the construction progress display control on the details panel.
    /// </summary>
    public class RCConstructionProgressDisplay : UIControl
    {
        /// <summary>
        /// Constructs a construction progress display control at the given position with the given size.
        /// </summary>
        /// <param name="position">The position of the construction progress display control.</param>
        /// <param name="size">The size of the construction progress display control.</param>
        public RCConstructionProgressDisplay(RCIntVector position, RCIntVector size)
            : base(position, size)
        {
            IViewService viewService = ComponentManager.GetInterface<IViewService>();
            this.productionDetailsView = viewService.CreateView<IProductionDetailsView>();

            this.progressBarSprite = UIResourceManager.GetResource<UISprite>("RC.App.Sprites.ProductionProgressBar");
            this.progressBarBrush = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(RCColor.Green, new RCIntVector(1, 1), UIWorkspace.Instance.PixelScaling);
            this.progressBarBrush.Upload();

            UIFont textFont = UIResourceManager.GetResource<UIFont>("RC.App.Fonts.Font5");
            this.underConstructionText = new UIString("Under Construction", textFont, UIWorkspace.Instance.PixelScaling, RCColor.White);
            this.textPosition = new RCIntVector(
                (PROGRESSBAR_INNER_RECT.Left + PROGRESSBAR_INNER_RECT.Right - this.underConstructionText.Width) / 2,
                PROGRESSBAR_SPRITE_POS.Y - this.underConstructionText.Font.CharBottomMaximum - 1);
        }

        /// <see cref="UIObject.Render_i"/>
        protected override void Render_i(IUIRenderContext renderContext)
        {
            /// Render the "Under Construction" text.
            renderContext.RenderString(this.underConstructionText, this.textPosition);

            /// Render the progress bar sprite.
            renderContext.RenderSprite(this.progressBarSprite, PROGRESSBAR_SPRITE_POS);

            /// Render the progress of the construction.
            RCNumber progressNorm = this.productionDetailsView.ConstructionProgressNormalized;
            if (progressNorm != -1)
            {
                int lineWidth = (int)(PROGRESSBAR_INNER_RECT.Width * progressNorm);
                if (lineWidth > 0)
                {
                    renderContext.RenderRectangle(this.progressBarBrush,
                        new RCIntRectangle(PROGRESSBAR_INNER_RECT.Left,
                                           PROGRESSBAR_INNER_RECT.Top,
                                           lineWidth,
                                           PROGRESSBAR_INNER_RECT.Height));
                }
            }
        }

        /// <summary>
        /// Reference to the production details view.
        /// </summary>
        private readonly IProductionDetailsView productionDetailsView;

        /// <summary>
        /// The sprite for rendering the progress bar.
        /// </summary>
        private readonly UISprite progressBarSprite;

        /// <summary>
        /// The brush that is used for rendering the progress bar.
        /// </summary>
        private readonly UISprite progressBarBrush;

        /// <summary>
        /// The "Under Construction" text that is displayed over the progress bar.
        /// </summary>
        private readonly UIString underConstructionText;

        /// <summary>
        /// The position of the "Under Construction" text.
        /// </summary>
        private readonly RCIntVector textPosition;

        /// <summary>
        /// Position informations of the progress bar.
        /// </summary>
        private static readonly RCIntVector PROGRESSBAR_SPRITE_POS = new RCIntVector(20, 14);
        private static readonly RCIntRectangle PROGRESSBAR_INNER_RECT = new RCIntRectangle(21, 15, 56, 2);
    }
}
