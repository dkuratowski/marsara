using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Maps.Core
{
    /// <summary>
    /// Contains the data attached to a cell.
    /// </summary>
    class CellData : ICellData
    {
        /// <summary>
        /// Constructs a CellData object.
        /// </summary>
        /// <param name="tileset">The tileset that contains the declaration of the fields.</param>
        public CellData(ITileSet tileset)
        {
            if (tileset == null) { throw new ArgumentNullException("tileset"); }

            this.tileset = tileset;
            this.boolFields = new List<Stack<bool>>();
            this.intFields = new List<Stack<int>>();
            this.initFlags = new List<bool>();
            this.writtenFields = new Stack<int>();
            this.isLocked = false;
        }

        #region ICellData methods

        /// <see cref="ICellData.ReadInt"/>
        public int ReadInt(int fieldIndex)
        {
            if (fieldIndex < 0) { throw new ArgumentOutOfRangeException("fieldIndex", "Field index cannot be negative!"); }
            if (this.tileset.GetCellDataFieldName(fieldIndex) == null) { throw new TileSetException(string.Format("Field with index '{0}' doesn't exist!", fieldIndex)); }
            if (this.tileset.GetCellDataFieldType(fieldIndex) != CellDataType.INT) { throw new TileSetException(string.Format("Type mismatch when reading field '{0}'!", fieldIndex)); }
            if (fieldIndex >= this.intFields.Count || !this.initFlags[fieldIndex]) { throw new TileSetException(string.Format("Field with index '{0}' is uninitialized!", fieldIndex)); }

            return this.intFields[fieldIndex].Peek();
        }

        /// <see cref="ICellData.ReadBool"/>
        public bool ReadBool(int fieldIndex)
        {
            if (fieldIndex < 0) { throw new ArgumentOutOfRangeException("fieldIndex", "Field index cannot be negative!"); }
            if (this.tileset.GetCellDataFieldName(fieldIndex) == null) { throw new TileSetException(string.Format("Field with index '{0}' doesn't exist!", fieldIndex)); }
            if (this.tileset.GetCellDataFieldType(fieldIndex) != CellDataType.BOOL) { throw new TileSetException(string.Format("Type mismatch when reading field '{0}'!", fieldIndex)); }
            if (fieldIndex >= this.boolFields.Count || !this.initFlags[fieldIndex]) { throw new TileSetException(string.Format("Field with index '{0}' is uninitialized!", fieldIndex)); }

            return this.boolFields[fieldIndex].Peek();
        }

        /// <see cref="ICellData.IsFieldInitialized"/>
        public bool IsFieldInitialized(int fieldIndex)
        {
            if (fieldIndex < 0) { throw new ArgumentOutOfRangeException("fieldIndex", "Field index cannot be negative!"); }
            if (this.tileset.GetCellDataFieldName(fieldIndex) == null) { throw new TileSetException(string.Format("Field with index '{0}' doesn't exist!", fieldIndex)); }

            return fieldIndex >= this.initFlags.Count || this.initFlags[fieldIndex];
        }

        /// <see cref="ICellData.WriteInt"/>
        public void WriteInt(int fieldIndex, int data)
        {
            if (this.isLocked) { throw new InvalidOperationException("CellData locked!"); }
            if (fieldIndex < 0) { throw new ArgumentOutOfRangeException("fieldIndex", "Field index cannot be negative!"); }
            if (this.tileset.GetCellDataFieldName(fieldIndex) == null) { throw new TileSetException(string.Format("Field with index '{0}' doesn't exist!", fieldIndex)); }
            if (this.tileset.GetCellDataFieldType(fieldIndex) != CellDataType.INT) { throw new TileSetException(string.Format("Type mismatch when reading field '{0}'!", fieldIndex)); }

            if (fieldIndex >= this.intFields.Count)
            {
                /// Insert missing fields with less or equal indices and indicate them uninitialized
                /// with the corresponding flags.
                for (int i = this.intFields.Count; i <= fieldIndex; i++)
                {
                    this.boolFields.Add(null);
                    this.intFields.Add(new Stack<int>());
                    this.initFlags.Add(false);
                }
            }

            this.intFields[fieldIndex].Push(data);
            this.writtenFields.Push(fieldIndex);
            this.initFlags[fieldIndex] = true;
        }

        /// <see cref="ICellData.WriteBool"/>
        public void WriteBool(int fieldIndex, bool data)
        {
            if (this.isLocked) { throw new InvalidOperationException("CellData locked!"); }
            if (fieldIndex < 0) { throw new ArgumentOutOfRangeException("fieldIndex", "Field index cannot be negative!"); }
            if (this.tileset.GetCellDataFieldName(fieldIndex) == null) { throw new TileSetException(string.Format("Field with index '{0}' doesn't exist!", fieldIndex)); }
            if (this.tileset.GetCellDataFieldType(fieldIndex) != CellDataType.BOOL) { throw new TileSetException(string.Format("Type mismatch when reading field '{0}'!", fieldIndex)); }

            if (fieldIndex >= this.boolFields.Count)
            {
                /// Insert missing fields with less or equal indices and indicate them uninitialized
                /// with the corresponding flags.
                for (int i = this.boolFields.Count; i <= fieldIndex; i++)
                {
                    this.boolFields.Add(new Stack<bool>());
                    this.intFields.Add(null);
                    this.initFlags.Add(false);
                }
            }

            this.boolFields[fieldIndex].Push(data);
            this.writtenFields.Push(fieldIndex);
            this.initFlags[fieldIndex] = true;
        }

        /// <see cref="ICellData.Lock"/>
        public void Lock()
        {
            this.isLocked = true;
        }

        /// <see cref="ICellData.Undo"/>
        public void Undo()
        {
            if (this.isLocked) { throw new InvalidOperationException("CellData locked!"); }
            if (this.writtenFields.Count == 0) { throw new InvalidOperationException("The very first write operation has already been undone!"); }

            int fieldIndex = this.writtenFields.Pop();
            if (this.tileset.GetCellDataFieldType(fieldIndex) == CellDataType.INT)
            {
                this.intFields[fieldIndex].Pop();
                if (this.intFields[fieldIndex].Count == 0) { this.initFlags[fieldIndex] = false; }
            }
            else if (this.tileset.GetCellDataFieldType(fieldIndex) == CellDataType.BOOL)
            {
                this.boolFields[fieldIndex].Pop();
                if (this.boolFields[fieldIndex].Count == 0) { this.initFlags[fieldIndex] = false; }
            }
            else
            {
                throw new InvalidOperationException("Unexpected field data type!");
            }
        }

        /// <see cref="ICellData.Clone"/>
        public ICellData Clone()
        {
            return new CellData(this);
        }

        #endregion ICellData methods

        /// <summary>
        /// Constructs a clone of the given CellData instance. It is possible to write the new CellData object
        /// even if the cloned CellData object has already been locked.
        /// </summary>
        /// <param name="other">The instance to clone.</param>
        private CellData(CellData other)
        {
            if (other == null) { throw new ArgumentNullException("other"); }

            this.tileset = other.tileset;
            this.boolFields = new List<Stack<bool>>();
            this.intFields = new List<Stack<int>>();
            this.initFlags = new List<bool>(other.initFlags);
            this.writtenFields = new Stack<int>();

            for (int i = 0; i < other.initFlags.Count; i++)
            {
                if (this.tileset.GetCellDataFieldType(i) == CellDataType.INT)
                {
                    this.boolFields.Add(null);
                    this.intFields.Add(new Stack<int>());
                    if (other.initFlags[i])
                    {
                        this.intFields[i].Push(other.intFields[i].Peek());
                        this.writtenFields.Push(i);
                        this.initFlags.Add(true);
                    }
                    else
                    {
                        this.initFlags.Add(false);
                    }
                }
                else if (this.tileset.GetCellDataFieldType(i) == CellDataType.BOOL)
                {
                    this.boolFields.Add(new Stack<bool>());
                    this.intFields.Add(null);
                    if (other.initFlags[i])
                    {
                        this.boolFields[i].Push(other.boolFields[i].Peek());
                        this.writtenFields.Push(i);
                        this.initFlags.Add(true);
                    }
                    else
                    {
                        this.initFlags.Add(false);
                    }
                }
                else
                {
                    throw new InvalidOperationException("Unexpected field data type!");
                }
            }

            this.isLocked = false;
        }

        /// <summary>
        /// Reference to the tileset that contains the declaration of the fields.
        /// </summary>
        private ITileSet tileset;

        /// <summary>
        /// List of the bool fields. Those indices are ignored that are not defined as booleans.
        /// </summary>
        private List<Stack<bool>> boolFields;

        /// <summary>
        /// List of the integer fields. Those indices are ignored that are not defined as integers.
        /// </summary>
        private List<Stack<int>> intFields;

        /// <summary>
        /// List of the written fields. Used for the undo operation.
        /// </summary>
        private Stack<int> writtenFields;

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
