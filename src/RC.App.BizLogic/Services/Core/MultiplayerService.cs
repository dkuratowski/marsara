using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Maps.ComponentInterfaces;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Scenarios;
using RC.Engine.Simulator.MotionControl;
using RC.App.BizLogic.BusinessComponents.Core;
using RC.App.BizLogic.Views;
using RC.App.BizLogic.Views.Core;
using RC.App.BizLogic.BusinessComponents;

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
            this.dssTask = null;
        }

        #region IComponent methods

        /// <see cref="IComponent.Start"/>
        public void Start()
        {
            /// TODO: remove when no longer necessary!
            this.taskManager = ComponentManager.GetInterface<ITaskManagerBC>();
            this.pathFinder = ComponentManager.GetInterface<IPathFinder>();

            this.scenarioManager = ComponentManager.GetInterface<IScenarioManagerBC>();
            this.selectionManager = ComponentManager.GetInterface<ISelectionManagerBC>();
            this.commandManager = ComponentManager.GetInterface<ICommandManagerBC>();
            this.fogOfWarBC = ComponentManager.GetInterface<IFogOfWarBC>();
            this.viewFactoryRegistry = ComponentManager.GetInterface<IViewFactoryRegistry>();
        }

        /// <see cref="IComponent.Stop"/>
        public void Stop()
        {
        }

        #endregion IComponent methods

        #region IMultiplayerService methods

        /// <see cref="IMultiplayerService.CreateNewGame"/>
        public void CreateNewGame(string mapFile, GameTypeEnum gameType, GameSpeedEnum gameSpeed)
        {
            /// TODO: this is only a PROTOTYPE implementation!
            this.scenarioManager.OpenScenario(mapFile);
            this.scenarioManager.ActiveScenario.Map.FinalizeMap();

            this.playerManager = new PlayerManager(this.scenarioManager.ActiveScenario);
            this.playerManager[0].ConnectRandomPlayer(RaceEnum.Terran);
            this.playerManager[1].ConnectRandomPlayer(RaceEnum.Terran);
            this.playerManager[2].ConnectRandomPlayer(RaceEnum.Terran);
            this.playerManager[3].ConnectRandomPlayer(RaceEnum.Terran);
            this.playerManager.Lock();
            this.selectionManager.Reset(this.playerManager[0].Player);
            this.fogOfWarBC.StartFogOfWar(this.playerManager[0].Player);
            this.commandDispatcher = new CommandDispatcher();
            this.triggeredScheduler = new TriggeredScheduler(1000 / (int)gameSpeed);
            this.triggeredScheduler.AddScheduledFunction(this.pathFinder.Flush);
            this.triggeredScheduler.AddScheduledFunction(this.ExecuteCommands);
            this.triggeredScheduler.AddScheduledFunction(this.scenarioManager.ActiveScenario.UpdateState);
            this.triggeredScheduler.AddScheduledFunction(this.scenarioManager.ActiveScenario.UpdateAnimations);
            this.triggeredScheduler.AddScheduledFunction(this.commandManager.Update);
            this.triggeredScheduler.AddScheduledFunction(this.fogOfWarBC.ExecuteUpdateIteration);
            this.triggeredScheduler.AddScheduledFunction(() => { if (this.GameUpdated != null) { this.GameUpdated(); } });
            this.testDssTaskCanFinishEvt = new ManualResetEvent(false);
            this.dssTask = this.taskManager.StartTask(this.TestDssTaskMethod, "DssThread");
        }

        /// <see cref="IMultiplayerService.LeaveCurrentGame"/>
        public void LeaveCurrentGame()
        {
            /// TODO: this is only a PROTOTYPE implementation!
            this.dssTask.Finished += this.OnDssTaskFinished;
            this.testDssTaskCanFinishEvt.Set();
        }

        /// <see cref="IMultiplayerService.PostCommand"/>
        public void PostCommand(RCCommand cmd)
        {
            this.commandDispatcher.PushOutgoingCommand(cmd);
        }

        /// <see cref="IMultiplayerService.GameUpdated"/>
        public event Action GameUpdated;

        #endregion IMultiplayerService methods

        #region Prototype code

        /// <summary>
        /// Internal function that executes the incoming commands in the current simulation frame.
        /// </summary>
        private void ExecuteCommands()
        {
            this.commandDispatcher.DispatchOutgoingCommands();
            List<RCCommand> incomingCommands = this.commandDispatcher.GetIncomingCommands();
            foreach (RCCommand command in incomingCommands)
            {
                command.Execute(this.scenarioManager.ActiveScenario);
            }
        }

        /// PROTOTYPE CODE
        private void TestDssTaskMethod(object param)
        {
            while (!this.testDssTaskCanFinishEvt.WaitOne(0))
            {
                foreach (RCPackage cmdPackage in this.commandDispatcher.GetOutgoingCommands())
                {
                    this.commandDispatcher.PushIncomingCommand(cmdPackage);
                }
                this.triggeredScheduler.Trigger();
            }
        }

        /// PROTOTYPE CODE
        private void OnDssTaskFinished(ITask sender, object message)
        {
            this.dssTask.Finished -= this.OnDssTaskFinished;
            this.dssTask = null;
            this.testDssTaskCanFinishEvt.Close();
            this.testDssTaskCanFinishEvt = null;
            this.triggeredScheduler.Dispose();
            this.triggeredScheduler = null;
            this.fogOfWarBC.StopFogOfWar(this.playerManager[0].Player);
            this.commandDispatcher = null;
            this.playerManager = null;
            this.scenarioManager.CloseScenario();
        }

        /// PROTOTYPE CODE
        private TriggeredScheduler triggeredScheduler;
        private CommandDispatcher commandDispatcher;
        private ManualResetEvent testDssTaskCanFinishEvt;
        private IPathFinder pathFinder;
        private ITaskManagerBC taskManager;

        #endregion Prototype code

        /// <summary>
        /// Reference to the task that executes the DSS-thread.
        /// </summary>
        private ITask dssTask;

        /// <summary>
        /// Reference to the player manager of the active scenario.
        /// </summary>
        private PlayerManager playerManager;

        /// <summary>
        /// Reference to the registry interface of the RC.App.BizLogic.ViewService component.
        /// </summary>
        private IViewFactoryRegistry viewFactoryRegistry;

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
    }
}
