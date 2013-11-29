using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Scenarios
{
    /// <summary>
    /// Interface of the unit types defined in the metadata.
    /// </summary>
    public interface IUnitType : IScenarioElementType
    {
        /// <summary>
        /// Gets the addon type that is necessary to be attached to the building that creates
        /// this type of units.
        /// </summary>
        IAddonType NecessaryAddon { get; }
    }
}
