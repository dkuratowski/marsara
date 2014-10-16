using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.BusinessComponents;
using RC.App.BizLogic.BusinessComponents.Core;
using RC.Common;
using RC.Common.ComponentModel;

namespace RC.App.BizLogic.Services.Core
{
    /// <summary>
    /// The implementation of the command service.
    /// </summary>
    [Component("RC.App.BizLogic.CommandService")]
    class CommandService : ICommandService, IComponent
    {
        #region IComponent methods

        /// <see cref="IComponent.Start"/>
        public void Start()
        {
            this.scenarioManager = ComponentManager.GetInterface<IScenarioManagerBC>();
            this.selectionManager = ComponentManager.GetInterface<ISelectionManagerBC>();
            this.commandManager = ComponentManager.GetInterface<ICommandManagerBC>();

            /// TODO: Temporary reference for sending fast commands!
            this.multiplayerService = ComponentManager.GetInterface<IMultiplayerService>();
        }

        /// <see cref="IComponent.Stop"/>
        public void Stop()
        {
        }

        #endregion IComponent methods

        #region ICommandService methods

        /// <see cref="ICommandService.Select"/>
        public void Select(RCIntRectangle displayedArea, RCIntVector position)
        {
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }

            RCIntVector mapSize = this.scenarioManager.ActiveScenario.Map.CellSize * CoordTransformationHelper.PIXEL_PER_NAVCELL_VECT;
            if (!new RCIntRectangle(0, 0, mapSize.X, mapSize.Y).Contains(displayedArea)) { throw new ArgumentOutOfRangeException("displayedArea"); }

            RCIntRectangle cellWindow;
            RCIntVector displayOffset;
            CoordTransformationHelper.CalculateCellWindow(displayedArea, out cellWindow, out displayOffset);

            RCNumVector mapCoords = new RCNumVector((displayedArea + position).X / BizLogicConstants.PIXEL_PER_NAVCELL,
                                                    (displayedArea + position).Y / BizLogicConstants.PIXEL_PER_NAVCELL) - CoordTransformationHelper.HALF_VECT;
            this.selectionManager.SelectEntity(mapCoords);
        }

        /// <see cref="ICommandService.Select"/>
        public void Select(RCIntRectangle displayedArea, RCIntRectangle selectionBox)
        {
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }

            RCIntVector mapSize = this.scenarioManager.ActiveScenario.Map.CellSize * CoordTransformationHelper.PIXEL_PER_NAVCELL_VECT;
            if (!new RCIntRectangle(0, 0, mapSize.X, mapSize.Y).Contains(displayedArea)) { throw new ArgumentOutOfRangeException("displayedArea"); }

            RCIntRectangle cellWindow;
            RCIntVector displayOffset;
            CoordTransformationHelper.CalculateCellWindow(displayedArea, out cellWindow, out displayOffset);

            RCNumRectangle mapCoords =
                new RCNumRectangle(
                    new RCNumVector((displayedArea + selectionBox.Location).X / BizLogicConstants.PIXEL_PER_NAVCELL,
                                    (displayedArea + selectionBox.Location).Y / BizLogicConstants.PIXEL_PER_NAVCELL) - CoordTransformationHelper.HALF_VECT,
                    new RCNumVector((RCNumber)selectionBox.Size.X / (RCNumber)BizLogicConstants.PIXEL_PER_NAVCELL,
                                    (RCNumber)selectionBox.Size.Y / (RCNumber)BizLogicConstants.PIXEL_PER_NAVCELL));
            this.selectionManager.SelectEntities(mapCoords);
        }

        /// <see cref="ICommandService.SelectType"/>
        public void SelectType(RCIntRectangle displayedArea, RCIntVector position)
        {
            /// TODO: implement!
        }

        /// <see cref="ICommandService.SendFastCommand"/>
        public void SendFastCommand(RCIntRectangle displayedArea, RCIntVector position)
        {
            /// TODO: This is a PROTOTYPE CODE!
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }

            RCIntVector mapSize = this.scenarioManager.ActiveScenario.Map.CellSize * CoordTransformationHelper.PIXEL_PER_NAVCELL_VECT;
            if (!new RCIntRectangle(0, 0, mapSize.X, mapSize.Y).Contains(displayedArea)) { throw new ArgumentOutOfRangeException("displayedArea"); }

            if (this.selectionManager.CurrentSelection.Count != 0)
            {
                RCIntRectangle cellWindow;
                RCIntVector displayOffset;
                CoordTransformationHelper.CalculateCellWindow(displayedArea, out cellWindow, out displayOffset);

                RCIntVector navCellCoords = new RCIntVector((displayedArea + position).X / BizLogicConstants.PIXEL_PER_NAVCELL,
                                                            (displayedArea + position).Y / BizLogicConstants.PIXEL_PER_NAVCELL);

                this.multiplayerService.PostCommand(RCCommand.Create(RC.App.BizLogic.Services.FastCommand.MNEMONIC,
                                                                     this.selectionManager.CurrentSelection.ToArray(),
                                                                     navCellCoords,
                                                                     -1,
                                                                     null));
            }
        }

        /// <see cref="ICommandService.PressCommandButton"/>
        public void PressCommandButton(RCIntVector panelPosition)
        {
            this.commandManager.PressCommandButton(panelPosition);
        }

        /// <see cref="ICommandService.SelectTargetPosition"/>
        public void SelectTargetPosition(RCIntRectangle displayedArea, RCIntVector position)
        {
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }

            RCIntVector mapSize = this.scenarioManager.ActiveScenario.Map.CellSize * CoordTransformationHelper.PIXEL_PER_NAVCELL_VECT;
            if (!new RCIntRectangle(0, 0, mapSize.X, mapSize.Y).Contains(displayedArea)) { throw new ArgumentOutOfRangeException("displayedArea"); }

            RCIntRectangle cellWindow;
            RCIntVector displayOffset;
            CoordTransformationHelper.CalculateCellWindow(displayedArea, out cellWindow, out displayOffset);

            RCIntVector navCellCoords = new RCIntVector((displayedArea + position).X / BizLogicConstants.PIXEL_PER_NAVCELL,
                                                        (displayedArea + position).Y / BizLogicConstants.PIXEL_PER_NAVCELL);
            this.commandManager.SelectTargetPosition(navCellCoords);
        }

        /// <see cref="ICommandService.CancelSelectingTargetPosition"/>
        public void CancelSelectingTargetPosition()
        {
            this.commandManager.CancelSelectingTargetPosition();
        }

        #endregion ICommandService methods

        /// <summary>
        /// Reference to the scenario manager business component.
        /// </summary>
        private IScenarioManagerBC scenarioManager;

        /// <summary>
        /// Reference to the selection manager business component.
        /// </summary>
        private ISelectionManagerBC selectionManager;

        /// <summary>
        /// Reference to the command manager business component.
        /// </summary>
        private ICommandManagerBC commandManager;

        /// <summary>
        /// TODO: Temporary reference for sending fast commands!
        /// </summary>
        private IMultiplayerService multiplayerService;
    }
}
