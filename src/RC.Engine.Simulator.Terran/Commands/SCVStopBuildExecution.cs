using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Commands;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.Terran.Buildings;
using RC.Engine.Simulator.Terran.Units;

namespace RC.Engine.Simulator.Terran.Commands
{
    /// <summary>
    /// Responsible for executing stop build commands for SCVs.
    /// </summary>
    class SCVStopBuildExecution : CmdExecutionBase
    {
        /// <summary>
        /// Creates a SCVStopBuildExecution instance.
        /// </summary>
        /// <param name="recipientSCV">The recipient SCV of this command execution.</param>
        public SCVStopBuildExecution(SCV recipientSCV)
            : base(new RCSet<Entity> { recipientSCV })
        {
            this.recipientSCV = this.ConstructField<SCV>("recipientSCV");
            this.recipientSCV.Write(recipientSCV);
        }

        #region Overrides

        /// <see cref="CmdExecutionBase.ContinueImpl"/>
        protected override bool ContinueImpl()
        {
            return !this.recipientSCV.Read().MotionControl.IsMoving;
        }

        /// <see cref="CmdExecutionBase.GetContinuation"/>
        protected override CmdExecutionBase GetContinuation()
        {
            return new StopExecution(this.recipientSCV.Read());
        }

        /// <see cref="CmdExecutionBase.InitializeImpl"/>
        protected override void InitializeImpl()
        {
            TerranBuilding building = this.recipientSCV.Read().ConstructionJob.ConstructedBuilding;
            this.recipientSCV.Read().ConstructionJob.DetachSCV();

            /// Move the SCV to the bottom-left corner of the building.
            RCIntRectangle buildingQuadRect = building.MapObject.QuadraticPosition;
            RCIntRectangle buildingCellRect = this.Scenario.Map.QuadToCellRect(buildingQuadRect);
            this.recipientSCV.Read().MotionControl.StartMoving(new RCNumVector(buildingCellRect.Left, buildingCellRect.Bottom - 1));
        }

        #endregion Overrides

        /// <summary>
        /// Reference to the recipient SCV of this command execution.
        /// </summary>
        private readonly HeapedValue<SCV> recipientSCV;
    }
}
