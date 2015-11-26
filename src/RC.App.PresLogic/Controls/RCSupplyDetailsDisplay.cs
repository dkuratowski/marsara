using RC.App.BizLogic.Services;
using RC.App.BizLogic.Views;
using RC.Common;
using RC.Common.ComponentModel;
using RC.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.PresLogic.Controls
{
    /// <summary>
    /// Represents the supply details display control on the details panel.
    /// </summary>
    public class RCSupplyDetailsDisplay : UIControl
    {
        /// <summary>
        /// Constructs a RCSupplyDetailsDisplay control at the given position with the given size.
        /// </summary>
        /// <param name="position">The position of the control.</param>
        /// <param name="size">The size of the control.</param>
        public RCSupplyDetailsDisplay(RCIntVector position, RCIntVector size)
            : base(position, size)
        {
            IViewService viewService = ComponentManager.GetInterface<IViewService>();
            this.playerView = viewService.CreateView<IPlayerView>();
            this.mapObjectDetailsView = viewService.CreateView<IMapObjectDetailsView>();
            this.selectionDetailsView = viewService.CreateView<ISelectionDetailsView>();

            UIFont textFont = UIResourceManager.GetResource<UIFont>("RC.App.Fonts.Font5");
            this.suppliesUsedText = new UIString("Supplies Used: {0}", textFont, UIWorkspace.Instance.PixelScaling, RCColor.White);
            this.suppliesProvidedText = new UIString("Supplies Provided: {0}", textFont, UIWorkspace.Instance.PixelScaling, RCColor.White);
            this.totalSuppliesText = new UIString("Total Supplies: {0}", textFont, UIWorkspace.Instance.PixelScaling, RCColor.White);
            this.suppliesMaxText = new UIString("Supplies Max: {0}", textFont, UIWorkspace.Instance.PixelScaling, RCColor.White);

            int totalHeightOfTexts = textFont.MinimumLineHeight * 4 + 3;
            this.suppliesUsedTextY = size.Y / 2 - totalHeightOfTexts / 2 + textFont.CharTopMaximum;
            this.suppliesProvidedTextY = this.suppliesUsedTextY + textFont.MinimumLineHeight + 1;
            this.totalSuppliesTextY = this.suppliesProvidedTextY + textFont.MinimumLineHeight + 1;
            this.suppliesMaxTextY = this.totalSuppliesTextY + textFont.MinimumLineHeight + 1;
            this.middleX = size.X / 2;
        }

        /// <see cref="UIObject.Render_i"/>
        protected override void Render_i(IUIRenderContext renderContext)
        {
            if (this.selectionDetailsView.SelectionCount != 1) { return; }
            int mapObjectID = this.selectionDetailsView.GetObjectID(0);

            int suppliesProvided = this.mapObjectDetailsView.GetSuppliesProvided(mapObjectID);
            if (suppliesProvided == -1) { return; }

            this.suppliesUsedText[0] = this.playerView.UsedSupply;
            this.suppliesProvidedText[0] = suppliesProvided;
            this.totalSuppliesText[0] = this.playerView.TotalSupply;
            this.suppliesMaxText[0] = this.playerView.MaxSupply;

            renderContext.RenderString(this.suppliesUsedText, new RCIntVector(this.middleX - this.suppliesUsedText.Width / 2, this.suppliesUsedTextY));
            renderContext.RenderString(this.suppliesProvidedText, new RCIntVector(this.middleX - this.suppliesProvidedText.Width / 2, this.suppliesProvidedTextY));
            renderContext.RenderString(this.totalSuppliesText, new RCIntVector(this.middleX - this.totalSuppliesText.Width / 2, this.totalSuppliesTextY));
            renderContext.RenderString(this.suppliesMaxText, new RCIntVector(this.middleX - this.suppliesMaxText.Width / 2, this.suppliesMaxTextY));
        }

        /// <summary>
        /// The texts to be displayed.
        /// </summary>
        private readonly UIString suppliesUsedText;
        private readonly UIString suppliesProvidedText;
        private readonly UIString totalSuppliesText;
        private readonly UIString suppliesMaxText;

        /// <summary>
        /// The Y coordinates of the texts to be displayed.
        /// </summary>
        private readonly int suppliesUsedTextY;
        private readonly int suppliesProvidedTextY;
        private readonly int totalSuppliesTextY;
        private readonly int suppliesMaxTextY;

        /// <summary>
        /// The X coordinate of the middle of this control.
        /// </summary>
        private readonly int middleX;

        /// <summary>
        /// Reference to the necessary views.
        /// </summary>
        private readonly IPlayerView playerView;
        private readonly IMapObjectDetailsView mapObjectDetailsView;
        private readonly ISelectionDetailsView selectionDetailsView;
    }
}
