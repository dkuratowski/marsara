using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.Maps.PublicInterfaces
{
    /// <summary>
    /// Enumerates the possible types of fields in cells.
    /// </summary>
    public enum CellDataType
    {
        [EnumMapping("BOOL")]
        BOOL = 0,           /// A boolean value

        [EnumMapping("INT")]
        INT = 1,            /// 32-bit signed integer

        UNKNOWN = -1        /// Used to indicate error cases (for internal use only)
    }

    /// <summary>
    /// Defines the interface for reading/writing data attached to a cell.
    /// </summary>
    public interface ICellData
    {
        /// <summary>
        /// Reads a 32-bit signed integer from the given field of this cell data.
        /// </summary>
        /// <param name="fieldIndex">The index of the field you want to read from.</param>
        /// <returns>The result data in case of success.</returns>
        /// <exception cref="TileSetException">
        /// In case of reading an uninitialized field.
        /// In case of type mismatch or invalid field index.
        /// </exception>
        int ReadInt(int fieldIndex);

        /// <summary>
        /// Reads a boolean value from the given field of this cell data.
        /// </summary>
        /// <param name="fieldIndex">The index of the field you want to read from.</param>
        /// <returns>The result data in case of success.</returns>
        /// <exception cref="TileSetException">
        /// In case of reading an uninitialized field.
        /// In case of type mismatch or invalid field index.
        /// </exception>
        bool ReadBool(int fieldIndex);

        /// <summary>
        /// Checks whether the given field of this cell data has been initialized or not.
        /// </summary>
        /// <param name="fieldIndex">The index of the field you want to check.</param>
        /// <returns>True if the given field has been initialized, false otherwise.</returns>
        /// <exception cref="TileSetException">
        /// In case of invalid field index.
        /// </exception>
        bool IsFieldInitialized(int fieldIndex);

        /// <summary>
        /// Writes a 32-bit signed integer to the given field of this cell data.
        /// </summary>
        /// <param name="fieldIndex">The index of the field you want to write to.</param>
        /// <param name="data">The data you want to write.</param>
        /// <exception cref="TileSetException">
        /// In case of type mismatch or invalid field index.
        /// </exception>
        void WriteInt(int fieldIndex, int data);

        /// <summary>
        /// Writes a boolean value to the given field of this cell data.
        /// </summary>
        /// <param name="fieldIndex">The index of the field you want to write to.</param>
        /// <param name="data">The data you want to write.</param>
        /// <exception cref="TileSetException">
        /// In case of type mismatch or invalid field index.
        /// </exception>
        void WriteBool(int fieldIndex, bool data);

        /// <summary>
        /// Undos the last write operation on this cell data.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// If the very first write operation has already been undone.
        /// </exception>
        void Undo();

        /// <summary>
        /// Locks the cell data. Writing data after lock is not possible. If the CellData has already been locked,
        /// this function has no effect.
        /// </summary>
        void Lock();

        /// <summary>
        /// Constructs a clone of this cell data. It is possible to write the new cell data object
        /// even if the cloned cell data object has already been locked. The new cell data object
        /// will contain only the current state of this cell data object.
        /// </summary>
        ICellData Clone();
    }
}
