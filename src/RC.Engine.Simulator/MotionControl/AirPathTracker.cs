using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Engine;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// The path-tracker implementation for entities in the air.
    /// </summary>
    public class AirPathTracker : PathTrackerBase
    {
        /// <summary>
        /// Constructs a AirPathTracker instance.
        /// </summary>
        /// <param name="controlledEntity">The entity that this path tracker controls.</param>
        public AirPathTracker(Entity controlledEntity) : base(controlledEntity) { }

        #region PathTrackerBase overrides

        /// <see cref="PathTrackerBase.IsActive"/>
        public override bool IsActive { get { return this.TargetPosition != RCNumVector.Undefined; } }

        /// <see cref="PathTrackerBase.CalculateTargetPositionForVTOL"/>
        public override RCNumVector CalculateTargetPositionForVTOL(bool vtolOnTheSpot)
        {
            if (vtolOnTheSpot)
            {
                /// Taking-off on the spot.
                return this.ValidatePosition(this.ControlledEntity.MotionControl.PositionVector.Read()) ?
                    this.ControlledEntity.MotionControl.PositionVector.Read() :
                    RCNumVector.Undefined;
            }

            /// Normal take-off.
            RCNumber topToMapEdgeDistance = this.ControlledEntity.Area.Top + (RCNumber)1 / (RCNumber)2;
            if (topToMapEdgeDistance < 0) { return RCNumVector.Undefined; }

            RCNumber transitionValue = topToMapEdgeDistance <= MAX_VTOL_TRANSITION ? topToMapEdgeDistance : MAX_VTOL_TRANSITION;
            RCNumVector positionAfterTakeOff = this.ControlledEntity.MotionControl.PositionVector.Read() - new RCNumVector(0, transitionValue);
            return this.ValidatePosition(positionAfterTakeOff) ? positionAfterTakeOff : RCNumVector.Undefined;
        }

        /// <see cref="PathTrackerBase.ValidatePosition"/>
        public override bool ValidatePosition(RCNumVector position)
        {
            if (position == RCNumVector.Undefined) { throw new ArgumentNullException("position"); }

            /// Detect collision with other entities in the air.
            RCNumRectangle newEntityArea = this.ControlledEntity.CalculateArea(position);
            if (this.ControlledEntity.Scenario.GetElementsOnMap<Entity>(newEntityArea, MapObjectLayerEnum.AirObjects)
                .Any(
                    collidingEntity =>
                        collidingEntity != this.ControlledEntity &&
                        collidingEntity.MotionControl.Status == MotionControlStatusEnum.InAir))
            {
                return false;
            }

            /// Detect collision with entity reservations in the air.
            return this.ControlledEntity.Scenario.GetMapObjects(newEntityArea, MapObjectLayerEnum.AirReservations).Count == 0;
        }

        /// <see cref="PathTrackerBase.CalculatePreferredVelocity"/>
        protected override RCNumVector CalculatePreferredVelocity()
        {
            return this.TargetPosition - this.ControlledEntity.MotionControl.PositionVector.Read();
        }

        /// <see cref="PathTrackerBase.CollectNearbyDynamicObstacles"/>
        protected override IEnumerable<DynamicObstacleInfo> CollectNearbyDynamicObstacles()
        {
            List<DynamicObstacleInfo> retList = new List<DynamicObstacleInfo>();
            RCSet<Entity> entitiesInRange = this.ControlledEntity.Locator.SearchNearbyEntities(ENVIRONMENT_SIGHT_RANGE);
            foreach (Entity entityInRange in entitiesInRange.Where(entity => entity.MotionControl.IsFlying))
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
            return MapUtils.ComputeDistance(this.ControlledEntity.MotionControl.PositionVector.Read(), this.TargetPosition);
        }

        /// <see cref="PathTrackerBase.ValidateVelocityImpl"/>
        protected override bool ValidateVelocityImpl(RCNumVector velocity)
        {
            RCIntVector nextCell = (this.ControlledEntity.MotionControl.PositionVector.Read() + velocity).Round();
            return nextCell.X >= 0 && nextCell.X < this.ControlledEntity.Scenario.Map.CellSize.X &&
                   nextCell.Y >= 0 && nextCell.Y < this.ControlledEntity.Scenario.Map.CellSize.Y;
        }

        #endregion PathTrackerBase overrides

        /// <summary>
        /// The range of the environment in quadratic tiles taken into account when collecting dynamic obstacles.
        /// </summary>
        private const int ENVIRONMENT_SIGHT_RANGE = 2;
    }
}
