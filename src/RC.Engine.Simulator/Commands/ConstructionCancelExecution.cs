using RC.Common;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Commands
{
    /// <summary>
    /// Responsible for executing construction cancel commands.
    /// </summary>
    public class ConstructionCancelExecution : CmdExecutionBase
    {
        /// <summary>
        /// Creates a ConstructionCancelExecution instance.
        /// </summary>
        /// <param name="recipientEntity">The recipient entity of this command execution.</param>
        public ConstructionCancelExecution(Entity recipientEntity)
            : base(new RCSet<Entity> { recipientEntity })
        {
            this.recipientEntity = this.ConstructField<Entity>("recipientEntity");
            this.recipientEntity.Write(recipientEntity);
        }

        #region Overrides

        /// <see cref="CmdExecutionBase.ContinueImpl"/>
        protected override bool ContinueImpl()
        {
            if (this.recipientEntity.Read().Biometrics.IsUnderConstruction) { this.recipientEntity.Read().Biometrics.CancelConstruct(); }
            return true;
        }

        #endregion Overrides

        /// <summary>
        /// Reference to the recipient entity of this command execution.
        /// </summary>
        private readonly HeapedValue<Entity> recipientEntity;
    }
}
