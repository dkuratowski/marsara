using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Engine.Pathfinder.PublicInterfaces
{
    /// <summary>
    /// The interface of an obstacle on the pathfinding grid.
    /// </summary>
    public interface IObstacle
    {
        /// <summary>
        /// Gets the area occupied by this obstacle on the pathfinding grid.
        /// </summary>
        RCIntRectangle Area { get; }
    }
}
