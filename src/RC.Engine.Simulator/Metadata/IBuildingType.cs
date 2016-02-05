using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.Metadata.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Metadata
{
    /// <summary>
    /// Interface of the building types defined in the metadata.
    /// This interface is defined as a class because we want to overload the equality/inequality operators.
    /// </summary>
    public class IBuildingType : IScenarioElementType
    {
        #region Interface methods

        /// <summary>
        /// Checks whether this building type has an addon type with the given name.
        /// </summary>
        /// <param name="addonTypeName">The name of the searched addon type.</param>
        /// <returns>True if this building type has an addon type with the given name, false otherwise.</returns>
        public bool HasAddonType(string addonTypeName)
        {
            return this.implementation.HasAddonType(addonTypeName);
        }

        /// <summary>
        /// Checks whether this building type has a unit type with the given name.
        /// </summary>
        /// <param name="unitTypeName">The name of the searched unit type.</param>
        /// <returns>True if this building type has a unit type with the given name, false otherwise.</returns>
        public bool HasUnitType(string unitTypeName)
        {
            return this.implementation.HasUnitType(unitTypeName);
        }


        /// <summary>
        /// Checks whether this building type has an upgrade type with the given name.
        /// </summary>
        /// <param name="upgradeTypeName">The name of the searched upgrade type.</param>
        /// <returns>True if this building type has an upgrade type with the given name, false otherwise.</returns>
        public bool HasUpgradeType(string upgradeTypeName)
        {
            return this.implementation.HasUpgradeType(upgradeTypeName);
        }


        /// <summary>
        /// Gets the addon type of this building type with the given name.
        /// </summary>
        /// <param name="addonTypeName">The name of the addon type.</param>
        /// <returns>The addon type with the given name.</returns>
        public IAddonType GetAddonType(string addonTypeName)
        {
            return new IAddonType(this.implementation.GetAddonType(addonTypeName));
        }

        /// <summary>
        /// Gets the unit type of this building type with the given name.
        /// </summary>
        /// <param name="unitTypeName">The name of the unit type.</param>
        /// <returns>The unit type with the given name.</returns>
        public IUnitType GetUnitType(string unitTypeName)
        {
            return new IUnitType(this.implementation.GetUnitType(unitTypeName));
        }

        /// <summary>
        /// Gets the upgrade type of this building type with the given name.
        /// </summary>
        /// <param name="upgradeTypeName">The name of the upgrade type.</param>
        /// <returns>The upgrade type with the given name.</returns>
        public IUpgradeType GetUpgradeType(string upgradeTypeName)
        {
            return new IUpgradeType(this.implementation.GetUpgradeType(upgradeTypeName));
        }

        /// <summary>
        /// Gets the addon types of this building type.
        /// </summary>
        public IEnumerable<IAddonType> AddonTypes
        {
            get
            {
                List<IAddonType> retList = new List<IAddonType>();
                foreach (IAddonTypeInternal addonType in this.implementation.AddonTypes) { retList.Add(new IAddonType(addonType)); }
                return retList;
            }
        }

        /// <summary>
        /// Gets the unit types of this building type.
        /// </summary>
        public IEnumerable<IUnitType> UnitTypes
        {
            get
            {
                List<IUnitType> retList = new List<IUnitType>();
                foreach (IUnitTypeInternal unitType in this.implementation.UnitTypes) { retList.Add(new IUnitType(unitType)); }
                return retList;
            }
        }

        /// <summary>
        /// Gets the upgrade types of this building type.
        /// </summary>
        public IEnumerable<IUpgradeType> UpgradeTypes
        {
            get
            {
                List<IUpgradeType> retList = new List<IUpgradeType>();
                foreach (IUpgradeTypeInternal upgradeType in this.implementation.UpgradeTypes) { retList.Add(new IUpgradeType(upgradeType)); }
                return retList;
            }
        }

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
        public RCSet<RCIntVector> CheckPlacementConstraints(Scenario scenario, RCIntVector position, IAddonType addonType, RCSet<Entity> entitiesToIgnore)
        {
            return this.implementation.CheckPlacementConstraints(scenario, position, addonType.AddonTypeImpl, entitiesToIgnore);
        }

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
        public RCSet<Tuple<RCIntRectangle, RCIntVector>> GetPlacementSuggestions(Scenario scenario, RCIntRectangle area)
        {
            return this.implementation.GetPlacementSuggestions(scenario, area);
        }

        /// <summary>
        /// Gets the quadratic position of the given addon type relative to the top-left quadratic tile of this building type.
        /// </summary>
        /// <param name="map">The map that is used for the calculations.</param>
        /// <param name="addonType">The addon type whose relative quadratic position to retrieve.</param>
        /// <returns>The quadratic position of the given addon type relative to the top-left quadratic tile of this building type.</returns>
        /// <exception cref="ArgumentException">
        /// If this building type is not defined as the main building for the given addon type.
        /// </exception>
        public RCIntVector GetRelativeAddonPosition(IMapAccess map, IAddonType addonType)
        {
            return this.implementation.GetRelativeAddonPosition(map, addonType.AddonTypeImpl);
        }

        #endregion Interface methods

        /// <summary>
        /// Constructs an instance of this interface with the given implementation.
        /// </summary>
        /// <param name="implementation">The implementation of this interface.</param>
        internal IBuildingType(IBuildingTypeInternal implementation) : base(implementation)
        {
            if (implementation == null) { throw new ArgumentNullException("implementation"); }
            this.implementation = implementation;
        }

        /// <summary>
        /// Gets the implementation of this interface.
        /// </summary>
        internal IBuildingTypeInternal BuildingTypeImpl { get { return this.implementation; } }

        /// <summary>
        /// Reference to the implementation of this interface.
        /// </summary>
        private IBuildingTypeInternal implementation;
    }
}
