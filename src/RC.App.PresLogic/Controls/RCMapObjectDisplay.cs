using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using RC.App.PresLogic.SpriteGroups;
using RC.Common.Configuration;
using RC.UI;
using RC.Common;
using RC.Common.Diagnostics;
using RC.Common.ComponentModel;
using RC.App.BizLogic.Views;
using RC.App.BizLogic.Services;

namespace RC.App.PresLogic.Controls
{
    /// <summary>
    /// Adds the following new functionalities to the map display control:
    ///     - displays the map objects
    /// </summary>
    public class RCMapObjectDisplay : RCMapDisplayExtension
    {
        /// <summary>
        /// Constructs an RCMapObjectDisplay extension for the given map display control.
        /// </summary>
        /// <param name="extendedControl">The map display control to extend.</param>
        public RCMapObjectDisplay(RCMapDisplay extendedControl)
            : base(extendedControl)
        {
            this.mapObjectView = null;
            this.mapObjectSprites = new List<SpriteGroup>();
        }

        /// <summary>
        /// Gets the sprite group of the map object types for the given player.
        /// </summary>
        /// <param name="player">The player that owns the sprite group..</param>
        /// <returns>The sprite group of the map object types for the given player.</returns>
        public SpriteGroup GetMapObjectSprites(PlayerEnum player) { return this.mapObjectSprites[player != PlayerEnum.Neutral ? (int)player : this.mapObjectSprites.Count - 1]; }

        #region Overrides

        /// <see cref="RCMapDisplayExtension.ConnectEx_i"/>
        protected override void ConnectEx_i()
        {
            IViewService viewService = ComponentManager.GetInterface<IViewService>();
            this.mapObjectView = viewService.CreateView<IMapObjectView>();

            IMetadataView metadataView = viewService.CreateView<IMetadataView>();
            this.mapObjectSprites.Add(new MapObjectSpriteGroup(metadataView, PlayerEnum.Player0));
            this.mapObjectSprites.Add(new MapObjectSpriteGroup(metadataView, PlayerEnum.Player1));
            this.mapObjectSprites.Add(new MapObjectSpriteGroup(metadataView, PlayerEnum.Player2));
            this.mapObjectSprites.Add(new MapObjectSpriteGroup(metadataView, PlayerEnum.Player3));
            this.mapObjectSprites.Add(new MapObjectSpriteGroup(metadataView, PlayerEnum.Player4));
            this.mapObjectSprites.Add(new MapObjectSpriteGroup(metadataView, PlayerEnum.Player5));
            this.mapObjectSprites.Add(new MapObjectSpriteGroup(metadataView, PlayerEnum.Player6));
            this.mapObjectSprites.Add(new MapObjectSpriteGroup(metadataView, PlayerEnum.Player7));
            this.mapObjectSprites.Add(new MapObjectSpriteGroup(metadataView, PlayerEnum.Neutral));

            this.shadowSprites = new ShadowSpriteGroup(metadataView);
        }

        /// <see cref="RCMapDisplayExtension.ConnectExBackgroundProc_i"/>
        protected override void ConnectExBackgroundProc_i()
        {
            foreach (SpriteGroup spriteGroup in this.mapObjectSprites) { spriteGroup.Load(); }
            this.shadowSprites.Load();
        }

        /// <see cref="RCMapDisplayExtension.DisconnectExBackgroundProc_i"/>
        protected override void DisconnectExBackgroundProc_i()
        {
            foreach (SpriteGroup spriteGroup in this.mapObjectSprites) { spriteGroup.Unload(); }
            this.shadowSprites.Unload();
            this.shadowSprites = null;
            this.mapObjectView = null;
            this.mapObjectSprites.Clear();
        }

        /// <see cref="RCMapDisplayExtension.RenderEx_i"/>
        protected override void RenderEx_i(IUIRenderContext renderContext)
        {
            foreach (Tuple<SpriteRenderInfo, PlayerEnum> renderItem in this.mapObjectView.GetVisibleMapObjectSprites())
            {
                SpriteRenderInfo mapObjectSpriteRenderInfo = renderItem.Item1;
                PlayerEnum ownerPlayer = renderItem.Item2;
                if (mapObjectSpriteRenderInfo.SpriteGroup == SpriteGroupEnum.MapObjectSpriteGroup)
                {
                    SpriteGroup spriteGroup = this.GetMapObjectSprites(ownerPlayer);
                    renderContext.RenderSprite(spriteGroup[mapObjectSpriteRenderInfo.Index],
                                               mapObjectSpriteRenderInfo.DisplayCoords,
                                               mapObjectSpriteRenderInfo.Section);
                }
                else if (mapObjectSpriteRenderInfo.SpriteGroup == SpriteGroupEnum.MapObjectShadowSpriteGroup)
                {
                    renderContext.RenderSprite(this.shadowSprites[mapObjectSpriteRenderInfo.Index],
                                               mapObjectSpriteRenderInfo.DisplayCoords,
                                               mapObjectSpriteRenderInfo.Section);
                }
            }
        }

        #endregion Overrides

        /// <summary>
        /// This sprite-group contains the sprites of the map object types.
        /// The Nth sprite group in this list contains the variant of the sprites for PlayerN.
        /// The last sprite group in this list contains the neutral variants of the sprites.
        /// </summary>
        private readonly List<SpriteGroup> mapObjectSprites;

        /// <summary>
        /// This sprite-group contains the sprites for rendering map object shadows.
        /// </summary>
        private SpriteGroup shadowSprites;

        /// <summary>
        /// Reference to the map object view.
        /// </summary>
        private IMapObjectView mapObjectView;
    }
}
