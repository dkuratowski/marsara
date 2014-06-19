using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Common
{
    /// <summary>
    /// Represents an RGB color.
    /// </summary>
    public struct RCColor
    {
        #region Operator overloads

        /// <summary>
        /// Compares two RCColor objects. The result specifies whether the components of the two RCColor objects are equal.
        /// </summary>
        /// <param name="lColor">A RCColor to compare.</param>
        /// <param name="rColor">A RCColor to compare.</param>
        /// <returns>True if the components of lColor and rColor are equal, false otherwise.</returns>
        public static bool operator ==(RCColor lColor, RCColor rColor)
        {
            return lColor.Equals(rColor);
        }

        /// <summary>
        /// Compares two RCColor objects. The result specifies whether the components of the two RCColor objects are unequal.
        /// </summary>
        /// <param name="lColor">A RCColor to compare.</param>
        /// <param name="rColor">A RCColor to compare.</param>
        /// <returns>True if the components of lColor and rColor differ, false otherwise.</returns>
        public static bool operator !=(RCColor lColor, RCColor rColor)
        {
            return !lColor.Equals(rColor);
        }

        #endregion Operator overloads

        #region Predefined colors

        /// <summary>
        /// List of the predefined CGA colors.
        /// </summary>
        public static readonly RCColor Black = new RCColor(0, 0, 0);
        public static readonly RCColor Blue = new RCColor(0, 0, 170);
        public static readonly RCColor Green = new RCColor(0, 170, 0);
        public static readonly RCColor Cyan = new RCColor(0, 170, 170);
        public static readonly RCColor Red = new RCColor(170, 0, 0);
        public static readonly RCColor Magenta = new RCColor(170, 0, 170);
        public static readonly RCColor Brown = new RCColor(170, 85, 0);
        public static readonly RCColor White = new RCColor(170, 170, 170);
        public static readonly RCColor Gray = new RCColor(85, 85, 85);
        public static readonly RCColor LightBlue = new RCColor(85, 85, 255);
        public static readonly RCColor LightGreen = new RCColor(85, 255, 85);
        public static readonly RCColor LightCyan = new RCColor(85, 255, 255);
        public static readonly RCColor LightRed = new RCColor(255, 85, 85);
        public static readonly RCColor LightMagenta = new RCColor(255, 85, 255);
        public static readonly RCColor Yellow = new RCColor(255, 255, 85);
        public static readonly RCColor WhiteHigh = new RCColor(255, 255, 255);

        #endregion Predefiend colors

        #region Public fields

        /// <summary>
        /// Initializes a new RCColor with the specified RGB components.
        /// </summary>
        /// <param name="r">The red component of the RCColor (0-255).</param>
        /// <param name="g">The green component of the RCColor (0-255).</param>
        /// <param name="b">The blue component of the RCColor (0-255).</param>
        public RCColor(int r, int g, int b)
        {
            if (r < 0 || r > 255) { throw new ArgumentOutOfRangeException("r"); }
            if (g < 0 || g > 255) { throw new ArgumentOutOfRangeException("g"); }
            if (b < 0 || b > 255) { throw new ArgumentOutOfRangeException("b"); }

            this.isDefined = true;
            this.r = (byte)r;
            this.g = (byte)g;
            this.b = (byte)b;
        }

        /// <summary>
        /// Initializes a new RCColor with the specified RCColor.
        /// </summary>
        /// <param name="other">The RCColor to initialize with.</param>
        public RCColor(RCColor other)
        {
            if (!other.isDefined) { throw new ArgumentNullException("other"); }

            this.isDefined = true;
            this.r = other.r;
            this.g = other.g;
            this.b = other.b;
        }

        /// <summary>
        /// Checks whether the specified object is a RCColor and contains the same components as this RCColor.
        /// </summary>
        /// <param name="obj">The object to test.</param>
        /// <returns>True if obj is a RCColor and has the same components as this RCColor.</returns>
        public override bool Equals(object obj)
        {
            return (obj is RCColor) && Equals((RCColor)obj);
        }

        /// <summary>
        /// Checks whether this RCColor contains the same components as the specified RCColor.
        /// </summary>
        /// <param name="other">The RCColor to test.</param>
        /// <returns>True if other RCColor has the same components as this RCColor.</returns>
        public bool Equals(RCColor other)
        {
            return (!this.isDefined && !other.isDefined) ||
                   (this.isDefined && other.isDefined && this.r == other.r && this.g == other.g && this.b == other.b);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCColor!"); }
            return this.r ^ this.g ^ this.b;
        }

        /// <summary>
        /// Gets the string representation of this RCColor.
        /// </summary>
        /// <returns>The string representation of this RCColor.</returns>
        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", this.r, this.g, this.b);
        }

        /// <summary>
        /// Gets the red component of this RCColor.
        /// </summary>
        public byte R
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCColor!"); }
                return this.r;
            }
        }

        /// <summary>
        /// Gets the green component of this RCColor.
        /// </summary>
        public byte G
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCColor!"); }
                return this.g;
            }
        }

        /// <summary>
        /// Gets the blue component of this RCColor.
        /// </summary>
        public byte B
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined RCColor!"); }
                return this.b;
            }
        }

        /// <summary>
        /// You can use this undefined RCColor as 'null' in reference types.
        /// </summary>
        public static readonly RCColor Undefined = new RCColor();

        #endregion Public fields

        #region Private fields

        /// <summary>
        /// The red component of this color.
        /// </summary>
        private byte r;

        /// <summary>
        /// The green component of this color.
        /// </summary>
        private byte g;

        /// <summary>
        /// The blue component of this color.
        /// </summary>
        private byte b;

        /// <summary>
        /// This flag is true if this is a defined RCColor.
        /// </summary>
        private bool isDefined;

        #endregion Private fields
    }
}
