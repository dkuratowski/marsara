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
    /// The sprite-group for drawing the shadows of flying map objects.
    /// </summary>
    class ShadowSpriteGroup : SpriteGroup
    {
        /// <summary>
        /// Constructs a ShadowSpriteGroup instance from the given sprite palette.
        /// </summary>
        /// <param name="spritePalette">The sprite palette that contains the sprites for drawing the shadows.</param>
        public ShadowSpriteGroup(IMetadataView metadataView)
        {
            if (metadataView == null) { throw new ArgumentNullException("metadataView"); }

            this.metadataView = metadataView;
        }

        /// <see cref="SpriteGroup.Load_i"/>
        protected override List<UISprite> Load_i()
        {
            List<UISprite> retList = new List<UISprite>();
            foreach (SpriteData shadowSpriteData in this.metadataView.GetShadowSpriteData())
            {
                UISprite shadowSprite = UIRoot.Instance.GraphicsPlatform.SpriteManager.LoadSprite(shadowSpriteData.ImageData, UIWorkspace.Instance.PixelScaling);
                shadowSprite.TransparentColor = shadowSpriteData.TransparentColor != RCColor.Undefined ? shadowSpriteData.TransparentColor : PresLogicConstants.DEFAULT_TRANSPARENT_COLOR;
                shadowSprite.Upload();
                retList.Add(shadowSprite);
            }
            return retList;
        }

        /// <summary>
        /// Reference to the metadata view.
        /// </summary>
        private readonly IMetadataView metadataView;
    }
}
