using RC.Common.ComponentModel;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// Represents a button listener that cancels the construction of the selected entity.
    /// </summary>
    class CancelConstructionButtonListener : ButtonListener
    {
        /// <see cref="ButtonListener.ButtonAvailability"/>
        public override AvailabilityEnum ButtonAvailability
        {
            get
            {
                return this.commandExecutor.GetCommandAvailability(
                    this.scenarioManagerBC.ActiveScenario,
                    COMMAND_TYPE,
                    null,
                    this.selectionManagerBC.CurrentSelection);
            }
        }

        /// <see cref="CommandInputListener.TryComplete"/>
        public override CommandInputListener.CompletionResultEnum TryComplete()
        {
            int[] currentSelection = this.selectionManagerBC.CurrentSelection.ToArray();
            if (currentSelection.Length != 1) { throw new InvalidOperationException("CancelConstructionButtonListener completion denied because the number of the currently selected entities is not 1!"); }

            Entity selectedEntity = this.scenarioManagerBC.ActiveScenario.GetElement<Entity>(currentSelection[0]);
            if (selectedEntity == null) { throw new InvalidOperationException("CancelConstructionButtonListener completion denied because the currently selected entity doesn't exist!"); }

            if (!selectedEntity.Biometrics.IsUnderConstruction) { throw new InvalidOperationException("CancelConstructionButtonListener completion denied because the currently selected entity is not under construction!"); }

            this.CommandBuilder.CommandType = COMMAND_TYPE;
            return CommandInputListener.CompletionResultEnum.Succeeded;
        }

        /// <see cref="CommandInputListener.Init"/>
        protected override void Init(XElement listenerElem)
        {
            base.Init(listenerElem);

            this.commandExecutor = ComponentManager.GetInterface<ICommandExecutor>();
            this.selectionManagerBC = ComponentManager.GetInterface<ISelectionManagerBC>();
            this.scenarioManagerBC = ComponentManager.GetInterface<IScenarioManagerBC>();
        }

        /// <summary>
        /// Reference to the selection manager business component.
        /// </summary>
        private ISelectionManagerBC selectionManagerBC;

        /// <summary>
        /// Reference to the scenario manager business component.
        /// </summary>
        private IScenarioManagerBC scenarioManagerBC;

        /// <summary>
        /// Reference to the command executor component.
        /// </summary>
        private ICommandExecutor commandExecutor;

        /// <summary>
        /// The type of the command handled by this listener.
        /// </summary>
        private const string COMMAND_TYPE = "CancelConstruction";
    }
}
