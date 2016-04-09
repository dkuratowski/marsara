using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.Core;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Simulator.Commands
{
    /// <summary>
    /// Responsible for executing land commands.
    /// </summary>
    public class LandExecution : PostponedCmdExecution
    {
        /// <summary>
        /// Creates a LandExecution instance.
        /// </summary>
        /// <param name="recipientEntity">The recipient entity of this command execution.</param>
        /// <param name="topLeftQuadTile">The coordinates of the top-left quadratic tile of the landing area.</param>
        public LandExecution(Entity recipientEntity, RCIntVector topLeftQuadTile)
            : base(recipientEntity)
        {
            this.recipientEntity = this.ConstructField<Entity>("recipientEntity");
            this.topLeftQuadTile = this.ConstructField<RCIntVector>("topLeftQuadTile");
            this.targetPosition = this.ConstructField<RCNumVector>("targetPosition");
            this.landOnTheSpot = this.ConstructField<byte>("landOnTheSpot");
            this.recipientEntity.Write(recipientEntity);
            this.topLeftQuadTile.Write(topLeftQuadTile);
            this.targetPosition.Write(RCNumVector.Undefined);
            this.landOnTheSpot.Write(0x00);
        }

        #region Overrides

        /// <see cref="PostponedCmdExecution.ContinueImpl_i"/>
        protected override bool ContinueImpl_i()
        {
            if (this.recipientEntity.Read().MotionControl.Status == MotionControlStatusEnum.InAir)
            {
                /// Recipient entity is in the air. Check if it is still moving.
                if (!this.recipientEntity.Read().MotionControl.IsMoving)
                {
                    /// Recipient entity finished moving. Check if it reached the calculated target position.
                    if (this.recipientEntity.Read().MotionControl.PositionVector.Read() == this.targetPosition.Read())
                    {
                        /// Reached the target position -> start landing.
                        /// TODO: if BeginLand returns false -> indicate the land failure to the user!
                        return !this.recipientEntity.Read().MotionControl.BeginLand(this.landOnTheSpot.Read() == 0x01);
                    }
                    else
                    {
                        /// Was unable to reach the calculated target position -> land execution finished without success.
                        return true;
                    }
                }

                /// Still moving.
                return false;
            }

            if (this.recipientEntity.Read().MotionControl.Status == MotionControlStatusEnum.Landing)
            {
                /// Wait while recipient entity is landing.
                return false;
            }

            /// Landing finished -> end of command execution.
            return true;
        }

        /// <see cref="PostponedCmdExecution.ContinueImpl_i"/>
        protected override void InitializeImpl_i()
        {
            IQuadTile topLeftTile = this.Scenario.Map.GetQuadTile(this.topLeftQuadTile.Read());
            ICell topLeftCell = topLeftTile.GetCell(new RCIntVector(0, 0));
            RCNumVector landPosition = topLeftCell.MapCoords - new RCNumVector(1, 1) / 2 - this.recipientEntity.Read().ElementType.Area.Read().Location;

            if (landPosition.Y - Constants.MAX_VTOL_TRANSITION >= 0)
            {
                this.targetPosition.Write(landPosition - new RCNumVector(0, Constants.MAX_VTOL_TRANSITION));
                this.landOnTheSpot.Write(0x00);
            }
            else
            {
                this.targetPosition.Write(landPosition);
                this.landOnTheSpot.Write(0x01);
            }
            this.recipientEntity.Read().MotionControl.StartMoving(this.targetPosition.Read());
        }

        /// <see cref="CmdExecutionBase.GetContinuation"/>
        protected override CmdExecutionBase GetContinuation()
        {
            if (this.recipientEntity.Read().MotionControl.Status != MotionControlStatusEnum.Fixed)
            {
                return new StopExecution(this.recipientEntity.Read());
            }
            else
            {
                return null;
            }
        }

        /// <see cref="CmdExecutionBase.GetCommandBeingExecuted"/>
        protected override string GetCommandBeingExecuted() { return "Land"; }

        #endregion Overrides

        /// <summary>
        /// Reference to the recipient entity of this command execution.
        /// </summary>
        private readonly HeapedValue<Entity> recipientEntity;

        /// <summary>
        /// The coordinates of the top-left quadratic tile of the landing area.
        /// </summary>
        private readonly HeapedValue<RCIntVector> topLeftQuadTile;

        /// <summary>
        /// The target position where to start the landing operation.
        /// </summary>
        private readonly HeapedValue<RCNumVector> targetPosition;

        /// <summary>
        /// This flag indicates whether the recipient entity shall land on the spot after it reached the target position.
        /// </summary>
        private readonly HeapedValue<byte> landOnTheSpot;
    }
}
