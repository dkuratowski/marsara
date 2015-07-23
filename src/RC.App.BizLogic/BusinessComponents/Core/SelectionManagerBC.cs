using System.Linq;
using RC.Common;
using System;
using System.Collections.Generic;
using RC.App.BizLogic.Views;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.Engine;

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
            this.currentSelection = new RCSet<int>();
            this.savedSelections = new RCSet<int>[CAPACITY];
            for (int i = 0; i < CAPACITY; i++)
            {
                this.savedSelections[i] = new RCSet<int>();
            }
        }

        /// <see cref="ISelectionManagerBC.GetEntity"/>
        public int GetEntity(RCNumVector position)
        {
            if (this.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (this.localPlayer == PlayerEnum.Neutral) { throw new InvalidOperationException("Selection manager not initialized!"); }

            this.Update();

            Entity entityAtPos = this.ActiveScenario.GetElementsOnMap<Entity>(position).FirstOrDefault();
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
            foreach (Entity entity in this.ActiveScenario.GetElementsOnMap<Entity>(selectionBox))
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

            Entity entityAtPos = this.ActiveScenario.GetElementsOnMap<Entity>(position).FirstOrDefault();
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
        public RCSet<int> CurrentSelection
        {
            get
            {
                if (this.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
                if (this.localPlayer == PlayerEnum.Neutral) { throw new InvalidOperationException("Selection manager not initialized!"); }

                this.Update();
                return new RCSet<int>(this.currentSelection);
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
            RCSet<int> idsToRemove = new RCSet<int>();
            foreach (int id in this.currentSelection)
            {
                Entity entity = this.ActiveScenario.GetElement<Entity>(id);
                if (entity == null) { idsToRemove.Add(id); }
            }
            foreach (RCSet<int> savedSelection in this.savedSelections)
            {
                foreach (int id in savedSelection)
                {
                    Entity entity = this.ActiveScenario.GetElement<Entity>(id);
                    if (entity == null) { idsToRemove.Add(id); }
                }
            }

            /// Remove the collected IDs.
            foreach (int idToRemove in idsToRemove)
            {
                this.currentSelection.Remove(idToRemove);
                foreach (RCSet<int> savedSelection in this.savedSelections)
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
        private RCSet<int> currentSelection;

        /// <summary>
        /// List of the saved selections mapped by their indices.
        /// </summary>
        private RCSet<int>[] savedSelections;

        /// <summary>
        /// The maximum number of selections that can be saved.
        /// </summary>
        private const int CAPACITY = 10;
    }
}
