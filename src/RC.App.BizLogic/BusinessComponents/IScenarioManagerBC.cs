using System;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.Metadata;

namespace RC.App.BizLogic.BusinessComponents
{
    /// <summary>
    /// The interface of the business component that is responsible for creating, opening, saving or closing game scenarios.
    /// The scenario that has been created or opened by this BC is called the active scenario. The active scenario can be
    /// accessed by any other BCs, services or views, or can be saved into a file using this BC. Only 1 active scenario can
    /// be opened at the same time.
    /// </summary>
    [ComponentInterface]
    interface IScenarioManagerBC
    {
        /// <summary>
        /// Creates a new game scenario.
        /// </summary>
        /// <param name="mapName">The name of the map of the new scenario.</param>
        /// <param name="tilesetName">The name of the tileset that the map of the new scenario is based on.</param>
        /// <param name="defaultTerrain">The default terrain of the map of the new scenario.</param>
        /// <param name="mapSize">
        /// The size of the map of the new scenario in quadratic tiles. The first coordinate of the vector is the width,
        /// the second coordinate of the vector is the height of the map. The constraints are the followings:
        /// the width of the map must be a multiple of 4, the height of the map must be a multiple of 2.
        /// </param>
        void NewScenario(string mapName, string tilesetName, string defaultTerrain, RCIntVector mapSize);

        /// <summary>
        /// Opens a scenario from the given file.
        /// </summary>
        /// <param name="filename">The name of the file to load from.</param>
        void OpenScenario(string filename);

        /// <summary>
        /// Saves the currently active scenario to the given file.
        /// </summary>
        /// <param name="filename">The name of the file to save.</param>
        void SaveScenario(string filename);

        /// <summary>
        /// Closes the currently active scenario. If there is no active scenario, then this function has no effect.
        /// </summary>
        void CloseScenario();

        /// <summary>
        /// Gets a reference to the currently active scenario.
        /// </summary>
        Scenario ActiveScenario { get; }

        /// <summary>
        /// Gets a reference to the simulation metadata.
        /// </summary>
        IScenarioMetadata Metadata { get; }

        /// <summary>
        /// This event is raised when the active scenario has been changed.
        /// </summary>
        event Action<Scenario> ActiveScenarioChanged;
    }
}
