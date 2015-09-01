using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.Views;
using RC.App.PresLogic.SpriteGroups;
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

            this.terrainRenderer = new MinimapTerrainRenderJob(minimapView, tileSpriteGroup, terrainObjectSpriteGroup);
            this.bufferSize = minimapView.MinimapPosition.Size;

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
            /// Render the terrain sprite using the underlying MinimapTerrainRenderJob instance and clone it.
            this.terrainRenderer.Execute();
            this.terrainPrimaryBuffer = this.terrainRenderer.Result;
            this.terrainSecondaryBuffer = this.CloneSprite(this.terrainPrimaryBuffer);
            this.terrainSecondaryBuffer.Upload();

            /// Create the other buffers with their default content.
            this.fowPrimaryBuffer = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(RCColor.Black, this.bufferSize, UIWorkspace.Instance.PixelScaling);
            this.fowSecondaryBuffer = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(RCColor.Black, this.bufferSize, UIWorkspace.Instance.PixelScaling);
            this.entitiesPrimaryBuffer = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(PresLogicConstants.DEFAULT_TRANSPARENT_COLOR, this.bufferSize, UIWorkspace.Instance.PixelScaling);
            this.entitiesSecondaryBuffer = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(PresLogicConstants.DEFAULT_TRANSPARENT_COLOR, this.bufferSize, UIWorkspace.Instance.PixelScaling);
            this.attackSignalsPrimaryBuffer = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(PresLogicConstants.DEFAULT_TRANSPARENT_COLOR, this.bufferSize, UIWorkspace.Instance.PixelScaling);
            this.attackSignalsSecondaryBuffer = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(PresLogicConstants.DEFAULT_TRANSPARENT_COLOR, this.bufferSize, UIWorkspace.Instance.PixelScaling);

            /// Set the transparent color of the other buffers.
            this.fowPrimaryBuffer.TransparentColor = PresLogicConstants.DEFAULT_TRANSPARENT_COLOR;
            this.fowSecondaryBuffer.TransparentColor = PresLogicConstants.DEFAULT_TRANSPARENT_COLOR;
            this.entitiesPrimaryBuffer.TransparentColor = PresLogicConstants.DEFAULT_TRANSPARENT_COLOR;
            this.entitiesSecondaryBuffer.TransparentColor = PresLogicConstants.DEFAULT_TRANSPARENT_COLOR;
            this.attackSignalsPrimaryBuffer.TransparentColor = PresLogicConstants.DEFAULT_TRANSPARENT_COLOR;
            this.attackSignalsSecondaryBuffer.TransparentColor = PresLogicConstants.DEFAULT_TRANSPARENT_COLOR;

            /// Upload the other buffers.
            this.fowPrimaryBuffer.Upload();
            this.fowSecondaryBuffer.Upload();
            this.entitiesPrimaryBuffer.Upload();
            this.entitiesSecondaryBuffer.Upload();
            this.attackSignalsPrimaryBuffer.Upload();
            this.attackSignalsSecondaryBuffer.Upload();
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
            clonedSprite.TransparentColor = spriteToClone.TransparentColor;
            return clonedSprite;
        }

        #endregion Internal methods

        /// <summary>
        /// The size of the buffers.
        /// </summary>
        private RCIntVector bufferSize;

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
        /// Reference to the terrain renderer.
        /// </summary>
        private MinimapTerrainRenderJob terrainRenderer;
    }
}
