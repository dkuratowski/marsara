using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.Commands;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.Terran.Buildings;
using RC.Engine.Simulator.Terran.Units;

namespace RC.Engine.Simulator.Terran.Commands
{
    /// <summary>
    /// Enumerates the special Terran SCV commands.
    /// </summary>
    public enum SCVCommandEnum
    {
        Undefined = -1,
        [EnumMapping("Repair")]
        Repair = 0,
        [EnumMapping("Gather")]
        Gather = 1,
        [EnumMapping("Return")]
        Return = 2,
        [EnumMapping("Build")]
        Build = 3,
        [EnumMapping("StopBuild")]
        StopBuild = 4,
        [EnumMapping("Move")]
        Move = 5,
        [EnumMapping("Stop")]
        Stop = 6,
        [EnumMapping("Attack")]
        Attack = 7,
        [EnumMapping("Patrol")]
        Patrol = 8,
        [EnumMapping("Hold")]
        Hold = 9
    }

    /// <summary>
    /// Factory for special Terran SCV command executions.
    /// </summary>
    class SCVCmdExecutionFactory : CommandExecutionFactoryBase<SCV>
    {
        /// <summary>
        /// Constructs a SCVCmdExecutionFactory instance.
        /// </summary>
        /// <param name="command">The type of the special Terran SCV command.</param>
        public SCVCmdExecutionFactory(SCVCommandEnum command)
            : base(command != SCVCommandEnum.Undefined ? EnumMap<SCVCommandEnum, string>.Map(command) : null, SCV.SCV_TYPE_NAME)
        {
            this.commandType = command;
        }

        /// <see cref="CommandExecutionFactoryBase.GetCommandAvailability"/>
        protected override AvailabilityEnum GetCommandAvailability(RCSet<SCV> scvsToHandle, RCSet<Entity> fullEntitySet, string parameter)
        {
            switch (this.commandType)
            {
                case SCVCommandEnum.Repair:
                    return this.CheckRepairAvailability(scvsToHandle, fullEntitySet);
                case SCVCommandEnum.Gather:
                    return this.CheckGatherAvailability(scvsToHandle, fullEntitySet);
                case SCVCommandEnum.Return:
                    return this.CheckReturnAvailability(scvsToHandle, fullEntitySet);
                case SCVCommandEnum.Build:
                    return this.CheckBuildAvailability(scvsToHandle, fullEntitySet, parameter);
                case SCVCommandEnum.StopBuild:
                    return this.CheckStopBuildAvailability(scvsToHandle, fullEntitySet);
                case SCVCommandEnum.Move:
                case SCVCommandEnum.Stop:
                case SCVCommandEnum.Attack:
                case SCVCommandEnum.Patrol:
                case SCVCommandEnum.Hold:
                    return this.CheckBasicCommandAvailability(scvsToHandle);
                default:
                    return AvailabilityEnum.Unavailable;
            }
        }

        /// <see cref="CommandExecutionFactoryBase.CreateCommandExecutions"/>
        protected override IEnumerable<CmdExecutionBase> CreateCommandExecutions(RCSet<SCV> scvsToHandle, RCSet<Entity> fullEntitySet, RCNumVector targetPosition, int targetEntityID, string parameter)
        {
            switch (this.commandType)
            {
                case SCVCommandEnum.Repair:
                    return this.CreateRepairExecutions(scvsToHandle, targetPosition, targetEntityID);
                case SCVCommandEnum.Gather:
                    return this.CreateGatherExecutions(scvsToHandle, targetPosition, targetEntityID);
                case SCVCommandEnum.Return:
                    return this.CreateReturnExecutions(scvsToHandle);
                case SCVCommandEnum.Build:
                    return this.CreateBuildExecutions(scvsToHandle, (RCIntVector)targetPosition, parameter);
                case SCVCommandEnum.StopBuild:
                    return this.CreateStopBuildExecutions(scvsToHandle);
                case SCVCommandEnum.Move:
                    return this.CreateMoveExecutions(scvsToHandle, fullEntitySet, targetPosition, targetEntityID);
                case SCVCommandEnum.Stop:
                    return this.CreateStopExecutions(scvsToHandle);
                case SCVCommandEnum.Attack:
                    return this.CreateAttackExecutions(scvsToHandle, fullEntitySet, targetPosition, targetEntityID);
                case SCVCommandEnum.Patrol:
                    return this.CreatePatrolExecutions(scvsToHandle, fullEntitySet, targetPosition);
                case SCVCommandEnum.Hold:
                    return this.CreateHoldExecutions(scvsToHandle);
                default:
                    return this.CreateUndefinedExecutions(scvsToHandle, fullEntitySet, targetPosition, targetEntityID);
            }
        }

