using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.App.BizLogic.Views
{
    /// <summary>
    /// Interface of views on the objects of the map of the currently running game.
    /// </summary>
    public interface IMapObjectView
    {
        /// <summary>
        /// Gets the list of the visible map object sprites to be rendered.
        /// </summary>
        /// <returns>
        /// A list of pairs of render informations of the visible map object sprites and their owner player.</returns>
        List<Tuple<SpriteRenderInfo, PlayerEnum>> GetVisibleMapObjectSprites();

        /// <summary>
        /// Gets the ID of the map object at the given position inside the displayed area.
        /// The term "display coordinates" means the same as in the description of the SpriteInst.DisplayCoords
        /// property.
        /// </summary>
        /// <param name="position">The position inside the map displayed area in pixels.</param>
        /// <returns>
        /// The ID of the map object at the given position or -1 if there is no map object at that position.
        /// </returns>
        int GetMapObjectID(RCIntVector position);
    }
}
