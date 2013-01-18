using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine
{
    /// <summary>
    /// Enumerates the possible terrain combination of mixed tiles. Mixed tiles can be placed at the
    /// transition of two terrain types. The letters in these values indicates that which part of the
    /// mixed tile has which terrain type of the transition (A or B). The tiles have four parts identified
    /// by the corresponding direction (north, east, south, west). The letters in the values are in that
    /// order.
    /// For example: assume that we have 'dirt' and 'grass' as two terrain types (A and B respectively)
    /// and we are looking for mixed tiles that can be placed at dirt-grass transitions. In this case
    /// TerrainCombination.ABBA indicates a mixed tile that has dirt at north and west and grass at east and
    /// south.
    /// Use TerrainCombination.Simple for simple tiles.
    /// </summary>
    public enum TerrainCombination
    {
        [EnumMapping("Simple")]
        Simple = 0x0, /// Simple tile of a terrain type

        [EnumMapping("AAAB")]
        AAAB = 0x1, /// North-A, East-A, South-A, West-B

        [EnumMapping("AABA")]
        AABA = 0x2, /// North-A, East-A, South-B, West-A

        [EnumMapping("AABB")]
        AABB = 0x3, /// North-A, East-A, South-B, West-B

        [EnumMapping("ABAA")]
        ABAA = 0x4, /// North-A, East-B, South-A, West-A

        [EnumMapping("ABAB")]
        ABAB = 0x5, /// North-A, East-B, South-A, West-B

        [EnumMapping("ABBA")]
        ABBA = 0x6, /// North-A, East-B, South-B, West-A

        [EnumMapping("ABBB")]
        ABBB = 0x7, /// North-A, East-B, South-B, West-B

        [EnumMapping("BAAA")]
        BAAA = 0x8, /// North-B, East-A, South-A, West-A

        [EnumMapping("BAAB")]
        BAAB = 0x9, /// North-B, East-A, South-A, West-B

        [EnumMapping("BABA")]
        BABA = 0xA, /// North-B, East-A, South-B, West-A

        [EnumMapping("BABB")]
        BABB = 0xB, /// North-B, East-A, South-B, West-B

        [EnumMapping("BBAA")]
        BBAA = 0xC, /// North-B, East-B, South-A, West-A

        [EnumMapping("BBAB")]
        BBAB = 0xD, /// North-B, East-B, South-A, West-B

        [EnumMapping("BBBA")]
        BBBA = 0xE, /// North-B, East-B, South-B, West-A
    }

    /// <summary>
    /// Represents a tile type in a tileset. A tile can be simple or mixed. In case of simple tiles
    /// TerrainA refers to the corresponding terrain type, TerrainB must be null and Combination must
    /// be TerrainCombination.Simple. In case of mixed tiles TerrainA refers to the first, TerrainB refers
    /// to the second terrain type and Combination must indicate the combination of these terrain types.
    /// </summary>
    public class TileType
    {
        /// <summary>
        /// Constructs a simple tile type for the given terrain type.
        /// </summary>
        /// <param name="terrainType">The name of the terrain type.</param>
        /// <param name="tileset">The tileset of the tile type.</param>
        public TileType(string terrainType, TileSet tileset)
        {
            if (terrainType == null) { throw new ArgumentNullException("terrainType"); }
            if (tileset == null) { throw new ArgumentNullException("tileset"); }

            this.tileset = tileset;
            this.terrainA = this.tileset.GetTerrainType(terrainType);
            this.terrainB = null;
            this.combination = TerrainCombination.Simple;
            this.tmpCurrentBranch = null;
            this.defaultBranch = null;
            this.variants = new List<Tuple<List<TileVariant>, ITileCondition>>();
        }

        /// <summary>
        /// Constructs a mixed tile type for the given terrain types and combination.
        /// </summary>
        /// <param name="terrainTypeA">The name of the first terrain type.</param>
        /// <param name="terrainTypeB">The name of the second terrain type.</param>
        /// <param name="tileset">The tileset of the tile type.</param>
        /// <remarks>Terrain type A must be the parent of terrain type B.</remarks>
        public TileType(string terrainTypeA, string terrainTypeB, TerrainCombination combination, TileSet tileset)
        {
            if (terrainTypeA == null) { throw new ArgumentNullException("terrainTypeA"); }
            if (terrainTypeB == null) { throw new ArgumentNullException("terrainTypeB"); }
            if (tileset == null) { throw new ArgumentNullException("tileset"); }
            if (combination == TerrainCombination.Simple) { throw new ArgumentException("combination", "Invalid combination for a mixed tile type!"); }

            this.tileset = tileset;
            TerrainType tA = this.tileset.GetTerrainType(terrainTypeA);
            TerrainType tB = this.tileset.GetTerrainType(terrainTypeB);
            if (tB.Parent != tA) { throw new ArgumentException(string.Format("TerrainType '{0}' must be the parent of TerrainType '{1}'!", terrainTypeA, terrainTypeB)); }

            this.terrainA = tA;
            this.terrainB = tB;
            this.combination = combination;
            this.tmpCurrentBranch = null;
            this.defaultBranch = null;
            this.variants = new List<Tuple<List<TileVariant>, ITileCondition>>();
        }

        #region TileVariant defining methods

        /// <summary>
        /// Adds a TileVariant to the list that corresponds to the current conditional branch or starts
        /// defining the default branch if there is no conditional branch in progress.
        /// </summary>
        /// <param name="variant">The new TileVariant to add to the branch.</param>
        public void AddVariant(TileVariant variant)
        {
            if (this.tileset.IsFinalized) { throw new InvalidOperationException("TileSet already finalized!"); }
            if (variant == null) { throw new ArgumentNullException("variant"); }
            if (variant.Tileset != this.tileset) { throw new TileSetException("The given TileVariant is in another TileSet!"); }

            if (this.defaultBranch != null && this.tmpCurrentBranch == null)
            {
                /// Add the variant to the default branch.
                this.defaultBranch.Item1.Add(variant);
            }
            else if (this.defaultBranch == null && this.tmpCurrentBranch != null)
            {
                /// Add the variant to the currently defined conditional branch.
                this.tmpCurrentBranch.Item1.Add(variant);
            }
            else if (this.defaultBranch == null && this.tmpCurrentBranch == null)
            {
                /// Start the default branch.
                this.defaultBranch =
                    new Tuple<List<TileVariant>, ITileCondition>(new List<TileVariant>() { variant }, null);
                this.variants.Add(this.defaultBranch);
            }
            else
            {
                throw new InvalidOperationException("Unexpected state!");
            }
        }

        /// <summary>
        /// Begins a new conditional branch.
        /// </summary>
        /// <param name="condition">The condition.</param>
        public void BeginConditionalBranch(ITileCondition condition)
        {
            if (this.tileset.IsFinalized) { throw new InvalidOperationException("TileSet already finalized!"); }
            if (condition == null) { throw new ArgumentNullException("condition"); }
            if (condition.Tileset != this.tileset) { throw new TileSetException("The given ITileCondition is in another TileSet!"); }
            if (this.defaultBranch != null) { throw new InvalidOperationException("Defining the default branch is in progress!"); }
            if (this.tmpCurrentBranch != null) { throw new InvalidOperationException("Call TileType.EndConditionalBranch before starting another conditional branch!"); }

            /// Start a new conditional branch
            this.tmpCurrentBranch = new Tuple<List<TileVariant>, ITileCondition>(new List<TileVariant>(), condition);
            this.variants.Add(this.tmpCurrentBranch);
        }

        /// <summary>
        /// Finish the current conditional branch.
        /// </summary>
        public void EndConditionalBranch()
        {
            if (this.tileset.IsFinalized) { throw new InvalidOperationException("TileSet already finalized!"); }
            if (this.defaultBranch != null) { throw new InvalidOperationException("Defining the default branch is in progress!"); }
            if (this.tmpCurrentBranch == null) { throw new InvalidOperationException("Defining a conditional branch is currently not in progress!"); }
            if (this.tmpCurrentBranch.Item1.Count == 0) { throw new InvalidOperationException("Conditional branch must define at least 1 TileVariant!"); }

            this.tmpCurrentBranch = null;
        }

        #endregion TileVariant defining methods

        #region TileVariant query methods

        /// <summary>
        /// Gets the number of tile variants available for the given isometric tile.
        /// </summary>
        /// <param name="isoTile">The isometric tile to get the number of available tile variants for.</param>
        /// <returns>The number of tile variants available for the given isometric tile.</returns>
        public int GetNumOfVariants(IIsoTile isoTile)
        {
            if (isoTile == null) { throw new ArgumentNullException("isoTile"); }

            foreach (Tuple<List<TileVariant>, ITileCondition> branch in this.variants)
            {
                if (branch.Item2 == null || branch.Item2.Check(isoTile))
                {
                    return branch.Item1.Count;
                }
            }

            throw new RCEngineException(string.Format("No matching conditional branch found for isometric tile at {0}!", isoTile.MapCoords));
        }

        /// <summary>
        /// Gets the given variant for the given isometric tile.
        /// </summary>
        /// <param name="isoTile">The isometric tile to get the variant for.</param>
        /// <param name="variantIdx">The index of the variant to get.</param>
        /// <returns>The given variant for the given isometric tile.</returns>
        public TileVariant GetVariant(IIsoTile isoTile, int variantIdx)
        {
            if (isoTile == null) { throw new ArgumentNullException("isoTile"); }
            if (variantIdx < 0) { throw new ArgumentOutOfRangeException("variantIdx", "Variant index must be non-negative!"); }
            
            foreach (Tuple<List<TileVariant>, ITileCondition> branch in this.variants)
            {
                if (branch.Item2 == null || branch.Item2.Check(isoTile))
                {
                    if (variantIdx >= branch.Item1.Count) { throw new ArgumentOutOfRangeException("variantIdx", string.Format("Variant with index {0} doesn't exists for isometric tile at {1}!", variantIdx, isoTile.MapCoords)); }
                    return branch.Item1[variantIdx];
                }
            }

            throw new RCEngineException(string.Format("No matching conditional branch found for isometric tile at {0}!", isoTile.MapCoords));
        }

        #endregion TileVariant query methods

        /// <summary>
        /// Check and finalize the TileType object. Buildup methods will be unavailable after calling this method.
        /// </summary>
        public void CheckAndFinalize()
        {
            if (tmpCurrentBranch != null) { throw new TileSetException("Conditional branch not closed!"); }
            if (defaultBranch == null) { throw new TileSetException("Default branch not defined!"); }

            foreach (Tuple<List<TileVariant>, ITileCondition> item in this.variants)
            {
                foreach (TileVariant variant in item.Item1)
                {
                    variant.CheckAndFinalize();
                }
            }
        }

        /// <summary>
        /// Gets the reference to the first terrain type. In case of mixed tiles, TerrainA is the parent of TerrainB in the terrain-tree.
        /// </summary>
        public TerrainType TerrainA { get { return this.terrainA; } }

        /// <summary>
        /// Gets the reference to the second terrain type in case of mixed tiles or null in case of simple tiles.
        /// In case of mixed tiles, TerrainB is a child of TerrainA in the terrain-tree.
        /// </summary>
        public TerrainType TerrainB { get { return this.terrainB; } }

        /// <summary>
        /// Gets the combination of terrain type A and B in case of mixed tiles or TerrainCombination.Simple in
        /// case of simple tiles.
        /// </summary>
        public TerrainCombination Combination { get { return this.combination; } }

        /// <summary>
        /// Reference to the first terrain type.
        /// </summary>
        private TerrainType terrainA;

        /// <summary>
        /// Reference to the second terrain type in case of mixed tiles or null in case of simple tiles.
        /// </summary>
        private TerrainType terrainB;

        /// <summary>
        /// The combination of terrain type A and B in case of mixed tiles or TerrainCombination.Simple in
        /// case of simple tiles.
        /// </summary>
        private TerrainCombination combination;

        /// <summary>
        /// List of the tile variants grouped by the corresponding conditions.
        /// </summary>
        private List<Tuple<List<TileVariant>, ITileCondition>> variants;

        /// <summary>
        /// Reference to the currently defined conditional branch. This is a temporary member.
        /// </summary>
        private Tuple<List<TileVariant>, ITileCondition> tmpCurrentBranch;

        /// <summary>
        /// Reference to the default branch.
        /// </summary>
        private Tuple<List<TileVariant>, ITileCondition> defaultBranch;

        /// <summary>
        /// Reference to the tileset of this tile type.
        /// </summary>
        private TileSet tileset;
    }
}
