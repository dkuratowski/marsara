using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Commands
{
    /// <summary>
    /// Responsible for executing production start commands.
    /// </summary>
    public class ProductionExecution : CmdExecutionBase
    {
        /// <summary>
        /// Creates a ProductionExecution instance.
        /// </summary>
        /// <param name="recipientEntity">The recipient entity of this command execution.</param>
        /// <param name="product">The typename of the product.</param>
        /// <param name="topLeftQuadTile">
        /// The coordinates of the top-left quadratic tile of the area where the recipient entity shall fly and land before starting the production
        /// or RCIntVector.Undefined if the recipient entity can start the production immediately.
        /// </param>
        public ProductionExecution(Entity recipientEntity, string product, RCIntVector topLeftQuadTile)
            : base(new RCSet<Entity> { recipientEntity })
        {
            this.recipientEntity = this.ConstructField<Entity>("recipientEntity");
            this.topLeftQuadTile = this.ConstructField<RCIntVector>("topLeftQuadTile");
            this.status = this.ConstructField<byte>("status");
            this.recipientEntity.Write(recipientEntity);
            this.topLeftQuadTile.Write(topLeftQuadTile);
            this.status.Write(INITIAL);
            this.product = product;
        }

        #region Overrides

        /// <see cref="CmdExecutionBase.ContinueImpl"/>
        protected override bool ContinueImpl()
        {
            if (this.status.Read() == INITIAL)
            {
                if (this.topLeftQuadTile.Read() == RCIntVector.Undefined ||
                    this.topLeftQuadTile.Read() == this.recipientEntity.Read().MapObject.QuadraticPosition.Location)
                {
                    /// If the recipient entity is currently at the target position or if there is no target position defined
                    ///     -> start production if possible and finish the command execution.
                    if (this.recipientEntity.Read().IsProductAvailable(this.product))
                    {
                        if (this.recipientEntity.Read().IsProductEnabled(this.product))
                        {
                            this.recipientEntity.Read().StartProduction(this.product);
                        }
                    }
                    return true;
                }

                /// If the recipient entity is a building that currently has an addon -> cancel the execution.
                Building recipientBuilding = this.recipientEntity.Read() as Building;
                if (recipientBuilding != null && recipientBuilding.CurrentAddon != null) { return true; }

                /// Otherwise begin a liftoff execution.
                this.status.Write(LIFTOFF);
                this.StartSubExecution(new LiftOffExecution(this.recipientEntity.Read()));
                return false;
            }
            else if (this.status.Read() == LIFTOFF)
            {
                /// If the recipient entity was unable to liftoff -> finish this execution.
                if (this.recipientEntity.Read().MotionControl.Status != MotionControlStatusEnum.InAir) { return true; }

                /// Otherwise begin a land execution.
                this.status.Write(LANDING);
                this.StartSubExecution(new LandExecution(this.recipientEntity.Read(), this.topLeftQuadTile.Read()));
                return false;
            }
            else if (this.status.Read() == LANDING)
            {
                /// If the recipient entity was unable to land -> finish this execution.
                if (this.recipientEntity.Read().MotionControl.Status != MotionControlStatusEnum.Fixed) { return true; }

                /// Otherwise start production if possible and finish this execution.
                if (this.recipientEntity.Read().IsProductAvailable(this.product))
                {
                    if (this.recipientEntity.Read().IsProductEnabled(this.product))
                    {
                        this.recipientEntity.Read().StartProduction(this.product);
                    }
                }
                return true;
            }
            else
            {
                throw new InvalidOperationException("Impossible case happened!");
            }
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

        #endregion Overrides

        /// <summary>
        /// Reference to the recipient entity of this command execution.
        /// </summary>
        private readonly HeapedValue<Entity> recipientEntity;

        /// <summary>
        /// The coordinates of the top-left quadratic tile of the area where the recipient entity shall fly and land before starting the production.
        /// </summary>
        private readonly HeapedValue<RCIntVector> topLeftQuadTile;

        /// <summary>
        /// The current status of this command execution.
        /// </summary>
        private readonly HeapedValue<byte> status;

        /// <summary>
        /// The typename of the product.
        /// </summary>
        /// TODO: heap this field!
        private readonly string product;

        /// <summary>
        /// The bytes that indicates the status of this command execution.
        /// </summary>
        private const byte INITIAL = 0x00;
        private const byte LIFTOFF = 0x01;
        private const byte LANDING = 0x02;
    }
}
