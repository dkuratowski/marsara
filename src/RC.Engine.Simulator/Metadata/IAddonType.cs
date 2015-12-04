using RC.Common;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.Metadata.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Metadata
{
    /// <summary>
    /// Interface of the addon types defined in the metadata.
    /// This interface is defined as a class because we want to overload the equality/inequality operators.
    /// </summary>
    public class IAddonType : IScenarioElementType
    {
        #region Interface methods

        /// <summary>
        /// Checks whether this addon type has an upgrade type with the given name.
        /// </summary>
        /// <param name="upgradeTypeName">The name of the searched upgrade type.</param>
        /// <returns>True if this addon type has an upgrade type with the given name, false otherwise.</returns>
        public bool HasUpgradeType(string upgradeTypeName)
        {
            return this.implementation.HasUpgradeType(upgradeTypeName);
        }

        /// <summary>
        /// Gets the upgrade type of this addon type with the given name.
        /// </summary>
        /// <param name="upgradeTypeName">The name of the upgrade type.</param>
        /// <returns>The upgrade type with the given name.</returns>
        public IUpgradeType GetUpgradeType(string upgradeTypeName)
        {
            return new IUpgradeType(this.implementation.GetUpgradeType(upgradeTypeName));
        }

        /// <summary>
        /// Gets the upgrade types of this addon type.
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
        public RCSet<RCIntVector> CheckPlacementConstraints(Building mainBuilding, RCIntVector position)
        {
            return this.implementation.CheckPlacementConstraints(mainBuilding, position);
        }

        #endregion Interface methods
        
        /// <summary>
        /// Constructs an instance of this interface with the given implementation.
        /// </summary>
        /// <param name="implementation">The implementation of this interface.</param>
        internal IAddonType(IAddonTypeInternal implementation) : base(implementation)
        {
            if (implementation == null) { throw new ArgumentNullException("implementation"); }
            this.implementation = implementation;
        }

        /// <summary>
        /// Gets the implementation of this interface.
        /// </summary>
        internal IAddonTypeInternal AddonTypeImpl { get { return this.implementation; } }

        /// <summary>
        /// Reference to the implementation of this interface.
        /// </summary>
        private IAddonTypeInternal implementation;
    }
}