        #region Availability checker methods

        /// <summary>
        /// Checks the availability of the Repair command for the given SCVs.
        /// </summary>
        /// <param name="scvsToHandle">The SCVs to check.</param>
        /// <param name="fullEntitySet">The set of selected entities.</param>
        /// <returns>The availability of the Repair command for the given SCVs.</returns>
        private AvailabilityEnum CheckRepairAvailability(RCSet<SCV> scvsToHandle, RCSet<Entity> fullEntitySet)
        {
            if (scvsToHandle.Count != fullEntitySet.Count) { return AvailabilityEnum.Unavailable; }
            return scvsToHandle.Any(scv => !scv.IsConstructing) ? AvailabilityEnum.Enabled : AvailabilityEnum.Unavailable;
        }

        /// <summary>
        /// Checks the availability of the Gather command for the given SCVs.
        /// </summary>
        /// <param name="scvsToHandle">The SCVs to check.</param>
        /// <param name="fullEntitySet">The set of selected entities.</param>
        /// <returns>The availability of the Gather command for the given SCVs.</returns>
        private AvailabilityEnum CheckGatherAvailability(RCSet<SCV> scvsToHandle, RCSet<Entity> fullEntitySet)
        {
            if (scvsToHandle.Count != fullEntitySet.Count) { return AvailabilityEnum.Unavailable; }
            return scvsToHandle.Any(scv => !scv.IsConstructing) ? AvailabilityEnum.Enabled : AvailabilityEnum.Unavailable;
            /// TODO: implement this method!
        }

        /// <summary>
        /// Checks the availability of the Return command for the given SCVs.
        /// </summary>
        /// <param name="scvsToHandle">The SCVs to check.</param>
        /// <param name="fullEntitySet">The set of selected entities.</param>
        /// <returns>The availability of the Return command for the given SCVs.</returns>
        private AvailabilityEnum CheckReturnAvailability(RCSet<SCV> scvsToHandle, RCSet<Entity> fullEntitySet)
        {
            if (scvsToHandle.Count != fullEntitySet.Count) { return AvailabilityEnum.Unavailable; }
            return scvsToHandle.Any(scv => !scv.IsConstructing) ? AvailabilityEnum.Enabled : AvailabilityEnum.Unavailable;
            /// TODO: implement this method!
        }

        /// <summary>
        /// Checks the availability of the Build command for the given SCVs.
        /// </summary>
        /// <param name="scvsToHandle">The SCVs to check.</param>
        /// <param name="fullEntitySet">The set of selected entities.</param>
        /// <param name="buildingTypeName">The name of the type of the building or null if no building is specified.</param>
        /// <returns>The availability of the Build command for the given SCVs.</returns>
        private AvailabilityEnum CheckBuildAvailability(RCSet<SCV> scvsToHandle, RCSet<Entity> fullEntitySet, string buildingTypeName)
        {
            if (scvsToHandle.Count != fullEntitySet.Count || scvsToHandle.Count != 1) { return AvailabilityEnum.Unavailable; }

            /// If the selected SCV is currently constructing -> unavailable.
            SCV scv = scvsToHandle.First();
            if (scv.IsConstructing) { return AvailabilityEnum.Unavailable; }

            /// If building type is not specified -> it is the parent build button availability check -> enable.
            if (buildingTypeName == null) { return AvailabilityEnum.Enabled; }

            /// Check the requirements of the building.
            IBuildingType buildingType = scv.Owner.Metadata.GetBuildingType(buildingTypeName);
            foreach (IRequirement requirement in buildingType.Requirements)
            {
                if (!scv.Owner.HasBuilding(requirement.RequiredBuildingType.Name)) { return AvailabilityEnum.Disabled; }
                if (requirement.RequiredAddonType != null && !scv.Owner.HasAddon(requirement.RequiredAddonType.Name)) { return AvailabilityEnum.Disabled; }
            }
            return AvailabilityEnum.Enabled;
        }

