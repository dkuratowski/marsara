using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.Views
{
    /// <summary>
    /// Interface of views of the command panel.
    /// </summary>
    public interface ICommandPanelView
    {
        /// <summary>
        /// Gets the sprite definitions for displaying the buttons on the command panel.
        /// </summary>
        /// <returns>The list of the sprite definitions for displaying the buttons on the command panel.</returns>
        List<SpriteDef> GetCmdButtonSpriteDefs();

        /// <summary>
        /// Gets the sprite to be displayed for the given command button.
        /// </summary>
        /// <param name="row">The row in which the command button is located on the command panel.</param>
        /// <param name="col">The column in which the command button is located on the command panel.</param>
        /// <returns>The sprite to be displayed for a given command button.</returns>
        /// <exception cref="InvalidOperationException">
        /// If there is no command button at the given position on the command panel.
        /// </exception>
        SpriteInst GetCmdButtonSprite(int row, int col);

        /// <summary>
        /// Gets the state of the given command button.
        /// </summary>
        /// <param name="row">The row in which the command button is located on the command panel.</param>
        /// <param name="col">The column in which the command button is located on the command panel.</param>
        /// <returns>
        /// The state of the given command button or CmdButtonStateEnum.None if there is no command button at the
        /// given position on the command panel.
        /// </returns>
        CmdButtonStateEnum GetCmdButtonState(int row, int col);

        /// <summary>
        /// Gets the currently active mouse input mode.
        /// </summary>
        MouseInputModeEnum InputMode { get; }

        /// <summary>
        /// The name of the currently selected building type if ICommandPanelView.InputMode is MouseInputModeEnum.BuildPositionInputMode;
        /// otherwise null.
        /// </summary>
        string SelectedBuildingType { get; }
    }
}
