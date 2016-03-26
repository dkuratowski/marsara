using System;
using System.Collections.Generic;
using System.Linq;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Core;
using RC.Engine.Simulator.Engine;
using RC.Engine.Pathfinder.PublicInterfaces;
using RC.Common.Diagnostics;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// The path-tracker implementation for entities on the ground.
    /// </summary>
    public class GroundPathTracker : PathTrackerBase, IAgentClient
    {
        /// <summary>
        /// Constructs a GroundPathTracker instance.
        /// </summary>
        /// <param name="controlledEntity">The entity that this path tracker controls.</param>
        public GroundPathTracker(Entity controlledEntity) : base(controlledEntity)
        {
            this.pathfinder = ComponentManager.GetInterface<IPathfinder>();
            this.pathfinderAgent = null;
        }

        #region IAgentClient members

        /// <see cref="IAgentClient.MaxSpeed"/>
        public RCNumber MaxSpeed { get { return this.ControlledEntity.ElementType.Speed.Read(); } }

        /// <see cref="IAgentClient.IsOverlapEnabled"/>
        public bool IsOverlapEnabled(IAgentClient otherClient)
        {
            return false;
            //throw new NotImplementedException();
        }

        #endregion IAgentClient members

        #region PathTrackerBase overrides

        /// <see cref="PathTrackerBase.ActivateImpl"/>
        protected override void ActivateImpl()
        {
            RCIntVector targetAgentCellCoords = this.TargetPosition.Round() + this.ControlledEntity.ElementType.ObstacleArea.Read().Location;
            this.pathfinderAgent.MoveTo(targetAgentCellCoords);
        }

        /// <see cref="PathTrackerBase.DeactivateImpl"/>
        protected override void DeactivateImpl()
        {
            this.pathfinderAgent.StopMoving();
        }

        /// <see cref="PathTrackerBase.CurrentWaypoint"/>
        protected override RCNumVector CurrentWaypoint
        {
            get
            {
                RCIntVector currentAgentCellCoords = this.pathfinderAgent.Area.Location;
                RCIntVector currentWaypointCellCoords = currentAgentCellCoords - this.ControlledEntity.ElementType.ObstacleArea.Read().Location;
                return this.TargetPosition.Round() != currentWaypointCellCoords ? currentWaypointCellCoords : this.TargetPosition;
            }
        }

        /// <see cref="PathTrackerBase.IsLastWaypoint"/>
        protected override bool IsLastWaypoint { get { return !this.pathfinderAgent.IsMoving; } }

        /// <see cref="PathTrackerBase.OnAttaching"/>
        public override bool OnAttaching(RCNumVector position)
        {
            if (this.pathfinderAgent != null) { throw new InvalidOperationException("The controlled entity has already been attached to the map!"); }
            RCIntVector agentCellCoords = position.Round() + this.ControlledEntity.ElementType.ObstacleArea.Read().Location;
            this.pathfinderAgent = this.pathfinder.PlaceAgent(this.ControlledEntity.ElementType.ObstacleArea.Read() + position.Round(), this);
            return this.pathfinderAgent != null;
        }

        /// <see cref="PathTrackerBase.OnDetached"/>
        public override void OnDetached()
        {
            if (this.pathfinderAgent == null) { throw new InvalidOperationException("The controlled entity is not attached to the map!"); }

            this.pathfinder.RemoveAgent(this.pathfinderAgent);
            this.pathfinderAgent = null;
        }

        /// <see cref="HeapedObject.DisposeImpl"/>
        protected override void DisposeImpl()
        {
            if (this.pathfinderAgent != null)
            {
                this.pathfinder.RemoveAgent(this.pathfinderAgent);
                this.pathfinderAgent = null;
            }
        }

        #endregion PathTrackerBase overrides

        /// <summary>
        /// Reference to the pathfinder agent that corresponds to this path-tracker.
        /// </summary>
        private IAgent pathfinderAgent;

        /// <summary>
        /// Reference to the pathfinder component.
        /// </summary>
        private readonly IPathfinder pathfinder;
    }
}
