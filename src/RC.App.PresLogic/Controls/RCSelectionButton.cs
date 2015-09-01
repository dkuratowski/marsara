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
    /// Represents a selection button on the details panel of the gameplay page.
    /// </summary>
    public class RCSelectionButton : UIButton, IDisposable
    {
        /// <summary>
        /// Constructs a selection button at the given rectangular area inside the details panel.
        /// </summary>
        /// <param name="layoutIndex">The index in the layout of this button on the details panel.</param>
        /// <param name="hpIndicatorSprites">
        /// List of the HP indicator icon sprite groups for each possible conditions.
        /// </param>
        public RCSelectionButton(int layoutIndex, Dictionary<MapObjectConditionEnum, SpriteGroup> hpIndicatorSprites)
            : base(BUTTON_POSITIONS[layoutIndex].Location, BUTTON_POSITIONS[layoutIndex].Size)
        {
            if (layoutIndex < 0) { throw new ArgumentOutOfRangeException("layoutIndex", "Selection button layout index must be non-negative!"); }
            if (hpIndicatorSprites == null) { throw new ArgumentNullException("hpIndicatorSprites"); }

            this.layoutIndex = layoutIndex;
            this.hpIndicatorSprites = hpIndicatorSprites;
            IViewService viewService = ComponentManager.GetInterface<IViewService>();
            this.detailsPanelView = viewService.CreateView<IDetailsView>();
            this.Pressed += this.OnButtonPressed;
        }

        #region IDisposable members

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            this.Pressed -= this.OnButtonPressed;
        }

        #endregion IDisposable members

        /// <see cref="UIObject.Render_i"/>
        protected override void Render_i(IUIRenderContext renderContext)
        {
            if (this.layoutIndex >= this.detailsPanelView.SelectionCount) { return; }

            MapObjectConditionEnum hpCondition = this.detailsPanelView.GetHPCondition(this.layoutIndex);
            if (!this.hpIndicatorSprites.ContainsKey(hpCondition)) { return; }

            SpriteInst hpSprite = this.detailsPanelView.GetHPIcon(this.layoutIndex);
            renderContext.RenderSprite(this.hpIndicatorSprites[hpCondition][hpSprite.Index],
                                       hpSprite.DisplayCoords,
                                       hpSprite.Section);
        }

        /// <summary>
        /// This method is called when this RCSelectionButton has been pressed.
        /// </summary>
        private void OnButtonPressed(UISensitiveObject sender)
        {
            if (sender != this) { throw new InvalidOperationException("Unexpected sender!"); }

            // TODO: implement this event handler!
        }

        /// <summary>
        /// Reference to the details panel view.
        /// </summary>
        private readonly IDetailsView detailsPanelView;

        /// <summary>
        /// The index in the layout of this button on the details panel.
        /// </summary>
        private readonly int layoutIndex;

        /// <summary>
        /// List of the HP indicator icon sprite groups for each possible conditions.
        /// </summary>
        private readonly Dictionary<MapObjectConditionEnum, SpriteGroup> hpIndicatorSprites;

        /// <summary>
        /// The position of the selection buttons inside the details panel based on their layout order.
        /// </summary>
        private static readonly RCIntRectangle[] BUTTON_POSITIONS = new RCIntRectangle[]
        {
            new RCIntRectangle(31, 7, 16, 16), new RCIntRectangle(31, 27, 16, 16),
            new RCIntRectangle(51, 7, 16, 16), new RCIntRectangle(51, 27, 16, 16),
            new RCIntRectangle(71, 7, 16, 16), new RCIntRectangle(71, 27, 16, 16),
            new RCIntRectangle(91, 7, 16, 16), new RCIntRectangle(91, 27, 16, 16),
            new RCIntRectangle(111, 7, 16, 16), new RCIntRectangle(111, 27, 16, 16),
            new RCIntRectangle(131, 7, 16, 16), new RCIntRectangle(131, 27, 16, 16),
        };
    }
}
