using RC.Engine.Simulator.Metadata.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Metadata
{
    /// <summary>
    /// Interface of the upgrade types defined in the metadata.
    /// This interface is defined as a class because we want to overload the equality/inequality operators.
    /// </summary>
    public class IUpgradeType : IScenarioElementType
    {
        #region Interface methods

        /// <summary>
        /// Gets the previous level of this upgrade type.
        /// </summary>
        IUpgradeType PreviousLevel { get { return this.implementation.PreviousLevel != null ? new IUpgradeType(this.implementation.PreviousLevel) : null; } }

        /// <summary>
        /// Gets the next level of this upgrade type.
        /// </summary>
        IUpgradeType NextLevel { get { return this.implementation.NextLevel != null ? new IUpgradeType(this.implementation.NextLevel) : null; } }

        #endregion Interface methods

        /// <summary>
        /// Constructs an instance of this interface with the given implementation.
        /// </summary>
        /// <param name="implementation">The implementation of this interface.</param>
        internal IUpgradeType(IUpgradeTypeInternal implementation) : base(implementation)
        {
            if (implementation == null) { throw new ArgumentNullException("implementation"); }
            this.implementation = implementation;
        }

        /// <summary>
        /// Gets the implementation of this interface.
        /// </summary>
        internal IUpgradeTypeInternal UpgradeTypeImpl { get { return this.implementation; } }

        /// <summary>
        /// Reference to the implementation of this interface.
        /// </summary>
        private IUpgradeTypeInternal implementation;
    }
}
