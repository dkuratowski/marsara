using RC.Common;
using RC.Engine.Simulator.Scenarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.Views;
using RC.Common.ComponentModel;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// Implementation of the selection manager business component.
    /// </summary>
    [Component("RC.App.BizLogic.SelectionManagerBC")]
    class SelectionManagerBC : ScenarioDependentComponent, ISelectionManagerBC
    {
        /// <summary>
        /// Constructs a SelectionManagerBC instance.
        /// </summary>
        public SelectionManagerBC()
        {
            this.localPlayer = PlayerEnum.Neutral;
        }

        #region Overrides from ScenarioDependentComponent

        /// <see cref="ScenarioDependentComponent.StartImpl"/>
        protected override void StartImpl()
        {
            this.fogOfWarBC = ComponentManager.GetInterface<IFogOfWarBC>();
        }

        /// <see cref="ScenarioDependentComponent.OnActiveScenarioChanged"/>
        protected override void OnActiveScenarioChanged(Scenario activeScenario)
        {
            this.localPlayer = PlayerEnum.Neutral;
        }

        #endregion Overrides from ScenarioDependentComponent

        #region ISelectionManagerBC methods

        /// <see cref="ISelectionManagerBC.Reset"/>
        public void Reset(PlayerEnum localPlayer)
        {
            if (this.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (localPlayer == PlayerEnum.Neutral) { throw new ArgumentException("Local player cannot be PlayerEnum.Neutral!", "localPlayer"); }

            this.localPlayer = localPlayer;
            this.currentSelection = new HashSet<int>();
            this.savedSelections = new HashSet<int>[CAPACITY];
            for (int i = 0; i < CAPACITY; i++)
            {
                this.savedSelections[i] = new HashSet<int>();
            }
        }

        /// <see cref="ISelectionManagerBC.GetEntity"/>
        public int GetEntity(RCNumVector position)
        {
            if (this.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (this.localPlayer == PlayerEnum.Neutral) { throw new InvalidOperationException("Selection manager not initialized!"); }

            this.Update();

            Entity entityAtPos = this.ActiveScenario.GetEntitiesOnMap<Entity>(position).FirstOrDefault();
            if (entityAtPos == null) { return -1; }
            return entityAtPos.ID.Read();
        }

        /// <see cref="ISelectionManagerBC.SelectEntities"/>
        public void SelectEntities(RCNumRectangle selectionBox)
        {
            if (this.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (this.localPlayer == PlayerEnum.Neutral) { throw new InvalidOperationException("Selection manager not initialized!"); }

            this.Update();

            bool ownerUnitFound = false;
            Entity ownerBuilding = null;
            Entity ownerAddon = null;
            Entity otherPlayerUnit = null;
            Entity otherPlayerBuilding = null;
            Entity otherPlayerAddon = null;
            Entity other = null;
            foreach (Entity entity in this.ActiveScenario.GetEntitiesOnMap<Entity>(selectionBox))
            {
                if (entity is Unit)
                {
                    /// If the entity is a unit then check its owner.
                    if (entity.Owner.PlayerIndex == (int)this.localPlayer)
                    {
                        /// If owned by the owner of this selector then add it to the current selection.
                        if (!ownerUnitFound)
                        {
                            this.currentSelection.Clear();
                            ownerUnitFound = true;
                        }
                        this.currentSelection.Add(entity.ID.Read());
                    }
                    else if (otherPlayerUnit == null && !ownerUnitFound)
                    {
                        /// If owned by another player then save its reference.
                        otherPlayerUnit = entity;
                    }
                }
                else if (entity is Building && !ownerUnitFound)
                {
                    /// If the entity is a building then check its owner.
                    if (entity.Owner.PlayerIndex == (int)this.localPlayer)
                    {
                        if (ownerBuilding == null) { ownerBuilding = entity; }
                    }
                    else if (otherPlayerBuilding == null)
                    {
                        otherPlayerBuilding = entity;
                    }
                }
                else if (entity is Addon && !ownerUnitFound)
                {
                    /// If the entity is an addon then check its owner.
                    if (entity.Owner.PlayerIndex == (int)this.localPlayer)
                    {
                        if (ownerAddon == null) { ownerAddon = entity; }
                    }
                    else if (otherPlayerAddon == null)
                    {
                        otherPlayerAddon = entity;
                    }
                }
                else if (other == null)
                {
                    other = entity;
                }
            }

            if (ownerUnitFound) { return; }

            this.currentSelection.Clear();
            if (ownerBuilding != null) { this.currentSelection.Add(ownerBuilding.ID.Read()); return; }
            if (ownerAddon != null) { this.currentSelection.Add(ownerAddon.ID.Read()); return; }
            if (otherPlayerUnit != null) { this.currentSelection.Add(otherPlayerUnit.ID.Read()); return; }
            if (otherPlayerBuilding != null) { this.currentSelection.Add(otherPlayerBuilding.ID.Read()); return; }
            if (otherPlayerAddon != null) { this.currentSelection.Add(otherPlayerAddon.ID.Read()); return; }
            if (other != null) { this.currentSelection.Add(other.ID.Read()); return; }
        }

        /// <see cref="ISelectionManagerBC.SelectEntity"/>
        public void SelectEntity(RCNumVector position)
        {
            if (this.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (this.localPlayer == PlayerEnum.Neutral) { throw new InvalidOperationException("Selection manager not initialized!"); }

            this.Update();

            Entity entityAtPos = this.ActiveScenario.GetEntitiesOnMap<Entity>(position).FirstOrDefault();
            if (entityAtPos == null) { return; }
            this.currentSelection.Clear();
            this.currentSelection.Add(entityAtPos.ID.Read());
        }

        /// <see cref="ISelectionManagerBC.AddRemoveEntityToSelection"/>
        public void AddRemoveEntityToSelection(RCNumVector position)
        {
            if (this.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (this.localPlayer == PlayerEnum.Neutral) { throw new InvalidOperationException("Selection manager not initialized!"); }

            this.Update();
            throw new NotImplementedException();
        }

        /// <see cref="ISelectionManagerBC.AddEntitiesToSelection"/>
        public void AddEntitiesToSelection(RCNumRectangle selectionBox)
        {
            if (this.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (this.localPlayer == PlayerEnum.Neutral) { throw new InvalidOperationException("Selection manager not initialized!"); }

            this.Update();
            throw new NotImplementedException();
        }

        /// <see cref="ISelectionManagerBC.LoadSelection"/>
        public void LoadSelection(int index)
        {
            if (this.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (this.localPlayer == PlayerEnum.Neutral) { throw new InvalidOperationException("Selection manager not initialized!"); }

            this.Update();
            throw new NotImplementedException();
        }

        /// <see cref="ISelectionManagerBC.SaveCurrentSelection"/>
        public void SaveCurrentSelection(int index)
        {
            if (this.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (this.localPlayer == PlayerEnum.Neutral) { throw new InvalidOperationException("Selection manager not initialized!"); }

            this.Update();
            throw new NotImplementedException();
        }

        /// <see cref="ISelectionManagerBC.CurrentSelection"/>
        public HashSet<int> CurrentSelection
        {
            get
            {
                if (this.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
                if (this.localPlayer == PlayerEnum.Neutral) { throw new InvalidOperationException("Selection manager not initialized!"); }

                this.Update();
                return new HashSet<int>(this.currentSelection);
            }
        }

        /// <see cref="ISelectionManagerBC.LocalPlayer"/>
        public PlayerEnum LocalPlayer
        {
            get
            {
                if (this.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
                return this.localPlayer;
            }
        }

        #endregion ISelectionManagerBC methods

        /// <summary>
        /// Updates the current and the saved selections.
        /// </summary>
        private void Update()
        {
            /// Collect the IDs to be removed.
            HashSet<int> idsToRemove = new HashSet<int>();
            foreach (int id in this.currentSelection)
            {
                if (this.ActiveScenario.GetEntity<Entity>(id) == null) { idsToRemove.Add(id); }
            }
            foreach (HashSet<int> savedSelection in this.savedSelections)
            {
                foreach (int id in savedSelection)
                {
                    if (this.ActiveScenario.GetEntity<Entity>(id) == null) { idsToRemove.Add(id); }
                }
            }

            /// Remove the collected IDs.
            foreach (int idToRemove in idsToRemove)
            {
                this.currentSelection.Remove(idToRemove);
                foreach (HashSet<int> savedSelection in this.savedSelections)
                {
                    savedSelection.Remove(idToRemove);
                }
            }
        }

        /// <summary>
        /// Reference to the Fog Of War business component.
        /// </summary>
        private IFogOfWarBC fogOfWarBC;

        /// <summary>
        /// The index of the player that owns this EntitySelector.
        /// </summary>
        private PlayerEnum localPlayer;

        /// <summary>
        /// The current selection that contains the IDs of the selected entities.
        /// </summary>
        private HashSet<int> currentSelection;

        /// <summary>
        /// List of the saved selections mapped by their indices.
        /// </summary>
        private HashSet<int>[] savedSelections;

        /// <summary>
        /// The maximum number of selections that can be saved.
        /// </summary>
        private const int CAPACITY = 10;
    }
}
