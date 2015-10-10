using System;
using System.Collections.Generic;
using System.Linq;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// The common base class of entity path tracker implementations.
    /// </summary>
    public abstract class PathTrackerBase : HeapedObject
    {
        /// <summary>
        /// Gets or sets the target position that this path tracker has to move its target. Set the value of this property to
        /// RCNumVector.Undefined to inactivate the path-tracker.
        /// </summary>
        public RCNumVector TargetPosition
        {
            get { return this.targetPosition.Read(); }
            set
            {
                this.closestDistanceToTarget.Write(-1);
                this.timeSinceClosestDistanceReached.Write(0);
                this.targetPosition.Write(value);
                this.SetTargetPositionImpl(value);
            }
        }

        /// <summary>
        /// Gets whether this path-tracker is currently active or not.
        /// </summary>
        public abstract bool IsActive { get; }

        /// <summary>
        /// Gets the currently preferred velocity.
        /// </summary>
        public RCNumVector PreferredVelocity
        {
            get
            {
                if (!this.IsActive) { throw new InvalidOperationException("Path-tracker is inactive!"); }
                return this.preferredVelocityCache.Value;
            }
        }

        /// <summary>
        /// Gets the list of the dynamic obstacles in the environment of the controlled target.
        /// </summary>
        /// <returns>A list that contains the dynamic obstacles in the environment of the controlled target.</returns>
        public IEnumerable<DynamicObstacleInfo> DynamicObstacles
        {
            get
            {
                if (!this.IsActive) { throw new InvalidOperationException("Path-tracker is inactive!"); }
                return this.dynamicObstaclesCache.Value;
            }
        }

        /// <summary>
        /// Checks whether the controlled target remains inside the followed path with the given velocity.
        /// </summary>
        /// <param name="velocity">The velocity to be check.</param>
        /// <returns>True if the given velocity is valid; otherwise false.</returns>
        public bool ValidateVelocity(RCNumVector velocity)
        {
            if (velocity == RCNumVector.Undefined) { throw new ArgumentNullException("velocity"); }
            if (!this.IsActive) { throw new InvalidOperationException("Path-tracker is inactive!"); }
            return this.ValidateVelocityImpl(velocity);
        }

        /// <summary>
        /// Calculates a target position for a VTOL operation into the map layer handled by this path-tracker.
        /// </summary>
        /// <param name="vtolOnTheSpot">True to calculate target position without transition.</param>
        /// <returns>The calculated target position or RCNumVector.Undefined if no target position could be found.</returns>
        /// <remarks>This method must be overriden in the derived classes.</remarks>
        public abstract RCNumVector CalculateTargetPositionForVTOL(bool vtolOnTheSpot);

        /// <summary>
        /// Updates the state of this path-tracker.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the path-tracker is currently inactive.</exception>
        internal void Update()
        {
            if (!this.IsActive) { throw new InvalidOperationException("Path-tracker is inactive!"); }

            this.UpdateDistanceToTarget();
            if (this.CheckInactivationCriteria())
            {
                // Path-tracker inactivation.
                this.TargetPosition = RCNumVector.Undefined;
            }

            /// Invalidate the caches.
            this.preferredVelocityCache.Invalidate();
            this.dynamicObstaclesCache.Invalidate();
        }

        #region Overridable methods

        /// <summary>
        /// Checks whether the given position is valid for the controlled entity.
        /// </summary>
        /// <param name="position">The position to be checked.</param>
        /// <returns>True if the given position is valid for the controlled entity; otherwise false.</returns>
        /// <remarks>Must be overriden in the derived classes.</remarks>
        public abstract bool ValidatePosition(RCNumVector position);

        /// <summary>
        /// Derived classes can implement additional procedures when the target position of this path tracker has been set by overriding this method.
        /// The default implementation does nothing.
        /// </summary>
        /// <param name="targetPosition">The new target position or RCNumVector.Undefined if this path-tracker has been inactivated.</param>
        protected virtual void SetTargetPositionImpl(RCNumVector targetPosition) { }

        /// <summary>
        /// Calculates the preferred velocity of this path-tracker.
        /// </summary>
        /// <returns>The calculated preferred velocity.</returns>
        /// <remarks>This method must be overriden in the derived classes.</remarks>
        protected abstract RCNumVector CalculatePreferredVelocity();

        /// <summary>
        /// Collects the nearby dynamic obstacles on the map.
        /// </summary>
        /// <returns>The enumerable list of nearby dynamic obstacles.</returns>
        /// <remarks>This method must be overriden in the derived classes.</remarks>
        protected abstract IEnumerable<DynamicObstacleInfo> CollectNearbyDynamicObstacles();

        /// <summary>
        /// Calculates the distance from the current position of the controlled entity to the target.
        /// </summary>
        /// <returns>The distance from the current position of the controlled entity to the target.</returns>
        protected abstract RCNumber CalculateDistanceToTarget();

        /// <summary>
        /// The internal implementation of the velocity validation.
        /// </summary>
        /// <param name="velocity">The velocity to be validated.</param>
        /// <returns>True if the given velocity is valid; otherwise false.</returns>
        protected abstract bool ValidateVelocityImpl(RCNumVector velocity);

        #endregion Overridable methods

        #region Protected members for the derived classes

        /// <summary>
        /// Constructs a PathTrackerBase instance.
        /// </summary>
        /// <param name="controlledEntity">The entity that this path tracker controls.</param>
        protected PathTrackerBase(Entity controlledEntity)
        {
            if (controlledEntity == null) { throw new ArgumentNullException("controlledEntity"); }

            this.targetPosition = this.ConstructField<RCNumVector>("targetPosition");
            this.closestDistanceToTarget = this.ConstructField<RCNumber>("closestDistanceToTarget");
            this.timeSinceClosestDistanceReached = this.ConstructField<short>("timeSinceClosestDistanceReached");
            this.controlledEntity = this.ConstructField<Entity>("controlledEntity");

            this.preferredVelocityCache = new CachedValue<RCNumVector>(this.CalculatePreferredVelocity);
            this.dynamicObstaclesCache = new CachedValue<IEnumerable<DynamicObstacleInfo>>(this.CollectNearbyDynamicObstacles);

            this.targetPosition.Write(RCNumVector.Undefined);
            this.closestDistanceToTarget.Write(-1);
            this.timeSinceClosestDistanceReached.Write(0);
            this.controlledEntity.Write(controlledEntity);
        }


        /// <summary>
        /// Gets a reference to the controlled entity of the path-tracker.
        /// </summary>
        protected Entity ControlledEntity { get { return this.controlledEntity.Read(); } }

        #endregion Protected members for the derived classes

        #region Internal methods

        /// <summary>
        /// Updates the distance to the target based on the current position of the controlled entity.
        /// </summary>
        private void UpdateDistanceToTarget()
        {
            RCNumber currentDistanceToTarget = this.CalculateDistanceToTarget();
            if (this.closestDistanceToTarget.Read() == -1 || currentDistanceToTarget < this.closestDistanceToTarget.Read())
            {
                this.closestDistanceToTarget.Write(currentDistanceToTarget);
                this.timeSinceClosestDistanceReached.Write(0);
            }
            else
            {
                this.timeSinceClosestDistanceReached.Write((short)(this.timeSinceClosestDistanceReached.Read() + 1));
            }
        }

        /// <summary>
        /// Checks whether this path-tracker can be inactivated or not.
        /// </summary>
        /// <returns>Return true to invalidate this path-tracker; otherwise false.</returns>
        private bool CheckInactivationCriteria()
        {
            /// TODO: replace this with a more sophisticated inactivation criteria if necessary!
            return this.ControlledEntity.Area.Contains(this.TargetPosition) || this.timeSinceClosestDistanceReached.Read() > 100;
        }

        #endregion Internal methods

        #region Heaped members

        /// <summary>
        /// The target position of this path-tracker or RCNumVector.Undefined if this path-tracker is currently inactive.
        /// </summary>
        private readonly HeapedValue<RCNumVector> targetPosition;

        /// <summary>
        /// The closest distance to the target position that has been reached during the following of the current path
        /// or -1 if the closes distance has to be considered as infinite.
        /// </summary>
        private readonly HeapedValue<RCNumber> closestDistanceToTarget;

        /// <summary>
        /// The number of elapsed frames since the closest distance to the target has been reached.
        /// </summary>
        private readonly HeapedValue<short> timeSinceClosestDistanceReached;

        /// <summary>
        /// The entity that this path tracker controls.
        /// </summary>
        private readonly HeapedValue<Entity> controlledEntity;

        #endregion Heaped members

        /// <summary>
        /// The maximum value of transition during a VTOL operation in map coordinates.
        /// </summary>
        protected static readonly RCNumber MAX_VTOL_TRANSITION = 8;

        /// <summary>
        /// Cache that stores the calculated preferred velocity during an update procedure.
        /// </summary>
        private CachedValue<RCNumVector> preferredVelocityCache;

        /// <summary>
        /// Cache that stores the list of the nearby dynamic obstacles.
        /// </summary>
        private CachedValue<IEnumerable<DynamicObstacleInfo>> dynamicObstaclesCache;
    }
}
