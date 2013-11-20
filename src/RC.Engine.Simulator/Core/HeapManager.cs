using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.InternalInterfaces;
using RC.Common.ComponentModel;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// This class manages the allocations on the simulation-heap.
    /// </summary>
    class HeapManager : IHeapManager, IHeapConnectorFactory
    {
        /// <summary>
        /// Constructs a HeapManager object.
        /// </summary>
        /// <param name="dataTypes">List of the data types.</param>
        public HeapManager(IEnumerable<HeapType> dataTypes)
        {
            if (dataTypes == null) { throw new ArgumentNullException("dataTypes"); }

            this.heap = new Heap(Constants.SIM_HEAP_PAGESIZE, Constants.SIM_HEAP_CAPACITY);
            this.typeIDs = new Dictionary<string, short>();
            this.types = new List<HeapType>();

            this.sectionObjectPool = new Queue<HeapSection>();
            this.freeSectionsHead = new HeapSection()
            {
                Address = 4,    /// Reserve the first 4 bytes for internal use.
                Length = -1,    /// Goes on to the end of the heap.
                Next = null,
                Prev = null
            };

            this.RegisterBuiltInTypes();
            this.RegisterNonBuiltInTypes(dataTypes);
        }

        #region IHeapManager members

        /// <see cref="IHeapManager.GetHeapType"/>
        public IHeapType GetHeapType(string typeName)
        {
            if (typeName == null) { throw new ArgumentNullException("typeName"); }
            if (!this.typeIDs.ContainsKey(typeName)) { throw new HeapException(string.Format("Unknown type '{0}'!", typeName)); }
            return this.types[this.typeIDs[typeName]];
        }

        /// <see cref="IHeapManager.GetHeapType"/>
        public IHeapType GetHeapType(short typeID)
        {
            if (typeID < 0 || typeID >= this.types.Count) { throw new ArgumentOutOfRangeException("typeID"); }
            return this.types[typeID];
        }

        /// <see cref="IHeapManager.New"/>
        public IHeapConnector New(short typeID)
        {
            if (typeID < 0 || typeID >= this.types.Count) { throw new ArgumentOutOfRangeException("typeID"); }

            HeapType type = this.types[typeID];
            HeapSection sectToAlloc = this.AllocateSection(type.AllocationSize);
            return this.CreateHeapConnector(sectToAlloc.Address, typeID);
        }

        /// <see cref="IHeapManager.NewArray"/>
        public IHeapConnector NewArray(short typeID, int count)
        {
            if (typeID < 0 || typeID >= this.types.Count) { throw new ArgumentOutOfRangeException("typeID"); }
            if (count <= 0) { throw new ArgumentOutOfRangeException("count"); }

            HeapType type = this.types[typeID];
            HeapSection sectToAlloc = this.AllocateSection(count * type.AllocationSize + 4); /// +4 is for storing the number of array-items.
            this.heap.WriteInt(sectToAlloc.Address, count);
            return this.CreateHeapConnector(sectToAlloc.Address + 4, typeID);
        }

        /// <see cref="IHeapManager.ComputeHash"/>
        public byte[] ComputeHash()
        {
            return this.heap.ComputeHash();
        }

        /// <see cref="IHeapManager.SaveState"/>
        public byte[] SaveState(List<IHeapConnector> externalRefs)
        {
            if (externalRefs == null) { throw new ArgumentNullException("externalRefs"); }
            
            /// Collect the free section BEFORE starting the save process.
            List<Tuple<int, int>> freeSections = new List<Tuple<int, int>>();
            HeapSection currFreeSect = this.freeSectionsHead;
            do
            {
                freeSections.Add(new Tuple<int, int>(currFreeSect.Address, currFreeSect.Length));
            } while (currFreeSect.Next != null);

            /// Save the linked-list of the external references.
            IHeapType extRefType = this.GetHeapType(EXT_REF_TYPE);
            IHeapConnector extRefListHead = null;
            IHeapConnector extRefListPrev = null;
            foreach (IHeapConnector extRef in externalRefs)
            {
                HeapConnector extRefImpl = (HeapConnector)extRef;

                IHeapConnector extRefSave = this.New(extRefType.ID);
                ((IValueWrite<int>)extRefSave.AccessField(extRefType.GetFieldIdx(EXT_REF_DATAADDRESS))).Write(extRefImpl.DataAddress);
                ((IValueWrite<short>)extRefSave.AccessField(extRefType.GetFieldIdx(EXT_REF_TYPEID))).Write(extRefImpl.DataType.ID);

                /// Set the head if necessary.
                if (extRefListHead == null) { extRefListHead = extRefSave; }

                /// Set the "next" pointer of the previous element if necessary.
                if (extRefListPrev != null) { extRefListPrev.AccessField(extRefType.GetFieldIdx(EXT_REF_NEXT)).PointTo(extRefSave); }

                extRefListPrev = extRefSave;
            }

            /// Save the linked-list of the free sections.
            IHeapType freeSectType = this.GetHeapType(FREE_SECT_TYPE);
            IHeapConnector freeSectListHead = null;
            IHeapConnector freeSectListPrev = null;
            foreach (Tuple<int, int> freeSect in freeSections)
            {
                IHeapConnector freeSectSave = this.New(freeSectType.ID);
                ((IValueWrite<int>)freeSectSave.AccessField(freeSectType.GetFieldIdx(FREE_SECT_ADDRESS))).Write(freeSect.Item1);
                ((IValueWrite<int>)freeSectSave.AccessField(freeSectType.GetFieldIdx(FREE_SECT_LENGTH))).Write(freeSect.Item2);

                /// Set the head if necessary.
                if (freeSectListHead == null) { freeSectListHead = freeSectSave; }

                /// Set the "next" pointer of the previous element if necessary.
                if (freeSectListPrev != null)
                {
                    freeSectListPrev.AccessField(freeSectType.GetFieldIdx(FREE_SECT_NEXT)).PointTo(freeSectSave);
                }

                freeSectListPrev = freeSectSave;
            }

            /// Set the "next" pointer of the last element to null.
            freeSectListPrev.AccessField(freeSectType.GetFieldIdx(FREE_SECT_NEXT)).PointTo(null);

            /// Save the dump-root to the heap.
            IHeapType dumpRootType = this.GetHeapType(DUMP_ROOT_TYPE);
            IHeapConnector dumpRoot = this.New(dumpRootType.ID);
            dumpRoot.AccessField(dumpRootType.GetFieldIdx(DUMP_ROOT_REFLISTHEAD)).PointTo(extRefListHead);
            dumpRoot.AccessField(dumpRootType.GetFieldIdx(DUMP_ROOT_FREESECTHEAD)).PointTo(freeSectListHead);

            /// Save the pointer to the dump-root at 0x00000000.
            this.heap.WriteInt(0, ((HeapConnector)dumpRoot).DataAddress);

            /// Dump the heap contents into a byte array.
            return this.heap.Dump();
        }

        /// <see cref="IHeapManager.LoadState"/>
        public List<IHeapConnector> LoadState(byte[] heapContent)
        {
            if (heapContent == null) { throw new ArgumentNullException("heapContent"); }

            /// Clear the free sections registry.
            this.heap = new Heap(heapContent, Constants.SIM_HEAP_CAPACITY);
            this.sectionObjectPool.Clear();
            this.freeSectionsHead = null;

            /// Load the dump-root from the heap.
            IHeapType dumpRootType = this.GetHeapType(DUMP_ROOT_TYPE);
            IHeapConnector dumpRoot = this.CreateHeapConnector(this.heap.ReadInt(0), dumpRootType.ID);
            this.heap.WriteInt(0, 0);

            /// Load the free sections.
            IHeapType freeSectType = this.GetHeapType(FREE_SECT_TYPE);
            HeapSection prevFreeSection = null;
            IHeapConnector currFreeSectAccess = dumpRoot.AccessField(dumpRootType.GetFieldIdx(DUMP_ROOT_FREESECTHEAD)).Dereference();
            while (currFreeSectAccess != null)
            {
                HeapSection currFreeSection = new HeapSection()
                {
                    Address = ((IValueRead<int>)currFreeSectAccess.AccessField(freeSectType.GetFieldIdx(FREE_SECT_ADDRESS))).Read(),
                    Length = ((IValueRead<int>)currFreeSectAccess.AccessField(freeSectType.GetFieldIdx(FREE_SECT_LENGTH))).Read(),
                    Next = null,
                    Prev = prevFreeSection
                };
                if (this.freeSectionsHead == null) { this.freeSectionsHead = currFreeSection; }
                if (prevFreeSection != null) { prevFreeSection.Next = currFreeSection; }
                prevFreeSection = currFreeSection;
                currFreeSectAccess = currFreeSectAccess.AccessField(freeSectType.GetFieldIdx(FREE_SECT_NEXT)).Dereference();
            }

            /// Load the external references
            IHeapType extRefType = this.GetHeapType(EXT_REF_TYPE);
            List<IHeapConnector> retList = new List<IHeapConnector>();
            IHeapConnector currExtRefAccess = dumpRoot.AccessField(dumpRootType.GetFieldIdx(DUMP_ROOT_REFLISTHEAD)).Dereference();
            while (currExtRefAccess != null)
            {
                retList.Add(
                    this.CreateHeapConnector(((IValueRead<int>)currExtRefAccess.AccessField(extRefType.GetFieldIdx(EXT_REF_DATAADDRESS))).Read(),
                    ((IValueRead<short>)currExtRefAccess.AccessField(extRefType.GetFieldIdx(EXT_REF_TYPEID))).Read()));
                currExtRefAccess = currExtRefAccess.AccessField(extRefType.GetFieldIdx(EXT_REF_NEXT)).Dereference();
            }
            return retList;
        }

        #endregion IHeapManager members

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

        #region Internal metadata parsing methods

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

            /// Register the internal types.
            List<KeyValuePair<string, string>> dumpRootFields = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>(DUMP_ROOT_PAGESIZE, "int"),
                new KeyValuePair<string, string>(DUMP_ROOT_REFLISTHEAD, string.Format("{0}*", EXT_REF_TYPE)),
                new KeyValuePair<string, string>(DUMP_ROOT_FREESECTHEAD, string.Format("{0}*", FREE_SECT_TYPE))
            };
            List<KeyValuePair<string, string>> extRefFields = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>(EXT_REF_DATAADDRESS, "int"),
                new KeyValuePair<string, string>(EXT_REF_TYPEID, "short"),
                new KeyValuePair<string, string>(EXT_REF_NEXT, string.Format("{0}*", EXT_REF_TYPE))
            };
            List<KeyValuePair<string, string>> freeSectFields = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>(FREE_SECT_ADDRESS, "int"),
                new KeyValuePair<string, string>(FREE_SECT_LENGTH, "int"),
                new KeyValuePair<string, string>(FREE_SECT_NEXT, string.Format("{0}*", FREE_SECT_TYPE))
            };
            this.RegisterType(new HeapType(DUMP_ROOT_TYPE, dumpRootFields));
            this.RegisterType(new HeapType(EXT_REF_TYPE, extRefFields));
            this.RegisterType(new HeapType(FREE_SECT_TYPE, freeSectFields));
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

        #endregion Internal metadata parsing methods

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

        private const string DUMP_ROOT_TYPE = "__DUMP_ROOT";
        private const string DUMP_ROOT_REFLISTHEAD = "RefListHead";
        private const string DUMP_ROOT_FREESECTHEAD = "FreeSectHead";
        private const string DUMP_ROOT_PAGESIZE = "PageSize";
        private const string EXT_REF_TYPE = "__EXT_REF";
        private const string EXT_REF_DATAADDRESS = "DataAddress";
        private const string EXT_REF_TYPEID = "TypeID";
        private const string EXT_REF_NEXT = "Next";
        private const string FREE_SECT_TYPE = "__FREE_SECT";
        private const string FREE_SECT_ADDRESS = "Address";
        private const string FREE_SECT_LENGTH = "Length";
        private const string FREE_SECT_NEXT = "Next";

        #endregion Private fields for allocation management
    }
}
