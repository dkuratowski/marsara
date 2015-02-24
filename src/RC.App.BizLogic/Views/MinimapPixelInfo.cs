using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.App.BizLogic.Views
{
    /// <summary>
    /// Contains informations for rendering a minimap pixel.
    /// </summary>
    public struct MinimapPixelInfo
    {
        /// <summary>
        /// Enumerates the possible render types of a minimap pixel.
        /// </summary>
        public enum EntityIndicatorTypeEnum
        {
            None = 0,
            Friendly = 1,
            AttackedFriendly = 2,
            NonFriendly = 3
        }

        /// <summary>
        /// The FOW-status at this minimap pixel.
        /// </summary>
        public FOWTypeEnum FOWStatus;

        /// <summary>
        /// The type of the entity indicator at this pixel.
        /// </summary>
        public EntityIndicatorTypeEnum EntityIndicatorType;

        /// <summary>
        /// The owner player of the entity indicated at this minimap pixel if EntityIndicatorType != EntityIndicatorTypeEnum.None,
        /// otherwise ignored.
        /// </summary>
        public PlayerEnum EntityOwner;

        /// <summary>
        /// The coordinates of this pixel on the minimap image.
        /// </summary>
        public RCIntVector PixelCoords;
    }
}
