using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.Simulator.InternalInterfaces
{
    /// <summary>
    /// Enumerates the predefined built-in types.
    /// </summary>
    enum BuiltInTypeEnum
    {
        NonBuiltIn = 0,
        [EnumMapping("byte")]
        Byte = 1,
        [EnumMapping("short")]
        Short = 2,
        [EnumMapping("int")]
        Integer = 3,
        [EnumMapping("long")]
        Long = 4,
        [EnumMapping("num")]
        Number = 5,
        [EnumMapping("intvect")]
        IntVector = 6,
        [EnumMapping("numvect")]
        NumVector = 7,
        [EnumMapping("intrect")]
        IntRectangle = 8,
        [EnumMapping("numrect")]
        NumRectangle = 9
    }

    /// <summary>
    /// Interface of a heap type definition.
    /// </summary>
    interface IHeapType
    {
        /// <summary>
        /// Gets the name of this type.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the ID of this type.
        /// </summary>
        short ID { get; }

        /// <summary>
        /// Gets the ID of the type pointed by this type if this type is a pointer; otherwise -1.
        /// </summary>
        short PointedTypeID { get; }

        /// <summary>
        /// Gets the appropriate value if this is a built-in type; otherwise BuiltInTypeEnum.NonBuiltIn.
        /// </summary>
        BuiltInTypeEnum BuiltInType { get; }

        /// <summary>
        /// Gets and enumerable list of the names of the fields in this type.
        /// </summary>
        IEnumerable<string> FieldNames { get; }

        /// <summary>
        /// Gets the type ID of the given field.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <returns>The type ID of the given field.</returns>
        short GetFieldTypeID(string fieldName);

        /// <summary>
        /// Gets the index of the given field.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <returns>The index of the given field.</returns>
        int GetFieldIdx(string fieldName);

        /// <summary>
        /// Checks if this heap type has a field with the given name or not.
        /// </summary>
        /// <param name="fieldName">The name of the field to check.</param>
        /// <returns>True if this heap type has a field with the given name; otherwise false.</returns>
        bool HasField(string fieldName);
    }
}
