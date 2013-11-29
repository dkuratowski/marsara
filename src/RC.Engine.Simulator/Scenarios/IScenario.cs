using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Simulator.Scenarios
{
    /// <summary>
    /// The interface of a scenario.
    /// </summary>
    public interface IScenario
    {
        /// <summary>
        /// Gets the map of the scenario.
        /// </summary>
        IMapAccess Map { get; }
    }
}
