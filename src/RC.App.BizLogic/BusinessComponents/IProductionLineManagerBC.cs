using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.Views;
using RC.Common;
using RC.Common.ComponentModel;

namespace RC.App.BizLogic.BusinessComponents
{
    /// <summary>
    /// Interface of the production line manager business component.
    /// </summary>
    [ComponentInterface]
    interface IProductionLineManagerBC
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
        RCNumber ProductionProgressNormalized { get; }

        /// <summary>
        /// Gets the icon of the given item in the currently active production line of the selected map object.
        /// </summary>
        /// <param name="itemIndex">The index of the item in the production line.</param>
        /// <returns>The icon of the given item in the currently active production line of the selected map object.</returns>
        /// <exception cref="InvalidOperationException">
        /// In the following cases:
        ///     - There is no selected map object.
        ///     - More than 1 map objects are selected.
        ///     - The selected map object has no active production line.
        ///     - The owner of the selected map object is not the local player.
        ///     - There is no item at the given index in the active production line of the selected map object.
        /// </exception>
        SpriteInst GetProductionButtonSprite(int itemIndex);
    }
}
