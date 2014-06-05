using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.PublicInterfaces;
using RC.Common;
using RC.UI;

namespace RC.App.PresLogic
{
    /// <summary>
    /// Base class of SpriteGroups in which the sprites are masked to a target color.
    /// </summary>
    abstract class MaskedSpriteGroup : SpriteGroup
    {
        /// <summary>
        /// Protected ctor.
        /// </summary>
        protected MaskedSpriteGroup() : base() { }

        #region Overriden methods from SpriteGroup

        /// <see cref="SpriteGroup.Load_i"/>
        protected override List<UISprite> Load_i()
        {
            List<UISprite> retList = new List<UISprite>();
            foreach (SpriteDef spriteDef in this.SpriteDefinitions)
            {
                if (this.IsMaskingForced || spriteDef.IsMaskableSprite)
                {
                    UISprite origSprite = UIRoot.Instance.GraphicsPlatform.SpriteManager.LoadSprite(
                        spriteDef.ImageData,
                        UIWorkspace.Instance.PixelScaling);
                    origSprite.TransparentColor = spriteDef.MaskColorStr != null ?
                                            UIResourceLoader.LoadColor(spriteDef.MaskColorStr) :
                                            DEFAULT_MASK_COLOR;
                    UISprite maskedSprite = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(
                        this.TargetColor,
                        origSprite.Size,
                        origSprite.PixelSize);
                    IUIRenderContext ctx =
                        UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateRenderContext(maskedSprite);
                    ctx.RenderSprite(origSprite, new RCIntVector(0, 0));
                    UIRoot.Instance.GraphicsPlatform.SpriteManager.CloseRenderContext(maskedSprite);
                    UIRoot.Instance.GraphicsPlatform.SpriteManager.DestroySprite(origSprite);
                    maskedSprite.TransparentColor = spriteDef.TransparentColorStr != null ?
                                            UIResourceLoader.LoadColor(spriteDef.TransparentColorStr) :
                                            DEFAULT_TRANSPARENT_COLOR;
                    maskedSprite.Upload();
                    retList.Add(maskedSprite);
                }
                else
                {
                    /// No masked sprite shall be created at the given index...
                    retList.Add(null);
                }
            }
            return retList;
        }
        
        #endregion Overriden methods from SpriteGroup

        #region Overridable methods

        /// <summary>
        /// Gets the sprite definitions to load.
        /// </summary>
        protected abstract IEnumerable<SpriteDef> SpriteDefinitions { get; }

        /// <summary>
        /// Gets whether masking shall be forced even in case of non-maskable sprite definitions.
        /// </summary>
        protected virtual bool IsMaskingForced { get { return false; } }

        /// <summary>
        /// The target color of the masked pixels.
        /// </summary>
        protected abstract UIColor TargetColor { get; }

        #endregion Overridable methods

        /// <summary>
        /// The default color of the transparent parts of the sprites.
        /// </summary>
        private static readonly UIColor DEFAULT_TRANSPARENT_COLOR = new UIColor(255, 0, 255);

        /// <summary>
        /// The default mask color of the sprites.
        /// </summary>
        private static readonly UIColor DEFAULT_MASK_COLOR = new UIColor(0, 255, 255);
    }
}
