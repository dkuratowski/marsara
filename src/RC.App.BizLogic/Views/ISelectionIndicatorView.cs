using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.Views
{
    /// <summary>
    /// Interface of views on the selection indicators of selected objects of the currently running game.
    /// </summary>
    public interface ISelectionIndicatorView
    {
        /// <summary>
        /// Gets the list of the visible selection indicators at the displayed area in the order as they shall be displayed.
        /// </summary>
        /// <returns>The list of display informations of the visible selection indicators.</returns>
        List<SelIndicatorInst> GetVisibleSelIndicators();

        /// <summary>
        /// Gets the index of the local player or PlayerEnum.Neutral if there is no active local player.
        /// </summary>
        PlayerEnum LocalPlayer { get; }
    }
}
