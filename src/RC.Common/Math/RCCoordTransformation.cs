using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Common
{
    /// <summary>
    /// Helper class for performing coordinate transformations between 2D coodinate-systems A and B.
    /// </summary>
    public class RCCoordTransformation
    {
        /// <summary>
        /// Constructs an RCCoordTransformation instance.
        /// </summary>
        /// <param name="nullVectorOfB">The null vector of coordinate-system B in coordinate-system A.</param>
        /// <param name="firstBaseVectorOfB">The first base vector of coordinate-system B in coordinate-system A.</param>
        /// <param name="secondBaseVectorOfB">The second base vector of coordinate-system B in coordinate-system A.</param>
        public RCCoordTransformation(RCNumVector nullVectorOfB, RCNumVector firstBaseVectorOfB, RCNumVector secondBaseVectorOfB)
        {
            if (nullVectorOfB == RCNumVector.Undefined) { throw new ArgumentNullException("nullVectorOfB"); }
            if (firstBaseVectorOfB == RCNumVector.Undefined) { throw new ArgumentNullException("firstBaseVectorOfB"); }
            if (secondBaseVectorOfB == RCNumVector.Undefined) { throw new ArgumentNullException("secondBaseVectorOfB"); }

            this.nullVectorOfB = nullVectorOfB;
            this.transformBA = new RCMatrix(firstBaseVectorOfB, secondBaseVectorOfB);
            this.transformAB = this.transformBA.Inverse;
        }

        /// <summary>
        /// Transforms the coordinates of the given vector from coordinate-system A to B.
        /// </summary>
        /// <param name="vect">The vector to be transformed.</param>
        /// <returns>The transformed vector.</returns>
        public RCNumVector TransformAB(RCNumVector vect)
        {
            if (vect == RCNumVector.Undefined) { throw new ArgumentNullException("vect"); }
            return this.transformAB * (vect - this.nullVectorOfB);
        }

        /// <summary>
        /// Transforms the coordinates of the given vector from coordinate-system B to A.
        /// </summary>
        /// <param name="vect">The vector to be transformed.</param>
        /// <returns>The transformed vector.</returns>
        public RCNumVector TransformBA(RCNumVector vect)
        {
            if (vect == RCNumVector.Undefined) { throw new ArgumentNullException("vect"); }
            return this.transformBA * vect + this.nullVectorOfB;
        }

        /// <summary>
        /// The null vector of coordinate-system B in coordinate-system A.
        /// </summary>
        private RCNumVector nullVectorOfB;

        /// <summary>
        /// The transformation matrix from coordinate-system B to A.
        /// </summary>
        private RCMatrix transformBA;

        /// <summary>
        /// The transformation matrix from coordinate-system A to B.
        /// </summary>
        private RCMatrix transformAB;
    }
}
