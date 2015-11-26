using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.Views
{
    /// <summary>
    /// Interface of views that provide detailed informations about the local player.
    /// </summary>
    public interface IPlayerView
    {
        /// <summary>
        /// Gets the current amount of minerals of the local player.
        /// </summary>
        int Minerals { get; }

        /// <summary>
        /// Gets the current amount of vespene gas of the local player.
        /// </summary>
        int VespeneGas { get; }

        /// <summary>
        /// Gets the amount of the supplies currently used by the local player.
        /// </summary>
        int UsedSupply { get; }

        /// <summary>
        /// Gets the total amount of supplies owned by the local player.
        /// </summary>
        int TotalSupply { get; }

        /// <summary>
        /// Gets the maximum amount of supplies can be owned by the local player.
        /// </summary>
        int MaxSupply { get; }
    }
}
