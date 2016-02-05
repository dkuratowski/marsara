using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.Engine;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Simulator.Metadata.Core
{
    /// <summary>
    /// Internal interface of the addon types defined in the metadata.
    /// </summary>
    interface IAddonTypeInternal : IScenarioElementTypeInternal
    {
        /// <summary>
        /// Checks whether this addon type has an upgrade type with the given name.
        /// </summary>
        /// <param name="upgradeTypeName">The name of the searched upgrade type.</param>
        /// <returns>True if this addon type has an upgrade type with the given name, false otherwise.</returns>
        bool HasUpgradeType(string upgradeTypeName);

        /// <summary>
        /// Gets the upgrade type of this addon type with the given name.
        /// </summary>
        /// <param name="upgradeTypeName">The name of the upgrade type.</param>
        /// <returns>The upgrade type with the given name.</returns>
        IUpgradeTypeInternal GetUpgradeType(string upgradeTypeName);

        /// <summary>
        /// Gets the upgrade types of this addon type.
        /// </summary>
        IEnumerable<IUpgradeTypeInternal> UpgradeTypes { get; }
    }
}
