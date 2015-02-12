using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.Views;
using RC.Common;
using RC.UI;

namespace RC.App.PresLogic
{
    /// <summary>
    /// Stores the sprite buffers of the minimap.
    /// </summary>
    class MinimapSpriteBuffer
    {
        /// <summary>
        /// Constructs a MinimapSpriteBuffer instance.
        /// </summary>
        /// <param name="minimapView">Reference to the view that is used to collect the informations for the initialization.</param>
        /// <param name="tileSpriteGroup">Reference to the sprite group of the isometric tiles.</param>
        /// <param name="terrainObjectSpriteGroup">Reference to the sprite group of the terrain objects.</param>
        public MinimapSpriteBuffer(IMinimapView minimapView, ISpriteGroup tileSpriteGroup, ISpriteGroup terrainObjectSpriteGroup)
        {
            if (minimapView == null) { throw new ArgumentNullException("minimapView"); }
            if (tileSpriteGroup == null) { throw new ArgumentNullException("tileSpriteGroup"); }
            if (terrainObjectSpriteGroup == null) { throw new ArgumentNullException("terrainObjectSpriteGroup"); }

            this.tileSpriteGroup = tileSpriteGroup;
            this.terrainObjectSpriteGroup = terrainObjectSpriteGroup;

            this.isoTileInfos = new List<SpriteInst>(minimapView.GetIsoTileSprites());
            this.terrainObjectInfos = new List<SpriteInst>(minimapView.GetTerrainObjectSprites());
            this.mapPixelSize = minimapView.MapPixelSize;
            this.minimapPixelSize = minimapView.MinimapPosition.Size;

            this.terrainPrimaryBuffer = null;
            this.terrainSecondaryBuffer = null;
            this.fowPrimaryBuffer = null;
            this.fowSecondaryBuffer = null;
            this.entitiesPrimaryBuffer = null;
            this.entitiesSecondaryBuffer = null;
            this.attackSignalsPrimaryBuffer = null;
            this.attackSignalsSecondaryBuffer = null;
        }

        #region Init & Destroy methods

        /// <summary>
        /// Initializes the buffers.
        /// </summary>
        public void InitBuffers()
        {
            this.terrainPrimaryBuffer = this.CreateTerrainSprite();
            this.terrainSecondaryBuffer = this.CloneSprite(this.terrainPrimaryBuffer);

            /// TODO: upload all!
            this.terrainPrimaryBuffer.Upload();
            this.terrainSecondaryBuffer.Upload();
        }

        /// <summary>
        /// Destroys the buffers.
        /// </summary>
        public void DestroyBuffers()
        {
            if (this.terrainPrimaryBuffer != null)
            {
                UIRoot.Instance.GraphicsPlatform.SpriteManager.DestroySprite(this.terrainPrimaryBuffer);
                this.terrainPrimaryBuffer = null;
            }
            if (this.terrainSecondaryBuffer != null)
            {
                UIRoot.Instance.GraphicsPlatform.SpriteManager.DestroySprite(this.terrainSecondaryBuffer);
                this.terrainSecondaryBuffer = null;
            }
            if (this.fowPrimaryBuffer != null)
            {
                UIRoot.Instance.GraphicsPlatform.SpriteManager.DestroySprite(this.fowPrimaryBuffer);
                this.fowPrimaryBuffer = null;
            }
            if (this.fowSecondaryBuffer != null)
            {
                UIRoot.Instance.GraphicsPlatform.SpriteManager.DestroySprite(this.fowSecondaryBuffer);
                this.fowSecondaryBuffer = null;
            }
            if (this.entitiesPrimaryBuffer != null)
            {
                UIRoot.Instance.GraphicsPlatform.SpriteManager.DestroySprite(this.entitiesPrimaryBuffer);
                this.entitiesPrimaryBuffer = null;
            }
            if (this.entitiesSecondaryBuffer != null)
            {
                UIRoot.Instance.GraphicsPlatform.SpriteManager.DestroySprite(this.entitiesSecondaryBuffer);
                this.entitiesSecondaryBuffer = null;
            }
            if (this.attackSignalsPrimaryBuffer != null)
            {
                UIRoot.Instance.GraphicsPlatform.SpriteManager.DestroySprite(this.attackSignalsPrimaryBuffer);
                this.attackSignalsPrimaryBuffer = null;
            }
            if (this.attackSignalsSecondaryBuffer != null)
            {
                UIRoot.Instance.GraphicsPlatform.SpriteManager.DestroySprite(this.attackSignalsSecondaryBuffer);
                this.attackSignalsSecondaryBuffer = null;
            }
        }

