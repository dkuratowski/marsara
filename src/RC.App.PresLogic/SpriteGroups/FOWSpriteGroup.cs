using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.Views;
using RC.Common;
using RC.Common.Configuration;
using RC.UI;

namespace RC.App.PresLogic.SpriteGroups
{
    /// <summary>
    /// The sprite-group for drawing the quadratic tiles of the map based on the current state of the Fog Of War.
    /// </summary>
    class FOWSpriteGroup : SpriteGroup
    {
        /// <summary>
        /// Constructs a FOWSpriteGroup instance from the given sprite palette.
        /// </summary>
        /// <param name="spritePalette">The sprite palette that contains the sprites for drawing the Fog Of War.</param>
        /// <param name="fowType">The type of the Fog Of War stored in this sprite-group.</param>
        public FOWSpriteGroup(ISpritePalette<FOWTypeEnum> spritePalette, FOWTypeEnum fowType)
        {
            if (spritePalette == null) { throw new ArgumentNullException("spritePalette"); }
            if (fowType == FOWTypeEnum.None) { throw new ArgumentException("FOWSpriteGroup cannot store sprites for FOWTypeEnum.None!", "fowType"); }
            if (spritePalette.TransparentColor == RCColor.Undefined) { throw new ArgumentException("Transparent color is not defined for FOW sprite palette!", "spritePalette"); }

            this.spritePalette = spritePalette;
            this.fowType = fowType;

            this.spriteIndices = new Dictionary<FOWTileFlagsEnum, int>();
            foreach (KeyValuePair<FOWTileFlagsEnum, string> item in SPRITE_NAMES)
            {
                this.spriteIndices[item.Key] = this.spritePalette.GetSpriteIndex(item.Value, this.fowType);
            }
        }

        /// <see cref="SpriteGroup.Load_i"/>
        protected override List<UISprite> Load_i()
        {
            /// Load the image of the sprite palette in order to be able to create the combined FOW-sprites.
            UISprite spritePaletteImg = UIRoot.Instance.GraphicsPlatform.SpriteManager.LoadSprite(this.spritePalette.ImageData, UIWorkspace.Instance.PixelScaling);
            spritePaletteImg.TransparentColor = this.spritePalette.TransparentColor;

            /// Create the sprites of this sprite-group.
            List<UISprite> retList = new List<UISprite>();
            Dictionary<FOWTileFlagsEnum, UISprite> fowSprites = new Dictionary<FOWTileFlagsEnum, UISprite>();
            for (FOWTileFlagsEnum flags = FOWTileFlagsEnum.None; flags < FOWTileFlagsEnum.All; flags++)
            {
                FOWTileFlagsEnum simplifiedFlags = this.SimplifyFlags(flags);
                if (!fowSprites.ContainsKey(simplifiedFlags)) { fowSprites[simplifiedFlags] = this.CreateFowSprite(simplifiedFlags, spritePaletteImg); }
                retList.Add(fowSprites[simplifiedFlags]);
            }

            /// Destroy the image of the sprite palette as it is no longer needed and return with the created sprite list.
            UIRoot.Instance.GraphicsPlatform.SpriteManager.DestroySprite(spritePaletteImg);
            return retList;
        }

        /// <summary>
        /// Simplifies the given FOW-flags.
        /// </summary>
        /// <param name="originalFlags">The FOW-flags to simplify.</param>
        /// <returns>The simplified FOW-flags.</returns>
        private FOWTileFlagsEnum SimplifyFlags(FOWTileFlagsEnum originalFlags)
        {
            if (originalFlags == FOWTileFlagsEnum.None) { return FOWTileFlagsEnum.None; }
            if (originalFlags.HasFlag(FOWTileFlagsEnum.Current)) { return FOWTileFlagsEnum.Current; }

            /// Remove the north-east flag if necessary.
            if (originalFlags.HasFlag(FOWTileFlagsEnum.NorthEast) && (originalFlags.HasFlag(FOWTileFlagsEnum.North) || originalFlags.HasFlag(FOWTileFlagsEnum.East)))
            {
                originalFlags &= ~FOWTileFlagsEnum.NorthEast;
            }

            /// Remove the south-east flag if necessary.
            if (originalFlags.HasFlag(FOWTileFlagsEnum.SouthEast) && (originalFlags.HasFlag(FOWTileFlagsEnum.East) || originalFlags.HasFlag(FOWTileFlagsEnum.South)))
            {
                originalFlags &= ~FOWTileFlagsEnum.SouthEast;
            }

            /// Remove the south-west flag if necessary.
            if (originalFlags.HasFlag(FOWTileFlagsEnum.SouthWest) && (originalFlags.HasFlag(FOWTileFlagsEnum.West) || originalFlags.HasFlag(FOWTileFlagsEnum.South)))
            {
                originalFlags &= ~FOWTileFlagsEnum.SouthWest;
            }

            /// Remove the north-west flag if necessary.
            if (originalFlags.HasFlag(FOWTileFlagsEnum.NorthWest) && (originalFlags.HasFlag(FOWTileFlagsEnum.North) || originalFlags.HasFlag(FOWTileFlagsEnum.West)))
            {
                originalFlags &= ~FOWTileFlagsEnum.NorthWest;
            }

            return originalFlags;
        }

