using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Common;
using RC.Engine.Simulator.InternalInterfaces;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Contains informations about a data type that can be stored on the simulation heap.
    /// </summary>
    class HeapType : IHeapType
    {
        #region Constructors

        /// <summary>
        /// Constructs a composite HeapType.
        /// </summary>
        /// <param name="name">The name of this composite type.</param>
        /// <param name="fields">The fields of this composite type.</param>
        public HeapType(string name, List<KeyValuePair<string, string>> fields)
        {
            if (name == null) { throw new ArgumentNullException("name"); }
            name = name.Trim();
            if (!IDENTIFIER_SYNTAX.IsMatch(name) || name.EndsWith("*")) { throw new HeapException(string.Format("Invalid type name '{0}'!", name)); }
            if (fields == null) { throw new ArgumentNullException("fields"); }
            //if (fields.Count == 0) { throw new HeapException(string.Format("User defined type '{0}' doesn't define any fields!", name)); }

            /// Members filled at initialization.
            this.name = name;
            this.builtInType = BuiltInTypeEnum.NonBuiltIn;
            this.pointedTypeID = -1;
            this.fieldTypeIDs = new List<short>();
            this.fieldOffsets = new List<int>();
            this.tmpFieldTypeNames = new List<string>();
            this.fieldIndices = new Dictionary<string, short>();
            foreach (KeyValuePair<string, string> field in fields)
            {
                string fieldName = field.Key.Trim();
                if (!IDENTIFIER_SYNTAX.IsMatch(fieldName) || fieldName.EndsWith("*")) { throw new HeapException(string.Format("Invalid field name '{0}' defined in type '{1}'!", fieldName, name)); }

                if (field.Value == null) { throw new ArgumentNullException(string.Format("fields[{0}]", fieldName)); }
                string fieldType = field.Value.Trim();
                if (!IDENTIFIER_SYNTAX.IsMatch(fieldType)) { throw new HeapException(string.Format("Invalid field type '{0}' defined for field '{1}' in type '{2}'!", fieldType, fieldName, name)); }

                this.fieldIndices.Add(fieldName, (short)this.fieldTypeIDs.Count);
                this.fieldTypeIDs.Add(-1); /// Will be filled during validation
                this.fieldOffsets.Add(-1); /// Will be filled during validation
                this.tmpFieldTypeNames.Add(fieldType);
            }

            /// Members that will be filled during validation.
            this.id = -1;
            this.allocationSize = -1;
        }

        /// <summary>
        /// Constructs a built-in HeapType.
        /// </summary>
        /// <param name="builtInType">The built-in type to be constructed.</param>
        public HeapType(BuiltInTypeEnum builtInType)
        {
            /// Members filled at initialization.
            switch (builtInType)
            {
                case BuiltInTypeEnum.Byte:
                    this.allocationSize = 1;
                    break;
                case BuiltInTypeEnum.Short:
                    this.allocationSize = 2;
                    break;
                case BuiltInTypeEnum.Integer:
                case BuiltInTypeEnum.Number:
                    this.allocationSize = 4;
                    break;
                case BuiltInTypeEnum.Long:
                    this.allocationSize = 8;
                    break;
                case BuiltInTypeEnum.IntVector:
                case BuiltInTypeEnum.NumVector:
                    this.allocationSize = 9;
                    break;
                case BuiltInTypeEnum.IntRectangle:
                case BuiltInTypeEnum.NumRectangle:
                    this.allocationSize = 17;
                    break;
                default:
                    throw new ArgumentException("Unexpected value of 'builtInType'!", "builtInType");
            }
            string name;
            if (!EnumMap<BuiltInTypeEnum, string>.TryMap(builtInType, out name) || name == null) { throw new ArgumentException("No name defined for the built-in type!", "builtInType"); }
            if (!IDENTIFIER_SYNTAX.IsMatch(name) || name.EndsWith("*")) { throw new HeapException(string.Format("Invalid built-in type name '{0}'!", name)); }
            this.name = name;
            this.builtInType = builtInType;
            this.pointedTypeID = -1;
            this.fieldTypeIDs = null;
            this.fieldOffsets = null;
            this.tmpFieldTypeNames = null;
            this.fieldIndices = null;

            /// Members that will be filled during validation.
            this.id = -1;
        }

        /// <summary>
        /// Creates a pointer HeapType that points to the given type.
        /// </summary>
        /// <param name="pointerTypeName">The name of this pointer type.</param>
        /// <param name="pointedTypeID">The ID of the pointed type.</param>
        public HeapType(string pointerTypeName, short pointedTypeID)
        {
            if (pointerTypeName == null) { throw new ArgumentNullException("pointerTypeName"); }
            if (pointedTypeID < 0) { throw new ArgumentOutOfRangeException("", "ID of the pointed type must be non-negative!"); }
            pointerTypeName = pointerTypeName.Trim();
            if (!IDENTIFIER_SYNTAX.IsMatch(pointerTypeName)) { throw new HeapException(string.Format("Invalid type name '{0}'!", pointerTypeName)); }

            /// Members filled at initialization.
            this.name = pointerTypeName;
            this.pointedTypeID = pointedTypeID;
            this.allocationSize = 4;
            this.builtInType = BuiltInTypeEnum.NonBuiltIn;
            this.fieldTypeIDs = null;
            this.fieldOffsets = null;
            this.fieldIndices = null;
            this.tmpFieldTypeNames = null;

            /// Members that will be filled during validation.
            this.id = -1;
        }

        #endregion Constructors

        #region Internal metadata parsing methods

        /// <summary>
        /// Sets the ID of this HeapType.
        /// </summary>
        /// <param name="id">The ID to be set.</param>
        /// <exception cref="InvalidOperationException">If the ID has already been set.</exception>
        public void SetID(short id)
        {
            if (id < 0) { throw new ArgumentOutOfRangeException("id", "ID must be non-negative!"); }
            if (this.id != -1) { throw new InvalidOperationException(string.Format("ID already set for type '{0}'!", this.name)); }
            this.id = id;
        }

        /// <summary>
        /// Creates and registers pointer types found in the field declarations if this is a composite type.
        /// Otherwise this function has no effect.
        /// </summary>
        /// <param name="typeIDs">The list of the type IDs mapped by their names.</param>
        /// <param name="types">The list of the types.</param>
        public void RegisterPointerTypes(ref Dictionary<string, short> typeIDs, ref List<HeapType> types)
        {
            if (typeIDs == null) { throw new ArgumentNullException("typeIDs"); }
            if (types == null) { throw new ArgumentNullException("types"); }

            if (this.fieldIndices == null) { return; }

            foreach (KeyValuePair<string, short> field in this.fieldIndices)
            {
                string fieldName = field.Key;
                short fieldIdx = field.Value;
                string fieldTypeName = this.tmpFieldTypeNames[fieldIdx];
                if (typeIDs.ContainsKey(fieldTypeName))
                {
                    /// Only has to set the type ID of the field.
                    this.fieldTypeIDs[fieldIdx] = typeIDs[fieldTypeName];
                }
                else if (fieldTypeName.EndsWith("*"))
                {
                    /// Registering the pointer types that are implicitly or explicitly in the type name string.
                    this.RegisterPointer(fieldTypeName.Substring(0, fieldTypeName.Length - 1), ref typeIDs, ref types);
                    this.fieldTypeIDs[fieldIdx] = typeIDs[fieldTypeName];
                }
                else
                {
                    /// Error: undefined type.
                    throw new HeapException(string.Format("Undefined type '{0}' of field '{1}' in '{2}'!", this.tmpFieldTypeNames[fieldIdx], fieldName, this.name));
                }
            }
        }

        /// <summary>
        /// Computes the offsets of the fields if this is a composite type. Otherwise this function has no effect.
        /// </summary>
        /// <param name="types">The list of the types.</param>
        public void ComputeFieldOffsets(List<HeapType> types)
        {
            if (types == null) { throw new ArgumentNullException("types"); }

            RCSet<short> triedTypeIDs = new RCSet<short>();
            this.ComputeFieldOffsetsInternal(types, ref triedTypeIDs);
        }

        /// <summary>
        /// Internal recursive method for registering the pointer types that are implicitly or explicitly in the type name string.
        /// </summary>
        /// <param name="pointedTypeName">The name of the pointed type.</param>
        /// <param name="typeIDs">The list of the type IDs mapped by their names.</param>
        /// <param name="types">The list of the types.</param>
        private void RegisterPointer(string pointedTypeName, ref Dictionary<string, short> typeIDs, ref List<HeapType> types)
        {
            /// If the pointed type is also a pointer then continue the recursion.
            if (pointedTypeName.EndsWith("*"))
            {
                this.RegisterPointer(pointedTypeName.Substring(0, pointedTypeName.Length - 1), ref typeIDs, ref types);
            }

            string pointerTypeName = string.Format("{0}*", pointedTypeName);
            if (typeIDs.ContainsKey(pointerTypeName)) { return; }
            if (types.Count == short.MaxValue) { throw new HeapException(string.Format("Number of possible types exceeded the limit of {0}!", short.MaxValue)); }

            HeapType ptrType = new HeapType(pointerTypeName, typeIDs[pointedTypeName]);
            ptrType.SetID((short)types.Count);
            typeIDs.Add(ptrType.Name, (short)types.Count);
            types.Add(ptrType);
        }

        /// <summary>
        /// Internal implementation of ComputeFieldOffsets.
        /// </summary>
        /// <param name="types">The list of the types.</param>
        /// <param name="triedTypeIDs">Used to avoid infinite loop.</param>
        private void ComputeFieldOffsetsInternal(List<HeapType> types, ref RCSet<short> triedTypeIDs)
        {
            if (this.fieldIndices == null) { return; }

            if (!triedTypeIDs.Add(this.id)) { throw new HeapException(string.Format("Infinite cycle found in the layout of element '{0}'!", this.name)); }
            int allocationSize = 0;
            for (int fieldIdx = 0; fieldIdx < this.fieldTypeIDs.Count; fieldIdx++)
            {
                this.fieldOffsets[fieldIdx] = allocationSize;
                short fieldTypeID = this.fieldTypeIDs[fieldIdx];
                HeapType fieldType = types[fieldTypeID];
                if (fieldType.AllocationSize == -1)
                {
                    /// Compute the allocation size of the field type first.
                    fieldType.ComputeFieldOffsetsInternal(types, ref triedTypeIDs);
                }
                allocationSize += fieldType.AllocationSize;
            }
            this.allocationSize = allocationSize;
            triedTypeIDs.Remove(this.id);
        }

        #endregion Internal metadata parsing methods

        #region IHeapType methods

        /// <see cref="IHeapType.Name"/>
        public string Name { get { return this.name; } }

        /// <see cref="IHeapType.ID"/>
        public short ID { get { return this.id; } }

        /// <see cref="IHeapType.PointedTypeID"/>
        public short PointedTypeID { get { return this.pointedTypeID; } }

        /// <see cref="IHeapType.BuiltInType"/>
        public BuiltInTypeEnum BuiltInType { get { return this.builtInType; } }

        /// <see cref="IHeapType.FieldNames"/>
        public IEnumerable<string> FieldNames { get { return this.fieldIndices.Keys; } }

        /// <see cref="IHeapType.GetFieldTypeID"/>
        public short GetFieldTypeID(string fieldName)
        {
            if (this.fieldTypeIDs == null) { throw new HeapException(string.Format("The type '{0}' is not a composite type!", this.name)); }
            if (!this.fieldIndices.ContainsKey(fieldName)) { throw new HeapException(string.Format("The type '{0}' doesn't contain field '{1}'!", this.name, fieldName)); }

            return this.fieldTypeIDs[this.fieldIndices[fieldName]];
        }

        /// <see cref="IHeapType.GetFieldIdx"/>
        public int GetFieldIdx(string fieldName)
        {
            if (fieldName == null) { throw new ArgumentNullException("fieldName"); }

            if (this.fieldIndices == null) { throw new HeapException(string.Format("The type '{0}' is not a composite type!", this.name)); }
            if (!this.fieldIndices.ContainsKey(fieldName)) { throw new HeapException(string.Format("Field '{0}' in type '{1}' doesn't exist!", fieldName, this.name)); }

            return this.fieldIndices[fieldName];
        }

        /// <see cref="IHeapType.HasField"/>
        public bool HasField(string fieldName)
        {
            if (fieldName == null) { throw new ArgumentNullException("fieldName"); }
            if (this.fieldIndices == null) { throw new HeapException(string.Format("The type '{0}' is not a composite type!", this.name)); }
            return this.fieldIndices.ContainsKey(fieldName);
        }

        #endregion IHeapType methods

        #region Public properties

        /// <summary>
        /// Gets he size to be allocated if this type is instantiated on the simulation heap.
        /// </summary>
        public int AllocationSize { get { return this.allocationSize; } }

        /// <summary>
        /// Gets the list of the type IDs of the fields mapped by their indices if this is a composite type; otherwise null.
        /// </summary>
        public List<short> FieldTypeIDs { get { return this.fieldTypeIDs; } }

        /// <summary>
        /// Gets the list of the offsets of the fields mapped by their indices if this is a composite type; otherwise null.
        /// </summary>
        public List<int> FieldOffsets { get { return this.fieldOffsets; } }

        /// <summary>
        /// Gets the list of the indices of the fields mapped by their names if this is a composite type; otherwise null.
        /// </summary>
        public Dictionary<string, short> FieldIndices { get { return this.fieldIndices; } }

        #endregion Public properties

        /// <summary>
        /// Gets the string representation of this heap type definition.
        /// </summary>
        /// <returns>The string representation of this heap type definition.</returns>
        public override string ToString()
        {
            return string.Format("{0}({1})", this.name, this.id);
        }

        /// <summary>
        /// The name of this type.
        /// </summary>
        private string name;

        /// <summary>
        /// The ID of this type or -1 if the ID has not yet been set.
        /// </summary>
        private short id;

        /// <summary>
        /// The ID of the type pointed by this type if this type is a pointer; otherwise -1.
        /// </summary>
        private short pointedTypeID;

        /// <summary>
        /// The size to be allocated if this type is instantiated on the simulation heap.
        /// </summary>
        private int allocationSize;

        /// <summary>
        /// The appropriate value if this is a built-in type; otherwise BuiltInType.NonBuiltIn.
        /// </summary>
        private BuiltInTypeEnum builtInType;

        /// <summary>
        /// List of the type IDs of the fields mapped by their indices if this is a composite type; otherwise null.
        /// </summary>
        private List<short> fieldTypeIDs;

        /// <summary>
        /// List of the offsets of the fields mapped by their indices if this is a composite type; otherwise null.
        /// </summary>
        private List<int> fieldOffsets;

        /// <summary>
        /// Temporary list for storing the names of the type of the fields mapped by their indices if this is a
        /// composite type; otherwise null.
        /// </summary>
        private List<string> tmpFieldTypeNames;

        /// <summary>
        /// List of the indices of the fields mapped by their names if this is a composite type; otherwise null.
        /// </summary>
        private Dictionary<string, short> fieldIndices;

        /// <summary>
        /// Regular expression for checking the syntax of the name of identifiers.
        /// </summary>
        private static readonly Regex IDENTIFIER_SYNTAX = new Regex(string.Format("^[a-zA-Z_][a-zA-Z0-9_]*{0}*$", Regex.Escape("*")));
    }
}
