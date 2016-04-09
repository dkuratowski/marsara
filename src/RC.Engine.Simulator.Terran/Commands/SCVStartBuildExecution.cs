using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Commands;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.Terran.Buildings;
using RC.Engine.Simulator.Terran.Units;

namespace RC.Engine.Simulator.Terran.Commands
{
    /// <summary>
    /// Responsible for executing start build commands for SCVs.
    /// </summary>
    class SCVStartBuildExecution : SCVBuildExecutionBase
    {
        /// <summary>
        /// Creates a SCVStartBuildExecution instance.
        /// </summary>
        /// <param name="recipientSCV">The recipient SCV of this command execution.</param>
        /// <param name="buildingType">The typename of the building.</param>
        /// <param name="topLeftQuadTile">The coordinates of the top-left quadratic tile of the building to be created.</param>
        public SCVStartBuildExecution(SCV recipientSCV, string buildingType, RCIntVector topLeftQuadTile)
            : base(recipientSCV)
        {
            this.topLeftQuadTile = this.ConstructField<RCIntVector>("topLeftQuadTile");
            this.targetArea = this.ConstructField<RCNumRectangle>("targetArea");
            this.topLeftQuadTile.Write(topLeftQuadTile);
            this.targetArea.Write(RCNumRectangle.Undefined);
            this.buildingTypeName = buildingType;
        }

        #region Overrides

        /// <see cref="CmdExecutionBase.InitializeImpl"/>
        protected override void InitializeImpl()
        {
            IBuildingType buildingType = this.RecipientSCV.Owner.Metadata.GetBuildingType(this.buildingTypeName);
            RCIntVector entityQuadSize = this.Scenario.Map.CellToQuadSize(buildingType.Area.Read().Size);
            RCIntRectangle targetPositionQuadRect = new RCIntRectangle(this.topLeftQuadTile.Read(), entityQuadSize);
            this.targetArea.Write((RCNumRectangle)this.Scenario.Map.QuadToCellRect(targetPositionQuadRect) - new RCNumVector(1, 1) / 2);
            this.TargetPosition = this.targetArea.Read().Location - buildingType.Area.Read().Location;
            this.RecipientSCV.MotionControl.StartMoving(this.TargetPosition);
            this.Status = SCVBuildExecutionStatusEnum.MovingToTarget;
        }

        /// <see cref="SCVBuildExecutionBase.ContinueMovingToTarget"/>
        protected override bool ContinueMovingToTarget()
        {
            /// Check the distance between the SCV and the target area.
            RCNumber distance = MapUtils.ComputeDistance(this.RecipientSCV.Area, this.targetArea.Read());
            if (distance > Weapon.NEARBY_DISTANCE)
            {
                /// Distance not reached yet -> continue execution if SCV is still moving.
                return !this.RecipientSCV.MotionControl.IsMoving;
            }

            TerranBuildingConstructionJob job = new TerranBuildingConstructionJob(
                this.RecipientSCV,
                this.RecipientSCV.Owner.Metadata.GetBuildingType(this.buildingTypeName),
                this.topLeftQuadTile.Read());
            if (!job.LockResources())
            {
                /// Unable to lock the necessary resources -> abort the job and cancel.
                job.Dispose();
                return true;
            }

            if (!job.Start())
            {
                /// Unable to start the job -> abort the job and cancel.
                job.Dispose();
                return true;
            }

            this.Status = SCVBuildExecutionStatusEnum.Constructing;
            return false;
        }

        #endregion Overrides

        /// <summary>
        /// The coordinates of the top-left quadratic tile of the building to be created.
        /// </summary>
        private readonly HeapedValue<RCIntVector> topLeftQuadTile;

        /// <summary>
        /// The target area that the recipient SCV shall reach before it could start constructing the building.
        /// </summary>
        private readonly HeapedValue<RCNumRectangle> targetArea;

        /// <summary>
        /// The typename of the building.
        /// </summary>
        /// TODO: heap this field!
        private readonly string buildingTypeName;
    }
}