        /// <summary>
        /// Creates a Fog Of War sprite for the given flags.
        /// </summary>
        /// <param name="flags">The FOW-flags.</param>
        /// <param name="spritePaletteImg">The image of the sprite palette.</param>
        /// <returns>The created Fog Of War sprite or null if no sprite needs to be created for the given flags.</returns>
        private UISprite CreateFowSprite(FOWTileFlagsEnum flags, UISprite spritePaletteImg)
        {
            if (flags == FOWTileFlagsEnum.None) { return null; }

            /// Create a new empty sprite and open a render context to it.
            UISprite retSprite =
                UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(
                    this.spritePalette.TransparentColor,
                    this.spritePalette.GetSection(this.spriteIndices[FOWTileFlagsEnum.Current]).Size,
                    UIWorkspace.Instance.PixelScaling);
            IUIRenderContext renderCtx = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateRenderContext(retSprite);

            /// Draw the sprites from the sprite palette to the created sprite according to the incoming flags.
            foreach (FOWTileFlagsEnum flag in this.spriteIndices.Keys)
            {
                if (flags.HasFlag(flag)) { this.CombineFowFlag(flag, spritePaletteImg, renderCtx); }
            }

            /// Close the render context of the created sprite and set its transparent color.
            UIRoot.Instance.GraphicsPlatform.SpriteManager.CloseRenderContext(retSprite);
            retSprite.TransparentColor = this.spritePalette.TransparentColor;
            retSprite.Upload();
            return retSprite;
        }

        /// <summary>
        /// Combine the given FOW-flag into the given target render context from the given source sprite.
        /// </summary>
        /// <param name="flag">The FOW-flag to combine into the target render context.</param>
        /// <param name="spritePaletteImg">The image of the sprite palette.</param>
        /// <param name="targetRenderCtx">The target render context.</param>
        private void CombineFowFlag(FOWTileFlagsEnum flag, UISprite spritePaletteImg, IUIRenderContext targetRenderCtx)
        {
            int spriteIndex = this.spriteIndices[flag];
            RCIntRectangle sourceSection = this.spritePalette.GetSection(spriteIndex);
            RCIntVector sourceOffset = this.spritePalette.GetOffset(spriteIndex);
            targetRenderCtx.RenderSprite(spritePaletteImg, sourceOffset, sourceSection);
        }

        /// <summary>
        /// The sprite palette that contains the sprites for drawing the Fog Of War.
        /// </summary>
        private readonly ISpritePalette<FOWTypeEnum> spritePalette;

        /// <summary>
        /// The type of the Fog Of War stored in this sprite-group.
        /// </summary>
        private readonly FOWTypeEnum fowType;

        /// <summary>
        /// The indices of the sprites in the sprite palette mapped by the corresponding FOW-flags.
        /// </summary>
        private readonly Dictionary<FOWTileFlagsEnum, int> spriteIndices;

        /// <summary>
        /// The names of the sprites in the sprite palette mapped by the corresponding FOW-flags.
        /// </summary>
        private static readonly Dictionary<FOWTileFlagsEnum, string> SPRITE_NAMES = new Dictionary<FOWTileFlagsEnum, string>
        {
            { FOWTileFlagsEnum.North, "Side_North" },
            { FOWTileFlagsEnum.NorthEast, "Corner_NorthEast" },
            { FOWTileFlagsEnum.East, "Side_East" },
            { FOWTileFlagsEnum.SouthEast, "Corner_SouthEast" },
            { FOWTileFlagsEnum.South, "Side_South" },
            { FOWTileFlagsEnum.SouthWest, "Corner_SouthWest" },
            { FOWTileFlagsEnum.West, "Side_West" },
            { FOWTileFlagsEnum.NorthWest, "Corner_NorthWest" },
            { FOWTileFlagsEnum.Current, "Filled" }
        };
    }
}
