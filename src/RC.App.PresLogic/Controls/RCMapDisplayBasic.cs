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
            this.tiles = new IsoTileSpriteGroup(tilesetView);
            this.terrainObjects = new TerrainObjectSpriteGroup(tilesetView);
        }

        /// <summary>
        /// Gets the sprites of the terrain object types.
        /// </summary>
        public SpriteGroup TerrainObjectSprites { get { return this.terrainObjects; } }

        #region Overrides

        /// <see cref="RCMapDisplay.StartProc_i"/>
        protected override void StartProc_i(object parameter)
        {
            this.tiles.Load();
            this.terrainObjects.Load();
        }

        /// <see cref="RCMapDisplay.StopProc_i"/>
        protected override void StopProc_i(object parameter)
        {
            this.tiles.Unload();
            this.terrainObjects.Unload();
        }

        /// <see cref="UIObject.Render_i"/>
        protected override void Render_i(IUIRenderContext renderContext)
        {
            if (this.DisplayedArea != RCIntRectangle.Undefined)
            {
                /// Display the currently visible tiles.
                foreach (MapSpriteInstance tileDisplayInfo in this.mapView.GetVisibleIsoTiles(this.DisplayedArea))
	            {
                    UISprite tileToDisplay = this.tiles[tileDisplayInfo.Index];
                    renderContext.RenderSprite(tileToDisplay, tileDisplayInfo.DisplayCoords, tileDisplayInfo.Section);
                }

                /// Display the currently visible terrain objects.
                foreach (MapSpriteInstance terrainObjDisplayInfo in this.mapView.GetVisibleTerrainObjects(this.DisplayedArea))
                {
                    UISprite terrainObjToDisplay = this.terrainObjects[terrainObjDisplayInfo.Index];
                    renderContext.RenderSprite(terrainObjToDisplay, terrainObjDisplayInfo.DisplayCoords, terrainObjDisplayInfo.Section);
                }
            }
        }

        #endregion Overrides

        /// <summary>
        /// List of the tiles in the tileset of the currently displayed map.
        /// </summary>
        private SpriteGroup tiles;

        /// <summary>
        /// List of the terrain objects in the tileset of the currently displayed map.
        /// </summary>
        private SpriteGroup terrainObjects;

        /// <summary>
        /// Reference to the map view.
        /// </summary>
        private IMapTerrainView mapView;

        /// <summary>
        /// The default color of the transparent parts of the tiles.
        /// </summary>
        public static readonly UIColor DEFAULT_TILE_TRANSPARENT_COLOR = new UIColor(255, 0, 255);
    }
}
