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
    /// The abstract base class of command executions that are automatically postponed when the recipient entity is taking off or landing and cancelled when
    /// the recipient entity is fixed.
    /// </summary>
    public abstract class PostponedCmdExecution : CmdExecutionBase
    {
        /// <summary>
        /// Creates a PostponedCmdExecution instance.
        /// </summary>
        /// <param name="recipientEntity">The recipient entity of this command execution.</param>
        protected PostponedCmdExecution(Entity recipientEntity)
            : base(new RCSet<Entity> { recipientEntity })
        {
            this.recipientEntity = this.ConstructField<Entity>("recipientEntity");
            this.waitingForTakeoffOrLand = this.ConstructField<byte>("waitingForTakeoffOrLand");
            this.waitingForTakeoffOrLand.Write(0x00);
            this.recipientEntity.Write(recipientEntity);
        }

        #region Overrides

        /// <see cref="CmdExecutionBase.ContinueImpl"/>
        protected sealed override bool ContinueImpl()
        {
            /// If the recipient entity is fixed -> execution cancelled.
            if (this.recipientEntity.Read().MotionControl.Status == MotionControlStatusEnum.Fixed)
            {
                return true;
            }

            /// If we are waiting for the current takeoff operation to complete -> check the completion.
            if (this.waitingForTakeoffOrLand.Read() == 0x01)
            {
                if (this.recipientEntity.Read().MotionControl.Status == MotionControlStatusEnum.InAir ||
                    this.recipientEntity.Read().MotionControl.Status == MotionControlStatusEnum.OnGround)
                {
                    this.InitializeImpl_i();
                    this.waitingForTakeoffOrLand.Write(0x00);
                }
                else
                {
                    return false;
                }
            }

            return this.ContinueImpl_i();
        }

        /// <see cref="CmdExecutionBase.InitializeImpl"/>
        protected sealed override void InitializeImpl()
        {
            /// If the recipient entity is fixed -> nothing to do.
            if (this.recipientEntity.Read().MotionControl.Status == MotionControlStatusEnum.Fixed)
            {
                return;
            }

            /// If the recipient entity is taking off or landing -> postpone the initialization.
            if (this.recipientEntity.Read().MotionControl.Status == MotionControlStatusEnum.TakingOff ||
                this.recipientEntity.Read().MotionControl.Status == MotionControlStatusEnum.Landing)
            {
                this.waitingForTakeoffOrLand.Write(0x01);
            }
            else
            {
                /// Otherwise initialize now.
                this.InitializeImpl_i();
            }
        }

        #endregion Overrides

        #region Overridables

        /// <summary>
        /// Same as CmdExecutionBase.ContinueImpl. Classes that derives from PostponedCmdExecution shall override this method instead of ContinueImpl.
        /// </summary>
        /// <returns>True if this command execution has finished; otherwise false.</returns>
        protected abstract bool ContinueImpl_i();

        /// <summary>
        /// Same as CmdExecutionBase.InitializeImpl. Classes that derives from PostponedCmdExecution can override this method instead of InitializeImpl.
        /// </summary>
        protected virtual void InitializeImpl_i() { }

        #endregion Overridables

        /// <summary>
        /// Reference to the recipient entity of this command execution.
        /// </summary>
        private readonly HeapedValue<Entity> recipientEntity;

        /// <summary>
        /// This flag indicates whether this command execution is still waiting for the current taking off or landing operation.
        /// </summary>
        private readonly HeapedValue<byte> waitingForTakeoffOrLand;
    }
}
