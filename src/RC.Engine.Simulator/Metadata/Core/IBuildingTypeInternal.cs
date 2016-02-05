using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Engine;

namespace RC.Engine.Simulator.Metadata.Core
{
    /// <summary>
    /// Internal interface of the building types defined in the metadata.
    /// </summary>
    interface IBuildingTypeInternal : IScenarioElementTypeInternal
    {
        /// <summary>
        /// Checks whether this building type has an addon type with the given name.
        /// </summary>
        /// <param name="addonTypeName">The name of the searched addon type.</param>
        /// <returns>True if this building type has an addon type with the given name, false otherwise.</returns>
        bool HasAddonType(string addonTypeName);

        /// <summary>
        /// Checks whether this building type has a unit type with the given name.
        /// </summary>
        /// <param name="unitTypeName">The name of the searched unit type.</param>
        /// <returns>True if this building type has a unit type with the given name, false otherwise.</returns>
        bool HasUnitType(string unitTypeName);

        /// <summary>
        /// Checks whether this building type has an upgrade type with the given name.
        /// </summary>
        /// <param name="upgradeTypeName">The name of the searched upgrade type.</param>
        /// <returns>True if this building type has an upgrade type with the given name, false otherwise.</returns>
        bool HasUpgradeType(string upgradeTypeName);

        /// <summary>
        /// Gets the addon type of this building type with the given name.
        /// </summary>
        /// <param name="addonTypeName">The name of the addon type.</param>
        /// <returns>The addon type with the given name.</returns>
        IAddonTypeInternal GetAddonType(string addonTypeName);

        /// <summary>
        /// Gets the unit type of this building type with the given name.
        /// </summary>
        /// <param name="unitTypeName">The name of the unit type.</param>
        /// <returns>The unit type with the given name.</returns>
        IUnitTypeInternal GetUnitType(string unitTypeName);

        /// <summary>
        /// Gets the upgrade type of this building type with the given name.
        /// </summary>
        /// <param name="upgradeTypeName">The name of the upgrade type.</param>
        /// <returns>The upgrade type with the given name.</returns>
        IUpgradeTypeInternal GetUpgradeType(string upgradeTypeName);

        /// <summary>
        /// Gets the addon types of this building type.
        /// </summary>
        IEnumerable<IAddonTypeInternal> AddonTypes { get; }

        /// <summary>
        /// Gets the unit types of this building type.
        /// </summary>
        IEnumerable<IUnitTypeInternal> UnitTypes { get; }

        /// <summary>
        /// Gets the upgrade types of this building type.
        /// </summary>
        IEnumerable<IUpgradeTypeInternal> UpgradeTypes { get; }

        /// <summary>
        /// Checks whether the constraints of this building type allows placing a building of this type together with an addon of the given addon type
        /// to the given scenario at the given quadratic position and collects all the violating quadratic coordinates
        /// relative to the given position.
        /// </summary>
        /// <param name="scenario">Reference to the given scenario.</param>
        /// <param name="position">The position to be checked.</param>
        /// <param name="addonType">The addon type to be checked.</param>
        /// <param name="entitiesToIgnore">
        /// The list of entities to be ignored during the check. All entities in this list shall belong to the given scenario.
        /// </param>
        /// <returns>
        /// The list of the quadratic coordinates (relative to the given position) violating the placement constraints of this building type.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If this building type is not defined as the main building for the given addon type.
        /// </exception>
        RCSet<RCIntVector> CheckPlacementConstraints(Scenario scenario, RCIntVector position, IAddonTypeInternal addonType, RCSet<Entity> entitiesToIgnore);

        /// <summary>
        /// Gets the placement suggestion boxes for this building type inside the given area on the map of the given scenario.
        /// </summary>
        /// <param name="scenario">The given scenario.</param>
        /// <param name="area">The area on the map of the given scenario in quadratic coordinates.</param>
        /// <returns>
        /// A list that contains pairs of an RCIntRectangle and an RCIntVector. Each of these pair gives informations about
        /// a suggestion box to the caller. The RCIntRectangle component represents the area whose visibility needs to be
        /// checked by the caller. If that area is visible then the RCIntVector component contains the coordinates of the
        /// top-left corner of the suggestion box relative to the RCIntRectangle component.
        /// </returns>
        RCSet<Tuple<RCIntRectangle, RCIntVector>> GetPlacementSuggestions(Scenario scenario, RCIntRectangle area);
        
        /// <summary>
        /// Gets the quadratic position of the given addon type relative to the top-left quadratic tile of this building type.
        /// </summary>
        /// <param name="map">The map that is used for the calculations.</param>
        /// <param name="addonType">The addon type whose relative quadratic position to retrieve.</param>
        /// <returns>The quadratic position of the given addon type relative to the top-left quadratic tile of this building type.</returns>
        /// <exception cref="ArgumentException">
        /// If this building type is not defined as the main building for the given addon type.
        /// </exception>
        RCIntVector GetRelativeAddonPosition(IMapAccess map, IAddonTypeInternal addonType);
    }
}
