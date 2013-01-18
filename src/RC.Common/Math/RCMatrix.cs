using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Common
{
    /// <summary>
    /// Represents a 2x2 matrix with RCNumber components.
    /// </summary>
    public struct RCMatrix
    {
        #region Operator overloads

        #region Comparision operators

        /// <summary>
        /// Compares two RCMatrix objects. The result specifies whether the values of the components of the two RCMatrix objects are equal.
        /// </summary>
        /// <param name="lMat">A RCMatrix to compare.</param>
        /// <param name="rMat">A RCMatrix to compare.</param>
        /// <returns>True if the components of lMat and rMat are equal, false otherwise.</returns>
        public static bool operator ==(RCMatrix lMat, RCMatrix rMat)
        {
            return lMat.Equals(rMat);
        }

        /// <summary>
        /// Compares two RCMatrix objects. The result specifies whether the values of the components of the two RCMatrix objects are unequal.
        /// </summary>
        /// <param name="lMat">A RCMatrix to compare.</param>
        /// <param name="rMat">A RCMatrix to compare.</param>
        /// <returns>True if the components of lMat and rMat differ, false otherwise.</returns>
        public static bool operator !=(RCMatrix lMat, RCMatrix rMat)
        {
            return !lMat.Equals(rMat);
        }

        #endregion Comparision operators

        #region Arithmetic operators

        /// <summary>
        /// Adds two matrices.
        /// </summary>
        /// <param name="lMat">The left hand operand of the addition.</param>
        /// <param name="rMat">The right hand operand of the addition.</param>
        /// <returns>The result of the addition.</returns>
        public static RCMatrix operator +(RCMatrix lMat, RCMatrix rMat)
        {
            return new RCMatrix(lMat.item00 + rMat.item00, lMat.item01 + rMat.item01,
                                lMat.item10 + rMat.item10, lMat.item11 + rMat.item11);
        }

        /// <summary>
        /// Substracts the right RCMatrix from the left RCMatrix.
        /// </summary>
        /// <param name="lMat">The left RCMatrix.</param>
        /// <param name="rMat">The right RCMatrix.</param>
        /// <returns>The result matrix.</returns>
        public static RCMatrix operator -(RCMatrix lMat, RCMatrix rMat)
        {
            return lMat + (-rMat);
        }

        /// <summary>
        /// Creates the opposite of the given RCMatrix.
        /// </summary>
        /// <param name="mat">The RCMatrix.</param>
        /// <returns>The opposite of the given RCMatrix.</returns>
        public static RCMatrix operator -(RCMatrix mat)
        {
            return new RCMatrix(-mat.item00, -mat.item01, -mat.item10, -mat.item11);
        }

        /// <summary>
        /// Multiplies two matrices.
        /// </summary>
        /// <param name="lMat">The left hand operand of the multiplication.</param>
        /// <param name="rMat">The right hand operand of the multiplication.</param>
        /// <returns>The result of the multiplication.</returns>
        public static RCMatrix operator *(RCMatrix lMat, RCMatrix rMat)
        {
            return new RCMatrix(lMat.item00 * rMat.item00 + lMat.item01 * rMat.item10,
                                lMat.item00 * rMat.item01 + lMat.item01 * rMat.item11,
                                lMat.item10 * rMat.item00 + lMat.item11 * rMat.item10,
                                lMat.item10 * rMat.item01 + lMat.item11 * rMat.item11);
        }

        /// <summary>
        /// Multiplies an x vector with an A matrix.
        /// </summary>
        /// <param name="mat">The A matrix.</param>
        /// <param name="vect">The v vector.</param>
        /// <returns>The result vector of the multiplication, that is: Ax.</returns>
        public static RCNumVector operator *(RCMatrix mat, RCNumVector vect)
        {
            return new RCNumVector(mat.item00 * vect.X + mat.item01 * vect.Y,
                                   mat.item10 * vect.X + mat.item11 * vect.Y);
        }

        /// <summary>
        /// Multiplies an A matrix with an s scalar.
        /// </summary>
        /// <param name="num">The s scalar.</param>
        /// <param name="mat">The A matrix.</param>
        /// <returns>The result matrix of the multiplication, that is: sA.</returns>
        public static RCMatrix operator *(RCNumber num, RCMatrix mat)
        {
            return new RCMatrix(num * mat.item00, num * mat.item01,
                                num * mat.item10, num * mat.item11);
        }

        /// <summary>
        /// Multiplies an A matrix with an s scalar.
        /// </summary>
        /// <param name="num">The s scalar.</param>
        /// <param name="mat">The A matrix.</param>
        /// <returns>The result matrix of the multiplication, that is: sA.</returns>
        public static RCMatrix operator *(RCMatrix mat, RCNumber num)
        {
            return num * mat;
        }

        /// <summary>
        /// Multiplies the left matrix with the inverse of the right matrix.
        /// </summary>
        /// <param name="lMat">The left matrix.</param>
        /// <param name="rMat">The right matrix.</param>
        /// <returns>The result matrix.</returns>
        public static RCMatrix operator /(RCMatrix lNum, RCMatrix rNum)
        {
            return lNum * rNum.Inverse;
        }

        /// <summary>
        /// Multiplies an A matrix with the inverse of an s scalar.
        /// </summary>
        /// <param name="mat">The A matrix.</param>
        /// <param name="num">The s scalar.</param>
        /// <returns>The result matrix of the multiplication, that is: (1/s)*A.</returns>
        public static RCMatrix operator /(RCMatrix mat, RCNumber num)
        {
            return new RCMatrix(mat.item00 / num, mat.item01 / num,
                                mat.item10 / num, mat.item11 / num);
        }

        #endregion Arithmetic operators

        #endregion Operator overloads

        #region Constructors

        /// <summary>
        /// Creates an RCMatrix with the given components.
        /// </summary>
        /// <param name="item00">The first item in the first row.</param>
        /// <param name="item01">The second item in the first row.</param>
        /// <param name="item10">The first item in the second row.</param>
        /// <param name="item11">The second item in the second row.</param>
        public RCMatrix(RCNumber item00, RCNumber item01, RCNumber item10, RCNumber item11)
        {
            this.item00 = item00;
            this.item01 = item01;
            this.item10 = item10;
            this.item11 = item11;

            this.determinantCache = default(CachedValue<RCNumber>);
            this.determinantCacheCreated = false;
            this.inverseCache = default(CachedValue<RCMatrixInternal>);
            this.inverseCacheCreated = false;
        }

        /// <summary>
        /// Creates an RCMatrix with the given columns.
        /// </summary>
        /// <param name="col0">The first column of the matrix.</param>
        /// <param name="col0">The second column of the matrix.</param>
        public RCMatrix(RCNumVector col0, RCNumVector col1)
        {
            if (col0 == RCNumVector.Undefined) { throw new ArgumentNullException("col0"); }
            if (col1 == RCNumVector.Undefined) { throw new ArgumentNullException("col1"); }

            this.item00 = col0.X;
            this.item01 = col1.X;
            this.item10 = col0.Y;
            this.item11 = col1.Y;

            this.determinantCache = default(CachedValue<RCNumber>);
            this.determinantCacheCreated = false;
            this.inverseCache = default(CachedValue<RCMatrixInternal>);
            this.inverseCacheCreated = false;
        }

        /// <summary>
        /// Initializes a new RCMatrix with the specified RCMatrix.
        /// </summary>
        /// <param name="other">The RCMatrix to initialize with.</param>
        public RCMatrix(RCMatrix other)
        {
            this.item00 = other.item00;
            this.item01 = other.item01;
            this.item10 = other.item10;
            this.item11 = other.item11;

            this.determinantCache = default(CachedValue<RCNumber>);
            this.determinantCacheCreated = false;
            this.inverseCache = default(CachedValue<RCMatrixInternal>);
            this.inverseCacheCreated = false;
        }

        #endregion Constructors

        #region Public fields

        /// <summary>
        /// Checks whether the specified object is an RCMatrix and contains the same components as this RCMatrix.
        /// </summary>
        /// <param name="obj">The object to test.</param>
        /// <returns>True if obj is a RCMatrix and has the same components as this RCMatrix.</returns>
        public override bool Equals(object obj)
        {
            return (obj is RCMatrix) && Equals((RCMatrix)obj);
        }

        /// <summary>
        /// Checks whether this RCMatrix contains the same components as the specified RCMatrix.
        /// </summary>
        /// <param name="other">The RCMatrix to test.</param>
        /// <returns>True if other RCMatrix has the same components as this RCMatrix.</returns>
        public bool Equals(RCMatrix other)
        {
            return this.item00 == other.item00 && this.item01 == other.item01 &&
                   this.item10 == other.item10 && this.item11 == other.item11;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return this.item00.GetHashCode() ^ this.item01.GetHashCode() ^
                   this.item10.GetHashCode() ^ this.item11.GetHashCode();
        }

        /// <summary>
        /// Gets the string representation of this RCMatrix.
        /// </summary>
        /// <returns>The string representation of this RCMatrix.</returns>
        public override string ToString()
        {
            return string.Format("({0} {1}; {2} {3})", this.item00, this.item01, this.item10, this.item11);
        }

        /// <summary>
        /// Gets the determinant of this matrix.
        /// </summary>
        public RCNumber Determinant { get { return this.DeterminantCache.Value; } }

        /// <summary>
        /// Gets the inverse of this matrix.
        /// </summary>
        public RCMatrix Inverse
        {
            get
            {
                RCMatrixInternal inverse = this.InverseCache.Value;
                return new RCMatrix(inverse.Item00, inverse.Item01, inverse.Item10, inverse.Item11);
            }
        }

        #endregion Public fields

        #region Constants

        /// <summary>
        /// The null-matrix is the matrix where every component is 0.
        /// </summary>
        public static readonly RCMatrix NullMatrix = new RCMatrix(0, 0, 0, 0);

        /// <summary>
        /// The identity-matrix is the matrix where every component in the main diagonal is 1 and every other component is 0.
        /// </summary>
        public static readonly RCMatrix Identity = new RCMatrix(1, 0, 0, 1);

        #endregion Constants

        #region Private fields

        /// <summary>
        /// Internal workaround to avoid cycles in the struct layout.
        /// </summary>
        private struct RCMatrixInternal
        {
            /// <summary>
            /// The first item in the first row.
            /// </summary>
            public RCNumber Item00;

            /// <summary>
            /// The second item in the first row.
            /// </summary>
            public RCNumber Item01;

            /// <summary>
            /// The first item in the second row.
            /// </summary>
            public RCNumber Item10;

            /// <summary>
            /// The second item in the second row.
            /// </summary>
            public RCNumber Item11;
        }

        /// <summary>
        /// Workaround: structs doesn't allow to define parameterless constructors, so we have to guarantee that
        /// the cache is created latest when first needed.
        /// </summary>
        private CachedValue<RCNumber> DeterminantCache
        {
            get
            {
                if (!this.determinantCacheCreated)
                {
                    RCNumber item00 = this.item00;
                    RCNumber item01 = this.item01;
                    RCNumber item10 = this.item10;
                    RCNumber item11 = this.item11;
                    this.determinantCache = new CachedValue<RCNumber>(
                        delegate()
                        {
                            return item00 * item11 - item01 * item10;
                        });

                    this.determinantCacheCreated = true;
                }

                return this.determinantCache;
            }
        }

        /// <summary>
        /// Workaround: structs doesn't allow to define parameterless constructors, so we have to guarantee that
        /// the cache is created latest when first needed.
        /// </summary>
        private CachedValue<RCMatrixInternal> InverseCache
        {
            get
            {
                if (!this.inverseCacheCreated)
                {
                    RCNumber determinant = this.Determinant;
                    if (determinant == 0) { throw new InvalidOperationException("Inverse of matrix doesn't exist!"); }

                    RCNumber item00 = this.item00;
                    RCNumber item01 = this.item01;
                    RCNumber item10 = this.item10;
                    RCNumber item11 = this.item11;

                    this.inverseCache = new CachedValue<RCMatrixInternal>(
                        delegate()
                        {
                            return new RCMatrixInternal()
                            {
                                Item00 = item11 / determinant,
                                Item01 = -item01 / determinant,
                                Item10 = -item10 / determinant,
                                Item11 = item00 / determinant
                            };
                        });

                    this.inverseCacheCreated = true;
                }

                return this.inverseCache;
            }
        }

        /// <summary>
        /// The first item in the first row.
        /// </summary>
        private RCNumber item00;

        /// <summary>
        /// The second item in the first row.
        /// </summary>
        private RCNumber item01;

        /// <summary>
        /// The first item in the second row.
        /// </summary>
        private RCNumber item10;

        /// <summary>
        /// The second item in the second row.
        /// </summary>
        private RCNumber item11;

        /// <summary>
        /// Cache for computing the determinant of this matrix.
        /// </summary>
        private CachedValue<RCNumber> determinantCache;

        /// <summary>
        /// This flag indicates whether the determinant cache has already been created or not.
        /// </summary>
        private bool determinantCacheCreated;

        /// <summary>
        /// Cache for computing the inverse of this matrix.
        /// </summary>
        private CachedValue<RCMatrixInternal> inverseCache;

        /// <summary>
        /// This flag indicates whether the inverse cache has already been created or not.
        /// </summary>
        private bool inverseCacheCreated;

        #endregion Private fields
    }
}
