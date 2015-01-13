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
    class ScenarioLoader : IScenarioLoader, IPlayerInitializer, IScenarioLoaderPluginInstall, IComponent
    {
        /// <summary>
        /// Constructs a ScenarioLoader instance.
        /// </summary>
        public ScenarioLoader()
        {
            this.metadata = null;
            this.entityConstraints = new Dictionary<string, List<EntityConstraint>>();
            this.playerInitializers = new Dictionary<RaceEnum, Action<Player>>();

            this.RegisterEntityConstraint(MineralField.MINERALFIELD_TYPE_NAME, new BuildableAreaConstraint());
            this.RegisterEntityConstraint(MineralField.MINERALFIELD_TYPE_NAME, new MinimumDistanceConstraint<StartLocation>(new RCIntVector(3, 3)));
            this.RegisterEntityConstraint(MineralField.MINERALFIELD_TYPE_NAME, new MinimumDistanceConstraint<Entity>(new RCIntVector(0, 0)));
            this.RegisterEntityConstraint(VespeneGeyser.VESPENEGEYSER_TYPE_NAME, new BuildableAreaConstraint());
            this.RegisterEntityConstraint(VespeneGeyser.VESPENEGEYSER_TYPE_NAME, new MinimumDistanceConstraint<StartLocation>(new RCIntVector(3, 3)));
            this.RegisterEntityConstraint(VespeneGeyser.VESPENEGEYSER_TYPE_NAME, new MinimumDistanceConstraint<Entity>(new RCIntVector(0, 0)));
            this.RegisterEntityConstraint(StartLocation.STARTLOCATION_TYPE_NAME, new BuildableAreaConstraint());
            this.RegisterEntityConstraint(StartLocation.STARTLOCATION_TYPE_NAME, new MinimumDistanceConstraint<ResourceObject>(new RCIntVector(3, 3)));
            this.RegisterEntityConstraint(StartLocation.STARTLOCATION_TYPE_NAME, new MinimumDistanceConstraint<StartLocation>(new RCIntVector(0, 0)));
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
            Scenario scenario = new Scenario(map);
            while (offset < data.Length)
            {
                int parsedBytes;
                RCPackage package = RCPackage.Parse(data, offset, data.Length - offset, out parsedBytes);
                if (package == null || !package.IsCommitted) { throw new SimulatorException("Syntax error!"); }
                offset += parsedBytes;
                if (package.PackageFormat.ID == ScenarioFileFormat.MINERAL_FIELD)
                {
                    IQuadTile quadTile = map.GetQuadTile(new RCIntVector(package.ReadShort(0), package.ReadShort(1)));
                    MineralField mineralField = new MineralField();
                    mineralField.ResourceAmount.Write(package.ReadInt(2));
                    scenario.AddEntityToScenario(mineralField);
                    scenario.AttachEntityToMap(mineralField, quadTile);
                }
                else if (package.PackageFormat.ID == ScenarioFileFormat.VESPENE_GEYSER)
                {
                    IQuadTile quadTile = map.GetQuadTile(new RCIntVector(package.ReadShort(0), package.ReadShort(1)));
                    VespeneGeyser vespeneGeyser = new VespeneGeyser();
                    vespeneGeyser.ResourceAmount.Write(package.ReadInt(2));
                    scenario.AddEntityToScenario(vespeneGeyser);
                    scenario.AttachEntityToMap(vespeneGeyser, quadTile);
                }
                else if (package.PackageFormat.ID == ScenarioFileFormat.START_LOCATION)
                {
                    StartLocation startLocation = new StartLocation(package.ReadByte(2));
                    scenario.AddEntityToScenario(startLocation);
                    scenario.AttachEntityToMap(startLocation, scenario.Map.GetQuadTile(new RCIntVector(package.ReadShort(0), package.ReadShort(1))));
                }
            }

            /// Check the constraints of the visible entities.
            foreach (Entity entity in scenario.GetEntitiesOnMap<Entity>())
            {
                QuadEntity quadEntity = entity as QuadEntity;
                if (quadEntity != null)
                {
                    scenario.DetachEntityFromMap(quadEntity);
                    if (quadEntity.ElementType.CheckConstraints(scenario, quadEntity.LastKnownQuadCoords).Count != 0) { throw new MapException(string.Format("Entity at {0} is voilating its placement constraints!", quadEntity.LastKnownQuadCoords)); }
                    scenario.AttachEntityToMap(quadEntity, scenario.Map.GetQuadTile(quadEntity.LastKnownQuadCoords));
                }
            }
            return scenario;
        }

        /// <see cref="IScenarioLoader.SaveScenario"/>
        public byte[] SaveScenario(Scenario scenario)
        {
            if (scenario == null) { throw new ArgumentNullException("scenario"); }

            /// Create the packages that describes the entities of the scenario.
            List<RCPackage> entityPackages = new List<RCPackage>();
            int retArrayLength = 0;
            foreach (MineralField mineralField in scenario.GetAllEntities<MineralField>())
            {
                RCPackage mineralFieldPackage = RCPackage.CreateCustomDataPackage(ScenarioFileFormat.MINERAL_FIELD);
                mineralFieldPackage.WriteShort(0, (short)mineralField.LastKnownQuadCoords.X);
                mineralFieldPackage.WriteShort(1, (short)mineralField.LastKnownQuadCoords.Y);
                mineralFieldPackage.WriteInt(2, mineralField.ResourceAmount.Read());
                entityPackages.Add(mineralFieldPackage);
                retArrayLength += mineralFieldPackage.PackageLength;
            }
            foreach (VespeneGeyser vespeneGeyser in scenario.GetAllEntities<VespeneGeyser>())
            {
                RCPackage vespeneGeyserPackage = RCPackage.CreateCustomDataPackage(ScenarioFileFormat.VESPENE_GEYSER);
                vespeneGeyserPackage.WriteShort(0, (short)vespeneGeyser.LastKnownQuadCoords.X);
                vespeneGeyserPackage.WriteShort(1, (short)vespeneGeyser.LastKnownQuadCoords.Y);
                vespeneGeyserPackage.WriteInt(2, vespeneGeyser.ResourceAmount.Read());
                entityPackages.Add(vespeneGeyserPackage);
                retArrayLength += vespeneGeyserPackage.PackageLength;
            }
            foreach (StartLocation startLocation in scenario.GetAllEntities<StartLocation>())
            {
                RCPackage startLocationPackage = RCPackage.CreateCustomDataPackage(ScenarioFileFormat.START_LOCATION);
                startLocationPackage.WriteShort(0, (short)startLocation.LastKnownQuadCoords.X);
                startLocationPackage.WriteShort(1, (short)startLocation.LastKnownQuadCoords.Y);
                startLocationPackage.WriteByte(2, (byte)startLocation.PlayerIndex);
                entityPackages.Add(startLocationPackage);
                retArrayLength += startLocationPackage.PackageLength;
            }

            /// Write the packages into the returned byte array.
            byte[] retArray = new byte[retArrayLength];
            int offset = 0;
            foreach (RCPackage package in entityPackages) { offset += package.WritePackageToBuffer(retArray, offset); }

            return retArray;
        }

        /// <see cref="IScenarioLoader.ScenarioMetadata"/>
        public IScenarioMetadata Metadata { get { return this.metadata; } }

        #endregion IScenarioLoader methods

        #region IPlayerInitializer methods

        /// <see cref="IPlayerInitializer.Initialize"/>
        public void Initialize(Player player, RaceEnum race)
        {
            if (player == null) { throw new ArgumentNullException("player"); }
            if (!this.playerInitializers.ContainsKey(race)) { throw new SimulatorException(string.Format("Player initializer not found for race '{0}'!", race)); }
            this.playerInitializers[race](player);
        }

        #endregion IPlayerInitializer methods

        #region IScenarioLoaderPluginInstall methods

        /// <see cref="IScenarioLoaderPluginInstall.RegisterEntityConstraint"/>
        public void RegisterEntityConstraint(string entityType, EntityConstraint constraint)
        {
            if (entityType == null) { throw new ArgumentNullException("entityType"); }
            if (!this.entityConstraints.ContainsKey(entityType)) { this.entityConstraints.Add(entityType, new List<EntityConstraint>()); }
            this.entityConstraints[entityType].Add(constraint);
        }

        /// <see cref="IScenarioLoaderPluginInstall.RegisterPlayerInitializer"/>
        public void RegisterPlayerInitializer(RaceEnum race, Action<Player> initializer)
        {
            if (initializer == null) { throw new ArgumentNullException("initializer"); }
            if (this.playerInitializers.ContainsKey(race)) { throw new InvalidOperationException(string.Format("Player initializer has already been registered for race '{0}'!", race)); }
            this.playerInitializers[race] = initializer;
        }

        #endregion IScenarioLoaderPluginInstall methods

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

            /// Register the entity constraints to the corresponding entity types.
            foreach (KeyValuePair<string, List<EntityConstraint>> item in this.entityConstraints)
            {
                ScenarioElementType elementType = this.metadata.GetElementTypeImpl(item.Key);
                foreach (EntityConstraint constraint in item.Value)
                {
                    elementType.AddPlacementConstraint(constraint);
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
        /// List of the registered entity constraints mapped by the names of the corresponding entity types.
        /// </summary>
        private Dictionary<string, List<EntityConstraint>> entityConstraints;

        /// <summary>
        /// List of the registered player initializers mapped by the corresponding races.
        /// </summary>
        private Dictionary<RaceEnum, Action<Player>> playerInitializers;

        /// <summary>
        /// Reference to the simulation metadata.
        /// </summary>
        private ScenarioMetadata metadata;
    }
}
