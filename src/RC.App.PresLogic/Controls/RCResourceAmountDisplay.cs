using RC.App.BizLogic.PublicInterfaces;
using RC.Common;
using RC.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.PresLogic.Controls
{
    /// <summary>
    /// Adds new functionality to the map display control for displaying the amount of resource in a mineral field or vespene geyser.
    /// </summary>
    public class RCResourceAmountDisplay : RCMapDisplayExtension
    {
        /// <summary>
        /// Constructs an RCResourceAmountDisplay extension for the given map display control.
        /// </summary>
        /// <param name="extendedControl">The map display control to extend.</param>
        /// <param name="dataView">Reference to a data view.</param>
        /// <param name="map">Reference to a map view.</param>
        public RCResourceAmountDisplay(RCMapDisplay extendedControl, IMapObjectDataView dataView, IMapView map)
            : base(extendedControl, map)
        {
            if (dataView == null) { throw new ArgumentNullException("dataView"); }

            this.mapObjectDataView = dataView;
            this.drawPosition = RCIntVector.Undefined;
            this.stringToRender = new UIString("R:{0}", UIResourceManager.GetResource<UIFont>("RC.App.Fonts.Font5"), UIWorkspace.Instance.PixelScaling, UIColor.White);
            this.backgroundBrush = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(UIColor.Black, new RCIntVector(1, this.stringToRender.Font.MinimumLineHeight), UIWorkspace.Instance.PixelScaling);
            this.backgroundBrush.Upload();
        }

        /// <see cref="RCMapDisplayExtension.StartExtension_i"/>
        protected override void StartExtension_i()
        {
            this.MouseSensor.Move += this.OnMouseMove;
        }

        /// <see cref="RCMapDisplayExtension.StopExtension_i"/>
        protected override void StopExtension_i()
        {
            this.MouseSensor.Move -= this.OnMouseMove;
        }

        /// <see cref="RCMapDisplayExtension.RenderExtension_i"/>
        protected override void RenderExtension_i(IUIRenderContext renderContext)
        {
            if (this.DisplayedArea != RCIntRectangle.Undefined && this.drawPosition != RCIntVector.Undefined && this.mapObjectDataView.ObjectID != -1)
            {
                if (this.mapObjectDataView.MineralsAmount != -1)
                {
                    this.stringToRender[0] = this.mapObjectDataView.MineralsAmount;
                    renderContext.RenderRectangle(this.backgroundBrush, new RCIntRectangle(this.drawPosition.X, this.drawPosition.Y, this.stringToRender.Width, this.backgroundBrush.Size.Y));
                    renderContext.RenderString(this.stringToRender, this.drawPosition + new RCIntVector(0, this.stringToRender.Font.CharTopMaximum));
                }
                else if (this.mapObjectDataView.VespeneGasAmount != -1)
                {
                    this.stringToRender[0] = this.mapObjectDataView.VespeneGasAmount;
                    renderContext.RenderRectangle(this.backgroundBrush, new RCIntRectangle(this.drawPosition.X, this.drawPosition.Y, this.stringToRender.Width, this.backgroundBrush.Size.Y));
                    renderContext.RenderString(this.stringToRender, this.drawPosition + new RCIntVector(0, this.stringToRender.Font.CharTopMaximum));
                }
            }
        }

        /// <summary>
        /// Called when the mouse pointer has been moved over the display.
        /// </summary>
        private void OnMouseMove(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            if (this.drawPosition != evtArgs.Position)
            {
                this.drawPosition = evtArgs.Position - new RCIntVector(0, this.stringToRender.Font.MinimumLineHeight);
            }
        }

        /// <summary>
        /// Reference to the map object data view or null if there is no map object whose resource data is being displayed.
        /// </summary>
        private IMapObjectDataView mapObjectDataView;

        /// <summary>
        /// The position to draw in the coordinate system of the display.
        /// </summary>
        private RCIntVector drawPosition;

        /// <summary>
        /// The rendered string.
        /// </summary>
        private UIString stringToRender;

        /// <summary>
        /// The brush that is used to draw the background.
        /// </summary>
        private UISprite backgroundBrush;
    }
}
