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
            this.savedSelections = new RCSet<int>[SELECTION_SAVE_CAPACITY];
            for (int i = 0; i < SELECTION_SAVE_CAPACITY; i++)
            {
                this.savedSelections[i] = new RCSet<int>();
            }
        }

        /// <see cref="ISelectionManagerBC.GetEntity"/>
        public int GetEntity(RCNumVector position)
        {
            if (this.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (this.localPlayer == PlayerEnum.Neutral) { throw new InvalidOperationException("Selection manager not initialized!"); }

            Entity entityAtPos = this.ActiveScenario.GetElementsOnMap<Entity>(position).FirstOrDefault();
            if (entityAtPos == null) { return -1; }
            return entityAtPos.ID.Read();
        }

        /// <see cref="ISelectionManagerBC.SelectEntities"/>
        public void SelectEntities(RCNumRectangle selectionBox)
        {
            if (this.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (this.localPlayer == PlayerEnum.Neutral) { throw new InvalidOperationException("Selection manager not initialized!"); }

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
                        if (this.currentSelection.Count == MAX_SELECTION_SIZE) { break; }
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

            Entity entityAtPos = this.ActiveScenario.GetElementsOnMap<Entity>(position).FirstOrDefault();
            if (entityAtPos == null) { return; }
            this.currentSelection.Clear();
            this.AddEntityToSelection(entityAtPos);
        }

        /// <see cref="ISelectionManagerBC.SelectEntity"/>
        public void SelectEntity(int entityID)
        {
            if (this.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (this.localPlayer == PlayerEnum.Neutral) { throw new InvalidOperationException("Selection manager not initialized!"); }

            Entity entity = this.ActiveScenario.GetElementOnMap<Entity>(entityID);
            if (entity == null) { throw new InvalidOperationException(string.Format("Entity with ID '{0}' doesn't exist!", entityID)); }

            this.currentSelection.Clear();
            this.AddEntityToSelection(entity);
        }

        /// <see cref="ISelectionManagerBC.AddRemoveEntityToSelection"/>
        public void AddRemoveEntityToSelection(RCNumVector position)
        {
            if (this.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (this.localPlayer == PlayerEnum.Neutral) { throw new InvalidOperationException("Selection manager not initialized!"); }

            Entity entityAtPos = this.ActiveScenario.GetElementsOnMap<Entity>(position).FirstOrDefault();
            if (entityAtPos == null) { return; }

            /// If the entity is already selected then remove it from the selection.
            if (this.currentSelection.Remove(entityAtPos.ID.Read())) { return; }

            /// Otherwise add it to the current selection if possible.
            this.AddEntityToSelection(entityAtPos);
        }

        /// <see cref="ISelectionManagerBC.AddEntitiesToSelection"/>
        public void AddEntitiesToSelection(RCNumRectangle selectionBox)
        {
            if (this.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (this.localPlayer == PlayerEnum.Neutral) { throw new InvalidOperationException("Selection manager not initialized!"); }

            this.Update();
            foreach (Entity entityToAdd in this.ActiveScenario.GetElementsOnMap<Entity>(selectionBox))
            {
                this.AddEntityToSelection(entityToAdd);
            }
        }

        /// <see cref="ISelectionManagerBC.RemoveEntityFromSelection"/>
        public void RemoveEntityFromSelection(int entityID)
        {
            if (this.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (this.localPlayer == PlayerEnum.Neutral) { throw new InvalidOperationException("Selection manager not initialized!"); }

            Entity entity = this.ActiveScenario.GetElementOnMap<Entity>(entityID);
            if (entity == null) { throw new InvalidOperationException(string.Format("Entity with ID '{0}' doesn't exist!", entityID)); }

            if (!this.currentSelection.Remove(entity.ID.Read()))
            {
                throw new InvalidOperationException(string.Format("Entity with ID '{0}' is not selected!", entityID));
            }
        }

        /// <see cref="ISelectionManagerBC.SelectEntitiesOfTheSameType"/>
        public void SelectEntitiesOfTheSameType(RCNumVector position, RCNumRectangle selectionBox)
        {
            if (this.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (this.localPlayer == PlayerEnum.Neutral) { throw new InvalidOperationException("Selection manager not initialized!"); }

            Entity entityAtPos = this.ActiveScenario.GetElementsOnMap<Entity>(position).FirstOrDefault();
            if (entityAtPos == null) { return; }

            this.currentSelection.Clear();
            if (BizLogicHelpers.GetMapObjectOwner(entityAtPos.MapObject) == this.localPlayer &&
                entityAtPos is Unit)
            {
                foreach (Entity entityToAdd in this.ActiveScenario.GetElementsOnMap<Entity>(selectionBox))
                {
                    if (BizLogicHelpers.GetMapObjectOwner(entityToAdd.MapObject) == this.localPlayer &&
                        entityToAdd.ElementType == entityAtPos.ElementType) // TODO: this might be problematic in case of Terran Siege Tanks!
                    {
                        this.AddEntityToSelection(entityToAdd);
                    }
                }
            }
            else
            {
                this.AddEntityToSelection(entityAtPos);
            }
        }

        /// <see cref="ISelectionManagerBC.SelectEntitiesOfTheSameType"/>
        public void SelectEntitiesOfTheSameTypeFromCurrentSelection(int entityID)
        {
            if (this.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (this.localPlayer == PlayerEnum.Neutral) { throw new InvalidOperationException("Selection manager not initialized!"); }
            if (!this.currentSelection.Contains(entityID)) { throw new InvalidOperationException(string.Format("Entity with ID '{0}' is not selected!", entityID)); }

            Entity entity = this.ActiveScenario.GetElementOnMap<Entity>(entityID);
            if (entity == null) { throw new InvalidOperationException(string.Format("Entity with ID '{0}' doesn't exist!", entityID)); }

            List<Entity> currentSelection = this.GetSelectedEntities();
            this.currentSelection.Clear();
            foreach (Entity entityToAdd in currentSelection)
            {
                if (entityToAdd.ElementType == entity.ElementType) // TODO: this might be problematic in case of Terran Siege Tanks!
                {
                    this.AddEntityToSelection(entityToAdd);
                }
            }
        }

        /// <see cref="ISelectionManagerBC.LoadSelection"/>
        public void LoadSelection(int index)
        {
            if (this.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (this.localPlayer == PlayerEnum.Neutral) { throw new InvalidOperationException("Selection manager not initialized!"); }

            throw new NotImplementedException();
        }

        /// <see cref="ISelectionManagerBC.SaveCurrentSelection"/>
        public void SaveCurrentSelection(int index)
        {
            if (this.ActiveScenario == null) { throw new InvalidOperationException("No active scenario!"); }
            if (this.localPlayer == PlayerEnum.Neutral) { throw new InvalidOperationException("Selection manager not initialized!"); }

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
                Entity entity = this.ActiveScenario.GetElementOnMap<Entity>(id);
                if (entity == null) { idsToRemove.Add(id); }
            }
            foreach (RCSet<int> savedSelection in this.savedSelections)
            {
                foreach (int id in savedSelection)
                {
                    Entity entity = this.ActiveScenario.GetElementOnMap<Entity>(id);
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
        /// Adds the given entity to the current selection if possible.
        /// </summary>
        /// <param name="entityToAdd">The entity to be added to the current selection if possible.</param>
        private void AddEntityToSelection(Entity entityToAdd)
        {
            if (this.currentSelection.Count == MAX_SELECTION_SIZE) { return; }

            List<Entity> currentSelection = this.GetSelectedEntities();
            if (currentSelection.Count == 0)
            {
                /// Empty selection -> simply add the entity.
                this.currentSelection.Add(entityToAdd.ID.Read());
                return;
            }

            if (currentSelection.Count == 1 &&
                BizLogicHelpers.GetMapObjectOwner(currentSelection[0].MapObject) == this.localPlayer &&
                BizLogicHelpers.GetMapObjectOwner(entityToAdd.MapObject) == this.localPlayer &&
                currentSelection[0] is Unit &&
                entityToAdd is Unit)
            {
                /// Only 1 entity is selected -> add if both the new and the selected entities are units and owned by the local player.
                this.currentSelection.Add(entityToAdd.ID.Read());
                return;
            }

            if (currentSelection.TrueForAll(selectedEntity =>
                    BizLogicHelpers.GetMapObjectOwner(selectedEntity.MapObject) == this.localPlayer &&
                    BizLogicHelpers.GetMapObjectOwner(entityToAdd.MapObject) == this.localPlayer &&
                    selectedEntity is Unit) &&
                entityToAdd is Unit)
            {
                /// Multiple entities are selected -> add if all selected entities and the new entity are units and owned by the local player.
                this.currentSelection.Add(entityToAdd.ID.Read());
                return;
            }
        }

        /// <summary>
        /// Gets the list of entities currently selected.
        /// </summary>
        /// <returns>The currently selected entities.</returns>
        private List<Entity> GetSelectedEntities()
        {
            List<Entity> retList = new List<Entity>();
            foreach (int selectedEntityID in this.currentSelection)
            {
                Entity selectedEntity = this.ActiveScenario.GetElementOnMap<Entity>(selectedEntityID);
                if (selectedEntity != null) { retList.Add(selectedEntity); }
            }
            return retList;
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
        private const int SELECTION_SAVE_CAPACITY = 10;

        /// <summary>
        /// The maximum number of objects that can be selected.
        /// </summary>
        private const int MAX_SELECTION_SIZE = 12;
    }
}
