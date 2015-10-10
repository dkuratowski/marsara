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
    /// Responsible for executing lift-off commands.
    /// </summary>
    public class LiftOffExecution : CmdExecutionBase
    {
        /// <summary>
        /// Creates a LiftOffExecution instance.
        /// </summary>
        /// <param name="recipientEntity">The recipient entity of this command execution.</param>
        public LiftOffExecution(Entity recipientEntity)
            : base(new RCSet<Entity> { recipientEntity })
        {
            this.recipientEntity = this.ConstructField<Entity>("recipientEntity");
            this.recipientEntity.Write(recipientEntity);
        }

        #region Overrides

        /// <see cref="CmdExecutionBase.ContinueImpl"/>
        protected override bool ContinueImpl()
        {
            // TODO: this is only a temporary implementation for testing!
            if (!this.recipientEntity.Read().MotionControl.IsFlying)
            {
                this.recipientEntity.Read().MotionControl.Unfix();
                this.recipientEntity.Read().MotionControl.TakeOff();
            }
            return true;
        }

        #endregion Overrides

        /// <summary>
        /// Reference to the recipient entity of this command execution.
        /// </summary>
        private readonly HeapedValue<Entity> recipientEntity;
    }
}
