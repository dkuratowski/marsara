using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.PublicInterfaces;
using RC.Common;
using RC.UI;

namespace RC.App.PresLogic.Controls
{
    /// <summary>
    /// Adds the following new functionalities to the map display control:
    ///     - indicates the selected map objects
    ///     - displays the selection box if a selection is currently in progress
    /// </summary>
    public class RCSelectionDisplay : RCMapDisplayExtension, IMapControl
    {
        /// <summary>
        /// Constructs an RCSelectionDisplay extension for the given map display control.
        /// </summary>
        /// <param name="extendedControl">The map display control to extend.</param>
        /// <param name="selIndicatorView">Reference to a selection indicator view.</param>
        public RCSelectionDisplay(RCMapDisplay extendedControl, ISelectionIndicatorView selIndicatorView)
            : base(extendedControl, selIndicatorView)
        {
            if (selIndicatorView == null) { throw new ArgumentNullException("selIndicatorView"); }
            this.selectionIndicatorView = selIndicatorView;
            this.selectionBox = RCIntRectangle.Undefined;

            this.lightGreenBrush = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(UIColor.LightGreen, new RCIntVector(1, 1), UIWorkspace.Instance.PixelScaling);
            this.greenBrush = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(UIColor.Green, new RCIntVector(1, 1), UIWorkspace.Instance.PixelScaling);
            this.yellowBrush = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(UIColor.Yellow, new RCIntVector(1, 1), UIWorkspace.Instance.PixelScaling);
            this.redBrush = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(UIColor.Red, new RCIntVector(1, 1), UIWorkspace.Instance.PixelScaling);
            this.lightGreenBrush.Upload();
            this.greenBrush.Upload();
            this.yellowBrush.Upload();
            this.redBrush.Upload();
        }

        /// <see cref="IMapControl.SelectionBox"/>
        public RCIntRectangle SelectionBox
        {
            get { return this.selectionBox; }
            set { this.selectionBox = value; }
        }
        
        #region Overrides

        /// <see cref="RCMapDisplayExtension.RenderExtension_i"/>
        protected override void RenderExtension_i(IUIRenderContext renderContext)
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

            /// Render the selection box if necessary.
            if (this.selectionBox != RCIntRectangle.Undefined)
            {
                renderContext.RenderRectangle(this.lightGreenBrush, this.selectionBox);
            }
        }

        #endregion Overrides

        /// <summary>
        /// The current selection box or RCIntRectangle.Undefined if selection box is currently turned off.
        /// </summary>
        private RCIntRectangle selectionBox;

        /// <summary>
        /// Reference to the selection indicator view.
        /// </summary>
        private ISelectionIndicatorView selectionIndicatorView;

        /// <summary>
        /// Brushes  for rendering.
        /// </summary>
        private UISprite lightGreenBrush;
        private UISprite greenBrush;
        private UISprite yellowBrush;
        private UISprite redBrush;
    }
}
