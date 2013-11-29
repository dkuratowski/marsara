using RC.Engine.Maps.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Scenarios
{
    /// <summary>
    /// Represents an RC scenario.
    /// </summary>
    class Scenario : IScenario
    {
        /// <summary>
        /// Constructs a Scenario instance.
        /// </summary>
        /// <param name="map">The map of the scenario.</param>
        public Scenario(IMapAccess map)
        {
            if (map == null) { throw new ArgumentNullException("map"); }
            this.map = map;
        }

        #region IScenario members

        /// <see cref="IScenario.Map"/>
        public IMapAccess Map { get { return this.map; } }

        #endregion IScenario members

        /// <summary>
        /// Reference to the map of the scenario.
        /// </summary>
        private IMapAccess map;
    }
}
