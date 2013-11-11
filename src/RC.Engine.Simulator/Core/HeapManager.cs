using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.InternalInterfaces;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// This class manages the allocations on the simulation-heap.
    /// </summary>
    class HeapManager : IHeapManager, IHeapDataFactory
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
            this.ParseMetadata(dataTypes);
        }

        #region IHeapManager members

        /// <see cref="IHeapManager.GetTypeID"/>
        public short GetTypeID(string type)
        {
            if (type == null) { throw new ArgumentNullException("type"); }
            if (!this.typeIDs.ContainsKey(type)) { throw new HeapException(string.Format("Unknown type '{0}'!", type)); }
            return this.typeIDs[type];
        }

        /// <see cref="IHeapManager.GetFieldIdx"/>
        public int GetFieldIdx(short typeID, string fieldName)
        {
            if (typeID < 0 || typeID >= this.types.Count) { throw new ArgumentOutOfRangeException("typeID"); }
            if (fieldName == null) { throw new ArgumentNullException("fieldName"); }

            HeapType type = this.types[typeID];
            if (type.FieldIndices == null) { throw new HeapException(string.Format("The type '{0}' is not a composite type!", type.Name)); }
            if (!type.FieldIndices.ContainsKey(fieldName)) { throw new HeapException(string.Format("Field '{0}' in type '{1}' doesn't exist!", fieldName, type.Name)); }

            return type.FieldIndices[fieldName];
        }

        /// <see cref="IHeapManager.GetFieldType"/>
        public string GetFieldType(short typeID, int fieldIdx)
        {
            return this.types[this.GetFieldTypeID(typeID, fieldIdx)].Name;
        }

        /// <see cref="IHeapManager.GetFieldTypeID"/>
        public short GetFieldTypeID(short typeID, int fieldIdx)
        {
            if (typeID < 0 || typeID >= this.types.Count) { throw new ArgumentOutOfRangeException("typeID"); }

            HeapType type = this.types[typeID];
            if (type.FieldTypeIDs == null) { throw new HeapException(string.Format("The type '{0}' is not a composite type!", type.Name)); }
            if (fieldIdx < 0 || fieldIdx >= type.FieldTypeIDs.Count) { throw new ArgumentOutOfRangeException("fieldIdx"); }

            return type.FieldTypeIDs[fieldIdx];
        }

        /// <see cref="IHeapManager.New"/>
        public IHeapData New(short typeID)
        {
            if (typeID < 0 || typeID >= this.types.Count) { throw new ArgumentOutOfRangeException("typeID"); }

            HeapType type = this.types[typeID];
            HeapSection sectToAlloc = this.AllocateSection(type.AllocationSize);
            return this.CreateHeapData(sectToAlloc.Address, typeID);
        }

        /// <see cref="IHeapManager.NewArray"/>
        public IHeapData NewArray(short typeID, int count)
        {
            if (typeID < 0 || typeID >= this.types.Count) { throw new ArgumentOutOfRangeException("typeID"); }
            if (count <= 0) { throw new ArgumentOutOfRangeException("count"); }

            HeapType type = this.types[typeID];
            HeapSection sectToAlloc = this.AllocateSection(count * type.AllocationSize + 4); /// +4 is for storing the number of array-items.
            this.heap.WriteInt(sectToAlloc.Address, count);
            return this.CreateHeapData(sectToAlloc.Address + 4, typeID);
        }

        /// <see cref="IHeapManager.ComputeHash"/>
        public byte[] ComputeHash()
        {
            return this.heap.ComputeHash();
        }

        /// <see cref="IHeapManager.SaveState"/>
        public byte[] SaveState(List<IHeapData> externalRefs)
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
            short EXT_REF_ID = this.GetTypeID(EXT_REF_TYPE);
            IHeapData extRefListHead = null;
            IHeapData extRefListPrev = null;
            foreach (IHeapData extRef in externalRefs)
            {
                HeapData extRefImpl = (HeapData)extRef;

                IHeapData extRefSave = this.New(EXT_REF_ID);
                ((IValueWrite<int>)extRefSave.AccessField(this.GetFieldIdx(EXT_REF_ID, EXT_REF_DATAADDRESS))).Write(extRefImpl.DataAddress);
                ((IValueWrite<short>)extRefSave.AccessField(this.GetFieldIdx(EXT_REF_ID, EXT_REF_TYPEID))).Write(extRefImpl.DataType.ID);

                /// Set the head if necessary.
                if (extRefListHead == null) { extRefListHead = extRefSave; }

                /// Set the "next" pointer of the previous element if necessary.
                if (extRefListPrev != null) { extRefListPrev.AccessField(this.GetFieldIdx(EXT_REF_ID, EXT_REF_NEXT)).PointTo(extRefSave); }

                extRefListPrev = extRefSave;
            }

            /// Save the linked-list of the free sections.
            short FREE_SECT_ID = this.GetTypeID(FREE_SECT_TYPE);
            IHeapData freeSectListHead = null;
            IHeapData freeSectListPrev = null;
            foreach (Tuple<int, int> freeSect in freeSections)
            {
                IHeapData freeSectSave = this.New(FREE_SECT_ID);
                ((IValueWrite<int>)freeSectSave.AccessField(this.GetFieldIdx(FREE_SECT_ID, FREE_SECT_ADDRESS))).Write(freeSect.Item1);
                ((IValueWrite<int>)freeSectSave.AccessField(this.GetFieldIdx(FREE_SECT_ID, FREE_SECT_LENGTH))).Write(freeSect.Item2);

                /// Set the head if necessary.
                if (freeSectListHead == null) { freeSectListHead = freeSectSave; }

                /// Set the "next" pointer of the previous element if necessary.
                if (freeSectListPrev != null)
                {
                    freeSectListPrev.AccessField(this.GetFieldIdx(FREE_SECT_ID, FREE_SECT_NEXT)).PointTo(freeSectSave);
                }

                freeSectListPrev = freeSectSave;
            }

            /// Set the "next" pointer of the last element to null.
            freeSectListPrev.AccessField(this.GetFieldIdx(FREE_SECT_ID, FREE_SECT_NEXT)).PointTo(null);

            /// Save the dump-root to the heap.
            short DUMP_ROOT_ID = this.GetTypeID(DUMP_ROOT_TYPE);
            IHeapData dumpRoot = this.New(DUMP_ROOT_ID);
            dumpRoot.AccessField(this.GetFieldIdx(DUMP_ROOT_ID, DUMP_ROOT_REFLISTHEAD)).PointTo(extRefListHead);
            dumpRoot.AccessField(this.GetFieldIdx(DUMP_ROOT_ID, DUMP_ROOT_FREESECTHEAD)).PointTo(freeSectListHead);

            /// Save the pointer to the dump-root at 0x00000000.
            this.heap.WriteInt(0, ((HeapData)dumpRoot).DataAddress);

            /// Dump the heap contents into a byte array.
            return this.heap.Dump();
        }

        /// <see cref="IHeapManager.LoadState"/>
        public List<IHeapData> LoadState(byte[] heapContent)
        {
            if (heapContent == null) { throw new ArgumentNullException("heapContent"); }

            /// Clear the free sections registry.
            this.heap = new Heap(heapContent, Constants.SIM_HEAP_CAPACITY);
            this.sectionObjectPool.Clear();
            this.freeSectionsHead = null;

            /// Load the dump-root from the heap.
            short DUMP_ROOT_ID = this.GetTypeID(DUMP_ROOT_TYPE);
            IHeapData dumpRoot = this.CreateHeapData(this.heap.ReadInt(0), DUMP_ROOT_ID);
            //new HeapData(this.heap.ReadInt(0), this.types[DUMP_ROOT_ID], this.heap, this.types, this.DeallocateSection);
            this.heap.WriteInt(0, 0);

            /// Load the free sections.
            short FREE_SECT_ID = this.GetTypeID(FREE_SECT_TYPE);
            HeapSection prevFreeSection = null;
            IHeapData currFreeSectAccess = dumpRoot.AccessField(this.GetFieldIdx(DUMP_ROOT_ID, DUMP_ROOT_FREESECTHEAD)).Dereference();
            while (currFreeSectAccess != null)
            {
                HeapSection currFreeSection = new HeapSection()
                {
                    Address = ((IValueRead<int>)currFreeSectAccess.AccessField(this.GetFieldIdx(FREE_SECT_ID, FREE_SECT_ADDRESS))).Read(),
                    Length = ((IValueRead<int>)currFreeSectAccess.AccessField(this.GetFieldIdx(FREE_SECT_ID, FREE_SECT_LENGTH))).Read(),
                    Next = null,
                    Prev = prevFreeSection
                };
                if (this.freeSectionsHead == null) { this.freeSectionsHead = currFreeSection; }
                if (prevFreeSection != null) { prevFreeSection.Next = currFreeSection; }
                prevFreeSection = currFreeSection;
                currFreeSectAccess = currFreeSectAccess.AccessField(this.GetFieldIdx(FREE_SECT_ID, FREE_SECT_NEXT)).Dereference();
            }

            /// Load the external references
            short EXT_REF_ID = this.GetTypeID(EXT_REF_TYPE);
            List<IHeapData> retList = new List<IHeapData>();
            IHeapData currExtRefAccess = dumpRoot.AccessField(this.GetFieldIdx(DUMP_ROOT_ID, DUMP_ROOT_REFLISTHEAD)).Dereference();
            while (currExtRefAccess != null)
            {
                retList.Add(
                    this.CreateHeapData(((IValueRead<int>)currExtRefAccess.AccessField(this.GetFieldIdx(EXT_REF_ID, EXT_REF_DATAADDRESS))).Read(),
                    ((IValueRead<short>)currExtRefAccess.AccessField(this.GetFieldIdx(EXT_REF_ID, EXT_REF_TYPEID))).Read()));
                currExtRefAccess = currExtRefAccess.AccessField(this.GetFieldIdx(EXT_REF_ID, EXT_REF_NEXT)).Dereference();
            }
            return retList;
        }

        #endregion IHeapManager members

        #region IHeapDataFactory members

        /// <see cref="IHeapDataFactory.CreateHeapData"/>
        public IHeapData CreateHeapData(int address, short typeID)
        {
            HeapType type = this.types[typeID];
            switch (type.BuiltInType)
            {
                case HeapType.BuiltInTypeEnum.NonBuiltIn:
                    return new HeapData(address, type, this.heap, this, this.DeallocateSection);
                case HeapType.BuiltInTypeEnum.Byte:
                    return new HeapByte(address, type, this.heap, this, this.DeallocateSection);
                case HeapType.BuiltInTypeEnum.Short:
                    return new HeapShort(address, type, this.heap, this, this.DeallocateSection);
                case HeapType.BuiltInTypeEnum.Integer:
                    return new HeapInt(address, type, this.heap, this, this.DeallocateSection);
                case HeapType.BuiltInTypeEnum.Long:
                    return new HeapLong(address, type, this.heap, this, this.DeallocateSection);
                case HeapType.BuiltInTypeEnum.Number:
                    return new HeapNumber(address, type, this.heap, this, this.DeallocateSection);
                case HeapType.BuiltInTypeEnum.IntVector:
                    return new HeapIntVector(address, type, this.heap, this, this.DeallocateSection);
                case HeapType.BuiltInTypeEnum.NumVector:
                    return new HeapNumVector(address, type, this.heap, this, this.DeallocateSection);
                case HeapType.BuiltInTypeEnum.IntRectangle:
                    return new HeapIntRectangle(address, type, this.heap, this, this.DeallocateSection);
                case HeapType.BuiltInTypeEnum.NumRectangle:
                    return new HeapNumRectangle(address, type, this.heap, this, this.DeallocateSection);
                default:
                    throw new InvalidOperationException("Impossible case happened!");
            }
        }

        #endregion IHeapDataFactory members

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
        private void ParseMetadata(IEnumerable<HeapType> dataTypes)
        {
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
            this.RegisterType(new HeapType(DUMP_ROOT_TYPE, dumpRootFields), ref this.typeIDs, ref this.types);
            this.RegisterType(new HeapType(EXT_REF_TYPE, extRefFields), ref this.typeIDs, ref this.types);
            this.RegisterType(new HeapType(FREE_SECT_TYPE, freeSectFields), ref this.typeIDs, ref this.types);

            /// Register all the composite types.
            foreach (HeapType dataType in dataTypes)
            {
                this.RegisterType(dataType, ref this.typeIDs, ref this.types);
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
            this.RegisterType(new HeapType(HeapType.BuiltInTypeEnum.Byte), ref this.typeIDs, ref this.types);
            this.RegisterType(new HeapType(HeapType.BuiltInTypeEnum.Short), ref this.typeIDs, ref this.types);
            this.RegisterType(new HeapType(HeapType.BuiltInTypeEnum.Integer), ref this.typeIDs, ref this.types);
            this.RegisterType(new HeapType(HeapType.BuiltInTypeEnum.Long), ref this.typeIDs, ref this.types);
            this.RegisterType(new HeapType(HeapType.BuiltInTypeEnum.Number), ref this.typeIDs, ref this.types);
            this.RegisterType(new HeapType(HeapType.BuiltInTypeEnum.IntVector), ref this.typeIDs, ref this.types);
            this.RegisterType(new HeapType(HeapType.BuiltInTypeEnum.NumVector), ref this.typeIDs, ref this.types);
            this.RegisterType(new HeapType(HeapType.BuiltInTypeEnum.IntRectangle), ref this.typeIDs, ref this.types);
            this.RegisterType(new HeapType(HeapType.BuiltInTypeEnum.NumRectangle), ref this.typeIDs, ref this.types);
        }

        /// <summary>
        /// Internal method for registering a new type to this manager.
        /// </summary>
        /// <param name="type">The type to be registered.</param>
        /// <param name="typeIDs">The list of the type IDs mapped by their names.</param>
        /// <param name="types">The list of the types.</param>
        private void RegisterType(HeapType type, ref Dictionary<string, short> typeIDs, ref List<HeapType> types)
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
