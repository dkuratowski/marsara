using RC.Common;
using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Metadata.Core
{
    /// <summary>
    /// Represents a value modifier that can be used for upgrading a value in the metadata.
    /// </summary>
    /// <typeparam name="T">The type of the value to be modified.</typeparam>
    abstract class ValueModifier<T> : IValueRead<T>
    {
        /// <see cref="IValueRead<T>.Read"/>
        public T Read()
        {
            if (this.modifiedValue == null) { return this.modification; }
            return this.CalculateModificationResult();
        }

        /// <summary>
        /// Gets or sets the value of the modification.
        /// </summary>
        public T Modification
        {
            get { return this.modification; }
            set { this.modification = value; }
        }

        /// <summary>
        /// Attaches the given value to be modified by this modifier.
        /// </summary>
        /// <param name="modifiedValue">The value to attach.</param>
        public void AttachModifiedValue(IValueRead<T> modifiedValue) { this.modifiedValue = modifiedValue; }

        /// <summary>
        /// Gets whether this value modifier has an attached modified value.
        /// </summary>
        /// <returns>True if this value modifier has an attached modified value; otherwise false.</returns>
        public bool HasAttachedModifiedValue() { return this.modifiedValue != null; }

        /// <summary>
        /// Constructs a ValueModifier instance.
        /// </summary>
        protected ValueModifier()
        {
            this.modifiedValue = null;
            this.modification = default(T);
        }

        /// <summary>
        /// Gets the value to be modified.
        /// </summary>
        protected IValueRead<T> ModifiedValue { get { return this.modifiedValue; } }

        /// <summary>
        /// Calculates the result of the modification.
        /// </summary>
        /// <returns>The calculated result of the modification.</returns>
        protected abstract T CalculateModificationResult();

        /// <summary>
        /// The value to be modified.
        /// </summary>
        private IValueRead<T> modifiedValue;

        /// <summary>
        /// The value of the modification.
        /// </summary>
        private T modification;
    }

    /// <summary>
    /// Value modifier implementation for integers.
    /// </summary>
    class IntValueModifier : ValueModifier<int>
    {
        /// <see cref="ValueModifier<T>.CalculateModificationResult"/>
        protected override int CalculateModificationResult()
        {
            return Math.Max(0, this.ModifiedValue.Read() + this.Modification);
        }
    }

    /// <summary>
    /// Value modifier implementation for integers.
    /// </summary>
    class NumberValueModifier : ValueModifier<RCNumber>
    {
        /// <see cref="ValueModifier<T>.CalculateModificationResult"/>
        protected override RCNumber CalculateModificationResult()
        {
            RCNumber calculatedResult = this.ModifiedValue.Read() + this.Modification;
            return calculatedResult > 0 ? calculatedResult : 0;
        }
    }
}
