using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Common
{
    /// <summary>
    /// Represents an ordered pair of fixed point x- and y-coordinates that defines a vector or point in a 2D plane.
    /// </summary>
    public struct RCNumVector
    {
        #region Operator overloads

        #region Arithmetic operators

        /// <summary>
        /// Translates a RCNumVector with a given RCNumVector.
        /// </summary>
        /// <param name="lVect">The RCNumVector to translate.</param>
        /// <param name="rVect">The RCNumVector to translate with.</param>
        /// <returns>The translated RCNumVector.</returns>
        public static RCNumVector operator +(RCNumVector lVect, RCNumVector rVect)
        {
            if (!lVect.isDefined) { throw new ArgumentNullException("lVect"); }
            if (!rVect.isDefined) { throw new ArgumentNullException("rVect"); }
            return new RCNumVector(lVect.x + rVect.x, lVect.y + rVect.y);
        }

        /// <summary>
        /// Translates a RCNumVector with the negative of a given RCNumVector.
        /// </summary>
        /// <param name="lVect">The RCNumVector to translate.</param>
        /// <param name="rVect">The RCNumVector to translate with.</param>
        /// <returns>The translated RCNumVector.</returns>
        public static RCNumVector operator -(RCNumVector lVect, RCNumVector rVect)
        {
            if (!lVect.isDefined) { throw new ArgumentNullException("lVect"); }
            if (!rVect.isDefined) { throw new ArgumentNullException("rVect"); }
            return new RCNumVector(lVect.x - rVect.x, lVect.y - rVect.y);
        }

        /// <summary>
        /// Scales an RCNumVector with a given factor.
        /// </summary>
        /// <param name="vect">The RCNumVector to scale.</param>
        /// <param name="fact">The scaling factor.</param>
        /// <returns>The scaled RCIntVector.</returns>
        public static RCNumVector operator *(RCNumVector vect, RCNumber fact)
        {
            if (!vect.isDefined) { throw new ArgumentNullException("vect"); }
            return new RCNumVector(vect.x * fact, vect.y * fact);
        }

        /// <summary>
        /// Computes the product of two given RCNumVector.
        /// </summary>
        /// <param name="lVect">The first RCNumVector.</param>
        /// <param name="rVect">The second RCNumVector.</param>
        /// <returns>
        /// The X coordinate of the result RCNumVector will be lVect.X * lVect.X.
        /// The Y coordinate of the result RCNumVector will be lVect.Y * lVect.Y.
        /// </returns>
        public static RCNumVector operator *(RCNumVector lVect, RCNumVector rVect)
        {
            if (!lVect.isDefined) { throw new ArgumentNullException("lVect"); }
            if (!rVect.isDefined) { throw new ArgumentNullException("rVect"); }
            return new RCNumVector(lVect.X * rVect.X, lVect.Y * rVect.Y);
        }

        /// <summary>
        /// Divides an RCNumVector with a given number.
        /// </summary>
        /// <param name="vect">The RCNumVector to divide.</param>
        /// <param name="divisor">The divisor.</param>
        /// <returns>The coordinates of vect will be divided with divisor.</returns>
        public static RCNumVector operator /(RCNumVector vect, RCNumber divisor)
        {
            if (!vect.isDefined) { throw new ArgumentNullException("vect"); }
            if (divisor == 0) { throw new DivideByZeroException(); }
            return new RCNumVector(vect.x / divisor, vect.y / divisor);
        }

        /// <summary>
        /// Compares two RCNumVector objects. The result specifies whether the values of the X and Y properties of the two RCNumVector objects are equal.
        /// </summary>
        /// <param name="lVect">A RCNumVector to compare.</param>
        /// <param name="rVect">A RCNumVector to compare.</param>
        /// <returns>True if the X and Y values of lVect and rVect are equal, false otherwise.</returns>
        public static bool operator ==(RCNumVector lVect, RCNumVector rVect)
        {
            return lVect.Equals(rVect);
        }

        /// <summary>
        /// Compares two RCNumVector objects. The result specifies whether the values of the X and Y properties of the two RCNumVector objects are unequal.
        /// </summary>
        /// <param name="lVect">A RCNumVector to compare.</param>
        /// <param name="rVect">A RCNumVector to compare.</param>
        /// <returns>True if the X and Y values of lVect and rVect differ, false otherwise.</returns>
        public static bool operator !=(RCNumVector lVect, RCNumVector rVect)
        {
            return !lVect.Equals(rVect);
        }

        #endregion Arithmetic operators

        #region Cast operators

        /// <summary>
        /// Implicit casting from RCIntVector to RCNumVector.
        /// </summary>
        /// <param name="vect">The integer vector to be casted.</param>
        /// <returns>The result RCNumVector.</returns>
        public static implicit operator RCNumVector(RCIntVector vect)
        {
            if (vect == RCIntVector.Undefined) { throw new ArgumentNullException("vect"); }
            return new RCNumVector(vect.X, vect.Y);
        }

        /// <summary>
        /// Explicit casting from RCNumVector to RCIntVector.
        /// </summary>
        /// <param name="vect">The RCNumVector to be casted.</param>
        /// <returns>The components of the result vector are the floor of the components of the input vector.</returns>
        public static explicit operator RCIntVector(RCNumVector vect)
        {
            if (vect == RCNumVector.Undefined) { throw new ArgumentNullException("vect"); }
            return new RCIntVector((int)vect.X, (int)vect.Y);
        }

        #endregion Cast operators

        #endregion Operator overloads

        #region Public fields

        /// <summary>
        /// Initializes a new RCNumVector with the specified coordinates.
        /// </summary>
        /// <param name="x">The horizontal coordinate of the RCNumVector.</param>
        /// <param name="y">The vertical coordinate of the RCNumVector.</param>
        public RCNumVector(RCNumber x, RCNumber y)
        {
            this.isDefined = true;
            this.x = x;
            this.y = y;
            this.lengthCache = default(CachedValue<RCNumber>);
            this.lengthCacheCreated = false;
        }

        /// <summary>
        /// Initializes a new RCNumVector with the specified RCNumVector.
        /// </summary>
        /// <param name="other">The RCNumVector to initialize with.</param>
        public RCNumVector(RCNumVector other)
        {
            if (!other.isDefined) { throw new ArgumentNullException("other"); }
            this.isDefined = true;
            this.x = other.x;
            this.y = other.y;
            this.lengthCache = default(CachedValue<RCNumber>);
            this.lengthCacheCreated = false;
        }

        /// <summary>
        /// Checks whether the specified object is an RCNumVector and contains the same coordinates as this RCNumVector.
        /// </summary>
        /// <param name="obj">The object to test.</param>
        /// <returns>True if obj is a RCNumVector and has the same coordinates as this RCNumVector.</returns>
        public override bool Equals(object obj)
        {
            return (obj is RCNumVector) && Equals((RCNumVector)obj);
        }

        /// <summary>
        /// Checks whether this RCNumVector contains the same coordinates as the specified RCNumVector.
        /// </summary>
        /// <param name="other">The RCNumVector to test.</param>
        /// <returns>True if other RCNumVector has the same coordinates as this RCNumVector.</returns>
        public bool Equals(RCNumVector other)
        {
            return (!this.isDefined && !other.isDefined) ||
                   (this.isDefined && other.isDefined && this.x == other.x && this.y == other.y);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCNumVector!"); }
            return this.x.GetHashCode() ^ this.y.GetHashCode();
        }

        /// <summary>
        /// Gets the string representation of this RCNumVector.
        /// </summary>
        /// <returns>The string representation of this RCNumVector.</returns>
        public override string ToString()
        {
            if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCNumVector!"); }
            return string.Format("({0}, {1})", this.x, this.y);
        }

        /// <summary>
        /// Rounds the components of this RCNumVector to the nearest integers.
        /// </summary>
        /// <returns>
        /// The components of the result vector are floor(n) if n - floor(n) is less than 0.5, otherwise ceiling(n).
        /// </returns>
        public RCIntVector Round()
        {
            if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCNumVector!"); }
            return new RCIntVector(this.x.Round(), this.y.Round());
        }

        /// <summary>
        /// Gets the horizontal coordinate of the RCNumVector.
        /// </summary>
        public RCNumber X
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCNumVector!"); }
                return this.x;
            }
        }

        /// <summary>
        /// Gets the vertical coordinate of the RCNumVector.
        /// </summary>
        public RCNumber Y
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCNumVector!"); }
                return this.y;
            }
        }

        /// <summary>
        /// Gets the length of this RCNumVector.
        /// </summary>
        public RCNumber Length
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCNumVector!"); }
                return this.LengthCache.Value;
            }
        }

        /// <summary>
        /// You can use this undefined RCNumVector as 'null' in reference types.
        /// </summary>
        public static readonly RCNumVector Undefined = new RCNumVector();

        #endregion Public fields

        #region Private fields

        /// <summary>
        /// Workaround: structs doesn't allow to define parameterless constructors, so we have to guarantee that
        /// the cache is created latest when first needed.
        /// </summary>
        private CachedValue<RCNumber> LengthCache
        {
            get
            {
                if (!this.lengthCacheCreated)
                {
                    RCNumber x = this.x;
                    RCNumber y = this.y;
                    this.lengthCache = new CachedValue<RCNumber>(
                        delegate()
                        {
                            return (x * x + y * y).Sqrt();
                        });

                    this.lengthCacheCreated = true;
                }

                return this.lengthCache;
            }
        }

        /// <summary>
        /// The horizontal coordinate of the RCNumVector.
        /// </summary>
        private RCNumber x;

        /// <summary>
        /// The vertical coordinate of the RCNumVector.
        /// </summary>
        private RCNumber y;

        /// <summary>
        /// The cache of the length of this RCNumVector.
        /// </summary>
        private CachedValue<RCNumber> lengthCache;

        /// <summary>
        /// This flag indicates whether the length cache has already been created or not.
        /// </summary>
        private bool lengthCacheCreated;

        /// <summary>
        /// This flag is true if this is a defined RCNumVector.
        /// </summary>
        private bool isDefined;

        #endregion Private fields
    }
}