using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Engine.Pathfinder.Core
{
    /// <summary>
    /// Enumerates the possible states of a path.
    /// </summary>
    enum PathStatusEnum
    {
        Partial = 0,        /// The path is partially calculated.
        Complete = 1,       /// The path is completely calculated.
        Broken = 2,         /// The path is broken.
    }
}
