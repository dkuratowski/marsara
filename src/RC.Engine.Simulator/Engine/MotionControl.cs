using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Core;
using RC.Engine.Simulator.MotionControl;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Common.Diagnostics;

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
            this.groundPathTracker.Write(new GroundPathTracker(owner));
            this.airPathTracker.Write(new AirPathTracker(owner));
            if (isFlying)
            {
                this.SetStatus(MotionControlStatusEnum.InAir);
                this.currentPathTracker.Write(this.airPathTracker.Read());
            }
            else
            {
                this.SetStatus(MotionControlStatusEnum.OnGround);
                this.currentPathTracker.Write(this.groundPathTracker.Read());
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
                        this.currentPathTracker.Read().NextPosition != RCNumVector.Undefined;
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
            this.SetStatus(MotionControlStatusEnum.Fixed);
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
            this.SetStatus(MotionControlStatusEnum.OnGround);
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

            this.currentPathTracker.Read().Activate(toCoords);
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
            this.currentPathTracker.Read().Deactivate();
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

            /// Calculate position after take-off.
            RCNumber topToMapEdgeDistance = this.owner.Read().Area.Top + (RCNumber)1 / (RCNumber)2;
            if (topToMapEdgeDistance < 0) { return false; }
            RCNumber transitionValue = topToMapEdgeDistance <= Constants.MAX_VTOL_TRANSITION ? topToMapEdgeDistance : Constants.MAX_VTOL_TRANSITION;
            RCNumVector positionAfterTakeOff = this.owner.Read().MotionControl.PositionVector.Read() - new RCNumVector(0, transitionValue);

            /// Try to reserve the position in the air and remove the reservation from the ground.
            if (!this.airPathTracker.Read().OnAttaching(positionAfterTakeOff)) { return false; }
            this.groundPathTracker.Read().OnDetached();

            /// Initialize the VTOL operation for take-off.
            this.Unfix();
            this.SetStatus(MotionControlStatusEnum.TakingOff);
            this.vtolOperationProgress.Write(0);
            this.vtolInitialPosition.Write(this.position.Read());
            this.vtolFinalPosition.Write(positionAfterTakeOff);
            this.currentPathTracker.Write(null);
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

            /// Calculate position after land.
            RCNumVector positionAfterLand = RCNumVector.Undefined;
            if (landOnTheSpot)
            {
                /// Landing on the spot.
                positionAfterLand = this.position.Read();
            }
            else
            {
                /// Normal landing.
                RCNumber bottomToMapEdgeDistance = this.owner.Read().Scenario.Map.CellSize.Y + (RCNumber)1 / (RCNumber)2
                                                 - this.owner.Read().Area.Bottom;
                if (bottomToMapEdgeDistance < 0) { return false; }

                RCNumber transitionValue = bottomToMapEdgeDistance <= Constants.MAX_VTOL_TRANSITION ? bottomToMapEdgeDistance : Constants.MAX_VTOL_TRANSITION;
                positionAfterLand = this.position.Read() + new RCNumVector(0, transitionValue);
            }

            /// Try to reserve the position on the ground and remove the reservation from the air.
            if (!this.groundPathTracker.Read().OnAttaching(positionAfterLand)) { return false; }
            this.airPathTracker.Read().OnDetached();

            /// Initialize the VTOL operation for landing.
            this.SetStatus(MotionControlStatusEnum.Landing);
            this.vtolOperationProgress.Write(0);
            this.vtolInitialPosition.Write(this.position.Read());
            this.vtolFinalPosition.Write(positionAfterLand);
            this.currentPathTracker.Write(null);

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
                if (this.currentPathTracker.Read().NextPosition != RCNumVector.Undefined)
                {
                    /// Update the position and velocity vectors and the state of the path-tracker.
                    this.UpdateVectors(this.currentPathTracker.Read().NextPosition);
                    this.currentPathTracker.Read().Update();
                }
                else
                {
                    /// Otherwise set the current velocity vector to (0;0).
                    this.velocity.Write(new RCNumVector(0, 0));
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
                this.SetPosition(this.vtolInitialPosition.Read() * (1 - this.vtolOperationProgress.Read()) +
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

            if (this.currentPathTracker.Read().OnAttaching(toPosition))
            {
                this.SetPosition(toPosition);
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
            this.currentPathTracker.Read().OnDetached();
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
        /// Updates the position and the velocity vectors.
        /// </summary>
        private void UpdateVectors(RCNumVector newPosition)
        {
            this.velocity.Write(newPosition - this.position.Read());
            this.SetPosition(newPosition);
        }

        /// <summary>
        /// Finishes the current VTOL operation immediately.
        /// </summary>
        private void FinishVTOLOperation()
        {
            this.vtolOperationProgress.Write(0);
            this.vtolInitialPosition.Write(RCNumVector.Undefined);
            this.vtolFinalPosition.Write(RCNumVector.Undefined);
            if (this.Status == MotionControlStatusEnum.TakingOff)
            {
                this.owner.Read().MapObject.SetShadowTransition(MAX_VTOL_TRANSITION_VECTOR);
                this.SetStatus(MotionControlStatusEnum.InAir);
                this.currentPathTracker.Write(this.airPathTracker.Read());
            }
            else
            {
                this.owner.Read().MapObject.SetShadowTransition(new RCNumVector(0, 0));
                this.SetStatus(MotionControlStatusEnum.OnGround);
                this.currentPathTracker.Write(this.groundPathTracker.Read());
            }
        }

        /// <summary>
        /// Helper method for setting the position vector and updating the map object of the owner entity.
        /// </summary>
        /// <param name="newPosition">The new value of the position vector.</param>
        private void SetPosition(RCNumVector newPosition)
        {
            if (newPosition == RCNumVector.Undefined) { throw new InvalidOperationException("Undefined position!"); }

            this.position.Write(newPosition);
            if (this.owner.Read().MapObject != null)
            {
                this.owner.Read().MapObject.SetLocation(this.owner.Read().Area);
            }
        }

        /// <summary>
        /// Helper method for setting the status of this motion control and the layer of the map object of the owner entity.
        /// </summary>
        /// <param name="newStatus">The new status of this motion control</param>
        private void SetStatus(MotionControlStatusEnum newStatus)
        {
            if (this.Status != newStatus)
            {
                this.status.Write((byte)newStatus);
                if (this.owner.Read().MapObject != null)
                {
                    this.owner.Read().ChangeMapObjectLayer(this.owner.Read().MapObject, this.IsFlying ? MapObjectLayerEnum.AirObjects : MapObjectLayerEnum.GroundObjects);
                }
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
        /// The increment of VTOL operation progress per frames.
        /// </summary>
        private static readonly RCNumber VTOL_INCREMENT_PER_FRAME = (RCNumber)1/(RCNumber)72;

        /// <summary>
        /// The shadow transition vector belonging to the maximum value of transition during a VTOL operation.
        /// </summary>
        private static readonly RCNumVector MAX_VTOL_TRANSITION_VECTOR = new RCNumVector(0, Constants.MAX_VTOL_TRANSITION);
    }
}
