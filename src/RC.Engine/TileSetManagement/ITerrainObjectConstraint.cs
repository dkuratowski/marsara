using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine
{
    /// <summary>
    /// Interface for constraints of terrain objects.
    /// </summary>
    public interface ITerrainObjectConstraint
    {
        /// <summary>
        /// Checks whether this constraint allows placing the given terrain object to the given position.
        /// </summary>
        /// <param name="terrainObj">The terrain object to be placed.</param>
        /// <param name="targetPos">The target position of the top-left quadratic tile of the terrain object.</param>
        /// <returns>True if placing the terrain object is allowed by this constraint, false otherwise.</returns>
        bool Check(TerrainObjectType terrainObj, IQuadTile targetPos);

        /// <summary>
        /// Gets the tileset of this constraint.
        /// </summary>
        TileSet Tileset { get; }
    }

    /// <summary>
    /// Represents a constraint on the type of an isometric tile at a given position relative to the top-left corner of
    /// the terrain object it is applied to.
    /// </summary>
    public class TileConstraint : ITerrainObjectConstraint
    {
        /// <summary>
        /// Constructs a TileConstraint on a simple tile.
        /// </summary>
        /// <param name="quadCoords">
        /// The relative position (from the top-left corner) of the quadratic tile whose parent isometric tile
        /// has to be checked.
        /// </param>
        /// <param name="terrain">The terrain type that the checked isometric tile has to be.</param>
        /// <param name="tileset">The tileset tha this constraint belongs to.</param>
        public TileConstraint(RCIntVector quadCoords, TerrainType terrain, TileSet tileset)
        {
            if (quadCoords == RCIntVector.Undefined) { throw new ArgumentNullException("quadCoords"); }
            if (terrain == null) { throw new ArgumentNullException("terrain"); }
            if (tileset == null) { throw new ArgumentNullException("tileset"); }
            if (terrain.Tileset != tileset) { throw new InvalidOperationException("The given terrain type is in another tileset!"); }

            this.allowedCombinations = new List<TerrainCombination>() { TerrainCombination.Simple };
            this.tileset = tileset;
            this.terrainA = terrain;
            this.terrainB = null;
        }

        /// <summary>
        /// Constructs a TileConstraint on a mixed tile.
        /// </summary>
        /// <param name="quadCoords">
        /// The relative position (from the top-left corner) of the quadratic tile whose parent isometric tile
        /// has to be checked.
        /// </param>
        /// <param name="terrainA">The terrain type that has to be the first terrain type of the checked isometric tile.</param>
        /// <param name="terrainB">The terrain type that has to be the second terrain type of the checked isometric tile.</param>
        /// <param name="combinations">The allowed combinations of the checked isometric tile.</param>
        /// <param name="tileset">The tileset that this constraint belongs to.</param>
        /// <remarks>Terrain type A must be the parent of terrain type B.</remarks>
        public TileConstraint(RCIntVector quadCoords,
                              TerrainType terrainA,
                              TerrainType terrainB,
                              List<TerrainCombination> combinations,
                              TileSet tileset)
        {
            if (quadCoords == RCIntVector.Undefined) { throw new ArgumentNullException("quadCoords"); }
            if (terrainA == null) { throw new ArgumentNullException("terrainA"); }
            if (terrainB == null) { throw new ArgumentNullException("terrainB"); }
            if (combinations == null || combinations.Count == 0) { throw new ArgumentNullException("combinations"); }
            if (tileset == null) { throw new ArgumentNullException("tileset"); }
            if (terrainA.Tileset != tileset) { throw new InvalidOperationException("The given terrain type is in another tileset!"); }
            if (terrainB.Tileset != tileset) { throw new InvalidOperationException("The given terrain type is in another tileset!"); }

            this.allowedCombinations = new List<TerrainCombination>();
            foreach (TerrainCombination comb in combinations)
            {
                if (comb == TerrainCombination.Simple) { throw new TileSetException(string.Format("Unexpected terrain combination '{0}' defined for tile constraint!", comb)); }
                this.allowedCombinations.Add(comb);
            }

            this.tileset = tileset;
            if (terrainB.Parent != terrainA) { throw new ArgumentException(string.Format("TerrainType '{0}' must be the parent of TerrainType '{1}'!", terrainA, terrainB)); }

            this.terrainA = terrainA;
            this.terrainB = terrainB;
        }

        /// <see cref="ITerrainObjectConstraint.Tileset"/>
        public TileSet Tileset { get { return this.tileset; } }

        /// <see cref="ITerrainObjectConstraint.Check"/>
        public bool Check(TerrainObjectType terrainObj, IQuadTile targetPos)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The terrain type that has to be the first terrain type of the checked isometric tile.
        /// </summary>
        private TerrainType terrainA;

        /// <summary>
        /// The terrain type that has to be the second terrain type of the checked isometric tile.
        /// </summary>
        private TerrainType terrainB;

        /// <summary>
        /// List of the allowed combinations of the checked isometric tile.
        /// </summary>
        private List<TerrainCombination> allowedCombinations;

        /// <summary>
        /// Reference to the tileset that this constraint belongs to.
        /// </summary>
        private TileSet tileset;
    }
}
