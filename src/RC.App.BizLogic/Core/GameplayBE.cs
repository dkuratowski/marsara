﻿using System;
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
            this.scenarioSimulator = ComponentManager.GetInterface<ISimulator>();
            this.pathFinder = ComponentManager.GetInterface<IPathFinder>();
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
            return new MapTerrainView(this.scenarioSimulator.Map);
        }

        /// <see cref="IGameplayBE.CreateMapObjectView"/>
        public IMapObjectView CreateMapObjectView()
        {
            return new MapObjectView(this.scenarioSimulator.Map, this.scenarioSimulator.GameObjects);
        }

        /// <see cref="IGameplayBE.CreateMapDebugView"/>
        public IMapDebugView CreateMapDebugView()
        {
            return new MapDebugView(this.scenarioSimulator.Map, this.pathFinder);
        }

        /// <see cref="IGameplayBE.CreateTileSetView"/>
        public ITileSetView CreateTileSetView()
        {
            return new TileSetView(this.scenarioSimulator.Map.Tileset);
        }

        /// <see cref="IGameplayBE.CreateTileSetView"/>
        /// PROTOTYPE CODE
        public void UpdateSimulation()
        {
            this.scenarioSimulator.SimulateNextFrame();
        }

        /// TODO: Remove this section when no longer necessary *********************************************************
        public void StartTestScenario()
        {
            byte[] mapBytes = File.ReadAllBytes(".\\maps\\testmap.rcm");
            MapHeader mapHeader = this.mapLoader.LoadMapHeader(mapBytes);
            IMapAccess testMap = this.mapLoader.LoadMap(this.tilesetStore.GetTileSet(mapHeader.TilesetName), mapBytes);
            this.scenarioSimulator.BeginScenario(testMap);
        }

        private IMapLoader mapLoader;
        private ITileSetStore tilesetStore;
        /// TODO_END ***************************************************************************************************

        #endregion IGameplayBE methods

        /// <summary>
        /// Reference to the RC.Engine.Simulator.Simulator component.
        /// </summary>
        private ISimulator scenarioSimulator;

        /// <summary>
        /// Reference to the RC.Engine.Simulator.PathFinder component.
        /// </summary>
        private IPathFinder pathFinder;
    }
}