        #endregion Init & Destroy methods

        #region Properties for accessing the minimap sprites

        /// <summary>
        /// The sprite for rendering the terrain on the minimap.
        /// </summary>
        public UISprite TerrainSprite { get { return this.terrainPrimaryBuffer; } }

        /// <summary>
        /// The sprite for rendering the Fog Of War on the minimap.
        /// </summary>
        public UISprite FOWSprite { get { return this.fowPrimaryBuffer; } }

        /// <summary>
        /// The sprite for rendering the entities on the minimap.
        /// </summary>
        public UISprite EntitiesSprite { get { return this.entitiesPrimaryBuffer; } }

        /// <summary>
        /// The sprite for rendering the attack signals on the minimap.
        /// </summary>
        public UISprite AttackSignalsSprite { get { return this.attackSignalsPrimaryBuffer; } }

        #endregion Properties for accessing the minimap sprites

        #region Methods for checkout the minimap sprites

        /// <summary>
        /// Checkout the terrain sprite for updating.
        /// </summary>
        /// <returns>The terrain sprite to be updated.</returns>
        public UISprite CheckoutTerrainSprite()
        {
            if (this.terrainSecondaryBuffer == null) { throw new InvalidOperationException("The sprite has already been checked out!"); }

            UISprite checkedoutSprite = this.terrainSecondaryBuffer;
            this.terrainSecondaryBuffer = null;
            return checkedoutSprite;
        }

        /// <summary>
        /// Checkout the Fog Of War sprite for updating.
        /// </summary>
        /// <returns>The Fog Of War sprite to be updated.</returns>
        public UISprite CheckoutFOWSprite()
        {
            if (this.fowSecondaryBuffer == null) { throw new InvalidOperationException("The sprite has already been checked out!"); }

            UISprite checkedoutSprite = this.fowSecondaryBuffer;
            this.fowSecondaryBuffer = null;
            return checkedoutSprite;
        }

        /// <summary>
        /// Checkout the entities sprite for updating.
        /// </summary>
        /// <returns>The entities sprite to be updated.</returns>
        public UISprite CheckoutEntitiesSprite()
        {
            if (this.entitiesSecondaryBuffer == null) { throw new InvalidOperationException("The sprite has already been checked out!"); }

            UISprite checkedoutSprite = this.entitiesSecondaryBuffer;
            this.entitiesSecondaryBuffer = null;
            return checkedoutSprite;
        }

        /// <summary>
        /// Checkout the attack signals sprite for updating.
        /// </summary>
        /// <returns>The attack signals sprite to be updated.</returns>
        public UISprite CheckoutAttackSignalsSprite()
        {
            if (this.attackSignalsSecondaryBuffer == null) { throw new InvalidOperationException("The sprite has already been checked out!"); }

            UISprite checkedoutSprite = this.attackSignalsSecondaryBuffer;
            this.attackSignalsSecondaryBuffer = null;
            return checkedoutSprite;
        }

        #endregion Methods for checkout the minimap sprites

        #region Methods for checkin the minimap sprites

        /// <summary>
        /// Checkin the updated terrain sprite.
        /// </summary>
        /// <param name="sprite">The sprite to checkin.</param>
        public void CheckinTerrainSprite(UISprite sprite)
        {
            if (sprite == null) { throw new ArgumentNullException("sprite"); }
            if (!sprite.IsUploaded) { throw new ArgumentException("The sprite is not uploaded to the graphics device!", "sprite"); }
            if (this.terrainSecondaryBuffer != null) { throw new InvalidOperationException("The sprite has not yet been checked out!"); }

            this.terrainSecondaryBuffer = this.terrainPrimaryBuffer;
            this.terrainPrimaryBuffer = sprite;
        }

