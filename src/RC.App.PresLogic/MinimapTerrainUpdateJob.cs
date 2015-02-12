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
    /// Background job that renders the terrain of the map into a sprite for the minimap.
    /// </summary>
    class MinimapTerrainUpdateJob : IMinimapBackgroundJob
    {
        /// <summary>
        /// Constructs a MinimapTerrainUpdateJob instance.
        /// </summary>
        /// <param name="targetSprite">The target of the render job.</param>
        /// <param name="tileSpriteGroup">Reference to the sprite group of the isometric tiles.</param>
        /// <param name="terrainObjectSpriteGroup">Reference to the sprite group of the terrain objects.</param>
        public MinimapTerrainUpdateJob(UISprite targetSprite, IMinimapView minimapView, ISpriteGroup tileSpriteGroup, ISpriteGroup terrainObjectSpriteGroup)
        {
            if (targetSprite == null) { throw new ArgumentNullException("targetSprite"); }
            if (minimapView == null) { throw new ArgumentNullException("minimapView"); }
            if (tileSpriteGroup == null) { throw new ArgumentNullException("tileSpriteGroup"); }
            if (terrainObjectSpriteGroup == null) { throw new ArgumentNullException("terrainObjectSpriteGroup"); }

            this.targetSprite = targetSprite;
            this.isoTileInfos = new List<SpriteInst>(minimapView.GetIsoTileSprites());
            this.terrainObjectInfos = new List<SpriteInst>(minimapView.GetTerrainObjectSprites());
            this.mapPixelSize = minimapView.MapPixelSize;
            this.minimapPixelSize = minimapView.MinimapPosition.Size;

            this.tileSpriteGroup = tileSpriteGroup;
            this.terrainObjectSpriteGroup = terrainObjectSpriteGroup;
        }

        #region IMinimapBackgroundJob methods

        /// <see cref="IMinimapBackgroundJob.Execute"/>
        public void Execute()
        {
            /// Destroy the old terrain sprite.
            UIRoot.Instance.GraphicsPlatform.SpriteManager.DestroySprite(this.targetSprite);

            /// TODO: this is a duplicate of MinimapSpriteBuffer.CreateTerrainSprite!!!

            /// Create the sprite that will contain the image of the full map in original size.
            UISprite fullMapImage = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(RCColor.Black, this.mapPixelSize);
            IUIRenderContext fullMapImageRenderContext = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateRenderContext(fullMapImage);

            /// Render the isometric tiles to the map image.
            foreach (SpriteInst tileDisplayInfo in this.isoTileInfos)
            {
                UISprite tileToDisplay = this.tileSpriteGroup[tileDisplayInfo.Index];
                fullMapImageRenderContext.RenderSprite(tileToDisplay, tileDisplayInfo.DisplayCoords, tileDisplayInfo.Section);
            }

            /// Render the terrain objects to the map image.
            foreach (SpriteInst terrainObjDisplayInfo in this.terrainObjectInfos)
            {
                UISprite terrainObjToDisplay = this.terrainObjectSpriteGroup[terrainObjDisplayInfo.Index];
                fullMapImageRenderContext.RenderSprite(terrainObjToDisplay, terrainObjDisplayInfo.DisplayCoords, terrainObjDisplayInfo.Section);
            }

            /// Close the render context of the map image.
            UIRoot.Instance.GraphicsPlatform.SpriteManager.CloseRenderContext(fullMapImage);

            /// Create the shrinked copy of the full map image.
            this.targetSprite = UIRoot.Instance.GraphicsPlatform.SpriteManager.ShrinkSprite(fullMapImage, this.minimapPixelSize, UIWorkspace.Instance.PixelScaling);

            /// Destroy the full map image as it is no longer needed.
            UIRoot.Instance.GraphicsPlatform.SpriteManager.DestroySprite(fullMapImage);

            /// Upload the create minimap image.
            this.targetSprite.Upload();
        }

        #endregion IMinimapBackgroundJob methods

        /// <summary>
        /// Gets the target sprite of this render job.
        /// </summary>
        public UISprite TargetSprite { get { return this.targetSprite; } }

        /// <summary>
        /// The target of the render job.
        /// </summary>
        private UISprite targetSprite;

        /// <summary>
        /// List of the informations for the rendering.
        /// </summary>
        private List<SpriteInst> isoTileInfos;
        private List<SpriteInst> terrainObjectInfos;
        private RCIntVector mapPixelSize;
        private RCIntVector minimapPixelSize;

        /// <summary>
        /// Reference to the sprite group of the isometric tiles.
        /// </summary>
        private readonly ISpriteGroup tileSpriteGroup;

        /// <summary>
        /// Reference to the sprite group of the terrain objects.
        /// </summary>
        private readonly ISpriteGroup terrainObjectSpriteGroup;
    }
}
