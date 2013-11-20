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
    class Simulator : ISimulator, ISimulatorPluginInstall, IComponent, IHeapedObjectFactoryHelper
    {
        /// <summary>
        /// Constructs a Simulator instance.
        /// </summary>
        public Simulator()
        {
            this.map = null;
            this.gameObjects = null;
            this.metadata = null;
            this.heapTypeContainers = new HashSet<Assembly>();
            this.inheritenceTree = new Dictionary<string, IHeapType[]>();
            this.heapManager = null;
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

            /// Collect the heap types defined in the heap type containers
            List<HeapType> heapTypes = new List<HeapType>();
            foreach (Assembly container in this.heapTypeContainers)
            {
                foreach (Type type in container.GetTypes())
                {
                    if (type.IsSubclassOf(typeof(HeapedObject)))
                    {
                        HeapType heapType = this.CreateHeapType(type);
                        this.inheritenceTree.Add(type.Name, null);
                        heapTypes.Add(heapType);
                    }
                }
            }

            /// Create the heap manager out of the found heap types.
            this.heapManager = new HeapManager(heapTypes);

            /// Create the inheritence tree.
            foreach (IHeapType heapType in heapTypes)
            {
                List<IHeapType> inheritencePath = new List<IHeapType>();
                IHeapType currHeapType = heapType;
                inheritencePath.Add(currHeapType);
                while (currHeapType.HasField(Constants.NAME_OF_BASE_TYPE_FIELD))
                {
                    currHeapType = this.heapManager.GetHeapType(currHeapType.GetFieldTypeID(Constants.NAME_OF_BASE_TYPE_FIELD));
                    inheritencePath.Add(currHeapType);
                }
                inheritencePath.Reverse();
                this.inheritenceTree[heapType.Name] = inheritencePath.ToArray();
            }
        }

        /// <see cref="IComponent.Stop"/>
        public void Stop()
        {
            /// Do nothing
        }

        #endregion IComponent methods

        #region ISimulatorPluginInstall methods

        /// <see cref="ISimulatorPluginInstall.RegisterHeapTypeContainer"/>
        public void RegisterHeapTypeContainer(Assembly assembly)
        {
            this.heapTypeContainers.Add(assembly);
        }

        #endregion ISimulatorPluginInstall methods

        #region IHeapedObjectFactoryHelper methods

        /// <see cref="IHeapedObjectFactoryHelper.HeapManager"/>
        public IHeapManager HeapManager { get { return this.heapManager; } }

        /// <see cref="IHeapedObjectFactoryHelper.GetInheritenceHierarchy"/>
        public IHeapType[] GetInheritenceHierarchy(string typeName)
        {
            if (typeName == null) { throw new ArgumentNullException("typeName"); }
            if (!this.inheritenceTree.ContainsKey(typeName)) { throw new InvalidOperationException(string.Format("Heap object type '{0}' doesn't exist!", typeName)); }
            return this.inheritenceTree[typeName];
        }

        #endregion IHeapedObjectFactoryHelper methods

        #region Internal type registration methods

        /// <summary>
        /// Creates HeapType from the given type.
        /// </summary>
        /// <param name="type">A type that represents a subclass of HeapedObject.</param>
        private HeapType CreateHeapType(Type type)
        {
            List<KeyValuePair<string, string>> fields = new List<KeyValuePair<string, string>>();
            foreach (FieldInfo field in type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                /// Check if the current field is a heaped field or not.
                if (!field.FieldType.IsGenericType ||
                    field.FieldType.IsGenericTypeDefinition ||
                    field.FieldType.ContainsGenericParameters)
                {
                    /// Not a heaped field -> don't care
                    continue;
                }

                /// Get the .NET type of the data behind the field.
                Type[] valueType = field.FieldType.GetGenericArguments();
                if (valueType.Length != 1) { throw new SimulatorException("Every heaped member of a HeapedObject must have exactly 1 type parameter!"); }

                /// Construct the heap type name based on the .NET type.
                if (field.FieldType.GetGenericTypeDefinition() == typeof(HeapedValue<>))
                {
                    /// Single value...
                    fields.Add(new KeyValuePair<string, string>(field.Name, this.GetFieldType(valueType[0])));
                }
                else if (field.FieldType.GetGenericTypeDefinition() == typeof(HeapedArray<>))
                {
                    /// Array...
                    fields.Add(new KeyValuePair<string, string>(field.Name, string.Format("{0}*", this.GetFieldType(valueType[0]))));
                }
            }

            /// Add an additional field for storing base class data if the type doesn't derive from HeapedObject directly.
            if (type.BaseType != typeof(HeapedObject))
            {
                fields.Add(new KeyValuePair<string, string>(Constants.NAME_OF_BASE_TYPE_FIELD, type.BaseType.Name));
            }
            return new HeapType(type.Name, fields);
        }

        /// <summary>
        /// Gets the heap type name of a field.
        /// </summary>
        /// <param name="fieldType">The .NET type of the field.</param>
        /// <returns>The heap type name of the field.</returns>
        private string GetFieldType(Type fieldType)
        {
            BuiltInTypeEnum builtInType = this.GetBuiltInType(fieldType);
            if (builtInType != BuiltInTypeEnum.NonBuiltIn)
            {
                string heapTypeName;
                if (!EnumMap<BuiltInTypeEnum, string>.Map(builtInType, out heapTypeName))
                {
                    throw new SimulatorException(string.Format("Invalid field type: {0}!", builtInType.ToString()));
                }
                return heapTypeName;
            }
            else
            {
                if (!fieldType.IsSubclassOf(typeof(HeapedObject))) { throw new SimulatorException(string.Format("Invalid reference to type '{0}'!", fieldType.Name)); }
                return string.Format("{0}*", fieldType.Name);
            }
        }

        /// <summary>
        /// Gets the built-in type of the given .NET type or HeapType.BuiltInTypeEnum.NonBuiltIn if the given
        /// .NET type is not a built-in type.
        /// </summary>
        /// <param name="dotnetType">The .NET type to be mapped.</param>
        /// <returns>
        /// The built-in type of the given .NET type or HeapType.BuiltInTypeEnum.NonBuiltIn if the given .NET
        /// type is not a built-in type.
        /// </returns>
        private BuiltInTypeEnum GetBuiltInType(Type dotnetType)
        {
            if (dotnetType == typeof(byte)) { return BuiltInTypeEnum.Byte; }
            else if (dotnetType == typeof(short)) { return BuiltInTypeEnum.Short; }
            else if (dotnetType == typeof(int)) { return BuiltInTypeEnum.Integer; }
            else if (dotnetType == typeof(long)) { return BuiltInTypeEnum.Long; }
            else if (dotnetType == typeof(RCNumber)) { return BuiltInTypeEnum.Number; }
            else if (dotnetType == typeof(RCIntVector)) { return BuiltInTypeEnum.IntVector; }
            else if (dotnetType == typeof(RCNumVector)) { return BuiltInTypeEnum.NumVector; }
            else if (dotnetType == typeof(RCIntRectangle)) { return BuiltInTypeEnum.IntRectangle; }
            else if (dotnetType == typeof(RCNumRectangle)) { return BuiltInTypeEnum.NumRectangle; }
            else { return BuiltInTypeEnum.NonBuiltIn; }
        }

        #endregion Internal type registration methods

        /// <summary>
        /// List of the assemblies that contains the heap types.
        /// </summary>
        private HashSet<Assembly> heapTypeContainers;

        /// <summary>
        /// The inheritence path of all registered types mapped by their names starting from the base type.
        /// </summary>
        private Dictionary<string, IHeapType[]> inheritenceTree;

        /// <summary>
        /// Reference to the heap manager.
        /// </summary>
        private IHeapManager heapManager;

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
