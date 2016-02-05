using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Common
{
    /// <summary>
    /// Stores a set of four RCNumbers that represent the location and the size of a rectangle in a 2D plane.
    /// </summary>
    public struct RCNumRectangle
    {
        #region Operator overloads

        #region Cast operators

        /// <summary>
        /// Implicit casting from RCIntRectangle to RCNumRectangle.
        /// </summary>
        /// <param name="rect">The integer rectangle to be casted.</param>
        /// <returns>The result RCNumRectangle.</returns>
        public static implicit operator RCNumRectangle(RCIntRectangle rect)
        {
            if (rect == RCIntRectangle.Undefined) { return RCNumRectangle.Undefined; }
            return new RCNumRectangle(rect.X, rect.Y, rect.Width, rect.Height);
        }

        /// <summary>
        /// Explicit casting from RCNumRectangle to RCIntRectangle.
        /// </summary>
        /// <param name="rect">The RCNumVector to be casted.</param>
        /// <returns>The components of the result rectangle are the floor of the components of the input rectangle.</returns>
        public static explicit operator RCIntRectangle(RCNumRectangle rect)
        {
            if (rect == RCNumRectangle.Undefined) { return RCIntRectangle.Undefined; }
            return new RCIntRectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
        }

        #endregion Cast operators

        #region Arithmetic operators

        /// <summary>
        /// Scales an RCNumRectangle with the given RCNumVector.
        /// </summary>
        /// <param name="rect">The RCNumRectangle to scale.</param>
        /// <param name="vect">The RCNumVector to scale with.</param>
        /// <returns>The position and size of rect will be scaled with vect.</returns>
        public static RCNumRectangle operator *(RCNumRectangle rect, RCNumVector vect)
        {
            if (!rect.isDefined) { throw new ArgumentNullException("rect"); }
            if (vect == RCNumVector.Undefined) { throw new ArgumentNullException("vect"); }
            return new RCNumRectangle(rect.x * vect.X, rect.y * vect.Y,
                                   rect.width * vect.X, rect.height * vect.Y);
        }

        /// <summary>
        /// Scales an RCNumRectangle with the given RCNumVector.
        /// </summary>
        /// <param name="vect">The RCNumVector to scale with.</param>
        /// <param name="rect">The RCNumRectangle to scale.</param>
        /// <returns>The position and size of rect will be scaled with vect.</returns>
        public static RCNumRectangle operator *(RCNumVector vect, RCNumRectangle rect)
        {
            return rect * vect;
        }

        /// <summary>
        /// Scales an RCNumRectangle with the given float.
        /// </summary>
        /// <param name="rect">The RCNumRectangle to scale.</param>
        /// <param name="scale">The float to scale with.</param>
        /// <returns>The position and size of rect will be scaled with scale.</returns>
        public static RCNumRectangle operator *(RCNumRectangle rect, float scale)
        {
            if (!rect.isDefined) { throw new ArgumentNullException("rect"); }
            if (scale <= 0.0f) { throw new ArgumentOutOfRangeException("scale"); }
            return new RCNumRectangle((int)((float)rect.x * scale), (int)((float)rect.y * scale),
                                   (int)((float)rect.width * scale), (int)((float)rect.height * scale));
        }

        /// <summary>
        /// Rescales an RCNumRectangle with the given RCNumVector.
        /// </summary>
        /// <param name="rect">The RCNumRectangle to rescale.</param>
        /// <param name="vect">The RCNumVector to rescale with.</param>
        /// <returns>The position and size of rect will be divided with vect.</returns>
        public static RCNumRectangle operator /(RCNumRectangle rect, RCNumVector vect)
        {
            if (!rect.isDefined) { throw new ArgumentNullException("rect"); }
            if (vect == RCNumVector.Undefined) { throw new ArgumentNullException("vect"); }
            return new RCNumRectangle(rect.x / vect.X, rect.y / vect.Y,
                                   rect.width / vect.X, rect.height / vect.Y);
        }

        /// <summary>
        /// Translates an RCNumRectangle with the given RCNumVector.
        /// </summary>
        /// <param name="rect">The RCNumRectangle to translate.</param>
        /// <param name="vect">The RCNumVector to translate with.</param>
        /// <returns>The position of rect will be translated with vect.</returns>
        public static RCNumRectangle operator +(RCNumRectangle rect, RCNumVector vect)
        {
            if (!rect.isDefined) { throw new ArgumentNullException("rect"); }
            if (vect == RCNumVector.Undefined) { throw new ArgumentNullException("vect"); }
            return new RCNumRectangle(rect.x + vect.X, rect.y + vect.Y, rect.width, rect.height);
        }

        /// <summary>
        /// Translates an RCNumRectangle with the given RCNumVector.
        /// </summary>
        /// <param name="rect">The RCNumRectangle to translate.</param>
        /// <param name="vect">The RCNumVector to translate with.</param>
        /// <returns>The position of rect will be translated with vect.</returns>
        public static RCNumRectangle operator +(RCNumVector vect, RCNumRectangle rect)
        {
            return rect + vect;
        }

        /// <summary>
        /// Translates an RCNumRectangle with the opposite of the given RCNumVector.
        /// </summary>
        /// <param name="rect">The RCNumRectangle to translate.</param>
        /// <param name="vect">The RCNumVector whose opposite to translate with.</param>
        /// <returns>The position of rect will be translated with the opposite of vect.</returns>
        public static RCNumRectangle operator -(RCNumRectangle rect, RCNumVector vect)
        {
            if (!rect.isDefined) { throw new ArgumentNullException("rect"); }
            if (vect == RCNumVector.Undefined) { throw new ArgumentNullException("vect"); }
            return new RCNumRectangle(rect.x - vect.X, rect.y - vect.Y, rect.width, rect.height);
        }

        /// <summary>
        /// Compares two RCNumRectangle objects. The result specifies whether the location and size of the two RCNumRectangle objects are equal.
        /// </summary>
        /// <param name="lRect">A RCNumRectangle to compare.</param>
        /// <param name="rRect">A RCNumRectangle to compare.</param>
        /// <returns>True if the location and size of lRect and rRect are equal, false otherwise.</returns>
        public static bool operator ==(RCNumRectangle lRect, RCNumRectangle rRect)
        {
            return lRect.Equals(rRect);
        }

        /// <summary>
        /// Compares two RCNumRectangle objects. The result specifies whether the values of the location and size of the two RCNumRectangle objects are unequal.
        /// </summary>
        /// <param name="lRect">A RCNumRectangle to compare.</param>
        /// <param name="rRect">A RCNumRectangle to compare.</param>
        /// <returns>True if the location and size of lRect and rRect differ, false otherwise.</returns>
        public static bool operator !=(RCNumRectangle lRect, RCNumRectangle rRect)
        {
            return !lRect.Equals(rRect);
        }

        #endregion Arithmetic operators

        #endregion Operator overloads

        #region Public fields

        /// <summary>
        /// Initializes a new RCNumRectangle with the specified location and size.
        /// </summary>
        /// <param name="x">The horizontal coordinate of the top-left corner of the RCNumRectangle.</param>
        /// <param name="y">The vertical coordinate of the top-left corner of the RCNumRectangle.</param>
        /// <param name="width">The width of the RCNumRectangle.</param>
        /// <param name="height">The height of the RCNumRectangle.</param>
        public RCNumRectangle(RCNumber x, RCNumber y, RCNumber width, RCNumber height)
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
        /// Initializes a new RCNumRectangle with the specified location and size.
        /// </summary>
        /// <param name="loc">The location of the top-left corner of the RCNumRectangle.</param>
        /// <param name="size">The size of the RCNumRectangle.</param>
        public RCNumRectangle(RCNumVector loc, RCNumVector size)
        {
            if (loc == RCNumVector.Undefined) { throw new ArgumentNullException("loc"); }
            if (size == RCNumVector.Undefined) { throw new ArgumentNullException("size"); }
            if (size.X <= 0) { throw new ArgumentOutOfRangeException("Width has to be greater than 0!", "width"); }
            if (size.Y <= 0) { throw new ArgumentOutOfRangeException("Height has to be greater than 0!", "height"); }

            this.isDefined = true;
            this.x = loc.X;
            this.y = loc.Y;
            this.width = size.X;
            this.height = size.Y;
        }

        /// <summary>
        /// Initializes a new RCNumRectangle with the specified RCNumRectangle.
        /// </summary>
        /// <param name="other">The RCNumRectangle to initialize with.</param>
        public RCNumRectangle(RCNumRectangle other)
        {
            if (!other.isDefined) { throw new ArgumentNullException("other"); }

            this.isDefined = true;
            this.x = other.x;
            this.y = other.y;
            this.width = other.width;
            this.height = other.height;
        }

        /// <summary>
        /// Checks whether the specified object is an RCNumRectangle and has the same location and size as this RCNumRectangle.
        /// </summary>
        /// <param name="obj">The object to test.</param>
        /// <returns>True if obj is a RCNumRectangle and has the same location and size as this RCNumRectangle.</returns>
        public override bool Equals(object obj)
        {
            return (obj is RCNumRectangle) && Equals((RCNumRectangle)obj);
        }

        /// <summary>
        /// Checks whether this RCNumRectangle has the same location and size as the specified RCNumRectangle.
        /// </summary>
        /// <param name="other">The RCNumRectangle to test.</param>
        /// <returns>True if other RCNumRectangle has same location and size as this RCNumRectangle.</returns>
        public bool Equals(RCNumRectangle other)
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
            if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCNumRectangle!"); }
            return this.x.GetHashCode() ^ this.y.GetHashCode() ^ this.width.GetHashCode() ^ this.height.GetHashCode();
        }

        /// <summary>
        /// Gets the string representation of this RCNumRectangle.
        /// </summary>
        /// <returns>The string representation of this RCNumRectangle.</returns>
        public override string ToString()
        {
            return string.Format("({0}, {1}, {2}, {3})", this.x, this.y, this.width, this.height);
        }

        /// <summary>
        /// Determines if the specified point is contained within this RCNumRectangle.
        /// </summary>
        /// <param name="x">The horizontal coordinate of the point to test.</param>
        /// <param name="y">The vertical coordinate of the point to test.</param>
        /// <returns>Returns true if the point is contained within this RCNumRectangle, false otherwise.</returns>
        public bool Contains(RCNumber x, RCNumber y)
        {
            if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCNumRectangle!"); }
            return x >= this.x && y >= this.y && x < this.Right && y < this.Bottom;
        }

        /// <summary>
        /// Determines if the specified point is contained within this RCNumRectangle.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <returns>Returns true if the point is contained within this RCNumRectangle, false otherwise.</returns>
        public bool Contains(RCNumVector point)
        {
            if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCNumRectangle!"); }
            if (point == RCNumVector.Undefined) { throw new ArgumentNullException("point"); }
            return this.Contains(point.X, point.Y);
        }

        /// <summary>
        /// Determines if the rectangular region represented by rect is entirely contained within this RCNumRectangle.
        /// </summary>
        /// <param name="rect">The RCNumRectangle to test.</param>
        /// <returns>True if the rectangular region represented by rect is entirely contained within this RCNumRectangle, false otherwise.</returns>
        public bool Contains(RCNumRectangle rect)
        {
            if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCNumRectangle!"); }
            if (!rect.isDefined) { throw new ArgumentNullException("rect"); }

            return this.Contains(rect.x, rect.y) &&
                   this.Contains(rect.x + rect.width - new RCNumber(1), rect.y + rect.height - new RCNumber(1));
        }

        /// <summary>
        /// Determines if this RCNumRectangle intersects with rect.
        /// </summary>
        /// <param name="rect">The RCNumRectangle to test.</param>
        /// <returns>True if there is any intersection, false otherwise.</returns>
        public bool IntersectsWith(RCNumRectangle rect)
        {
            if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCNumRectangle!"); }
            if (!rect.isDefined) { throw new ArgumentNullException("rect"); }

            return (this.Left < rect.Right) && (this.Right > rect.Left) && (this.Top < rect.Bottom) && (this.Bottom > rect.Top);
        }

        /// <summary>
        /// Replaces this RCNumRectangle with the intersection of itself and the specified Rectangle.
        /// </summary>
        /// <param name="other">The other RCNumRectangle to intersect with.</param>
        /// <remarks>
        /// If this and other RCRectangles don't intersect each other, then this RCNumRectangle becomes RCNumRectangle.Undefined.
        /// </remarks>
        public void Intersect(RCNumRectangle other)
        {
            if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCNumRectangle!"); }
            if (!other.isDefined) { throw new ArgumentNullException("other"); }

            this = RCNumRectangle.Intersect(this, other);
        }

        /// <summary>
        /// Returns a third RCNumRectangle that represents the intersection of two other RCRectangles.
        /// If there is no intersection, RCNumRectangle.Undefined is returned.
        /// </summary>
        /// <param name="lRect">An RCNumRectangle to intersect.</param>
        /// <param name="rRect">An RCNumRectangle to intersect.</param>
        /// <returns>
        /// An RCNumRectangle that is the intersection of lRect and rRect or RCNumRectangle.Undefined if there is no intersection.
        /// </returns>
        public static RCNumRectangle Intersect(RCNumRectangle lRect, RCNumRectangle rRect)
        {
            if (!lRect.isDefined) { throw new ArgumentNullException("lRect"); }
            if (!rRect.isDefined) { throw new ArgumentNullException("rRect"); }

            RCNumber intersectLeft, intersectRight, intersectTop, intersectBottom;

            /// Compute the horizontal coordinates of the intersection
            if (!IntersectHorizontal(ref lRect, ref rRect, out intersectLeft, out intersectRight) &&
                !IntersectHorizontal(ref rRect, ref lRect, out intersectLeft, out intersectRight))
            {
                return RCNumRectangle.Undefined;
            }

            /// Compute the vertical coordinates of the intersection
            if (!IntersectVertical(ref lRect, ref rRect, out intersectTop, out intersectBottom) &&
                !IntersectVertical(ref rRect, ref lRect, out intersectTop, out intersectBottom))
            {
                return RCNumRectangle.Undefined;
            }

            return new RCNumRectangle(intersectLeft, intersectTop, intersectRight - intersectLeft, intersectBottom - intersectTop);
        }

        /// <summary>
        /// Gets the horizontal coordinate of the left edge of this RCNumRectangle.
        /// </summary>
        public RCNumber Left
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCNumRectangle!"); }
                return this.x;
            }
        }

        /// <summary>
        /// The horizontal coordinate of the first point at the right edge of this RCNumRectangle that is not contained in the rectangle.
        /// </summary>
        public RCNumber Right
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCNumRectangle!"); }
                return this.x + this.width;
            }
        }

        /// <summary>
        /// Gets the vertical coordinate of the top edge of this RCNumRectangle.
        /// </summary>
        public RCNumber Top
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCNumRectangle!"); }
                return this.y;
            }
        }

        /// <summary>
        /// The vertical coordinate of the first point at the bottom edge of this RCNumRectangle that is not contained in the rectangle.
        /// </summary>
        public RCNumber Bottom
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCNumRectangle!"); }
                return this.y + this.height;
            }
        }

        /// <summary>
        /// Gets or sets the location of the top-left corner of this RCNumRectangle.
        /// </summary>
        public RCNumVector Location
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCNumRectangle!"); }
                return new RCNumVector(this.x, this.y);
            }
            set
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCNumRectangle!"); }
                if (value == RCNumVector.Undefined) { throw new ArgumentNullException("Location"); }
                this.x = value.X;
                this.y = value.Y;
            }
        }

        /// <summary>
        /// Gets or sets the size of this RCNumRectangle.
        /// </summary>
        public RCNumVector Size
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCNumRectangle!"); }
                return new RCNumVector(this.width, this.height);
            }
            set
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCNumRectangle!"); }
                if (value == RCNumVector.Undefined) { throw new ArgumentNullException("Size"); }
                if (value.X <= 0) { throw new ArgumentOutOfRangeException("Width has to be greater than 0!", "Size"); }
                if (value.Y <= 0) { throw new ArgumentOutOfRangeException("Height has to be greater than 0!", "Size"); }
                this.width = value.X;
                this.height = value.Y;
            }
        }

        /// <summary>
        /// Gets or sets the width of this RCNumRectangle.
        /// </summary>
        public RCNumber Width
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCNumRectangle!"); }
                return this.width;
            }
            set
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCNumRectangle!"); }
                if (value <= 0) { throw new ArgumentOutOfRangeException("Width has to be greater than 0!", "Width"); }
                this.width = value;
            }
        }

        /// <summary>
        /// Gets or sets the height of this RCNumRectangle.
        /// </summary>
        public RCNumber Height
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCNumRectangle!"); }
                return this.height;
            }
            set
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCNumRectangle!"); }
                if (value <= 0) { throw new ArgumentOutOfRangeException("Height has to be greater than 0!", "Height"); }
                this.height = value;
            }
        }

        /// <summary>
        /// Gets or sets the horizontal coordinate of the top-left corner of this RCNumRectangle.
        /// </summary>
        public RCNumber X
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCNumRectangle!"); }
                return this.x;
            }
            set
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCNumRectangle!"); }
                this.x = value;
            }
        }

        /// <summary>
        /// Gets or sets the vertical coordinate of the top-left corner of this RCNumRectangle.
        /// </summary>
        public RCNumber Y
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCNumRectangle!"); }
                return this.y;
            }
            set
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCNumRectangle!"); }
                this.y = value;
            }
        }

        /// <summary>
        /// You can use this undefined RCNumRectangle as 'null' in reference types.
        /// </summary>
        public static readonly RCNumRectangle Undefined = new RCNumRectangle();

        #endregion Public fields

        #region Private fields

        /// <summary>
        /// Internal helper function for computing intersections.
        /// </summary>
        private static bool IntersectHorizontal(ref RCNumRectangle lRect, ref RCNumRectangle rRect, out RCNumber intersectLeft, out RCNumber intersectRight)
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
        private static bool IntersectVertical(ref RCNumRectangle lRect, ref RCNumRectangle rRect, out RCNumber intersectTop, out RCNumber intersectBottom)
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
        /// The horizontal coordinate of the top-left corner of this RCNumRectangle.
        /// </summary>
        private RCNumber x;

        /// <summary>
        /// The vertical coordinate of the top-left corner of this RCNumRectangle.
        /// </summary>
        private RCNumber y;

        /// <summary>
        /// The width of this RCNumRectangle.
        /// </summary>
        private RCNumber width;

        /// <summary>
        /// The height of this RCNumRectangle.
        /// </summary>
        private RCNumber height;

        /// <summary>
        /// This flag is true if this is a defined RCNumRectangle.
        /// </summary>
        private bool isDefined;

        #endregion Private fields
    }
}
