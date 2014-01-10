using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.PublicInterfaces;
using RC.UI;
using RC.App.PresLogic.Controls;

namespace RC.App.PresLogic
{
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
            foreach (SpriteDef terrainObjectType in this.tilesetView.GetTerrainObjectTypes())
            {
                UISprite terrainObject = UIRoot.Instance.GraphicsPlatform.SpriteManager.LoadSprite(terrainObjectType.ImageData, UIWorkspace.Instance.PixelScaling);
                terrainObject.TransparentColor = terrainObjectType.TransparentColorStr != null ?
                                        UIResourceLoader.LoadColor(terrainObjectType.TransparentColorStr) :
                                        RCMapDisplayBasic.DEFAULT_TILE_TRANSPARENT_COLOR;
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
