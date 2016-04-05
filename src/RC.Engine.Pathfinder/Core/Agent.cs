using RC.Common;
using RC.Common.Diagnostics;
using RC.Engine.Pathfinder.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Engine.Pathfinder.Core
{
    /// <summary>
    /// Represents an agent on the pathfinding grid.
    /// </summary>
    class Agent : IAgent
    {
        /// <summary>
        /// Constructs a static agent with the given area.
        /// </summary>
        /// <param name="area">The area of this agent.</param>
        /// <param name="gridLayer">Reference to the grid layer that this agent is placed.</param>
        /// <param name="client">Reference to the client of this agent.</param>
        public Agent(RCIntRectangle area, Grid gridLayer, IAgentClient client)
        {
            this.agentArea = area;
            this.grid = gridLayer;
            this.movingSize = area.Width == area.Height && area.Width <= this.grid.MaxMovingSize ? area.Width : -1;
            this.client = client;
            this.currentPath = null;
            this.currentStepIndex = -1;
            this.movingStatus = this.movingSize != -1 ? AgentMovingStatusEnum.Stopped : AgentMovingStatusEnum.Static;
            this.stoppedStateTimer = this.movingStatus == AgentMovingStatusEnum.Stopped ? STOPPED_STATE_WAITING_TIME : -1;
            this.deadlockTimer = -1;
            this.agentsWaitingFor = new RCSet<Agent>();
        }

        #region IAgent members

        /// <see cref="IAgent.MoveTo"/>
        public void MoveTo(RCIntVector targetPosition)
        {
            if (targetPosition == RCIntVector.Undefined) { throw new ArgumentNullException("targetPosition"); }
            if (this.movingSize == -1) { throw new NotSupportedException("This agent is not supported to move!"); }

            this.stepBuffer = 0;
            this.stoppedStateTimer = -1;
            this.deadlockTimer = -1;
            this.MovingStatus = AgentMovingStatusEnum.Moving;
            targetPosition = new RCIntVector(Math.Max(targetPosition.X, 0), Math.Max(targetPosition.Y, 0));
            this.currentPath = new Path(this.grid[this.agentArea.Location.X, this.agentArea.Location.Y], this.grid[targetPosition.X, targetPosition.Y], this);
            this.currentStepIndex = 0;
            this.agentsWaitingFor.Clear();
        }

        /// <see cref="IAgent.StopMoving"/>
        public void StopMoving()
        {
            this.currentPath = null;
            this.currentStepIndex = -1;
            this.stepBuffer = 0;
            this.agentsWaitingFor.Clear();

            if (this.movingStatus == AgentMovingStatusEnum.Static)
            {
                this.deadlockTimer = -1;
                return;
            }

            if (this.movingStatus == AgentMovingStatusEnum.Moving)
            {
                this.MovingStatus = AgentMovingStatusEnum.Stopped;
                this.stoppedStateTimer = STOPPED_STATE_WAITING_TIME;
            }
        }

        /// <see cref="IAgent.IsMoving"/>
        public bool IsMoving
        {
            get
            {
                if (this.movingSize == -1) { return false; }
                return this.currentPath != null;
            }
        }

        /// <see cref="IAgent.Area"/>
        public RCIntRectangle Area { get { return this.agentArea; } }

        #endregion IAgent members

        /// <summary>
        /// Updates this agent.
        /// </summary>
        public void Update()
        {
            if (this.movingStatus == AgentMovingStatusEnum.Static)
            {
                if (this.deadlockTimer != -1)
                {
                    this.deadlockTimer--;
                    if (this.deadlockTimer == 0)
                    {
                        this.stepBuffer = 0;
                        this.stoppedStateTimer = -1;
                        this.deadlockTimer = -1;
                        this.MovingStatus = AgentMovingStatusEnum.Moving;
                        this.currentPath = new Path(this.grid[this.agentArea.Location.X, this.agentArea.Location.Y], this.currentPath.TargetCell, this);
                        this.currentStepIndex = 0;
                        this.agentsWaitingFor.Clear();
                        TraceManager.WriteAllTrace("Repath (back from deadlock wait)", TraceFilters.INFO);
                    }
                }
            }
            else if (this.movingStatus == AgentMovingStatusEnum.Stopped)
            {
                this.stoppedStateTimer--;
                if (this.stoppedStateTimer == 0)
                {
                    this.stoppedStateTimer = -1;
                    this.MovingStatus = AgentMovingStatusEnum.Static;
                }
            }
            else if (this.movingStatus == AgentMovingStatusEnum.Moving)
            {
                this.UpdateMoving();
            }
        }

        /// <summary>
        /// Steps this agent one cell towards the given direction.
        /// </summary>
        /// <param name="stepDirection">The direction of the step.</param>
        public void Step(int stepDirection)
        {
            this.agentArea += GridDirections.DIRECTION_TO_VECTOR[stepDirection];
        }

        /// <summary>
        /// Gets the moving size of this agent or -1 if this agent is not supported to move.
        /// </summary>
        public int MovingSize { get { return this.movingSize; } }

        /// <summary>
        /// Gets the moving state of this agent.
        /// </summary>
        public AgentMovingStatusEnum MovingStatus
        {
            get { return this.movingStatus; }
            private set
            {
                AgentMovingStatusEnum oldMovingStatus = this.movingStatus;
                this.movingStatus = value;
                if (this.movingStatus != oldMovingStatus && this.MovingStatusChanged != null)
                {
                    this.MovingStatusChanged(this, oldMovingStatus);
                }
            }
        }

        /// <summary>
        /// Gets the client of this agent.
        /// </summary>
        public IAgentClient Client { get { return this.client; } }

        /// <summary>
        /// This event is raised when the moving status of this agent has been changed.
        /// The first parameter of this event is the agent whose moving status has been changed.
        /// The second parameter of this event is the old moving status of the agent.
        /// </summary>
        public event Action<Agent, AgentMovingStatusEnum> MovingStatusChanged;

        /// <summary>
        /// Updates this agent when it is in the Moving state.
        /// </summary>
        private void UpdateMoving()
        {
            if (this.currentStepIndex < this.currentPath.CalculatedStepCount)
            {
                /// We still have calculated steps -> follow the next one.
                this.stepBuffer += this.client.MaxSpeed;
                while (this.currentStepIndex < this.currentPath.CalculatedStepCount && this.stepBuffer >= this.currentPath.GetStepLength(this.currentStepIndex))
                {
                    /// Execute the current step.
                    RCSet<Agent> collidingAgents = null;
                    if (this.grid.StepAgent(this, this.currentPath.GetStepDirection(this.currentStepIndex), out collidingAgents))
                    {
                        /// Step was successful.
                        this.stepBuffer -= this.currentPath.GetStepLength(this.currentStepIndex);
                        this.currentStepIndex++;
                        this.agentsWaitingFor.Clear();
                    }
                    else
                    {
                        /// Step was unsuccessful -> handle the detected collision.
                        this.HandleCollision(collidingAgents);
                        this.stepBuffer = 0;
                        break;
                    }
                }
            }
            else
            {
                this.stepBuffer = 0;

                /// No more calculated steps -> check the status of the path.
                if (this.currentPath.Status == PathStatusEnum.Complete)
                {
                    if (this.currentPath.CalculatedStepCount > 0)
                    {
                        /// Try to go closer -> repath.
                        /// TODO: Repath is needed only if the agent is not at the target cell!
                        this.currentPath = new Path(this.grid[this.agentArea.Location.X, this.agentArea.Location.Y], this.currentPath.TargetCell, this);
                        this.currentStepIndex = 0;
                        this.agentsWaitingFor.Clear();
                        TraceManager.WriteAllTrace("Repath (try to go closer)", TraceFilters.INFO);
                    }
                    else
                    {
                        /// Cannot go closer -> stop this agent.
                        this.currentPath = null;
                        this.currentStepIndex = -1;
                        this.MovingStatus = AgentMovingStatusEnum.Stopped;
                        this.stoppedStateTimer = STOPPED_STATE_WAITING_TIME;
                        this.agentsWaitingFor.Clear();
                    }
                    return;
                }

                /// The path is not complete -> continue the calculation.
                this.currentPath.CalculateNextRegion();
                if (this.currentPath.Status == PathStatusEnum.Broken)
                {
                    /// Path is broken -> repath.
                    this.currentPath = new Path(this.grid[this.agentArea.Location.X, this.agentArea.Location.Y], this.currentPath.TargetCell, this);
                    this.currentStepIndex = 0;
                    this.agentsWaitingFor.Clear();
                    TraceManager.WriteAllTrace("Repath (current path broken)", TraceFilters.INFO);
                }
            }
        }

        /// <summary>
        /// Handle the case when this agent is colliding with the given other agents during a step.
        /// </summary>
        /// <param name="collidingAgents">The given other agents.</param>
        private void HandleCollision(RCSet<Agent> collidingAgents)
        {
            /// Group the colliding agents whether they are in Moving state or not.
            RCSet<Agent> staticCollidingAgents = new RCSet<Agent>();
            RCSet<Agent> movingCollidingAgents = new RCSet<Agent>();
            foreach (Agent collidingAgent in collidingAgents)
            {
                if (collidingAgent.movingStatus == AgentMovingStatusEnum.Stopped)
                {
                    collidingAgent.MovingStatus = AgentMovingStatusEnum.Static;
                    collidingAgent.stoppedStateTimer = -1;
                }
                if (collidingAgent.movingStatus != AgentMovingStatusEnum.Moving)
                {
                    staticCollidingAgents.Add(collidingAgent);
                }
                else
                {
                    movingCollidingAgents.Add(collidingAgent);
                }
            }

            if (staticCollidingAgents.Count != 0)
            {
                /// Collision with static agent -> repath.
                this.currentPath = new Path(this.grid[this.agentArea.Location.X, this.agentArea.Location.Y], this.currentPath.TargetCell, this);
                this.currentStepIndex = 0;
                this.agentsWaitingFor.Clear();
                TraceManager.WriteAllTrace("Repath (static colliding agents)", TraceFilters.INFO);
            }
            else if (movingCollidingAgents.Count != 0)
            {
                /// Collision with moving agent -> check for deadlock!
                bool deadlockFound = false;
                foreach (Agent movingCollidingAgent in movingCollidingAgents)
                {
                    if (movingCollidingAgent.agentsWaitingFor.Contains(this))
                    {
                        /// Deadlock situation!
                        movingCollidingAgent.agentsWaitingFor.Remove(this);
                        deadlockFound = true;
                    }
                    else
                    {
                        this.agentsWaitingFor.Add(movingCollidingAgent);
                    }
                }
                if (deadlockFound)
                {
                    TraceManager.WriteAllTrace("Deadlock found", TraceFilters.INFO);
                    this.agentsWaitingFor.Clear();
                    this.MovingStatus = AgentMovingStatusEnum.Static;
                    this.deadlockTimer = DEADLOCK_WAITING_TIME;
                }
            }
        }

        /// <summary>
        /// The area of this agent on the pathfinding grid.
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
        /// The index of the current step on the path being followed by this agent or -1 if this agent is not following a path currently.
        /// </summary>
        private int currentStepIndex;

        /// <summary>
        /// The moving state of this agent.
        /// </summary>
        private AgentMovingStatusEnum movingStatus;

        /// <summary>
        /// The remaining time before this agent goes to Static state if it's in Stopped state; otherwise -1.
        /// </summary>
        private int stoppedStateTimer;

        /// <summary>
        /// The remaining waiting time before this agent recalculates its path if it's in deadlock; otherwise -1.
        /// </summary>
        private int deadlockTimer;

        /// <summary>
        /// Reference to the agents that this agent is waiting for.
        /// </summary>
        private RCSet<Agent> agentsWaitingFor;

        /// <summary>
        /// The moving size of this agent or -1 if this agent is not supported to move.
        /// </summary>
        private readonly int movingSize;

        /// <summary>
        /// Reference to the pathfinding grid that this agent is placed.
        /// </summary>
        private readonly Grid grid;

        /// <summary>
        /// Reference to the client of this agent.
        /// </summary>
        private readonly IAgentClient client;

        /// <summary>
        /// The waiting time of an agent in Stopped state before it automatically goes to Static state.
        /// </summary>
        private const int STOPPED_STATE_WAITING_TIME = 10;

        /// <summary>
        /// The waiting time of an agent in deadlock state.
        /// </summary>
        private const int DEADLOCK_WAITING_TIME = 10;
    }
}
