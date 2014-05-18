using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Common
{
    /// <summary>
    /// Using this structure you can automatically cache a value of any type.
    /// </summary>
    /// <typeparam name="T">The type of the data you want to cache.</typeparam>
    public struct CachedValue<T>
    {
        /// <summary>
        /// The prototype of the method that will be automatically called when the cached
        /// value is invalid and has to be recomputed.
        /// </summary>
        /// <returns>The new value to cache.</returns>
        public delegate T ValueSource();

        /// <summary>
        /// Constructs an instance of this struct.
        /// </summary>
        /// <param name="source">The source of the cached value.</param>
        public CachedValue(ValueSource source)
        {
            this.cachedValue = default(T);
            this.isValid = false;
            this.source = source;
        }

        /// <summary>
        /// Gets the cached value and recompute it when it's invalid.
        /// </summary>
        public T Value
        {
            get
            {
                if (!this.isValid)
                {
                    this.cachedValue = this.source != null ? this.source() : default(T);
                    this.isValid = true;
                }

                return this.cachedValue;
            }
        }

        /// <summary>
        /// Invalidates the cached value.
        /// </summary>
        public void Invalidate()
        {
            this.isValid = false;
        }

        /// <summary>
        /// The cached value.
        /// </summary>
        private T cachedValue;

        /// <summary>
        /// This flag indicates whether the cached value is valid or has to be recomputed.
        /// </summary>
        private bool isValid;

        /// <summary>
        /// The method that will be used to recompute the cached value when it's invalid.
        /// </summary>
        private ValueSource source;
    }
}
