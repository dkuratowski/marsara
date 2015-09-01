using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.Views
{
    /// <summary>
    /// Interface of views of the details panel.
    /// </summary>
    public interface IDetailsView
    {
        /// <summary>
        /// Gets the number of currently selected map objects.
        /// </summary>
        int SelectionCount { get; }

        /// <summary>
        /// Gets the condition of the HP of the map object with the given selection ordinal.
        /// </summary>
        /// <param name="selectionOrdinal">The selection ordinal of the map object.</param>
        /// <returns>The condition of the HP of the map object with the given selection ordinal.</returns>
        MapObjectConditionEnum GetHPCondition(int selectionOrdinal);

        /// <summary>
        /// Gets the HP indicator icon of the map object with the given selection ordinal.
        /// </summary>
        /// <param name="selectionOrdinal">The selection ordinal of the map object.</param>
        /// <returns>The HP indicator icon of the map object with the given selection ordinal.</returns>
        SpriteInst GetHPIcon(int selectionOrdinal);
    }
}
