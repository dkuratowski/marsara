using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Commands;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.Terran.Buildings;
using RC.Engine.Simulator.Terran.Units;

namespace RC.Engine.Simulator.Terran.Commands
{
    /// <summary>
    /// Enumerates the possible states of an SCV build execution.
    /// </summary>
    enum SCVBuildExecutionStatusEnum
    {
        MovingToTarget = 0x00,      /// The SCV is currently moving towards the target.
        Constructing = 0x01,        /// The SCV is currently constructing.
        MovingToFreePlace = 0x02    /// The SCV has finished construction and is trying to move to a free place.
    }

    /// <summary>
    /// The common base class of SCV build executions.
    /// </summary>
    abstract class SCVBuildExecutionBase : CmdExecutionBase
    {
        #region Overrides

        /// <see cref="CmdExecutionBase.ContinueImpl"/>
        protected override bool ContinueImpl()
        {
            if (this.Status == SCVBuildExecutionStatusEnum.MovingToTarget)
            {
                /// Check if we have to do anything in this frame.
                if (this.timeSinceLastDistanceCheck.Read() < TIME_BETWEEN_DISTANCE_CHECKS)
                {
                    /// Nothing to do now.
                    this.timeSinceLastDistanceCheck.Write(this.timeSinceLastDistanceCheck.Read() + 1);
                    return false;
                }

                /// Perform a state refresh in this frame.
                this.timeSinceLastDistanceCheck.Write(0);

                return this.ContinueMovingToTarget();
            }
            else if (this.Status == SCVBuildExecutionStatusEnum.Constructing)
            {
                return this.ContinueConstructing();
            }
            else if (this.Status == SCVBuildExecutionStatusEnum.MovingToFreePlace)
            {
                /// Wait while the SCV is still moving.
                return !this.recipientSCV.Read().MotionControl.IsMoving;
            }
            else
            {
                throw new InvalidOperationException("Impossible case happened!");
            }
        }

        /// <see cref="CmdExecutionBase.GetContinuation"/>
        protected override CmdExecutionBase GetContinuation()
        {
            if (this.Status == SCVBuildExecutionStatusEnum.MovingToTarget)
            {
                return new MoveExecution(this.recipientSCV.Read(), this.targetPosition.Read(), -1);
            }
            else
            {
                return new StopExecution(this.recipientSCV.Read());
            }
        }

        #endregion Overrides

        /// <summary>
        /// Constructs an SCVBuildExecutionBase instance.
        /// </summary>
        /// <param name="recipientSCV">The recipient SCV of this command execution.</param>
        protected SCVBuildExecutionBase(SCV recipientSCV)
            : base(new RCSet<Entity> { recipientSCV })
        {
            this.targetPosition = this.ConstructField<RCNumVector>("targetPosition");
            this.recipientSCV = this.ConstructField<SCV>("recipientSCV");
            this.status = this.ConstructField<byte>("status");
            this.timeToNextScvMoveDuringConstruction = this.ConstructField<int>("timeToNextScvMoveDuringConstruction");
            this.timeSinceLastDistanceCheck = this.ConstructField<int>("timeSinceLastDistanceCheck");

            this.targetPosition.Write(RCNumVector.Undefined);
            this.recipientSCV.Write(recipientSCV);
            this.Status = SCVBuildExecutionStatusEnum.MovingToTarget;
            this.timeToNextScvMoveDuringConstruction.Write(0);
            this.timeSinceLastDistanceCheck.Write(0);
        }

        /// <summary>
        /// Gets the recipient SCV of this command execution.
        /// </summary>
        protected SCV RecipientSCV { get { return this.recipientSCV.Read(); } }

        /// <summary>
        /// Gets or sets the target position of this command execution.
        /// </summary>
        protected RCNumVector TargetPosition
        {
            get { return this.targetPosition.Read(); }
            set { this.targetPosition.Write(value); }
        }

        /// <summary>
        /// Sets or gets the status of this command execution.
        /// </summary>
        protected SCVBuildExecutionStatusEnum Status
        {
            get { return (SCVBuildExecutionStatusEnum) this.status.Read(); }
            set { this.status.Write((byte)value); }
        }

        /// <summary>
        /// Continues the execution of this command in the MOVING_TO_TARGET state. This method shall be overriden in the derived
        /// classes.
        /// </summary>
        /// <returns>True if this command execution has been finished; otherwise false.</returns>
        protected abstract bool ContinueMovingToTarget();

        ///// <summary>
        ///// Gets the constructed building.
        ///// </summary>
        //protected abstract TerranBuilding GetConstructedBuilding();

