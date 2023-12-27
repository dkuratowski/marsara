using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Common.ComponentModel;
using RC.NetworkingSystem;

namespace RC.App.BizLogic.BusinessComponents
{
    /// <summary>
    /// The interface for accessing the LAN.
    /// </summary>
    [ComponentInterface]
    public interface ILocalAreaNetworkBC
    {
        /// <summary>
        /// Gets a reference to the LAN.
        /// </summary>
        INetwork LAN { get; }
    }
}
