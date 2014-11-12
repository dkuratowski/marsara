using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.UI;
using RC.Common.ComponentModel;
using RC.App.BizLogic;
using RC.App.BizLogic.Views;
using RC.App.BizLogic.Services;

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
        public RCMapDisplayBasic(RCIntVector position, RCIntVector size)
            : base(position, size)
        {
            this.mapView = null;
            this.tiles = null;
            this.terrainObjects = null;
        }

        /// <summary>
        /// Gets the sprites of the terrain object types.
        /// </summary>
        public SpriteGroup TerrainObjectSprites { get { return this.terrainObjects; } }

        #region Overrides

        /// <see cref="RCMapDisplay.MapView"/>
        protected override IMapView MapView { get { return this.mapView; } }

        /// <see cref="RCMapDisplay.Connect_i"/>
        protected override void Connect_i()
        {
            IViewService viewService = ComponentManager.GetInterface<IViewService>();
            this.mapView = viewService.CreateView<IMapTerrainView>();
            ITileSetView tilesetView = viewService.CreateView<ITileSetView>();
            this.tiles = new IsoTileSpriteGroup(tilesetView);
            this.terrainObjects = new TerrainObjectSpriteGroup(tilesetView);
        }

        /// <see cref="RCMapDisplay.ConnectBackgroundProc_i"/>
        protected override void ConnectBackgroundProc_i(object parameter)
        {
            this.tiles.Load();
            this.terrainObjects.Load();
        }

        /// <see cref="RCMapDisplay.DisconnectBackgroundProc_i"/>
        protected override void DisconnectBackgroundProc_i(object parameter)
        {
            this.tiles.Unload();
            this.terrainObjects.Unload();
            this.mapView = null;
            this.tiles = null;
            this.terrainObjects = null;
        }

        /// <see cref="UIObject.Render_i"/>
        protected override void Render_i(IUIRenderContext renderContext)
        {
            if (this.DisplayedArea != RCIntRectangle.Undefined)
            {
                /// Display the currently visible tiles.
                foreach (SpriteInst tileDisplayInfo in this.mapView.GetVisibleIsoTiles(this.DisplayedArea))
	            {
                    UISprite tileToDisplay = this.tiles[tileDisplayInfo.Index];
                    renderContext.RenderSprite(tileToDisplay, tileDisplayInfo.DisplayCoords, tileDisplayInfo.Section);
                }

                /// Display the currently visible terrain objects.
                foreach (SpriteInst terrainObjDisplayInfo in this.mapView.GetVisibleTerrainObjects(this.DisplayedArea))
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
    }
}
