using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Engine.Pathfinder.Core
{
    /// <summary>
    /// Represents a path to be followed.
    /// </summary>
    class Path
    {
        /// <summary>
        /// Constructs a path for the given agent.
        /// </summary>
        /// <param name="sourceCell">The source cell of this path.</param>
        /// <param name="targetCell">The target cell of this path.</param>
        /// <param name="agent">The given agent</param>
        public Path(Cell sourceCell, Cell targetCell, Agent agent)
        {
            if (sourceCell == null) { throw new ArgumentNullException("sourceCell"); }
            if (targetCell == null) { throw new ArgumentNullException("targetCell"); }
            if (agent == null) { throw new ArgumentNullException("agent"); }

            this.sourceCell = sourceCell;
            this.targetCell = targetCell;
            this.grid = this.sourceCell.Sector.Grid;
            this.agent = agent;

            /// Initialize the high- and low-level paths.
            this.highLevelPath = null;
            this.lowLevelPath = null;
            this.regionIndex = 0;
            this.isBroken = false;
        }

        /// <summary>
        /// Gets the target cell of this path.
        /// </summary>
        public Cell TargetCell { get { return this.targetCell; } }

        /// <summary>
        /// Gets the number of the calculated steps on this path.
        /// </summary>
        public int CalculatedStepCount { get { return this.lowLevelPath != null ? this.lowLevelPath.Count - 1 : 0; } }

        /// <summary>
        /// Gets the current status of this path.
        /// </summary>
        public PathStatusEnum Status
        {
            get
            {
                if (this.isBroken) { return PathStatusEnum.Broken; }
                if (this.highLevelPath == null) { return PathStatusEnum.Partial; }
                return this.regionIndex < this.highLevelPath.Count ? PathStatusEnum.Partial : PathStatusEnum.Complete;
            }
        }

        /// <summary>
        /// Gets the direction to be followed in the given step.
        /// </summary>
        /// <param name="stepIdx">The index of the step.</param>
        /// <returns>The direction to be followed in the given step.</returns>
        public int GetStepDirection(int stepIdx)
        {
            if (stepIdx < 0) { throw new IndexOutOfRangeException(string.Format("The requested step index '{0}' shall be greater than 0!")); }
            if (stepIdx >= this.CalculatedStepCount) { throw new IndexOutOfRangeException(string.Format("The requested step index '{0}' exceeds the number of calculated steps on this path!")); }
            return GridDirections.VECTOR_TO_DIRECTION(this.lowLevelPath[stepIdx + 1].Coords - this.lowLevelPath[stepIdx].Coords);
        }


        /// <summary>
        /// Gets the length of the given step.
        /// </summary>
        /// <param name="stepIdx">The index of the step.</param>
        /// <returns>The length of the given step.</returns>
        public RCNumber GetStepLength(int stepIdx)
        {
            return GridDirections.DIRECTION_TO_STEPLENGTH[this.GetStepDirection(stepIdx)];
        }

        /// <summary>
        /// Calculates the next region on this path.
        /// </summary>
        /// <exception cref="InvalidOperationException">If this path is not in state PathStatusEnum.Partial.</exception>
        public void CalculateNextRegion()
        {
            if (this.Status != PathStatusEnum.Partial) { throw new InvalidOperationException("The state of this path shall be PathStatusEnum.Partial!"); }

            /// Calculate the high-level path if not yet calculated.
            if (this.highLevelPath == null)
            {
                SectorSubdivision sourceSubdivision = this.sourceCell.Sector.GetSubdivisionForAgent(this.agent);
                Region sourceRegion = this.sourceCell.GetRegion(sourceSubdivision);
                RegionGraph regionGraph = new RegionGraph(this.targetCell, this.agent);
                PathfindingAlgorithm<Region> highLevelPathfinding = new PathfindingAlgorithm<Region>(sourceRegion, regionGraph);
                PathfindingResult<Region> highLevelPathfindingResult = highLevelPathfinding.Execute();
                this.highLevelPath = new List<Tuple<Region, RCSet<Cell>>>();
                for (int i = 0; i < highLevelPathfindingResult.Path.Count; i++)
                {
                    Region currentRegion = highLevelPathfindingResult.Path[i];
                    Region nextRegion = i < highLevelPathfindingResult.Path.Count - 1 ? highLevelPathfindingResult.Path[i + 1] : null;
                    RCSet<Cell> targetCells = nextRegion != null ? regionGraph.GetTransitionCells(currentRegion, nextRegion) : new RCSet<Cell> { this.targetCell };
                    this.highLevelPath.Add(Tuple.Create(currentRegion, targetCells));
                }
            }

            /// Check if the region to be calculated is still valid -> if not then this path is broken.
            if (!this.highLevelPath[this.regionIndex].Item1.IsValid)
            {
                this.isBroken = true;
                return;
            }

            IGraph<Cell> graph = null;
            if (this.regionIndex < this.highLevelPath.Count - 1)
            {
                graph = new TransitRegionGraph(
                    this.highLevelPath[this.regionIndex].Item1,
                    this.highLevelPath[this.regionIndex + 1].Item1.Subdivision.Sector.Center,
                    this.highLevelPath[this.regionIndex].Item2);
            }
            else
            {
                graph = new TransitRegionGraph(
                    this.highLevelPath[this.regionIndex].Item1,
                    this.targetCell.Coords,
                    this.highLevelPath[this.regionIndex].Item2);
            }
            Cell fromCell = this.lowLevelPath != null ? this.lowLevelPath[this.lowLevelPath.Count - 1] : this.sourceCell;
            PathfindingAlgorithm<Cell> lowLevelPathfinding = new PathfindingAlgorithm<Cell>(fromCell, graph);
            PathfindingResult<Cell> lowLevelPathfindingResult = lowLevelPathfinding.Execute();

            /// Check if a transition cell of the current region could be found -> if not then this path is broken.
            if (this.regionIndex < this.highLevelPath.Count - 1 && !lowLevelPathfindingResult.TargetFound)
            {
                this.isBroken = true;
                return;
            }

            /// Save the next section of the low level path and move to the next region.
            if (this.lowLevelPath == null)
            {
                this.lowLevelPath = lowLevelPathfindingResult.Path;
            }
            else
            {
                this.lowLevelPath.RemoveAt(this.lowLevelPath.Count - 1);
                this.lowLevelPath.AddRange(lowLevelPathfindingResult.Path);
            }
            this.regionIndex++;
        }

        /// <summary>
        /// The source cell of this path.
        /// </summary>
        private readonly Cell sourceCell;

        /// <summary>
        /// The target cell of this path.
        /// </summary>
        private readonly Cell targetCell;

        /// <summary>
        /// The grid on which this path is calculated.
        /// </summary>
        private readonly Grid grid;

        /// <summary>
        /// The agent for which this path is to be calculated.
        /// </summary>
        private readonly Agent agent;

        /// <summary>
        /// The calculated high-level path with its regions and the corresponding transitions cells.
        /// </summary>
        private List<Tuple<Region, RCSet<Cell>>> highLevelPath;

        /// <summary>
        /// The calculated low-level path.
        /// </summary>
        private List<Cell> lowLevelPath;

        /// <summary>
        /// The index of the region to be calculated next.
        /// </summary>
        private int regionIndex;

        /// <summary>
        /// This flag indicates whether this path has been broken or not.
        /// </summary>
        private bool isBroken;
    }
}
