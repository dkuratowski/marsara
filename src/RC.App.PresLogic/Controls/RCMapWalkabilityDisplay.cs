using RC.Common;
using RC.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.ComponentModel;
using RC.App.BizLogic.Views;
using RC.App.BizLogic.Services;

namespace RC.App.PresLogic.Controls
{
    /// <summary>
    /// Adds the following new functionalities to the map display control:
    ///     - displays the walkability of the map cells
    /// </summary>
    public class RCMapWalkabilityDisplay : RCMapDisplayExtension
    {
        /// <summary>
        /// Constructs an RCMapWalkabilityDisplay extension for the given map display control.
        /// </summary>
        /// <param name="extendedControl">The map display control to extend.</param>
        public RCMapWalkabilityDisplay(RCMapDisplay extendedControl)
            : base(extendedControl)
        {
            this.mapTerrainView = null;

            this.greenBrush = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(RCColor.Green, new RCIntVector(1, 1), UIWorkspace.Instance.PixelScaling);
            this.greenBrush.Upload();
        }

        #region Overrides

        /// <see cref="RCMapDisplayExtension.ConnectEx_i"/>
        protected override void ConnectEx_i()
        {
            IViewService viewService = ComponentManager.GetInterface<IViewService>();
            this.mapTerrainView = viewService.CreateView<IMapTerrainView>();
        }

        /// <see cref="RCMapDisplayExtension.DisconnectEx_i"/>
        protected override void DisconnectEx_i()
        {
            this.mapTerrainView = null;
        }

        /// <see cref="RCMapDisplayExtension.RenderEx_i"/>
        protected override void RenderEx_i(IUIRenderContext renderContext)
        {
            foreach (RCIntRectangle walkableCell in this.mapTerrainView.GetWalkableCells())
            {
                renderContext.RenderRectangle(this.greenBrush, walkableCell);
            }
        }

        #endregion Overrides

        /// <summary>
        /// Reference to the map terrain view.
        /// </summary>
        private IMapTerrainView mapTerrainView;

        /// <summary>
        /// The green brush to draw non-walkable cells.
        /// </summary>
        private UISprite greenBrush;
    }
}
