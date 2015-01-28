using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.Services;
using RC.App.BizLogic.Views;
using RC.Common;
using RC.Common.ComponentModel;
using RC.UI;

namespace RC.App.PresLogic
{
    /// <summary>
    /// Helper class for rendering the terrain of the map into a given render context.
    /// </summary>
    class MapTerrainRenderer
    {
        /// <summary>
        /// Constructs a MapTerrainRenderer instance.
        /// </summary>
        /// <param name="tileSpriteGroup">Reference to the sprite group of the isometric tiles.</param>
        /// <param name="terrainObjectSpriteGroup">Reference to the sprite group of the terrain objects.</param>
        public MapTerrainRenderer(SpriteGroup tileSpriteGroup, SpriteGroup terrainObjectSpriteGroup)
        {
            if (tileSpriteGroup == null) { throw new ArgumentNullException("tileSpriteGroup"); }
            if (terrainObjectSpriteGroup == null) { throw new ArgumentNullException("terrainObjectSpriteGroup"); }

            IViewService viewService = ComponentManager.GetInterface<IViewService>();
            this.mapTerrainView = viewService.CreateView<IMapTerrainView>();
            this.tileSpriteGroup = tileSpriteGroup;
            this.terrainObjectSpriteGroup = terrainObjectSpriteGroup;
        }

        /// <summary>
        /// Renders the given area of the map into the given render context.
        /// </summary>
        /// <param name="renderContext">The context of the rendering operation.</param>
        public void Render(IUIRenderContext renderContext)
        {
            /// Render the isometric tiles inside the rendered area.
            foreach (SpriteInst tileDisplayInfo in this.mapTerrainView.GetVisibleIsoTiles())
            {
                UISprite tileToDisplay = this.tileSpriteGroup[tileDisplayInfo.Index];
                renderContext.RenderSprite(tileToDisplay, tileDisplayInfo.DisplayCoords, tileDisplayInfo.Section);
            }

            /// Render the terrain objects inside the rendered area.
            foreach (SpriteInst terrainObjDisplayInfo in this.mapTerrainView.GetVisibleTerrainObjects())
            {
                UISprite terrainObjToDisplay = this.terrainObjectSpriteGroup[terrainObjDisplayInfo.Index];
                renderContext.RenderSprite(terrainObjToDisplay, terrainObjDisplayInfo.DisplayCoords, terrainObjDisplayInfo.Section);
            }
        }

        /// <summary>
        /// Reference to the map terrain view.
        /// </summary>
        private IMapTerrainView mapTerrainView;

        /// <summary>
        /// Reference to the sprite group of the isometric tiles.
        /// </summary>
        private SpriteGroup tileSpriteGroup;

        /// <summary>
        /// Reference to the sprite group of the terrain objects.
        /// </summary>
        private SpriteGroup terrainObjectSpriteGroup;
    }
}
