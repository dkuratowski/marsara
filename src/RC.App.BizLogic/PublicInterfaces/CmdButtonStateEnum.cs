using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.PublicInterfaces
{
    /// <summary>
    /// Enumerates the possible states of a command button.
    /// </summary>
    public enum CmdButtonStateEnum
    {
        NotVisible = 0,     // The command button is not visible.
        Enabled = 1,        // The command button is visible and enabled.
        Disabled = 2,       // The command button is visible but disabled.
        Highlighted = 3     // The command button is visible, enabled and highlighted.
    }
}
