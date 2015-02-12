using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.PresLogic
{
    /// <summary>
    /// The common interface of the minimap background jobs.
    /// </summary>
    interface IMinimapBackgroundJob
    {
        /// <summary>
        /// Executes this job. This method will be called from the minimap background thread.
        /// </summary>
        void Execute();
    }
}
