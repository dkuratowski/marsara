using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Maps.PublicInterfaces
{
    /// <summary>
    /// Defines the interface of an isometric tile type definition.
    /// </summary>
    public interface IIsoTileType
    {
        /// <summary>
        /// Gets the reference to the first terrain type. In case of mixed tiles, TerrainA is the parent of TerrainB
        /// in the terrain-tree.
        /// </summary>
        ITerrainType TerrainA { get; }

        /// <summary>
        /// Gets the reference to the second terrain type in case of mixed tiles or null in case of simple tiles.
        /// In case of mixed tiles, TerrainB is a child of TerrainA in the terrain-tree.
        /// </summary>
        ITerrainType TerrainB { get; }

        /// <summary>
        /// Gets the combination of terrain type A and B in case of mixed tiles or TerrainCombination.Simple in
        /// case of simple tiles.
        /// </summary>
        TerrainCombination Combination { get; }

        /// <summary>
        /// Gets the number of tile variants available for the given isometric tile.
        /// </summary>
        /// <param name="isoTile">The isometric tile to get the number of available tile variants for.</param>
        /// <returns>The number of tile variants available for the given isometric tile.</returns>
        int GetNumOfVariants(IIsoTile isoTile);

        /// <summary>
        /// Gets the given variant for the given isometric tile.
        /// </summary>
        /// <param name="isoTile">The isometric tile to get the variant for.</param>
        /// <param name="variantIdx">The index of the variant to get.</param>
        /// <returns>The given variant for the given isometric tile.</returns>
        IIsoTileVariant GetVariant(IIsoTile isoTile, int variantIdx);
    }
}
