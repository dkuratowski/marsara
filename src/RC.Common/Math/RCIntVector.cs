using System;

namespace RC.Common
{
    /// <summary>
    /// Represents an ordered pair of integer x- and y-coordinates that defines a vector or point in a 2D plane.
    /// </summary>
    public struct RCIntVector
    {
        #region Operator overloads

        /// <summary>
        /// Translates a RCIntVector with a given RCIntVector.
        /// </summary>
        /// <param name="lVect">The RCIntVector to translate.</param>
        /// <param name="rVect">The RCIntVector to translate with.</param>
        /// <returns>The translated RCIntVector.</returns>
        public static RCIntVector operator +(RCIntVector lVect, RCIntVector rVect)
        {
            if (!lVect.isDefined) { throw new ArgumentNullException("lVect"); }
            if (!rVect.isDefined) { throw new ArgumentNullException("rVect"); }
            return new RCIntVector(lVect.x + rVect.x, lVect.y + rVect.y);
        }

        /// <summary>
        /// Translates a RCIntVector with the negative of a given RCIntVector.
        /// </summary>
        /// <param name="lVect">The RCIntVector to translate.</param>
        /// <param name="rVect">The RCIntVector to translate with.</param>
        /// <returns>The translated RCIntVector.</returns>
        public static RCIntVector operator -(RCIntVector lVect, RCIntVector rVect)
        {
            if (!lVect.isDefined) { throw new ArgumentNullException("lVect"); }
            if (!rVect.isDefined) { throw new ArgumentNullException("rVect"); }
            return new RCIntVector(lVect.x - rVect.x, lVect.y - rVect.y);
        }

        /// <summary>
        /// Scales a RCIntVector with a given integer factor.
        /// </summary>
        /// <param name="vect">The RCIntVector to scale.</param>
        /// <param name="fact">The scaling factor.</param>
        /// <returns>The scaled RCIntVector.</returns>
        public static RCIntVector operator *(RCIntVector vect, int fact)
        {
            if (!vect.isDefined) { throw new ArgumentNullException("vect"); }
            return new RCIntVector(vect.x * fact, vect.y * fact);
        }

        /// <summary>
        /// Scales a RCIntVector with a given integer factor.
        /// </summary>
        /// <param name="fact">The scaling factor.</param>
        /// <param name="vect">The RCIntVector to scale.</param>
        /// <returns>The scaled RCIntVector.</returns>
        public static RCIntVector operator *(int fact, RCIntVector vect)
        {
            return vect * fact;
        }

        /// <summary>
        /// Scales a RCIntVector with a given float factor.
        /// </summary>
        /// <param name="vect">The RCIntVector to scale.</param>
        /// <param name="fact">The scaling factor.</param>
        /// <returns>The scaled RCIntVector.</returns>
        public static RCIntVector operator *(RCIntVector vect, float fact)
        {
            if (!vect.isDefined) { throw new ArgumentNullException("vect"); }
            return new RCIntVector((int)(vect.x * fact), (int)(vect.y * fact));
        }

        /// <summary>
        /// Scales a RCIntVector with a given float factor.
        /// </summary>
        /// <param name="fact">The scaling factor.</param>
        /// <param name="vect">The RCIntVector to scale.</param>
        /// <returns>The scaled RCIntVector.</returns>
        public static RCIntVector operator *(float fact, RCIntVector vect)
        {
            return vect * fact;
        }

        /// <summary>
        /// Computes the product of two given RCIntVectors.
        /// </summary>
        /// <param name="lVect">The first RCIntVector.</param>
        /// <param name="rVect">The second RCIntVector.</param>
        /// <returns>
        /// The X coordinate of the result RCIntVector will be lVect.X * lVect.X.
        /// The Y coordinate of the result RCIntVector will be lVect.Y * lVect.Y.
        /// </returns>
        public static RCIntVector operator *(RCIntVector lVect, RCIntVector rVect)
        {
            if (!lVect.isDefined) { throw new ArgumentNullException("lVect"); }
            if (!rVect.isDefined) { throw new ArgumentNullException("rVect"); }
            return new RCIntVector(lVect.X * rVect.X, lVect.Y * rVect.Y);
        }

        /// <summary>
        /// Divides an RCIntVector with a given integer.
        /// </summary>
        /// <param name="vect">The RCIntVector to divide.</param>
        /// <param name="divisor">The integer.</param>
        /// <returns>The coordinates of vect will be divided with divisor.</returns>
        public static RCIntVector operator /(RCIntVector vect, int divisor)
        {
            if (!vect.isDefined) { throw new ArgumentNullException("vect"); }
            if (divisor == 0) { throw new DivideByZeroException(); }
            return new RCIntVector(vect.x / divisor, vect.y / divisor);
        }

        /// <summary>
        /// Divides an RCIntVector with a given float.
        /// </summary>
        /// <param name="vect">The RCIntVector to divide.</param>
        /// <param name="divisor">The float.</param>
        /// <returns>The coordinates of vect will be divided with divisor.</returns>
        public static RCIntVector operator /(RCIntVector vect, float divisor)
        {
            if (!vect.isDefined) { throw new ArgumentNullException("vect"); }
            if (divisor == 0.0f) { throw new DivideByZeroException(); }
            return new RCIntVector((int)((float)vect.x / divisor), (int)((float)vect.y / divisor));
        }

