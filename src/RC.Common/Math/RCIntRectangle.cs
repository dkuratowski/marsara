using System;

namespace RC.Common
{
    /// <summary>
    /// Stores a set of four integers that represent the location and the size of a rectangle in a 2D plane.
    /// </summary>
    public struct RCIntRectangle
    {
        #region Operator overloads

        /// <summary>
        /// Scales an RCIntRectangle with the given RCIntVector.
        /// </summary>
        /// <param name="rect">The RCIntRectangle to scale.</param>
        /// <param name="vect">The RCIntVector to scale with.</param>
        /// <returns>The position and size of rect will be scaled with vect.</returns>
        public static RCIntRectangle operator *(RCIntRectangle rect, RCIntVector vect)
        {
            if (!rect.isDefined) { throw new ArgumentNullException("rect"); }
            if (vect == RCIntVector.Undefined) { throw new ArgumentNullException("vect"); }
            return new RCIntRectangle(rect.x * vect.X, rect.y * vect.Y,
                                   rect.width * vect.X, rect.height * vect.Y);
        }

        /// <summary>
        /// Scales an RCIntRectangle with the given RCIntVector.
        /// </summary>
        /// <param name="vect">The RCIntVector to scale with.</param>
        /// <param name="rect">The RCIntRectangle to scale.</param>
        /// <returns>The position and size of rect will be scaled with vect.</returns>
        public static RCIntRectangle operator *(RCIntVector vect, RCIntRectangle rect)
        {
            return rect * vect;
        }

        /// <summary>
        /// Scales an RCIntRectangle with the given float.
        /// </summary>
        /// <param name="rect">The RCIntRectangle to scale.</param>
        /// <param name="scale">The float to scale with.</param>
        /// <returns>The position and size of rect will be scaled with scale.</returns>
        public static RCIntRectangle operator *(RCIntRectangle rect, float scale)
        {
            if (!rect.isDefined) { throw new ArgumentNullException("rect"); }
            if (scale <= 0.0f) { throw new ArgumentOutOfRangeException("scale"); }
            return new RCIntRectangle((int)((float)rect.x * scale), (int)((float)rect.y * scale),
                                   (int)((float)rect.width * scale), (int)((float)rect.height * scale));
        }

        /// <summary>
        /// Rescales an RCIntRectangle with the given RCIntVector.
        /// </summary>
        /// <param name="rect">The RCIntRectangle to rescale.</param>
        /// <param name="vect">The RCIntVector to rescale with.</param>
        /// <returns>The position and size of rect will be divided with vect.</returns>
        public static RCIntRectangle operator /(RCIntRectangle rect, RCIntVector vect)
        {
            if (!rect.isDefined) { throw new ArgumentNullException("rect"); }
            if (vect == RCIntVector.Undefined) { throw new ArgumentNullException("vect"); }
            return new RCIntRectangle(rect.x / vect.X, rect.y / vect.Y,
                                   rect.width / vect.X, rect.height / vect.Y);
        }

        /// <summary>
        /// Translates an RCIntRectangle with the given RCIntVector.
        /// </summary>
        /// <param name="rect">The RCIntRectangle to translate.</param>
        /// <param name="vect">The RCIntVector to translate with.</param>
        /// <returns>The position of rect will be translated with vect.</returns>
        public static RCIntRectangle operator +(RCIntRectangle rect, RCIntVector vect)
        {
            if (!rect.isDefined) { throw new ArgumentNullException("rect"); }
            if (vect == RCIntVector.Undefined) { throw new ArgumentNullException("vect"); }
            return new RCIntRectangle(rect.x + vect.X, rect.y + vect.Y, rect.width, rect.height);
        }

        /// <summary>
        /// Translates an RCIntRectangle with the given RCIntVector.
        /// </summary>
        /// <param name="rect">The RCIntRectangle to translate.</param>
        /// <param name="vect">The RCIntVector to translate with.</param>
        /// <returns>The position of rect will be translated with vect.</returns>
        public static RCIntRectangle operator +(RCIntVector vect, RCIntRectangle rect)
        {
            return rect + vect;
        }

