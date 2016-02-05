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
        /// <returns>
        /// A list that contains the dynamic obstacles in the environment of the controlled target. A dynamic obstacle is described by
        /// a rectangle that is the area currently occupied by the obstacle, and a vector that is the current velocity of the obstacle.
        /// </returns>
        public IEnumerable<Tuple<RCNumRectangle, RCNumVector>> DynamicObstacles
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
        /// Checks whether the given move is valid for the controlled entity.
        /// </summary>
        /// <param name="fromPosition">
        /// The starting position of the move to be checked or RCNumVector.Undefined if the move has no starting position
        /// (e.g. to check if the controlled entity can be placed to the given target position onto the map).
        /// </param>
        /// <param name="toPosition">The target position of the move to be checked.</param>
        /// <returns>True if the given move is valid for the controlled entity; otherwise false.</returns>
        /// <remarks>Must be overriden in the derived classes.</remarks>
        public abstract bool ValidateMove(RCNumVector fromPosition, RCNumVector toPosition);

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
        /// Checks whether the given entity is considered as an obstacle for this path-tracker.
        /// </summary>
        /// <param name="entity">The entity to be checked.</param>
        /// <returns>True if the given entity is considered as an obstacle for this path-tracker; otherwise false.</returns>
        /// <remarks>This method must be overriden in the derived classes.</remarks>
        protected abstract bool IsObstacle(Entity entity);

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
        /// <param name="targetDistanceThreshold">If the controlled entity is closer to the target position than this threshold, it is considered to arrive.</param>
        protected PathTrackerBase(Entity controlledEntity, RCNumber targetDistanceThreshold)
        {
            if (controlledEntity == null) { throw new ArgumentNullException("controlledEntity"); }
            if (targetDistanceThreshold < 0) { throw new ArgumentOutOfRangeException("targetDistanceThreshold", "Target distance threshold cannot be negative!"); }

            this.targetPosition = this.ConstructField<RCNumVector>("targetPosition");
            this.closestDistanceToTarget = this.ConstructField<RCNumber>("closestDistanceToTarget");
            this.timeSinceClosestDistanceReached = this.ConstructField<short>("timeSinceClosestDistanceReached");
            this.controlledEntity = this.ConstructField<Entity>("controlledEntity");
            this.targetDistanceThreshold = this.ConstructField<RCNumber>("targetDistanceThreshold");

            this.preferredVelocityCache = new CachedValue<RCNumVector>(this.CalculatePreferredVelocity);
            this.dynamicObstaclesCache = new CachedValue<IEnumerable<Tuple<RCNumRectangle, RCNumVector>>>(this.CollectNearbyDynamicObstacles);

            this.targetPosition.Write(RCNumVector.Undefined);
            this.closestDistanceToTarget.Write(-1);
            this.timeSinceClosestDistanceReached.Write(0);
            this.controlledEntity.Write(controlledEntity);
            this.targetDistanceThreshold.Write(targetDistanceThreshold);
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
            return MapUtils.ComputeDistance(this.ControlledEntity.MotionControl.PositionVector.Read(), this.TargetPosition) < this.targetDistanceThreshold.Read() ||
                   this.timeSinceClosestDistanceReached.Read() > 100;
        }

        /// <summary>
        /// Collects the nearby dynamic obstacles on the map.
        /// </summary>
        /// <returns>The enumerable list of nearby dynamic obstacles.</returns>
        private IEnumerable<Tuple<RCNumRectangle, RCNumVector>> CollectNearbyDynamicObstacles()
        {
            List<Tuple<RCNumRectangle, RCNumVector>> retList = new List<Tuple<RCNumRectangle, RCNumVector>>();
            RCSet<Entity> entitiesInRange = this.ControlledEntity.Locator.SearchNearbyEntities(ENVIRONMENT_SIGHT_RANGE);
            foreach (Entity entityInRange in entitiesInRange)
            {
                if (this.IsObstacle(entityInRange) &&
                    !this.controlledEntity.Read().IsOverlapEnabled(entityInRange) &&
                    !entityInRange.IsOverlapEnabled(this.controlledEntity.Read()))
                {
                    retList.Add(Tuple.Create(entityInRange.Area, entityInRange.MotionControl.VelocityVector.Read()));
                }
            }
            return retList;
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

        /// <summary>
        /// If the controlled entity is closer to the target position than this threshold, it is considered to arrive.
        /// </summary>
        private readonly HeapedValue<RCNumber> targetDistanceThreshold;

        #endregion Heaped members

        /// <summary>
        /// Cache that stores the calculated preferred velocity during an update procedure.
        /// </summary>
        private CachedValue<RCNumVector> preferredVelocityCache;

        /// <summary>
        /// Cache that stores the list of the nearby dynamic obstacles.
        /// </summary>
        private CachedValue<IEnumerable<Tuple<RCNumRectangle, RCNumVector>>> dynamicObstaclesCache;

        /// <summary>
        /// The range of the environment in quadratic tiles taken into account when collecting dynamic obstacles.
        /// </summary>
        private const int ENVIRONMENT_SIGHT_RANGE = 2;
    }
}
