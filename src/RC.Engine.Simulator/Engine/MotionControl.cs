using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Core;
using RC.Engine.Simulator.MotionControl;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// Enumerates the possible states of this motion control.
    /// </summary>
    public enum MotionControlStatusEnum
    {
        Fixed = 0x00,      /// The owner of this motion control is currently on the ground and fixed in its current position.
        OnGround = 0x01,   /// The owner of this motion control is currently on the ground.
        TakingOff = 0x02,  /// The owner of this motion control is currently taking off.
        InAir = 0x03,      /// The owner of this motion control is currently in the air.
        Landing = 0x04     /// The owner of this motion control is currently landing.
    }

    /// <summary>
    /// Responsible for controlling the motion of a given entity.
    /// </summary>
    public class MotionControl : HeapedObject
    {
        /// <summary>
        /// Constructs a motion control instance for the given entity.
        /// </summary>
        /// <param name="owner">The owner of this motion control.</param>
        /// <param name="isFlying">A flag indicating whether this owner of this motion control is initially flying.</param>
        public MotionControl(Entity owner, bool isFlying)
        {
            if (owner == null) { throw new ArgumentNullException("owner"); }

            this.owner = this.ConstructField<Entity>("owner");
            this.position = this.ConstructField<RCNumVector>("position");
            this.velocity = this.ConstructField<RCNumVector>("velocity");
            this.status = this.ConstructField<byte>("status");
            this.vtolOperationProgress = this.ConstructField<RCNumber>("vtolOperationProgress");
            this.vtolInitialPosition = this.ConstructField<RCNumVector>("vtolInitialPosition");
            this.vtolFinalPosition = this.ConstructField<RCNumVector>("vtolFinalPosition");
            this.currentPathTracker = this.ConstructField<PathTrackerBase>("currentPathTracker");
            this.groundPathTracker = this.ConstructField<PathTrackerBase>("groundPathTracker");
            this.airPathTracker = this.ConstructField<PathTrackerBase>("airPathTracker");

            this.owner.Write(owner);
            this.position.Write(RCNumVector.Undefined);
            this.velocity.Write(new RCNumVector(0, 0));
            this.vtolOperationProgress.Write(0);
            this.vtolInitialPosition.Write(RCNumVector.Undefined);
            this.vtolFinalPosition.Write(RCNumVector.Undefined);
            this.groundPathTracker.Write(new GroundPathTracker(owner, TARGET_DISTANCE_THRESHOLD));
            this.airPathTracker.Write(new AirPathTracker(owner, TARGET_DISTANCE_THRESHOLD));
            if (isFlying)
            {
                this.status.Write((byte)MotionControlStatusEnum.InAir);
                this.currentPathTracker.Write(this.airPathTracker.Read());
                this.velocityGraph = new HexadecagonalVelocityGraph(owner.ElementType.Speed != null ? owner.ElementType.Speed.Read() : 0); // TODO: max speed might change based on upgrades!
            }
            else
            {
                this.status.Write((byte)MotionControlStatusEnum.OnGround);
                this.currentPathTracker.Write(this.groundPathTracker.Read());
                this.velocityGraph = new OctagonalVelocityGraph(owner.ElementType.Speed != null ? owner.ElementType.Speed.Read() : 0); // TODO: max speed might change based on upgrades!
            }
        }

        /// <summary>
        /// Gets the current status of this motion control.
        /// </summary>
        public MotionControlStatusEnum Status { get { return (MotionControlStatusEnum)this.status.Read(); } }

        /// <summary>
        /// Gets whether the owner of this motion control is currently performing a move operation or not.
        /// </summary>
        public bool IsMoving
        {
            get
            {
                if (this.position.Read() == RCNumVector.Undefined) { throw new InvalidOperationException("The owner is currently detached from the map!"); }
                return (this.Status == MotionControlStatusEnum.OnGround || this.Status == MotionControlStatusEnum.InAir) &&
                        this.currentPathTracker.Read().TargetPosition != RCNumVector.Undefined;
            }
        }

        /// <summary>
        /// Gets whether the owner of this motion control is currently in the air, is landing or is taking off.
        /// </summary>
        public bool IsFlying
        {
            get
            {
                if (this.position.Read() == RCNumVector.Undefined) { throw new InvalidOperationException("The owner is currently detached from the map!"); }
                return this.Status != MotionControlStatusEnum.OnGround && this.Status != MotionControlStatusEnum.Fixed;
            }
        }

        /// <summary>
        /// Gets the position vector managed by this motion control.
        /// </summary>
        public IValueRead<RCNumVector> PositionVector { get { return this.position; } }

        /// <summary>
        /// Gets the velocity vector managed by this motion control.
        /// </summary>
        public IValueRead<RCNumVector> VelocityVector
        {
            get
            {
                if (this.position.Read() == RCNumVector.Undefined) { throw new InvalidOperationException("The owner is currently detached from the map!"); }
                return this.velocity;
            }
        }

        /// <summary>
        /// Fixes the current position of the owner of this motion control. After calling this method the owner cannot move or takeoff until
        /// MotionControl.Unfix is called.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// If the status of this motion control is not MotionControlStatusEnum.Landed.
        /// </exception>
        public void Fix()
        {
            if (this.position.Read() == RCNumVector.Undefined) { throw new InvalidOperationException("The owner is currently detached from the map!"); }
            if (this.Status != MotionControlStatusEnum.OnGround) { throw new InvalidOperationException("The owner is currently not on the ground!"); }

            this.StopMoving();

            this.owner.Read().OnFixed();
            this.status.Write((byte)MotionControlStatusEnum.Fixed);
        }

        /// <summary>
        /// Unfixes the current position of the owner of this motion control. After calling this method the owner can move and takeoff again.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// If the status of this motion control is not MotionControlStatusEnum.Fixed.
        /// </exception>
        public void Unfix()
        {
            if (this.position.Read() == RCNumVector.Undefined) { throw new InvalidOperationException("The owner is currently detached from the map!"); }
            if (this.Status != MotionControlStatusEnum.Fixed) { throw new InvalidOperationException("The owner is currently not fixed on the ground!"); }

            this.owner.Read().OnUnfixed();
            this.status.Write((byte)MotionControlStatusEnum.OnGround);
        }

        /// <summary>
        /// Orders this motion control to start moving to the given position.
        /// </summary>
        /// <param name="toCoords">The target position.</param>
        /// <exception cref="InvalidOperationException">
        /// If the status of this motion control is neither MotionControlStatusEnum.OnGround nor MotionControlStatusEnum.InAir.
        /// </exception>
        public void StartMoving(RCNumVector toCoords)
        {
            if (toCoords == RCNumVector.Undefined) { throw new ArgumentNullException("toCoords"); }
            if (this.position.Read() == RCNumVector.Undefined) { throw new InvalidOperationException("The owner is currently detached from the map!"); }
            if (this.Status != MotionControlStatusEnum.OnGround && this.Status != MotionControlStatusEnum.InAir) { throw new InvalidOperationException("The owner is currently fixed on the ground, landing or taking off!"); }

            this.currentPathTracker.Read().TargetPosition = toCoords;
        }

        /// <summary>
        /// Orders this motion control to stop at its current position. If the status of this motion control is Fixed, TakingOff or Landing then this function has no effect.
        /// </summary>
        public void StopMoving()
        {
            if (this.position.Read() == RCNumVector.Undefined) { throw new InvalidOperationException("The owner is currently detached from the map!"); }

            if (this.Status == MotionControlStatusEnum.Fixed ||
                this.Status == MotionControlStatusEnum.TakingOff ||
                this.Status == MotionControlStatusEnum.Landing)
            {
                return;
            }
            this.currentPathTracker.Read().TargetPosition = RCNumVector.Undefined;
            this.velocity.Write(new RCNumVector(0, 0));
        }

        /// <summary>
        /// Begins the takeoff of the owner of this motion control.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the status of this motion control is not MotionControlStatusEnum.Fixed.</exception>
        /// <returns>True if the takeoff operation started successfully; otherwise false.</returns>
        public bool BeginTakeOff()
        {
            if (this.position.Read() == RCNumVector.Undefined) { throw new InvalidOperationException("The owner is currently detached from the map!"); }
            if (this.Status != MotionControlStatusEnum.Fixed) { throw new InvalidOperationException("The owner is currently not fixed!"); }

            /// Reserve the position on the map.
            RCNumVector positionAfterTakeOff = this.airPathTracker.Read().CalculateTargetPositionForVTOL(false);
            if (positionAfterTakeOff == RCNumVector.Undefined) { return false; }
            this.owner.Read().ReservePositionInAir(positionAfterTakeOff);

            /// Initialize the VTOL operation for take-off.
            this.Unfix();
            this.status.Write((byte)MotionControlStatusEnum.TakingOff);
            this.vtolOperationProgress.Write(0);
            this.vtolInitialPosition.Write(this.position.Read());
            this.vtolFinalPosition.Write(positionAfterTakeOff);
            this.currentPathTracker.Write(null);
            this.velocityGraph = null;

            return true;
        }

        /// <summary>
        /// Begins the landing of the owner of this motion control.
        /// </summary>
        /// <param name="landOnTheSpot">True to land without transition.</param>
        /// <exception cref="InvalidOperationException">If the status of this motion control is not MotionControlStatusEnum.Flying.</exception>
        /// <returns>True if the landing operation started successfully; otherwise false.</returns>
        public bool BeginLand(bool landOnTheSpot)
        {
            if (this.position.Read() == RCNumVector.Undefined) { throw new InvalidOperationException("The owner is currently detached from the map!"); }
            if (this.Status != MotionControlStatusEnum.InAir) { throw new InvalidOperationException("The owner is currently not in the air!"); }

            this.StopMoving();

            /// Reserve the position on the map.
            RCNumVector positionAfterLand = this.groundPathTracker.Read().CalculateTargetPositionForVTOL(landOnTheSpot);
            if (positionAfterLand == RCNumVector.Undefined) { return false; }
            this.owner.Read().ReservePositionOnGround(positionAfterLand);

            /// Initialize the VTOL operation for landing.
            this.status.Write((byte)MotionControlStatusEnum.Landing);
            this.vtolOperationProgress.Write(0);
            this.vtolInitialPosition.Write(this.position.Read());
            this.vtolFinalPosition.Write(positionAfterLand);
            this.currentPathTracker.Write(null);
            this.velocityGraph = null;

            return true;
        }

        /// <summary>
        /// Updates the state of this motion control.
        /// </summary>
        public void UpdateState()
        {
            /// Do nothing if the owner is detached from the map.
            if (this.position.Read() == RCNumVector.Undefined) { return; }

            if (this.Status == MotionControlStatusEnum.OnGround || this.Status == MotionControlStatusEnum.InAir)
            {
                /// Set the appropriate shadow transition on the map object of the owner.
                this.owner.Read().MapObject.SetShadowTransition(this.Status == MotionControlStatusEnum.InAir ? MAX_VTOL_TRANSITION_VECTOR : new RCNumVector(0, 0));

                /// Continue the current path-tracking if active.
                if (this.currentPathTracker.Read().IsActive)
                {
                    /// Get the current target position of the path-tracker.
                    RCNumVector targetPosition = this.currentPathTracker.Read().TargetPosition;

                    /// Update the state of the path-tracker.
                    this.currentPathTracker.Read().Update();

                    /// Update the velocity vector.
                    this.UpdateVelocity();

                    /// Update the position based on the new velocity.
                    this.UpdatePosition();

                    /// If the path-tracker finished working, check if we reached the threshold to the target position.
                    if (!this.currentPathTracker.Read().IsActive &&
                        MapUtils.ComputeDistance(this.position.Read(), targetPosition) < TARGET_DISTANCE_THRESHOLD)
                    {
                        /// If yes, move the entity exactly to the target position.
                        if (this.currentPathTracker.Read().ValidatePosition(targetPosition))
                        {
                            this.position.Write(targetPosition);
                        }
                    }
                }
            }
            else if (this.Status == MotionControlStatusEnum.TakingOff || this.Status == MotionControlStatusEnum.Landing)
            {
                /// Continue the current VTOL operation.
                bool finished = false;
                this.vtolOperationProgress.Write(this.vtolOperationProgress.Read() + VTOL_INCREMENT_PER_FRAME);
                if (this.vtolOperationProgress.Read() > 1)
                {
                    this.vtolOperationProgress.Write(1);
                    finished = true;
                }

                /// Calculate the new position vector and adjust the shadow transition of the owner's map object.
                this.position.Write(this.vtolInitialPosition.Read() * (1 - this.vtolOperationProgress.Read()) +
                                    this.vtolFinalPosition.Read() * this.vtolOperationProgress.Read());
                if (this.Status == MotionControlStatusEnum.TakingOff)
                {
                    this.owner.Read().MapObject.SetShadowTransition(MAX_VTOL_TRANSITION_VECTOR * this.vtolOperationProgress.Read());
                }
                else if (this.Status == MotionControlStatusEnum.Landing)
                {
                    this.owner.Read().MapObject.SetShadowTransition(MAX_VTOL_TRANSITION_VECTOR * (1 - this.vtolOperationProgress.Read()));
                }

                /// Finish the VTOL operation if its progress has reached 1.
                if (finished)
                {
                    this.FinishVTOLOperation();
                    if (this.Status == MotionControlStatusEnum.OnGround) { this.Fix(); }
                }
            }
        }

        /// <summary>
        /// This method is called when the owner entity is being attached to the map.
        /// </summary>
        /// <param name="toPosition">The target position.</param>
        /// <returns>True if the position has been set successfully; otherwise false.</returns>
        /// <exception cref="InvalidOperationException">
        /// If the status of this motion control is neither MotionControlStatusEnum.OnGround nor MotionControlStatusEnum.InAir.
        /// </exception>
        /// <remarks>The method is called from Entity.AttachToMap.</remarks>
        internal bool OnOwnerAttachingToMap(RCNumVector toPosition)
        {
            if (this.Status != MotionControlStatusEnum.OnGround && this.Status != MotionControlStatusEnum.InAir) { throw new InvalidOperationException("Invalid MotionControl state!"); }
            if (toPosition == RCNumVector.Undefined) { throw new ArgumentNullException("toPosition"); }

            if (this.currentPathTracker.Read().ValidatePosition(toPosition))
            {
                this.position.Write(toPosition);
                return true;
            }
            return false;
        }

        /// <summary>
        /// This method is called when the owner entity has been detached from the map. A call to this method will automatically stop any moving and VTOL operation.
        /// If a takeoff operation was in progress, then the status of this motion control will automatically be changed to MotionControlStatusEnum.Flying.
        /// If a landing operation was in progress, then the status of this motion control will automatically be changed to MotionControlStatusEnum.Landed.
        /// If neither a takeoff nor a landing operation was in progress, then the status of this motion control won't be changed.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the status of this motion control is MotionControlStatusEnum.Fixed.</exception>
        /// <remarks>The method is called from Entity.DetachFromMap.</remarks>
        internal void OnOwnerDetachedFromMap()
        {
            if (this.Status == MotionControlStatusEnum.Fixed) { throw new InvalidOperationException("The owner is currently fixed on the ground!"); }
            if (this.Status == MotionControlStatusEnum.TakingOff || this.Status == MotionControlStatusEnum.Landing) { this.FinishVTOLOperation(); }

            this.StopMoving();
            this.position.Write(RCNumVector.Undefined);
        }

        /// <see cref="HeapedObject.DisposeImpl"/>
        protected override void DisposeImpl()
        {
            this.groundPathTracker.Read().Dispose();
            this.airPathTracker.Read().Dispose();
            this.currentPathTracker.Write(null);
        }

        /// <summary>
        /// Updates the velocity vector.
        /// </summary>
        private void UpdateVelocity()
        {
            if (this.currentPathTracker.Read().IsActive)
            {
                /// Update the velocity of this entity if the path-tracker is still active.
                int currVelocityIndex = 0;
                int bestVelocityIndex = -1;
                RCNumber minPenalty = 0;
                List<RCNumVector> admissibleVelocities = new List<RCNumVector>(this.velocityGraph.GetAdmissibleVelocities(this.velocity.Read()));

                foreach (RCNumVector admissibleVelocity in admissibleVelocities)
                {
                    if (this.currentPathTracker.Read().ValidateVelocity(admissibleVelocity))
                    {
                        RCNumVector checkedVelocity = admissibleVelocity * 2 - this.velocity.Read(); /// We calculate with RVOs instead of VOs.
                        RCNumber timeToCollisionMin = -1;
                        foreach (DynamicObstacleInfo obstacle in this.currentPathTracker.Read().DynamicObstacles)
                        {
                            RCNumber timeToCollision = this.CalculateTimeToCollision(this.owner.Read().Area, checkedVelocity, obstacle.Position, obstacle.Velocity);
                            if (timeToCollision >= 0 && (timeToCollisionMin < 0 || timeToCollision < timeToCollisionMin)) { timeToCollisionMin = timeToCollision; }
                        }

                        if (timeToCollisionMin != 0)
                        {
                            RCNumber penalty = (timeToCollisionMin > 0 ? (RCNumber)1 / timeToCollisionMin : 0)
                                             + MapUtils.ComputeDistance(this.currentPathTracker.Read().PreferredVelocity, admissibleVelocity);
                            if (bestVelocityIndex == -1 || penalty < minPenalty)
                            {
                                minPenalty = penalty;
                                bestVelocityIndex = currVelocityIndex;
                            }
                        }
                    }
                    currVelocityIndex++;
                }

                /// Update the velocity based on the index of the best velocity.
                this.velocity.Write(bestVelocityIndex != -1 ? admissibleVelocities[bestVelocityIndex] : new RCNumVector(0, 0));
            }
            else
            {
                /// Stop the entity if the path-tracker became inactive.
                this.velocity.Write(new RCNumVector(0, 0));
            }
        }

        /// <summary>
        /// Updates the position vector.
        /// </summary>
        private void UpdatePosition()
        {
            if (this.velocity.Read() != new RCNumVector(0, 0))
            {
                RCNumVector newPosition = this.position.Read() + this.velocity.Read();
                if (this.currentPathTracker.Read().ValidatePosition(newPosition))
                {
                    this.position.Write(newPosition);
                }
                else
                {
                    this.velocity.Write(new RCNumVector(0, 0));
                }
            }
        }

        /// <summary>
        /// Finishes the current VTOL operation immediately.
        /// </summary>
        private void FinishVTOLOperation()
        {
            this.owner.Read().RemoveReservation();

            this.vtolOperationProgress.Write(0);
            this.vtolInitialPosition.Write(RCNumVector.Undefined);
            this.vtolFinalPosition.Write(RCNumVector.Undefined);
            if (this.Status == MotionControlStatusEnum.TakingOff)
            {
                this.owner.Read().MapObject.SetShadowTransition(MAX_VTOL_TRANSITION_VECTOR);
                this.status.Write((byte)MotionControlStatusEnum.InAir);
                this.currentPathTracker.Write(this.airPathTracker.Read());
                this.velocityGraph = new HexadecagonalVelocityGraph(this.owner.Read().ElementType.Speed.Read()); // TODO: max speed might change based on upgrades!
            }
            else
            {
                this.owner.Read().MapObject.SetShadowTransition(new RCNumVector(0, 0));
                this.status.Write((byte)MotionControlStatusEnum.OnGround);
                this.currentPathTracker.Write(this.groundPathTracker.Read());
                this.velocityGraph = new OctagonalVelocityGraph(this.owner.Read().ElementType.Speed.Read()); // TODO: max speed might change based on upgrades!
            }
        }

        /// <summary>
        /// Calculates the time to collision between two moving rectangular objects.
        /// </summary>
        /// <param name="rectangleA">The rectangular area of the first object.</param>
        /// <param name="velocityA">The velocity of the first object.</param>
        /// <param name="rectangleB">The rectangular area of the second object.</param>
        /// <param name="velocityB">The velocity of the second object.</param>
        /// <returns>The time to the collision between the two objects or a negative number if the two objects won't collide in the future.</returns>
        private RCNumber CalculateTimeToCollision(RCNumRectangle rectangleA, RCNumVector velocityA, RCNumRectangle rectangleB, RCNumVector velocityB)
        {
            /// Calculate the relative velocity of A with respect to B.
            RCNumVector relativeVelocityOfA = velocityA - velocityB;
            if (relativeVelocityOfA == new RCNumVector(0, 0)) { return -1; }

            /// Calculate the center of the first object and enlarge the area of the second object with the size of the first object.
            RCNumVector centerOfA = new RCNumVector((rectangleA.Left + rectangleA.Right) / 2,
                                                    (rectangleA.Top + rectangleA.Bottom) / 2);
            RCNumRectangle enlargedB = new RCNumRectangle(rectangleB.X - rectangleA.Width / 2,
                                                          rectangleB.Y - rectangleA.Height / 2,
                                                          rectangleA.Width + rectangleB.Width,
                                                          rectangleA.Height + rectangleB.Height);

            /// Calculate the collision time interval in the X dimension.
            bool isParallelX = relativeVelocityOfA.X == 0;
            RCNumber collisionTimeLeft = !isParallelX ? (enlargedB.Left - centerOfA.X) / relativeVelocityOfA.X : 0;
            RCNumber collisionTimeRight = !isParallelX ? (enlargedB.Right - centerOfA.X) / relativeVelocityOfA.X : 0;
            RCNumber collisionTimeBeginX = !isParallelX ? (collisionTimeLeft < collisionTimeRight ? collisionTimeLeft : collisionTimeRight) : 0;
            RCNumber collisionTimeEndX = !isParallelX ? (collisionTimeLeft > collisionTimeRight ? collisionTimeLeft : collisionTimeRight) : 0;

            /// Calculate the collision time interval in the Y dimension.
            bool isParallelY = relativeVelocityOfA.Y == 0;
            RCNumber collisionTimeTop = !isParallelY ? (enlargedB.Top - centerOfA.Y) / relativeVelocityOfA.Y : 0;
            RCNumber collisionTimeBottom = !isParallelY ? (enlargedB.Bottom - centerOfA.Y) / relativeVelocityOfA.Y : 0;
            RCNumber collisionTimeBeginY = !isParallelY ? (collisionTimeTop < collisionTimeBottom ? collisionTimeTop : collisionTimeBottom) : 0;
            RCNumber collisionTimeEndY = !isParallelY ? (collisionTimeTop > collisionTimeBottom ? collisionTimeTop : collisionTimeBottom) : 0;

            if (!isParallelX && !isParallelY)
            {
                /// Both X and Y dimensions have finite collision time interval.
                if (collisionTimeBeginX <= collisionTimeBeginY && collisionTimeBeginY < collisionTimeEndX && collisionTimeEndX <= collisionTimeEndY)
                {
                    return collisionTimeBeginY;
                }
                else if (collisionTimeBeginY <= collisionTimeBeginX && collisionTimeBeginX < collisionTimeEndX && collisionTimeEndX <= collisionTimeEndY)
                {
                    return collisionTimeBeginX;
                }
                else if (collisionTimeBeginY <= collisionTimeBeginX && collisionTimeBeginX < collisionTimeEndY && collisionTimeEndY <= collisionTimeEndX)
                {
                    return collisionTimeBeginX;
                }
                else if (collisionTimeBeginX <= collisionTimeBeginY && collisionTimeBeginY < collisionTimeEndY && collisionTimeEndY <= collisionTimeEndX)
                {
                    return collisionTimeBeginY;
                }
                else
                {
                    return -1;
                }
            }
            else if (!isParallelX && isParallelY)
            {
                /// Only X dimension has finite collision time interval.
                return centerOfA.Y > enlargedB.Top && centerOfA.Y < enlargedB.Bottom ? collisionTimeBeginX : -1;
            }
            else if (isParallelX && !isParallelY)
            {
                /// Only Y dimension has finite collision time interval.
                return centerOfA.X > enlargedB.Left && centerOfA.X < enlargedB.Right ? collisionTimeBeginY : -1;
            }
            else
            {
                /// None of the dimensions have finite collision time interval.
                return -1;
            }
        }

        /// <summary>
        /// Reference to the owner of this motion control.
        /// </summary>
        private readonly HeapedValue<Entity> owner;

        /// <summary>
        /// The position of the owner of this motion control or RCNumVector.Undefined if this owner is detached from the map.
        /// </summary>
        private readonly HeapedValue<RCNumVector> position;

        /// <summary>
        /// The velocity of the owner of this motion control.
        /// </summary>
        private readonly HeapedValue<RCNumVector> velocity;

        /// <summary>
        /// This byte indicates the current status of this motion control. The possible values of this byte is defined in MotionControlStatusEnum.
        /// </summary>
        private readonly HeapedValue<byte> status;

        /// <summary>
        /// The initial position of the current VTOL operation or RCNumVector.Undefined if there is no VTOL operation currently in progress.
        /// </summary>
        private readonly HeapedValue<RCNumVector> vtolInitialPosition;

        /// <summary>
        /// The final position of the current VTOL operation or RCNumVector.Undefined if there is no VTOL operation currently in progress.
        /// </summary>
        private readonly HeapedValue<RCNumVector> vtolFinalPosition;

        /// <summary>
        /// A value between 0 and 1 that indicates the progress of the current VTOL operation. A value of 0 means that the current VTOL operation has
        /// just begun while a value of 1 means that it has completely finished.
        /// </summary>
        private readonly HeapedValue<RCNumber> vtolOperationProgress;

        /// <summary>
        /// Reference to the current path-tracker of this motion control or null if a VTOL operation is currently in progress or the owner is fixed.
        /// </summary>
        private readonly HeapedValue<PathTrackerBase> currentPathTracker;

        /// <summary>
        /// Reference to the ground path-tracker of this motion control.
        /// </summary>
        private readonly HeapedValue<PathTrackerBase> groundPathTracker;

        /// <summary>
        /// Reference to the air path-tracker of this motion control.
        /// </summary>
        private readonly HeapedValue<PathTrackerBase> airPathTracker;

        /// <summary>
        /// Reference to the velocity graph used by this motion control or null if a VTOL operation is currently in progress.
        /// </summary>
        private VelocityGraph velocityGraph;

        /// <summary>
        /// The increment of VTOL operation progress per frames.
        /// </summary>
        private static readonly RCNumber VTOL_INCREMENT_PER_FRAME = (RCNumber)1/(RCNumber)72;

        /// <summary>
        /// The shadow transition vector belonging to the maximum value of transition during a VTOL operation.
        /// </summary>
        private static readonly RCNumVector MAX_VTOL_TRANSITION_VECTOR = new RCNumVector(0, Constants.MAX_VTOL_TRANSITION);

        /// <summary>
        /// If the controlled entity is closer to the target position than this threshold, it is considered to arrive.
        /// </summary>
        private static readonly RCNumber TARGET_DISTANCE_THRESHOLD = (RCNumber)1 / (RCNumber)2;
    }
}