        /// <summary>
        /// Translates an RCIntRectangle with the opposite of the given RCIntVector.
        /// </summary>
        /// <param name="rect">The RCIntRectangle to translate.</param>
        /// <param name="vect">The RCIntVector whose opposite to translate with.</param>
        /// <returns>The position of rect will be translated with the opposite of vect.</returns>
        public static RCIntRectangle operator -(RCIntRectangle rect, RCIntVector vect)
        {
            if (!rect.isDefined) { throw new ArgumentNullException("rect"); }
            if (vect == RCIntVector.Undefined) { throw new ArgumentNullException("vect"); }
            return new RCIntRectangle(rect.x - vect.X, rect.y - vect.Y, rect.width, rect.height);
        }

        /// <summary>
        /// Compares two RCIntRectangle objects. The result specifies whether the location and size of the two RCIntRectangle objects are equal.
        /// </summary>
        /// <param name="lRect">A RCIntRectangle to compare.</param>
        /// <param name="rRect">A RCIntRectangle to compare.</param>
        /// <returns>True if the location and size of lRect and rRect are equal, false otherwise.</returns>
        public static bool operator ==(RCIntRectangle lRect, RCIntRectangle rRect)
        {
            return lRect.Equals(rRect);
        }

        /// <summary>
        /// Compares two RCIntRectangle objects. The result specifies whether the values of the location and size of the two RCIntRectangle objects are unequal.
        /// </summary>
        /// <param name="lRect">A RCIntRectangle to compare.</param>
        /// <param name="rRect">A RCIntRectangle to compare.</param>
        /// <returns>True if the location and size of lRect and rRect differ, false otherwise.</returns>
        public static bool operator !=(RCIntRectangle lRect, RCIntRectangle rRect)
        {
            return !lRect.Equals(rRect);
        }

        #endregion Operator overloads

        #region Public fields

