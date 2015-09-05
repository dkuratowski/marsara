using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Common.ComponentModel;

namespace RC.App.BizLogic.Services
{
    /// <summary>
    /// The interface of the selection service for selecting map objects.
    /// </summary>
    [ComponentInterface]
    public interface ISelectionService
    {
        /// <summary>
        /// Clears the current selection and selects an object on the map at the given position. If there is
        /// no object on the map at the given position then this function has no effect.
        /// </summary>
        /// <param name="position">The position inside the displayed area in pixels.</param>
        void Select(RCIntVector position);

        /// <summary>
        /// Clears the current selection and selects the objects on the map inside the given selection box. If there is
        /// no object on the map inside the given selection box then this function has no effect.
        /// </summary>
        /// <param name="selectionBox">The selection box inside the displayed area in pixels.</param>
        void Select(RCIntRectangle selectionBox);

        /// <summary>
        /// Clears the current selection and selects the object with the given ID.
        /// </summary>
        /// <param name="objectID">The ID of the object to be selected.</param>
        /// <exception cref="InvalidOperationException">
        /// If there is no object with the given ID.
        /// </exception>
        void Select(int objectID);

        /// <summary>
        /// Adds/removes the object on the map at the given position to/from the current selection. If there is no object on the
        /// map at the given position then this function has no effect.
        /// </summary>
        /// <param name="position">The position inside the displayed area in pixels.</param>
        void AddOrRemoveFromSelection(RCIntVector position);

        /// <summary>
        /// Adds the objects on the map inside the given selection box to the current selection. If there is
        /// no object on the map inside the given selection box then this function has no effect.
        /// </summary>
        /// <param name="selectionBox">The selection box inside the displayed area in pixels.</param>
        void AddToSelection(RCIntRectangle selectionBox);

        /// <summary>
        /// Removes the object with the given ID from the current selection.
        /// </summary>
        /// <param name="objectID">The ID of the object to be selected.</param>
        /// <exception cref="InvalidOperationException">
        /// If there is no object with the given ID.
        /// </exception>
        void RemoveFromSelection(int objectID);

        /// <summary>
        /// Clears the current selection and selects every objects inside the displayed area that has the same
        /// type as the object at the given position. If there is no object on the map at the given position then
        /// this function has no effect.
        /// </summary>
        /// <param name="position">The position inside the displayed area in pixels.</param>
        void SelectType(RCIntVector position);

        /// <summary>
        /// Filters the current selection to contain only objects having the same type as the object with the given
        /// selection ordinal.
        /// </summary>
        /// <param name="selectionOrdinal">The selection ordinal of the map object.</param>
        /// <exception cref="InvalidOperationException">
        /// If there is no object selected at the given selection ordinal.
        /// </exception>
        void SelectTypeFromCurrentSelection(int selectionOrdinal);
    }
}
