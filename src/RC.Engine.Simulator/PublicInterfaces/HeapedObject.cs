using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.Diagnostics;
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
        private static int objectCount = 0; // TODO: remove

        /// <summary>
        /// Constructs a HeapedObject instance.
        /// </summary>
        public HeapedObject()
        {
            this.heapManager = ComponentManager.GetInterface<IHeapManagerInternals>();
            
            this.isDisposed = false;
            this.ctorIndex = -1;
            this.heapConnectors = null;

            this.inheritenceHierarchy = this.heapManager.GetInheritenceHierarchy(this.GetType().Name);
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

            this.heapManager.AttachingHeapedObjects += this.OnAttachingHeapedObject;
            this.heapManager.SynchronizingHeapedObjects += this.OnSynchronizingFields;
            this.heapManager.DetachingHeapedObjects += this.OnDetachingHeapedObject;

            // TODO: remove
            //objectCount++;
            //TraceManager.WriteAllTrace(string.Format("HeapedObject_Create -> Count = {0}", objectCount), TraceFilters.INFO);
        }

        /// <summary>
        /// Gets the heap connectors of this object on the simulation heap starting from the base class.
        /// </summary>
        internal IEnumerable<IHeapConnector> HeapConnectors { get { return this.heapConnectors; } }

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
            IHeapType fieldType = this.heapManager.GetHeapType(this.inheritenceHierarchy[this.ctorIndex].GetFieldTypeID(name));
            if (fieldType.PointedTypeID != -1)
            {
                IHeapType pointedType = this.heapManager.GetHeapType(fieldType.PointedTypeID);
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
            IHeapType fieldType = this.heapManager.GetHeapType(this.inheritenceHierarchy[this.ctorIndex].GetFieldTypeID(name));
            if (fieldType.PointedTypeID == -1) { throw new InvalidOperationException(string.Format("Field '{0}' of type '{1}' is not an array!", name, this.inheritenceHierarchy[this.ctorIndex].Name)); }

            IHeapType pointedType = this.heapManager.GetHeapType(fieldType.PointedTypeID);
            if (pointedType.PointedTypeID != -1)
            {
                IHeapType arrayItemType = this.heapManager.GetHeapType(pointedType.PointedTypeID);
                if (arrayItemType.PointedTypeID != -1 || typeOfRequestedValue.Name != arrayItemType.Name) { throw new InvalidOperationException(string.Format("Array field '{0}' in type '{1}' is not '{2}'!", name, this.inheritenceHierarchy[this.ctorIndex].Name, typeOfRequestedValue.Name)); }
                retObj = new HeapedArrayImpl<T>(this.heapManager);
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
                    retObj = new HeapedArrayImpl<T>(this.heapManager);
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

            /// Attach this instance to the heap if necessary.
            if (this.heapManager.IsHeapAttached)
            {
                this.AttachToHeap();
                this.SynchToHeap();
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
                /// Execute custom disposal procedures of the derived classes.
                this.DisposeImpl();

                /// Detach from the heap if necessary.
                if (this.heapManager.IsHeapAttached) { this.DetachFromHeap(); }

                for (int i = 0; i < this.inheritenceHierarchy.Length; i++)
                {
                    foreach (IHeapedFieldAccessor fieldAccessor in this.heapedFields[i].Values)
                    {
                        fieldAccessor.Dispose();
                    }
                }
                this.heapManager.AttachingHeapedObjects -= this.OnAttachingHeapedObject;
                this.heapManager.SynchronizingHeapedObjects -= this.OnSynchronizingFields;
                this.heapManager.DetachingHeapedObjects -= this.OnDetachingHeapedObject;
                this.isDisposed = true;
            }

            // TODO: remove
            //objectCount--;
            //TraceManager.WriteAllTrace(string.Format("HeapedObject_Destroy -> Count = {0}", objectCount), TraceFilters.INFO);
        }

        /// <summary>
        /// The derived classes can implement custom disposal procedures by overriding this method.
        /// The default implementation does nothing.
        /// </summary>
        protected virtual void DisposeImpl() { }

        #endregion IDisposable methods

        #region Heap event handlers

        /// <summary>
        /// Handler of the IHeapConnectionService.AttachingHeapedObjects event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">The arguments of the event.</param>
        private void OnAttachingHeapedObject(object sender, EventArgs args)
        {
            if (this.isDisposed) { throw new ObjectDisposedException("HeapedObject"); }
            if (this.ctorIndex != this.inheritenceHierarchy.Length) { throw new InvalidOperationException("Not every fields have been constructed yet!"); }
            if (this.heapConnectors != null) { throw new InvalidOperationException("Heaped object already attached to the simulation heap!"); }

            this.AttachToHeap();
        }

        /// <summary>
        /// Handler of the IHeapConnectionService.SynchronizingHeapedObjects event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">The arguments of the event.</param>
        private void OnSynchronizingFields(object sender, EventArgs args)
        {
            if (this.isDisposed) { throw new ObjectDisposedException("HeapedObject"); }
            if (this.ctorIndex != this.inheritenceHierarchy.Length) { throw new InvalidOperationException("Not every fields have been constructed yet!"); }

            this.SynchToHeap();
        }

        /// <summary>
        /// Handler of the IHeapConnectionService.DetachingHeapedObjects event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">The arguments of the event.</param>
        private void OnDetachingHeapedObject(object sender, EventArgs args)
        {
            if (this.isDisposed) { throw new ObjectDisposedException("HeapedObject"); }
            if (this.ctorIndex != this.inheritenceHierarchy.Length) { throw new InvalidOperationException("Not every fields have been constructed yet!"); }
            if (this.heapConnectors == null) { throw new InvalidOperationException("Heaped object not attached to the simulation heap!"); }

            this.DetachFromHeap();
        }

        /// <summary>
        /// Attaches this instance to the heap.
        /// </summary>
        private void AttachToHeap()
        {
            /// Construct the data structure on the heap for this object.
            this.heapConnectors = new IHeapConnector[this.inheritenceHierarchy.Length];
            this.heapConnectors[this.inheritenceHierarchy.Length - 1] = this.heapManager.New(this.inheritenceHierarchy[this.inheritenceHierarchy.Length - 1].ID);

            /// Create heap connectors for every level in the inheritence hierarchy.
            for (int i = this.inheritenceHierarchy.Length - 1; i >= 0; i--)
            {
                /// Create the heap connector for current level.
                if (i != this.inheritenceHierarchy.Length - 1)
                {
                    int baseFieldIdx = this.inheritenceHierarchy[i + 1].GetFieldIdx(Constants.NAME_OF_BASE_TYPE_FIELD);
                    this.heapConnectors[i] = this.heapConnectors[i + 1].AccessField(baseFieldIdx);
                }
            }
        }

        /// <summary>
        /// Synchronizes the fields of this instance to the heap.
        /// </summary>
        private void SynchToHeap()
        {
            /// Create the heap connectors for the fields of every level in the inheritence hierarchy.
            for (int i = this.inheritenceHierarchy.Length - 1; i >= 0; i--)
            {
                foreach (KeyValuePair<string, IHeapedFieldAccessor> field in this.heapedFields[i])
                {
                    field.Value.AttachToHeap(this.heapConnectors[i].AccessField(this.inheritenceHierarchy[i].GetFieldIdx(field.Key)));
                }
            }
        }

        /// <summary>
        /// Detaches this instance from the heap.
        /// </summary>
        private void DetachFromHeap()
        {
            /// Detach the field accessors from the heap.
            for (int i = this.inheritenceHierarchy.Length - 1; i >= 0; i--)
            {
                foreach (KeyValuePair<string, IHeapedFieldAccessor> field in this.heapedFields[i])
                {
                    field.Value.DetachFromHeap();
                }
            }
            this.heapConnectors[this.inheritenceHierarchy.Length - 1].Delete();
        }

        #endregion Heap event handlers

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
        /// Reference to the heap manager.
        /// </summary>
        private IHeapManagerInternals heapManager;

        /// <summary>
        /// Reference to the heap connectors of this heaped object starting from the base class if this heaped object is
        /// connected to the heap; otherwise null.
        /// </summary>
        private IHeapConnector[] heapConnectors;

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
