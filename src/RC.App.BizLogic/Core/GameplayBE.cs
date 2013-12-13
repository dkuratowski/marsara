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
            this.mapLoader = ComponentManager.GetInterface<IMapLoader>();
            this.tilesetStore = ComponentManager.GetInterface<ITileSetStore>();
            this.scenarioLoader = ComponentManager.GetInterface<IScenarioLoader>();
        }

        /// <see cref="IComponent.Stop"/>
        public void Stop()
        {
            /// Do nothing
        }

        #endregion IComponent methods

        #region IGameplayBE methods

        /// <see cref="IGameplayBE.CreateMapTerrainView"/>
        public IMapTerrainView CreateMapTerrainView()
        {
            return new MapTerrainView(this.testMap);
        }

        /// <see cref="IGameplayBE.CreateMapObjectView"/>
        public IMapObjectView CreateMapObjectView()
        {
            return new MapObjectView(this.testScenario);
            /// TODO: get the map content manager from the loaded scenario !!!
            //return new MapObjectView(this.testMap, new BspMapContentManager<IGameObject>(
            //            new RCNumRectangle(-(RCNumber)1 / (RCNumber)2,
            //                               -(RCNumber)1 / (RCNumber)2,
            //                               this.testMap.CellSize.X,
            //                               this.testMap.CellSize.Y),
            //                               16,
            //                               10));
        }

        /// <see cref="IGameplayBE.CreateTileSetView"/>
        public ITileSetView CreateTileSetView()
        {
            return new TileSetView(this.testMap.Tileset);
        }

        /// <see cref="IGameplayBE.CreateMetadataView"/>
        public IMetadataView CreateMetadataView()
        {
            return new MetadataView(this.scenarioLoader.Metadata);
        }

        /// <see cref="IGameplayBE.CreateTileSetView"/>
        /// PROTOTYPE CODE
        public void UpdateSimulation()
        {
            this.testScenario.StepAnimations();
        }

        /// TODO: Remove this section when no longer necessary *********************************************************
        public void StartTestScenario()
        {
            byte[] mapBytes = File.ReadAllBytes(".\\maps\\testmap2.rcm");
            MapHeader mapHeader = this.mapLoader.LoadMapHeader(mapBytes);
            this.testMap = this.mapLoader.LoadMap(this.tilesetStore.GetTileSet(mapHeader.TilesetName), mapBytes);
            this.testScenario = this.scenarioLoader.LoadScenario(this.testMap, mapBytes);
        }

        private Scenario testScenario;
        private IMapAccess testMap;
        private IMapLoader mapLoader;
        private ITileSetStore tilesetStore;
        /// TODO_END ***************************************************************************************************

        #endregion IGameplayBE methods

        /// <summary>
        /// Reference to the RC.Engine.Simulator.ScenarioLoader component.
        /// </summary>
        private IScenarioLoader scenarioLoader;
    }
}
