using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine
{
    /// <summary>
    /// Enumerates the possible types of fields in navigation cells.
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
    /// Contains the data attached to a navigation cell.
    /// </summary>
    public class CellData
    {
        /// <summary>
        /// Constructs a CellData object.
        /// </summary>
        /// <param name="tileset">The tileset that contains the declaration of the fields.</param>
        public CellData(TileSet tileset)
        {
            if (tileset == null) { throw new ArgumentNullException("tileset"); }

            this.tileset = tileset;
            this.boolFields = new List<bool>();
            this.intFields = new List<int>();
            this.initFlags = new List<bool>();
            this.isLocked = false;
        }

        /// <summary>
        /// Constructs a clone of the given CellData instance. It is possible to write the new CellData object
        /// even if the cloned CellData object has already been locked.
        /// </summary>
        /// <param name="other">The instance to clone.</param>
        public CellData(CellData other)
        {
            if (other == null) { throw new ArgumentNullException("other"); }

            this.tileset = other.tileset;
            this.boolFields = new List<bool>(other.boolFields);
            this.intFields = new List<int>(other.intFields);
            this.initFlags = new List<bool>(other.initFlags);
            this.isLocked = false;
        }

        #region Read methods

        /// <summary>
        /// Reads a 32-bit signed integer from the given field of this cell data.
        /// </summary>
        /// <param name="fieldIndex">The index of the field you want to read from.</param>
        /// <returns>The result data in case of success.</returns>
        /// <exception cref="TileSetException">
        /// In case of reading an uninitialized field.
        /// In case of type mismatch or invalid field index.
        /// </exception>
        public int ReadInt(int fieldIndex)
        {
            if (fieldIndex < 0) { throw new ArgumentOutOfRangeException("fieldIndex", "Field index cannot be negative!"); }
            if (this.tileset.GetFieldName(fieldIndex) == null) { throw new TileSetException(string.Format("Field with index '{0}' doesn't exist!", fieldIndex)); }
            if (this.tileset.GetFieldType(fieldIndex) != CellDataType.INT) { throw new TileSetException(string.Format("Type mismatch when reading field '{0}'!", fieldIndex)); }
            if (fieldIndex >= this.intFields.Count || !this.initFlags[fieldIndex]) { throw new TileSetException(string.Format("Field with index '{0}' is uninitialized!", fieldIndex)); }

            return this.intFields[fieldIndex];
        }

        /// <summary>
        /// Reads a boolean value from the given field of this cell data.
        /// </summary>
        /// <param name="fieldIndex">The index of the field you want to read from.</param>
        /// <returns>The result data in case of success.</returns>
        /// <exception cref="TileSetException">
        /// In case of reading an uninitialized field.
        /// In case of type mismatch or invalid field index.
        /// </exception>
        public bool ReadBool(int fieldIndex)
        {
            if (fieldIndex < 0) { throw new ArgumentOutOfRangeException("fieldIndex", "Field index cannot be negative!"); }
            if (this.tileset.GetFieldName(fieldIndex) == null) { throw new TileSetException(string.Format("Field with index '{0}' doesn't exist!", fieldIndex)); }
            if (this.tileset.GetFieldType(fieldIndex) != CellDataType.BOOL) { throw new TileSetException(string.Format("Type mismatch when reading field '{0}'!", fieldIndex)); }
            if (fieldIndex >= this.boolFields.Count || !this.initFlags[fieldIndex]) { throw new TileSetException(string.Format("Field with index '{0}' is uninitialized!", fieldIndex)); }

            return this.boolFields[fieldIndex];
        }

        /// <summary>
        /// Checks whether the given field of this cell data has been initialized or not.
        /// </summary>
        /// <param name="fieldIndex">The index of the field you want to check.</param>
        /// <returns>True if the given field has been initialized, false otherwise.</returns>
        /// <exception cref="TileSetException">
        /// In case of invalid field index.
        /// </exception>
        public bool IsFieldInitialized(int fieldIndex)
        {
            if (fieldIndex < 0) { throw new ArgumentOutOfRangeException("fieldIndex", "Field index cannot be negative!"); }
            if (this.tileset.GetFieldName(fieldIndex) == null) { throw new TileSetException(string.Format("Field with index '{0}' doesn't exist!", fieldIndex)); }

            return fieldIndex >= this.initFlags.Count || this.initFlags[fieldIndex];
        }

        #endregion Read methods

        #region Write methods

        /// <summary>
        /// Writes a 32-bit signed integer to the given field of this cell data.
        /// </summary>
        /// <param name="fieldIndex">The index of the field you want to write to.</param>
        /// <param name="data">The data you want to write.</param>
        /// <exception cref="TileSetException">
        /// In case of type mismatch or invalid field index.
        /// </exception>
        public void WriteInt(int fieldIndex, int data)
        {
            if (this.isLocked) { throw new InvalidOperationException("CellData locked!"); }
            if (fieldIndex < 0) { throw new ArgumentOutOfRangeException("fieldIndex", "Field index cannot be negative!"); }
            if (this.tileset.GetFieldName(fieldIndex) == null) { throw new TileSetException(string.Format("Field with index '{0}' doesn't exist!", fieldIndex)); }
            if (this.tileset.GetFieldType(fieldIndex) != CellDataType.INT) { throw new TileSetException(string.Format("Type mismatch when reading field '{0}'!", fieldIndex)); }

            if (fieldIndex >= this.intFields.Count)
            {
                /// Insert missing fields with less or equal indices and indicate them uninitialized
                /// with the corresponding flags.
                for (int i = this.intFields.Count; i <= fieldIndex; i++)
                {
                    this.boolFields.Add(default(bool));
                    this.intFields.Add(default(int));
                    this.initFlags.Add(false);
                }
            }

            this.intFields[fieldIndex] = data;
            this.initFlags[fieldIndex] = true;
        }

        /// <summary>
        /// Writes a boolean value to the given field of this cell data.
        /// </summary>
        /// <param name="fieldIndex">The index of the field you want to write to.</param>
        /// <param name="data">The data you want to write.</param>
        /// <exception cref="TileSetException">
        /// In case of type mismatch or invalid field index.
        /// </exception>
        public void WriteBool(int fieldIndex, bool data)
        {
            if (this.isLocked) { throw new InvalidOperationException("CellData locked!"); }
            if (fieldIndex < 0) { throw new ArgumentOutOfRangeException("fieldIndex", "Field index cannot be negative!"); }
            if (this.tileset.GetFieldName(fieldIndex) == null) { throw new TileSetException(string.Format("Field with index '{0}' doesn't exist!", fieldIndex)); }
            if (this.tileset.GetFieldType(fieldIndex) != CellDataType.BOOL) { throw new TileSetException(string.Format("Type mismatch when reading field '{0}'!", fieldIndex)); }

            if (fieldIndex >= this.boolFields.Count)
            {
                /// Insert missing fields with less or equal indices and indicate them uninitialized
                /// with the corresponding flags.
                for (int i = this.boolFields.Count; i <= fieldIndex; i++)
                {
                    this.boolFields.Add(default(bool));
                    this.intFields.Add(default(int));
                    this.initFlags.Add(false);
                }
            }

            this.boolFields[fieldIndex] = data;
            this.initFlags[fieldIndex] = true;
        }

        #endregion Write methods

        /// <summary>
        /// Locks the cell data. Writing data after lock is not possible. If the CellData has already been locked,
        /// this function has no effect.
        /// </summary>
        public void Lock()
        {
            this.isLocked = true;
        }

        /// <summary>
        /// Reference to the tileset that contains the declaration of the fields.
        /// </summary>
        private TileSet tileset;

        /// <summary>
        /// List of the bool fields. Those indices are ignored that are not defined as booleans.
        /// </summary>
        private List<bool> boolFields;

        /// <summary>
        /// List of the integer fields. Those indices are ignored that are not defined as integers.
        /// </summary>
        private List<int> intFields;

        /// <summary>
        /// Flags that indicates which fields have been initialized.
        /// </summary>
        private List<bool> initFlags;

        /// <summary>
        /// This flag indicates whether this CellData object has been locked or not.
        /// </summary>
        private bool isLocked;
    }
}
