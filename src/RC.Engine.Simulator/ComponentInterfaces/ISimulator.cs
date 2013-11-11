using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.ComponentModel;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.ComponentInterfaces
{
    /// <summary>
    /// Component interface for simulating RC-Scenarios (for example multiplayer games). Note that only one scenario can be simulated
    /// at a time.
    /// </summary>
    [ComponentInterface]
    public interface ISimulator
    {
        /// <summary>
        /// Begins a new scenario on the given map.
        /// </summary>
        /// <param name="map">The map of the scenario.</param>
        /// <exception cref="InvalidOperationException">If another scenario is currently being simulated.</exception>
        void BeginScenario(IMapAccess map);

        /// <summary>
        /// Stops the simulation of the scenario currently being simulated.
        /// </summary>
        /// <returns>The map of the scenario.</returns>
        /// <exception cref="InvalidOperationException">If there is no scenario currently being simulated.</exception>
        IMapAccess EndScenario();

        /// <summary>
        /// Simulates the next frame of the scenario.
        /// </summary>
        void SimulateNextFrame();

        /// <summary>
        /// Gets the map of the currently simulated scenario.
        /// </summary>
        /// <exception cref="InvalidOperationException">If there is no scenario currently being simulated.</exception>
        IMapAccess Map { get; }

        /// <summary>
        /// Gets the map content manager that contains the game objects of the scenario currently being simulated.
        /// </summary>
        IMapContentManager<IGameObject> GameObjects { get; }
    }
}
