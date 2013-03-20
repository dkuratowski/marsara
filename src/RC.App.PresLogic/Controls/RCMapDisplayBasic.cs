using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.UI;
using RC.Common.ComponentModel;
using RC.App.BizLogic;
using RC.App.BizLogic.PublicInterfaces;

namespace RC.App.PresLogic.Controls
{
    /// <summary>
    /// Implements the basic functionalities of a map display control that can be extended with other functionalities using the
    /// RCMapDisplayExtension abstract class.
    /// </summary>
    public class RCMapDisplayBasic : RCMapDisplay
    {
        /// <summary>
        /// Constructs a map display control at the given position with the given size.
        /// </summary>
        /// <param name="position">The position of the map display control.</param>
        /// <param name="size">The size of the map display control.</param>
        /// <param name="map">Reference to a map view.</param>
        /// <param name="tilesetView">Reference to a tileset view.</param>
        public RCMapDisplayBasic(RCIntVector position, RCIntVector size, IMapTerrainView map, ITileSetView tilesetView)
            : base(position, size, map)
        {
            if (map == null) { throw new ArgumentNullException("map"); }
            if (tilesetView == null) { throw new ArgumentNullException("tilesetView"); }

            this.mapView = map;
            this.tilesetView = tilesetView;
            this.tileset = new List<UISprite>();
        }

        #region Overrides

        /// <see cref="RCMapDisplay.StartProc_i"/>
        protected override void StartProc_i(object parameter)
        {
            /// Load the tileset.
            foreach (IsoTileTypeInfo tileType in this.tilesetView.GetIsoTileTypes())
            {
                UISprite tile = UIRoot.Instance.GraphicsPlatform.SpriteManager.LoadSprite(tileType.ImageData, UIWorkspace.Instance.PixelScaling);
                tile.TransparentColor = tileType.TransparentColorStr != null ?
                                        UIResourceLoader.LoadColor(tileType.TransparentColorStr) :
                                        DEFAULT_TILE_TRANSPARENT_COLOR;
                tile.Upload();
                this.tileset.Add(tile);
            }
        }

        /// <see cref="RCMapDisplay.StopProc_i"/>
        protected override void StopProc_i(object parameter)
        {
            /// Unload the tileset.
            foreach (UISprite tile in this.tileset)
            {
                UIRoot.Instance.GraphicsPlatform.SpriteManager.DestroySprite(tile);
            }
            this.tileset.Clear();
        }

        /// <see cref="UIObject.Render_i"/>
        protected override void Render_i(IUIRenderContext renderContext)
        {
            if (this.DisplayedArea != RCIntRectangle.Undefined)
            {
                /// Display the currently visible tiles.
                foreach (IsoTileDisplayInfo tileDisplayInfo in this.mapView.GetVisibleIsoTiles(this.DisplayedArea))
	            {
                    UISprite tileToDisplay = this.tileset[tileDisplayInfo.IsoTileTypeIndex];
                    renderContext.RenderSprite(tileToDisplay, tileDisplayInfo.DisplayCoords);
	            }
            }
        }

        #endregion Overrides

        /// <summary>
        /// List of the tiles in the tileset of the currently displayed map.
        /// </summary>
        private List<UISprite> tileset;

        /// <summary>
        /// Reference to the map view.
        /// </summary>
        private IMapTerrainView mapView;

        /// <summary>
        /// Reference to the tileset view.
        /// </summary>
        private ITileSetView tilesetView;

        /// <summary>
        /// The default color of the transparent parts of the tiles.
        /// </summary>
        public readonly UIColor DEFAULT_TILE_TRANSPARENT_COLOR = new UIColor(255, 0, 255);
    }
}
