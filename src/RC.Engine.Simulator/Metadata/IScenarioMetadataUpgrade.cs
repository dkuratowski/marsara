using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Metadata
{
    /// <summary>
    /// Interface for upgrading the metadata informations for RC scenarios.
    /// </summary>
    public interface IScenarioMetadataUpgrade
    {
        /// <summary>
        /// Gets the upgrade interface of the scenario element type with the given name.
        /// </summary>
        /// <param name="typeName">The name of the element type.</param>
        /// <returns>The upgrade interface of the scenario element type with the given name.</returns>
        IScenarioElementTypeUpgrade GetElementTypeUpgrade(string typeName);

        /// <summary>
        /// Attaches an underlying metadata to be upgraded.
        /// </summary>
        /// <param name="metadata">The metadata to be upgraded.</param>
        void AttachMetadata(IScenarioMetadata metadata);
    }
}
