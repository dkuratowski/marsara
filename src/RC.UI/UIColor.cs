using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.UI
{
    /// <summary>
    /// Represents an RGB color.
    /// </summary>
    public struct UIColor
    {
        #region Operator overloads

        /// <summary>
        /// Compares two UIColor objects. The result specifies whether the components of the two UIColor objects are equal.
        /// </summary>
        /// <param name="lColor">A UIColor to compare.</param>
        /// <param name="rColor">A UIColor to compare.</param>
        /// <returns>True if the components of lColor and rColor are equal, false otherwise.</returns>
        public static bool operator ==(UIColor lColor, UIColor rColor)
        {
            return lColor.Equals(rColor);
        }

        /// <summary>
        /// Compares two UIColor objects. The result specifies whether the components of the two UIColor objects are unequal.
        /// </summary>
        /// <param name="lColor">A UIColor to compare.</param>
        /// <param name="rColor">A UIColor to compare.</param>
        /// <returns>True if the components of lColor and rColor differ, false otherwise.</returns>
        public static bool operator !=(UIColor lColor, UIColor rColor)
        {
            return !lColor.Equals(rColor);
        }

        #endregion Operator overloads

        #region Predefined colors

        /// <summary>
        /// List of the predefined CGA colors.
        /// </summary>
        public static readonly UIColor Black = new UIColor(0, 0, 0);
        public static readonly UIColor Blue = new UIColor(0, 0, 170);
        public static readonly UIColor Green = new UIColor(0, 170, 0);
        public static readonly UIColor Cyan = new UIColor(0, 170, 170);
        public static readonly UIColor Red = new UIColor(170, 0, 0);
        public static readonly UIColor Magenta = new UIColor(170, 0, 170);
        public static readonly UIColor Brown = new UIColor(170, 85, 0);
        public static readonly UIColor White = new UIColor(170, 170, 170);
        public static readonly UIColor Gray = new UIColor(85, 85, 85);
        public static readonly UIColor LightBlue = new UIColor(85, 85, 255);
        public static readonly UIColor LightGreen = new UIColor(85, 255, 85);
        public static readonly UIColor LightCyan = new UIColor(85, 255, 255);
        public static readonly UIColor LightRed = new UIColor(255, 85, 85);
        public static readonly UIColor LightMagenta = new UIColor(255, 85, 255);
        public static readonly UIColor Yellow = new UIColor(255, 255, 85);
        public static readonly UIColor WhiteHigh = new UIColor(255, 255, 255);

        #endregion Predefiend colors

        #region Public fields

        /// <summary>
        /// Initializes a new UIColor with the specified RGB components.
        /// </summary>
        /// <param name="r">The red component of the UIColor (0-255).</param>
        /// <param name="g">The green component of the UIColor (0-255).</param>
        /// <param name="b">The blue component of the UIColor (0-255).</param>
        public UIColor(int r, int g, int b)
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
        /// Initializes a new UIColor with the specified UIColor.
        /// </summary>
        /// <param name="other">The UIColor to initialize with.</param>
        public UIColor(UIColor other)
        {
            if (!other.isDefined) { throw new ArgumentNullException("other"); }

            this.isDefined = true;
            this.r = other.r;
            this.g = other.g;
            this.b = other.b;
        }

        /// <summary>
        /// Checks whether the specified object is a UIColor and contains the same components as this UIColor.
        /// </summary>
        /// <param name="obj">The object to test.</param>
        /// <returns>True if obj is a UIColor and has the same components as this UIColor.</returns>
        public override bool Equals(object obj)
        {
            return (obj is UIColor) && Equals((UIColor)obj);
        }

        /// <summary>
        /// Checks whether this UIColor contains the same components as the specified UIColor.
        /// </summary>
        /// <param name="other">The UIColor to test.</param>
        /// <returns>True if other UIColor has the same components as this UIColor.</returns>
        public bool Equals(UIColor other)
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
            if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined UIColor!"); }
            return this.r ^ this.g ^ this.b;
        }

        /// <summary>
        /// Gets the string representation of this UIColor.
        /// </summary>
        /// <returns>The string representation of this UIColor.</returns>
        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", this.r, this.g, this.b);
        }

        /// <summary>
        /// Gets the red component of this UIColor.
        /// </summary>
        public byte R
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined UIColor!"); }
                return this.r;
            }
        }

        /// <summary>
        /// Gets the green component of this UIColor.
        /// </summary>
        public byte G
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined UIColor!"); }
                return this.g;
            }
        }

        /// <summary>
        /// Gets the blue component of this UIColor.
        /// </summary>
        public byte B
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined UIColor!"); }
                return this.b;
            }
        }

        /// <summary>
        /// You can use this undefined UIColor as 'null' in reference types.
        /// </summary>
        public static readonly UIColor Undefined = new UIColor();

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
        /// This flag is true if this is a defined UIColor.
        /// </summary>
        private bool isDefined;

        #endregion Private fields
    }
}
