using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.App.BizLogic.Views
{
    /// <summary>
    /// Interface definition of the minimap views.
    /// </summary>
    public interface IMinimapView
    {
        /// <summary>
        /// Gets the list of the sprites to render the isometric tiles of the full map.
        /// </summary>
        /// <returns>The list of display informations of the sprites.</returns>
        List<SpriteInst> GetIsoTileSprites();

        /// <summary>
        /// Gets the list of the sprites to render the terrain objects of the full map.
        /// </summary>
        /// <returns>The list of display informations of the sprites.</returns>
        List<SpriteInst> GetTerrainObjectSprites();

        /// <summary>
        /// Gets the location of the indicator of the current map window on the minimap display.
        /// </summary>
        RCIntRectangle WindowIndicator { get; }

        /// <summary>
        /// The position of the minimap inside the minimap control.
        /// </summary>
        RCIntRectangle MinimapPosition { get; }

        /// <summary>
        /// Gets the size of the full map in pixels.
        /// </summary>
        RCIntVector MapPixelSize { get; }
    }
}
