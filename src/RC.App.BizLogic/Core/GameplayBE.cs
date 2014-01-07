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
            return new MapTerrainView(this.multiplayerGameManager.GameScenario.Map);
        }

        /// <see cref="IGameplayBE.CreateMapObjectView"/>
        public IMapObjectView CreateMapObjectView()
        {
            return new MapObjectView(this.multiplayerGameManager.GameScenario);
        }

        /// <see cref="IGameplayBE.CreateMapObjectControlView"/>
        public IMapObjectControlView CreateMapObjectControlView()
        {
            return new MapObjectControlView(this.multiplayerGameManager.GameScenario);
        }

        /// <see cref="IGameplayBE.CreateTileSetView"/>
        public ITileSetView CreateTileSetView()
        {
            return new TileSetView(this.multiplayerGameManager.GameScenario.Map.Tileset);
        }

        /// <see cref="IGameplayBE.CreateMetadataView"/>
        public IMetadataView CreateMetadataView()
        {
            return new MetadataView(this.scenarioLoader.Metadata);
        }

        /// TODO: Remove this section when no longer necessary *********************************************************
        public void StartTestScenario()
        {
            this.multiplayerGameManager.CreateNewGame(".\\maps\\testmap3.rcm", GameTypeEnum.Melee, GameSpeedEnum.Fastest);
        }

        public void StopTestScenario()
        {
            this.multiplayerGameManager.LeaveCurrentGame();
        }

        /// TODO_END ***************************************************************************************************

        #endregion IGameplayBE methods

        /// <summary>
        /// Reference to the RC.Engine.Simulator.ScenarioLoader component.
        /// </summary>
        private IScenarioLoader scenarioLoader;

        /// <summary>
        /// Reference to the RC.App.BizLogic.MultiplayerGameManager component.
        /// </summary>
        private IMultiplayerGameManager multiplayerGameManager;
    }
}
