using RC.Common.ComponentModel;
using RC.Engine.Simulator.Engine;

namespace RC.Engine.Simulator.ComponentInterfaces
{
    /// <summary>
    /// This interface is used by the plugins of the scenario loader to install themselves.
    /// </summary>
    [PluginInstallInterface]
    public interface IScenarioLoaderPluginInstall
    {
        /// <summary>
        /// Registers the given placement constraint for the given entity type.
        /// </summary>
        /// <param name="entityType">The name of the entity type.</param>
        /// <param name="constraint">The constraint to register.</param>
        void RegisterEntityConstraint(string entityType, EntityPlacementConstraint constraint);

        /// <summary>
        /// Registers the given placement suggestion provider for the given building type.
        /// </summary>
        /// <param name="buildingType">The name of the building type.</param>
        /// <param name="provider">The suggestion provider to register.</param>
        void RegisterPlacementSuggestionProvider(string buildingType, BuildingPlacementSuggestionProvider provider);
    }
}
