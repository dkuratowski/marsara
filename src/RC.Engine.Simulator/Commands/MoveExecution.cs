using System;
using System.Collections.Generic;
using RC.Common;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.Scenarios;

namespace RC.Engine.Simulator.Commands
{
    /// <summary>
    /// The default implementation of the move command execution.
    /// </summary>
    public class MoveExecution : CmdExecutionBase
    {
        /// <summary>
        /// Constructs a move command execution instance.
        /// </summary>
        /// <param name="recipientEntities">The recipient entities of this command execution.</param>
        public MoveExecution(HashSet<Entity> recipientEntities, RCNumVector targetPosition)
            : base(recipientEntities)
        {
            if (targetPosition == RCNumVector.Undefined) { throw new ArgumentNullException("targetPosition"); }

            this.targetPosition = this.ConstructField<RCNumVector>("targetPosition");
            this.targetPosition.Write(targetPosition);
        }

        #region Overrides

        /// <see cref="CmdExecutionBase.ContinueImpl"/>
        protected override bool ContinueImpl()
        {
            /// TODO: implement
            return true;
        }

        #endregion Overrides

        /// <summary>
        /// The target position of this move execution.
        /// </summary>
        private HeapedValue<RCNumVector> targetPosition;
    }
}
