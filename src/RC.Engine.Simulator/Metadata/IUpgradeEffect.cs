using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Metadata
{
    /// <summary>
    /// The interface of upgrade effects.
    /// </summary>
    public interface IUpgradeEffect
    {
        /// <summary>
        /// Performs this effect on the given metadata upgrade interface.
        /// </summary>
        /// <param name="metadataUpgrade">The metadata upgrade interface on which to perform this effect.</param>
        void Perform(IScenarioMetadataUpgrade metadataUpgrade);
    }
}
