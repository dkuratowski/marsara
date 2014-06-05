using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.PublicInterfaces
{
    /// <summary>
    /// Interface of views connecting the command panel with the backend.
    /// </summary>
    public interface ICommandPanelView
    {
        /// <summary>
        /// Gets the sprite definitions for displaying the buttons on the command panel.
        /// </summary>
        /// <returns>The list of the sprite definitions for displaying the buttons on the command panel.</returns>
        List<SpriteDef> GetCmdButtonSpriteDefs();

        /// <summary>
        /// Gets a reference to the view of the given command button.
        /// </summary>
        /// <param name="row">The row in which the command button is located on the command panel.</param>
        /// <param name="col">The column in which the command button is located on the command panel.</param>
        /// <returns>A reference to the view of the given command button.</returns>
        ICommandButtonView this[int row, int col] { get; }
    }
}
