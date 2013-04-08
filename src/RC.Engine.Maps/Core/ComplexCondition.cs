using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Maps.Core
{
    /// <summary>
    /// Enumerates the logical operators can be used for defining complex conditions.
    /// </summary>
    enum LogicalOp
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
    class ComplexCondition : IIsoTileCondition
    {
        /// <summary>
        /// Constructs a ComplexCondition instance.
        /// </summary>
        /// <param name="subconditions">The subconditions connected by a logical operator.</param>
        /// <param name="logicalOp">The operator.</param>
        /// <param name="tileset">The tileset of this condition.</param>
        public ComplexCondition(List<IIsoTileCondition> subconditions, LogicalOp logicalOp, TileSet tileset)
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

            this.subconditions = new List<IIsoTileCondition>(subconditions);
            this.logicalOp = logicalOp;
            this.tileset = tileset;
        }

        /// <see cref="IIsoTileCondition.Check"/>
        public bool Check(IIsoTile isoTile)
        {
            bool retVal;
            if (this.logicalOp == LogicalOp.AND)
            {
                retVal = true;
                foreach (IIsoTileCondition subcond in this.subconditions)
                {
                    if (!subcond.Check(isoTile)) { retVal = false; break; }
                }
            }
            else if (this.logicalOp == LogicalOp.OR)
            {
                retVal = false;
                foreach (IIsoTileCondition subcond in this.subconditions)
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

        /// <see cref="IIsoTileCondition.Tileset"/>
        public ITileSet Tileset { get { return this.tileset; } }

        /// <summary>
        /// List of the subconditions connected by a logical operator.
        /// </summary>
        private List<IIsoTileCondition> subconditions;

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
