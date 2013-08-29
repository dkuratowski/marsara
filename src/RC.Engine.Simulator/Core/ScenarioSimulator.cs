using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Maps.Core;
using RC.Common;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Implementation of the scenario simulator component.
    /// </summary>
    [Component("RC.Engine.Simulator.ScenarioSimulator")]
    class ScenarioSimulator : IScenarioSimulator
    {
        /// <summary>
        /// Constructs a ScenarioSimulator instance.
        /// </summary>
        public ScenarioSimulator()
        {
            this.map = null;
            this.gameObjects = null;
        }

        #region IScenarioSimulator methods

        /// <see cref="IScenarioSimulator.BeginScenario"/>
        public void BeginScenario(IMapAccess map)
        {
            if (map == null) { throw new ArgumentNullException("map"); }
            if (this.map != null) { throw new InvalidOperationException("Simulation of another scenario is currently running!"); }

            /// TODO: this is a dummy implementation!
            this.map = map;
            this.gameObjects = new BspMapContentManager<IGameObject>(
                        new RCNumRectangle(-(RCNumber)1 / (RCNumber)2,
                                           -(RCNumber)1 / (RCNumber)2,
                                           map.CellSize.X,
                                           map.CellSize.Y),
                                           Constants.BSP_NODE_CAPACITY,
                                           Constants.BSP_MIN_NODE_SIZE);
            this.pathFinder.Initialize(this.map);
            this.CreateTestObjects();
        }

        /// <see cref="IScenarioSimulator.BeginScenario"/>
        public IMapAccess EndScenario()
        {
            if (this.map == null) { throw new InvalidOperationException("There is no scenario currently being simulated!"); }

            /// TODO: this is a dummy implementation!
            IMapAccess map = this.map;
            this.map = null;
            this.gameObjects = null;
            return map;
        }

        /// <see cref="IScenarioSimulator.BeginScenario"/>
        public IMapAccess Map
        {
            get
            {
                if (this.map == null) { throw new InvalidOperationException("There is no scenario currently being simulated!"); }
                return this.map;
            }
        }

        /// <see cref="IScenarioSimulator.GameObjects"/>
        public IMapContentManager<IGameObject> GameObjects
        {
            get
            {
                if (this.gameObjects == null) { throw new InvalidOperationException("There is no scenario currently being simulated!"); }
                return this.gameObjects;
            }
        }

        #endregion IScenarioSimulator methods

        /// **************** PROTOTYPE CODE *******************
        private void CreateTestObjects()
        {
            for (int i = 0; i < Constants.TEST_OBJECT_NUM; i++)
            {
                RCNumVector testObjPos = new RCNumVector(
                    (RCNumber)RandomService.DefaultGenerator.Next(Constants.TEST_OBJECT_MAXCOORD * 1024) / (RCNumber)1024,
                    (RCNumber)RandomService.DefaultGenerator.Next(Constants.TEST_OBJECT_MAXCOORD * 1024) / (RCNumber)1024);
                RCNumVector testObjSize = new RCNumVector(
                    (RCNumber)RandomService.DefaultGenerator.Next(Constants.TEST_OBJECT_MINSIZE * 1024, (Constants.TEST_OBJECT_MAXSIZE + 1) * 1024) / (RCNumber)1024,
                    (RCNumber)RandomService.DefaultGenerator.Next(Constants.TEST_OBJECT_MINSIZE * 1024, (Constants.TEST_OBJECT_MAXSIZE + 1) * 1024) / (RCNumber)1024);
                RCNumRectangle testObjRect = new RCNumRectangle(testObjPos - testObjSize / 2, testObjSize);

                if (gameObjects.GetContents(testObjRect).Count != 0 || this.pathFinder.CheckObstacleIntersection(testObjRect))
                {
                    i--;
                    continue;
                }
                GameObject testObj = new GameObject(testObjRect);
                gameObjects.AttachContent(testObj);
            }
        }
        /// ************ END OF PROTOTYPE CODE ****************

        /// <summary>
        /// Reference to the map of the scenario currently being simulated.
        /// </summary>
        private IMapAccess map;

        /// <summary>
        /// Reference to the map content manager that contains the game objects of the scenario currently being simulated.
        /// </summary>
        private IMapContentManager<IGameObject> gameObjects;

        /// <summary>
        /// Reference to the pathfinder component.
        /// </summary>
        [ComponentReference]
        private IPathFinder pathFinder;
    }
}
