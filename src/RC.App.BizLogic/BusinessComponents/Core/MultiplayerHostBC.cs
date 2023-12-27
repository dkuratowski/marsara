using System;
using System.Collections.Generic;
using System.Threading;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Common.Diagnostics;
using RC.App.BizLogic.BusinessComponents;
using RC.DssServices;
using RC.NetworkingSystem;
using RC.App.BizLogic.Services;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// The implementation of the multiplayer service.
    /// </summary>
    [Component("RC.App.BizLogic.MultiplayerHostBC")]
    class MultiplayerHostBC : IMultiplayerHostBC, IComponent
    {
        /// <summary>
        /// Constructs a MultiplayerHostBC instance.
        /// </summary>
        public MultiplayerHostBC()
        {
            this.hostTask = null;
            this.setup = null;
            this.simulator = null;
        }

        #region IComponent methods

        /// <see cref="IComponent.Start"/>
        public void Start()
        {
            this.lan = ComponentManager.GetInterface<ILocalAreaNetworkBC>();
            this.taskManager = ComponentManager.GetInterface<ITaskManagerBC>();
            this.scenarioManager = ComponentManager.GetInterface<IScenarioManagerBC>();
        }

        /// <see cref="IComponent.Stop"/>
        public void Stop()
        {
        }

        #endregion IComponent methods

        #region IMultiplayerHostBC methods

        /// <see cref="IMultiplayerHostBC.BeginHosting"/>
        public void BeginHosting(string hostName, byte[] mapBytes, GameTypeEnum gameType, GameSpeedEnum gameSpeed)
        {
            if (this.hostTask != null) { throw new InvalidOperationException("Multiplayer game hosting is already in progress!"); }

            IPlayerManager playerManager = new PlayerManager(this.scenarioManager.ActiveScenario);
            this.setup = new MultiplayerHostSetup(hostName, mapBytes, playerManager); // TODO: don't forget to dispose!
            this.simulator = new MultiplayerSimulator(playerManager);
            this.hostTask = this.taskManager.StartTask(this.DssHostMethod, "DssThread", playerManager);
        }

        #endregion IMultiplayerHostBC methods

        private void DssHostMethod(object param)
        {
            IPlayerManager playerManager = (IPlayerManager)param;
            DssServiceAccess.CreateDSS(playerManager.NumberOfSlots, this.lan.LAN, this.simulator, this.setup);
        }

        /// <summary>
        /// Reference to the host background task.
        /// </summary>
        private ITask hostTask;

        /// <summary>
        /// Reference to the LAN.
        /// </summary>
        private ILocalAreaNetworkBC lan;

        /// <summary>
        /// Reference to the task manager.
        /// </summary>
        private ITaskManagerBC taskManager;

        /// <summary>
        /// Reference to the scenario manager business component.
        /// </summary>
        private IScenarioManagerBC scenarioManager;

        private MultiplayerHostSetup setup;

        private MultiplayerSimulator simulator;
    }
}
