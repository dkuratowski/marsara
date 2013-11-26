using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.PublicInterfaces
{
    /// <summary>
    /// The type of the members/references of HeapedObjects that must be synchronized with the simulation heap when
    /// it is attached.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the member. This can be any of the built-in types (byte, short, int, long, RCIntVector,
    /// RCIntRectangle, RCNumVector or RCNumRectangle) or any class that derives from HeapedObject.
    /// </typeparam>
    public abstract class HeapedValue<T> : IValue<T>
    {
        #region IValue<T> members

        /// <see cref="IValue<T>.Read"/>
        public abstract T Read();

        /// <see cref="IValue<T>.ValueChanged"/>
        public event EventHandler ValueChanged;

        /// <see cref="IValue<T>.Write"/>
        public abstract void Write(T newVal);

        #endregion IValue<T> members

        /// <summary>
        /// Raises the ValueChanged event if anybody is subscribed to it.
        /// </summary>
        protected internal void RaiseValueChangedEvt()
        {
            if (this.ValueChanged != null) { this.ValueChanged(this, null); }
        }
    }
}
