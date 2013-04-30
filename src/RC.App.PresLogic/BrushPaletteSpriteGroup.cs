using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.UI;
using RC.Common;

namespace RC.App.PresLogic
{
    class BrushPaletteSpriteGroup : SpriteGroup
    {
        /// <summary>
        /// Constructs a BrushPaletteSpriteGroup instance.
        /// </summary>
        public BrushPaletteSpriteGroup()
            : base()
        {
        }

        /// <see cref="SpriteGroup.Load_i"/>
        protected override List<UISprite> Load_i()
        {
            /// TODO: This is a hardcoded implementation. The colors of the brush palette will come from the backend.
            List<UISprite> retList = new List<UISprite>();
            UISprite lightGreenBrush = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(UIColor.LightGreen, new RCIntVector(1, 1), UIWorkspace.Instance.PixelScaling);
            UISprite yellowBrush = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(UIColor.Yellow, new RCIntVector(1, 1), UIWorkspace.Instance.PixelScaling);
            UISprite redBrush = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(UIColor.Red, new RCIntVector(1, 1), UIWorkspace.Instance.PixelScaling);
            UISprite greenBrush = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(UIColor.Green, new RCIntVector(1, 1), UIWorkspace.Instance.PixelScaling);
            retList.Add(lightGreenBrush);
            retList.Add(yellowBrush);
            retList.Add(redBrush);
            retList.Add(greenBrush);
            foreach (UISprite brush in retList) { brush.Upload(); }
            return retList;
        }
    }
}
