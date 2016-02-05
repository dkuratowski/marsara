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
using RC.Engine.Simulator.Terran.Units;

namespace RC.Engine.Simulator.Terran.Commands
{
    /// <summary>
    /// Responsible for executing start build commands for SCVs.
    /// </summary>
    class SCVStartBuildCmdExecution : CmdExecutionBase
    {
        /// <summary>
        /// Creates a SCVStartBuildCmdExecution instance.
        /// </summary>
        /// <param name="recipientSCV">The recipient SCV of this command execution.</param>
        /// <param name="buildingType">The typename of the building.</param>
        /// <param name="topLeftQuadTile">The coordinates of the top-left quadratic tile of the building to be created.</param>
        public SCVStartBuildCmdExecution(SCV recipientSCV, string buildingType, RCIntVector topLeftQuadTile)
            : base(new RCSet<Entity> { recipientSCV })
        {
            this.recipientSCV = this.ConstructField<SCV>("recipientSCV");
            this.topLeftQuadTile = this.ConstructField<RCIntVector>("topLeftQuadTile");
            this.targetPosition = this.ConstructField<RCNumVector>("targetPosition");
            this.status = this.ConstructField<byte>("status");
            this.timeToNextScvMove = this.ConstructField<int>("timeToNextScvMove");
            this.recipientSCV.Write(recipientSCV);
            this.topLeftQuadTile.Write(topLeftQuadTile);
            this.targetPosition.Write(RCNumVector.Undefined);
            this.status.Write(0xFF);
            this.buildingTypeName = buildingType;
            this.timeToNextScvMove.Write(0);
        }

        #region Overrides

        /// <see cref="CmdExecutionBase.ContinueImpl"/>
        protected override bool ContinueImpl()
        {
            if (this.status.Read() == MOVING_TO_TARGET)
            {
                if (!this.recipientSCV.Read().MotionControl.IsMoving)
                {
                    if (this.recipientSCV.Read().MotionControl.PositionVector.Read() != this.targetPosition.Read())
                    {
                        /// SCV could not reach the target position -> finish this command execution.
                        return true;
                    }

                    IElementFactory elementFactory = ComponentManager.GetInterface<IElementFactory>();
                    if (!elementFactory.CreateElement(this.buildingTypeName, this.topLeftQuadTile.Read(), this.recipientSCV.Read()))
                    {
                        /// The requested building could not be created -> finish this command execution.
                        return true;
                    }

                    this.status.Write(CONSTRUCTING);
                }
                return false;
            }
            else if (this.status.Read() == CONSTRUCTING)
            {
                Building buildingUnderConstruction = this.recipientSCV.Read().BuildingUnderConstruction;
                if (this.recipientSCV.Read().ContinueConstruct())
                {
                    /// The SCV has finished the construction -> move it to a free place and finish this command execution.
                    this.MoveScvToFreePlace(buildingUnderConstruction);
                    this.status.Write(MOVING_TO_FREE_PLACE);
                    return false;
                }

                /// Continue the construction activity of the SCV.
                this.MakeScvActivityDuringConstruction();
                return false;
            }
            else if (this.status.Read() == MOVING_TO_FREE_PLACE)
            {
                /// Wait while the SCV is still moving.
                return !this.recipientSCV.Read().MotionControl.IsMoving;
            }
            else
            {
                throw new InvalidOperationException("Impossible case happened!");
            }
        }

        /// <see cref="CmdExecutionBase.GetContinuation"/>
        protected override CmdExecutionBase GetContinuation()
        {
            return new StopExecution(this.recipientSCV.Read());
        }

        /// <see cref="CmdExecutionBase.InitializeImpl"/>
        protected override void InitializeImpl()
        {
            IBuildingType buildingType = this.recipientSCV.Read().Owner.Metadata.GetBuildingType(this.buildingTypeName);
            RCIntVector entityQuadSize = this.Scenario.Map.CellToQuadSize(buildingType.Area.Read());
            RCIntRectangle targetPositionQuadRect = new RCIntRectangle(this.topLeftQuadTile.Read(), entityQuadSize);
            RCNumRectangle targetPositionRect = (RCNumRectangle)this.Scenario.Map.QuadToCellRect(targetPositionQuadRect) - new RCNumVector(1, 1) / 2;
            this.targetPosition.Write(targetPositionRect.Location + (targetPositionRect.Size / 2));
            this.recipientSCV.Read().MotionControl.StartMoving(this.targetPosition.Read());
            this.status.Write(MOVING_TO_TARGET);
        }

