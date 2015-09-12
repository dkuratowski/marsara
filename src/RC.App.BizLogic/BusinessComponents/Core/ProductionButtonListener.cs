using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.ComponentInterfaces;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// Represents a button listener that starts creating a unit of a given type in the selected building when the appropriate
    /// button is pressed on the command panel.
    /// </summary>
    class ProductionButtonListener : ButtonListener
    {
        /// <see cref="ButtonListener.ButtonAvailability"/>
        public override AvailabilityEnum ButtonAvailability
        {
            get
            {
                return this.commandExecutor.GetCommandAvailability(
                    this.scenarioManagerBC.ActiveScenario,
                    ProductionButtonListener.COMMAND_TYPE,
                    this.selectionManagerBC.CurrentSelection);
            }
        }

        /// <see cref="ButtonListener.IsHighlighted"/>
        public override bool IsHighlighted
        {
            get
            {
                return this.commandExecutor.GetCommandsBeingExecuted(
                    scenarioManagerBC.ActiveScenario,
                    selectionManagerBC.CurrentSelection)
                    .Contains(ProductionButtonListener.COMMAND_TYPE);
            }
        }

        /// <see cref="CommandInputListener.CheckCompletionStatus"/>
        /// TODO: implement this method!
        public override bool CheckCompletionStatus() { return true; }

        /// <see cref="CommandInputListener.TryComplete"/>
        /// TODO: implement this method!
        public override CommandInputListener.CompletionResultEnum TryComplete()
        {
            this.CommandBuilder.CommandType = COMMAND_TYPE;
            this.CommandBuilder.Parameter = this.selectedProductType;
            return CommandInputListener.CompletionResultEnum.Succeeded;
        }

        /// <see cref="CommandInputListener.Init"/>
        protected override void Init(XElement listenerElem)
        {
            base.Init(listenerElem);

            XAttribute productAttr = listenerElem.Attribute(PRODUCT_ATTR);
            if (productAttr == null) { throw new InvalidOperationException("Product type not defined for a production button listener!"); }

            this.selectedProductType = productAttr.Value;
            this.commandExecutor = ComponentManager.GetInterface<ICommandExecutor>();
            this.selectionManagerBC = ComponentManager.GetInterface<ISelectionManagerBC>();
            this.scenarioManagerBC = ComponentManager.GetInterface<IScenarioManagerBC>();
        }

        /// <summary>
        /// The type of the command selected by this listener.
        /// </summary>
        private string selectedProductType;

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
        private const string PRODUCT_ATTR = "product";

        /// <summary>
        /// The type of the command selected by this type of listeners.
        /// </summary>
        private const string COMMAND_TYPE = "StartProduction";
    }
}
