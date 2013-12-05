using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.Core;
using RC.Common;
using System.IO;
using System.Reflection;
using RC.Engine.Simulator.InternalInterfaces;

namespace RC.Engine.Simulator.Scenarios
{
    /// <summary>
    /// Implementation of the ScenarioLoader component.
    /// </summary>
    [Component("RC.Engine.Simulator.ScenarioLoader")]
    class ScenarioLoader : IScenarioLoader, IComponent
    {
        /// <summary>
        /// Constructs a ScenarioLoader instance.
        /// </summary>
        public ScenarioLoader()
        {
            this.metadata = null;
        }

        #region IScenarioLoader methods

        /// <see cref="IScenarioLoader.NewScenario"/>
        public Scenario NewScenario(IMapAccess map)
        {
            if (map == null) { throw new ArgumentNullException("map"); }
            return new Scenario(map);
        }

        /// <see cref="IScenarioLoader.LoadScenario"/>
        public Scenario LoadScenario(IMapAccess map, byte[] data)
        {
            if (map == null) { throw new ArgumentNullException("map"); }
            if (data == null) { throw new ArgumentNullException("data"); }

            /// Load the packages from the byte array.
            int offset = 0;
            Scenario retObj = new Scenario(map);
            while (offset < data.Length)
            {
                int parsedBytes;
                RCPackage package = RCPackage.Parse(data, offset, data.Length - offset, out parsedBytes);
                if (package == null || !package.IsCommitted) { throw new SimulatorException("Syntax error!"); }
                offset += parsedBytes;
                if (package.PackageFormat.ID == ScenarioFileFormat.MINERAL_FIELD)
                {
                    /// TODO: Load the mineral field!
                }
                else if (package.PackageFormat.ID == ScenarioFileFormat.VESPENE_GEYSER)
                {
                    /// TODO: Load the vespene geyser!
                }
                else if (package.PackageFormat.ID == ScenarioFileFormat.START_LOCATION)
                {
                    /// TODO: Load the start location!
                    RCIntVector quadCoords = new RCIntVector(package.ReadShort(0), package.ReadShort(1));
                    int playerIndex = package.ReadByte(2);
                    IScenarioElementType startLocationType = this.metadata.GetElementType(StartLocation.ELEMENT_TYPE_NAME);
                    RCIntVector quadSize = map.CellToQuadSize(startLocationType.Area.Read());
                    RCNumRectangle mapCoords = map.QuadToCellRect(new RCIntRectangle(quadCoords, quadSize));
                    //StartLocation startLocation = new StartLocation(mapCoords, playerIndex);
                }
            }

            return retObj;
        }

        /// <see cref="IScenarioLoader.SaveScenario"/>
        public byte[] SaveScenario(Scenario scenario)
        {
            throw new NotImplementedException();
        }

        /// <see cref="IScenarioLoader.ScenarioMetadata"/>
        public IScenarioMetadata Metadata { get { return this.metadata; } }

        #endregion IScenarioLoader methods

        #region IComponent methods

        /// <see cref="IComponent.Start"/>
        public void Start()
        {
            /// Load the simulation metadata files from the configured directory
            DirectoryInfo rootDir = new DirectoryInfo(Constants.METADATA_DIR);
            this.metadata = new ScenarioMetadata();
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
        /// Reference to the simulation metadata.
        /// </summary>
        private ScenarioMetadata metadata;
    }
}
