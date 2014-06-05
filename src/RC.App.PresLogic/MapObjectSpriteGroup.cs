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
    /// <summary>
    /// This sprite group loads the sprites defined in the simulation metadata and masks them to a target color that
    /// corresponds to the given player.
    /// </summary>
    class MapObjectSpriteGroup : MaskedSpriteGroup
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

        #region Overriden from MaskedSpriteGroup

        /// <see cref="MaskedSpriteGroup.SpriteDefinitions"/>
        protected override IEnumerable<SpriteDef> SpriteDefinitions { get { return this.metadataView.GetMapObjectTypes(); } }

        /// <see cref="MaskedSpriteGroup.IsMaskingForced"/>
        protected override bool IsMaskingForced { get { return this.owner == PlayerEnum.Neutral; } }

        /// <see cref="MaskedSpriteGroup.TargetColor"/>
        protected override UIColor TargetColor { get { return PLAYER_COLOR_MAPPINGS[this.owner]; } }

        #endregion Overriden from MaskedSpriteGroup

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
