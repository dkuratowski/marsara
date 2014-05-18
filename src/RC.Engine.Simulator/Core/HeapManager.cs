using System;
using System.Collections.Generic;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.InternalInterfaces;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Common.ComponentModel;
using System.Reflection;
using System.IO;
using RC.Common;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// This class manages the simulation-heap.
    /// </summary>
    [Component("RC.Engine.Simulator.HeapManager")]
    class HeapManager : IHeapManagerInternals, IHeapManagerPluginInstall, IHeapManager, IComponent, IHeapConnectorFactory
    {
        /// <summary>
        /// Constructs a HeapManager object.
        /// </summary>
        public HeapManager()
        {
            this.heap = null;
            this.typeIDs = new Dictionary<string, short>();
            this.types = new List<HeapType>();
            this.heapTypeContainers = new HashSet<Assembly>();
            this.heapTypeContainers.Add(this.GetType().Assembly);
            this.inheritenceTree = new Dictionary<string, IHeapType[]>();

            this.sectionObjectPool = new Queue<HeapSection>();
            this.freeSectionsHead = null;

            this.RegisterBuiltInTypes();
        }

        /// <summary>
        /// Constructs a HeapManager object with the given heap types.
        /// </summary>
        /// <param name="types">The heap types.</param>
        /// <remarks>
        /// WARNING!!! RC.Engine.Simulator.HeapManager is a component and must be initialized by the ComponentManager!
        /// Use this constructor only for testing purposes!!!
        /// </remarks>
        internal HeapManager(IEnumerable<HeapType> types)
            : this()
        {
            this.RegisterNonBuiltInTypes(types);
        }

        #region IHeapManager members

        /// <see cref="IHeapManager.CreateHeap"/>
        public void CreateHeap()
        {
            if (this.heap != null) { throw new InvalidOperationException("Simulation heap already created or loaded!"); }

            /// Create the underlying heap.
            this.heap = new Heap(Constants.SIM_HEAP_PAGESIZE, Constants.SIM_HEAP_CAPACITY);
            this.freeSectionsHead = new HeapSection()
            {
                Address = 4,    /// Reserve the first 4 bytes for internal use.
                Length = -1,    /// Goes on to the end of the heap.
                Next = null,
                Prev = null
            };

            /// Attach the existing HeapedObjects to the heap.
            if (this.AttachingHeapedObjects != null) { this.AttachingHeapedObjects(this, null); }
            if (this.SynchronizingHeapedObjects != null) { this.SynchronizingHeapedObjects(this, null); }
        }

        /// <see cref="IHeapManager.UnloadHeap"/>
        public void UnloadHeap()
        {
            if (this.heap == null) { throw new InvalidOperationException("No simulation heap created or loaded currently!"); }

            /// Detach the existing HeapedObjects from the heap.
            if (this.DetachingHeapedObjects != null) { this.DetachingHeapedObjects(this, null); }

            /// Close the underlying heap.
            this.heap = null;
            this.sectionObjectPool.Clear();
            this.freeSectionsHead = null;
        }

        /// <see cref="IHeapManager.ComputeHash"/>
        public byte[] ComputeHash()
        {
            if (this.heap == null) { throw new InvalidOperationException("No simulation heap created or loaded currently!"); }
            return this.heap.ComputeHash();
        }

        #endregion IHeapManager members

        #region IHeapManagerInternals members

        /// <see cref="IHeapManagerInternals.GetInheritenceHierarchy"/>
        public IHeapType[] GetInheritenceHierarchy(string typeName)
        {
            if (typeName == null) { throw new ArgumentNullException("typeName"); }
            if (!this.inheritenceTree.ContainsKey(typeName)) { throw new InvalidOperationException(string.Format("Heap object type '{0}' doesn't exist!", typeName)); }
            return this.inheritenceTree[typeName];
        }

        /// <see cref="IHeapManagerInternals.IsHeapAttached"/>
        public bool IsHeapAttached { get { return this.heap != null; } }

        /// <see cref="IHeapManagerInternals.AttachingHeapedObjects"/>
        public event EventHandler AttachingHeapedObjects;

        /// <see cref="IHeapManagerInternals.SynchronizingHeapedObjects"/>
        public event EventHandler SynchronizingHeapedObjects;

        /// <see cref="IHeapManagerInternals.DetachingHeapedObjects"/>
        public event EventHandler DetachingHeapedObjects;

        /// <see cref="IHeapManagerInternals.GetHeapType"/>
        public IHeapType GetHeapType(string typeName)
        {
            if (typeName == null) { throw new ArgumentNullException("typeName"); }
            if (!this.typeIDs.ContainsKey(typeName)) { throw new HeapException(string.Format("Unknown type '{0}'!", typeName)); }
            return this.types[this.typeIDs[typeName]];
        }

        /// <see cref="IHeapManagerInternals.GetHeapType"/>
        public IHeapType GetHeapType(short typeID)
        {
            if (typeID < 0 || typeID >= this.types.Count) { throw new ArgumentOutOfRangeException("typeID"); }
            return this.types[typeID];
        }

        /// <see cref="IHeapManagerInternals.New"/>
        public IHeapConnector New(short typeID)
        {
            if (this.heap == null) { throw new InvalidOperationException("No simulation heap created or loaded currently!"); }
            if (typeID < 0 || typeID >= this.types.Count) { throw new ArgumentOutOfRangeException("typeID"); }

            HeapType type = this.types[typeID];
            HeapSection sectToAlloc = this.AllocateSection(type.AllocationSize);
            return this.CreateHeapConnector(sectToAlloc.Address, typeID);
        }

        /// <see cref="IHeapManagerInternals.NewArray"/>
        public IHeapConnector NewArray(short typeID, int count)
        {
            if (this.heap == null) { throw new InvalidOperationException("No simulation heap created or loaded currently!"); }
            if (typeID < 0 || typeID >= this.types.Count) { throw new ArgumentOutOfRangeException("typeID"); }
            if (count <= 0) { throw new ArgumentOutOfRangeException("count"); }

            HeapType type = this.types[typeID];
            HeapSection sectToAlloc = this.AllocateSection(count * type.AllocationSize + 4); /// +4 is for storing the number of array-items.
            this.heap.WriteInt(sectToAlloc.Address, count);
            return this.CreateHeapConnector(sectToAlloc.Address + 4, typeID);
        }

        #endregion IHeapManagerInternals members

        #region IHeapManagerPluginInstall methods

        /// <see cref="IHeapManagerPluginInstall.RegisterHeapTypeContainer"/>
        public void RegisterHeapTypeContainer(Assembly assembly)
        {
            this.heapTypeContainers.Add(assembly);
        }

        #endregion IHeapManagerPluginInstall methods

        #region IComponent methods

        /// <see cref="IComponent.Start"/>
        public void Start()
        {
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

            /// Register the found heap types.
            this.RegisterNonBuiltInTypes(heapTypes);

            /// Create the inheritence tree.
            foreach (IHeapType heapType in heapTypes)
            {
                List<IHeapType> inheritencePath = new List<IHeapType>();
                IHeapType currHeapType = heapType;
                inheritencePath.Add(currHeapType);
                while (currHeapType.HasField(Constants.NAME_OF_BASE_TYPE_FIELD))
                {
                    currHeapType = this.GetHeapType(currHeapType.GetFieldTypeID(Constants.NAME_OF_BASE_TYPE_FIELD));
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

        #region IHeapConnectorFactory members

        /// <see cref="IHeapConnectorFactory.CreateHeapConnector"/>
        public IHeapConnector CreateHeapConnector(int address, short typeID)
        {
            HeapType type = this.types[typeID];
            switch (type.BuiltInType)
            {
                case BuiltInTypeEnum.NonBuiltIn:
                    return new HeapConnector(address, type, this.heap, this, this.DeallocateSection);
                case BuiltInTypeEnum.Byte:
                    return new HeapByteConnector(address, type, this.heap, this, this.DeallocateSection);
                case BuiltInTypeEnum.Short:
                    return new HeapShortConnector(address, type, this.heap, this, this.DeallocateSection);
                case BuiltInTypeEnum.Integer:
                    return new HeapIntConnector(address, type, this.heap, this, this.DeallocateSection);
                case BuiltInTypeEnum.Long:
                    return new HeapLongConnector(address, type, this.heap, this, this.DeallocateSection);
                case BuiltInTypeEnum.Number:
                    return new HeapNumberConnector(address, type, this.heap, this, this.DeallocateSection);
                case BuiltInTypeEnum.IntVector:
                    return new HeapIntVectorConnector(address, type, this.heap, this, this.DeallocateSection);
                case BuiltInTypeEnum.NumVector:
                    return new HeapNumVectorConnector(address, type, this.heap, this, this.DeallocateSection);
                case BuiltInTypeEnum.IntRectangle:
                    return new HeapIntRectangleConnector(address, type, this.heap, this, this.DeallocateSection);
                case BuiltInTypeEnum.NumRectangle:
                    return new HeapNumRectangleConnector(address, type, this.heap, this, this.DeallocateSection);
                default:
                    throw new InvalidOperationException("Impossible case happened!");
            }
        }

        #endregion IHeapConnectorFactory members

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
                /// Check if the type of the current field is a generic type.
                if (!field.FieldType.IsGenericType ||
                    field.FieldType.IsGenericTypeDefinition ||
                    field.FieldType.ContainsGenericParameters)
                {
                    /// Not a generic type -> don't care
                    continue;
                }

                /// Chcek if the field is a heaped field or not.
                if (field.FieldType.GetGenericTypeDefinition() != typeof(HeapedValue<>) &&
                    field.FieldType.GetGenericTypeDefinition() != typeof(HeapedArray<>))
                {
                    /// Not a heaped field -> don't care.
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

        #region Internal allocation management methods

        /// <summary>
        /// Allocates a section with the given size from the normal free sections.
        /// </summary>
        /// <param name="size">The size of the section to allocate.</param>
        /// <returns>The allocated section.</returns>
        private HeapSection AllocateSection(int size)
        {
            HeapSection currFreeSect = this.freeSectionsHead;
            while (currFreeSect.Length != -1 && currFreeSect.Length < size && currFreeSect != null)
            {
                currFreeSect = currFreeSect.Next;
            }

            if (currFreeSect == null) { throw new HeapException("Heap is full!"); }
            
            if (currFreeSect.Length == size)
            {
                /// Size exactly fits, allocate the section
                if (currFreeSect.Next != null && currFreeSect.Prev != null)
                {
                    /// Non-head
                    currFreeSect.Prev.Next = currFreeSect.Next;
                    currFreeSect.Next.Prev = currFreeSect.Prev;
                    currFreeSect.Next = null;
                    currFreeSect.Prev = null;
                }
                else if (currFreeSect.Prev == null && currFreeSect.Next != null)
                {
                    /// Head
                    this.freeSectionsHead = currFreeSect.Next;
                    currFreeSect.Next.Prev = null;
                    currFreeSect.Next = null;
                }
                else
                {
                    throw new HeapException("Unexpected case!");
                }

                return currFreeSect;
            }
            else
            {
                /// Size doesn't fit exactly, need to split the section.
                HeapSection newSection = this.sectionObjectPool.Count != 0
                                          ? this.sectionObjectPool.Dequeue()
                                          : new HeapSection();
                newSection.Address = currFreeSect.Address;
                newSection.Length = size;
                newSection.Next = null;
                newSection.Prev = null;

                currFreeSect.Address = newSection.Address + newSection.Length;
                if (currFreeSect.Length != -1) { currFreeSect.Length -= size; }
                return newSection;
            }
        }

        /// <summary>
        /// Deallocates the given simulation heap section.
        /// </summary>
        /// <param name="address">The start address of the section to be deallocated.</param>
        /// <param name="length">The length of the section to be deallocated.</param>
        private void DeallocateSection(int address, int length)
        {
            HeapSection section = this.sectionObjectPool.Count != 0
                          ? this.sectionObjectPool.Dequeue()
                          : new HeapSection();
            section.Address = address;
            section.Length = length;
            section.Next = null;
            section.Prev = null;

            HeapSection currFreeSect = this.freeSectionsHead;
            while (section.Address > currFreeSect.Address)
            {
                currFreeSect = currFreeSect.Next;
            }

            if (section.Address + section.Length < currFreeSect.Address)
            {
                /// Insert the deallocated section before currFreeSect.
                section.Next = currFreeSect;
                section.Prev = currFreeSect.Prev;
                currFreeSect.Prev = section;
                if (section.Prev != null)
                {
                    section.Prev.Next = section;
                }
                else
                {
                    this.freeSectionsHead = section;
                }
            }
            else if (section.Address + section.Length == currFreeSect.Address)
            {
                /// Merge the deallocated section with currFreeSect.
                currFreeSect.Address -= section.Length;
                if (currFreeSect.Length != -1) { currFreeSect.Length += section.Length; }
                this.sectionObjectPool.Enqueue(section);
                section = currFreeSect;
            }
            else
            {
                throw new HeapException("Unexpected case!");
            }

            if (section.Prev != null)
            {
                if (section.Prev.Address + section.Prev.Length == section.Address)
                {
                    /// Merge the section with section.Prev.
                    HeapSection sectToDel = section.Prev;
                    section.Address -= section.Prev.Length;
                    if (section.Length != -1) { section.Length += section.Prev.Length; }
                    section.Prev = section.Prev.Prev;
                    this.sectionObjectPool.Enqueue(sectToDel);
                    if (section.Prev != null)
                    {
                        section.Prev.Next = section;
                    }
                    else
                    {
                        this.freeSectionsHead = section;
                    }
                }
                else if (section.Prev.Address + section.Prev.Length > section.Address)
                {
                    throw new HeapException("Unexpected case!");
                }
            }
        }

        #endregion Internal allocation management methods

        #region Internal heap type registration methods

        /// <summary>
        /// Internal method for parsing the incoming metadata.
        /// </summary>
        /// <param name="dataTypes">List of the data types.</param>
        private void RegisterNonBuiltInTypes(IEnumerable<HeapType> dataTypes)
        {
            /// Register all the composite types.
            foreach (HeapType dataType in dataTypes)
            {
                this.RegisterType(dataType);
            }

            /// Register all the pointer types.
            List<HeapType> allNonPointerTypes = new List<HeapType>(this.types);
            foreach (HeapType element in allNonPointerTypes)
            {
                element.RegisterPointerTypes(ref this.typeIDs, ref this.types);
            }

            /// Compute the missing allocation sizes and field offsets
            foreach (HeapType element in allNonPointerTypes)
            {
                element.ComputeFieldOffsets(this.types);
            }
        }

        /// <summary>
        /// Internal method for registering the built-in types to this manager.
        /// </summary>
        private void RegisterBuiltInTypes()
        {
            this.RegisterType(new HeapType(BuiltInTypeEnum.Byte));
            this.RegisterType(new HeapType(BuiltInTypeEnum.Short));
            this.RegisterType(new HeapType(BuiltInTypeEnum.Integer));
            this.RegisterType(new HeapType(BuiltInTypeEnum.Long));
            this.RegisterType(new HeapType(BuiltInTypeEnum.Number));
            this.RegisterType(new HeapType(BuiltInTypeEnum.IntVector));
            this.RegisterType(new HeapType(BuiltInTypeEnum.NumVector));
            this.RegisterType(new HeapType(BuiltInTypeEnum.IntRectangle));
            this.RegisterType(new HeapType(BuiltInTypeEnum.NumRectangle));
        }

        /// <summary>
        /// Internal method for registering a new type to this manager.
        /// </summary>
        /// <param name="type">The type to be registered.</param>
        /// <param name="typeIDs">The list of the type IDs mapped by their names.</param>
        /// <param name="types">The list of the types.</param>
        private void RegisterType(HeapType type)
        {
            if (this.typeIDs.ContainsKey(type.Name)) { throw new HeapException(string.Format("Type '{0}' already defined!", type.Name)); }
            if (this.types.Count == short.MaxValue) { throw new HeapException(string.Format("Number of possible types exceeded the limit of {0}!", short.MaxValue)); }

            type.SetID((short)this.types.Count);
            this.typeIDs.Add(type.Name, (short)this.types.Count);
            this.types.Add(type);
        }

        #endregion Internal heap type registration methods

        /// <summary>
        /// List of the assemblies that contains the heap types.
        /// </summary>
        private HashSet<Assembly> heapTypeContainers;

        /// <summary>
        /// The inheritence path of all registered types mapped by their names starting from the base type.
        /// </summary>
        private Dictionary<string, IHeapType[]> inheritenceTree;

        /// <summary>
        /// Reference to the heap.
        /// </summary>
        private IHeap heap;

        /// <summary>
        /// List of the types mapped by their IDs.
        /// </summary>
        private List<HeapType> types;

        /// <summary>
        /// List of the type IDs mapped by their names.
        /// </summary>
        private Dictionary<string, short> typeIDs;

        #region Private fields for allocation management

        /// <summary>
        /// The head of the linked-list that contains the free sections on the heap.
        /// </summary>
        private HeapSection freeSectionsHead;

        /// <summary>
        /// FIFO list of the currently inactive HeapSection objects.
        /// </summary>
        private Queue<HeapSection> sectionObjectPool;

        #endregion Private fields for allocation management
    }
}
