using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.App.BizLogic.Views
{
    /// <summary>
    /// Interface of views on the Fog Of War of the currently opened map.
    /// </summary>
    public interface IFogOfWarView
    {
        /// <summary>
        /// Gets the list of the Fog Of War tiles to update at the displayed area.
        /// </summary>
        /// <returns>The list of render informations of the Fog Of War tiles to update.</returns>
        List<SpriteRenderInfo> GetFOWTiles();
    }
}