        /// <summary>
        /// Initializes a new RCIntRectangle with the specified location and size.
        /// </summary>
        /// <param name="x">The horizontal coordinate of the top-left corner of the RCIntRectangle.</param>
        /// <param name="y">The vertical coordinate of the top-left corner of the RCIntRectangle.</param>
        /// <param name="width">The width of the RCIntRectangle.</param>
        /// <param name="height">The height of the RCIntRectangle.</param>
        public RCIntRectangle(int x, int y, int width, int height)
        {
            if (width <= 0) { throw new ArgumentOutOfRangeException("Width has to be greater than 0!", "width"); }
            if (height <= 0) { throw new ArgumentOutOfRangeException("Height has to be greater than 0!", "height"); }

            this.isDefined = true;
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        /// <summary>
        /// Initializes a new RCIntRectangle with the specified location and size.
        /// </summary>
        /// <param name="loc">The location of the top-left corner of the RCIntRectangle.</param>
        /// <param name="size">The size of the RCIntRectangle.</param>
        public RCIntRectangle(RCIntVector loc, RCIntVector size)
        {
            if (loc == RCIntVector.Undefined) { throw new ArgumentNullException("loc"); }
            if (size == RCIntVector.Undefined) { throw new ArgumentNullException("size"); }
            if (size.X <= 0) { throw new ArgumentOutOfRangeException("Width has to be greater than 0!", "width"); }
            if (size.Y <= 0) { throw new ArgumentOutOfRangeException("Height has to be greater than 0!", "height"); }

            this.isDefined = true;
            this.x = loc.X;
            this.y = loc.Y;
            this.width = size.X;
            this.height = size.Y;
        }

        /// <summary>
        /// Initializes a new RCIntRectangle with the specified RCIntRectangle.
        /// </summary>
        /// <param name="other">The RCIntRectangle to initialize with.</param>
        public RCIntRectangle(RCIntRectangle other)
        {
            if (!other.isDefined) { throw new ArgumentNullException("other"); }

            this.isDefined = true;
            this.x = other.x;
            this.y = other.y;
            this.width = other.width;
            this.height = other.height;
        }

        /// <summary>
        /// Checks whether the specified object is an RCIntRectangle and has the same location and size as this RCIntRectangle.
        /// </summary>
        /// <param name="obj">The object to test.</param>
        /// <returns>True if obj is a RCIntRectangle and has the same location and size as this RCIntRectangle.</returns>
        public override bool Equals(object obj)
        {
            return (obj is RCIntRectangle) && Equals((RCIntRectangle)obj);
        }

        /// <summary>
        /// Checks whether this RCIntRectangle has the same location and size as the specified RCIntRectangle.
        /// </summary>
        /// <param name="other">The RCIntRectangle to test.</param>
        /// <returns>True if other RCIntRectangle has same location and size as this RCIntRectangle.</returns>
        public bool Equals(RCIntRectangle other)
        {
            return (!this.isDefined && !other.isDefined) ||
                   (this.isDefined && other.isDefined && this.x == other.x && this.y == other.y &&
                                                         this.width == other.width && this.height == other.height);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCIntRectangle!"); }
            return this.x ^ this.y ^ this.width ^ this.height;
        }

        /// <summary>
        /// Gets the string representation of this RCIntRectangle.
        /// </summary>
        /// <returns>The string representation of this RCIntRectangle.</returns>
        public override string ToString()
        {
            return string.Format("({0}, {1}, {2}, {3})", this.x, this.y, this.width, this.height);
        }

        /// <summary>
        /// Determines if the specified point is contained within this RCIntRectangle.
        /// </summary>
        /// <param name="x">The horizontal coordinate of the point to test.</param>
        /// <param name="y">The vertical coordinate of the point to test.</param>
        /// <returns>Returns true if the point is contained within this RCIntRectangle, false otherwise.</returns>
        public bool Contains(int x, int y)
        {
            if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCIntRectangle!"); }
            return x >= this.x && y >= this.y && x < this.Right && y < this.Bottom;
        }

        /// <summary>
        /// Determines if the specified point is contained within this RCIntRectangle.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <returns>Returns true if the point is contained within this RCIntRectangle, false otherwise.</returns>
        public bool Contains(RCIntVector point)
        {
            if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCIntRectangle!"); }
            if (point == RCIntVector.Undefined) { throw new ArgumentNullException("point"); }
            return this.Contains(point.X, point.Y);
        }

        /// <summary>
        /// Determines if the rectangular region represented by rect is entirely contained within this RCIntRectangle.
        /// </summary>
        /// <param name="rect">The RCIntRectangle to test.</param>
        /// <returns>True if the rectangular region represented by rect is entirely contained within this RCIntRectangle, false otherwise.</returns>
        public bool Contains(RCIntRectangle rect)
        {
            if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCIntRectangle!"); }
            if (!rect.isDefined) { throw new ArgumentNullException("rect"); }

            return this.Contains(rect.x, rect.y) &&
                   this.Contains(rect.x + rect.width - 1, rect.y + rect.height - 1);
        }

        /// <summary>
        /// Determines if this RCIntRectangle intersects with rect.
        /// </summary>
        /// <param name="rect">The RCIntRectangle to test.</param>
        /// <returns>True if there is any intersection, false otherwise.</returns>
        public bool IntersectsWith(RCIntRectangle rect)
        {
            if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCIntRectangle!"); }
            if (!rect.isDefined) { throw new ArgumentNullException("rect"); }

            return (this.Left < rect.Right) && (this.Right > rect.Left) && (this.Top < rect.Bottom) && (this.Bottom > rect.Top);
        }

        /// <summary>
        /// Replaces this RCIntRectangle with the intersection of itself and the specified Rectangle.
        /// </summary>
        /// <param name="other">The other RCIntRectangle to intersect with.</param>
        /// <remarks>
        /// If this and other RCRectangles don't intersect each other, then this RCIntRectangle becomes RCIntRectangle.Undefined.
        /// </remarks>
        public void Intersect(RCIntRectangle other)
        {
            if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCIntRectangle!"); }
            if (!other.isDefined) { throw new ArgumentNullException("other"); }

            this = RCIntRectangle.Intersect(this, other);
        }

        /// <summary>
        /// Returns a third RCIntRectangle that represents the intersection of two other RCRectangles.
        /// If there is no intersection, RCIntRectangle.Undefined is returned.
        /// </summary>
        /// <param name="lRect">An RCIntRectangle to intersect.</param>
        /// <param name="rRect">An RCIntRectangle to intersect.</param>
        /// <returns>
        /// An RCIntRectangle that is the intersection of lRect and rRect or RCIntRectangle.Undefined if there is no intersection.
        /// </returns>
        public static RCIntRectangle Intersect(RCIntRectangle lRect, RCIntRectangle rRect)
        {
            if (!lRect.isDefined) { throw new ArgumentNullException("lRect"); }
            if (!rRect.isDefined) { throw new ArgumentNullException("rRect"); }

            int intersectLeft, intersectRight, intersectTop, intersectBottom;

            /// Compute the horizontal coordinates of the intersection
            if (!IntersectHorizontal(ref lRect, ref rRect, out intersectLeft, out intersectRight) &&
                !IntersectHorizontal(ref rRect, ref lRect, out intersectLeft, out intersectRight))
            {
                return RCIntRectangle.Undefined;
            }

            /// Compute the vertical coordinates of the intersection
            if (!IntersectVertical(ref lRect, ref rRect, out intersectTop, out intersectBottom) &&
                !IntersectVertical(ref rRect, ref lRect, out intersectTop, out intersectBottom))
            {
                return RCIntRectangle.Undefined;
            }

            return new RCIntRectangle(intersectLeft, intersectTop, intersectRight - intersectLeft, intersectBottom - intersectTop);
        }
        
        /// <summary>
        /// Gets the horizontal coordinate of the left edge of this RCIntRectangle.
        /// </summary>
        public int Left
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCIntRectangle!"); }
                return this.x;
            }
        }

