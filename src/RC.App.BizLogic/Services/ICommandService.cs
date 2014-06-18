using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Common.ComponentModel;

namespace RC.App.BizLogic.Services
{
    /// <summary>
    /// The interface of the command service for sending simulation commands to the game engine.
    /// </summary>
    [ComponentInterface]
    public interface ICommandService
    {
        /// <summary>
        /// Clears the current selection and selects an object on the map at the given position. If there is
        /// no object on the map at the given position then this function has no effect.
        /// </summary>
        /// <param name="displayedArea">The area of the map currently being displayed in pixels.</param>
        /// <param name="position">The position inside the map displayed area in pixels.</param>
        void Select(RCIntRectangle displayedArea, RCIntVector position);

        /// <summary>
        /// Clears the current selection and selects an object on the map inside the given selection box. If there is
        /// no object on the map inside the given selection box then this function has no effect.
        /// </summary>
        /// <param name="displayedArea">The area of the map currently being displayed in pixels.</param>
        /// <param name="selectionBox">The selection box inside the map displayed area in pixels.</param>
        void Select(RCIntRectangle displayedArea, RCIntRectangle selectionBox);

        /// <summary>
        /// Clears the current selection and selects every objects inside the given displayed area that has the same
        /// type as the object at the given position. If there is no object on the map at the given position then
        /// this function has no effect.
        /// </summary>
        /// <param name="displayedArea">The area of the map currently being displayed in pixels.</param>
        /// <param name="position">The position inside the map displayed area in pixels.</param>
        void SelectType(RCIntRectangle displayedArea, RCIntVector position);

        /// <summary>
        /// Sends a fast command for the current selection. The target position of the fast command will be the
        /// given position.
        /// </summary>
        /// <param name="displayedArea">The area of the map currently being displayed in pixels.</param>
        /// <param name="position">The position inside the map displayed area in pixels.</param>
        /// <exception cref="InvalidOperationException">
        /// If fast command cannot be sent in the current state.
        /// </exception>
        void FastCommand(RCIntRectangle displayedArea, RCIntVector position);

        /// <summary>
        /// Notifies the backend that the given command button has been pressed on the command panel.
        /// </summary>
        /// <param name="panelPosition">The position of the pressed button.</param>
        /// <exception cref="InvalidOperationException">
        /// If there is no command button at the given position in the current state.
        /// </exception>
        void CommandBtnPressed(RCIntVector panelPosition);

        /// <summary>
        /// Notifies the backend that a target position has been selected on the map.
        /// </summary>
        /// <param name="displayedArea">The area of the map currently being displayed in pixels.</param>
        /// <param name="position">The position inside the map displayed area in pixels.</param>
        /// <exception cref="InvalidOperationException">
        /// If target position cannot be selected in the current state.
        /// </exception>
        void PositionSelected(RCIntRectangle displayedArea, RCIntVector position);

        /// <summary>
        /// Notifies the backend that a build position has been selected on the map.
        /// </summary>
        /// <param name="displayedArea">The area of the map currently being displayed in pixels.</param>
        /// <param name="position">The position inside the map displayed area in pixels.</param>
        /// <exception cref="InvalidOperationException">
        /// If build position cannot be selected in the current state.
        /// </exception>
        void BuildPositionSelected(RCIntRectangle displayedArea, RCIntVector position);
    }
}
