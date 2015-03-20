using RC.Common;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.Scenarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Commands
{
    /// <summary>
    /// Responsible for executing attack commands.
    /// </summary>
    public class AttackExecution : CmdExecutionBase
    {
        /// <summary>
        /// Creates an AttackExecution instance.
        /// </summary>
        /// <param name="recipientEntity">The recipient entity of this command execution.</param>
        /// <param name="targetPosition">The target position.</param>
        /// <param name="targetEntityID">The ID of the entity to attack or -1 if no such entity is defined.</param>
        public AttackExecution(Entity recipientEntity, RCNumVector targetPosition, int targetEntityID)
            : base(new HashSet<Entity> { recipientEntity })
        {
            this.recipientEntity = this.ConstructField<Entity>("recipientEntity");
            this.targetEntity = this.ConstructField<Entity>("targetEntity");
            this.recipientEntity.Write(recipientEntity);
            this.targetEntity.Write(null);
        }

        #region Overrides

        /// <see cref="CmdExecutionBase.ContinueImpl"/>
        protected override bool ContinueImpl()
        {
            return false;
        }

        /// <see cref="CmdExecutionBase.CommandBeingExecuted"/>
        public override string CommandBeingExecuted { get { return "Attack"; } }

        #endregion Overrides

        /// <summary>
        /// Reference to the recipient entity of this command execution.
        /// </summary>
        private readonly HeapedValue<Entity> recipientEntity;

        /// <summary>
        /// Reference to the target entity to attack.
        /// </summary>
        private readonly HeapedValue<Entity> targetEntity;
    }
}
