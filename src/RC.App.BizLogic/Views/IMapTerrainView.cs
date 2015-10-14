using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.App.BizLogic.Views
{
    /// <summary>
    /// Interface of views on the terrain of the currently opened map.
    /// </summary>
    public interface IMapTerrainView
    {
        /// <summary>
        /// Gets the list of the visible terrain sprites to be rendered.
        /// </summary>
        /// <returns>The list of render informations of the visible terrain sprites.</returns>
        List<SpriteRenderInfo> GetVisibleTerrainSprites();

        /// <summary>
        /// Gets the display coordinates of the isometric tile at the given position inside the displayed area.
        /// The term "display coordinates" means the same as in the description of the SpriteInst.DisplayCoords
        /// property.
        /// </summary>
        /// <param name="position">The position inside the map displayed area in pixels.</param>
        /// <returns>The display coordinates of the isometric tile at the given position.</returns>
        RCIntVector GetIsoTileDisplayCoords(RCIntVector position);

        /// <summary>
        /// Gets the display coordinates of the terrain object at the given position inside the displayed area.
        /// The term "display coordinates" means the same as in the description of the SpriteInst.DisplayCoords
        /// property.
        /// </summary>
        /// <param name="position">The position inside the map displayed area in pixels.</param>
        /// <returns>
        /// The display coordinates of the terrain object at the given position or RCIntVector.Undefined if there is no
        /// terrain object at that position.
        /// </returns>
        RCIntVector GetTerrainObjectDisplayCoords(RCIntVector position);

        /// <summary>
        /// Gets the list of the walkable cells at the displayed area.
        /// </summary>
        /// <returns>The list of the walkable cells.</returns>
        List<RCIntRectangle> GetWalkableCells();
    }
}
