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
    /// Responsible for executing continue build commands for SCVs.
    /// </summary>
    class SCVContinueBuildExecution : SCVBuildExecutionBase
    {
        /// <summary>
        /// Creates a SCVContinueBuildExecution instance.
        /// </summary>
        /// <param name="recipientSCV">The recipient SCV of this command execution.</param>
        /// <param name="targetPosition">The target position.</param>
        /// <param name="targetBuildingID">The ID of the target building whose construction to be continued.</param>
        public SCVContinueBuildExecution(SCV recipientSCV, RCNumVector targetPosition, int targetBuildingID)
            : base(recipientSCV)
        {
            this.targetBuildingID = this.ConstructField<int>("targetBuildingID");
            this.TargetPosition = targetPosition;
            this.targetBuildingID.Write(targetBuildingID);
        }

        #region Overrides

        /// <see cref="CmdExecutionBase.InitializeImpl"/>
        protected override void InitializeImpl()
        {
            this.RecipientSCV.MotionControl.StartMoving(this.TargetPosition);
            this.Status = SCVBuildExecutionStatusEnum.MovingToTarget;
        }

        /// <see cref="SCVBuildExecutionBase.ContinueMovingToTarget"/>
        protected override bool ContinueMovingToTarget()
        {
            /// First try to retrieve the target building from the scenario.
            TerranBuilding targetBuilding = this.Scenario.GetElementOnMap<TerranBuilding>(this.targetBuildingID.Read(), MapObjectLayerEnum.GroundObjects);
            if (targetBuilding == null)
            {
                /// Target building not found -> finish command execution.
                return true;
            }

            /// Check the distance between the SCV and the target building.
            RCNumber distance = MapUtils.ComputeDistance(this.RecipientSCV.Area, targetBuilding.Area);
            if (distance > Weapon.NEARBY_DISTANCE)
            {
                /// Distance not reached yet -> continue execution if SCV is still moving.
                return !this.RecipientSCV.MotionControl.IsMoving;
            }

            /// Check if the target building has an inactive construction job.
            if (targetBuilding.ConstructionJob == null || targetBuilding.ConstructionJob.AttachedSCV != null)
            {
                /// Target building doesn't have an inactive construction job -> finish command execution.
                return true;
            }

            /// Attach the SCV to the construction job of the target building.
            targetBuilding.ConstructionJob.AttachSCV(this.RecipientSCV);
            this.Status = SCVBuildExecutionStatusEnum.Constructing;
            return false;
        }

        #endregion Overrides

        /// <summary>
        /// The ID of the building whose construction to be continued.
        /// </summary>
        private readonly HeapedValue<int> targetBuildingID;
    }
}
