using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Common.Diagnostics;
using RC.DssServices;
using RC.Engine.Simulator.Commands;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.MotionControl;
using RC.App.BizLogic.BusinessComponents.Core;
using RC.App.BizLogic.BusinessComponents;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Pathfinder.PublicInterfaces;
using RC.NetworkingSystem;

namespace RC.App.BizLogic.Services.Core
{
    /// <summary>
    /// The implementation of the multiplayer service.
    /// </summary>
    [Component("RC.App.BizLogic.MultiplayerService")]
    class MultiplayerService : IMultiplayerService, IComponent
    {
        /// <summary>
        /// Constructs a MultiplayerGameManager instance.
        /// </summary>
        public MultiplayerService()
        {
        }

        #region IComponent methods

        /// <see cref="IComponent.Start"/>
        public void Start()
        {
            TraceManager.WriteAllTrace("MultiplayerService.Start", TraceManager.GetTraceFilterID("RC.App.BizLogic.Info"));

            this.multiplayerHost = ComponentManager.GetInterface<IMultiplayerHostBC>();

            this.scenarioManager = ComponentManager.GetInterface<IScenarioManagerBC>();
            this.selectionManager = ComponentManager.GetInterface<ISelectionManagerBC>();
            this.commandManager = ComponentManager.GetInterface<ICommandManagerBC>();
            this.fogOfWarBC = ComponentManager.GetInterface<IFogOfWarBC>();
            this.mapWindowBC = ComponentManager.GetInterface<IMapWindowBC>();
        }

        /// <see cref="IComponent.Stop"/>
        public void Stop()
        {
        }

        #endregion IComponent methods

        #region IMultiplayerService methods

        /// <see cref="IMultiplayerService.HostNewGame"/>
        public void HostNewGame(string hostName, string mapFile, GameTypeEnum gameType, GameSpeedEnum gameSpeed)
        {
            byte[] mapBytes = File.ReadAllBytes(mapFile);
            this.scenarioManager.OpenScenario(mapBytes);
            this.scenarioManager.ActiveScenario.Map.FinalizeMap();
            this.multiplayerHost.BeginHosting(hostName, mapBytes, gameType, gameSpeed);
        }

        /// <see cref="IMultiplayerService.JoinToExistingGame"/>
        public void JoinToExistingGame(string hostName, string guestName)
        {
            throw new NotImplementedException();
        }

        /// <see cref="IMultiplayerService.StartHostedGame"/>
        public void StartHostedGame()
        {
            throw new NotImplementedException();
        }

        /// <see cref="IMultiplayerService.LeaveCurrentGame"/>
        public void LeaveCurrentGame()
        {
            throw new NotImplementedException();
        }

        /// <see cref="IMultiplayerService.PostCommand"/>
        public void PostCommand(RCCommand cmd)
        {
            throw new NotImplementedException();
        }

        /// <see cref="IMultiplayerService.GameUpdated"/>
        public event Action GameUpdated;

        #endregion IMultiplayerService methods

        /// <summary>
        /// Interface to the host business component.
        /// </summary>
        private IMultiplayerHostBC multiplayerHost;

        /// <summary>
        /// Reference to the player manager of the active scenario.
        /// </summary>
        private PlayerManager playerManager;

        /// <summary>
        /// Reference to the scenario manager business component.
        /// </summary>
        private IScenarioManagerBC scenarioManager;

        /// <summary>
        /// Reference to the selection manager business component.
        /// </summary>
        private ISelectionManagerBC selectionManager;

        /// <summary>
        /// Reference to the command manager business component.
        /// </summary>
        private ICommandManagerBC commandManager;

        /// <summary>
        /// Reference to the Fog Of War business component.
        /// </summary>
        private IFogOfWarBC fogOfWarBC;

        /// <summary>
        /// Reference to the map window business component.
        /// </summary>
        private IMapWindowBC mapWindowBC;
    }
}
