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
            this.mapObjectDetailsView = null;

            this.greenBrush = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(RCColor.Green, new RCIntVector(1, 1), UIWorkspace.Instance.PixelScaling);
            this.yellowBrush = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(RCColor.Yellow, new RCIntVector(1, 1), UIWorkspace.Instance.PixelScaling);
            this.redBrush = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(RCColor.Red, new RCIntVector(1, 1), UIWorkspace.Instance.PixelScaling);
            this.lightGreenBrush = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(RCColor.LightGreen, new RCIntVector(1, 1), UIWorkspace.Instance.PixelScaling);
            this.lightBlueBrush = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(RCColor.LightBlue, new RCIntVector(1, 1), UIWorkspace.Instance.PixelScaling);
            this.lightMagentaBrush = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(RCColor.LightMagenta, new RCIntVector(1, 1), UIWorkspace.Instance.PixelScaling);
            this.greenBrush.Upload();
            this.yellowBrush.Upload();
            this.redBrush.Upload();
            this.lightGreenBrush.Upload();
            this.lightBlueBrush.Upload();
            this.lightMagentaBrush.Upload();

            this.hpConditionBrushes = new Dictionary<MapObjectConditionEnum, UISprite>
            {
                {MapObjectConditionEnum.Excellent, this.lightGreenBrush},
                {MapObjectConditionEnum.Moderate, this.yellowBrush},
                {MapObjectConditionEnum.Critical, this.redBrush},
            };
        }
        
        #region Overrides

        /// <see cref="RCMapDisplayExtension.ConnectEx_i"/>
        protected override void ConnectEx_i()
        {
            IViewService viewService = ComponentManager.GetInterface<IViewService>();
            this.selectionIndicatorView = viewService.CreateView<ISelectionIndicatorView>();
            this.mapObjectDetailsView = viewService.CreateView<IMapObjectDetailsView>();
        }

        /// <see cref="RCMapDisplayExtension.DisconnectEx_i"/>
        protected override void DisconnectEx_i()
        {
            this.selectionIndicatorView = null;
            this.mapObjectDetailsView = null;
        }

        /// <see cref="RCMapDisplayExtension.RenderEx_i"/>
        protected override void RenderEx_i(IUIRenderContext renderContext)
        {
            /// Render the selection indicators of the selected map objects.
            List<SelIndicatorInst> selectionIndicators = this.selectionIndicatorView.GetVisibleSelIndicators();
            foreach (SelIndicatorInst selIndicator in selectionIndicators)
            {
                /// Render the indicator rectangle.
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

                /// Render the shield if exists.
                int indicatorIndex = 0;
                if (selIndicator.ShieldNorm != -1)
                {
                    int lineWidth = (int)(selIndicator.IndicatorRect.Width * selIndicator.ShieldNorm);
                    if (lineWidth > 0)
                    {
                        renderContext.RenderRectangle(this.lightBlueBrush,
                            new RCIntRectangle(selIndicator.IndicatorRect.Left,
                                               selIndicator.IndicatorRect.Bottom + indicatorIndex,
                                               (int)(selIndicator.IndicatorRect.Width * selIndicator.ShieldNorm),
                                               1));
                    }
                    indicatorIndex++;
                }

                /// Render the HP if exists.
                if (selIndicator.HpNorm != -1)
                {
                    int lineWidth = (int)(selIndicator.IndicatorRect.Width * selIndicator.HpNorm);
                    if (lineWidth > 0)
                    {
                        MapObjectConditionEnum hpCondition = this.mapObjectDetailsView.GetHPCondition(selIndicator.ObjectID);
                        renderContext.RenderRectangle(this.hpConditionBrushes[hpCondition],
                            new RCIntRectangle(selIndicator.IndicatorRect.Left,
                                               selIndicator.IndicatorRect.Bottom + indicatorIndex,
                                               (int)(selIndicator.IndicatorRect.Width * selIndicator.HpNorm),
                                               1));
                    }
                    indicatorIndex++;
                }

                /// Render the energy if exists in case of friendly objects.
                if (selIndicator.EnergyNorm != -1 && selIndicator.SelIndicatorType == SelIndicatorTypeEnum.Friendly)
                {
                    int lineWidth = (int)(selIndicator.IndicatorRect.Width * selIndicator.EnergyNorm);
                    if (lineWidth > 0)
                    {
                        renderContext.RenderRectangle(this.lightMagentaBrush,
                            new RCIntRectangle(selIndicator.IndicatorRect.Left,
                                               selIndicator.IndicatorRect.Bottom + indicatorIndex,
                                               (int)(selIndicator.IndicatorRect.Width * selIndicator.EnergyNorm),
                                               1));
                    }
                }
            }
        }

        #endregion Overrides

        /// <summary>
        /// Reference to the selection indicator view.
        /// </summary>
        private ISelectionIndicatorView selectionIndicatorView;

        /// <summary>
        /// Reference to the map object details view.
        /// </summary>
        private IMapObjectDetailsView mapObjectDetailsView;

        /// <summary>
        /// Resources for rendering.
        /// </summary>
        private readonly UISprite greenBrush;
        private readonly UISprite yellowBrush;
        private readonly UISprite redBrush;
        private readonly UISprite lightGreenBrush;
        private readonly UISprite lightBlueBrush;
        private readonly UISprite lightMagentaBrush;

        /// <summary>
        /// The brushes for rendering HP mapped by the corresponding conditions.
        /// </summary>
        private readonly Dictionary<MapObjectConditionEnum, UISprite> hpConditionBrushes;
    }
}
