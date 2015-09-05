using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.Views
{
    /// <summary>
    /// Interface of views that provides detailed informations about the current selection.
    /// </summary>
    public interface ISelectionDetailsView
    {
        /// <summary>
        /// Gets the number of currently selected map objects.
        /// </summary>
        int SelectionCount { get; }

        /// <summary>
        /// Gets the ID of the map object with the given selection ordinal.
        /// </summary>
        /// <param name="selectionOrdinal">The selection ordinal of the map object.</param>
        /// <returns>The ID of the map object with the given selection ordinal.</returns>
        int GetObjectID(int selectionOrdinal);
    }
}
