using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Core;
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
        /// <param name="targetDistanceThreshold">If the controlled entity is closer to the target position than this threshold, it is considered to arrive.</param>
        public AirPathTracker(Entity controlledEntity, RCNumber targetDistanceThreshold) : base(controlledEntity, targetDistanceThreshold) { }

        #region PathTrackerBase overrides

        /// <see cref="PathTrackerBase.IsActive"/>
        public override bool IsActive { get { return this.TargetPosition != RCNumVector.Undefined; } }

        /// <see cref="PathTrackerBase.CalculateTargetPositionForVTOL"/>
        public override RCNumVector CalculateTargetPositionForVTOL(bool vtolOnTheSpot)
        {
            if (vtolOnTheSpot)
            {
                /// Taking-off on the spot.
                return this.ValidateMove(RCNumVector.Undefined, this.ControlledEntity.MotionControl.PositionVector.Read()) ?
                    this.ControlledEntity.MotionControl.PositionVector.Read() :
                    RCNumVector.Undefined;
            }

            /// Normal take-off.
            RCNumber topToMapEdgeDistance = this.ControlledEntity.Area.Top + (RCNumber)1 / (RCNumber)2;
            if (topToMapEdgeDistance < 0) { return RCNumVector.Undefined; }

            RCNumber transitionValue = topToMapEdgeDistance <= Constants.MAX_VTOL_TRANSITION ? topToMapEdgeDistance : Constants.MAX_VTOL_TRANSITION;
            RCNumVector positionAfterTakeOff = this.ControlledEntity.MotionControl.PositionVector.Read() - new RCNumVector(0, transitionValue);
            return this.ValidateMove(RCNumVector.Undefined, positionAfterTakeOff) ? positionAfterTakeOff : RCNumVector.Undefined;
        }

        /// <see cref="PathTrackerBase.ValidateMove"/>
        public override bool ValidateMove(RCNumVector fromPosition, RCNumVector toPosition)
        {
            if (toPosition == RCNumVector.Undefined) { throw new ArgumentNullException("toPosition"); }

            bool collisionAtStart = false;
            bool collisionAtEnd = true;
            if (fromPosition != RCNumVector.Undefined)
            {
                /// Detect collision with other entities at the starting position of the move.
                RCNumRectangle newEntityArea = this.ControlledEntity.CalculateArea(fromPosition);
                collisionAtStart =
                    this.ControlledEntity.Scenario.GetElementsOnMap<Entity>(newEntityArea, MapObjectLayerEnum.AirObjects,
                        MapObjectLayerEnum.AirReservations)
                        .Any(
                            collidingEntity =>
                                collidingEntity != this.ControlledEntity &&
                                !this.ControlledEntity.IsOverlapEnabled(collidingEntity) &&
                                !collidingEntity.IsOverlapEnabled(this.ControlledEntity));
            }

            if (toPosition != fromPosition)
            {
                /// Detect collision with other entities at the target position of the move.
                RCNumRectangle newEntityArea = this.ControlledEntity.CalculateArea(toPosition);
                collisionAtEnd =
                    this.ControlledEntity.Scenario.GetElementsOnMap<Entity>(newEntityArea, MapObjectLayerEnum.AirObjects,
                        MapObjectLayerEnum.AirReservations)
                        .Any(
                            collidingEntity =>
                                collidingEntity != this.ControlledEntity &&
                                !this.ControlledEntity.IsOverlapEnabled(collidingEntity) &&
                                !collidingEntity.IsOverlapEnabled(this.ControlledEntity));
            }

            /// The controlled entity shall collide at the starting position or not collide at either endpoints of the move.
            return collisionAtStart || !collisionAtEnd;
        }

        /// <see cref="PathTrackerBase.CalculatePreferredVelocity"/>
        protected override RCNumVector CalculatePreferredVelocity()
        {
            return this.TargetPosition - this.ControlledEntity.MotionControl.PositionVector.Read();
        }

        /// <see cref="PathTrackerBase.IsObstacle"/>
        protected override bool IsObstacle(Entity entity)
        {
            return entity.MotionControl.IsFlying;
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
    }
}