        /// <summary>
        /// The horizontal coordinate of the first point at the right edge of this RCIntRectangle that is not contained in the rectangle.
        /// </summary>
        public int Right
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCIntRectangle!"); }
                return this.x + this.width;
            }
        }

        /// <summary>
        /// Gets the vertical coordinate of the top edge of this RCIntRectangle.
        /// </summary>
        public int Top
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCIntRectangle!"); }
                return this.y;
            }
        }

        /// <summary>
        /// The vertical coordinate of the first point at the bottom edge of this RCIntRectangle that is not contained in the rectangle.
        /// </summary>
        public int Bottom
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCIntRectangle!"); }
                return this.y + this.height;
            }
        }

        /// <summary>
        /// Gets or sets the location of the top-left corner of this RCIntRectangle.
        /// </summary>
        public RCIntVector Location
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCIntRectangle!"); }
                return new RCIntVector(this.x, this.y);
            }
            set
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCIntRectangle!"); }
                if (value == RCIntVector.Undefined) { throw new ArgumentNullException("Location"); }
                this.x = value.X;
                this.y = value.Y;
            }
        }

        /// <summary>
        /// Gets or sets the size of this RCIntRectangle.
        /// </summary>
        public RCIntVector Size
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCIntRectangle!"); }
                return new RCIntVector(this.width, this.height);
            }
            set
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCIntRectangle!"); }
                if (value == RCIntVector.Undefined) { throw new ArgumentNullException("Size"); }
                if (value.X <= 0) { throw new ArgumentOutOfRangeException("Width has to be greater than 0!", "Size"); }
                if (value.Y <= 0) { throw new ArgumentOutOfRangeException("Height has to be greater than 0!", "Size"); }
                this.width = value.X;
                this.height = value.Y;
            }
        }

        /// <summary>
        /// Gets or sets the width of this RCIntRectangle.
        /// </summary>
        public int Width
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCIntRectangle!"); }
                return this.width;
            }
            set
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCIntRectangle!"); }
                if (value <= 0) { throw new ArgumentOutOfRangeException("Width has to be greater than 0!", "Width"); }
                this.width = value;
            }
        }

        /// <summary>
        /// Gets or sets the height of this RCIntRectangle.
        /// </summary>
        public int Height
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCIntRectangle!"); }
                return this.height;
            }
            set
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCIntRectangle!"); }
                if (value <= 0) { throw new ArgumentOutOfRangeException("Height has to be greater than 0!", "Height"); }
                this.height = value;
            }
        }

        /// <summary>
        /// Gets or sets the horizontal coordinate of the top-left corner of this RCIntRectangle.
        /// </summary>
        public int X
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCIntRectangle!"); }
                return this.x;
            }
            set
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCIntRectangle!"); }
                this.x = value;
            }
        }

        /// <summary>
        /// Gets or sets the vertical coordinate of the top-left corner of this RCIntRectangle.
        /// </summary>
        public int Y
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCIntRectangle!"); }
                return this.y;
            }
            set
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCIntRectangle!"); }
                this.y = value;
            }
        }

        /// <summary>
        /// You can use this undefined RCIntRectangle as 'null' in reference types.
        /// </summary>
        public static readonly RCIntRectangle Undefined = new RCIntRectangle();

        #endregion Public fields

        #region Private fields

        /// <summary>
        /// Internal helper function for computing intersections.
        /// </summary>
        private static bool IntersectHorizontal(ref RCIntRectangle lRect, ref RCIntRectangle rRect, out int intersectLeft, out int intersectRight)
        {
            if ((lRect.Left <= rRect.Left && rRect.Left < lRect.Right && lRect.Right <= rRect.Right))
            {
                intersectLeft = rRect.Left;
                intersectRight = lRect.Right;
                return true;
            }
            else if (rRect.Left <= lRect.Left && lRect.Left < lRect.Right && lRect.Right <= rRect.Right)
            {
                intersectLeft = lRect.Left;
                intersectRight = lRect.Right;
                return true;
            }
            else if (rRect.Left <= lRect.Left && lRect.Left < rRect.Right && rRect.Right <= lRect.Right)
            {
                intersectLeft = lRect.Left;
                intersectRight = rRect.Right;
                return true;
            }
            else
            {
                intersectLeft = 0;
                intersectRight = 0;
                return false;
            }
        }

        /// <summary>
        /// Internal helper function for computing intersections.
        /// </summary>
        private static bool IntersectVertical(ref RCIntRectangle lRect, ref RCIntRectangle rRect, out int intersectTop, out int intersectBottom)
        {
            if (lRect.Top <= rRect.Top && rRect.Top < lRect.Bottom && lRect.Bottom <= rRect.Bottom)
            {
                intersectTop = rRect.Top;
                intersectBottom = lRect.Bottom;
                return true;
            }
            else if (rRect.Top <= lRect.Top && lRect.Top < lRect.Bottom && lRect.Bottom <= rRect.Bottom)
            {
                intersectTop = lRect.Top;
                intersectBottom = lRect.Bottom;
                return true;
            }
            else if (rRect.Top <= lRect.Top && lRect.Top < rRect.Bottom && rRect.Bottom <= lRect.Bottom)
            {
                intersectTop = lRect.Top;
                intersectBottom = rRect.Bottom;
                return true;
            }
            else
            {
                intersectTop = 0;
                intersectBottom = 0;
                return false;
            }
        }

        /// <summary>
        /// The horizontal coordinate of the top-left corner of this RCIntRectangle.
        /// </summary>
        private int x;

        /// <summary>
        /// The vertical coordinate of the top-left corner of this RCIntRectangle.
        /// </summary>
        private int y;

        /// <summary>
        /// The width of this RCIntRectangle.
        /// </summary>
        private int width;

        /// <summary>
        /// The height of this RCIntRectangle.
        /// </summary>
        private int height;

        /// <summary>
        /// This flag is true if this is a defined RCIntRectangle.
        /// </summary>
        private bool isDefined;

        #endregion Private fields
    }
}
