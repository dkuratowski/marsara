using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Maps.PublicInterfaces;
using RC.Common;
using RC.App.BizLogic.BusinessComponents.Core;
using RC.App.BizLogic.BusinessComponents;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.Scenarios;

namespace RC.App.BizLogic.Views.Core
{
    /// <summary>
    /// Base class of views on game maps.
    /// </summary>
    abstract class MapViewBase : IMapView
    {
        /// <summary>
        /// Constructs a MapViewBase instance.
        /// </summary>
        public MapViewBase()
        {
            this.scenarioManager = ComponentManager.GetInterface<IScenarioManagerBC>();
        }

        #region IMapView methods

        /// <see cref="IMapView.MapSize"/>
        public RCIntVector MapSize { get { return this.Map.CellSize * CoordTransformationHelper.PIXEL_PER_NAVCELL_VECT; } }

        #endregion IMapView methods

        /// <summary>
        /// Gets the map of the active scenario.
        /// </summary>
        protected IMapAccess Map { get { return this.scenarioManager.ActiveScenario.Map; } }

        /// <summary>
        /// Gets the active scenario.
        /// </summary>
        protected Scenario Scenario { get { return this.scenarioManager.ActiveScenario; } }

        /// <summary>
        /// Reference to the scenario manager business component.
        /// </summary>
        private IScenarioManagerBC scenarioManager;
    }
}