        /// <summary>
        /// Checks the availability of the StopBuild command for the given SCVs.
        /// </summary>
        /// <param name="scvsToHandle">The SCVs to check.</param>
        /// <param name="fullEntitySet">The set of selected entities.</param>
        /// <returns>The availability of the StopBuild command for the given SCVs.</returns>
        private AvailabilityEnum CheckStopBuildAvailability(RCSet<SCV> scvsToHandle, RCSet<Entity> fullEntitySet)
        {
            if (scvsToHandle.Count != fullEntitySet.Count) { return AvailabilityEnum.Unavailable; }
            return scvsToHandle.All(scv => scv.IsConstructing) ? AvailabilityEnum.Enabled : AvailabilityEnum.Unavailable;
        }

        /// <summary>
        /// Checks the availability of the basic commands for the given SCVs.
        /// </summary>
        /// <param name="scvsToHandle">The SCVs to check.</param>
        /// <returns>The availability of the basic commands for the given SCVs.</returns>
        private AvailabilityEnum CheckBasicCommandAvailability(RCSet<SCV> scvsToHandle)
        {
            return scvsToHandle.Any(scv => !scv.IsConstructing) ? AvailabilityEnum.Enabled : AvailabilityEnum.Unavailable;
        }

        #endregion Availability checker methods

        #region Execution creator methods

        /// <summary>
        /// Creates return executions for the given SCVs.
        /// </summary>
        /// <param name="scvsToHandle">The SCVs to return.</param>
        private IEnumerable<CmdExecutionBase> CreateReturnExecutions(RCSet<SCV> scvsToHandle)
        {
            // TODO: implement!
            yield break;
        }

        /// <summary>
        /// Creates repair executions for the given SCVs.
        /// </summary>
        /// <param name="scvsToHandle">The reapiring SCVs.</param>
        /// <param name="targetPosition">The target position.</param>
        /// <param name="targetEntityID">The ID of the target entity or -1 if undefined.</param>
        private IEnumerable<CmdExecutionBase> CreateRepairExecutions(RCSet<SCV> scvsToHandle, RCNumVector targetPosition, int targetEntityID)
        {
            MagicBox magicBox = new MagicBox(new RCSet<Entity>(scvsToHandle), targetPosition);

            Entity targetEntity = scvsToHandle.First().Scenario.GetElementOnMap<Entity>(targetEntityID, MapObjectLayerEnum.GroundObjects, MapObjectLayerEnum.AirObjects);
            if (targetEntity == null)
            {
                /// If there is no target entity -> create simple move executions.
                foreach (SCV scv in scvsToHandle.Where(scv => !scv.IsConstructing))
                {
                    yield return new MoveExecution(scv, magicBox.GetTargetPosition(scv), targetEntityID);
                }
            }

            /// Create the repair command executions if the target is valid for a repair command.
            if (this.IsValidTargetForRepair(targetEntity))
            {
                foreach (SCV scv in scvsToHandle.Where(scv => !scv.IsConstructing))
                {
                    yield return new SCVRepairExecution(scv, targetPosition, targetEntityID);
                }
            }
        }

        /// <summary>
        /// Creates gather executions for the given SCVs.
        /// </summary>
        /// <param name="scvsToHandle">The SCVs to gather.</param>
        /// <param name="targetPosition">The target position.</param>
        /// <param name="targetEntityID">The ID of the target entity or -1 if undefined.</param>
        private IEnumerable<CmdExecutionBase> CreateGatherExecutions(RCSet<SCV> scvsToHandle, RCNumVector targetPosition, int targetEntityID)
        {
            // TODO: implement!
            yield break;
        }

