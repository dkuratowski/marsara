using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.UI;
using RC.Common;
using RC.Common.ComponentModel;
using RC.App.BizLogic.Views;
using RC.App.BizLogic.Services;

namespace RC.App.PresLogic.Controls
{
    /// <summary>
    /// Adds new functionality to the map display control for displaying the isometric grid of the map and/or highlight the isometric tile
    /// that is currently under the mouse pointer.
    /// </summary>
    public class RCIsoTileDisplay : RCMapDisplayExtension
    {
        /// <summary>
        /// Constructs an RCIsoTileDisplay extension for the given map display control.
        /// </summary>
        /// <param name="extendedControl">The map display control to extend.</param>
        public RCIsoTileDisplay(RCMapDisplay extendedControl)
            : base(extendedControl)
        {
            this.mapTerrainView = null;
            this.highlightIsoTile = false;
            this.isotileHighlightedSprite = UIResourceManager.GetResource<UISprite>("RC.MapEditor.Sprites.IsotileHighlighted");
            this.isotileNormalSprite = UIResourceManager.GetResource<UISprite>("RC.MapEditor.Sprites.IsotileNormal");
            this.lastKnownMousePosition = RCIntVector.Undefined;
        }

        #region Extension settings

        /// <summary>
        /// Gets or sets whether this RCIsoTileDisplay extension has to highlight the isometric tile under the mouse pointer.
        /// </summary>
        public bool HighlightIsoTile
        {
            get { return this.highlightIsoTile; }
            set { this.highlightIsoTile = value; }
        }

        /// <summary>
        /// Gets or sets whether this RCIsoTileDisplay extension has to display the isometric grid of the map or not.
        /// </summary>
        /// <remarks>TODO: implement this functionality if needed.</remarks>
        public bool DisplayIsoGrid
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        #endregion Extension settings

        /// <see cref="RCMapDisplayExtension.ConnectEx_i"/>
        protected override void ConnectEx_i()
        {
            IViewService viewService = ComponentManager.GetInterface<IViewService>();
            this.mapTerrainView = viewService.CreateView<IMapTerrainView>();
            this.MouseSensor.Move += this.OnMouseMove;
        }

        /// <see cref="RCMapDisplayExtension.DisconnectEx_i"/>
        protected override void DisconnectEx_i()
        {
            this.mapTerrainView = null;
            this.MouseSensor.Move -= this.OnMouseMove;
        }

        /// <see cref="RCMapDisplayExtension.RenderEx_i"/>
        protected override void RenderEx_i(IUIRenderContext renderContext)
        {
            if (this.highlightIsoTile &&
                this.lastKnownMousePosition != RCIntVector.Undefined)
            {
                renderContext.RenderSprite(this.isotileHighlightedSprite, this.mapTerrainView.GetIsoTileDisplayCoords(this.lastKnownMousePosition));
            }
        }

        /// <summary>
        /// Called when the mouse pointer has been moved over the display.
        /// </summary>
        private void OnMouseMove(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            if (this.lastKnownMousePosition != evtArgs.Position)
            {
                this.lastKnownMousePosition = evtArgs.Position;
            }
        }

        /// <summary>
        /// This flag indicates whether this RCIsoTileDisplay extension has to highlight the isometric tile under the mouse pointer.
        /// </summary>
        private bool highlightIsoTile;

        /// <summary>
        /// The sprite that is used to highlighting the isometric tile under the mouse pointer.
        /// </summary>
        private UISprite isotileHighlightedSprite;

        /// <summary>
        /// The sprite that is used to display the isometric grid of the map.
        /// </summary>
        private UISprite isotileNormalSprite;

        /// <summary>
        /// The last known position of the mouse cursor in the coordinate system of the display.
        /// </summary>
        private RCIntVector lastKnownMousePosition;

        /// <summary>
        /// Reference to the map terrain view.
        /// </summary>
        private IMapTerrainView mapTerrainView;
    }
}
