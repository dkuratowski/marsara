using System;
using System.Collections.Generic;
using System.Linq;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Engine;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// The path-tracker implementation for entities on the ground.
    /// </summary>
    public class GroundPathTracker : PathTrackerBase
    {
        /// <summary>
        /// Constructs a GroundPathTracker instance.
        /// </summary>
        /// <param name="controlledEntity">The entity that this path tracker controls.</param>
        public GroundPathTracker(Entity controlledEntity) : base(controlledEntity)
        {
            this.pathFinder = ComponentManager.GetInterface<IPathFinder>();
            this.currentPath = null;
        }

        #region PathTrackerBase overrides

        /// <see cref="PathTrackerBase.IsActive"/>
        public override bool IsActive { get { return this.currentPath != null && this.currentPath.IsReadyForUse; } }

        /// <see cref="PathTrackerBase.CalculateTargetPositionForVTOL"/>
        public override RCNumVector CalculateTargetPositionForVTOL(bool vtolOnTheSpot)
        {
            if (vtolOnTheSpot)
            {
                /// Landing on the spot.
                return this.ValidatePosition(this.ControlledEntity.MotionControl.PositionVector.Read()) ?
                    this.ControlledEntity.MotionControl.PositionVector.Read() :
                    RCNumVector.Undefined;
            }
            
            /// Normal landing.
            RCNumber bottomToMapEdgeDistance = this.ControlledEntity.Scenario.Map.CellSize.Y + (RCNumber)1 / (RCNumber)2
                                             - this.ControlledEntity.Area.Bottom;
            if (bottomToMapEdgeDistance < 0) { return RCNumVector.Undefined; }

            RCNumber transitionValue = bottomToMapEdgeDistance <= MAX_VTOL_TRANSITION ? bottomToMapEdgeDistance : MAX_VTOL_TRANSITION;
            RCNumVector positionAfterLand = this.ControlledEntity.MotionControl.PositionVector.Read() + new RCNumVector(0, transitionValue);
            return this.ValidatePosition(positionAfterLand) ? positionAfterLand : RCNumVector.Undefined;
        }

        /// <see cref="PathTrackerBase.ValidatePosition"/>
        public override bool ValidatePosition(RCNumVector position)
        {
            if (position == RCNumVector.Undefined) { throw new ArgumentNullException("position"); }

            if (this.pathFinder.GetNavMeshNode(position) == null)
            {
                /// The entity wants to go to a non-walkable position on the map -> invalid position
                return false;
            }

            /// Detect collision with other entities.
            RCNumRectangle newEntityArea = this.ControlledEntity.CalculateArea(position);
            bool collisionWithOtherEntities =
                this.ControlledEntity.Scenario.GetElementsOnMap<Entity>(newEntityArea, MapObjectLayerEnum.GroundObjects, MapObjectLayerEnum.GroundReservations)
                .Any(collidingEntity => collidingEntity != this.ControlledEntity);

            /// Return true if there is no collision with other entities.
            return !collisionWithOtherEntities;
        }

        /// <see cref="PathTrackerBase.SetTargetPositionImpl"/>
        protected override void SetTargetPositionImpl(RCNumVector targetPosition)
        {
            if (targetPosition == RCNumVector.Undefined)
            {
                /// Path-tracker inactivation -> Delete the current path.
                this.currentPath = null;
            }
            else
            {
                /// Start searching a new path.
                this.currentPath = this.pathFinder.StartPathSearching(this.ControlledEntity.MotionControl.PositionVector.Read(),
                                                                      this.TargetPosition,
                                                                      PATHFINDING_ITERATION_LIMIT);
            }
        }

        /// <see cref="PathTrackerBase.CalculatePreferredVelocity"/>
        protected override RCNumVector CalculatePreferredVelocity()
        {
            RCNumVector currentPosition = this.ControlledEntity.MotionControl.PositionVector.Read();
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

        /// <see cref="PathTrackerBase.CollectNearbyDynamicObstacles"/>
        protected override IEnumerable<DynamicObstacleInfo> CollectNearbyDynamicObstacles()
        {
            List<DynamicObstacleInfo> retList = new List<DynamicObstacleInfo>();
            RCSet<Entity> entitiesInRange = this.ControlledEntity.Locator.SearchNearbyEntities(ENVIRONMENT_SIGHT_RANGE);
            foreach (Entity entityInRange in entitiesInRange.Where(entity => !entity.MotionControl.IsFlying))
            {
                retList.Add(new DynamicObstacleInfo()
                {
                    Position = entityInRange.Area,
                    Velocity = entityInRange.MotionControl.VelocityVector.Read()
                });
            }
            return retList;
        }

        /// <see cref="PathTrackerBase.CalculateDistanceToTarget"/>
        protected override RCNumber CalculateDistanceToTarget()
        {
            /// Search the index of the node on the current path based on the position of the entity.
            RCNumVector currentPosition = this.ControlledEntity.MotionControl.PositionVector.Read();
            INavMeshNode currentNode = this.pathFinder.GetNavMeshNode(currentPosition);
            if (currentNode == null) { throw new InvalidOperationException("Entity position is out of the walkable area of the map!"); }
            int currentNodeIdxOnPath = this.currentPath.IndexOf(currentNode);
            if (currentNodeIdxOnPath == -1) { throw new InvalidOperationException("Followed path lost!"); }

            /// Check if the entity is at the last node on the path -> trivial case.
            if (currentNodeIdxOnPath == this.currentPath.Length - 1) { return MapUtils.ComputeDistance(currentPosition, this.currentPath.ToCoords); }

            /// In case of direct path check if the entity is at just 1 node before the last node on the path -> trivial case.
            if (this.currentPath.IsTargetFound && currentNodeIdxOnPath + 1 == this.currentPath.Length - 1) { return MapUtils.ComputeDistance(currentPosition, this.currentPath.ToCoords); }

            /// Calculate cumulative distance in the non-trivial cases.
            int lastMiddleNodeIdx = this.currentPath.Length - (this.currentPath.IsTargetFound ? 2 : 1);
            RCNumber distanceToTarget = MapUtils.ComputeDistance(currentPosition, this.currentPath[currentNodeIdxOnPath + 1].Polygon.Center);
            for (int i = currentNodeIdxOnPath + 1; i < lastMiddleNodeIdx; i++)
            {
                distanceToTarget += MapUtils.ComputeDistance(this.currentPath[i].Polygon.Center, this.currentPath[i + 1].Polygon.Center);
            }
            distanceToTarget += MapUtils.ComputeDistance(this.currentPath[lastMiddleNodeIdx].Polygon.Center, this.currentPath.ToCoords);
            return distanceToTarget;
        }

        /// <see cref="PathTrackerBase.ValidateVelocityImpl"/>
        protected override bool ValidateVelocityImpl(RCNumVector velocity)
        {
            INavMeshNode node = this.pathFinder.GetNavMeshNode(this.ControlledEntity.MotionControl.PositionVector.Read() + velocity);
            return node != null && this.currentPath.IndexOf(node) != -1;
        }

        #endregion PathTrackerBase overrides

        /// <summary>
        /// Reference to the current path being searched or followed or null if there is no path currently being searched followed.
        /// </summary>
        private IPath currentPath;

        /// <summary>
        /// Reference to the RC.Engine.Simulator.PathFinder component.
        /// </summary>
        private readonly IPathFinder pathFinder;

        /// <summary>
        /// The maximum number of iterations in a pathfinding execution.
        /// </summary>
        private const int PATHFINDING_ITERATION_LIMIT = 1000;

        /// <summary>
        /// The range of the environment in quadratic tiles taken into account when collecting dynamic obstacles.
        /// </summary>
        private const int ENVIRONMENT_SIGHT_RANGE = 2;
    }
}
