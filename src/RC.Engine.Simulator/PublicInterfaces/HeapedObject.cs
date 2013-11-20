using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.InternalInterfaces;
using RC.Engine.Simulator.Core;
using RC.Common;

namespace RC.Engine.Simulator.PublicInterfaces
{
    /// <summary>
    /// Common base class of objects that must be synchronized with the simulation heap when it is attached.
    /// </summary>
    public abstract class HeapedObject : IDisposable
    {
        /// <summary>
        /// Constructs a HeapedObject instance.
        /// </summary>
        public HeapedObject()
        {
            this.factoryHelper = ComponentManager.GetInterface<IHeapedObjectFactoryHelper>();
            
            this.isDisposed = false;
            this.ctorIndex = -1;

            this.inheritenceHierarchy = this.factoryHelper.GetInheritenceHierarchy(this.GetType().Name);
            this.heapedFields = new Dictionary<string, IHeapedFieldAccessor>[this.inheritenceHierarchy.Length];
            for (int i = 0; i < this.inheritenceHierarchy.Length; i++)
            {
                this.heapedFields[i] = new Dictionary<string, IHeapedFieldAccessor>();
                foreach (string fieldName in this.inheritenceHierarchy[i].FieldNames)
                {
                    if (fieldName != Constants.NAME_OF_BASE_TYPE_FIELD)
                    {
                        this.heapedFields[i].Add(fieldName, null);
                        if (this.ctorIndex == -1) { this.ctorIndex = i; }
                    }
                }
            }
            if (this.ctorIndex == -1) { throw new InvalidOperationException("Impossible case!"); }
        }

        #region Field construction methods for the constructors

        /// <summary>
        /// Call this method from the currently running constructor to construct the accessor object of the given field.
        /// </summary>
        /// <typeparam name="T">The data type of the field.</typeparam>
        /// <param name="name">The name of the field.</param>
        /// <returns>The accessor object of the field.</returns>
        protected HeapedValue<T> ConstructField<T>(string name)
        {
            if (this.isDisposed) { throw new ObjectDisposedException("HeapedObject"); }
            if (this.ctorIndex == this.inheritenceHierarchy.Length) { throw new InvalidOperationException("Last constructor already finished!"); }
            if (!this.heapedFields[this.ctorIndex].ContainsKey(name)) { throw new InvalidOperationException(string.Format("Field '{0}' doesn't exist in type '{1}'!", name, this.inheritenceHierarchy[this.ctorIndex].Name)); }
            if (this.heapedFields[this.ctorIndex][name] != null) { throw new InvalidOperationException(string.Format("Field '{0}' in type '{1}' already constructed!", name, this.inheritenceHierarchy[this.ctorIndex].Name)); }

            Type typeOfRequestedValue = typeof(T);

            HeapedValueImpl<T> retObj = null;
            IHeapType fieldType = this.factoryHelper.HeapManager.GetHeapType(this.inheritenceHierarchy[this.ctorIndex].GetFieldTypeID(name));
            if (fieldType.PointedTypeID != -1)
            {
                IHeapType pointedType = this.factoryHelper.HeapManager.GetHeapType(fieldType.PointedTypeID);
                if (pointedType.PointedTypeID != -1 || typeOfRequestedValue.Name != pointedType.Name) { throw new InvalidOperationException(string.Format("Field '{0}' in type '{1}' is not '{2}'!", name, this.inheritenceHierarchy[this.ctorIndex].Name, typeOfRequestedValue.Name)); }
                retObj = new HeapedValueImpl<T>();
            }
            else
            {
                if ((fieldType.BuiltInType == BuiltInTypeEnum.Byte && typeOfRequestedValue == TYPE_OF_BYTE)
                 || (fieldType.BuiltInType == BuiltInTypeEnum.Short && typeOfRequestedValue == TYPE_OF_SHORT)
                 || (fieldType.BuiltInType == BuiltInTypeEnum.Integer && typeOfRequestedValue == TYPE_OF_INT)
                 || (fieldType.BuiltInType == BuiltInTypeEnum.Long && typeOfRequestedValue == TYPE_OF_LONG)
                 || (fieldType.BuiltInType == BuiltInTypeEnum.Number && typeOfRequestedValue == TYPE_OF_NUMBER)
                 || (fieldType.BuiltInType == BuiltInTypeEnum.IntVector && typeOfRequestedValue == TYPE_OF_INTVECT)
                 || (fieldType.BuiltInType == BuiltInTypeEnum.NumVector && typeOfRequestedValue == TYPE_OF_NUMVECT)
                 || (fieldType.BuiltInType == BuiltInTypeEnum.IntRectangle && typeOfRequestedValue == TYPE_OF_INTRECT)
                 || (fieldType.BuiltInType == BuiltInTypeEnum.NumRectangle && typeOfRequestedValue == TYPE_OF_NUMRECT))
                {
                    retObj = new HeapedValueImpl<T>();
                }
                else
                {
                    throw new InvalidOperationException(string.Format("Type mismatch when constructing field '{0}' of type '{1}'!", name, this.inheritenceHierarchy[this.ctorIndex].Name));
                }
            }

            this.heapedFields[this.ctorIndex][name] = retObj;
            this.StepCtorIfNecessary();
            return retObj;
        }

