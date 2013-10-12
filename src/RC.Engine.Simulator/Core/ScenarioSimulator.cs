﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Maps.Core;
using RC.Common;
using System.IO;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Implementation of the scenario simulator component.
    /// </summary>
    [Component("RC.Engine.Simulator.ScenarioSimulator")]
    class ScenarioSimulator : IScenarioSimulator, IComponentStart
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

            this.map = map;
            this.gameObjects = new BspMapContentManager<IGameObject>(
                        new RCNumRectangle(-(RCNumber)1 / (RCNumber)2,
                                           -(RCNumber)1 / (RCNumber)2,
                                           map.CellSize.X,
                                           map.CellSize.Y),
                                           Constants.BSP_NODE_CAPACITY,
                                           Constants.BSP_MIN_NODE_SIZE);
            this.simulationElements = new Dictionary<int, ISimulationElement>();
            this.simulationElementList = new List<ISimulationElement>();
        }

        /// <see cref="IScenarioSimulator.BeginScenario"/>
        public IMapAccess EndScenario()
        {
            if (this.map == null) { throw new InvalidOperationException("There is no scenario currently being simulated!"); }

            IMapAccess map = this.map;
            this.map = null;
            this.gameObjects = null;
            this.simulationElements = null;
            this.simulationElementList = null;
            return map;
        }

        /// <see cref="IScenarioSimulator.SimulateNextFrame"/>
        public void SimulateNextFrame()
        {
            if (this.map == null) { throw new InvalidOperationException("There is no scenario currently being simulated!"); }

            foreach (ISimulationElement simElement in this.simulationElementList)
            {
                simElement.SimulateNextFrame();
            }
        }

        /// <see cref="IScenarioSimulator.GetElementByUID"/>
        public ISimulationElement GetElementByUID(int uid)
        {
            if (this.map == null) { throw new InvalidOperationException("There is no scenario currently being simulated!"); }

            return this.simulationElements.ContainsKey(uid) ? this.simulationElements[uid] : null;
        }

        /// <see cref="IScenarioSimulator.Map"/>
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
                if (this.map == null) { throw new InvalidOperationException("There is no scenario currently being simulated!"); }
                return this.gameObjects;
            }
        }

        #endregion IScenarioSimulator methods

        #region IComponentStart methods

        /// <see cref="IComponentStart.Start"/>
        public void Start()
        {
            /// Load the tilesets from the configured directory
            DirectoryInfo rootDir = new DirectoryInfo(Constants.METADATA_DIR);
            FileInfo[] metadataFiles = rootDir.GetFiles("*.xml", SearchOption.AllDirectories);
            this.metadata = new SimulationMetadata();
            foreach (FileInfo metadataFile in metadataFiles)
            {
                /// TODO: this is a hack! Later we will have binary metadata format.
                string xmlStr = File.ReadAllText(metadataFile.FullName);
                string imageDir = metadataFile.DirectoryName;
                XmlMetadataReader.Read(xmlStr, imageDir, this.metadata);
            }
            this.metadata.CheckAndFinalize();

            this.simulationHeapMgr = new SimulationHeapMgr(this.metadata.CompositeTypes);
        }

        #endregion IComponentStart methods

        /// <summary>
        /// Reference to the map of the scenario currently being simulated.
        /// </summary>
        private IMapAccess map;

        /// <summary>
        /// Reference to the map content manager that contains the game objects of the scenario currently being simulated.
        /// </summary>
        private IMapContentManager<IGameObject> gameObjects;

        /// <summary>
        /// List of the simulation elements mapped by their UIDs.
        /// </summary>
        private Dictionary<int, ISimulationElement> simulationElements;

        /// <summary>
        /// List of the simulation elements in order of simulation.
        /// </summary>
        private List<ISimulationElement> simulationElementList;

        /// <summary>
        /// Reference to the simulation metadata.
        /// </summary>
        private SimulationMetadata metadata;

        /// <summary>
        /// Reference to the simulation heap manager.
        /// </summary>
        private ISimulationHeapMgr simulationHeapMgr;
    }
}
