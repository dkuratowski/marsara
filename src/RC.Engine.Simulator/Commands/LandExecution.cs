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
    /// Responsible for executing land commands.
    /// </summary>
    public class LandExecution : CmdExecutionBase
    {
        /// <summary>
        /// Creates a LandExecution instance.
        /// </summary>
        /// <param name="recipientEntity">The recipient entity of this command execution.</param>
        public LandExecution(Entity recipientEntity)
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
            if (this.recipientEntity.Read().MotionControl.Status == MotionControlStatusEnum.InAir)
            {
                this.recipientEntity.Read().MotionControl.Land(false);
                return false;
            }
            
            if (this.recipientEntity.Read().MotionControl.Status == MotionControlStatusEnum.Landing)
            {
                return false;
            }

            if (this.recipientEntity.Read().MotionControl.Status == MotionControlStatusEnum.OnGround)
            {
                this.recipientEntity.Read().MotionControl.Fix();
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
