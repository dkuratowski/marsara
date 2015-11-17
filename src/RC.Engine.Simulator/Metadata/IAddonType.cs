using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.Engine;

namespace RC.Engine.Simulator.Metadata
{
    /// <summary>
    /// Interface of the addon types defined in the metadata.
    /// </summary>
    public interface IAddonType : IScenarioElementType
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
        IUpgradeType GetUpgradeType(string upgradeTypeName);

        /// <summary>
        /// Gets the upgrade types of this addon type.
        /// </summary>
        IEnumerable<IUpgradeType> UpgradeTypes { get; }

        /// <summary>
        /// Checks whether the constraints of this addon type allows placing an addon of this type together with the given main building to its scenario
        /// at the given quadratic position and collects all the violating quadratic coordinates relative to the given position.
        /// </summary>
        /// <param name="mainBuilding">Reference to the main building of the addon to be checked.</param>
        /// <param name="position">The position to be checked.</param>
        /// <returns>
        /// The list of the quadratic coordinates (relative to the given position) violating the constraints of this addon type.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the type of the given building is not defined as the main building for this addon type.
        /// </exception>
        RCSet<RCIntVector> CheckPlacementConstraints(Building mainBuilding, RCIntVector position);
    }
}
