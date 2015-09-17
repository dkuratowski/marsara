using System;
using RC.Engine.Maps.PublicInterfaces;
using RC.App.BizLogic.BusinessComponents;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.Engine;

namespace RC.App.BizLogic.Views.Core
{
    /// <summary>
    /// Base class of views on game maps.
    /// </summary>
    abstract class MapViewBase
    {
        /// <summary>
        /// Constructs a MapViewBase instance.
        /// </summary>
        public MapViewBase()
        {
            this.scenarioManager = ComponentManager.GetInterface<IScenarioManagerBC>();
            this.mapWindowBC = ComponentManager.GetInterface<IMapWindowBC>();
        }

        /// <summary>
        /// Gets the map of the active scenario.
        /// </summary>
        protected IMapAccess Map { get { return this.scenarioManager.ActiveScenario.Map; } }

        /// <summary>
        /// Gets the active scenario.
        /// </summary>
        protected Scenario Scenario { get { return this.scenarioManager.ActiveScenario; } }

        /// <summary>
        /// Gets the map window business component.
        /// </summary>
        protected IMapWindowBC MapWindowBC { get { return this.mapWindowBC; } }

        /// <summary>
        /// Gets the entity from the active scenario with the given ID.
        /// </summary>
        /// <param name="entityID">The ID of entity to get.</param>
        /// <returns>The entity from the active scenario with the given ID.</returns>
        /// <exception cref="InvalidOperationException">If there is no entity with the given ID in the active scenario.</exception>
        protected Entity GetEntity(int entityID)
        {
            Entity entity = this.Scenario.GetElementOnMap<Entity>(entityID);
            if (entity == null) { throw new InvalidOperationException(String.Format("Entity with ID '{0}' doesn't exist!", entityID)); }

            return entity;
        }

        /// <summary>
        /// Reference to the scenario manager business component.
        /// </summary>
        private readonly IScenarioManagerBC scenarioManager;

        /// <summary>
        /// Reference to the map window business component.
        /// </summary>
        private readonly IMapWindowBC mapWindowBC;
    }
}
