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
    /// Displays detailed informations about resource amount of the selected map object on the details panel.
    /// </summary>
    public class RCResourceAmountDisplay : RCTextInformationDisplay
    {
        /// <summary>
        /// Constructs a RCResourceAmountDisplay control at the given position with the given size.
        /// </summary>
        /// <param name="position">The position of the control.</param>
        /// <param name="size">The size of the control.</param>
        public RCResourceAmountDisplay(RCIntVector position, RCIntVector size)
            : base(position, size)
        {
            IViewService viewService = ComponentManager.GetInterface<IViewService>();
            this.mapObjectDetailsView = viewService.CreateView<IMapObjectDetailsView>();
            this.selectionDetailsView = viewService.CreateView<ISelectionDetailsView>();

            UIFont textFont = UIResourceManager.GetResource<UIFont>("RC.App.Fonts.Font5");
            this.mineralsText = new List<UIString> { new UIString("Minerals: {0}", textFont, UIWorkspace.Instance.PixelScaling, RCColor.White) };
            this.vespeneGasText = new List<UIString> { new UIString("Vespene Gas: {0}", textFont, UIWorkspace.Instance.PixelScaling, RCColor.White) };
            this.depletedText = new List<UIString> { new UIString("Depleted", textFont, UIWorkspace.Instance.PixelScaling, RCColor.White) };
        }

        /// <see cref="RCTextInformationDisplay.GetDisplayedTexts"/>
        protected override List<UIString> GetDisplayedTexts()
        {
            /// Check if there is 1 object is selected.
            if (this.selectionDetailsView.SelectionCount != 1) { return new List<UIString>(); }
            int mapObjectID = this.selectionDetailsView.GetObjectID(0);

            /// Check if the selected object has informations about its mineral amount to be displayed.
            int minerals = this.mapObjectDetailsView.GetMineralsAmount(mapObjectID);
            if (minerals != -1)
            {
                this.mineralsText[0][0] = minerals;
                return this.mineralsText;
            }

            /// Check if the selected object has informations about its vespene gas amount to be displayed.
            int vespeneGas = this.mapObjectDetailsView.GetVespeneGasAmount(mapObjectID);
            if (vespeneGas != -1)
            {
                if (vespeneGas > 0)
                {
                    this.vespeneGasText[0][0] = vespeneGas;
                    return this.vespeneGasText;
                }
                else
                {
                    return this.depletedText;
                }
            }

            /// No informations to be displayed.
            return new List<UIString>();
        }

        /// <summary>
        /// The texts to be displayed.
        /// </summary>
        private readonly List<UIString> mineralsText;
        private readonly List<UIString> vespeneGasText;
        private readonly List<UIString> depletedText;

        /// <summary>
        /// Reference to the necessary views.
        /// </summary>
        private readonly IMapObjectDetailsView mapObjectDetailsView;
        private readonly ISelectionDetailsView selectionDetailsView;
    }
}
