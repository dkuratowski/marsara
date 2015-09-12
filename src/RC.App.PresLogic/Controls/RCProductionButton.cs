using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.Services;
using RC.App.BizLogic.Views;
using RC.App.PresLogic.SpriteGroups;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Common.Diagnostics;
using RC.UI;

namespace RC.App.PresLogic.Controls
{
    /// <summary>
    /// Represents a production button on the production display of the details panel.
    /// </summary>
    public class RCProductionButton : UIButton, IDisposable
    {
        /// <summary>
        /// Constructs a production button at the given layout index inside the production display.
        /// </summary>
        /// <param name="productIconSprites">The product icon sprite group.</param>
        /// <param name="layoutIndex">The index in the layout of this button on the production display.</param>
        public RCProductionButton(ISpriteGroup productIconSprites, int layoutIndex)
            : base(BUTTON_POSITIONS[layoutIndex].Location, BUTTON_POSITIONS[layoutIndex].Size)
        {
            if (layoutIndex < 0) { throw new ArgumentOutOfRangeException("layoutIndex", "Production button layout index must be non-negative!"); }
            if (productIconSprites == null) { throw new ArgumentNullException("productIconSprites"); }

            IViewService viewService = ComponentManager.GetInterface<IViewService>();
            this.productionLineView = viewService.CreateView<IProductionLineView>();

            this.productionButtonSprite = UIResourceManager.GetResource<UISprite>("RC.App.Sprites.ProductionButton");
            this.layoutIndex = layoutIndex;
            this.productIconSprites = productIconSprites;

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
            /// Check if this button has to be even rendered.
            if (this.layoutIndex >= this.productionLineView.Capacity) { return; }

            /// If there is a production item belonging to this button, then we have to render its icon.
            if (this.layoutIndex < this.productionLineView.ItemCount)
            {
                SpriteInst itemIcon = this.productionLineView[this.layoutIndex];
                renderContext.RenderSprite(this.productIconSprites[itemIcon.Index],
                                           itemIcon.DisplayCoords,
                                           itemIcon.Section);
            }

            /// Render the button sprite.
            renderContext.RenderSprite(this.productionButtonSprite, new RCIntVector(0, 0));
        }

        /// <summary>
        /// This method is called when this RCProductionButton has been pressed.
        /// </summary>
        private void OnButtonPressed(UISensitiveObject sender)
        {
            if (sender != this) { throw new InvalidOperationException("Unexpected sender!"); }

            /// TODO: implement this method!
            TraceManager.WriteAllTrace(string.Format("ProdButton {0}", this.layoutIndex), PresLogicTraceFilters.INFO);
        }

        /// <summary>
        /// Reference to the production line view.
        /// </summary>
        private readonly IProductionLineView productionLineView;

        /// <summary>
        /// The index in the layout of this button on the production display.
        /// </summary>
        private readonly int layoutIndex;

        /// <summary>
        /// The sprite for rendering the production button.
        /// </summary>
        private readonly UISprite productionButtonSprite;

        /// <summary>
        /// The product icon sprite group.
        /// </summary>
        private readonly ISpriteGroup productIconSprites;

        /// <summary>
        /// The position of the production buttons inside the production display based on their layout order.
        /// </summary>
        private static readonly RCIntRectangle[] BUTTON_POSITIONS = new RCIntRectangle[]
        {
            new RCIntRectangle(10, 0, 20, 20),
            new RCIntRectangle(10, 20, 20, 20),
            new RCIntRectangle(30, 20, 20, 20),
            new RCIntRectangle(50, 20, 20, 20),
            new RCIntRectangle(70, 20, 20, 20)
        };
    }
}
