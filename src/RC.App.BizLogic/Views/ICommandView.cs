using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.App.BizLogic.Views
{
    /// <summary>
    /// Interface of views of the command panel.
    /// </summary>
    public interface ICommandView
    {
        /// <summary>
        /// Gets the sprite definitions for displaying the buttons on the command panel.
        /// </summary>
        /// <returns>The list of the sprite definitions for displaying the buttons on the command panel.</returns>
        List<SpriteDef> GetCmdButtonSpriteDefs();

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
        /// Gets whether the command service is waiting for a target position.
        /// </summary>
        bool IsWaitingForTargetPosition { get; }

        /// <summary>
        /// The name of the currently selected building type if the ICommandView.IsWaitingForTargetPosition flag is true and
        /// the command service is waiting for a target position for a build command; otherwise null.
        /// </summary>
        string SelectedBuildingType { get; }
    }
}
