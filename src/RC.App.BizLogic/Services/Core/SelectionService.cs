using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.BusinessComponents;
using RC.Common;
using RC.Common.ComponentModel;

namespace RC.App.BizLogic.Services.Core
{
    /// <summary>
    /// The implementation of the selection service.
    /// </summary>
    [Component("RC.App.BizLogic.SelectionService")]
    class SelectionService : ISelectionService, IComponent
    {
        #region IComponent methods

        /// <see cref="IComponent.Start"/>
        public void Start()
        {
            this.scenarioManager = ComponentManager.GetInterface<IScenarioManagerBC>();
            this.selectionManager = ComponentManager.GetInterface<ISelectionManagerBC>();
            this.mapWindowBC = ComponentManager.GetInterface<IMapWindowBC>();
        }

        /// <see cref="IComponent.Stop"/>
        public void Stop()
        {
        }

        #endregion IComponent methods

        #region ISelectionService methods

        /// <see cref="ISelectionService.Select"/>
        public void Select(RCIntVector position)
        {
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }

            this.selectionManager.SelectEntity(this.mapWindowBC.AttachedWindow.WindowToMapCoords(position));
        }

        /// <see cref="ISelectionService.Select"/>
        public void Select(RCIntRectangle selectionBox)
        {
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }

            this.selectionManager.SelectEntities(this.mapWindowBC.AttachedWindow.WindowToMapRect(selectionBox));
        }

        /// <see cref="ISelectionService.Select"/>
        public void Select(int objectID)
        {
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }

            this.selectionManager.SelectEntity(objectID);
        }

        /// <see cref="ISelectionService.AddOrRemoveFromSelection"/>
        public void AddOrRemoveFromSelection(RCIntVector position)
        {
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }

            this.selectionManager.AddRemoveEntityToSelection(this.mapWindowBC.AttachedWindow.WindowToMapCoords(position));
        }

        /// <see cref="ISelectionService.AddToSelection"/>
        public void AddToSelection(RCIntRectangle selectionBox)
        {
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }

            this.selectionManager.AddEntitiesToSelection(this.mapWindowBC.AttachedWindow.WindowToMapRect(selectionBox));
        }

        /// <see cref="ISelectionService.RemoveFromSelection"/>
        public void RemoveFromSelection(int objectID)
        {
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }

            this.selectionManager.RemoveEntityFromSelection(objectID);
        }

        /// <see cref="ISelectionService.SelectType"/>
        public void SelectType(RCIntVector position)
        {
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }

            this.selectionManager.SelectEntitiesOfTheSameType(this.mapWindowBC.AttachedWindow.WindowToMapCoords(position),
                                                              this.mapWindowBC.AttachedWindow.WindowMapCoords);
        }

        /// <see cref="ISelectionService.SelectTypeFromCurrentSelection"/>
        public void SelectTypeFromCurrentSelection(int selectionOrdinal)
        {
            if (this.scenarioManager.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }

            int[] currentSelectionArray = this.selectionManager.CurrentSelection.ToArray();
            this.selectionManager.SelectEntitiesOfTheSameTypeFromCurrentSelection(currentSelectionArray[selectionOrdinal]);
        }

        #endregion ISelectionService methods

        /// <summary>
        /// Reference to the scenario manager business component.
        /// </summary>
        private IScenarioManagerBC scenarioManager;

        /// <summary>
        /// Reference to the selection manager business component.
        /// </summary>
        private ISelectionManagerBC selectionManager;

        /// <summary>
        /// Reference to the map window business component.
        /// </summary>
        private IMapWindowBC mapWindowBC;
    }
}
