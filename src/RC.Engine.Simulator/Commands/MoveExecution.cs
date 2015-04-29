using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.Scenarios;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Simulator.Commands
{
    /// <summary>
    /// Responsible for executing move commands.
    /// </summary>
    public class MoveExecution : CmdExecutionBase
    {
        /// <summary>
        /// Creates a MoveExecution instance.
        /// </summary>
        /// <param name="recipientEntity">The recipient entity of this command execution.</param>
        /// <param name="targetPosition">The target position.</param>
        /// <param name="targetEntityID">The ID of the entity to follow or -1 if no such entity is defined.</param>
        public MoveExecution(Entity recipientEntity, RCNumVector targetPosition, int targetEntityID)
            : base(new HashSet<Entity> { recipientEntity })
        {
            this.targetPosition = this.ConstructField<RCNumVector>("targetPosition");
            this.targetEntityID = this.ConstructField<int>("targetEntityID");
            this.recipientEntity = this.ConstructField<Entity>("recipientEntity");
            this.targetEntity = this.ConstructField<Entity>("targetEntity");
            this.timeSinceLastCheck = this.ConstructField<int>("timeSinceLastCheck");
            this.targetPosition.Write(targetPosition);
            this.targetEntityID.Write(targetEntityID);
            this.recipientEntity.Write(recipientEntity);
            this.timeSinceLastCheck.Write(0);
        }

        #region Overrides

        /// <see cref="CmdExecutionBase.ContinueImpl"/>
        protected override bool ContinueImpl()
        {
            /// Check if we have to do anything in this frame.
            if (this.timeSinceLastCheck.Read() < TIME_BETWEEN_DISTANCE_CHECKS)
            {
                /// Nothing to do now.
                this.timeSinceLastCheck.Write(this.timeSinceLastCheck.Read() + 1);
                return false;
            }

            /// Perform a state refresh in this frame.
            this.timeSinceLastCheck.Write(0);
            if (this.targetEntity.Read() == null)
            {
                /// No target to follow -> simple move operation without any target entity.
                return !this.recipientEntity.Read().IsMoving;
            }

            /// Continue follow the target.
            this.ContinueFollow();
            return false;
        }

        /// <see cref="CmdExecutionBase.GetContinuation"/>
        protected override CmdExecutionBase GetContinuation()
        {
            return new StopExecution(this.recipientEntity.Read());
        }

        /// <see cref="CmdExecutionBase.Initialize"/>
        protected override void Initialize()
        {
            this.targetEntity.Write(this.LocateEntity(this.targetEntityID.Read()));
            if (this.targetEntity.Read() == null)
            {
                /// Target entity is not defined or could not be located -> simply move to the target position.
                this.recipientEntity.Read().StartMoving(this.targetPosition.Read());
            }
            else
            {
                /// Target entity is defined and could be located -> calculate its distance from the recipient entity.
                RCNumber distance = MapUtils.ComputeDistance(this.recipientEntity.Read().BoundingBox, this.targetEntity.Read().BoundingBox);
                if (distance <= MAX_DISTANCE)
                {
                    /// Close enough -> not necessary to start approaching.
                    this.recipientEntity.Read().StopMoving();
                }
                else
                {
                    /// Too far -> start approaching
                    this.recipientEntity.Read().StartMoving(this.targetEntity.Read().PositionValue.Read());
                }
            }
        }

        /// <see cref="CmdExecutionBase.CommandBeingExecuted"/>
        protected override string GetCommandBeingExecuted() { return "Move"; }

        #endregion Overrides

        /// <summary>
        /// Continue the execution in case of a follow command.
        /// </summary>
        /// <returns>True if execution is finished; otherwise false.</returns>
        private void ContinueFollow()
        {
            /// Check if target entity still can be located.
            this.targetEntity.Write(this.LocateEntity(this.targetEntity.Read().ID.Read()));
            if (this.targetEntity.Read() == null) { return; }

            /// Calculate its distance from the recipient entity.
            RCNumber distance = MapUtils.ComputeDistance(this.recipientEntity.Read().BoundingBox, this.targetEntity.Read().BoundingBox);
            if (distance <= MAX_DISTANCE)
            {
                /// Close enough -> stop the recipient entity.
                this.recipientEntity.Read().StopMoving();
            }
            else
            {
                /// Too far -> start approaching again.
                this.recipientEntity.Read().StartMoving(this.targetEntity.Read().PositionValue.Read());
            }
        }

        /// <summary>
        /// The target position of this move execution.
        /// </summary>
        private readonly HeapedValue<RCNumVector> targetPosition;

        /// <summary>
        /// The ID of the target entity of this move execution.
        /// </summary>
        private readonly HeapedValue<int> targetEntityID;

        /// <summary>
        /// Reference to the recipient entity of this command execution.
        /// </summary>
        private readonly HeapedValue<Entity> recipientEntity;

        /// <summary>
        /// Reference to the target entity to follow or null if the target entity has not yet been found.
        /// </summary>
        private readonly HeapedValue<Entity> targetEntity;

        /// <summary>
        /// The elapsed time since last distance check operation.
        /// </summary>
        private readonly HeapedValue<int> timeSinceLastCheck;

        /// <summary>
        /// The maximum allowed distance between the recipient and the target entities in cells.
        /// </summary>
        private static readonly RCNumber MAX_DISTANCE = 12;

        /// <summary>
        /// The time between distance check operations.
        /// </summary>
        private const int TIME_BETWEEN_DISTANCE_CHECKS = 12;
    }
}
