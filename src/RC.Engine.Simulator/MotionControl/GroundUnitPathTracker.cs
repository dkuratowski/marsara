using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Scenarios;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// The actuator implementation for ground units.
    /// </summary>
    public class GroundUnitPathTracker : PathTrackerBase
    {
        /// <summary>
        /// Constructs a GroundUnitPathTracker instance.
        /// </summary>
        /// <param name="controlledEntity">The entity that this path tracker controls.</param>
        public GroundUnitPathTracker(Entity controlledEntity) : base(controlledEntity) { }

        #region PathTrackerBase overrides

        /// <see cref="PathTrackerBase.CollectNearbyDynamicObstacles"/>
        protected override IEnumerable<DynamicObstacleInfo> CollectNearbyDynamicObstacles()
        {
            /// TODO: change this method to return only ground units as dynamic obstacles!
            List<DynamicObstacleInfo> retList = new List<DynamicObstacleInfo>();
            HashSet<Entity> entitiesInRange = this.ControlledEntity.Scenario.GetEntitiesOnMap<Entity>(
                new RCNumRectangle(this.ControlledEntity.PositionValue.Read() - new RCNumVector(ENVIRONMENT_SIGHT_RANGE, ENVIRONMENT_SIGHT_RANGE),
                                   new RCNumVector(ENVIRONMENT_SIGHT_RANGE, ENVIRONMENT_SIGHT_RANGE) * 2));
            foreach (Entity entityInRange in entitiesInRange)
            {
                if (entityInRange != this.ControlledEntity)
                {
                    retList.Add(new DynamicObstacleInfo()
                                {
                                    Position = entityInRange.Position,
                                    Velocity = entityInRange.Velocity
                                });
                }
            }
            return retList;
        }

        /// <see cref="PathTrackerBase.CalculateDistanceToTarget"/>
        protected override RCNumber CalculateDistanceToTarget()
        {
            /// Search the index of the node on the current path based on the position of the entity.
            RCNumVector currentPosition = this.ControlledEntity.PositionValue.Read();
            INavMeshNode currentNode = this.PathFinder.GetNavMeshNode(currentPosition);
            if (currentNode == null) { throw new InvalidOperationException("Entity position is out of the walkable area of the map!"); }
            int currentNodeIdxOnPath = this.CurrentPath.IndexOf(currentNode);
            if (currentNodeIdxOnPath == -1) { throw new InvalidOperationException("Followed path lost!"); }

            /// Check if the entity is at the last node on the path -> trivial case.
            if (currentNodeIdxOnPath == this.CurrentPath.Length - 1) { return MapUtils.ComputeDistance(currentPosition, this.CurrentPath.ToCoords); }

            /// In case of direct path check if the entity is at just 1 node before the last node on the path -> trivial case.
            if (this.CurrentPath.IsTargetFound && currentNodeIdxOnPath + 1 == this.CurrentPath.Length - 1) { return MapUtils.ComputeDistance(currentPosition, this.CurrentPath.ToCoords); }

            /// Calculate cumulative distance in the non-trivial cases.
            int lastMiddleNodeIdx = this.CurrentPath.Length - (this.CurrentPath.IsTargetFound ? 2 : 1);
            RCNumber distanceToTarget = MapUtils.ComputeDistance(currentPosition, this.CurrentPath[currentNodeIdxOnPath + 1].Polygon.Center);
            for (int i = currentNodeIdxOnPath + 1; i < lastMiddleNodeIdx; i++)
            {
                distanceToTarget += MapUtils.ComputeDistance(this.CurrentPath[i].Polygon.Center, this.CurrentPath[i + 1].Polygon.Center);
            }
            distanceToTarget += MapUtils.ComputeDistance(this.CurrentPath[lastMiddleNodeIdx].Polygon.Center, this.CurrentPath.ToCoords);
            return distanceToTarget;
        }

        /// <see cref="PathTrackerBase.ValidateVelocityImpl"/>
        protected override bool ValidateVelocityImpl(RCNumVector velocity)
        {
            INavMeshNode node = this.PathFinder.GetNavMeshNode(this.ControlledEntity.PositionValue.Read() + velocity);
            return node != null && this.CurrentPath.IndexOf(node) != -1;
        }

        #endregion PathTrackerBase overrides

        /// <summary>
        /// The range of the environment taken into account when collecting dynamic obstacles.
        /// </summary>
        private static readonly RCNumber ENVIRONMENT_SIGHT_RANGE = 5;
    }
}
