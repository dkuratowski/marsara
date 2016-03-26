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
        ReadyToFollow = 0,  /// The path is ready to be followed.
        Finished = 1,       /// Path following has been finished.
        Calculating = 2,    /// The path is being calculated.
        Broken = 3,         /// The path has been broken and no longer can be followed.
    }
}
