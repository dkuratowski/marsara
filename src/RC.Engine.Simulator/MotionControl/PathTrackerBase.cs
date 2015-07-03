using System;
using System.Collections.Generic;
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
    public abstract class PathTrackerBase : HeapedObject, IMotionControlEnvironment
    {
        /// <summary>
        /// Constructs a PathTrackerBase instance.
        /// </summary>
        /// <param name="controlledEntity">The entity that this path tracker controls.</param>
        public PathTrackerBase(Entity controlledEntity)
        {
            if (controlledEntity == null) { throw new ArgumentNullException("controlledEntity"); }

            this.currentPathFromCoords = this.ConstructField<RCNumVector>("currentPathFromCoords");
            this.currentPathToCoords = this.ConstructField<RCNumVector>("currentPathToCoords");
            this.closestDistanceToTarget = this.ConstructField<RCNumber>("closestDistanceToTarget");
            this.timeSinceClosestDistanceReached = this.ConstructField<short>("timeSinceClosestDistanceReached");
            this.controlledEntity = this.ConstructField<Entity>("controlledEntity");

            this.pathFinder = ComponentManager.GetInterface<IPathFinder>();

            this.currentPath = null;
            this.preferredVelocityCache = new CachedValue<RCNumVector>(this.CalculatePreferredVelocity);
            this.dynamicObstaclesCache = new CachedValue<IEnumerable<DynamicObstacleInfo>>(this.CollectNearbyDynamicObstacles);

            this.currentPathFromCoords.Write(RCNumVector.Undefined);
            this.currentPathToCoords.Write(RCNumVector.Undefined);
            this.closestDistanceToTarget.Write(-1);
            this.timeSinceClosestDistanceReached.Write(0);
            this.controlledEntity.Write(controlledEntity);
        }

        /// <summary>
        /// Gets or sets the target position that this path tracker has to move its target. Set the value of this property to
        /// RCNumVector.Undefined to inactivate the path-tracker.
        /// </summary>
        public RCNumVector TargetPosition
        {
            get { return this.currentPathToCoords.Read(); }
            set
            {
                this.closestDistanceToTarget.Write(-1);
                this.timeSinceClosestDistanceReached.Write(0);
                this.currentPathToCoords.Write(value);

                if (value == RCNumVector.Undefined)
                {
                    /// Path-tracker inactivation.
                    this.currentPath = null;
                    this.currentPathFromCoords.Write(RCNumVector.Undefined);
                    return;
                }
                else
                {
                    /// Start searching a new path.
                    this.currentPathFromCoords.Write(this.ControlledEntity.PositionValue.Read());
                    this.currentPath = this.pathFinder.StartPathSearching(this.currentPathFromCoords.Read(),
                                                                          this.currentPathToCoords.Read(),
                                                                          PATHFINDING_ITERATION_LIMIT);
                }
            }
        }

        /// <summary>
        /// Gets whether this path-tracker is currently active or not.
        /// </summary>
        public bool IsActive { get { return this.currentPath != null && this.currentPath.IsReadyForUse; } }

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
                this.closestDistanceToTarget.Write(-1);
                this.timeSinceClosestDistanceReached.Write(0);
                this.currentPathToCoords.Write(RCNumVector.Undefined);
                this.currentPathFromCoords.Write(RCNumVector.Undefined);
                this.currentPath = null;
            }

            /// Invalidate the caches.
            this.preferredVelocityCache.Invalidate();
            this.dynamicObstaclesCache.Invalidate();
        }

        #region IMotionControlEnvironment members

        /// <see cref="IMotionControlEnvironment.PreferredVelocity"/>
        public RCNumVector PreferredVelocity
        {
            get
            {
                if (!this.IsActive) { throw new InvalidOperationException("Path-tracker is inactive!"); }
                return this.preferredVelocityCache.Value;
            }
        }

        /// <see cref="IMotionControlEnvironment.DynamicObstacles"/>
        public IEnumerable<DynamicObstacleInfo> DynamicObstacles
        {
            get
            {
                if (!this.IsActive) { throw new InvalidOperationException("Path-tracker is inactive!"); }
                return this.dynamicObstaclesCache.Value;
            }
        }

        /// <see cref="IMotionControlEnvironment.ValidateVelocity"/>
        public bool ValidateVelocity(RCNumVector velocity)
        {
            if (velocity == RCNumVector.Undefined) { throw new ArgumentNullException("velocity"); }
            if (!this.IsActive) { throw new InvalidOperationException("Path-tracker is inactive!"); }
            return this.ValidateVelocityImpl(velocity);
        }

        #endregion IMotionControlEnvironment members

        #region Overridable methods

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

        /// <summary>
        /// Checks whether this path-tracker can be invalidated or not.
        /// </summary>
        /// <returns>Return true to invalidate this path-tracker; otherwise false.</returns>
        protected virtual bool CheckInactivationCriteria()
        {
            /// TODO: replace this with a more sophisticated inactivation criteria if necessary!
            return this.ControlledEntity.Position.Contains(this.TargetPosition) || this.timeSinceClosestDistanceReached.Read() > 100;
        }

        #endregion Overridable methods

        #region Protected members for the derived classes

        /// <summary>
        /// Gets a reference to the controlled entity of the path-tracker.
        /// </summary>
        protected Entity ControlledEntity { get { return this.controlledEntity.Read(); } }

        /// <summary>
        /// Gets a reference to the currently followed path, or null if there is no path currently being followed.
        /// </summary>
        protected IPath CurrentPath { get { return this.currentPath; } }

        /// <summary>
        /// Gets a reference to the RC.Engine.Simulator.PathFinder component.
        /// </summary>
        protected IPathFinder PathFinder { get { return this.pathFinder; } }

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
        /// Internal method for calculating the preferred velocity based on the current position and the current
        /// path being followed.
        /// </summary>
        /// <returns>The calculated preferred velocity.</returns>
        private RCNumVector CalculatePreferredVelocity()
        {
            RCNumVector currentPosition = this.ControlledEntity.PositionValue.Read();
            INavMeshNode currentNode = this.pathFinder.GetNavMeshNode(currentPosition);
            if (currentNode == null) { throw new InvalidOperationException("Entity position is out of the walkable area of the map!"); }
            int currentNodeIdxOnPath = this.currentPath.IndexOf(currentNode);
            if (currentNodeIdxOnPath == -1) { throw new InvalidOperationException("Followed path lost!"); }

            RCNumVector preferredVelocity = RCNumVector.Undefined;
            if (currentNodeIdxOnPath == this.currentPath.Length - 1)
            {
                preferredVelocity = this.TargetPosition - currentPosition;
            }
            else
            {
                INavMeshNode nextPathNode = this.currentPath[currentNodeIdxOnPath + 1];
                INavMeshEdge edgeToNextNode = currentNode.GetEdge(nextPathNode);
                preferredVelocity = (edgeToNextNode.TransitionVector + edgeToNextNode.Midpoint - currentPosition) / 2;
            }
            return preferredVelocity;
        }

        #endregion Internal methods

        #region Heaped members

        /// <summary>
        /// The beginning coordinates of the current path or RCNumVector.Undefined if there is currently no path being searched
        /// or followed.
        /// </summary>
        private readonly HeapedValue<RCNumVector> currentPathFromCoords;

        /// <summary>
        /// The target coordinates of the current path or RCNumVector.Undefined if there is currently no path being searched
        /// or followed.
        /// </summary>
        private readonly HeapedValue<RCNumVector> currentPathToCoords;

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
        /// Reference to the current path being searched or followed or null if there is no path currently being searched followed.
        /// </summary>
        private IPath currentPath;

        /// <summary>
        /// Reference to the RC.Engine.Simulator.PathFinder component.
        /// </summary>
        private IPathFinder pathFinder;

        /// <summary>
        /// Cache that stores the calculated preferred velocity during an update procedure.
        /// </summary>
        private CachedValue<RCNumVector> preferredVelocityCache;

        /// <summary>
        /// Cache that stores the list of the nearby dynamic obstacles.
        /// </summary>
        private CachedValue<IEnumerable<DynamicObstacleInfo>> dynamicObstaclesCache;

        /// <summary>
        /// The maximum number of iterations in a pathfinding execution.
        /// </summary>
        private const int PATHFINDING_ITERATION_LIMIT = 1000;
    }
}
