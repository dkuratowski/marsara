using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.Views
{
    /// <summary>
    /// Enumerates the possible sprite groups that can be loaded.
    /// </summary>
    public enum SpriteGroupEnum
    {
        IsoTileSpriteGroup = 0,
        TerrainObjectSpriteGroup = 1,
        MapObjectSpriteGroup = 2,
        PartialFogOfWarSpriteGroup = 3,
        FullFogOfWarSpriteGroup = 4,
        CommandButtonSpriteGroup = 5,
        HPIconSpriteGroup = 6,
        MapObjectShadowSpriteGroup = 7
    }
}
