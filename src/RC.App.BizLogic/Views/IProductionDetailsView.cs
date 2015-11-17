using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.App.BizLogic.Views
{
    /// <summary>
    /// Interface of views that provide informations about the production details of the currently selected map object.
    /// </summary>
    public interface IProductionDetailsView
    {
        /// <summary>
        /// Gets the capacity of the currently active production line of the selected map object.
        /// </summary>
        /// <returns>
        /// The capacity of the currently active production line of the selected map object.
        /// This property is 0 in the following cases:
        ///     - There is no selected map object.
        ///     - More than 1 map objects are selected.
        ///     - The selected map object has no active production line.
        ///     - The owner of the selected map object is not the local player.
        /// </returns>
        /// <remarks>A production line is active if and only if at least 1 item has been added to it.</remarks>
        int ProductionLineCapacity { get; }

        /// <summary>
        /// Gets the number of items in the currently active production line of the selected map object.
        /// </summary>
        /// <returns>
        /// The number of items in the currently active production line of the selected map object.
        /// This property is 0 in the following cases:
        ///     - There is no selected map object.
        ///     - More than 1 map objects are selected.
        ///     - The selected map object has no active production line.
        ///     - The owner of the selected map object is not the local player.
        /// </returns>
        /// <remarks>A production line is active if and only if at least 1 item has been added to it.</remarks>
        int ProductionLineItemCount { get; }

        /// <summary>
        /// Gets the progress of the currently running production job in the currently active production line of the
        /// selected map object. The value is normalized between 0 and 1.
        /// </summary>
        /// <returns>
        /// The progress of the current production job normalized between 0 and 1.
        /// This property is -1 in the following cases:
        ///     - There is no selected map object.
        ///     - More than 1 map objects are selected.
        ///     - The selected map object has no active production line.
        ///     - The owner of the selected map object is not the local player.
        /// </returns>
        RCNumber ProductionLineProgressNormalized { get; }

        /// <summary>
        /// Gets the construction progress of the selected map object. The value is normalized between 0 and 1.
        /// </summary>
        /// <returns>
        /// The construction progress of the selected map object normalized between 0 and 1.
        /// This property is -1 in the following cases:
        ///     - There is no selected map object.
        ///     - More than 1 map objects are selected.
        ///     - The selected map object is currently not under construction.
        ///     - The owner of the selected map object is not the local player.
        /// </returns>
        RCNumber ConstructionProgressNormalized { get; }

        /// <summary>
        /// Gets the icon of the given job in the currently active production line of the selected map object.
        /// </summary>
        /// <param name="jobIndex">The index of the job in the production line.</param>
        /// <returns>The icon of the given job in the currently active production line of the selected map object.</returns>
        /// <exception cref="InvalidOperationException">
        /// In the following cases:
        ///     - There is no selected map object.
        ///     - More than 1 map objects are selected.
        ///     - The selected map object has no active production line.
        ///     - The owner of the selected map object is not the local player.
        ///     - There is no job at the given index in the active production line of the selected map object.
        /// </exception>
        SpriteRenderInfo GetProductionJobIcon(int jobIndex);
    }
}