        /// <summary>
        /// Call this method from the currently running constructor to construct the accessor object of the given array field.
        /// </summary>
        /// <typeparam name="T">The data type of the items in the array.</typeparam>
        /// <param name="name">The name of the field.</param>
        /// <returns>The accessor object of the field.</returns>
        protected HeapedArray<T> ConstructArrayField<T>(string name)
        {
            if (this.isDisposed) { throw new ObjectDisposedException("HeapedObject"); }
            if (this.ctorIndex == this.inheritenceHierarchy.Length) { throw new InvalidOperationException("Last constructor already finished!"); }
            if (!this.heapedFields[this.ctorIndex].ContainsKey(name)) { throw new InvalidOperationException(string.Format("Field '{0}' doesn't exist in type '{1}'!", name, this.inheritenceHierarchy[this.ctorIndex].Name)); }
            if (this.heapedFields[this.ctorIndex][name] != null) { throw new InvalidOperationException(string.Format("Field '{0}' in type '{1}' already constructed!", name, this.inheritenceHierarchy[this.ctorIndex].Name)); }

            Type typeOfRequestedValue = typeof(T);

            HeapedArrayImpl<T> retObj = null;
            IHeapType fieldType = this.factoryHelper.HeapManager.GetHeapType(this.inheritenceHierarchy[this.ctorIndex].GetFieldTypeID(name));
            if (fieldType.PointedTypeID == -1) { throw new InvalidOperationException(string.Format("Field '{0}' of type '{1}' is not an array!", name, this.inheritenceHierarchy[this.ctorIndex].Name)); }

            IHeapType pointedType = this.factoryHelper.HeapManager.GetHeapType(fieldType.PointedTypeID);
            if (pointedType.PointedTypeID != -1)
            {
                IHeapType arrayItemType = this.factoryHelper.HeapManager.GetHeapType(pointedType.PointedTypeID);
                if (arrayItemType.PointedTypeID != -1 || typeOfRequestedValue.Name != arrayItemType.Name) { throw new InvalidOperationException(string.Format("Array field '{0}' in type '{1}' is not '{2}'!", name, this.inheritenceHierarchy[this.ctorIndex].Name, typeOfRequestedValue.Name)); }
                retObj = new HeapedArrayImpl<T>();
            }
            else
            {
                if ((pointedType.BuiltInType == BuiltInTypeEnum.Byte && typeOfRequestedValue == TYPE_OF_BYTE)
                 || (pointedType.BuiltInType == BuiltInTypeEnum.Short && typeOfRequestedValue == TYPE_OF_SHORT)
                 || (pointedType.BuiltInType == BuiltInTypeEnum.Integer && typeOfRequestedValue == TYPE_OF_INT)
                 || (pointedType.BuiltInType == BuiltInTypeEnum.Long && typeOfRequestedValue == TYPE_OF_LONG)
                 || (pointedType.BuiltInType == BuiltInTypeEnum.Number && typeOfRequestedValue == TYPE_OF_NUMBER)
                 || (pointedType.BuiltInType == BuiltInTypeEnum.IntVector && typeOfRequestedValue == TYPE_OF_INTVECT)
                 || (pointedType.BuiltInType == BuiltInTypeEnum.NumVector && typeOfRequestedValue == TYPE_OF_NUMVECT)
                 || (pointedType.BuiltInType == BuiltInTypeEnum.IntRectangle && typeOfRequestedValue == TYPE_OF_INTRECT)
                 || (pointedType.BuiltInType == BuiltInTypeEnum.NumRectangle && typeOfRequestedValue == TYPE_OF_NUMRECT))
                {
                    retObj = new HeapedArrayImpl<T>();
                }
                else
                {
                    throw new InvalidOperationException(string.Format("Type mismatch when constructing array field '{0}' of type '{1}'!", name, this.inheritenceHierarchy[this.ctorIndex].Name));
                }
            }

            this.heapedFields[this.ctorIndex][name] = retObj;
            this.StepCtorIfNecessary();
            return retObj;
        }

