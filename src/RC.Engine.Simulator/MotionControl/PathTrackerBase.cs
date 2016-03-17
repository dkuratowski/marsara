using System;
using System.Collections.Generic;
using System.Linq;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Common.Diagnostics;
using RC.Engine.Simulator.Core;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// The common base class of entity path tracker implementations.
    /// </summary>
    public abstract class PathTrackerBase : HeapedObject
    {
        /// <summary>
        /// Activates this path tracker to move its controlled entity to the given target position.
        /// </summary>
        /// <param name="targetPosition">The given target position.</param>
        public void Activate(RCNumVector targetPosition)
        {
            if (targetPosition == RCNumVector.Undefined) { throw new ArgumentNullException("targetPosition"); }

            this.Deactivate();
            this.targetPosition.Write(targetPosition);
            this.ActivateImpl();

            this.currentPosition.Write(this.CalculateNextPosition());
        }

        /// <summary>
        /// Deactivates this path-tracker. If this path-tracker is currently deactivated then this function has no effect.
        /// </summary>
        public void Deactivate()
        {
            this.DeactivateImpl();
            this.currentPosition.Write(RCNumVector.Undefined);
            this.targetPosition.Write(RCNumVector.Undefined);
        }

        /// <summary>
        /// Gets the next position of the controlled entity if this path-tracker is active; otherwise RCNumVector.Undefined.
        /// </summary>
        public RCNumVector NextPosition { get { return this.currentPosition.Read(); } }

        /// <summary>
        /// This method is called when the controlled entity is being attached to the map.
        /// </summary>
        /// <param name="toPosition">The target position.</param>
        /// <returns>True if the controlled entity has been successfully attached to the map; otherwise false.</returns>
        public virtual bool OnAttaching(RCNumVector position) { return true; }

        /// <summary>
        /// This method is called when the controlled entity is being detached from the map.
        /// </summary>
        public virtual void OnDetached() { }

        /// <summary>
        /// Updates the state of this path-tracker.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the path-tracker is currently inactive.</exception>
        internal void Update()
        {
            if (this.currentPosition.Read() == RCNumVector.Undefined) { throw new InvalidOperationException("Path-tracker is inactive!"); }

            if (this.IsLastWaypoint && this.currentPosition.Read() == this.CurrentWaypoint)
            {
                // Last waypoint reached -> deactivate this path-tracker.
                this.Deactivate();
                return;
            }

            // Calculate the next position of the controlled entity.
            this.currentPosition.Write(this.CalculateNextPosition());
        }

        #region Overridable methods

        /// <summary>
        /// Activates this path tracker to move its controlled entity to the target position.
        /// Note: the target position is available via the PathTrackerBase.TargetPosition property for the derived classes.
        /// </summary>
        protected virtual void ActivateImpl() { }

        /// <summary>
        /// Deactivates this path-tracker. If this path-tracker is currently deactivated then this function has no effect.
        /// </summary>
        protected virtual void DeactivateImpl() { }

        /// <summary>
        /// Gets the current waypoint to be reached by the controlled entity.
        /// </summary>
        protected abstract RCNumVector CurrentWaypoint { get; }

        /// <summary>
        /// Gets whether the current waypoint is the last to be reached in the current path-tracking procedure.
        /// </summary>
        protected abstract bool IsLastWaypoint { get; }

        #endregion Overridable methods

        #region Protected members for the derived classes

        /// <summary>
        /// Constructs a PathTrackerBase instance.
        /// </summary>
        /// <param name="controlledEntity">The entity that this path tracker controls.</param>
        /// <param name="targetDistanceThreshold">If the controlled entity is closer to the target position than this threshold, it is considered to arrive.</param>
        protected PathTrackerBase(Entity controlledEntity)
        {
            if (controlledEntity == null) { throw new ArgumentNullException("controlledEntity"); }

            this.currentPosition = this.ConstructField<RCNumVector>("currentPosition");
            this.targetPosition = this.ConstructField<RCNumVector>("targetPosition");
            this.controlledEntity = this.ConstructField<Entity>("controlledEntity");

            this.currentPosition.Write(RCNumVector.Undefined);
            this.targetPosition.Write(RCNumVector.Undefined);
            this.controlledEntity.Write(controlledEntity);
        }
        
        /// <summary>
        /// Gets a reference to the controlled entity of the path-tracker.
        /// </summary>
        protected Entity ControlledEntity { get { return this.controlledEntity.Read(); } }

        /// <summary>
        /// Gets the target position of this path-tracker or RCNumVector.Undefined if there is no target position currently.
        /// </summary>
        protected RCNumVector TargetPosition { get { return this.targetPosition.Read(); } }

        #endregion Protected members for the derived classes

        #region Internal methods

        /// <summary>
        /// Calculates the next position of the controlled entity from its current position and the next waypoint.
        /// </summary>
        /// <returns>The calculated next position of the controlled entity.</returns>
        private RCNumVector CalculateNextPosition()
        {
            RCNumVector currentPosition = this.controlledEntity.Read().MotionControl.PositionVector.Read();
            RCNumber distToWP = MapUtils.ComputeDistance(currentPosition, this.CurrentWaypoint);
            if (distToWP > this.controlledEntity.Read().ElementType.Speed.Read())
            {
                // Move towards the next waypoint.
                RCNumVector translationVect = (this.controlledEntity.Read().ElementType.Speed.Read() / distToWP) * (this.CurrentWaypoint - currentPosition);
                return currentPosition + translationVect;
            }
            else
            {
                // Next waypoint can be reached in this step.
                return this.CurrentWaypoint;
            }
        }

        #endregion Internal methods

        #region Heaped members

        /// <summary>
        /// The current position of the controlled entity if this path-tracker is active; otherwise RCNumVector.Undefined.
        /// </summary>
        private readonly HeapedValue<RCNumVector> currentPosition;

        /// <summary>
        /// The target position of this path-tracker or RCNumVector.Undefined if there is no target position currently.
        /// </summary>
        private readonly HeapedValue<RCNumVector> targetPosition;

        /// <summary>
        /// The entity that this path tracker is controlling.
        /// </summary>
        private readonly HeapedValue<Entity> controlledEntity;

        #endregion Heaped members
    }
}
