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
    /// Adds the following new functionalities to the map display control:
    ///     - displays the walkability of the map cells
    /// </summary>
    public class RCMapWalkabilityDisplay : RCMapDisplayExtension
    {
        /// <summary>
        /// Constructs an RCMapWalkabilityDisplay extension for the given map display control.
        /// </summary>
        /// <param name="extendedControl">The map display control to extend.</param>
        /// <param name="mapTerrainView">Reference to a map terrain view.</param>
        public RCMapWalkabilityDisplay(RCMapDisplay extendedControl, IMapTerrainView mapTerrainView)
            : base(extendedControl, mapTerrainView)
        {
            if (mapTerrainView == null) { throw new ArgumentNullException("mapTerrainView"); }
            this.mapTerrainView = mapTerrainView;

            this.greenBrush = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(UIColor.Green, new RCIntVector(1, 1), UIWorkspace.Instance.PixelScaling);
            this.greenBrush.Upload();
        }

        #region Overrides

        /// <see cref="RCMapDisplayExtension.RenderExtension_i"/>
        protected override void RenderExtension_i(IUIRenderContext renderContext)
        {
            foreach (RCIntRectangle walkableCell in this.mapTerrainView.GetWalkableCells(this.DisplayedArea))
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
