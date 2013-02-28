using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.PublicInterfaces;

namespace RC.Engine.Core
{
    /// <summary>
    /// Selects a tile variant for an isometric tile depending on it's parent.
    /// </summary>
    class NeighbourCondition : IIsoTileCondition
    {
        /// <summary>
        /// Constructs a NeighbourCondition instance.
        /// </summary>
        /// <param name="combination">The terrain combination of the neighbour to check.</param>
        /// <param name="direction">The direction of the neighbour to check.</param>
        /// <param name="tileset">The tileset of this condition.</param>
        public NeighbourCondition(TerrainCombination combination, MapDirection direction, TileSet tileset)
        {
            if (tileset == null) { throw new ArgumentNullException("tileset"); }

            this.combination = combination;
            this.direction = direction;
            this.tileset = tileset;
        }

        /// <see cref="IIsoTileCondition.Check"/>
        public bool Check(IIsoTile isoTile)
        {
            if (isoTile == null) { throw new ArgumentNullException("isoTile"); }

            IIsoTile neighbour = isoTile.GetNeighbour(this.direction);
            if (neighbour == null) { return false; }

            if (isoTile.Type.TerrainA != neighbour.Type.TerrainA || isoTile.Type.TerrainB != neighbour.Type.TerrainB) { return false; }
            return this.combination == neighbour.Type.Combination;
        }

        /// <summary>
        /// Gets the terrain combination of the neighbour to check.
        /// </summary>
        public TerrainCombination Combination { get { return this.combination; } }

        /// <summary>
        /// Gets the direction of the neighbour to check.
        /// </summary>
        public MapDirection Direction { get { return this.direction; } }

        /// <see cref="IIsoTileCondition.Tileset"/>
        public ITileSet Tileset { get { return this.tileset; } }

        /// <summary>
        /// The terrain combination of the neighbour to check.
        /// </summary>
        private TerrainCombination combination;

        /// <summary>
        /// The direction of the neighbour to check.
        /// </summary>
        private MapDirection direction;

        /// <summary>
        /// Reference to the tileset of this condition.
        /// </summary>
        private TileSet tileset;
    }
}