        #endregion Overrides

        /// <summary>
        /// 
        /// </summary>
        private void MakeScvActivityDuringConstruction()
        {
            /// Decrease the timer.
            if (this.timeToNextScvMove.Read() > 0)
            {
                this.timeToNextScvMove.Write(this.timeToNextScvMove.Read() - 1);
                return;
            }

            /// Generate a random position inside the building area and move the SCV there.
            /// TODO: do not use the default random generator because the engine shall be deterministic!
            RCIntRectangle buildingQuadRect = this.recipientSCV.Read().BuildingUnderConstruction.MapObject.QuadraticPosition;
            RCIntRectangle buildingCellRect = this.Scenario.Map.QuadToCellRect(buildingQuadRect);
            RCNumVector movePosition = new RCNumVector(
                RandomService.DefaultGenerator.Next(buildingCellRect.Left, buildingCellRect.Right),
                RandomService.DefaultGenerator.Next(buildingCellRect.Top, buildingCellRect.Bottom));
            this.recipientSCV.Read().MotionControl.StartMoving(movePosition);

            /// Reset the timer.
            this.timeToNextScvMove.Write(TIME_BETWEEN_SCV_MOVES);
        }

        /// <summary>
        /// Moves the SCV to a free place if construction finished.
        /// </summary>
        /// <param name="buildingUnderConstruction">The building under construction.</param>
        private void MoveScvToFreePlace(Building buildingUnderConstruction)
        {
            /// Do nothing if the building has been destroyed in the meantime.
            if (buildingUnderConstruction == null || buildingUnderConstruction.Scenario == null) { return; }

            /// Otherwise find a free place for the SCV.
            EntityNeighbourhoodIterator cellIterator = new EntityNeighbourhoodIterator(buildingUnderConstruction);
            IEnumerator<ICell> cellEnumerator = cellIterator.GetEnumerator();

            RCIntVector targetCell = RCIntVector.Undefined;
            while (cellEnumerator.MoveNext())
            {
                if (this.recipientSCV.Read().MotionControl.ValidatePosition(cellEnumerator.Current.MapCoords))
                {
                    targetCell = cellEnumerator.Current.MapCoords;
                    break;
                }
            }

            if (targetCell != RCNumVector.Undefined)
            {
                /// Move the SCV to the target cell if found.
                this.recipientSCV.Read().MotionControl.StartMoving(targetCell);
            }
            else
            {
                /// Otherwise move the SCV to the bottom-left corner of the building.
                RCIntRectangle buildingQuadRect = buildingUnderConstruction.MapObject.QuadraticPosition;
                RCIntRectangle buildingCellRect = this.Scenario.Map.QuadToCellRect(buildingQuadRect);
                this.recipientSCV.Read().MotionControl.StartMoving(new RCNumVector(buildingCellRect.Left, buildingCellRect.Bottom - 1));
            }
        }

        /// <summary>
        /// Reference to the recipient SCV of this command execution.
        /// </summary>
        private readonly HeapedValue<SCV> recipientSCV;

        /// <summary>
        /// The coordinates of the top-left quadratic tile of the building to be created.
        /// </summary>
        private readonly HeapedValue<RCIntVector> topLeftQuadTile;

        /// <summary>
        /// The target position on the map where the SCV shall start construction.
        /// </summary>
        private readonly HeapedValue<RCNumVector> targetPosition;

        /// <summary>
        /// The current status of this command execution.
        /// </summary>
        private readonly HeapedValue<byte> status;

        /// <summary>
        /// Time to the next SCV move operation during construction.
        /// </summary>
        private readonly HeapedValue<int> timeToNextScvMove;

        /// <summary>
        /// The typename of the building.
        /// </summary>
        /// TODO: heap this field!
        private readonly string buildingTypeName;

        /// <summary>
        /// The bytes that indicates the status of this command execution.
        /// </summary>
        private const byte MOVING_TO_TARGET = 0x00;
        private const byte CONSTRUCTING = 0x01;
        private const byte MOVING_TO_FREE_PLACE = 0x02;

        /// <summary>
        /// The number of frames between SCV move operations.
        /// </summary>
        private const int TIME_BETWEEN_SCV_MOVES = 60;
    }
}
