using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Implementation of the scenario simulator component.
    /// </summary>
    [Component("RC.Engine.Simulator.ScenarioSimulator")]
    class ScenarioSimulator : IScenarioSimulator
    {
        /// <summary>
        /// Constructs a ScenarioSimulator instance.
        /// </summary>
        public ScenarioSimulator()
        {
            this.map = null;
        }

        #region IScenarioSimulator methods

        /// <see cref="IScenarioSimulator.BeginScenario"/>
        public void BeginScenario(IMapAccess map)
        {
            if (map == null) { throw new ArgumentNullException("map"); }
            if (this.map != null) { throw new InvalidOperationException("Simulation of another scenario is currently running!"); }

            /// TODO: this is a dummy implementation!
            this.map = map;
        }

        /// <see cref="IScenarioSimulator.BeginScenario"/>
        public IMapAccess EndScenario()
        {
            if (this.map == null) { throw new InvalidOperationException("There is no scenario currently being simulated!"); }

            /// TODO: this is a dummy implementation!
            IMapAccess map = this.map;
            this.map = null;
            return map;
        }

        /// <see cref="IScenarioSimulator.BeginScenario"/>
        public IMapAccess Map
        {
            get
            {
                if (this.map == null) { throw new InvalidOperationException("There is no scenario currently being simulated!"); }

                /// TODO: this is a dummy implementation!
                return this.map;
            }
        }

        #endregion IScenarioSimulator methods

        /// <summary>
        /// Reference to the map of the scenario currently being simulated.
        /// </summary>
        private IMapAccess map;
    }
}
