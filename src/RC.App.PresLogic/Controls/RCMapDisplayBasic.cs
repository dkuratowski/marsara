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
        /// <param name="isoTileSpriteGroup">Reference to the sprites of the isometric tile types.</param>
        /// <param name="terrainObjectSpriteGroup">Reference to the sprites of the terrain object types.</param>
        /// <param name="position">The position of the map display control.</param>
        /// <param name="size">The size of the map display control.</param>
        public RCMapDisplayBasic(ISpriteGroup isoTileSpriteGroup, ISpriteGroup terrainObjectSpriteGroup, RCIntVector position, RCIntVector size)
            : base(position, size)
        {
            if (isoTileSpriteGroup == null) { throw new ArgumentNullException("isoTileSpriteGroup"); }
            if (terrainObjectSpriteGroup == null) { throw new ArgumentNullException("terrainObjectSpriteGroup"); }

            this.mapTerrainView = null;
            this.isoTileSpriteGroup = isoTileSpriteGroup;
            this.terrainObjectSpriteGroup = terrainObjectSpriteGroup;
        }

        #region Overrides

        /// <see cref="RCMapDisplay.Connect_i"/>
        protected override void Connect_i()
        {
            this.mapTerrainView = ComponentManager.GetInterface<IViewService>().CreateView<IMapTerrainView>();
        }

        /// <see cref="RCMapDisplay.Disconnect_i"/>
        protected override void Disconnect_i()
        {
            this.mapTerrainView = null;
        }

        /// <see cref="UIObject.Render_i"/>
        protected override void Render_i(IUIRenderContext renderContext)
        {
            if (this.ConnectionStatus == ConnectionStatusEnum.Online)
            {
                /// Render the isometric tiles inside the displayed area.
                foreach (SpriteInst tileDisplayInfo in this.mapTerrainView.GetVisibleIsoTiles())
                {
                    UISprite tileToDisplay = this.isoTileSpriteGroup[tileDisplayInfo.Index];
                    renderContext.RenderSprite(tileToDisplay, tileDisplayInfo.DisplayCoords, tileDisplayInfo.Section);
                }

                /// Render the terrain objects inside the displayed area.
                foreach (SpriteInst terrainObjDisplayInfo in this.mapTerrainView.GetVisibleTerrainObjects())
                {
                    UISprite terrainObjToDisplay = this.terrainObjectSpriteGroup[terrainObjDisplayInfo.Index];
                    renderContext.RenderSprite(terrainObjToDisplay, terrainObjDisplayInfo.DisplayCoords,
                        terrainObjDisplayInfo.Section);
                }
            }
        }

        #endregion Overrides

        /// <summary>
        /// Reference to the sprites of the isometric tile types.
        /// </summary>
        private readonly ISpriteGroup isoTileSpriteGroup;

        /// <summary>
        /// Reference to the sprites of the terrain object types.
        /// </summary>
        private readonly ISpriteGroup terrainObjectSpriteGroup;

        /// <summary>
        /// Reference to the map view.
        /// </summary>
        private IMapTerrainView mapTerrainView;
    }
}
