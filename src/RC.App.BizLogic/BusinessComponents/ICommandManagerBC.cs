using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.Views;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Common.Configuration;
using RC.Engine.Simulator.Commands;

namespace RC.App.BizLogic.BusinessComponents
{
    /// <summary>
    /// Interface of the command manager business component.
    /// </summary>
    [ComponentInterface]
    interface ICommandManagerBC
    {
        /// <summary>
        /// Gets the sprite palettes used by the command input nodes.
        /// </summary>
        IEnumerable<ISpritePalette> SpritePalettes { get; }

        /// <summary>
        /// Notifies the command manager that the given command button has been pressed on the command panel.
        /// </summary>
        /// <param name="panelPosition">The position of the pressed button on the command panel.</param>
        /// <exception cref="InvalidOperationException">
        /// If there is no command button at the given position in the current state or the button is disabled.
        /// </exception>
        void PressCommandButton(RCIntVector panelPosition);

        /// <summary>
        /// Notifies the command manager that a target position has been selected on the map.
        /// </summary>
        /// <param name="position">The position on the map (in map coordinates).</param>
        /// <exception cref="InvalidOperationException">
        /// If target position cannot be selected in the current state.
        /// </exception>
        void SelectTargetPosition(RCNumVector position);

        /// <summary>
        /// Notifies the command manager that the user cancelled selecting a target position.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// If target position cannot be selected in the current state.
        /// </exception>
        void CancelSelectingTargetPosition();

        /// <summary>
        /// Gets the sprite to be displayed for the given command button.
        /// </summary>
        /// <param name="panelPosition">The position of the command button on the command panel (row; column).</param>
        /// <returns>The sprite to be displayed for a given command button.</returns>
        /// <exception cref="InvalidOperationException">
        /// If there is no command button at the given position on the command panel.
        /// </exception>
        SpriteInst GetCmdButtonSprite(RCIntVector panelPosition);

        /// <summary>
        /// Gets the state of the given command button.
        /// </summary>
        /// <param name="panelPosition">The position of the command button on the command panel (row; column).</param>
        /// <returns>
        /// The state of the given command button or CommandButtonStateEnum.Invisible if there is no command button at the
        /// given position on the command panel.
        /// </returns>
        CommandButtonStateEnum GetCmdButtonState(RCIntVector panelPosition);

        /// <summary>
        /// Updates the state of the command input procedure.
        /// </summary>
        void Update();

        /// <summary>
        /// Gets whether the command manager is waiting for a target position.
        /// </summary>
        bool IsWaitingForTargetPosition { get; }

        /// <summary>
        /// The name of the currently selected building type if the ICommandManagerBC.IsWaitingForTargetPosition flag is true and
        /// the command manager is waiting for a target position for a build command; otherwise null.
        /// </summary>
        string SelectedBuildingType { get; }

        /// <summary>
        /// This event is raised when a new command has been created.
        /// </summary>
        event Action<RCCommand> NewCommand;
    }
}
