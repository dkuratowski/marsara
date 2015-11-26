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
    /// Represents the resource amount display control on the details panel.
    /// </summary>
    public class RCResourceAmountDisplay : UIControl
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
            this.mineralsText = new UIString("Minerals: {0}", textFont, UIWorkspace.Instance.PixelScaling, RCColor.White);
            this.vespeneGasText = new UIString("Vespene Gas: {0}", textFont, UIWorkspace.Instance.PixelScaling, RCColor.White);
            this.depletedText = new UIString("Depleted", textFont, UIWorkspace.Instance.PixelScaling, RCColor.White);

            this.textY = size.Y / 2 - textFont.MinimumLineHeight / 2 + textFont.CharTopMaximum;
            this.middleX = size.X / 2;
        }

        /// <see cref="UIObject.Render_i"/>
        protected override void Render_i(IUIRenderContext renderContext)
        {
            if (this.selectionDetailsView.SelectionCount != 1) { return; }
            int mapObjectID = this.selectionDetailsView.GetObjectID(0);

            int minerals = this.mapObjectDetailsView.GetMineralsAmount(mapObjectID);
            int vespeneGas = this.mapObjectDetailsView.GetVespeneGasAmount(mapObjectID);
            if (minerals != -1)
            {
                this.mineralsText[0] = minerals;
                renderContext.RenderString(this.mineralsText, new RCIntVector(this.middleX - this.mineralsText.Width / 2, this.textY));
            }
            else if (vespeneGas != -1)
            {
                this.vespeneGasText[0] = vespeneGas;
                UIString stringToRender = vespeneGas > 0 ? this.vespeneGasText : this.depletedText;
                renderContext.RenderString(stringToRender, new RCIntVector(this.middleX - stringToRender.Width / 2, this.textY));
            }
        }

        /// <summary>
        /// The texts to be displayed.
        /// </summary>
        private readonly UIString mineralsText;
        private readonly UIString vespeneGasText;
        private readonly UIString depletedText;

        /// <summary>
        /// The Y coordinate of the text to be displayed.
        /// </summary>
        private readonly int textY;

        /// <summary>
        /// The X coordinate of the middle of this control.
        /// </summary>
        private readonly int middleX;

        /// <summary>
        /// Reference to the necessary views.
        /// </summary>
        private readonly IMapObjectDetailsView mapObjectDetailsView;
        private readonly ISelectionDetailsView selectionDetailsView;
    }
}
