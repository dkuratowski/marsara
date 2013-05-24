using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Common
{
    /// <summary>
    /// Represents a 32-bit signed fixed point number.
    /// </summary>
    public struct RCNumber
    {
        #region Operator overloads

        #region Arithmetic operators

        /// <summary>
        /// Adds two RCNumbers.
        /// </summary>
        /// <param name="lNum">The left hand operand of the addition.</param>
        /// <param name="rNum">The right hand operand of the addition.</param>
        /// <returns>The result of the addition.</returns>
        public static RCNumber operator +(RCNumber lNum, RCNumber rNum)
        {
            return new RCNumber(lNum.rawValue + rNum.rawValue);
        }

        /// <summary>
        /// Substracts an RCNumber from another RCNumber.
        /// </summary>
        /// <param name="lNum">The left hand operand of the substraction.</param>
        /// <param name="rNum">The right hand operand of the substraction.</param>
        /// <returns>The result of the substraction.</returns>
        public static RCNumber operator -(RCNumber lNum, RCNumber rNum)
        {
            return new RCNumber(lNum.rawValue - rNum.rawValue);
        }

        /// <summary>
        /// Creates the opposite of the given RCNumber.
        /// </summary>
        /// <param name="num">The RCNumber.</param>
        /// <returns>The opposite of the given RCNumber.</returns>
        public static RCNumber operator -(RCNumber num)
        {
            return new RCNumber(-num.rawValue);
        }

        /// <summary>
        /// Multiplies two RCNumbers.
        /// </summary>
        /// <param name="lNum">The left hand operand of the multiplication.</param>
        /// <param name="rNum">The right hand operand of the multiplication.</param>
        /// <returns>The result of the multiplication.</returns>
        public static RCNumber operator *(RCNumber lNum, RCNumber rNum)
        {
            return new RCNumber((lNum.rawValue * rNum.rawValue) >> FRACTION_WIDTH);
        }

        /// <summary>
        /// Divides an RCNumber with another RCNumber.
        /// </summary>
        /// <param name="lNum">The left hand operand of the division.</param>
        /// <param name="rNum">The right hand operand of the division.</param>
        /// <returns>The result of the division.</returns>
        public static RCNumber operator /(RCNumber lNum, RCNumber rNum)
        {
            return new RCNumber(((lNum.rawValue << DIVISION_SHIFT) / rNum.rawValue) >> (DIVISION_SHIFT - FRACTION_WIDTH));
        }
        
        #endregion Arithmetic operators

        #region Comparision operators

        /// <summary>
        /// Compares two RCNumbers.
        /// </summary>
        /// <param name="lNum">A RCNumber to compare.</param>
        /// <param name="rNum">A RCNumber to compare.</param>
        /// <returns>True if the two RCNumbers are equal, false otherwise.</returns>
        public static bool operator ==(RCNumber lNum, RCNumber rNum)
        {
            return lNum.Equals(rNum);
        }

        /// <summary>
        /// Compares two RCNumbers.
        /// </summary>
        /// <param name="lNum">A RCNumber to compare.</param>
        /// <param name="rNum">A RCNumber to compare.</param>
        /// <returns>True if the two RCNumbers are not equal, false otherwise.</returns>
        public static bool operator !=(RCNumber lNum, RCNumber rNum)
        {
            return !lNum.Equals(rNum);
        }

        /// <summary>
        /// Compares two RCNumbers.
        /// </summary>
        /// <param name="lNum">A RCNumber to compare.</param>
        /// <param name="rNum">A RCNumber to compare.</param>
        /// <returns>True if the first RCNumber is less than or equals with the second RCNumber, false otherwise.</returns>
        public static bool operator <=(RCNumber lNum, RCNumber rNum)
        {
            return lNum.rawValue <= rNum.rawValue;
        }

        /// <summary>
        /// Compares two RCNumbers.
        /// </summary>
        /// <param name="lNum">A RCNumber to compare.</param>
        /// <param name="rNum">A RCNumber to compare.</param>
        /// <returns>True if the first RCNumber is greater than or equals with the second RCNumber, false otherwise.</returns>
        public static bool operator >=(RCNumber lNum, RCNumber rNum)
        {
            return lNum.rawValue >= rNum.rawValue;
        }

        /// <summary>
        /// Compares two RCNumbers.
        /// </summary>
        /// <param name="lNum">A RCNumber to compare.</param>
        /// <param name="rNum">A RCNumber to compare.</param>
        /// <returns>True if the first RCNumber is less than the second RCNumber, false otherwise.</returns>
        public static bool operator <(RCNumber lNum, RCNumber rNum)
        {
            return lNum.rawValue < rNum.rawValue;
        }

        /// <summary>
        /// Compares two RCNumbers.
        /// </summary>
        /// <param name="lNum">A RCNumber to compare.</param>
        /// <param name="rNum">A RCNumber to compare.</param>
        /// <returns>True if the first RCNumber is greater than the second RCNumber, false otherwise.</returns>
        public static bool operator >(RCNumber lNum, RCNumber rNum)
        {
            return lNum.rawValue > rNum.rawValue;
        }
        
        #endregion Comparision operators

        #region Cast operators

        /// <summary>
        /// Implicit casting from long to RCNumber.
        /// </summary>
        /// <param name="n">The long value to be casted.</param>
        /// <returns>The result RCNumber.</returns>
        public static implicit operator RCNumber(long n)
        {
            return new RCNumber(n << FRACTION_WIDTH);
        }

        /// <summary>
        /// Explicit casting from RCNumber to int.
        /// </summary>
        /// <param name="n">The number to be casted.</param>
        /// <returns>The result value is floor(n).</returns>
        public static explicit operator int(RCNumber n)
        {
            return n.CastedValueCache.Value;
        }

        #endregion Cast operators

        #endregion Operator overloads

        #region Public fields

        /// <summary>
        /// Constructs an RCNumber initialized with the given raw value.
        /// </summary>
        /// <param name="bits">The raw value that contains the bits of the RCNumber.</param>
        public RCNumber(long bits)
        {
            this.rawValue = bits;
            this.roundedValueCache = default(CachedValue<int>);
            this.castedValueCache = default(CachedValue<int>);
            this.roundedValueCacheCreated = false;
            this.castedValueCacheCreated = false;
        }

        /// <summary>
        /// Checks whether the specified object is an RCNumber and has the same value as this RCNumber.
        /// </summary>
        /// <param name="obj">The object to test.</param>
        /// <returns>True if obj is an RCNumber and has the same value as this RCNumber.</returns>
        public override bool Equals(object obj)
        {
            return (obj is RCNumber) && Equals((RCNumber)obj);
        }

        /// <summary>
        /// Checks whether this RCNumber contains the same value as the specified RCNumber.
        /// </summary>
        /// <param name="other">The RCNumber to test.</param>
        /// <returns>True if other RCNumber has the same value as this RCNumber.</returns>
        public bool Equals(RCNumber other)
        {
            return this.rawValue == other.rawValue;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return this.rawValue.GetHashCode();
        }

        /// <summary>
        /// Gets the string representation of this RCNumber.
        /// </summary>
        /// <returns>The string representation of this RCNumber.</returns>
        public override string ToString()
        {
            long absValue = Math.Abs(this.rawValue);
            long integerPart = (absValue & INTEGER_MASK) >> FRACTION_WIDTH;
            long fractionPart = absValue & FRACTION_MASK;
            return string.Format(this.rawValue < 0 ? FORMAT_STRING_NEG : FORMAT_STRING_POS, integerPart, fractionPart * DISPLAYED_FRACTION_MAX / FRACTION_MAX);
        }

        /// <summary>
        /// Rounds this RCNumber to the nearest integer.
        /// </summary>
        /// <returns>
        /// The result value is floor(n) if n - floor(n) is less than 0.5. Otherwise the return value is ceiling(n).
        /// </returns>
        public int Round()
        {
            return this.RoundedValueCache.Value;
        }

        /// <summary>
        /// Gets the bits of this RCNumber.
        /// </summary>
        public int Bits { get { return (int)this.rawValue; } }

        #endregion Public fields

        #region Private fields

        /// <summary>
        /// Workaround: structs doesn't allow to define parameterless constructors, so we have to guarantee that
        /// the cache is created latest when first needed.
        /// </summary>
        private CachedValue<int> RoundedValueCache
        {
            get
            {
                if (!this.roundedValueCacheCreated)
                {
                    long rawValue = this.rawValue;
                    this.roundedValueCache = new CachedValue<int>(
                        delegate()
                        {
                            long absValue = Math.Abs(rawValue);
                            bool isNegative = rawValue < 0;
                            long integerPart = (absValue & INTEGER_MASK) >> FRACTION_WIDTH;
                            long fractionPartShifted = (absValue & FRACTION_MASK) >> (FRACTION_WIDTH - 1);
                            if (fractionPartShifted == 0)
                            {
                                return isNegative ? -(int)integerPart : (int)integerPart;
                            }
                            else
                            {
                                return isNegative ? -((int)integerPart + 1) : ((int)integerPart + 1);
                            }
                        });

                    this.roundedValueCacheCreated = true;
                }

                return this.roundedValueCache;
            }
        }

        /// <summary>
        /// Workaround: structs doesn't allow to define parameterless constructors, so we have to guarantee that
        /// the cache is created latest when first needed.
        /// </summary>
        private CachedValue<int> CastedValueCache
        {
            get
            {
                if (!this.castedValueCacheCreated)
                {
                    long rawValue = this.rawValue;
                    this.castedValueCache = new CachedValue<int>(
                        delegate()
                        {
                            return (int)(rawValue >> FRACTION_WIDTH);
                        });

                    this.castedValueCacheCreated = true;
                }

                return this.castedValueCache;
            }
        }

        /// <summary>
        /// The underlying bitfield that stores the bits of the RCNumber.
        /// </summary>
        private long rawValue;

        /// <summary>
        /// Cache for the rounded value of the RCNumber.
        /// </summary>
        private CachedValue<int> roundedValueCache;

        /// <summary>
        /// This flag indicates whether the rounded value cache has already been created or not.
        /// </summary>
        private bool roundedValueCacheCreated;

        /// <summary>
        /// Cache for the integer value casted from the RCNumber.
        /// </summary>
        private CachedValue<int> castedValueCache;

        /// <summary>
        /// This flag indicates whether the casted value cache has already been created or not.
        /// </summary>
        private bool castedValueCacheCreated;

        /// <summary>
        /// The total number of the bits of RCNumbers.
        /// </summary>
        private const int TOTAL_WIDTH = 64;

        /// <summary>
        /// The number of the bits in the fraction part of RCNumbers.
        /// </summary>
        private const int FRACTION_WIDTH = 10;

        /// <summary>
        /// Number of the displayed decimal digits.
        /// </summary>
        private const int DISPLAYED_FRACTION_DECIMAL_DIGITS = 3;

        /// <summary>
        /// The number of the bits in the integer part of RCNumbers.
        /// </summary>
        private const int INTEGER_WIDTH = TOTAL_WIDTH - FRACTION_WIDTH;

        /// <summary>
        /// The maximum value of the fraction part.
        /// </summary>
        private static readonly int FRACTION_MAX = (int)Math.Pow(2, FRACTION_WIDTH);

        /// <summary>
        /// The format string for the RCNumber.ToString method in case of positive numbers.
        /// </summary>
        private static readonly string FORMAT_STRING_POS = string.Format("{{0}}.{{1:d{0}}}", DISPLAYED_FRACTION_DECIMAL_DIGITS);

        /// <summary>
        /// The format string for the RCNumber.ToString method in case of negative numbers.
        /// </summary>
        private static readonly string FORMAT_STRING_NEG = string.Format("-{{0}}.{{1:d{0}}}", DISPLAYED_FRACTION_DECIMAL_DIGITS);

        /// <summary>
        /// The maximum value of the decimal form of the fraction part.
        /// </summary>
        private static readonly int DISPLAYED_FRACTION_MAX = (int)Math.Pow(10, DISPLAYED_FRACTION_DECIMAL_DIGITS);

        /// <summary>
        /// Bitmask for the integer part.
        /// </summary>
        private const long INTEGER_MASK = ~0L << FRACTION_WIDTH;

        /// <summary>
        /// Bitmask for the fraction part.
        /// </summary>
        private const long FRACTION_MASK = ~INTEGER_MASK;

        /// <summary>
        /// Number of bits shifted in a division operation.
        /// </summary>
        private const int DIVISION_SHIFT = TOTAL_WIDTH / 2;

        #endregion Private fields
    }
}
