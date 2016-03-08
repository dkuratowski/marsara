using RC.Common;
using RC.Engine.Pathfinder.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Engine.Pathfinder.Core
{
    /// <summary>
    /// Represents an agent on a pathfinding grid-layer.
    /// </summary>
    class Agent : IMovingObstacle, IDisposable
    {
        /// <summary>
        /// Constructs an agent with the given position and size.
        /// </summary>
        /// <param name="position">The position of this agent.</param>
        /// <param name="size">The size of this agent.</param>
        /// <param name="gridLayer">Reference to the grid layer that this agent belongs to.</param>
        /// <param name="client">Reference to the client of this agent.</param>
        public Agent(RCIntVector position, int size, GridLayer gridLayer, IObstacleClient client)
        {
            this.isDisposed = false;
            this.agentArea = new RCIntRectangle(position.X, position.Y, size, size);
            this.size = size;
            this.gridLayer = gridLayer;
            this.client = client;
            this.currentPath = null;
        }

        /// <summary>
        /// Constructs a static agent with the given area.
        /// </summary>
        /// <param name="area">The area of this agent.</param>
        /// <param name="gridLayer">Reference to the grid layer that this agent is placed.</param>
        /// <param name="client">Reference to the client of this agent.</param>
        public Agent(RCIntRectangle area, GridLayer gridLayer, IObstacleClient client)
        {
            this.isDisposed = false;
            this.agentArea = area;
            this.gridLayer = gridLayer;
            this.size = -1;
            this.client = client;
            this.currentPath = null;
        }

        #region IMovingObstacle members

        /// <see cref="IMovingObstacle.MoveTo"/>
        public void MoveTo(RCIntVector targetPosition)
        {
            if (this.isDisposed) { throw new InvalidOperationException("Agent already disposed!"); }
            if (this.size == -1) { throw new InvalidOperationException("Static agent cannot move!"); }
            if (targetPosition == RCIntVector.Undefined) { throw new ArgumentNullException("targetPosition"); }

            this.stepBuffer = 0;
            this.currentPath = new Path(this.gridLayer[this.agentArea.Location.X, this.agentArea.Location.Y], this.gridLayer[targetPosition.X, targetPosition.Y]);
        }

        /// <see cref="IMovingObstacle.StopMoving"/>
        public void StopMoving()
        {
            if (this.isDisposed) { throw new InvalidOperationException("Agent already disposed!"); }
            if (this.size == -1) { throw new InvalidOperationException("Static agent cannot move!"); }

            this.currentPath = null;
            this.stepBuffer = 0;
        }

        /// <see cref="IMovingObstacle.IsMoving"/>
        public bool IsMoving
        {
            get
            {
                if (this.isDisposed) { throw new InvalidOperationException("Agent already disposed!"); }
                if (this.size == -1) { throw new InvalidOperationException("Static agent cannot move!"); }

                return this.currentPath != null;
            }
        }

        /// <see cref="IMovingObstacle.Area"/>
        public RCIntRectangle Area
        {
            get
            {
                if (this.isDisposed) { throw new InvalidOperationException("Agent already disposed!"); }
                return this.agentArea;
            }
        }
        
        #endregion IMovingObstacle members

        #region IDisposable members

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            if (this.isDisposed) { throw new InvalidOperationException("Agent already disposed!"); }
            this.isDisposed = true;
        }

        #endregion IDisposable members

        /// <summary>
        /// Updates this agent.
        /// </summary>
        public void Update()
        {
            if (this.isDisposed) { throw new InvalidOperationException("Agent already disposed!"); }

            /// If this agent is not following a path -> do nothing.
            if (this.currentPath == null) { return; }

            if (this.currentPath.Status == PathStatusEnum.Ready)
            {
                /// The path is ready to follow -> follow it until we have buffer.
                this.stepBuffer += this.client.MaxSpeed;
                while (this.currentPath.Status == PathStatusEnum.Ready && this.stepBuffer >= this.currentPath.CurrentStepLength)
                {
                    /// Step towards this.currentPath.CurrentStepDirection!
                    this.agentArea += GridDirections.DIRECTION_TO_VECTOR[this.currentPath.CurrentStepDirection];
                    this.stepBuffer -= this.currentPath.CurrentStepLength;
                    this.currentPath.CalculateNextStep();
                }

                if (this.currentPath.Status == PathStatusEnum.Finished)
                {
                    /// Path following finished -> delete the path and stop this agent.
                    this.currentPath = null;
                    this.stepBuffer = 0;
                }
                //else if (this.currentPath.Status == PathStatusEnum.Broken)
                //{
                //    /// Path being followed has been broken -> recalculate the path.
                //    this.currentPath = new Path(this.gridLayer[this.agentArea.Location.X, this.agentArea.Location.Y], this.currentPath.TargetCell);
                //}
            }
        }

        /// <summary>
        /// This flag indicates whether this agent has already been disposed.
        /// </summary>
        private bool isDisposed;

        /// <summary>
        /// The area of this agent on the grid-layer it is placed.
        /// </summary>
        private RCIntRectangle agentArea;

        /// <summary>
        /// The step-buffer remained from the previous updates.
        /// </summary>
        private RCNumber stepBuffer;

        /// <summary>
        /// Reference to the path being followed by this agent or null if this agent is not following a path currently.
        /// </summary>
        private Path currentPath;

        /// <summary>
        /// The size of this agent or -1 if this is a static agent.
        /// </summary>
        private readonly int size;

        /// <summary>
        /// Reference to the grid layer that this agent belongs to.
        /// </summary>
        private readonly GridLayer gridLayer;

        /// <summary>
        /// Reference to the client of this agent.
        /// </summary>
        private readonly IObstacleClient client;

        private static readonly RCNumber DIAGONAL_DISTANCE_PER_STEP = RCNumber.ROOT_OF_TWO;
        private static readonly RCNumber STRAIGHT_DISTANCE_PER_STEP = 1;
    }
}
