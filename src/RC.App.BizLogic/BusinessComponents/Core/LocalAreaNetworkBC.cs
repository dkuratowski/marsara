using System;
using System.Collections.Generic;
using System.Threading;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Common.Diagnostics;
using RC.NetworkingSystem;
using RC.App.BizLogic.BusinessComponents;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// The BC wrapper over the RC.NetworkingSystem.
    /// </summary>
    [Component("RC.App.BizLogic.LocalAreaNetworkBC")]
    class LocalAreaNetworkBC : ILocalAreaNetworkBC, IComponent
    {
        /// <summary>
        /// Constructs a LocalAreaNetworkBC instance.
        /// </summary>
        public LocalAreaNetworkBC()
        {
        }

        #region IComponent methods

        /// <see cref="IComponent.Start"/>
        public void Start()
        {
            TraceManager.WriteAllTrace("LocalAreaNetworkBC.Start", TraceManager.GetTraceFilterID("RC.App.BizLogic.Info"));

            this.lan = Network.CreateLocalAreaNetwork(new List<int> { 25000, 25001, 25002, 25003});
        }

        /// <see cref="IComponent.Stop"/>
        public void Stop()
        {
            this.lan.ShutdownNetwork();
        }

        #endregion IComponent methods

        #region ILocalAreaNetworkBC methods

        /// <see cref="ILocalAreaNetworkBC.LAN"/>
        public INetwork LAN { get { return this.lan; } }

        #endregion ILocalAreaNetworkBC methods

        /// <summary>
        /// Reference to the LAN.
        /// </summary>
        private INetwork lan;
    }
}
