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
    public interface IFogOfWarView : IMapView
    {
        /// <summary>
        /// Gets the list of the partial Fog Of War tiles to update at the given area.
        /// </summary>
        /// <param name="displayedArea">The area of the map to be displayed in pixels.</param>
        /// <returns>The list of display informations of the partial Fog Of War tiles to update.</returns>
        List<SpriteInst> GetPartialFOWTiles(RCIntRectangle displayedArea);

        /// <summary>
        /// Gets the list of the full Fog Of War tiles to update at the given area.
        /// </summary>
        /// <param name="displayedArea">The area of the map to be displayed in pixels.</param>
        /// <returns>The list of display informations of the full Fog Of War tiles to update.</returns>
        List<SpriteInst> GetFullFOWTiles(RCIntRectangle displayedArea);
    }
}
