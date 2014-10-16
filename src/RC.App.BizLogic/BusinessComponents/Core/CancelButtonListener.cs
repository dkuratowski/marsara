using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using RC.Common.Configuration;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// Represents a button listener that cancels the current command input process when the appropriate button is pressed on the command panel.
    /// </summary>
    class CancelButtonListener : ButtonListener
    {
        /// <see cref="CommandInputListener.TryComplete"/>
        public override bool TryComplete() { return false; }
    }
}
