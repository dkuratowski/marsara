using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Maps.Core
{
    /// <summary>
    /// Represents a tile type in a tileset. A tile can be simple or mixed. In case of simple tiles
    /// TerrainA refers to the corresponding terrain type, TerrainB must be null and Combination must
    /// be TerrainCombination.Simple. In case of mixed tiles TerrainA refers to the first, TerrainB refers
    /// to the second terrain type and Combination must indicate the combination of these terrain types.
    /// </summary>
    class IsoTileType : IIsoTileType
    {
        /// <summary>
        /// Constructs a simple tile type for the given terrain type.
        /// </summary>
        /// <param name="terrainType">The name of the terrain type.</param>
        /// <param name="tileset">The tileset of the tile type.</param>
        public IsoTileType(string terrainType, TileSet tileset)
        {
            if (terrainType == null) { throw new ArgumentNullException("terrainType"); }
            if (tileset == null) { throw new ArgumentNullException("tileset"); }

            this.tileset = tileset;
            this.terrainA = this.tileset.GetTerrainTypeImpl(terrainType);
            this.terrainB = null;
            this.combination = TerrainCombination.Simple;
            this.tmpCurrentBranch = null;
            this.defaultBranch = null;
            this.variants = new List<Tuple<List<IsoTileVariant>, IIsoTileCondition>>();
        }

        /// <summary>
        /// Constructs a mixed tile type for the given terrain types and combination.
        /// </summary>
        /// <param name="terrainTypeA">The name of the first terrain type.</param>
        /// <param name="terrainTypeB">The name of the second terrain type.</param>
        /// <param name="tileset">The tileset of the tile type.</param>
        /// <remarks>Terrain type A must be the parent of terrain type B.</remarks>
        public IsoTileType(string terrainTypeA, string terrainTypeB, TerrainCombination combination, TileSet tileset)
        {
            if (terrainTypeA == null) { throw new ArgumentNullException("terrainTypeA"); }
            if (terrainTypeB == null) { throw new ArgumentNullException("terrainTypeB"); }
            if (tileset == null) { throw new ArgumentNullException("tileset"); }
            if (combination == TerrainCombination.Simple) { throw new ArgumentException("combination", "Invalid combination for a mixed tile type!"); }

            this.tileset = tileset;
            TerrainType tA = this.tileset.GetTerrainTypeImpl(terrainTypeA);
            TerrainType tB = this.tileset.GetTerrainTypeImpl(terrainTypeB);
            if (tB.Parent != tA) { throw new ArgumentException(string.Format("TerrainType '{0}' must be the parent of TerrainType '{1}'!", terrainTypeA, terrainTypeB)); }

            this.terrainA = tA;
            this.terrainB = tB;
            this.combination = combination;
            this.tmpCurrentBranch = null;
            this.defaultBranch = null;
            this.variants = new List<Tuple<List<IsoTileVariant>, IIsoTileCondition>>();
        }

        #region IIsoTileType methods
        
        /// <see cref="IIsoTileType.TerrainA"/>
        public ITerrainType TerrainA { get { return this.terrainA; } }

        /// <see cref="IIsoTileType.TerrainB"/>
        public ITerrainType TerrainB { get { return this.terrainB; } }

        /// <see cref="IIsoTileType.Combination"/>
        public TerrainCombination Combination { get { return this.combination; } }

        /// <see cref="IIsoTileType.GetNumOfVariants"/>
        public int GetNumOfVariants(IIsoTile isoTile)
        {
            if (isoTile == null) { throw new ArgumentNullException("isoTile"); }

            foreach (Tuple<List<IsoTileVariant>, IIsoTileCondition> branch in this.variants)
            {
                if (branch.Item2 == null || branch.Item2.Check(isoTile))
                {
                    return branch.Item1.Count;
                }
            }

            throw new MapException(string.Format("No matching conditional branch found for isometric tile at {0}!", isoTile.MapCoords));
        }

        /// <see cref="IIsoTileType.GetVariant"/>
        public IIsoTileVariant GetVariant(IIsoTile isoTile, int variantIdx)
        {
            if (isoTile == null) { throw new ArgumentNullException("isoTile"); }
            if (variantIdx < 0) { throw new ArgumentOutOfRangeException("variantIdx", "Variant index must be non-negative!"); }

            foreach (Tuple<List<IsoTileVariant>, IIsoTileCondition> branch in this.variants)
            {
                if (branch.Item2 == null || branch.Item2.Check(isoTile))
                {
                    if (variantIdx >= branch.Item1.Count) { throw new ArgumentOutOfRangeException("variantIdx", string.Format("Variant with index {0} doesn't exists for isometric tile at {1}!", variantIdx, isoTile.MapCoords)); }
                    return branch.Item1[variantIdx];
                }
            }

            throw new MapException(string.Format("No matching conditional branch found for isometric tile at {0}!", isoTile.MapCoords));
        }

        #endregion IIsoTileType methods

        #region TileVariant defining methods

        /// <summary>
        /// Adds a TileVariant to the list that corresponds to the current conditional branch or starts
        /// defining the default branch if there is no conditional branch in progress.
        /// </summary>
        /// <param name="variant">The new TileVariant to add to the branch.</param>
        public void AddVariant(IsoTileVariant variant)
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
                    new Tuple<List<IsoTileVariant>, IIsoTileCondition>(new List<IsoTileVariant>() { variant }, null);
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
        public void BeginConditionalBranch(IIsoTileCondition condition)
        {
            if (this.tileset.IsFinalized) { throw new InvalidOperationException("TileSet already finalized!"); }
            if (condition == null) { throw new ArgumentNullException("condition"); }
            if (this.defaultBranch != null) { throw new InvalidOperationException("Defining the default branch is in progress!"); }
            if (this.tmpCurrentBranch != null) { throw new InvalidOperationException("Call TileType.EndConditionalBranch before starting another conditional branch!"); }

            /// Start a new conditional branch
            this.tmpCurrentBranch = new Tuple<List<IsoTileVariant>, IIsoTileCondition>(new List<IsoTileVariant>(), condition);
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

        /// <summary>
        /// Check and finalize the TileType object. Buildup methods will be unavailable after calling this method.
        /// </summary>
        public void CheckAndFinalize()
        {
            if (tmpCurrentBranch != null) { throw new TileSetException("Conditional branch not closed!"); }
            if (defaultBranch == null) { throw new TileSetException("Default branch not defined!"); }

            foreach (Tuple<List<IsoTileVariant>, IIsoTileCondition> item in this.variants)
            {
                foreach (IsoTileVariant variant in item.Item1)
                {
                    variant.CheckAndFinalize();
                }
            }
        }

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
        private List<Tuple<List<IsoTileVariant>, IIsoTileCondition>> variants;

        /// <summary>
        /// Reference to the currently defined conditional branch. This is a temporary member.
        /// </summary>
        private Tuple<List<IsoTileVariant>, IIsoTileCondition> tmpCurrentBranch;

        /// <summary>
        /// Reference to the default branch.
        /// </summary>
        private Tuple<List<IsoTileVariant>, IIsoTileCondition> defaultBranch;

        /// <summary>
        /// Reference to the tileset of this tile type.
        /// </summary>
        private TileSet tileset;
    }
}
