using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// Interface of the player manager business object.
    /// </summary>
    interface IPlayerManager
    {
        /// <summary>
        /// Gets the player slot at the given index.
        /// </summary>
        /// <param name="index">The index of the player slot to get.</param>
        /// <returns>The player slot at the given index.</returns>
        IPlayerSlot this[int index] { get; }

        /// <summary>
        /// Gets the number of available player slots.
        /// </summary>
        int NumberOfSlots { get; }
    }
}
