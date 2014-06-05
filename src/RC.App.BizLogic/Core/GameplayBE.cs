using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.ComponentModel;
using RC.App.BizLogic.PublicInterfaces;
using RC.Engine.Simulator.ComponentInterfaces;
using System.IO;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Maps.ComponentInterfaces;
using RC.App.BizLogic.ComponentInterfaces;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Maps.Core;
using RC.Common;
using RC.Engine.Simulator.Scenarios;
using System.Threading;

namespace RC.App.BizLogic.Core
{
    /// <summary>
    /// The implementation of the gameplay backend component.
    /// </summary>
    [Component("RC.App.BizLogic.GameplayBE")]
    class GameplayBE : IGameplayBE, IComponent
    {
        /// <summary>
        /// Constructs a GameplayBE instance.
        /// </summary>
        public GameplayBE()
        {
        }

        #region IComponent methods

        /// <see cref="IComponent.Start"/>
        public void Start()
        {
            this.scenarioLoader = ComponentManager.GetInterface<IScenarioLoader>();
            this.multiplayerGameManager = ComponentManager.GetInterface<IMultiplayerGameManager>();
            this.viewFactoryRegistry = ComponentManager.GetInterface<IViewFactoryRegistry>();
        }

        /// <see cref="IComponent.Stop"/>
        public void Stop()
        {
            /// Do nothing
        }

        #endregion IComponent methods

        #region IGameplayBE methods

        /// TODO: Remove this section when no longer necessary *********************************************************
        public void StartTestScenario()
        {
            this.multiplayerGameManager.CreateNewGame(".\\maps\\testmap4.rcm", GameTypeEnum.Melee, GameSpeedEnum.Fastest);
            this.RegisterFactoryMethods();
        }

        public void StopTestScenario()
        {
            this.UnregisterFactoryMethods();
            this.multiplayerGameManager.LeaveCurrentGame();
        }

        /// TODO_END ***************************************************************************************************

        #endregion IGameplayBE methods
        
        #region View factory methods

        /// <summary>
        /// Creates a view of type IMapTerrainView.
        /// </summary>
        /// <returns>The created view.</returns>
        private IMapTerrainView CreateMapTerrainView()
        {
            return new MapTerrainView(this.multiplayerGameManager.GameScenario.Map);
        }

        /// <summary>
        /// Creates a view of type ITileSetView.
        /// </summary>
        /// <returns>The created view.</returns>
        private ITileSetView CreateTileSetView()
        {
            return new TileSetView(this.multiplayerGameManager.GameScenario.Map.Tileset);
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
            return new MapObjectView(this.multiplayerGameManager.GameScenario);
        }

        /// <summary>
        /// Creates a view of type ISelectionIndicatorView.
        /// </summary>
        /// <returns>The created view.</returns>
        private ISelectionIndicatorView CreateSelIndicatorView()
        {
            return new SelectionIndicatorView(this.multiplayerGameManager.Selector);
        }

        /// <summary>
        /// Creates a view of type IMapObjectControlView.
        /// </summary>
        /// <returns>The created view.</returns>
        private IMapObjectControlView CreateMapObjectControlView()
        {
            return new MapObjectControlView(this.multiplayerGameManager.GameScenario, this.multiplayerGameManager.Selector);
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

        /// <summary>
        /// Reference to the RC.Engine.Simulator.ScenarioLoader component.
        /// </summary>
        private IScenarioLoader scenarioLoader;

        /// <summary>
        /// Reference to the RC.App.BizLogic.MultiplayerGameManager component.
        /// </summary>
        private IMultiplayerGameManager multiplayerGameManager;

        /// <summary>
        /// Reference to the registry interface of the RC.App.BizLogic.ViewFactory component.
        /// </summary>
        private IViewFactoryRegistry viewFactoryRegistry;
    }
}
