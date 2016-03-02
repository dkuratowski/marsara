using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.Commands;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.Terran.Units;

namespace RC.Engine.Simulator.Terran.Commands
{
    /// <summary>
    /// Responsible for executing repair commands for SCVs.
    /// </summary>
    class SCVRepairExecution : CmdExecutionBase
    {
        /// <summary>
        /// Creates an SCVRepairExecution instance.
        /// </summary>
        /// <param name="recipientSCV">The recipient SCV of this command execution.</param>
        /// <param name="targetPosition">The target position.</param>
        /// <param name="targetEntityID">The ID of the entity to repair.</param>
        public SCVRepairExecution(SCV recipientSCV, RCNumVector targetPosition, int targetEntityID)
            : base(new RCSet<Entity> {recipientSCV})
        {
            if (targetEntityID == -1) { throw new ArgumentOutOfRangeException("targetEntityID", "Target entity ID cannot be negative!"); }

            this.targetPosition = this.ConstructField<RCNumVector>("targetPosition");
            this.targetEntityID = this.ConstructField<int>("targetEntityID");
            this.recipientSCV = this.ConstructField<SCV>("recipientSCV");
            this.targetEntity = this.ConstructField<Entity>("targetEntity");
            this.timeSinceLastCheck = this.ConstructField<int>("timeSinceLastCheck");
            this.isRepairing = this.ConstructField<byte>("isRepairing");
            this.targetPosition.Write(targetPosition);
            this.targetEntityID.Write(targetEntityID);
            this.recipientSCV.Write(recipientSCV);
            this.targetEntity.Write(null);
            this.timeSinceLastCheck.Write(0);
            this.isRepairing.Write(0x00);
        }

        #region Overrides

        /// <see cref="CmdExecutionBase.InitializeImpl"/>
        protected override bool ContinueImpl()
        {
            if (this.isRepairing.Read() == 0x01)
            {
                /// In repairing state.

                /// Check if the target entity is still can be located.
                this.targetEntity.Write(this.LocateEntity(this.targetEntityID.Read()));
                if (this.targetEntity.Read() == null)
                {
                    /// Target entity could not be located anymore -> finish this command execution.
                    return true;
                }
                
                /// Check if the target entity is still being repaired.
                if (this.recipientSCV.Read().Armour.Target == null)
                {
                    /// Stopped repairing target entity -> start approaching if still damaged, finish command execution otherwise.
                    if (this.targetEntity.Read().Biometrics.HP == this.targetEntity.Read().ElementType.MaxHP.Read())
                    {
                        return true;
                    }
                    this.recipientSCV.Read().MotionControl.StartMoving(this.targetEntity.Read().MotionControl.PositionVector.Read());
                    this.isRepairing.Write(0x00);
                    return false;
                }
                else
                {
                    /// Still repairing target entity -> Perform a repair step on the target entity and finish this command execution if performing
                    ///                                  the repair step failed.
                    return !this.targetEntity.Read().Biometrics.Repair();
                }
            }
            else
            {
                /// In moving state.

                /// Check if we have to do anything in this frame.
                if (this.timeSinceLastCheck.Read() < TIME_BETWEEN_DISTANCE_CHECKS)
                {
                    /// Nothing to do now.
                    this.timeSinceLastCheck.Write(this.timeSinceLastCheck.Read() + 1);
                    return false;
                }

                /// Perform a state refresh in this frame.
                this.timeSinceLastCheck.Write(0);

                /// Check if we still have a target entity.
                if (this.targetEntity.Read() == null)
                {
                    /// Target lost -> finish command execution if the current move operation has finished.
                    return !this.recipientSCV.Read().MotionControl.IsMoving;
                }

                /// Check if the target entity still can be located.
                this.targetEntity.Write(this.LocateEntity(this.targetEntityID.Read()));
                if (this.targetEntity.Read() == null)
                {
                    /// Target could not be located anymore -> finish command execution if the current move operation has finished.
                    return !this.recipientSCV.Read().MotionControl.IsMoving;
                }

                /// Target entity still can be located -> Start attack with the repair tool.
                this.recipientSCV.Read().Armour.StartAttack(this.targetEntityID.Read(), SCV.SCV_REPAIR_TOOL_NAME);
                if (this.recipientSCV.Read().Armour.Target != null)
                {
                    /// Repair started -> stop moving.
                    this.recipientSCV.Read().MotionControl.StopMoving();
                    this.isRepairing.Write(0x01);
                    return false;
                }

                /// Too far -> start approaching if still damaged, finish command execution otherwise.
                if (this.targetEntity.Read().Biometrics.HP == this.targetEntity.Read().ElementType.MaxHP.Read())
                {
                    return true;
                }
                this.recipientSCV.Read().MotionControl.StartMoving(this.targetEntity.Read().MotionControl.PositionVector.Read());
                this.isRepairing.Write(0x00);
                return false;
            }
        }

        /// <see cref="CmdExecutionBase.InitializeImpl"/>
        protected override void InitializeImpl()
        {
            /// Try to locate the target entity.
            this.targetEntity.Write(this.LocateEntity(this.targetEntityID.Read()));
            if (this.targetEntity.Read() == null)
            {
                /// Target entity could not be located -> simply start moving to the target position.
                this.recipientSCV.Read().MotionControl.StartMoving(this.targetPosition.Read());
                this.isRepairing.Write(0x00);
                return;
            }

            /// Target entity found -> Start attack with the repair tool.
            this.recipientSCV.Read().Armour.StartAttack(this.targetEntityID.Read(), SCV.SCV_REPAIR_TOOL_NAME);
            if (this.recipientSCV.Read().Armour.Target != null)
            {
                /// Repair started -> stop moving.
                this.recipientSCV.Read().MotionControl.StopMoving();
                this.isRepairing.Write(0x01);
                return;
            }

            /// Too far -> start approaching.
            this.recipientSCV.Read().MotionControl.StartMoving(this.targetEntity.Read().MotionControl.PositionVector.Read());
            this.isRepairing.Write(0x00);
        }

        /// <see cref="CmdExecutionBase.GetContinuation"/>
        protected override CmdExecutionBase GetContinuation()
        {
            return new StopExecution(this.recipientSCV.Read());
        }

        /// <see cref="CmdExecutionBase.CommandBeingExecuted"/>
        protected override string GetCommandBeingExecuted() { return "Repair"; }

        #endregion Overrides

        /// <summary>
        /// The target position of this repair execution.
        /// </summary>
        private readonly HeapedValue<RCNumVector> targetPosition;

        /// <summary>
        /// The ID of the target entity of this repair execution.
        /// </summary>
        private readonly HeapedValue<int> targetEntityID;

        /// <summary>
        /// Reference to the recipient SCV of this repair execution.
        /// </summary>
        private readonly HeapedValue<SCV> recipientSCV;

        /// <summary>
        /// Reference to the target entity to repair or null if the target entity has not yet been found.
        /// </summary>
        private readonly HeapedValue<Entity> targetEntity;

        /// <summary>
        /// The elapsed time since last distance check operation.
        /// </summary>
        private readonly HeapedValue<int> timeSinceLastCheck;

        /// <summary>
        /// This flag indicates whether the recipient SCV is currently repairing its target (0x01) or not (0x00).
        /// </summary>
        private readonly HeapedValue<byte> isRepairing;

        /// <summary>
        /// The time between distance check operations.
        /// </summary>
        private const int TIME_BETWEEN_DISTANCE_CHECKS = 12;
    }
}
