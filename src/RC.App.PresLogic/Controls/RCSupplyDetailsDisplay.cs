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
    /// Displays detailed informations about the supplies provided by the selected map object on the details panel.
    /// </summary>
    public class RCSupplyDetailsDisplay : RCTextInformationDisplay
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

            this.textList = new List<UIString>
            {
                this.suppliesUsedText,
                this.suppliesProvidedText,
                this.totalSuppliesText,
                this.suppliesMaxText
            };
        }

        /// <see cref="RCTextInformationDisplay.GetDisplayedTexts"/>
        protected override List<UIString> GetDisplayedTexts()
        {
            /// Check if there is 1 object is selected.
            if (this.selectionDetailsView.SelectionCount != 1) { return new List<UIString>(); }
            int mapObjectID = this.selectionDetailsView.GetObjectID(0);

            /// Check if the selected object has supply informations that can be displayed.
            int suppliesProvided = this.mapObjectDetailsView.GetSuppliesProvided(mapObjectID);
            if (suppliesProvided == -1) { return new List<UIString>(); }

            /// Refresh the texts with the actual supply informations.
            this.suppliesUsedText[0] = this.playerView.UsedSupply;
            this.suppliesProvidedText[0] = suppliesProvided;
            this.totalSuppliesText[0] = this.playerView.TotalSupply;
            this.suppliesMaxText[0] = this.playerView.MaxSupply;

            return this.textList;
        }

        /// <summary>
        /// The texts to be displayed.
        /// </summary>
        private readonly UIString suppliesUsedText;
        private readonly UIString suppliesProvidedText;
        private readonly UIString totalSuppliesText;
        private readonly UIString suppliesMaxText;

        /// <summary>
        /// The texts to be displayed in a list.
        /// </summary>
        private readonly List<UIString> textList;

        /// <summary>
        /// Reference to the necessary views.
        /// </summary>
        private readonly IPlayerView playerView;
        private readonly IMapObjectDetailsView mapObjectDetailsView;
        private readonly ISelectionDetailsView selectionDetailsView;
    }
}
