using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.PublicInterfaces
{
    /// <summary>
    /// The type of the array members of HeapedObjects that must be synchronized with the simulation heap when it is
    /// attached.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the array items. This can be any of the built-in types (byte, short, int, long, RCIntVector,
    /// RCIntRectangle, RCNumVector or RCNumRectangle) or any class that derives from HeapedObject.
    /// </typeparam>
    public abstract class HeapedArray<T> : IEnumerable<IValue<T>>
    {
        /// <summary>
        /// Gets a value from this array at the given index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public abstract IValue<T> this[int index] { get; }

        /// <summary>
        /// Deletes the current array and creates a new one with the given length.
        /// </summary>
        /// <param name="length">The length of the new array.</param>
        public abstract void New(int length);

        #region IEnumerable<IValue<T>> methods

        /// <see cref="IEnumerable<IValue<T>>.GetEnumerator"/>
        public IEnumerator<IValue<T>> GetEnumerator()
        {
            return this.GetEnumeratorImpl();
        }

        /// <see cref="IEnumerable.GetEnumerator"/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumeratorImpl();
        }

        /// <summary>
        /// Keep compiler happy...
        /// </summary>
        protected abstract IEnumerator<IValue<T>> GetEnumeratorImpl();

        #endregion IEnumerable<IValue<T>> methods
    }
}
