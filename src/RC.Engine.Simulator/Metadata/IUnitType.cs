using RC.Engine.Simulator.Metadata.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Metadata
{
    /// <summary>
    /// Interface of the unit types defined in the metadata.
    /// This interface is defined as a class because we want to overload the equality/inequality operators.
    /// </summary>
    public class IUnitType : IScenarioElementType
    {
        #region Interface methods

        /// <summary>
        /// Gets the addon type that is necessary to be attached to the building that creates
        /// this type of units.
        /// </summary>
        public IAddonType NecessaryAddon { get { return this.implementation.NecessaryAddon != null ? new IAddonType(this.implementation.NecessaryAddon) : null; } }

        #endregion Interface methods

        /// <summary>
        /// Constructs an instance of this interface with the given implementation.
        /// </summary>
        /// <param name="implementation">The implementation of this interface.</param>
        internal IUnitType(IUnitTypeInternal implementation) : base(implementation)
        {
            if (implementation == null) { throw new ArgumentNullException("implementation"); }
            this.implementation = implementation;
        }

        /// <summary>
        /// Gets the implementation of this interface.
        /// </summary>
        internal IUnitTypeInternal UnitTypeImpl { get { return this.implementation; } }

        /// <summary>
        /// Reference to the implementation of this interface.
        /// </summary>
        private IUnitTypeInternal implementation;
    }
}