        /// <summary>
        /// Creates build executions for the given SCVs.
        /// </summary>
        /// <param name="scvsToHandle">The SCVs to build.</param>
        /// <param name="topLeftQuadTile">The coordinates of the top-left quadratic tile of the building.</param>
        /// <param name="buildingType">The type of the building.</param>
        private IEnumerable<CmdExecutionBase> CreateBuildExecutions(RCSet<SCV> scvsToHandle, RCIntVector topLeftQuadTile, string buildingType)
        {
            foreach (SCV scv in scvsToHandle.Where(scv => !scv.IsConstructing))
            {
                yield return new SCVStartBuildExecution(scv, buildingType, topLeftQuadTile);
            }
        }

        /// <summary>
        /// Creates stop-build executions for the given SCVs.
        /// </summary>
        /// <param name="scvsToHandle">The SCVs to stop building.</param>
        private IEnumerable<CmdExecutionBase> CreateStopBuildExecutions(RCSet<SCV> scvsToHandle)
        {
            foreach (SCV scv in scvsToHandle.Where(scv => scv.IsConstructing))
            {
                yield return new SCVStopBuildExecution(scv);
            }
        }

        /// <summary>
        /// Creates move executions for the given SCVs.
        /// </summary>
        /// <param name="scvsToHandle">The SCVs to move.</param>
        /// <param name="fullEntitySet">The set of selected entities.</param>
        /// <param name="targetPosition">The target position.</param>
        /// <param name="targetEntityID">The ID of the target entity or -1 if undefined.</param>
        private IEnumerable<CmdExecutionBase> CreateMoveExecutions(RCSet<SCV> scvsToHandle, RCSet<Entity> fullEntitySet, RCNumVector targetPosition, int targetEntityID)
        {
            MagicBox magicBox = new MagicBox(fullEntitySet, targetPosition);
            foreach (SCV scv in scvsToHandle.Where(scv => !scv.IsConstructing))
            {
                yield return new MoveExecution(scv, magicBox.GetTargetPosition(scv), targetEntityID);
            }
        }

        /// <summary>
        /// Creates stop executions for the given SCVs.
        /// </summary>
        /// <param name="scvsToHandle">The SCVs to stop.</param>
        private IEnumerable<CmdExecutionBase> CreateStopExecutions(RCSet<SCV> scvsToHandle)
        {
            foreach (SCV scv in scvsToHandle)
            {
                if (scv.IsConstructing)
                {
                    yield return new SCVStopBuildExecution(scv);
                }
                else
                {
                    yield return new StopExecution(scv);
                }
            }
        }

        /// <summary>
        /// Creates hold executions for the given SCVs.
        /// </summary>
        /// <param name="scvsToHandle">The SCVs to hold.</param>
        private IEnumerable<CmdExecutionBase> CreateHoldExecutions(RCSet<SCV> scvsToHandle)
        {
            foreach (SCV scv in scvsToHandle.Where(scv => !scv.IsConstructing))
            {
                yield return new HoldExecution(scv);
            }
        }

        /// <summary>
        /// Creates attack executions for the given SCVs.
        /// </summary>
        /// <param name="scvsToHandle">The attacking SCVs.</param>
        /// <param name="fullEntitySet">The set of selected entities.</param>
        /// <param name="targetPosition">The target position.</param>
        /// <param name="targetEntityID">The ID of the target entity or -1 if undefined.</param>
        private IEnumerable<CmdExecutionBase> CreateAttackExecutions(RCSet<SCV> scvsToHandle, RCSet<Entity> fullEntitySet, RCNumVector targetPosition, int targetEntityID)
        {
            MagicBox magicBox = new MagicBox(fullEntitySet, targetPosition);
            foreach (SCV scv in scvsToHandle.Where(scv => !scv.IsConstructing))
            {
                yield return new AttackExecution(scv, magicBox.GetTargetPosition(scv), targetEntityID);
            }
        }

