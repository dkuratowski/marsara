using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.Metadata;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// Represents a button listener that starts producing an entity of a given type in the selected building when the appropriate
    /// button is pressed on the command panel.
    /// </summary>
    class ProductionButtonListener : ButtonListener
    {
        /// <summary>
        /// Gets the type of the product selected by this listener.
        /// </summary>
        public string SelectedProductType { get { return this.selectedProductTypeName; } }

        /// <see cref="ButtonListener.ButtonAvailability"/>
        public override AvailabilityEnum ButtonAvailability
        {
            get
            {
                return this.commandExecutor.GetCommandAvailability(
                    this.scenarioManagerBC.ActiveScenario,
                    ProductionButtonListener.COMMAND_TYPE,
                    this.selectedProductTypeName,
                    this.selectionManagerBC.CurrentSelection);
            }
        }

        /// <see cref="CommandInputListener.TryComplete"/>
        public override CommandInputListener.CompletionResultEnum TryComplete()
        {
            /// First we have to check the resources of the local player.
            Player localPlayer = this.scenarioManagerBC.ActiveScenario.GetPlayer((int)this.selectionManagerBC.LocalPlayer);
            IScenarioElementType selectedProductType = this.scenarioLoader.Metadata.GetElementType(this.selectedProductTypeName);
            int mineralsNeeded = selectedProductType.MineralCost != null ? selectedProductType.MineralCost.Read() : 0;
            int vespeneGasNeeded = selectedProductType.GasCost != null ? selectedProductType.GasCost.Read() : 0;
            int supplyNeeded = selectedProductType.SupplyUsed != null ? selectedProductType.SupplyUsed.Read() : 0;
            if (mineralsNeeded > localPlayer.Minerals || vespeneGasNeeded > localPlayer.VespeneGas || localPlayer.UsedSupply + supplyNeeded > localPlayer.TotalSupply)
            {
                /// Insufficient resources or supply (TODO: send and error message up to the user!)
                return CommandInputListener.CompletionResultEnum.FailedButContinue;
            }
            
            /// Resources are OK.
            this.CommandBuilder.CommandType = COMMAND_TYPE;
            this.CommandBuilder.Parameter = this.selectedProductTypeName;
            return CommandInputListener.CompletionResultEnum.Succeeded;
        }

        /// <see cref="CommandInputListener.Init"/>
        protected override void Init(XElement listenerElem)
        {
            base.Init(listenerElem);

            XAttribute productAttr = listenerElem.Attribute(PRODUCT_ATTR);
            if (productAttr == null) { throw new InvalidOperationException("Product type not defined for a production button listener!"); }

            this.selectedProductTypeName = productAttr.Value;
            this.commandExecutor = ComponentManager.GetInterface<ICommandExecutor>();
            this.selectionManagerBC = ComponentManager.GetInterface<ISelectionManagerBC>();
            this.scenarioManagerBC = ComponentManager.GetInterface<IScenarioManagerBC>();
            this.scenarioLoader = ComponentManager.GetInterface<IScenarioLoader>();
        }

        /// <summary>
        /// The name of the type of the product selected by this listener.
        /// </summary>
        private string selectedProductTypeName;

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
        /// Reference to the scenario loader component.
        /// </summary>
        private IScenarioLoader scenarioLoader;

        /// <summary>
        /// The supported XML-nodes and attributes.
        /// </summary>
        private const string PRODUCT_ATTR = "product";

        /// <summary>
        /// The type of the command selected by this type of listeners.
        /// </summary>
        private const string COMMAND_TYPE = "StartProduction";
    }
}
