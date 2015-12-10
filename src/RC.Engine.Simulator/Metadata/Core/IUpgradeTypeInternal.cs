using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Metadata.Core
{
    /// <summary>
    /// Internal interface of the upgrade types defined in the metadata.
    /// </summary>
    interface IUpgradeTypeInternal : IScenarioElementTypeInternal
    {
        /// <summary>
        /// Gets the previous level of this upgrade type.
        /// </summary>
        IUpgradeTypeInternal PreviousLevel { get; }

        /// <summary>
        /// Gets the next level of this upgrade type.
        /// </summary>
        IUpgradeTypeInternal NextLevel { get; }

        /// <summary>
        /// Gets the effects of this upgrade type.
        /// </summary>
        IEnumerable<IUpgradeEffect> Effects { get; }
    }
}
