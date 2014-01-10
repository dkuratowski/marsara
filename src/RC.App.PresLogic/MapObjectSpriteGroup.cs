using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.UI;
using RC.App.BizLogic.PublicInterfaces;
using RC.App.PresLogic.Controls;
using RC.Common;

namespace RC.App.PresLogic
{
    class MapObjectSpriteGroup : SpriteGroup
    {
        /// <summary>
        /// Constructs a MapObjectSpriteGroup instance.
        /// </summary>
        /// <param name="metadataView">Reference to the metadata view.</param>
        /// <param name="owner">The owner of the map objects in this group.</param>
        public MapObjectSpriteGroup(IMetadataView metadataView, PlayerEnum owner)
            : base()
        {
            if (metadataView == null) { throw new ArgumentNullException("metadataView"); }

            this.metadataView = metadataView;
            this.owner = owner;
        }

        /// <see cref="SpriteGroup.Load_i"/>
        protected override List<UISprite> Load_i()
        {
            List<UISprite> retList = new List<UISprite>();
            foreach (SpriteDef objType in this.metadataView.GetMapObjectTypes())
            {
                if (this.owner == PlayerEnum.Neutral || objType.HasOwner)
                {
                    UISprite origSprite = UIRoot.Instance.GraphicsPlatform.SpriteManager.LoadSprite(
                        objType.ImageData,
                        UIWorkspace.Instance.PixelScaling);
                    origSprite.TransparentColor = objType.OwnerMaskColorStr != null ?
                                            UIResourceLoader.LoadColor(objType.OwnerMaskColorStr) :
                                            RCMapObjectDisplay.DEFAULT_MAPOBJECT_OWNERMASK_COLOR;
                    UISprite objSprite = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(
                        PLAYER_COLOR_MAPPINGS[this.owner],
                        origSprite.Size,
                        origSprite.PixelSize);
                    IUIRenderContext ctx =
                        UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateRenderContext(objSprite);
                    ctx.RenderSprite(origSprite, new RCIntVector(0, 0));
                    UIRoot.Instance.GraphicsPlatform.SpriteManager.CloseRenderContext(objSprite);
                    UIRoot.Instance.GraphicsPlatform.SpriteManager.DestroySprite(origSprite);
                    objSprite.TransparentColor = objType.TransparentColorStr != null ?
                                            UIResourceLoader.LoadColor(objType.TransparentColorStr) :
                                            RCMapObjectDisplay.DEFAULT_MAPOBJECT_TRANSPARENT_COLOR;
                    objSprite.Upload();
                    retList.Add(objSprite);
                }
                else
                {
                    /// No sprite for the current owner at the given index...
                    retList.Add(null);
                }
            }
            return retList;
        }

        /// <summary>
        /// Reference to a view on the metadata of the game engine.
        /// </summary>
        private IMetadataView metadataView;

        /// <summary>
        /// The owner of the map objects in this group.
        /// </summary>
        private PlayerEnum owner;

        /// <summary>
        /// Defines the colors of the players.
        /// </summary>
        private static readonly Dictionary<PlayerEnum, UIColor> PLAYER_COLOR_MAPPINGS = new Dictionary<PlayerEnum, UIColor>()
        {
            { PlayerEnum.Neutral, UIColor.Black },
            { PlayerEnum.Player0, UIColor.Red },
            { PlayerEnum.Player1, UIColor.Blue },
            { PlayerEnum.Player2, UIColor.Cyan },
            { PlayerEnum.Player3, UIColor.Magenta },
            { PlayerEnum.Player4, UIColor.LightMagenta },
            { PlayerEnum.Player5, UIColor.Green },
            { PlayerEnum.Player6, UIColor.WhiteHigh },
            { PlayerEnum.Player7, UIColor.Yellow }
        };
    }
}
