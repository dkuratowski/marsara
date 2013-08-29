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
using RC.App.BizLogic.InternalInterfaces;

namespace RC.App.BizLogic.Core
{
    /// <summary>
    /// The implementation of the gameplay backend component.
    /// </summary>
    [Component("RC.App.BizLogic.GameplayBE")]
    class GameplayBE : IGameplayBE
    {
        /// <summary>
        /// Constructs a GameplayBE instance.
        /// </summary>
        public GameplayBE()
        {
        }

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

        /// <see cref="IGameplayBE.CreateTileSetView"/>
        public ITileSetView CreateTileSetView()
        {
            return new TileSetView(this.scenarioSimulator.Map.Tileset);
        }

        /// TODO: Remove this section when no longer necessary *********************************************************
        public void StartTestScenario()
        {
            byte[] mapBytes = File.ReadAllBytes(".\\maps\\testmap.rcm");
            MapHeader mapHeader = this.mapLoader.LoadMapHeader(mapBytes);
            IMapAccess testMap = this.mapLoader.LoadMap(this.tilesetStore.GetTileSet(mapHeader.TilesetName), mapBytes);
            this.scenarioSimulator.BeginScenario(testMap);
        }

        [ComponentReference]
        private IMapLoader mapLoader;
        [ComponentReference]
        private ITileSetStore tilesetStore;
        /// TODO_END ***************************************************************************************************

        #endregion IGameplayBE methods

        /// <summary>
        /// Reference to the RC.Engine.Simulator.ScenarioSimulator component.
        /// </summary>
        [ComponentReference]
        private IScenarioSimulator scenarioSimulator;
    }
}
