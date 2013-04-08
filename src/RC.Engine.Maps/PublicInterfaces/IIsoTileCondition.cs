using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Maps.PublicInterfaces
{
    /// <summary>
    /// Interface of conditions to select the correct tile variant for an isometric tile.
    /// </summary>
    public interface IIsoTileCondition
    {
        /// <summary>
        /// Checks whether the condition is satisfied at the given IsoTile or not.
        /// </summary>
        /// <param name="isoTile">The isometric tile to check.</param>
        /// <returns>True if the condition is satisfied, false otherwise.</returns>
        bool Check(IIsoTile isoTile);

        /// <summary>
        /// Gets the tileset that this condition belongs to.
        /// </summary>
        ITileSet Tileset { get; }
    }
}
