using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.Services;
using RC.App.BizLogic.Views;
using RC.Common;
using RC.Common.ComponentModel;
using RC.UI;

namespace RC.App.PresLogic.Controls
{
    /// <summary>
    /// Adds the following new functionalities to the map display control:
    ///     - indicates the selected map objects
    ///     - displays the selection box if a selection is currently in progress
    /// </summary>
    public class RCSelectionDisplay : RCMapDisplayExtension
    {
        /// <summary>
        /// Constructs an RCSelectionDisplay extension for the given map display control.
        /// </summary>
        /// <param name="extendedControl">The map display control to extend.</param>
        public RCSelectionDisplay(RCMapDisplay extendedControl)
            : base(extendedControl)
        {
            this.selectionIndicatorView = null;

            this.greenBrush = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(RCColor.Green, new RCIntVector(1, 1), UIWorkspace.Instance.PixelScaling);
            this.yellowBrush = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(RCColor.Yellow, new RCIntVector(1, 1), UIWorkspace.Instance.PixelScaling);
            this.redBrush = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(RCColor.Red, new RCIntVector(1, 1), UIWorkspace.Instance.PixelScaling);
            this.greenBrush.Upload();
            this.yellowBrush.Upload();
            this.redBrush.Upload();
        }
        
        #region Overrides

        /// <see cref="RCMapDisplayExtension.MapView"/>
        protected override IMapView MapView { get { return this.selectionIndicatorView; } }

        /// <see cref="RCMapDisplayExtension.ConnectEx_i"/>
        protected override void ConnectEx_i()
        {
            IViewService viewService = ComponentManager.GetInterface<IViewService>();
            this.selectionIndicatorView = viewService.CreateView<ISelectionIndicatorView>();
        }

        /// <see cref="RCMapDisplayExtension.DisconnectEx_i"/>
        protected override void DisconnectEx_i()
        {
            this.selectionIndicatorView = null;
        }

        /// <see cref="RCMapDisplayExtension.RenderEx_i"/>
        protected override void RenderEx_i(IUIRenderContext renderContext)
        {
            /// Render the selection indicators of the selected map objects.
            List<SelIndicatorInst> selectionIndicators = this.selectionIndicatorView.GetVisibleSelIndicators(this.DisplayedArea);
            foreach (SelIndicatorInst selIndicator in selectionIndicators)
            {
                if (selIndicator.SelIndicatorType == SelIndicatorTypeEnum.Friendly)
                {
                    renderContext.RenderRectangle(this.greenBrush, selIndicator.IndicatorRect);
                }
                else if (selIndicator.SelIndicatorType == SelIndicatorTypeEnum.Neutral)
                {
                    renderContext.RenderRectangle(this.yellowBrush, selIndicator.IndicatorRect);
                }
                else if (selIndicator.SelIndicatorType == SelIndicatorTypeEnum.Enemy)
                {
                    renderContext.RenderRectangle(this.redBrush, selIndicator.IndicatorRect);
                }
                /// TODO: render the HP, energy & shield values under the selection indicator
            }
        }

        #endregion Overrides

        /// <summary>
        /// Reference to the selection indicator view.
        /// </summary>
        private ISelectionIndicatorView selectionIndicatorView;

        /// <summary>
        /// Resources for rendering.
        /// </summary>
        private UISprite greenBrush;
        private UISprite yellowBrush;
        private UISprite redBrush;
    }
}
