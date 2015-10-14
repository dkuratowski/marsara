using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.UI;
using RC.App.PresLogic.Controls;
using RC.App.BizLogic.Views;
using RC.Common;

namespace RC.App.PresLogic.SpriteGroups
{
    /// <summary>
    /// The sprite-group of the terrain objects.
    /// </summary>
    class TerrainObjectSpriteGroup : SpriteGroup
    {
        /// <summary>
        /// Constructs a TerrainObjectSpriteGroup instance.
        /// </summary>
        /// <param name="tilesetView">Reference to a view on the tileset.</param>
        public TerrainObjectSpriteGroup(ITileSetView tilesetView)
            : base()
        {
            if (tilesetView == null) { throw new ArgumentNullException("tilesetView"); }
            this.tilesetView = tilesetView;
        }

        /// <see cref="SpriteGroup.Load_i"/>
        protected override List<UISprite> Load_i()
        {
            List<UISprite> retList = new List<UISprite>();
            foreach (SpriteData terrainObjectType in this.tilesetView.GetTerrainObjectTypes())
            {
                UISprite terrainObject = UIRoot.Instance.GraphicsPlatform.SpriteManager.LoadSprite(terrainObjectType.ImageData, UIWorkspace.Instance.PixelScaling);
                terrainObject.TransparentColor = terrainObjectType.TransparentColor != RCColor.Undefined
                                               ? terrainObjectType.TransparentColor
                                               : PresLogicConstants.DEFAULT_TRANSPARENT_COLOR;
                terrainObject.Upload();
                retList.Add(terrainObject);
            }
            return retList;
        }

        /// <summary>
        /// Reference to a view on the tileset.
        /// </summary>
        private ITileSetView tilesetView;
    }
}