        #region Internal construction management methods

        /// <summary>
        /// Continues the execution of this command in the CONSTRUCTING state.
        /// </summary>
        private bool ContinueConstructing()
        {
            if (this.recipientSCV.Read().ContinueConstruct())
            {
                /// The SCV has finished the construction -> stop using the build tool, move the SCV to a free place and finish
                ///                                          this command execution.
                this.recipientSCV.Read().Armour.StopAttack();
                this.MoveScvToFreePlace();
                this.Status = SCVBuildExecutionStatusEnum.MovingToFreePlace;
                return false;
            }

            /// Continue the construction activity of the SCV.
            this.MakeScvActivityDuringConstruction();
            return false;
        }

        /// <summary>
        /// Do SCV activities during construction.
        /// </summary>
        private void MakeScvActivityDuringConstruction()
        {
            /// Start using the build tool of the SCV and decrease the move-timer.
            TerranBuilding constructedBuilding = this.RecipientSCV.ConstructionJob.ConstructedBuilding;
            if (this.timeToNextScvMoveDuringConstruction.Read() > 0)
            {
                if (!this.recipientSCV.Read().MotionControl.IsMoving && this.recipientSCV.Read().Armour.Target == null)
                {
                    this.recipientSCV.Read().Armour.StartAttack(constructedBuilding.ID.Read(), SCV.SCV_BUILD_TOOL_NAME);
                }
                this.timeToNextScvMoveDuringConstruction.Write(this.timeToNextScvMoveDuringConstruction.Read() - 1);
                return;
            }

            /// Generate a random position inside the building area and move the SCV there.
            /// TODO: do not use the default random generator because the engine shall be deterministic!
            RCIntRectangle buildingQuadRect = constructedBuilding.MapObject.QuadraticPosition;
            RCIntRectangle buildingCellRect = this.Scenario.Map.QuadToCellRect(buildingQuadRect);
            RCNumVector movePosition = new RCNumVector(
                RandomService.DefaultGenerator.Next(buildingCellRect.Left, buildingCellRect.Right),
                RandomService.DefaultGenerator.Next(buildingCellRect.Top, buildingCellRect.Bottom));
            this.recipientSCV.Read().MotionControl.StartMoving(movePosition);
            this.recipientSCV.Read().Armour.StopAttack();

            /// Reset the timer.
            this.timeToNextScvMoveDuringConstruction.Write(TIME_BETWEEN_SCV_MOVES);
        }

        /// <summary>
        /// Moves the SCV to a free place if construction finished.
        /// </summary>
        private void MoveScvToFreePlace()
        {
            /// Search for an entity that is behind the recipient SCV. Do nothing if there is no such entity.
            Entity overlappingEntity =
                this.Scenario.GetElementsOnMap<Entity>(this.RecipientSCV.MotionControl.PositionVector.Read(),
                    MapObjectLayerEnum.GroundObjects).FirstOrDefault(entity => entity != this.RecipientSCV);
            if (overlappingEntity == null) { return; }

            /// Otherwise move the SCV to the bottom-left corner of the overlapping entity.
            RCIntRectangle buildingQuadRect = overlappingEntity.MapObject.QuadraticPosition;
            RCIntRectangle buildingCellRect = this.Scenario.Map.QuadToCellRect(buildingQuadRect);
            this.recipientSCV.Read().MotionControl.StartMoving(new RCNumVector(buildingCellRect.Left, buildingCellRect.Bottom - 1));
        }

        #endregion Internal construction management methods

        /// <summary>
        /// The target position on the map where the SCV shall move to start/continue the construction.
        /// </summary>
        private readonly HeapedValue<RCNumVector> targetPosition;

        /// <summary>
        /// Reference to the recipient SCV of this command execution.
        /// </summary>
        private readonly HeapedValue<SCV> recipientSCV;

        /// <summary>
        /// The current status of this command execution.
        /// </summary>
        private readonly HeapedValue<byte> status;

        /// <summary>
        /// Time to the next SCV move operation during construction.
        /// </summary>
        private readonly HeapedValue<int> timeToNextScvMoveDuringConstruction;

        /// <summary>
        /// The elapsed time since last distance check operation.
        /// </summary>
        private readonly HeapedValue<int> timeSinceLastDistanceCheck;

        /// <summary>
        /// The time between distance check operations.
        /// </summary>
        private const int TIME_BETWEEN_DISTANCE_CHECKS = 12;

        /// <summary>
        /// The number of frames between SCV move operations during construction.
        /// </summary>
        private const int TIME_BETWEEN_SCV_MOVES = 60;
    }
}
