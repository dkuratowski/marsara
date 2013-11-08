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
    class SimulationHeapMgr : ISimulationHeapMgr
    {
        /// <summary>
        /// Constructs a SimulationHeapMgr object.
        /// </summary>
        /// <param name="dataTypes">List of the data types.</param>
        public SimulationHeapMgr(IEnumerable<SimHeapType> dataTypes)
        {
            if (dataTypes == null) { throw new ArgumentNullException("dataTypes"); }

            this.heap = new SimulationHeap(Constants.SIM_HEAP_PAGESIZE, Constants.SIM_HEAP_CAPACITY);
            this.typeIDs = new Dictionary<string, short>();
            this.types = new List<SimHeapType>();

            this.sectionObjectPool = new Queue<SimHeapSection>();
            this.freeSectionsHead = new SimHeapSection()
            {
                Address = 4,    /// Reserve the first 4 bytes for internal use.
                Length = -1,    /// Goes on to the end of the heap.
                Next = null,
                Prev = null
            };

            this.RegisterBuiltInTypes();
            this.ParseMetadata(dataTypes);
        }

        #region ISimulationHeapMgr Members

        /// <see cref="ISimulationHeapMgr.GetTypeID"/>
        public int GetTypeID(string type)
        {
            if (type == null) { throw new ArgumentNullException("type"); }
            if (!this.typeIDs.ContainsKey(type)) { throw new SimulationHeapException(string.Format("Unknown type '{0}'!", type)); }
            return this.typeIDs[type];
        }

        /// <see cref="ISimulationHeapMgr.GetFieldIdx"/>
        public int GetFieldIdx(int typeID, string fieldName)
        {
            if (typeID < 0 || typeID >= this.types.Count) { throw new ArgumentOutOfRangeException("typeID"); }
            if (fieldName == null) { throw new ArgumentNullException("fieldName"); }

            SimHeapType type = this.types[typeID];
            if (type.FieldIndices == null) { throw new SimulationHeapException(string.Format("The type '{0}' is not a composite type!", type.Name)); }
            if (!type.FieldIndices.ContainsKey(fieldName)) { throw new SimulationHeapException(string.Format("Field '{0}' in type '{1}' doesn't exist!", fieldName, type.Name)); }

            return type.FieldIndices[fieldName];
        }

        /// <see cref="ISimulationHeapMgr.GetFieldType"/>
        public string GetFieldType(int typeID, int fieldIdx)
        {
            return this.types[this.GetFieldTypeID(typeID, fieldIdx)].Name;
        }

        /// <see cref="ISimulationHeapMgr.GetFieldTypeID"/>
        public int GetFieldTypeID(int typeID, int fieldIdx)
        {
            if (typeID < 0 || typeID >= this.types.Count) { throw new ArgumentOutOfRangeException("typeID"); }

            SimHeapType type = this.types[typeID];
            if (type.FieldTypeIDs == null) { throw new SimulationHeapException(string.Format("The type '{0}' is not a composite type!", type.Name)); }
            if (fieldIdx < 0 || fieldIdx >= type.FieldTypeIDs.Count) { throw new ArgumentOutOfRangeException("fieldIdx"); }

            return type.FieldTypeIDs[fieldIdx];
        }

        /// <see cref="ISimulationHeapMgr.New"/>
        public ISimHeapAccess New(int typeID)
        {
            if (typeID < 0 || typeID >= this.types.Count) { throw new ArgumentOutOfRangeException("typeID"); }

            SimHeapType type = this.types[typeID];
            SimHeapSection sectToAlloc = this.AllocateSection(type.AllocationSize);
            SimHeapAccess accessObj = new SimHeapAccess(sectToAlloc.Address, this.types[typeID], this.heap, this.types, this.DeallocateSection);
            return accessObj;
        }

        /// <see cref="ISimulationHeapMgr.NewArray"/>
        public ISimHeapAccess NewArray(int typeID, int count)
        {
            if (typeID < 0 || typeID >= this.types.Count) { throw new ArgumentOutOfRangeException("typeID"); }
            if (count <= 0) { throw new ArgumentOutOfRangeException("count"); }

            SimHeapType type = this.types[typeID];
            SimHeapSection sectToAlloc = this.AllocateSection(count * type.AllocationSize + 4); /// +4 is for storing the number of array-items.
            this.heap.WriteInt(sectToAlloc.Address, count);
            SimHeapAccess accessObj = new SimHeapAccess(sectToAlloc.Address + 4, this.types[typeID], this.heap, this.types, this.DeallocateSection);
            return accessObj;
        }

        /// <see cref="ISimulationHeapMgr.ComputeHash"/>
        public byte[] ComputeHash()
        {
            return this.heap.ComputeHash();
        }

        /// <see cref="ISimulationHeapMgr.SaveState"/>
        public byte[] SaveState(List<ISimHeapAccess> externalRefs)
        {
            if (externalRefs == null) { throw new ArgumentNullException("externalRefs"); }
            
            /// Collect the free section BEFORE starting the save process.
            List<Tuple<int, int>> freeSections = new List<Tuple<int, int>>();
            SimHeapSection currFreeSect = this.freeSectionsHead;
            do
            {
                freeSections.Add(new Tuple<int, int>(currFreeSect.Address, currFreeSect.Length));
            } while (currFreeSect.Next != null);

            /// Save the linked-list of the external references.
            int EXT_REF_ID = this.GetTypeID(EXT_REF_TYPE);
            ISimHeapAccess extRefListHead = null;
            ISimHeapAccess extRefListPrev = null;
            foreach (ISimHeapAccess extRef in externalRefs)
            {
                SimHeapAccess extRefImpl = (SimHeapAccess)extRef;

                ISimHeapAccess extRefSave = this.New(EXT_REF_ID);
                extRefSave.AccessField(this.GetFieldIdx(EXT_REF_ID, EXT_REF_DATAADDRESS)).WriteInt(extRefImpl.DataAddress);
                extRefSave.AccessField(this.GetFieldIdx(EXT_REF_ID, EXT_REF_TYPEID)).WriteShort(extRefImpl.DataType.ID);

                /// Set the head if necessary.
                if (extRefListHead == null) { extRefListHead = extRefSave; }

                /// Set the "next" pointer of the previous element if necessary.
                if (extRefListPrev != null) { extRefListPrev.AccessField(this.GetFieldIdx(EXT_REF_ID, EXT_REF_NEXT)).PointTo(extRefSave); }

                extRefListPrev = extRefSave;
            }

            /// Save the linked-list of the free sections.
            int FREE_SECT_ID = this.GetTypeID(FREE_SECT_TYPE);
            ISimHeapAccess freeSectListHead = null;
            ISimHeapAccess freeSectListPrev = null;
            foreach (Tuple<int, int> freeSect in freeSections)
            {
                ISimHeapAccess freeSectSave = this.New(FREE_SECT_ID);
                freeSectSave.AccessField(this.GetFieldIdx(FREE_SECT_ID, FREE_SECT_ADDRESS)).WriteInt(freeSect.Item1);
                freeSectSave.AccessField(this.GetFieldIdx(FREE_SECT_ID, FREE_SECT_LENGTH)).WriteInt(freeSect.Item2);

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
            int DUMP_ROOT_ID = this.GetTypeID(DUMP_ROOT_TYPE);
            ISimHeapAccess dumpRoot = this.New(DUMP_ROOT_ID);
            dumpRoot.AccessField(this.GetFieldIdx(DUMP_ROOT_ID, DUMP_ROOT_REFLISTHEAD)).PointTo(extRefListHead);
            dumpRoot.AccessField(this.GetFieldIdx(DUMP_ROOT_ID, DUMP_ROOT_FREESECTHEAD)).PointTo(freeSectListHead);

            /// Save the pointer to the dump-root at 0x00000000.
            this.heap.WriteInt(0, ((SimHeapAccess)dumpRoot).DataAddress);

            /// Dump the heap contents into a byte array.
            return this.heap.Dump();
        }

        /// <see cref="ISimulationHeapMgr.LoadState"/>
        public List<ISimHeapAccess> LoadState(byte[] heapContent)
        {
            if (heapContent == null) { throw new ArgumentNullException("heapContent"); }

            /// Clear the free sections registry.
            this.heap = new SimulationHeap(heapContent, Constants.SIM_HEAP_CAPACITY);
            this.sectionObjectPool.Clear();
            this.freeSectionsHead = null;

            /// Load the dump-root from the heap.
            int DUMP_ROOT_ID = this.GetTypeID(DUMP_ROOT_TYPE);
            ISimHeapAccess dumpRoot = new SimHeapAccess(this.heap.ReadInt(0), this.types[DUMP_ROOT_ID], this.heap, this.types, this.DeallocateSection);
            this.heap.WriteInt(0, 0);

            /// Load the free sections.
            int FREE_SECT_ID = this.GetTypeID(FREE_SECT_TYPE);
            SimHeapSection prevFreeSection = null;
            ISimHeapAccess currFreeSectAccess = dumpRoot.AccessField(this.GetFieldIdx(DUMP_ROOT_ID, DUMP_ROOT_FREESECTHEAD)).Dereference();
            while (currFreeSectAccess != null)
            {
                SimHeapSection currFreeSection = new SimHeapSection()
                {
                    Address = currFreeSectAccess.AccessField(this.GetFieldIdx(FREE_SECT_ID, FREE_SECT_ADDRESS)).ReadInt(),
                    Length = currFreeSectAccess.AccessField(this.GetFieldIdx(FREE_SECT_ID, FREE_SECT_LENGTH)).ReadInt(),
                    Next = null,
                    Prev = prevFreeSection
                };
                if (this.freeSectionsHead == null) { this.freeSectionsHead = currFreeSection; }
                if (prevFreeSection != null) { prevFreeSection.Next = currFreeSection; }
                prevFreeSection = currFreeSection;
                currFreeSectAccess = currFreeSectAccess.AccessField(this.GetFieldIdx(FREE_SECT_ID, FREE_SECT_NEXT)).Dereference();
            }

            /// Load the external references
            int EXT_REF_ID = this.GetTypeID(EXT_REF_TYPE);
            List<ISimHeapAccess> retList = new List<ISimHeapAccess>();
            ISimHeapAccess currExtRefAccess = dumpRoot.AccessField(this.GetFieldIdx(DUMP_ROOT_ID, DUMP_ROOT_REFLISTHEAD)).Dereference();
            while (currExtRefAccess != null)
            {
                retList.Add(new SimHeapAccess(
                    currExtRefAccess.AccessField(this.GetFieldIdx(EXT_REF_ID, EXT_REF_DATAADDRESS)).ReadInt(),
                    this.types[currExtRefAccess.AccessField(this.GetFieldIdx(EXT_REF_ID, EXT_REF_TYPEID)).ReadShort()],
                    this.heap,
                    this.types,
                    this.DeallocateSection
                    ));
                currExtRefAccess = currExtRefAccess.AccessField(this.GetFieldIdx(EXT_REF_ID, EXT_REF_NEXT)).Dereference();
            }
            return retList;
        }

        #endregion ISimulationHeapMgr Members

        #region Internal allocation management methods

        /// <summary>
        /// Allocates a section with the given size from the normal free sections.
        /// </summary>
        /// <param name="size">The size of the section to allocate.</param>
        /// <returns>The allocated section.</returns>
        private SimHeapSection AllocateSection(int size)
        {
            SimHeapSection currFreeSect = this.freeSectionsHead;
            while (currFreeSect.Length != -1 && currFreeSect.Length < size && currFreeSect != null)
            {
                currFreeSect = currFreeSect.Next;
            }

            if (currFreeSect == null) { throw new SimulationHeapException("SimulationHeap is full!"); }
            
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
                    throw new SimulationHeapException("Unexpected case!");
                }

                return currFreeSect;
            }
            else
            {
                /// Size doesn't fit exactly, need to split the section.
                SimHeapSection newSection = this.sectionObjectPool.Count != 0
                                          ? this.sectionObjectPool.Dequeue()
                                          : new SimHeapSection();
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
            SimHeapSection section = this.sectionObjectPool.Count != 0
                          ? this.sectionObjectPool.Dequeue()
                          : new SimHeapSection();
            section.Address = address;
            section.Length = length;
            section.Next = null;
            section.Prev = null;

            SimHeapSection currFreeSect = this.freeSectionsHead;
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
                throw new SimulationHeapException("Unexpected case!");
            }

            if (section.Prev != null)
            {
                if (section.Prev.Address + section.Prev.Length == section.Address)
                {
                    /// Merge the section with section.Prev.
                    SimHeapSection sectToDel = section.Prev;
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
                    throw new SimulationHeapException("Unexpected case!");
                }
            }
        }

        #endregion Internal allocation management methods

        #region Internal metadata parsing methods

        /// <summary>
        /// Internal method for parsing the incoming metadata.
        /// </summary>
        /// <param name="dataTypes">List of the data types.</param>
        private void ParseMetadata(IEnumerable<SimHeapType> dataTypes)
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
            this.RegisterType(new SimHeapType(DUMP_ROOT_TYPE, dumpRootFields), ref this.typeIDs, ref this.types);
            this.RegisterType(new SimHeapType(EXT_REF_TYPE, extRefFields), ref this.typeIDs, ref this.types);
            this.RegisterType(new SimHeapType(FREE_SECT_TYPE, freeSectFields), ref this.typeIDs, ref this.types);

            /// Register all the composite types.
            foreach (SimHeapType dataType in dataTypes)
            {
                this.RegisterType(dataType, ref this.typeIDs, ref this.types);
            }

            /// Register all the pointer types.
            List<SimHeapType> allNonPointerTypes = new List<SimHeapType>(this.types);
            foreach (SimHeapType element in allNonPointerTypes)
            {
                element.RegisterPointerTypes(ref this.typeIDs, ref this.types);
            }

            /// Compute the missing allocation sizes and field offsets
            foreach (SimHeapType element in allNonPointerTypes)
            {
                element.ComputeFieldOffsets(this.types);
            }
        }

        /// <summary>
        /// Internal method for registering the built-in types to this manager.
        /// </summary>
        private void RegisterBuiltInTypes()
        {
            this.RegisterType(new SimHeapType(BuiltInTypeEnum.Byte), ref this.typeIDs, ref this.types);
            this.RegisterType(new SimHeapType(BuiltInTypeEnum.Short), ref this.typeIDs, ref this.types);
            this.RegisterType(new SimHeapType(BuiltInTypeEnum.Integer), ref this.typeIDs, ref this.types);
            this.RegisterType(new SimHeapType(BuiltInTypeEnum.Long), ref this.typeIDs, ref this.types);
            this.RegisterType(new SimHeapType(BuiltInTypeEnum.Number), ref this.typeIDs, ref this.types);
            this.RegisterType(new SimHeapType(BuiltInTypeEnum.IntVector), ref this.typeIDs, ref this.types);
            this.RegisterType(new SimHeapType(BuiltInTypeEnum.NumVector), ref this.typeIDs, ref this.types);
            this.RegisterType(new SimHeapType(BuiltInTypeEnum.IntRectangle), ref this.typeIDs, ref this.types);
            this.RegisterType(new SimHeapType(BuiltInTypeEnum.NumRectangle), ref this.typeIDs, ref this.types);
        }

        /// <summary>
        /// Internal method for registering a new type to this manager.
        /// </summary>
        /// <param name="type">The type to be registered.</param>
        /// <param name="typeIDs">The list of the type IDs mapped by their names.</param>
        /// <param name="types">The list of the types.</param>
        private void RegisterType(SimHeapType type, ref Dictionary<string, short> typeIDs, ref List<SimHeapType> types)
        {
            if (this.typeIDs.ContainsKey(type.Name)) { throw new SimulationHeapException(string.Format("Type '{0}' already defined!", type.Name)); }
            if (this.types.Count == short.MaxValue) { throw new SimulationHeapException(string.Format("Number of possible types exceeded the limit of {0}!", short.MaxValue)); }

            type.SetID((short)this.types.Count);
            this.typeIDs.Add(type.Name, (short)this.types.Count);
            this.types.Add(type);
        }

        #endregion Internal metadata parsing methods

        /// <summary>
        /// Reference to the heap.
        /// </summary>
        private ISimulationHeap heap;

        /// <summary>
        /// List of the types mapped by their IDs.
        /// </summary>
        private List<SimHeapType> types;

        /// <summary>
        /// List of the type IDs mapped by their names.
        /// </summary>
        private Dictionary<string, short> typeIDs;

        #region Private fields for allocation management

        /// <summary>
        /// The head of the linked-list that contains the free sections on the heap.
        /// </summary>
        private SimHeapSection freeSectionsHead;

        /// <summary>
        /// FIFO list of the currently inactive SimHeapSection objects.
        /// </summary>
        private Queue<SimHeapSection> sectionObjectPool;

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
