using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Metadata.Core
{
    /// <summary>
    /// Internal interface of the unit types defined in the metadata.
    /// </summary>
    interface IUnitTypeInternal : IScenarioElementTypeInternal
    {
        /// <summary>
        /// Gets the addon type that is necessary to be attached to the building that creates
        /// this type of units.
        /// </summary>
        IAddonTypeInternal NecessaryAddon { get; }
    }
}
