using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.UI;

namespace RC.App.PresLogic
{
    class MapObjectSpriteGroup : SpriteGroup
    {
        /// <summary>
        /// Constructs a MapObjectSpriteGroup instance.
        /// </summary>
        public MapObjectSpriteGroup()
            : base()
        {
        }

        /// <see cref="SpriteGroup.Load_i"/>
        protected override List<UISprite> Load_i()
        {
            List<UISprite> retList = new List<UISprite>();
            /// TODO: load the map object sprites from the backend.
            UISprite testMarineSprite = UIRoot.Instance.GraphicsPlatform.SpriteManager.LoadSprite(".\\sprites\\units\\marine_test.png", UIWorkspace.Instance.PixelScaling);
            testMarineSprite.TransparentColor = new UIColor(255, 0, 255);
            testMarineSprite.Upload();
            retList.Add(testMarineSprite);
            return retList;
        }
    }
}
