using RC.Common;
using RC.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.ComponentModel;
using RC.App.BizLogic.Services;
using RC.App.BizLogic.Views;

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
        public RCResourceAmountDisplay(RCMapDisplay extendedControl)
            : base(extendedControl)
        {
            this.mapObjectDataView = null;
            this.drawPosition = RCIntVector.Undefined;
            this.stringToRender = new UIString("R:{0}", UIResourceManager.GetResource<UIFont>("RC.App.Fonts.Font5"), UIWorkspace.Instance.PixelScaling, RCColor.White);
            this.backgroundBrush = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(RCColor.Black, new RCIntVector(1, this.stringToRender.Font.MinimumLineHeight), UIWorkspace.Instance.PixelScaling);
            this.backgroundBrush.Upload();
        }

        /// <summary>
        /// Starts reading the data of the given map object.
        /// </summary>
        /// <param name="objectID">The ID of the map object to read.</param>
        public void StartReadingMapObject(int objectID)
        {
            if (this.mapObjectDataView != null) { throw new InvalidOperationException("Reading map object has already been started!"); }
            this.mapObjectDataView = ComponentManager.GetInterface<IViewService>().CreateView<IMapObjectDataView, int>(objectID);
        }

        /// <summary>
        /// Stops reading the data of the map object currently being read. If reading has already been stopped or has not yet
        /// started then this function has no effect.
        /// </summary>
        public void StopReadingMapObject()
        {
            this.mapObjectDataView = null;
        }

        /// <summary>
        /// Gets the ID of the map object that is currently being read or -1 if there is no map object currently being read.
        /// </summary>
        public int MapObjectID { get { return this.mapObjectDataView != null ? this.mapObjectDataView.ObjectID : -1; } }

        /// <see cref="RCMapDisplayExtension.ConnectEx_i"/>
        protected override void ConnectEx_i()
        {
            IViewService viewService = ComponentManager.GetInterface<IViewService>();
            this.MouseSensor.Move += this.OnMouseMove;
        }

        /// <see cref="RCMapDisplayExtension.DisconnectEx_i"/>
        protected override void DisconnectEx_i()
        {
            this.MouseSensor.Move -= this.OnMouseMove;
        }

        /// <see cref="RCMapDisplayExtension.RenderEx_i"/>
        protected override void RenderEx_i(IUIRenderContext renderContext)
        {
            if (this.drawPosition != RCIntVector.Undefined && this.mapObjectDataView != null)
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
