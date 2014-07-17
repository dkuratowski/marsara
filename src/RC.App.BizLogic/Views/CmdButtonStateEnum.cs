using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.Views
{
    /// <summary>
    /// Enumerates the possible states of a command button.
    /// </summary>
    public enum CmdButtonStateEnum
    {
        None = 0,           // The command button doesn't exist.
        Enabled = 1,        // The command button is enabled.
        Disabled = 2,       // The command button is disabled.
        Highlighted = 3     // The command button is enabled and highlighted.
    }
}
