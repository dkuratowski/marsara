using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.Services;
using RC.App.BizLogic.Views;
using RC.App.PresLogic.SpriteGroups;
using RC.Common;
using RC.Common.ComponentModel;
using RC.UI;

namespace RC.App.PresLogic.Controls
{
    /// <summary>
    /// Represents the production line display control on the details panel.
    /// </summary>
    public class RCProductionLineDisplay : UIControl
    {
        /// <summary>
        /// Constructs a production line display control at the given position with the given size.
        /// </summary>
        /// <param name="productIconSprites">The product icon sprite group.</param>
        /// <param name="position">The position of the production line display control.</param>
        /// <param name="size">The size of the production line display control.</param>
        public RCProductionLineDisplay(ISpriteGroup productIconSprites, RCIntVector position, RCIntVector size)
            : base(position, size)
        {
            if (productIconSprites == null) { throw new ArgumentNullException("productIconSprites"); }

            IViewService viewService = ComponentManager.GetInterface<IViewService>();
            this.productionLineView = viewService.CreateView<IProductionLineView>();

            this.progressBarSprite = UIResourceManager.GetResource<UISprite>("RC.App.Sprites.ProductionProgressBar");
            this.progressBarBrush = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(RCColor.Green, new RCIntVector(1, 1), UIWorkspace.Instance.PixelScaling);
            this.progressBarBrush.Upload();

            for (int buttonIndex = 0; buttonIndex < PRODUCTION_BUTTON_COUNT; buttonIndex++)
            {
                RCProductionButton prodButton = new RCProductionButton(productIconSprites, buttonIndex);
                this.Attach(prodButton);
                this.AttachSensitive(prodButton);
            }
        }

        /// <see cref="UIObject.Render_i"/>
        protected override void Render_i(IUIRenderContext renderContext)
        {
            /// Render the progress bar sprite.
            renderContext.RenderSprite(this.progressBarSprite, PROGRESSBAR_SPRITE_POS);

            /// Render the progress of the current production job.
            RCNumber progressNorm = this.productionLineView.ProgressNormalized;
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
        /// Reference to the production line view.
        /// </summary>
        private readonly IProductionLineView productionLineView;

        /// <summary>
        /// The sprite for rendering the progress bar.
        /// </summary>
        private readonly UISprite progressBarSprite;

        /// <summary>
        /// The brush that is used for rendering the progress bar.
        /// </summary>
        private readonly UISprite progressBarBrush;

        /// <summary>
        /// Position informations of the progress bar.
        /// </summary>
        private static readonly RCIntVector PROGRESSBAR_SPRITE_POS = new RCIntVector(32, 14);
        private static readonly RCIntRectangle PROGRESSBAR_INNER_RECT = new RCIntRectangle(33, 15, 56, 2);

        /// <summary>
        /// The number of production buttons.
        /// </summary>
        private const int PRODUCTION_BUTTON_COUNT = 5;
    }
}
