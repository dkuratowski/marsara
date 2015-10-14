using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.Views;
using RC.Common;
using RC.UI;

namespace RC.App.PresLogic.SpriteGroups
{
    /// <summary>
    /// This sprite group loads the HP indicator icons of the map objects to be displayed on the details panel.
    /// </summary>
    class HPIconSpriteGroup : MaskedSpriteGroup
    {
        /// <summary>
        /// Constructs a HPIconSpriteGroup instance.
        /// </summary>
        /// <param name="metadataView">Reference to the metadata view.</param>
        /// <param name="condition">The condition for which this sprite group is created.</param>
        public HPIconSpriteGroup(IMetadataView metadataView, MapObjectConditionEnum condition)
        {
            if (metadataView == null) { throw new ArgumentNullException("metadataView"); }

            this.metadataView = metadataView;
            this.condition = condition;
        }

        #region Overriden from MaskedSpriteGroup

        /// <see cref="MaskedSpriteGroup.SpriteDefinitions"/>
        protected override IEnumerable<SpriteData> SpriteDefinitions { get { return this.metadataView.GetMapObjectHPIconData(); } }

        /// <see cref="MaskedSpriteGroup.IsMaskingForced"/>
        protected override bool IsMaskingForced { get { return this.condition == MapObjectConditionEnum.Undefined; } }

        /// <see cref="MaskedSpriteGroup.TargetColor"/>
        protected override RCColor TargetColor { get { return HPICON_COLOR_MAPPINGS[this.condition]; } }

        #endregion Overriden from MaskedSpriteGroup

        /// <summary>
        /// Reference to a view on the metadata of the game engine.
        /// </summary>
        private readonly IMetadataView metadataView;

        /// <summary>
        /// The condition for which this sprite group is created.
        /// </summary>
        private readonly MapObjectConditionEnum condition;

        /// <summary>
        /// Defines the colors for the different conditions.
        /// </summary>
        private static readonly Dictionary<MapObjectConditionEnum, RCColor> HPICON_COLOR_MAPPINGS = new Dictionary<MapObjectConditionEnum, RCColor>()
        {
            { MapObjectConditionEnum.Undefined, RCColor.Black },
            { MapObjectConditionEnum.Excellent, RCColor.LightGreen },
            { MapObjectConditionEnum.Moderate, RCColor.Yellow },
            { MapObjectConditionEnum.Critical, RCColor.Red },
        };
    }
}
