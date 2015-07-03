using RC.Common.ComponentModel;
using RC.Engine.Simulator.Engine;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// The abstract base class of all business components that depend on the currently active scenario.
    /// </summary>
    abstract class ScenarioDependentComponent : IComponent
    {
        #region IComponent members

        /// <see cref="IComponent.Start"/>
        public void Start()
        {
            this.scenarioManagerBC = ComponentManager.GetInterface<IScenarioManagerBC>();
            this.scenarioManagerBC.ActiveScenarioChanged += this.OnActiveScenarioChanged;
            this.StartImpl();
        }

        /// <see cref="IComponent.Stop"/>
        public void Stop()
        {
            this.StopImpl();
            this.scenarioManagerBC = null;
        }
        
        #endregion IComponent members

        #region Protected members

        /// <summary>
        /// Gets the currently active scenario or null if there is no active scenario.
        /// </summary>
        protected Scenario ActiveScenario { get { return this.scenarioManagerBC.ActiveScenario; } }

        #endregion Protected members

        #region Overridables

        /// <summary>
        /// Internal implementation of the component start procedure that can be overriden in the derived classes.
        /// </summary>
        protected virtual void StartImpl() { }

        /// <summary>
        /// Internal implementation of the component stop procedure that can be overriden in the derived classes.
        /// </summary>
        protected virtual void StopImpl() { }

        /// <summary>
        /// This method is called when the active scenario has been changed.
        /// </summary>
        /// <param name="activeScenario">The active scenario or null if there is no active scenario.</param>
        protected abstract void OnActiveScenarioChanged(Scenario activeScenario);

        #endregion Overridables

        /// <summary>
        /// Reference to the scenario manager business component.
        /// </summary>
        private IScenarioManagerBC scenarioManagerBC;
    }
}
