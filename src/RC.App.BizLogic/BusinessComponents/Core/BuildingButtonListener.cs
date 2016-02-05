using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using RC.Common.ComponentModel;
using RC.Common.Configuration;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.Metadata;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// Represents a button listener that selects a building type when the appropriate button is pressed on the command panel.
    /// </summary>
    class BuildingButtonListener : ButtonListener
    {
        /// <see cref="ButtonListener.ButtonAvailability"/>
        public override AvailabilityEnum ButtonAvailability
        {
            get
            {
                return this.commandExecutor.GetCommandAvailability(
                    this.scenarioManagerBC.ActiveScenario,
                    BuildingButtonListener.COMMAND_TYPE,
                    this.selectedBuildingTypeName,
                    this.selectionManagerBC.CurrentSelection);
            }
        }

        /// <see cref="CommandInputListener.CheckCompletionStatus"/>
        public override bool CheckCompletionStatus()
        {
            return this.commandExecutor.GetCommandAvailability(
                this.scenarioManagerBC.ActiveScenario,
                BuildingButtonListener.COMMAND_TYPE,
                this.selectedBuildingTypeName,
                this.selectionManagerBC.CurrentSelection) == AvailabilityEnum.Enabled;
        }

        /// <see cref="CommandInputListener.TryComplete"/>
        public override CommandInputListener.CompletionResultEnum TryComplete()
        {
            /// First we have to check the resources of the local player.
            Player localPlayer = this.scenarioManagerBC.ActiveScenario.GetPlayer((int)this.selectionManagerBC.LocalPlayer);
            IBuildingType selectedBuildingType = localPlayer.Metadata.GetBuildingType(this.selectedBuildingTypeName);
            int mineralsNeeded = selectedBuildingType.MineralCost != null ? selectedBuildingType.MineralCost.Read() : 0;
            int vespeneGasNeeded = selectedBuildingType.GasCost != null ? selectedBuildingType.GasCost.Read() : 0;
            int supplyNeeded = selectedBuildingType.SupplyUsed != null ? selectedBuildingType.SupplyUsed.Read() : 0;
            if (mineralsNeeded > localPlayer.Minerals || vespeneGasNeeded > localPlayer.VespeneGas || (supplyNeeded > 0 && localPlayer.UsedSupply + supplyNeeded > localPlayer.TotalSupply))
            {
                /// Insufficient resources or supply (TODO: send and error message up to the user!)
                return CommandInputListener.CompletionResultEnum.FailedButContinue;
            }

            /// Resources are OK.
            this.CommandBuilder.Parameter = this.selectedBuildingTypeName;
            return CommandInputListener.CompletionResultEnum.Succeeded;
        }

        /// <see cref="CommandInputListener.Init"/>
        protected override void Init(XElement listenerElem)
        {
            base.Init(listenerElem);

            XAttribute buildingAttr = listenerElem.Attribute(BUILDING_ATTR);
            if (buildingAttr == null) { throw new InvalidOperationException("Building type not defined for a building button listener!"); }

            this.selectedBuildingTypeName = buildingAttr.Value;
            this.commandExecutor = ComponentManager.GetInterface<ICommandExecutor>();
            this.selectionManagerBC = ComponentManager.GetInterface<ISelectionManagerBC>();
            this.scenarioManagerBC = ComponentManager.GetInterface<IScenarioManagerBC>();
        }

        /// <summary>
        /// The name of the type of the building selected by this listener.
        /// </summary>
        private string selectedBuildingTypeName;

        /// <summary>
        /// Reference to the command executor component.
        /// </summary>
        private ICommandExecutor commandExecutor;

        /// <summary>
        /// Reference to the selection manager business component.
        /// </summary>
        private ISelectionManagerBC selectionManagerBC;

        /// <summary>
        /// Reference to the scenario manager business component.
        /// </summary>
        private IScenarioManagerBC scenarioManagerBC;

        /// <summary>
        /// The supported XML-nodes and attributes.
        /// </summary>
        private const string BUILDING_ATTR = "building";

        /// <summary>
        /// The type of the command selected by this type of listeners.
        /// </summary>
        private const string COMMAND_TYPE = "Build";
    }
}
