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
        public MapObjectSpriteGroup(IMetadataView metadataView, Player owner)
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
            foreach (MapSpriteType objType in this.metadataView.GetMapObjectTypes())
            {
                if (owner == Player.Neutral || objType.HasPlayer)
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
        private Player owner;

        /// <summary>
        /// Defines the colors of the players.
        /// </summary>
        private static readonly Dictionary<Player, UIColor> PLAYER_COLOR_MAPPINGS = new Dictionary<Player, UIColor>()
        {
            { Player.Neutral, UIColor.Black },
            { Player.Player1, UIColor.Red },
            { Player.Player2, UIColor.Blue },
            { Player.Player3, UIColor.Cyan },
            { Player.Player4, UIColor.Magenta },
            { Player.Player5, UIColor.LightMagenta },
            { Player.Player6, UIColor.Green },
            { Player.Player7, UIColor.WhiteHigh },
            { Player.Player8, UIColor.Yellow }
        };
    }
}
