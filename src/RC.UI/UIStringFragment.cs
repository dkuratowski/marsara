using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.UI
{

    /// <summary>
    /// Contains informations that are needed to a render a fragment of a UIString.
    /// </summary>
    public struct UIStringFragment
    {
        #region Operator overloads

        /// <summary>
        /// Compares two UIStringFragment objects.
        /// </summary>
        /// <param name="lFrag">A UIStringFragment to compare.</param>
        /// <param name="rFrag">A UIStringFragment to compare.</param>
        /// <returns>True if lFrag and rFrag are equal, false otherwise.</returns>
        public static bool operator ==(UIStringFragment lFrag, UIStringFragment rFrag)
        {
            return lFrag.Equals(rFrag);
        }

        /// <summary>
        /// Compares two UIStringFragment objects.
        /// </summary>
        /// <param name="lFrag">A UIStringFragment to compare.</param>
        /// <param name="rFrag">A UIStringFragment to compare.</param>
        /// <returns>True if lFrag and rFrag differs, false otherwise.</returns>
        public static bool operator !=(UIStringFragment lFrag, UIStringFragment rFrag)
        {
            return !lFrag.Equals(rFrag);
        }

        #endregion Operator overloads

        #region Public fields

        /// <summary>
        /// Constructs a UIStringFragment structure.
        /// </summary>
        /// <param name="source">The source sprite of this fragment.</param>
        /// <param name="section">The section to cut from the source sprite of this fragment.</param>
        /// <param name="offset">The offset of this fragment over the baseline of the text.</param>
        /// <param name="cursorStep">
        /// The number of pixels the cursor shall be stepped after rendering of this fragment.
        /// </param>
        public UIStringFragment(UISprite source,
                                RCIntRectangle section,
                                int offset,
                                int cursorStep)
        {
            if ((source == null || section == RCIntRectangle.Undefined) &&
                (source != null || section != RCIntRectangle.Undefined))
            {
                throw new ArgumentException("Both of source and section or none of them must be defined!");
            }

            if (cursorStep < 0) { throw new ArgumentOutOfRangeException("cursorStep"); }

            this.isDefined = true;
            this.source = source;
            this.section = section;
            this.offset = offset;
            this.cursorStep = cursorStep;
        }

        /// <summary>
        /// Constructs a UIStringFragment structure that indicates only a cursor padding.
        /// </summary>
        /// <param name="cursorStep">The amount of the padding in logical pixels.</param>
        public UIStringFragment(int cursorStep)
        {
            if (cursorStep < 0) { throw new ArgumentOutOfRangeException("cursorStep"); }

            this.isDefined = true;
            this.source = null;
            this.section = RCIntRectangle.Undefined;
            this.offset = -1;
            this.cursorStep = cursorStep;
        }

        /// <summary>
        /// Checks whether the specified object is an UIStringFragment and equals with this UIStringFragment.
        /// </summary>
        /// <param name="obj">The object to test.</param>
        /// <returns>True if obj is a UIStringFragment and equals with this UIStringFragment.</returns>
        public override bool Equals(object obj)
        {
            return (obj is UIStringFragment) && Equals((UIStringFragment)obj);
        }

        /// <summary>
        /// Checks whether this UIStringFragment equals with the specified UIStringFragment.
        /// </summary>
        /// <param name="other">The UIStringFragment to test.</param>
        /// <returns>True if other UIStringFragment equals with this UIStringFragment.</returns>
        public bool Equals(UIStringFragment other)
        {
            return (!this.isDefined && !other.isDefined) ||
                   (this.isDefined && other.isDefined &&
                    this.source == other.source &&
                    this.section == other.section &&
                    this.offset == other.offset &&
                    this.cursorStep == other.cursorStep);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined UIStringFragment!"); }
            return this.section.GetHashCode() ^ this.source.GetHashCode() ^ this.offset ^ this.cursorStep;
        }
        
        /// <summary>
        /// Gets or sets the source sprite of this fragment. This property is null in case of cursor padding.
        /// </summary>
        public UISprite Source
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined UIStringFragment!"); }
                return this.source;
            }
        }

        /// <summary>
        /// Gets or sets the section to cut from the source sprite of this fragment. This property is
        /// RCIntRectangle.Undefined in case of cursor padding
        /// </summary>
        public RCIntRectangle Section
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined UIStringFragment!"); }
                return this.section;
            }
        }


        /// <summary>
        /// Gets or sets the offset of this fragment over the baseline of the text. This property is 0
        /// in case of cursor padding.
        /// </summary>
        public int Offset
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined UIStringFragment!"); }
                return this.offset;
            }
        }


        /// <summary>
        /// Gets or sets the number of pixels the cursor shall be stepped after rendering of this fragment.
        /// </summary>
        public int CursorStep
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined UIStringFragment!"); }
                return this.cursorStep;
            }
        }

        /// <summary>
        /// You can use this undefined UIStringFragment as 'null' in reference types.
        /// </summary>
        public static readonly UIStringFragment Undefined = new UIStringFragment();

        #endregion Public fields

        #region Private fields

        /// <summary>
        /// The source sprite of this fragment. This property is null in case of cursor padding.
        /// </summary>
        private UISprite source;

        /// <summary>
        /// The section to cut from the source sprite of this fragment. This property is
        /// RCIntRectangle.Undefined in case of cursor padding
        /// </summary>
        private RCIntRectangle section;

        /// <summary>
        /// The offset of this fragment over the baseline of the text. This property is -1
        /// in case of cursor padding.
        /// </summary>
        private int offset;

        /// <summary>
        /// The number of pixels the cursor shall be stepped after rendering of this fragment.
        /// </summary>
        private int cursorStep;

        /// <summary>
        /// This flag is true if this is a defined UIStringFragment.
        /// </summary>
        private bool isDefined;

        #endregion Private fields
    }
}
