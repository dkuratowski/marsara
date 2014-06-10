using RC.Common;
using RC.Engine.Simulator.Scenarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.Views;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// This class is responsible for handling the entity selections of the local player during gameplay.
    /// </summary>
    class EntitySelector
    {
        /// <summary>
        /// Constructs an EntitySelector for the given player.
        /// </summary>
        /// <param name="scenario">Reference to the scenario in which to perform the selections.</param>
        /// <param name="owner">The index of the player that owns this EntitySelector.</param>
        public EntitySelector(Scenario scenario, PlayerEnum owner)
        {
            if (scenario == null) { throw new ArgumentNullException("scenario"); }
            if (owner == PlayerEnum.Neutral) { throw new ArgumentException("Owner cannot be PlayerEnum.Neutral!", "owner"); }

            this.scenario = scenario;
            this.owner = owner;

            this.currentSelection = new HashSet<int>();
            this.savedSelections = new HashSet<int>[CAPACITY];
            for (int i = 0; i < CAPACITY; i++)
            {
                this.savedSelections[i] = new HashSet<int>();
            }
        }

        /// <summary>
        /// Select entities inside the given selection box.
        /// </summary>
        /// <param name="selectionBox">The selection box in which to select the entities (in map coordinates).</param>
        /// <remarks>
        /// The selection will happen in the following priority order:
        ///     - All the units of the owner inside the box.
        ///     - One of the buildings of the owner inside the box.
        ///     - One of the addons of the owner inside the box.
        ///     - One of the units of another player inside the box.
        ///     - One of the buildings of another player inside the box.
        ///     - One of the addons of another player inside the box.
        ///     - One of the other entities inside the box.
        /// If there are no entities inside the selection box then the current selection will remain.
        /// </remarks>
        public void Select(RCNumRectangle selectionBox)
        {
            this.Update();

            bool ownerUnitFound = false;
            Entity ownerBuilding = null;
            Entity ownerAddon = null;
            Entity otherPlayerUnit = null;
            Entity otherPlayerBuilding = null;
            Entity otherPlayerAddon = null;
            Entity other = null;
            foreach (Entity entity in this.scenario.VisibleEntities.GetContents(selectionBox))
            {
                if (entity is Unit)
                {
                    /// If the entity is a unit then check its owner.
                    if (entity.Owner.PlayerIndex == (int)this.owner)
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
                    if (entity.Owner.PlayerIndex == (int)this.owner)
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
                    if (entity.Owner.PlayerIndex == (int)this.owner)
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

        /// <summary>
        /// Select the entity at the given position.
        /// </summary>
        /// <param name="position">The position at which to select an entity (in map coordinates).</param>
        /// <remarks>
        /// The selection will happen in the following priority order:
        ///     - One of the units of the owner at the given position.
        ///     - One of the buildings of the owner at the given position.
        ///     - One of the addons of the owner at the given position.
        ///     - One of the units of another player at the given position.
        ///     - One of the buildings of another player at the given position.
        ///     - One of the addons of another player at the given position.
        ///     - One of the other entity at the given position.
        /// If there is no entity at the given position then calling this function has no effect.
        /// </remarks>
        public void Select(RCNumVector position)
        {
            this.Update();

            HashSet<Entity> entitiesAtPos = this.scenario.VisibleEntities.GetContents(position);
            if (entitiesAtPos.Count == 0) { return; }

            Entity entityAtPos = null;
            foreach (Entity e in entitiesAtPos) { entityAtPos = e; break; }
            this.currentSelection.Clear();
            this.currentSelection.Add(entityAtPos.ID.Read());
        }

        /// <summary>
        /// Adds or remove the entity at the given position to or from the current selection.
        /// </summary>
        /// <param name="position">The given position (in map coordinates).</param>
        /// <remarks>
        /// If the entity at the given position cannot be added to the current selection for any reason then this
        /// function has no effect. The possible reasons are the followings:
        ///     - A building or an addon is currently selected.
        ///     - Some units of the owner is currently selected and the entity is a building or an addon.
        ///     - Some units of the owner is currently selected and the entity is owned by another player.
        ///     - An entity of another player is currently selected.
        /// </remarks>
        public void AddRemoveEntityToSelection(RCNumVector position)
        {
            this.Update();
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds the entities inside the given selection box to the current selection.
        /// </summary>
        /// <param name="selectionBox">The given selection box (in map coordinates).</param>
        /// <remarks>
        /// Only entities inside the given selection box that can be added to the current selection will be added.
        /// The possible reasons why an entity cannot be added to the current selection are the followings:
        ///     - A building or an addon is currently selected.
        ///     - Some units of the owner is currently selected and the entity is a building or an addon.
        ///     - Some units of the owner is currently selected and the entity is owned by another player.
        ///     - An entity of another player is currently selected.
        /// </remarks>
        public void AddEntitiesToSelection(RCNumRectangle selectionBox)
        {
            this.Update();
            throw new NotImplementedException();
        }

        /// <summary>
        /// Loads the selection that were saved with the given index.
        /// </summary>
        /// <param name="index">The index of the saved selection (0-9).</param>
        /// <remarks>
        /// If there is no selection that were saved with the given index then calling this function has no effect.
        /// If every entities in the selection that were saved with the given index has already been destroyed then
        /// calling this function has no effect.
        /// </remarks>
        public void LoadSelection(int index)
        {
            this.Update();
            throw new NotImplementedException();
        }

        /// <summary>
        /// Saves the current selection with the given index.
        /// </summary>
        /// <param name="index">The index of the saved selection (0-9).</param>
        /// <remarks>
        /// If the current selection contains an entity of another player then an empty selection will be saved.
        /// If another selection has already been saved with the given index then it will be overwritten.
        /// </remarks>
        public void SaveSelection(int index)
        {
            this.Update();
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the current selection.
        /// </summary>
        public HashSet<int> CurrentSelection
        {
            get
            {
                this.Update();
                return new HashSet<int>(this.currentSelection);
            }
        }

        /// <summary>
        /// Gets the target scenario of this entity selector.
        /// </summary>
        public Scenario TargetScenario { get { return this.scenario; } }

        /// <summary>
        /// Gets the index of the player that owns this EntitySelector.
        /// </summary>
        public PlayerEnum Owner { get { return this.owner; } }

        /// <summary>
        /// Updates the current and the saved selections.
        /// </summary>
        private void Update()
        {
            /// Collect the IDs to be removed.
            HashSet<int> idsToRemove = new HashSet<int>();
            foreach (int id in this.currentSelection)
            {
                if (this.scenario.GetEntity<Entity>(id) == null) { idsToRemove.Add(id); }
            }
            foreach (HashSet<int> savedSelection in this.savedSelections)
            {
                foreach (int id in savedSelection)
                {
                    if (this.scenario.GetEntity<Entity>(id) == null) { idsToRemove.Add(id); }
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
        /// The index of the player that owns this EntitySelector.
        /// </summary>
        private PlayerEnum owner;

        /// <summary>
        /// Reference to the scenario in which to perform the selections.
        /// </summary>
        private Scenario scenario;

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
