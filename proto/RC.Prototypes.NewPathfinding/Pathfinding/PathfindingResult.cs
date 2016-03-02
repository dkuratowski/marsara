using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Prototypes.NewPathfinding.Pathfinding
{
    /// <summary>
    /// Contains informations about the result of a pathfinding.
    /// </summary>
    class PathfindingResult<TNode>
    {
        public long ElapsedTime;
        public List<TNode> Path;
        public List<TNode> ExploredNodes;
    }
}