        /// <summary>
        /// Checkin the updated Fog Of War sprite.
        /// </summary>
        /// <param name="sprite">The sprite to checkin.</param>
        public void CheckinFOWSprite(UISprite sprite)
        {
            if (sprite == null) { throw new ArgumentNullException("sprite"); }
            if (!sprite.IsUploaded) { throw new ArgumentException("The sprite is not uploaded to the graphics device!", "sprite"); }
            if (this.fowSecondaryBuffer != null) { throw new InvalidOperationException("The sprite has not yet been checked out!"); }

            this.fowSecondaryBuffer = this.fowPrimaryBuffer;
            this.fowPrimaryBuffer = sprite;
        }

        /// <summary>
        /// Checkin the updated entities sprite.
        /// </summary>
        /// <param name="sprite">The sprite to checkin.</param>
        public void CheckinEntitiesSprite(UISprite sprite)
        {
            if (sprite == null) { throw new ArgumentNullException("sprite"); }
            if (!sprite.IsUploaded) { throw new ArgumentException("The sprite is not uploaded to the graphics device!", "sprite"); }
            if (this.entitiesSecondaryBuffer != null) { throw new InvalidOperationException("The sprite has not yet been checked out!"); }

            this.entitiesSecondaryBuffer = this.entitiesPrimaryBuffer;
            this.entitiesPrimaryBuffer = sprite;
        }

        /// <summary>
        /// Checkin the updated attack signals sprite.
        /// </summary>
        /// <param name="sprite">The sprite to checkin.</param>
        public void CheckinAttackSignalsSprite(UISprite sprite)
        {
            if (sprite == null) { throw new ArgumentNullException("sprite"); }
            if (!sprite.IsUploaded) { throw new ArgumentException("The sprite is not uploaded to the graphics device!", "sprite"); }
            if (this.attackSignalsSecondaryBuffer != null) { throw new InvalidOperationException("The sprite has not yet been checked out!"); }

            this.attackSignalsSecondaryBuffer = this.attackSignalsPrimaryBuffer;
            this.attackSignalsPrimaryBuffer = sprite;
        }

        #endregion Methods for checkin the minimap sprites

        #region Internal methods

        /// <summary>
        /// Creates the sprite for rendering the terrain on the minimap.
        /// </summary>
        /// <returns>The created sprite.</returns>
        private UISprite CreateTerrainSprite()
        {
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
            UISprite minimapImage = UIRoot.Instance.GraphicsPlatform.SpriteManager.ShrinkSprite(fullMapImage, this.minimapPixelSize, UIWorkspace.Instance.PixelScaling);

            /// Destroy the full map image as it is no longer needed.
            UIRoot.Instance.GraphicsPlatform.SpriteManager.DestroySprite(fullMapImage);

            return minimapImage;
        }

        /// <summary>
        /// Clones the given sprite.
        /// </summary>
        /// <param name="spriteToClone">The sprite to be cloned.</param>
        /// <returns>The cloned sprite.</returns>
        private UISprite CloneSprite(UISprite spriteToClone)
        {
            UISprite clonedSprite = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(RCColor.Black, spriteToClone.Size, spriteToClone.PixelSize);
            IUIRenderContext clonedSpriteCtx = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateRenderContext(clonedSprite);
            clonedSpriteCtx.RenderSprite(spriteToClone, new RCIntVector(0, 0));
            UIRoot.Instance.GraphicsPlatform.SpriteManager.CloseRenderContext(clonedSprite);
            return clonedSprite;
        }

        #endregion Internal methods

        /// <summary>
        /// List of the stored buffers.
        /// </summary>
        private UISprite terrainPrimaryBuffer;
        private UISprite terrainSecondaryBuffer;
        private UISprite fowPrimaryBuffer;
        private UISprite fowSecondaryBuffer;
        private UISprite entitiesPrimaryBuffer;
        private UISprite entitiesSecondaryBuffer;
        private UISprite attackSignalsPrimaryBuffer;
        private UISprite attackSignalsSecondaryBuffer;

        /// <summary>
        /// List of the informations for the initialization.
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
