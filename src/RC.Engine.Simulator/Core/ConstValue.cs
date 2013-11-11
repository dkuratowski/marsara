using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Represents a constant value that is readonly and cannot be changed.
    /// </summary>
    /// <typeparam name="T">The data type of the constant.</typeparam>
    class ConstValue<T> : IValueRead<T>
    {
        /// <summary>
        /// Constructs a ConstValue instance.
        /// </summary>
        /// <param name="value">The value of the constant.</param>
        public ConstValue(T value)
        {
            this.value = value;
        }

        /// <see cref="IValueRead<T>.Read"/>
        public T Read()
        {
            return this.value;
        }

        /// <summary>
        /// The value of the constant.
        /// </summary>
        private T value;
    }
}
