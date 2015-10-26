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
            if (this.recipientEntity.Read().MotionControl.Status == MotionControlStatusEnum.Fixed)
            {
                if (this.recipientEntity.Read().MotionControl.BeginTakeOff())
                {
                    /// Sucessfully beginned to takeoff -> continue monitoring the entity until it gets into the air.
                    return false;
                }
                else
                {
                    /// Unable to takeoff -> command execution finished.
                    /// TODO: send a message to the user that the takeoff operation failed.
                    return true;
                }
            }

            return this.recipientEntity.Read().MotionControl.Status == MotionControlStatusEnum.InAir;
        }

        /// <see cref="CmdExecutionBase.GetContinuation"/>
        protected override CmdExecutionBase GetContinuation()
        {
            if (this.recipientEntity.Read().MotionControl.Status == MotionControlStatusEnum.InAir)
            {
                return new StopExecution(this.recipientEntity.Read());
            }
            else
            {
                return null;
            }
        }

        #endregion Overrides

        /// <summary>
        /// Reference to the recipient entity of this command execution.
        /// </summary>
        private readonly HeapedValue<Entity> recipientEntity;
    }
}