        /// <summary>
        /// Creates patrol executions for the given SCVs.
        /// </summary>
        /// <param name="scvsToHandle">The SCVs to patrol.</param>
        /// <param name="fullEntitySet">The set of selected entities.</param>
        /// <param name="targetPosition">The target position.</param>
        private IEnumerable<CmdExecutionBase> CreatePatrolExecutions(RCSet<SCV> scvsToHandle, RCSet<Entity> fullEntitySet, RCNumVector targetPosition)
        {
            MagicBox magicBox = new MagicBox(fullEntitySet, targetPosition);
            foreach (SCV scv in scvsToHandle.Where(scv => !scv.IsConstructing))
            {
                yield return new PatrolExecution(scv, magicBox.GetTargetPosition(scv));
            }
        }

        /// <summary>
        /// Creates command executions for undefined command type.
        /// </summary>
        /// <param name="scvsToHandle">The SCVs to order.</param>
        /// <param name="fullEntitySet">The set of selected entities.</param>
        /// <param name="targetPosition">The target position.</param>
        /// <param name="targetEntityID">The ID of the target entity or -1 if undefined.</param>
        private IEnumerable<CmdExecutionBase> CreateUndefinedExecutions(RCSet<SCV> scvsToHandle, RCSet<Entity> fullEntitySet, RCNumVector targetPosition, int targetEntityID)
        {
            Entity targetEntity = scvsToHandle.First().Scenario.GetElementOnMap<Entity>(targetEntityID, MapObjectLayerEnum.GroundObjects);
            TerranBuilding targetBuilding = targetEntity as TerranBuilding;

            MagicBox magicBox = new MagicBox(fullEntitySet, targetPosition);
            foreach (SCV scv in scvsToHandle.Where(scv => !scv.IsConstructing))
            {
                if (targetBuilding != null && targetBuilding.Owner == scv.Owner &&
                    targetBuilding.ConstructionJob != null && !targetBuilding.ConstructionJob.IsFinished &&
                    targetBuilding.ConstructionJob.AttachedSCV == null)
                {
                    /// The target entity is a friendly Terran building that is under construction but its construction is
                    /// not currently in progress -> start a continue build command.
                    yield return new SCVContinueBuildExecution(scv, targetPosition, targetBuilding.ID.Read());
                }
                else if (targetEntity != null && targetEntity.Owner == scv.Owner && this.IsValidTargetForRepair(targetEntity))
                {
                    /// The target entity is a friendly entity and is valid for a repair command -> start a repair command.
                    yield return new SCVRepairExecution(scv, targetPosition, targetEntityID);
                }
                else if (targetEntity != null && targetEntity.Owner != null && targetEntity.Owner != scv.Owner)
                {
                    /// The target entity is an enemy entity -> start an attack execution.
                    yield return new AttackExecution(scv, magicBox.GetTargetPosition(scv), targetEntityID);
                }
                else
                {
                    /// In any other cases -> start a move execution.
                    yield return new MoveExecution(scv, magicBox.GetTargetPosition(scv), targetEntityID);
                }
                /// TODO: Handle the cases for Repair, Gather and Return commands!
            }
        }

        #endregion Execution creator methods

        /// <summary>
        /// Checks whether the given entity is a valid target for a repair command.
        /// </summary>
        /// <param name="checkedEntity">The entity to be checked.</param>
        /// <returns>True if the given entity is a valid target for a repair command; otherwise false.</returns>
        private bool IsValidTargetForRepair(Entity checkedEntity)
        {
            /// Check if the target is a damaged Terran building or addon that is not under construction or is a damaged Terran mechanical unit.
            // TODO: Check for additional unit types as well!
            return (checkedEntity is Addon ||
                    checkedEntity is TerranBuilding ||
                    checkedEntity is SCV ||
                    checkedEntity is Goliath) &&
                  !checkedEntity.Biometrics.IsUnderConstruction &&
                   checkedEntity.Biometrics.HP != -1 &&
                   checkedEntity.Biometrics.HP < checkedEntity.ElementType.MaxHP.Read();
        }

        /// <summary>
        /// The type of the command execution that this factory creates.
        /// </summary>
        private readonly SCVCommandEnum commandType;
    }
}
