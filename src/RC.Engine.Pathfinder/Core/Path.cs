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
    /// TODO: this is only a dummy implementation!
    /// </summary>
    class Path
    {
        /// <summary>
        /// Constructs a path.
        /// </summary>
        /// <param name="sourceCell">The source cell of this path.</param>
        /// <param name="targetCell">The target cell of this path.</param>
        public Path(Cell sourceCell, Cell targetCell)
        {
            if (sourceCell == null) { throw new ArgumentNullException("sourceCell"); }
            if (targetCell == null) { throw new ArgumentNullException("targetCell"); }

            this.sourceCell = sourceCell;
            this.targetCell = targetCell;
            this.currentCell = this.sourceCell;
            this.isBroken = false;

            // TODO: finish!
            //this.highLevelPath = new PathfindingAlgorithm<Region>(this.sourceCell.GetRegion(
        }

        /// <summary>
        /// Gets the target cell of this path.
        /// </summary>
        public Cell TargetCell { get { return this.targetCell; } }

        /// <summary>
        /// Gets the direction to be followed in the current step.
        /// </summary>
        /// <exception cref="InvalidOperationException">If this path is not in state PathStatusEnum.Ready.</exception>
        public int CurrentStepDirection { get { return GridDirections.VECTOR_TO_DIRECTION(this.targetCell.Coords - this.currentCell.Coords); } }

        /// <summary>
        /// Gets length of the current step.
        /// </summary>
        /// <exception cref="InvalidOperationException">If this path is not in state PathStatusEnum.Ready.</exception>
        public RCNumber CurrentStepLength { get { return GridDirections.DIRECTION_TO_STEPLENGTH[this.CurrentStepDirection]; } }

        /// <summary>
        /// Gets the current status of this path.
        /// </summary>
        public PathStatusEnum Status
        {
            get
            {
                if (this.isBroken) { return PathStatusEnum.Broken; }
                return this.currentCell != this.targetCell ? PathStatusEnum.ReadyToFollow : PathStatusEnum.Finished;
            }
        }

        /// <summary>
        /// Calculates the next step on this path.
        /// </summary>
        /// <exception cref="InvalidOperationException">If this path is not in state PathStatusEnum.Ready.</exception>
        public void CalculateNextStep()
        {
            if (this.Status != PathStatusEnum.ReadyToFollow) { throw new InvalidOperationException("The state of this path shall be PathStatusEnum.ReadyToFollow!"); }
            this.currentCell = this.currentCell.GetNeighbour(this.CurrentStepDirection);
        }

        /// <summary>
        /// Brakes this path manually.
        /// </summary>
        public void Brake()
        {
            if (this.Status != PathStatusEnum.ReadyToFollow) { throw new InvalidOperationException("The state of this path shall be PathStatusEnum.ReadyToFollow!"); }
            this.isBroken = true;
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
        /// The current cell of this path.
        /// </summary>
        private Cell currentCell;

        /// <summary>
        /// This flag indicates whether this path is currently broken.
        /// </summary>
        private bool isBroken;

        /// <summary>
        /// The current high-level path.
        /// </summary>
        private PathfindingAlgorithm<Region> highLevelPath;

        /// <summary>
        /// The current low-level path.
        /// </summary>
        private PathfindingAlgorithm<Cell> lowLevelPath;
    }
}
