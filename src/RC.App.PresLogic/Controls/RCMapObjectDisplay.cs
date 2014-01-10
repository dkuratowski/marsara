using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.PublicInterfaces;
using RC.UI;
using RC.Common;
using RC.Common.Diagnostics;

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
        /// <param name="mapObjectView">Reference to a map object view.</param>
        /// <param name="metadataView">Reference to the metadata view.</param>
        public RCMapObjectDisplay(RCMapDisplay extendedControl, IMapObjectView mapObjectView, IMetadataView metadataView)
            : base(extendedControl, mapObjectView)
        {
            if (mapObjectView == null) { throw new ArgumentNullException("mapObjectView"); }
            if (metadataView == null) { throw new ArgumentNullException("metadataView"); }

            this.mapObjectView = mapObjectView;

            this.mapObjectSprites = new List<SpriteGroup>();
            this.mapObjectSprites.Add(new MapObjectSpriteGroup(metadataView, PlayerEnum.Player0));
            this.mapObjectSprites.Add(new MapObjectSpriteGroup(metadataView, PlayerEnum.Player1));
            this.mapObjectSprites.Add(new MapObjectSpriteGroup(metadataView, PlayerEnum.Player2));
            this.mapObjectSprites.Add(new MapObjectSpriteGroup(metadataView, PlayerEnum.Player3));
            this.mapObjectSprites.Add(new MapObjectSpriteGroup(metadataView, PlayerEnum.Player4));
            this.mapObjectSprites.Add(new MapObjectSpriteGroup(metadataView, PlayerEnum.Player5));
            this.mapObjectSprites.Add(new MapObjectSpriteGroup(metadataView, PlayerEnum.Player6));
            this.mapObjectSprites.Add(new MapObjectSpriteGroup(metadataView, PlayerEnum.Player7));
            this.mapObjectSprites.Add(new MapObjectSpriteGroup(metadataView, PlayerEnum.Neutral));
        }

        /// <summary>
        /// Gets the sprite group of the map object types for the given player.
        /// </summary>
        /// <param name="player">The player that owns the sprite group..</param>
        /// <returns>The sprite group of the map object types for the given player.</returns>
        public SpriteGroup GetMapObjectSprites(PlayerEnum player) { return this.mapObjectSprites[player != PlayerEnum.Neutral ? (int)player : this.mapObjectSprites.Count - 1]; }

        #region Overrides

        /// <see cref="RCMapDisplayExtension.StartExtensionProc_i"/>
        protected override void StartExtensionProc_i()
        {
            foreach (SpriteGroup spriteGroup in this.mapObjectSprites)
            {
                spriteGroup.Load();
            }
        }

        /// <see cref="RCMapDisplayExtension.StopExtensionProc_i"/>
        protected override void StopExtensionProc_i()
        {
            foreach (SpriteGroup spriteGroup in this.mapObjectSprites)
            {
                spriteGroup.Unload();
            }
        }

        /// <see cref="RCMapDisplayExtension.RenderExtension_i"/>
        protected override void RenderExtension_i(IUIRenderContext renderContext)
        {
            /// Retrieve the list of the visible map objects.
            List<ObjectInst> mapObjects = this.mapObjectView.GetVisibleMapObjects(this.DisplayedArea);

            /// Render the object sprites.
            foreach (ObjectInst obj in mapObjects)
            {
                foreach (SpriteInst spriteToDisplay in obj.Sprites)
                {
                    if (spriteToDisplay.Index != -1)
                    {
                        SpriteGroup spriteGroup = this.GetMapObjectSprites(obj.Owner);
                        renderContext.RenderSprite(spriteGroup[spriteToDisplay.Index],
                                                   spriteToDisplay.DisplayCoords,
                                                   spriteToDisplay.Section);
                    }
                }
            }
        }

        #endregion Overrides

        /// <summary>
        /// This sprite-group contains the sprites of the map object types.
        /// The Nth sprite group in this list contains the variant of the sprites for PlayerN.
        /// The last sprite group in this list contains the neutral variants of the sprites.
        /// </summary>
        private List<SpriteGroup> mapObjectSprites;

        /// <summary>
        /// Reference to the map object view.
        /// </summary>
        private IMapObjectView mapObjectView;

        /// <summary>
        /// The default color of the transparent parts of the map object sprites.
        /// </summary>
        public static readonly UIColor DEFAULT_MAPOBJECT_TRANSPARENT_COLOR = new UIColor(255, 0, 255);

        /// <summary>
        /// The default owner mask color of the map object sprites.
        /// </summary>
        public static readonly UIColor DEFAULT_MAPOBJECT_OWNERMASK_COLOR = new UIColor(0, 255, 255);
    }
}
