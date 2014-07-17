using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.Views
{
    /// <summary>
    /// Enumerates the possible mouse input modes during gameplay.
    /// </summary>
    public enum MouseInputModeEnum
    {
        NormalInputMode = 0,            /// Normal input mode (left-click: map object selection; right-click: fast command)
        TargetPositionInputMode = 1,    /// Position input mode (left-click: target position selection; right-click: back to normal mode)
        BuildPositionInputMode = 2      /// Build position input mode (left-click: build position selection; right-click: back to normal mode)
    }
}