        /// <summary>
        /// Steps the constructor index if necessary.
        /// </summary>
        private void StepCtorIfNecessary()
        {
            /// Step the constructor index until we found a field that is not yet constructed.
            while (this.ctorIndex < this.inheritenceHierarchy.Length)
            {
                foreach (KeyValuePair<string, IHeapedFieldAccessor> item in this.heapedFields[this.ctorIndex])
                {
                    if (item.Value == null) { return; }
                }
                this.ctorIndex++;
            }
            
            /// If we reached the end, turn on all the field accessor objects.
            for (int i = 0; i < this.inheritenceHierarchy.Length; i++)
            {
                foreach (IHeapedFieldAccessor fieldAccessor in this.heapedFields[i].Values)
                {
                    fieldAccessor.ReadyToUse();
                }
            }
        }

        #endregion Field construction methods for the constructors

        #region IDisposable methods

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            if (this.ctorIndex != this.inheritenceHierarchy.Length) { throw new InvalidOperationException("Last constructor not yet finished!"); }
            if (!this.isDisposed)
            {
                /// TODO: put disposing code here!
                for (int i = 0; i < this.inheritenceHierarchy.Length; i++)
                {
                    foreach (IHeapedFieldAccessor fieldAccessor in this.heapedFields[i].Values)
                    {
                        fieldAccessor.Dispose();
                    }
                }
                this.isDisposed = true;
            }
        }

        #endregion IDisposable methods

        /// <summary>
        /// The index of the currently running constructor.
        /// </summary>
        private int ctorIndex;

        /// <summary>
        /// The heaped fields of this HeapedObject mapped by their names and grouped by the classes in the inheritence hierarchy
        /// starting from the base class.
        /// </summary>
        private Dictionary<string, IHeapedFieldAccessor>[] heapedFields;

        /// <summary>
        /// List of the composite heap types representing this object on the simulation heap starting from the base class.
        /// </summary>
        private IHeapType[] inheritenceHierarchy;

        /// <summary>
        /// Reference to the factory helper component.
        /// </summary>
        private IHeapedObjectFactoryHelper factoryHelper;

        /// <summary>
        /// Indicates whether this object has already been disposed or not.
        /// </summary>
        private bool isDisposed;

        /// <summary>
        /// The runtime type of the built-in types.
        /// </summary>
        private static readonly Type TYPE_OF_BYTE = typeof(byte);
        private static readonly Type TYPE_OF_SHORT = typeof(short);
        private static readonly Type TYPE_OF_INT = typeof(int);
        private static readonly Type TYPE_OF_LONG = typeof(long);
        private static readonly Type TYPE_OF_NUMBER = typeof(RCNumber);
        private static readonly Type TYPE_OF_INTVECT = typeof(RCIntVector);
        private static readonly Type TYPE_OF_NUMVECT = typeof(RCNumVector);
        private static readonly Type TYPE_OF_INTRECT = typeof(RCIntRectangle);
        private static readonly Type TYPE_OF_NUMRECT = typeof(RCNumRectangle);
    }
}
