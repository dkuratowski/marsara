using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using RC.App.BizLogic.ComponentInterfaces;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Maps.ComponentInterfaces;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Scenarios;
using RC.App.BizLogic.PublicInterfaces;

namespace RC.App.BizLogic.Core
{
    [Component("RC.App.BizLogic.MultiplayerGameManager")]
    class MultiplayerGameManager : IMultiplayerGameManager, IComponent
    {
        /// <summary>
        /// Constructs a MultiplayerGameManager instance.
        /// </summary>
        public MultiplayerGameManager()
        {
            this.dssTask = null;
        }

        #region IComponent methods

        /// <see cref="IComponent.Start"/>
        public void Start()
        {
            this.scenarioLoader = ComponentManager.GetInterface<IScenarioLoader>();
            this.tilesetStore = ComponentManager.GetInterface<ITileSetStore>();
            this.mapLoader = ComponentManager.GetInterface<IMapLoader>();
            this.taskManager = ComponentManager.GetInterface<ITaskManager>();
            this.pathFinder = ComponentManager.GetInterface<IPathFinder>();
        }

        /// <see cref="IComponent.Stop"/>
        public void Stop()
        {
        }

        #endregion IComponent methods

        #region IMultiplayerGameManager methods

        /// <see cref="IMultiplayerGameManager.CreateNewGame"/>
        public IMultiplayerOperation CreateNewGame(string mapFile, GameTypeEnum gameType, GameSpeedEnum gameSpeed)
        {
            byte[] mapBytes = File.ReadAllBytes(mapFile);
            MapHeader mapHeader = this.mapLoader.LoadMapHeader(mapBytes);
            IMapAccess map = this.mapLoader.LoadMap(this.tilesetStore.GetTileSet(mapHeader.TilesetName), mapBytes);
            this.pathFinder.Initialize(map, 5000); /// TODO: call this from a background task!
            this.gameScenario = this.scenarioLoader.LoadScenario(map, mapBytes);
            StartLocation startLocation = this.gameScenario.GetVisibleEntities<StartLocation>()[0];
            this.gameScenario.CreatePlayer(0, startLocation, RaceEnum.Terran);
            this.gameScenario.FinalizePlayers();
            this.entitySelector = new EntitySelector(this.gameScenario, 0);
            this.commandDispatcher = new CommandDispatcher();
            this.triggeredScheduler = new TriggeredScheduler(1000 / (int)gameSpeed);
            this.triggeredScheduler.AddScheduledFunction(this.ExecuteFrame);
            this.triggeredScheduler.AddScheduledFunction(this.gameScenario.StepAnimations);
            this.testDssTaskCanFinishEvt = new ManualResetEvent(false);
            this.dssTask = this.taskManager.StartTask(this.TestDssTaskMethod, "DssThread");
            return null; /// TODO: this is only a PROTOTYPE implementation!
        }

        /// <see cref="IMultiplayerGameManager.JoinExistingGame"/>
        public IMultiplayerOperation JoinExistingGame(Guid gameID)
        {
            throw new NotImplementedException();
        }

        /// <see cref="IMultiplayerGameManager.GetAvailableGames"/>
        public List<GameInfo> GetAvailableGames()
        {
            throw new NotImplementedException();
        }

        /// <see cref="IMultiplayerGameManager.LeaveCurrentGame"/>
        public IMultiplayerOperation LeaveCurrentGame()
        {
            this.dssTask.Finished += this.OnDssTaskFinished;
            this.testDssTaskCanFinishEvt.Set();
            return null; /// TODO: this is only a PROTOTYPE implementation!
        }

        /// <see cref="IMultiplayerGameManager.PostCommand"/>
        public void PostCommand(RCCommand cmd)
        {
            this.commandDispatcher.PushOutgoingCommand(cmd);
        }

        /// <see cref="IMultiplayerGameManager.GameScenario"/>
        public Scenario GameScenario
        {
            get { return this.gameScenario; }
        }

        /// <see cref="IMultiplayerGameManager.Selector"/>
        public EntitySelector Selector
        {
            get { return this.entitySelector; }
        }

        /// <see cref="IMultiplayerGameManager.GameScenario"/>
        public IMultiplayerOperation StartCurrentGame()
        {
            throw new NotImplementedException();
        }

        /// <see cref="IMultiplayerGameManager.GameScenario"/>
        public event GameCountdownHdl GameCountdown;

        #endregion IMultiplayerGameManager methods

        /// <summary>
        /// Internal function that actually executes the next simulation frame.
        /// </summary>
        private void ExecuteFrame()
        {
            this.commandDispatcher.DispatchOutgoingCommands();
            List<RCCommand> incomingCommands = this.commandDispatcher.GetIncomingCommands();
            foreach (RCCommand command in incomingCommands)
            {
                command.Execute();
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
            this.testDssTaskCanFinishEvt.Close();
            this.testDssTaskCanFinishEvt = null;
            this.triggeredScheduler.Dispose();
            this.dssTask = null;
            this.triggeredScheduler = null;
            this.commandDispatcher = null;
            this.gameScenario.Map.Close();
            this.gameScenario = null;
        }

        /// PROTOTYPE CODE
        private TriggeredScheduler triggeredScheduler;
        private CommandDispatcher commandDispatcher;
        private ManualResetEvent testDssTaskCanFinishEvt;

        /// <summary>
        /// Reference to the task that executes the DSS-thread.
        /// </summary>
        private ITask dssTask;

        /// <summary>
        /// Reference to the active scenario.
        /// </summary>
        private Scenario gameScenario;

        /// <summary>
        /// Reference to the entity selector of the local player.
        /// </summary>
        private EntitySelector entitySelector;

        /// <summary>
        /// Reference to the RC.Engine.Simulator.ScenarioLoader component.
        /// </summary>
        private IScenarioLoader scenarioLoader;

        /// <summary>
        /// Reference to the RC.App.BizLogic.TileSetStore component.
        /// </summary>
        private ITileSetStore tilesetStore;

        /// <summary>
        /// Reference to the RC.Engine.Maps.MapLoader component.
        /// </summary>
        private IMapLoader mapLoader;

        /// <summary>
        /// Reference to the RC.App.PresLogic.TaskManagerAdapter component.
        /// </summary>
        private ITaskManager taskManager;

        /// <summary>
        /// Reference to the RC.Engine.Simulator.PathFinder component.
        /// </summary>
        private IPathFinder pathFinder;
    }
}
