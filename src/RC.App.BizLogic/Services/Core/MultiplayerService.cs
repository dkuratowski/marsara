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
            this.scenarioLoader = ComponentManager.GetInterface<IScenarioLoader>();
            this.tilesetManager = ComponentManager.GetInterface<ITilesetManagerBC>();
            this.mapLoader = ComponentManager.GetInterface<IMapLoader>();
            this.navmeshLoader = ComponentManager.GetInterface<INavMeshLoader>();
            this.taskManager = ComponentManager.GetInterface<ITaskManagerBC>();
            this.pathFinder = ComponentManager.GetInterface<IPathFinder>();
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
            byte[] mapBytes = File.ReadAllBytes(mapFile);
            MapHeader mapHeader = this.mapLoader.LoadMapHeader(mapBytes);
            IMapAccess map = this.mapLoader.LoadMap(this.tilesetManager.GetTileSet(mapHeader.TilesetName), mapBytes);
            this.pathFinder.Initialize(this.navmeshLoader.LoadNavMesh(mapBytes), MAX_PATHFINDING_ITERATIONS_PER_FRAMES);
            this.gameScenario = this.scenarioLoader.LoadScenario(map, mapBytes);
            this.playerManager = new PlayerManager(this.gameScenario);
            this.playerManager[0].ConnectRandomPlayer(RaceEnum.Terran);
            this.playerManager[1].ConnectRandomPlayer(RaceEnum.Terran);
            this.playerManager[2].ConnectRandomPlayer(RaceEnum.Terran);
            this.playerManager[3].ConnectRandomPlayer(RaceEnum.Terran);
            this.playerManager.Lock();
            this.entitySelector = new EntitySelector(this.gameScenario, this.playerManager[0].Player);
            this.commandDispatcher = new CommandDispatcher();
            this.triggeredScheduler = new TriggeredScheduler(1000 / (int)gameSpeed);
            this.triggeredScheduler.AddScheduledFunction(this.pathFinder.Flush);
            this.triggeredScheduler.AddScheduledFunction(this.ExecuteCommands);
            this.triggeredScheduler.AddScheduledFunction(this.gameScenario.UpdateState);
            this.triggeredScheduler.AddScheduledFunction(this.gameScenario.UpdateAnimations);
            this.testDssTaskCanFinishEvt = new ManualResetEvent(false);
            this.dssTask = this.taskManager.StartTask(this.TestDssTaskMethod, "DssThread");
            this.RegisterFactoryMethods();
        }

        /// <see cref="IMultiplayerService.LeaveCurrentGame"/>
        public void LeaveCurrentGame()
        {
            /// TODO: this is only a PROTOTYPE implementation!
            this.UnregisterFactoryMethods();
            this.dssTask.Finished += this.OnDssTaskFinished;
            this.testDssTaskCanFinishEvt.Set();
        }

        /// <see cref="IMultiplayerService.PostCommand"/>
        public void PostCommand(RCCommand cmd)
        {
            this.commandDispatcher.PushOutgoingCommand(cmd);
        }

        #endregion IMultiplayerService methods

        #region View factory methods

        /// <summary>
        /// Creates a view of type IMapTerrainView.
        /// </summary>
        /// <returns>The created view.</returns>
        private IMapTerrainView CreateMapTerrainView()
        {
            return new MapTerrainView(this.gameScenario.Map);
        }

        /// <summary>
        /// Creates a view of type ITileSetView.
        /// </summary>
        /// <returns>The created view.</returns>
        private ITileSetView CreateTileSetView()
        {
            return new TileSetView(this.gameScenario.Map.Tileset);
        }

        /// <summary>
        /// Creates a view of type IMetadataView.
        /// </summary>
        /// <returns>The created view.</returns>
        private IMetadataView CreateMetadataView()
        {
            return new MetadataView(this.scenarioLoader.Metadata);
        }

        /// <summary>
        /// Creates a view of type IMapObjectView.
        /// </summary>
        /// <returns>The created view.</returns>
        private IMapObjectView CreateMapObjectView()
        {
            return new MapObjectView(this.gameScenario);
        }

        /// <summary>
        /// Creates a view of type ISelectionIndicatorView.
        /// </summary>
        /// <returns>The created view.</returns>
        private ISelectionIndicatorView CreateSelIndicatorView()
        {
            return new SelectionIndicatorView(this.entitySelector);
        }

        /// <summary>
        /// Creates a view of type IMapObjectControlView.
        /// </summary>
        /// <returns>The created view.</returns>
        private IMapObjectControlView CreateMapObjectControlView()
        {
            return new MapObjectControlView(this.gameScenario, this.entitySelector);
        }

        /// <summary>
        /// Registers the implemented factory methods to the view factory.
        /// </summary>
        private void RegisterFactoryMethods()
        {
            this.viewFactoryRegistry.RegisterViewFactory(this.CreateMapTerrainView);
            this.viewFactoryRegistry.RegisterViewFactory(this.CreateTileSetView);
            this.viewFactoryRegistry.RegisterViewFactory(this.CreateMetadataView);
            this.viewFactoryRegistry.RegisterViewFactory(this.CreateMapObjectView);
            this.viewFactoryRegistry.RegisterViewFactory(this.CreateSelIndicatorView);
            this.viewFactoryRegistry.RegisterViewFactory(this.CreateMapObjectControlView);
        }

        /// <summary>
        /// Unregisters the implemented factory methods from the view factory.
        /// </summary>
        private void UnregisterFactoryMethods()
        {
            this.viewFactoryRegistry.UnregisterViewFactory<IMapTerrainView>();
            this.viewFactoryRegistry.UnregisterViewFactory<ITileSetView>();
            this.viewFactoryRegistry.UnregisterViewFactory<IMetadataView>();
            this.viewFactoryRegistry.UnregisterViewFactory<IMapObjectView>();
            this.viewFactoryRegistry.UnregisterViewFactory<ISelectionIndicatorView>();
            this.viewFactoryRegistry.UnregisterViewFactory<IMapObjectControlView>();
        }

        #endregion View factory methods

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
                command.Execute(this.gameScenario);
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

        #endregion Prototype code

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
        /// Reference to the player manager of the active scenario.
        /// </summary>
        private PlayerManager playerManager;

        /// <summary>
        /// Reference to the RC.Engine.Simulator.ScenarioLoader component.
        /// </summary>
        private IScenarioLoader scenarioLoader;

        /// <summary>
        /// Reference to the RC.App.BizLogic.TilesetManager business component.
        /// </summary>
        private ITilesetManagerBC tilesetManager;

        /// <summary>
        /// Reference to the RC.Engine.Maps.MapLoader component.
        /// </summary>
        private IMapLoader mapLoader;

        /// <summary>
        /// Reference to the RC.Engine.Maps.NavMeshLoader component.
        /// </summary>
        private INavMeshLoader navmeshLoader;

        /// <summary>
        /// Reference to the task manager business component.
        /// </summary>
        private ITaskManagerBC taskManager;

        /// <summary>
        /// Reference to the RC.Engine.Simulator.PathFinder component.
        /// </summary>
        private IPathFinder pathFinder;

        /// <summary>
        /// Reference to the registry interface of the RC.App.BizLogic.ViewService component.
        /// </summary>
        private IViewFactoryRegistry viewFactoryRegistry;

        /// <summary>
        /// The maximum number of pathfinding iterations per frames.
        /// </summary>
        private const int MAX_PATHFINDING_ITERATIONS_PER_FRAMES = 2500;
    }
}
