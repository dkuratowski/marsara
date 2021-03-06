﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using RC.App.BizLogic.Views;
using RC.Common.ComponentModel;
using RC.Common.Configuration;
using RC.Engine.Simulator.ComponentInterfaces;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// Represents a button listener that selects a command type when the appropriate button is pressed on the command panel.
    /// </summary>
    class CommandButtonListener : ButtonListener
    {
        /// <see cref="ButtonListener.ButtonAvailability"/>
        public override AvailabilityEnum ButtonAvailability
        {
            get
            {
                return this.commandExecutor.GetCommandAvailability(
                    this.scenarioManagerBC.ActiveScenario,
                    this.selectedCommandType,
                    null,
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
                    .Contains(this.selectedCommandType);
            }
        }

        /// <see cref="CommandInputListener.CheckCompletionStatus"/>
        public override bool CheckCompletionStatus()
        {
            return this.commandExecutor.GetCommandAvailability(
                this.scenarioManagerBC.ActiveScenario,
                this.selectedCommandType,
                null,
                this.selectionManagerBC.CurrentSelection) == AvailabilityEnum.Enabled;
        }

        /// <see cref="CommandInputListener.TryComplete"/>
        /// TODO: implement this method!
        public override CommandInputListener.CompletionResultEnum TryComplete()
        {
            this.CommandBuilder.CommandType = this.selectedCommandType;
            return CommandInputListener.CompletionResultEnum.Succeeded;
        }

        /// <see cref="CommandInputListener.Init"/>
        protected override void Init(XElement listenerElem)
        {
            base.Init(listenerElem);

            XAttribute commandAttr = listenerElem.Attribute(COMMAND_ATTR);
            if (commandAttr == null) { throw new InvalidOperationException("Command type not defined for a command button listener!"); }

            this.selectedCommandType = commandAttr.Value;
            this.commandExecutor = ComponentManager.GetInterface<ICommandExecutor>();
            this.selectionManagerBC = ComponentManager.GetInterface<ISelectionManagerBC>();
            this.scenarioManagerBC = ComponentManager.GetInterface<IScenarioManagerBC>();
        }

        /// <summary>
        /// The type of the command selected by this listener.
        /// </summary>
        private string selectedCommandType;

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
        private const string COMMAND_ATTR = "command";
    }
}
