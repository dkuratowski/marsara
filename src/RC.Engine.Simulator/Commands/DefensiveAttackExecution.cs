using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Commands
{
    /// <summary>
    /// Responsible for executing defensive attack commands.
    /// </summary>
    class DefensiveAttackExecution : CmdExecutionBase
    {
        /// <summary>
        /// Creates an DefensiveAttackExecution instance.
        /// </summary>
        /// <param name="recipientEntity">The recipient entity of this command execution.</param>
        /// <param name="targetEntityID">The ID of the entity to attack or -1 if no such entity is defined.</param>
        public DefensiveAttackExecution(Entity recipientEntity, int targetEntityID)
            : base(new RCSet<Entity> { recipientEntity })
        {
            this.recipientEntity = this.ConstructField<Entity>("recipientEntity");
            this.targetEntityID = this.ConstructField<int>("targetEntityID");
            this.timeSinceLastCheck = this.ConstructField<int>("timeSinceLastCheck");

            this.recipientEntity.Write(recipientEntity);
            this.targetEntityID.Write(targetEntityID);
            this.timeSinceLastCheck.Write(0);
        }

        #region Overrides

        /// <see cref="CmdExecutionBase.ContinueImpl"/>
        protected override bool ContinueImpl()
        {
            /// Check if we have to do anything in this frame.
            if (this.timeSinceLastCheck.Read() < TIME_BETWEEN_CHECKS)
            {
                /// Nothing to do now.
                this.timeSinceLastCheck.Write(this.timeSinceLastCheck.Read() + 1);
                return false;
            }

            /// Perform a state refresh in this frame.
            this.timeSinceLastCheck.Write(0);

            /// Try to locate the target entity.
            Entity targetEntity = this.LocateEntity(this.targetEntityID.Read());
            if (targetEntity == null)
            {
                /// Target entity cannot be located -> attack execution finished.
                return true;
            }

            /// Target entity located -> Start attack with a standard weapon.
            this.recipientEntity.Read().Armour.StartAttack(targetEntity.ID.Read());
            if (this.recipientEntity.Read().Armour.Target == null)
            {
                /// Unable to attack the target entity -> attack execution finished.
                return true;
            }

            /// Target entity is still in attack range -> continue.
            return false;
        }

        /// <see cref="CmdExecutionBase.GetContinuation"/>
        protected override CmdExecutionBase GetContinuation()
        {
            return new DefensiveStopExecution(this.recipientEntity.Read());
        }

        /// <see cref="CmdExecutionBase.CommandBeingExecuted"/>
        protected override string GetCommandBeingExecuted() { return "Attack"; }

        #endregion Overrides

        /// <summary>
        /// Reference to the recipient entity of this command execution.
        /// </summary>
        private readonly HeapedValue<Entity> recipientEntity;

        /// <summary>
        /// The ID of the target entity to attack.
        /// </summary>
        private readonly HeapedValue<int> targetEntityID;

        /// <summary>
        /// The elapsed time since last check operation.
        /// </summary>
        private readonly HeapedValue<int> timeSinceLastCheck;

        /// <summary>
        /// The time between check operations.
        /// </summary>
        private const int TIME_BETWEEN_CHECKS = 12;
    }
}
