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
    public class SimulationHeapMgr : ISimulationHeapMgr // TODO: make private
    {
        /// <summary>
        /// Constructs a SimulationHeapMgr object.
        /// </summary>
        /// <param name="heap">Reference to the heap.</param>
        public SimulationHeapMgr(ISimulationHeap heap, Dictionary<string, Dictionary<string, string>> metadata)
        {
            if (heap == null) { throw new ArgumentNullException("heap"); }
            if (metadata == null) { throw new ArgumentNullException("metadata"); }

            this.heap = heap;
            this.typeIDs = new Dictionary<string, short>();
            this.types = new List<SimElementType>();

            this.RegisterBuiltInTypes();
            this.ParseMetadata(metadata);
        }

        #region ISimulationHeapMgr Members

        /// <see cref="ISimulationHeapMgr.GetTypeID"/>
        public int GetTypeID(string type)
        {
            throw new NotImplementedException();
        }

        /// <see cref="ISimulationHeapMgr.GetFieldIdx"/>
        public int GetFieldIdx(int typeID, string fieldName)
        {
            throw new NotImplementedException();
        }

        /// <see cref="ISimulationHeapMgr.GetFieldType"/>
        public string GetFieldType(int typeID, int fieldIdx)
        {
            throw new NotImplementedException();
        }

        /// <see cref="ISimulationHeapMgr.GetFieldTypeID"/>
        public int GetFieldTypeID(int typeID, int fieldIdx)
        {
            throw new NotImplementedException();
        }

        /// <see cref="ISimulationHeapMgr.New"/>
        public ISimElement New(int typeID)
        {
            throw new NotImplementedException();
        }

        /// <see cref="ISimulationHeapMgr.NewArray"/>
        public ISimElement NewArray(int typeID, int count)
        {
            throw new NotImplementedException();
        }

        #endregion ISimulationHeapMgr Members

        /// <summary>
        /// Internal method for parsing the incoming metadata.
        /// </summary>
        /// <param name="metadata">The metadata to be parsed.</param>
        private void ParseMetadata(Dictionary<string, Dictionary<string, string>> metadata)
        {
            /// Register all the composite types.
            foreach (KeyValuePair<string, Dictionary<string, string>> element in metadata)
            {
                this.RegisterType(new SimElementType(element.Key, element.Value), ref this.typeIDs, ref this.types);
            }

            /// Register all the pointer types.
            List<SimElementType> allNonPointerTypes = new List<SimElementType>(this.types);
            foreach (SimElementType element in allNonPointerTypes)
            {
                element.RegisterPointerTypes(ref this.typeIDs, ref this.types);
            }

            /// Compute the missing allocation sizes and field offsets
            foreach (SimElementType element in allNonPointerTypes)
            {
                element.ComputeFieldOffsets(this.types);
            }
        }

        /// <summary>
        /// Internal method for registering the built-in types to this manager.
        /// </summary>
        private void RegisterBuiltInTypes()
        {
            this.RegisterType(new SimElementType(BuiltInTypeEnum.Byte), ref this.typeIDs, ref this.types);
            this.RegisterType(new SimElementType(BuiltInTypeEnum.Short), ref this.typeIDs, ref this.types);
            this.RegisterType(new SimElementType(BuiltInTypeEnum.Integer), ref this.typeIDs, ref this.types);
            this.RegisterType(new SimElementType(BuiltInTypeEnum.Long), ref this.typeIDs, ref this.types);
        }

        /// <summary>
        /// Internal method for registering a new type to this manager.
        /// </summary>
        /// <param name="type">The type to be registered.</param>
        /// <param name="typeIDs">The list of the type IDs mapped by their names.</param>
        /// <param name="types">The list of the types.</param>
        private void RegisterType(SimElementType type, ref Dictionary<string, short> typeIDs, ref List<SimElementType> types)
        {
            if (this.typeIDs.ContainsKey(type.Name)) { throw new SimulationHeapException(string.Format("Type '{0}' already defined!", type.Name)); }
            if (this.types.Count == short.MaxValue) { throw new SimulationHeapException(string.Format("Number of possible types exceeded the limit of {0}!", short.MaxValue)); }

            type.SetID((short)this.types.Count);
            this.typeIDs.Add(type.Name, (short)this.types.Count);
            this.types.Add(type);
        }

        /// <summary>
        /// Reference to the heap.
        /// </summary>
        private ISimulationHeap heap;

        /// <summary>
        /// The types mapped by their IDs.
        /// </summary>
        private List<SimElementType> types;

        /// <summary>
        /// List of the type IDs mapped by their names.
        /// </summary>
        private Dictionary<string, short> typeIDs;
    }
}