        /// <summary>
        /// Compares two RCIntVector objects. The result specifies whether the values of the X and Y properties of the two RCIntVector objects are equal.
        /// </summary>
        /// <param name="lVect">A RCIntVector to compare.</param>
        /// <param name="rVect">A RCIntVector to compare.</param>
        /// <returns>True if the X and Y values of lVect and rVect are equal, false otherwise.</returns>
        public static bool operator ==(RCIntVector lVect, RCIntVector rVect)
        {
            return lVect.Equals(rVect);
        }

        /// <summary>
        /// Compares two RCIntVector objects. The result specifies whether the values of the X and Y properties of the two RCIntVector objects are unequal.
        /// </summary>
        /// <param name="lVect">A RCIntVector to compare.</param>
        /// <param name="rVect">A RCIntVector to compare.</param>
        /// <returns>True if the X and Y values of lVect and rVect differ, false otherwise.</returns>
        public static bool operator !=(RCIntVector lVect, RCIntVector rVect)
        {
            return !lVect.Equals(rVect);
        }

        #endregion Operator overloads

        #region Public fields

        /// <summary>
        /// Initializes a new RCIntVector with the specified coordinates.
        /// </summary>
        /// <param name="x">The horizontal coordinate of the RCIntVector.</param>
        /// <param name="y">The vertical coordinate of the RCIntVector.</param>
        public RCIntVector(int x, int y)
        {
            this.isDefined = true;
            this.x = x;
            this.y = y;
            this.lengthCache = default(CachedValue<float>);
            this.lengthCacheCreated = false;
        }

        /// <summary>
        /// Initializes a new RCIntVector with the specified RCIntVector.
        /// </summary>
        /// <param name="other">The RCIntVector to initialize with.</param>
        public RCIntVector(RCIntVector other)
        {
            if (!other.isDefined) { throw new ArgumentNullException("other"); }
            this.isDefined = true;
            this.x = other.x;
            this.y = other.y;
            this.lengthCache = default(CachedValue<float>);
            this.lengthCacheCreated = false;
        }

        /// <summary>
        /// Checks whether the specified object is an RCIntVector and contains the same coordinates as this RCIntVector.
        /// </summary>
        /// <param name="obj">The object to test.</param>
        /// <returns>True if obj is a RCIntVector and has the same coordinates as this RCIntVector.</returns>
        public override bool Equals(object obj)
        {
            return (obj is RCIntVector) && Equals((RCIntVector)obj);
        }

        /// <summary>
        /// Checks whether this RCIntVector contains the same coordinates as the specified RCIntVector.
        /// </summary>
        /// <param name="other">The RCIntVector to test.</param>
        /// <returns>True if other RCIntVector has the same coordinates as this RCIntVector.</returns>
        public bool Equals(RCIntVector other)
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
            if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCIntVector!"); }
            return this.x ^ this.y;
        }

        /// <summary>
        /// Gets the string representation of this RCIntVector.
        /// </summary>
        /// <returns>The string representation of this RCIntVector.</returns>
        public override string ToString()
        {
            if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCIntVector!"); }
            return string.Format("({0}, {1})", this.x, this.y);
        }

        /// <summary>
        /// Gets the horizontal coordinate of the RCIntVector.
        /// </summary>
        public int X
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCIntVector!"); }
                return this.x;
            }
        }

        /// <summary>
        /// Gets the vertical coordinate of the RCIntVector.
        /// </summary>
        public int Y
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCIntVector!"); }
                return this.y;
            }
        }

        /// <summary>
        /// Gets the length of this RCIntVector.
        /// </summary>
        public float Length
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCIntVector!"); }
                return this.LengthCache.Value;
            }
        }

        /// <summary>
        /// You can use this undefined RCIntVector as 'null' in reference types.
        /// </summary>
        public static readonly RCIntVector Undefined = new RCIntVector();

        #endregion Public fields

        #region Private fields

        /// <summary>
        /// Workaround: structs doesn't allow to define parameterless constructors, so we have to guarantee that
        /// the cache is created latest when first needed.
        /// </summary>
        private CachedValue<float> LengthCache
        {
            get
            {
                if (!this.lengthCacheCreated)
                {
                    int x = this.x;
                    int y = this.y;
                    this.lengthCache = new CachedValue<float>(
                        delegate()
                        {
                            return (float)Math.Sqrt((double)x * (double)x + (double)y * (double)y);
                        });

                    this.lengthCacheCreated = true;
                }

                return this.lengthCache;
            }
        }

        /// <summary>
        /// The horizontal coordinate of the RCIntVector.
        /// </summary>
        private int x;

        /// <summary>
        /// The vertical coordinate of the RCIntVector.
        /// </summary>
        private int y;

        /// <summary>
        /// The cache of the length of this RCIntVector.
        /// </summary>
        private CachedValue<float> lengthCache;

        /// <summary>
        /// This flag indicates whether the length cache has already been created or not.
        /// </summary>
        private bool lengthCacheCreated;

        /// <summary>
        /// This flag is true if this is a defined RCIntVector.
        /// </summary>
        private bool isDefined;

        #endregion Private fields
    }
}
