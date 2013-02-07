using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine
{
    /// <summary>
    /// Interface of conditions to select the correct tile variant for an isometric tile.
    /// </summary>
    public interface ITileCondition
    {
        /// <summary>
        /// Checks whether the condition is satisfied at the given IsoTile or not.
        /// </summary>
        /// <param name="isoTile">The isometric tile to check.</param>
        /// <returns>True if the condition is satisfied, false otherwise.</returns>
        bool Check(IIsoTile isoTile);

        /// <summary>
        /// Gets the tileset of this condition.
        /// </summary>
        TileSet Tileset { get; }
    }

    /// <summary>
    /// Selects a tile variant for an isometric tile depending on it's parent.
    /// </summary>
    public class NeighbourCondition : ITileCondition
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

        /// <see cref="ITileCondition.Check"/>
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

        /// <see cref="ITileCondition.Tileset"/>
        public TileSet Tileset { get { return this.tileset; } }

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

    /// <summary>
    /// Enumerates the logical operators can be used for defining complex conditions.
    /// </summary>
    public enum LogicalOp
    {
        [EnumMapping(XmlTileSetConstants.COMPLEXCOND_AND_ELEM)]
        AND = 0,

        [EnumMapping(XmlTileSetConstants.COMPLEXCOND_OR_ELEM)]
        OR = 1,

        [EnumMapping(XmlTileSetConstants.COMPLEXCOND_NOT_ELEM)]
        NOT = 2
    }

    /// <summary>
    /// Represents a complex condition with a logical operator.
    /// </summary>
    public class ComplexCondition : ITileCondition
    {
        /// <summary>
        /// Constructs a ComplexCondition instance.
        /// </summary>
        /// <param name="subconditions">The subconditions connected by a logical operator.</param>
        /// <param name="logicalOp">The operator.</param>
        /// <param name="tileset">The tileset of this condition.</param>
        public ComplexCondition(List<ITileCondition> subconditions, LogicalOp logicalOp, TileSet tileset)
        {
            if (subconditions == null) { throw new ArgumentNullException("subconditions"); }
            if (tileset == null) { throw new ArgumentNullException("tileset"); }

            if (logicalOp == LogicalOp.AND || logicalOp == LogicalOp.OR)
            {
                if (subconditions.Count < 2) { throw new TileSetException("At least 2 subconditions must be defined in case of LogicalOp.AND or LogicalOp.OR operators!"); }
            }
            else
            {
                if (subconditions.Count != 1) { throw new TileSetException("Only one subcondition must be defined in case of LogicalOp.NOT operator!"); }
            }

            this.subconditions = new List<ITileCondition>(subconditions);
            this.logicalOp = logicalOp;
            this.tileset = tileset;
        }

        /// <see cref="ITileCondition.Check"/>
        public bool Check(IIsoTile isoTile)
        {
            bool retVal;
            if (this.logicalOp == LogicalOp.AND)
            {
                retVal = true;
                foreach (ITileCondition subcond in this.subconditions)
                {
                    if (!subcond.Check(isoTile)) { retVal = false; break; }
                }
            }
            else if (this.logicalOp == LogicalOp.OR)
            {
                retVal = false;
                foreach (ITileCondition subcond in this.subconditions)
                {
                    if (subcond.Check(isoTile)) { retVal = true; break; }
                }
            }
            else
            {
                retVal = !this.subconditions[0].Check(isoTile);
            }
            return retVal;
        }

        /// <see cref="ITileCondition.Tileset"/>
        public TileSet Tileset { get { return this.tileset; } }

        /// <summary>
        /// List of the subconditions connected by a logical operator.
        /// </summary>
        private List<ITileCondition> subconditions;

        /// <summary>
        /// The logical operator.
        /// </summary>
        private LogicalOp logicalOp;

        /// <summary>
        /// Reference to the tileset of this condition.
        /// </summary>
        private TileSet tileset;
    }
}
