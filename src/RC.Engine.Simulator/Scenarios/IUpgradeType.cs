using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Scenarios
{
    /// <summary>
    /// Interface of the upgrade types defined in the metadata.
    /// </summary>
    public interface IUpgradeType : IScenarioElementType
    {
        /// <summary>
        /// Gets the previous level of this upgrade type.
        /// </summary>
        IUpgradeType PreviousLevel { get; }

        /// <summary>
        /// Gets the next level of this upgrade type.
        /// </summary>
        IUpgradeType NextLevel { get; }
    }
}
