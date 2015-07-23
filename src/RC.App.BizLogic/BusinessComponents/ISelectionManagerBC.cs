using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.Views;
using RC.Common;
using RC.Common.ComponentModel;

namespace RC.App.BizLogic.BusinessComponents
{
    /// <summary>
    /// Interface of the selection manager business component.
    /// </summary>
    [ComponentInterface]
    public interface ISelectionManagerBC
    {
        /// <summary>
        /// Resets the selection manager.
        /// </summary>
        /// <param name="localPlayer">The index of the local player.</param>
        void Reset(PlayerEnum localPlayer);

        /// <summary>
        /// Gets the ID of the entity at the given position.
        /// </summary>
        /// <param name="position">The position at which to search for an entity (in map coordinates).</param>
        /// <returns>The ID of the entity at the given position or -1 if there is no entity at the given position.</returns>
        int GetEntity(RCNumVector position);

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
        void SelectEntities(RCNumRectangle selectionBox);

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
        void SelectEntity(RCNumVector position);

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
        void AddRemoveEntityToSelection(RCNumVector position);

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
        void AddEntitiesToSelection(RCNumRectangle selectionBox);

        /// <summary>
        /// Loads the selection that were saved with the given index.
        /// </summary>
        /// <param name="index">The index of the saved selection (0-9).</param>
        /// <remarks>
        /// If there is no selection that were saved with the given index then calling this function has no effect.
        /// If every entities in the selection that were saved with the given index has already been destroyed then
        /// calling this function has no effect.
        /// </remarks>
        void LoadSelection(int index);

        /// <summary>
        /// Saves the current selection with the given index.
        /// </summary>
        /// <param name="index">The index of the saved selection (0-9).</param>
        /// <remarks>
        /// If the current selection contains an entity of another player then an empty selection will be saved.
        /// If another selection has already been saved with the given index then it will be overwritten.
        /// </remarks>
        void SaveCurrentSelection(int index);

        /// <summary>
        /// Gets the IDs of the currently selected entities.
        /// </summary>
        RCSet<int> CurrentSelection { get; }

        /// <summary>
        /// Gets the index of the local player or PlayerEnum.Neutral if the selection manager has not yet been initialized.
        /// </summary>
        PlayerEnum LocalPlayer { get; }
    }
}
