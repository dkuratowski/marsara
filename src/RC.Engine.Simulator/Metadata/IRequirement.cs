using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Metadata
{
    /// <summary>
    /// Represents a requirement for creating a building/unit/addon/upgrade type.
    /// </summary>
    public interface IRequirement
    {
        /// <summary>
        /// Gets the required building type defined by this requirement.
        /// </summary>
        IBuildingType RequiredBuildingType { get; }

        /// <summary>
        /// Gets the required addon type defined by this requirement.
        /// </summary>
        IAddonType RequiredAddonType { get; }
    }
}
