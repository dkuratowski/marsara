using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using RC.Common.Configuration;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// Represents a button listener that selects a command type when the appropriate button is pressed on the command panel.
    /// </summary>
    class CommandButtonListener : ButtonListener
    {
        /// <see cref="CommandInputListener.CheckCompletionStatus"/>
        /// TODO: implement this method!
        public override bool CheckCompletionStatus() { return true; }

        /// <see cref="CommandInputListener.TryComplete"/>
        /// TODO: implement this method!
        public override bool TryComplete()
        {
            this.CommandBuilder.CommandType = this.selectedCommandType;
            return true;
        }

        /// <see cref="CommandInputListener.Init"/>
        protected override void Init(XElement listenerElem)
        {
            base.Init(listenerElem);

            XAttribute commandAttr = listenerElem.Attribute(COMMAND_ATTR);
            if (commandAttr == null) { throw new InvalidOperationException("Command type not defined for a command button listener!"); }
            this.selectedCommandType = commandAttr.Value;
        }

        /// <summary>
        /// The type of the command selected by this listener.
        /// </summary>
        private string selectedCommandType;

        /// <summary>
        /// The supported XML-nodes and attributes.
        /// </summary>
        private const string COMMAND_ATTR = "command";
    }
}
