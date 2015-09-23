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
        /// Sends a fast command for the current selection. The target position of the fast command will be the
        /// given position.
        /// </summary>
        /// <param name="position">The position inside the displayed area in pixels.</param>
        /// <exception cref="InvalidOperationException">
        /// If fast command cannot be sent in the current state.
        /// </exception>
        void SendFastCommand(RCIntVector position);

        /// <summary>
        /// Sends a fast command for the current selection on the minimap. The target position of the fast command will be the
        /// given position.
        /// </summary>
        /// <param name="position">The position inside the minimap control in pixels.</param>
        /// <exception cref="InvalidOperationException">
        /// If fast command cannot be sent in the current state.
        /// </exception>
        void SendFastCommandOnMinimap(RCIntVector position);

        /// <summary>
        /// Notifies the backend that the given command button has been pressed on the command panel.
        /// </summary>
        /// <param name="panelPosition">The position of the pressed button.</param>
        /// <exception cref="InvalidOperationException">
        /// If there is no command button at the given position in the current state.
        /// </exception>
        void PressCommandButton(RCIntVector panelPosition);

        /// <summary>
        /// Notifies the backend that the given production button has been pressed on the production line display.
        /// </summary>
        /// <param name="panelPosition">The position of the pressed button.</param>
        /// <exception cref="InvalidOperationException">
        /// If there is no production button at the given position in the current state.
        /// </exception>
        void PressProductionButton(int panelPosition);

        /// <summary>
        /// Notifies the backend that a target position has been selected on the map.
        /// </summary>
        /// <param name="position">The position inside the displayed area in pixels.</param>
        /// <exception cref="InvalidOperationException">
        /// If target position cannot be selected in the current state.
        /// </exception>
        void SelectTargetPosition(RCIntVector position);

        /// <summary>
        /// Notifies the backend that a target position has been selected on the minimap.
        /// </summary>
        /// <param name="position">The position inside the minimap control in pixels.</param>
        /// <exception cref="InvalidOperationException">
        /// If target position cannot be selected in the current state.
        /// </exception>
        void SelectTargetPositionOnMinimap(RCIntVector position);

        /// <summary>
        /// Notifies the backend that the user cancelled selecting a target position.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// If target position cannot be selected in the current state.
        /// </exception>
        void CancelSelectingTargetPosition();
    }
}
