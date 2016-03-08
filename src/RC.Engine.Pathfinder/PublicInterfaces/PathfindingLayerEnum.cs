using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Engine.Pathfinder.PublicInterfaces
{
    /// <summary>
    /// Enumerates the possible layers of the pathfinding grid.
    /// </summary>
    public enum PathfindingLayerEnum
    {
        Ground = 0,     /// The ground layer.
        Air = 1         /// The air layer.
    }
}
