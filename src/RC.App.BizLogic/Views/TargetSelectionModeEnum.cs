using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.Views
{
    /// <summary>
    /// Enumerates the possible target selection modes.
    /// </summary>
    public enum TargetSelectionModeEnum
    {
        NoTargetSelection = 0,          /// There is no target selection currently in progress.
        TargetPositionSelection = 1,    /// Selection of a target position on the map is currently in progress.
        BuildingLocationSelection = 2   /// Selection of a location for a building is currently in progress.
    }
}
