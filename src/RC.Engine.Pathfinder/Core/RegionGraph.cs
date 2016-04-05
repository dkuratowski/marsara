using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Engine.Pathfinder.Core
{
    /// <summary>
    /// Represents a graph of grid regions for a high-level pathfinding.
    /// </summary>
    class RegionGraph : IGraph<Region>
    {
        /// <summary>
        /// Constructs a RegionGraph instance with the given target cell for the given agent.
        /// </summary>
        /// <param name="targetCell">The given target cell.</param>
        /// <param name="agent">The given agent.</param>
        //public RegionGraph(Region targetRegion, Agent agent)
        public RegionGraph(Cell targetCell, Agent agent)
        {
            this.agent = agent;
            this.targetCell = targetCell;
            SectorSubdivision targetSubdivision = this.targetCell.Sector.GetSubdivisionForAgent(this.agent);
            this.targetRegion = this.targetCell.GetRegion(targetSubdivision);
            this.transitionCells = new Dictionary<Region, Dictionary<Region, RCSet<Cell>>>();
        }

        #region IGraph<Region> members

        /// <see cref="IGraph&lt;Region&gt;.Distance"/>
        public int Distance(Region regionA, Region regionB)
        {
            return this.Distance(regionA.Subdivision.Sector.Center, regionB.Subdivision.Sector.Center);
        }

        /// <see cref="IGraph&lt;Region&gt;.EstimationToTarget"/>
        public int EstimationToTarget(Region node)
        {
            return this.Distance(node.Subdivision.Sector.Center, this.targetCell.Coords);
        }

        /// <see cref="IGraph&lt;Region&gt;.GetNeighbours"/>
        public IEnumerable<Region> GetNeighbours(Region node)
        {
            if (!this.transitionCells.ContainsKey(node))
            {
                this.transitionCells.Add(node, new Dictionary<Region,RCSet<Cell>>());
                for (int direction = 0; direction < GridDirections.DIRECTION_COUNT; direction++)
                {
                    Sector neighbourSector = node.Subdivision.Sector.GetNeighbour(direction);
                    if (neighbourSector == null) { continue; }

                    SectorSubdivision neighbourSubdivision = neighbourSector.GetSubdivisionForAgent(this.agent);
                    foreach (Region neighbourRegion in neighbourSubdivision.Regions)
                    {
                        RCSet<Cell> transitionCells = node.GetTransitionCells(neighbourRegion, direction);
                        if (transitionCells.Count > 0)
                        {
                            this.transitionCells[node][neighbourRegion] = transitionCells;
                        }
                    }
                }
            }
            return this.transitionCells[node].Keys;
        }

        /// <see cref="IGraph&lt;Region&gt;.IsTargetNode"/>
        public bool IsTargetNode(Region node)
        {
            return node == this.targetRegion;
        }

        #endregion IGraph<Region> members

        /// <summary>
        /// Gets the transition cells from the given source region into the given target region.
        /// </summary>
        /// <param name="fromRegion">The given source region.</param>
        /// <param name="toRegion">The given target region.</param>
        /// <returns>The transition cells from the given source region into the given target region.</returns>
        public RCSet<Cell> GetTransitionCells(Region fromRegion, Region toRegion)
        {
            return this.transitionCells[fromRegion][toRegion];
        }

        /// <summary>
        /// Calculates the distance between 2 points on the pathfinding graph.
        /// </summary>
        /// <param name="pointA">The first point.</param>
        /// <param name="pointB">The second point.</param>
        /// <returns>The distance between the 2 points on the pathfinding graph.</returns>
        private int Distance(RCIntVector pointA, RCIntVector pointB)
        {
            int horizontalDistance = Math.Abs(pointA.X - pointB.X);
            int verticalDistance = Math.Abs(pointA.Y - pointB.Y);
            int difference = Math.Abs(horizontalDistance - verticalDistance);
            return Math.Min(horizontalDistance, verticalDistance) * Grid.DIAGONAL_UNIT_DISTANCE + difference * Grid.STRAIGHT_UNIT_DISTANCE;
        }

        /// <summary>
        /// Reference to the target region.
        /// </summary>
        private Region targetRegion;

        /// <summary>
        /// Reference to the target cell.
        /// </summary>
        private Cell targetCell;

        /// <summary>
        /// Reference to the agent.
        /// </summary>
        private Agent agent;

        /// <summary>
        /// The lists of transition cells from one region to another region.
        /// </summary>
        private Dictionary<Region, Dictionary<Region, RCSet<Cell>>> transitionCells;
    }
}
