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
using System.IO;
using System.Reflection;
using RC.Engine.Simulator.InternalInterfaces;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Implementation of the simulator component.
    /// </summary>
    [Component("RC.Engine.Simulator.Simulator")]
    class Simulator : ISimulator, IComponent
    {
        /// <summary>
        /// Constructs a Simulator instance.
        /// </summary>
        public Simulator()
        {
            this.map = null;
            this.gameObjects = null;
            this.metadata = null;
        }

        #region ISimulator methods

        /// <see cref="ISimulator.BeginScenario"/>
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
        }

        /// <see cref="ISimulator.BeginScenario"/>
        public IMapAccess EndScenario()
        {
            if (this.map == null) { throw new InvalidOperationException("There is no scenario currently being simulated!"); }

            IMapAccess map = this.map;
            this.map = null;
            this.gameObjects = null;
            return map;
        }

        /// <see cref="ISimulator.SimulateNextFrame"/>
        public void SimulateNextFrame()
        {
            if (this.map == null) { throw new InvalidOperationException("There is no scenario currently being simulated!"); }

            /// TODO: write frame simulation code here!
        }

        /// <see cref="ISimulator.Map"/>
        public IMapAccess Map
        {
            get
            {
                if (this.map == null) { throw new InvalidOperationException("There is no scenario currently being simulated!"); }
                return this.map;
            }
        }

        /// <see cref="ISimulator.GameObjects"/>
        public IMapContentManager<IGameObject> GameObjects
        {
            get
            {
                if (this.map == null) { throw new InvalidOperationException("There is no scenario currently being simulated!"); }
                return this.gameObjects;
            }
        }

        #endregion ISimulator methods

        #region IComponent methods

        /// <see cref="IComponent.Start"/>
        public void Start()
        {
            /// Load the simulation metadata files from the configured directory
            DirectoryInfo rootDir = new DirectoryInfo(Constants.METADATA_DIR);
            this.metadata = new SimMetadata();
            if (rootDir.Exists)
            {
                FileInfo[] metadataFiles = rootDir.GetFiles("*.xml", SearchOption.AllDirectories);
                foreach (FileInfo metadataFile in metadataFiles)
                {
                    /// TODO: this is a hack! Later we will have binary metadata format.
                    string xmlStr = File.ReadAllText(metadataFile.FullName);
                    string imageDir = metadataFile.DirectoryName;
                    XmlMetadataReader.Read(xmlStr, imageDir, this.metadata);
                }
            }
            this.metadata.CheckAndFinalize();
        }

        /// <see cref="IComponent.Stop"/>
        public void Stop()
        {
            /// Do nothing
        }

        #endregion IComponent methods

        /// <summary>
        /// Reference to the map of the scenario currently being simulated.
        /// </summary>
        private IMapAccess map;

        /// <summary>
        /// Reference to the map content manager that contains the game objects of the scenario currently being simulated.
        /// </summary>
        private IMapContentManager<IGameObject> gameObjects;

        /// <summary>
        /// Reference to the simulation metadata.
        /// </summary>
        private SimMetadata metadata;
    }
}
