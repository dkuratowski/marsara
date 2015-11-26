using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RC.Common;

namespace RC.UI
{
    /// <summary>
    /// Represents a string that can be rendered to an IUIRenderContext.
    /// </summary>
    public class UIString : IDisposable
    {
        /// <summary>
        /// Constructs a UIString from the given composite format string.
        /// </summary>
        /// <param name="format">The composite format of the UIString.</param>
        /// <param name="font">The font of the string.</param>
        /// <param name="pixelSize">The pixel size of the string.</param>
        /// <param name="color">The color of the string.</param>
        /// <remarks>
        /// The composite format string consists of zero or more runs of text intermixed with zero or more indexed
        /// placeholders, called format items, that correspond to the variable sections of this UIString.
        /// The formatting process replaces each format item with the corresponding variable section of this UIString.
        /// The syntax of a format item is as follows: {index}.
        /// Here the index is an integer that represents the index of the corresponding variable section.
        /// The leading and trailing brace characters, "{" and "}", are required. To specify a single literal brace
        /// character in format, specify two leading or trailing brace characters; that is, "{{" or "}}".
        /// Note that every whitespace characters in the format string will be replaced by spaces.
        /// </remarks>
        public UIString(string format, UIFont font, RCIntVector pixelSize, RCColor color)
        {
            if (format == null || format.Length == 0) { throw new ArgumentNullException("format"); }
            if (font == null) { throw new ArgumentNullException("font"); }
            if (pixelSize == RCIntVector.Undefined) { throw new ArgumentNullException("pixelSize"); }
            if (color == RCColor.Undefined) { throw new ArgumentNullException("color"); }

            /// Parse the incoming format string.
            format = Regex.Replace(format, "\\s", " ");
            List<PlaceHolder> placeHolders = SearchPlaceHolders(format);
            this.constantParts = ChopFormatString(format, placeHolders);

            /// Create or collect the necessary images
            this.font = font;
            this.fontSprite = this.font.GetFontSprite(pixelSize, color);
            this.stringRibbon = CreateEmptyStringRibbon(this.constantParts, this.font, this.fontSprite);

            this.variableIndices = new List<int>();
            foreach (PlaceHolder ph in placeHolders)
            {
                this.variableIndices.Add(ph.Number);
            }

            /// Fill the ribbon and create the internal data structures for rendering this string.
            this.Initialize();
            if (this.stringRibbon != null)
            {
                //this.stringRibbon.Save(format + ".png");
                this.stringRibbon.TransparentColor = this.fontSprite.TransparentColor;
                this.stringRibbon.Upload();
            }
            this.allFragmentCache = null;
            this.widthCache = new CachedValue<int>(this.ComputeWidth);
        }

        /// <summary>
        /// Sets a parameter of this UIString.
        /// </summary>
        /// <param name="index">The index of the parameter to set.</param>
        public object this[int index]
        {
            set
            {
                if (!this.variableParts.ContainsKey(index)) { throw new ArgumentException("Variable with the given index doesn't exist!", "index"); }

                string newValue = value != null ? value.ToString() : null;
                string currentValue = this.variableParts[index];

                /// Set only if the new value differs from the current.
                if (newValue != currentValue)
                {
                    if (newValue != null)
                    {
                        /// Build up the fragment list for the new variable part
                        List<UIStringFragment> newVarFragments = new List<UIStringFragment>();
                        for (int i = 0; i < newValue.Length; ++i)
                        {
                            if (newValue[i] == ' ')
                            {
                                newVarFragments.Add(new UIStringFragment(this.font.MinimumSpaceWidth));
                                continue;
                            }

                            RCIntRectangle fontSection;
                            int offset;
                            this.font.GetCharacter(newValue[i], out fontSection, out offset);

                            int cursorStep = (i < newValue.Length - 1 && newValue[i + 1] != ' ')
                                           ? (fontSection.Width + this.font.SpaceBetweenChars)
                                           : (fontSection.Width);
                            newVarFragments.Add(new UIStringFragment(this.fontSprite, fontSection, offset, cursorStep));
                        }

                        this.variableParts[index] = newValue;
                        this.variableFragments[index] = newVarFragments;
                    }
                    else
                    {
                        this.variableParts[index] = null;
                        this.variableFragments[index] = null;
                    }

                    /// Delete the cache because it has been changed.
                    this.allFragmentCache = null;
                    this.widthCache.Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets the enumerable collection of the constant and variable fragments inside this UIString.
        /// </summary>
        public IEnumerable<UIStringFragment> Fragments
        {
            get
            {
                if (this.allFragmentCache == null)
                {
                    this.allFragmentCache = new List<UIStringFragment>();
                    int currVariableOccurence = 0;
                    bool charPaddingNecessary = false;
                    for (int i = 0; i < this.constantFragments.Count; ++i)
                    {
                        UIStringFragment constFragment = this.constantFragments[i];
                        if (constFragment != UIStringFragment.Undefined)
                        {
                            /// If the previous fragment said that character padding is necessary and
                            /// the current fragment doesn't starts with whitespace then insert a
                            /// character padding.
                            string constPart = this.constantParts[i];
                            if (charPaddingNecessary && !constPart.StartsWith(" "))
                            {
                                this.allFragmentCache.Add(new UIStringFragment(this.font.SpaceBetweenChars));
                            }

                            /// Current fragment is a constant fragment
                            this.allFragmentCache.Add(constFragment);

                            /// Character padding can be necessary if the current fragment doesn't ends with
                            /// whitespace.
                            charPaddingNecessary = !constPart.EndsWith(" ");
                        }
                        else
                        {
                            /// Current fragment is a variable fragment
                            int varIndex = this.variableIndices[currVariableOccurence];
                            List<UIStringFragment> varFragments = this.variableFragments[varIndex];
                            if (varFragments != null)
                            {
                                /// If the previous fragment said that character padding is necessary and
                                /// the current fragment doesn't starts with whitespace then insert a
                                /// character padding.
                                string variablePart = this.variableParts[varIndex];
                                if (charPaddingNecessary && !variablePart.StartsWith(" "))
                                {
                                    this.allFragmentCache.Add(new UIStringFragment(this.font.SpaceBetweenChars));
                                }

                                foreach (UIStringFragment varFragment in varFragments)
                                {
                                    this.allFragmentCache.Add(varFragment);
                                }

                                /// Character padding can be necessary if the current fragment doesn't ends with
                                /// whitespace.
                                charPaddingNecessary = !variablePart.EndsWith(" ");
                            }
                            currVariableOccurence++;
                        }
                    }
                }

                return this.allFragmentCache;
            }
        }

        /// <summary>
        /// Gets the width of this UIString in logical pixels.
        /// </summary>
        public int Width { get { return this.widthCache.Value; } }

        /// <summary>
        /// Gets the UIFont of this UIString.
        /// </summary>
        public UIFont Font { get { return this.font; } }

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            if (this.stringRibbon != null)
            {
                UIRoot.Instance.GraphicsPlatform.SpriteManager.DestroySprite(this.stringRibbon);
            }
        }

        #region Internal methods

        /// <summary>
        /// Builds up the internal data structures of this UIString and fill the string ribbon.
        /// </summary>
        private void Initialize()
        {
            this.constantFragments = new List<UIStringFragment>();
            this.variableFragments = new Dictionary<int, List<UIStringFragment>>();
            this.variableParts = new Dictionary<int, string>();

            IUIRenderContext ctx = this.stringRibbon != null
                                 ? UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateRenderContext(this.stringRibbon)
                                 : null;

            int cursorX = 0;
            int cursorY = this.font.CharTopMaximum;

            int currentVarOccurence = 0;

            for (int i = 0; i < this.constantParts.Count; ++i)
            {
                string fragment = this.constantParts[i];
                if (fragment != null)
                {
                    int fragmentStartX = cursorX;

                    /// Constant fragment
                    for (int j = 0; j < fragment.Length; ++j)
                    {
                        if (fragment[j] == ' ')
                        {
                            cursorX += this.font.MinimumSpaceWidth;
                            continue;
                        }

                        RCIntRectangle fontSection;
                        int offset;
                        this.font.GetCharacter(fragment[j], out fontSection, out offset);

                        ctx.RenderSprite(this.fontSprite,
                                         new RCIntVector(cursorX, cursorY + offset),
                                         fontSection);

                        cursorX += fontSection.Width;
                        if (j < fragment.Length - 1 && fragment[j + 1] != ' ')
                        {
                            cursorX += this.font.SpaceBetweenChars;
                        }
                    }

                    /// End of constant fragment
                    int fragmentWidth = cursorX - fragmentStartX;
                    this.constantFragments.Add(new UIStringFragment(this.stringRibbon,
                                                                    new RCIntRectangle(fragmentStartX, 0, fragmentWidth, this.stringRibbon.Size.Y),
                                                                    (-1) * cursorY,
                                                                    fragmentWidth));
                }
                else
                {
                    /// Variable fragment
                    int varIndex = this.variableIndices[currentVarOccurence];
                    if (!this.variableParts.ContainsKey(varIndex))
                    {
                        this.variableParts[varIndex] = null;
                    }
                    if (!this.variableFragments.ContainsKey(varIndex))
                    {
                        this.variableFragments[varIndex] = null;
                    }
                    this.constantFragments.Add(UIStringFragment.Undefined);
                    currentVarOccurence++;
                }
            }

            if (this.stringRibbon != null) { UIRoot.Instance.GraphicsPlatform.SpriteManager.CloseRenderContext(this.stringRibbon); }
        }

        /// <summary>
        /// Computes the width of this UIString in logical pixels.
        /// </summary>
        /// <returns>The width of this UIString in logical pixels.</returns>
        private int ComputeWidth()
        {
            int width = 0;
            foreach (UIStringFragment fragment in this.Fragments)
            {
                width += fragment.CursorStep;
            }
            return width;
        }

        #endregion Internal methods

        #region Private static format string parsing methods

        /// <summary>
        /// Searches any placeholders in the given format string.
        /// </summary>
        /// <param name="format">The format string to search in.</param>
        /// <returns>The list of the found placeholders.</returns>
        private static List<PlaceHolder> SearchPlaceHolders(string format)
        {
            RCSet<int> escapedOpeningBraces = SearchForwardInFormatString(format, "{{");
            RCSet<int> escapedClosingBraces = SearchBackwardInFormatString(format, "}}");

            List<PlaceHolder> retList = new List<PlaceHolder>();
            int startSearchFrom = 0;
            while (startSearchFrom < format.Length)
            {
                int idxOfOpeningBrace = format.IndexOf('{', startSearchFrom);
                if (idxOfOpeningBrace == -1) { break; } /// No more placeholders

                if (escapedOpeningBraces.Contains(idxOfOpeningBrace))
                {
                    /// Part of an escaped opening brace
                    startSearchFrom = idxOfOpeningBrace + 2;
                    continue;
                }
                if (escapedOpeningBraces.Contains(idxOfOpeningBrace - 1))
                {
                    /// Part of an escaped opening brace
                    startSearchFrom = idxOfOpeningBrace + 1;
                    continue;
                }

                /// Opening brace of placeholder found, search for closing brace
                startSearchFrom = idxOfOpeningBrace + 1;
                if (startSearchFrom == format.Length) { throw new ArgumentException("Format string parsing error!", "format"); }

                int idxOfClosingBrace = format.IndexOf('}', startSearchFrom);
                if (idxOfClosingBrace == -1) { throw new ArgumentException("Format string parsing error!", "format"); }
                if (escapedClosingBraces.Contains(idxOfClosingBrace) ||
                    escapedClosingBraces.Contains(idxOfClosingBrace - 1))
                {
                    /// Part of an escaped closing brace
                    throw new ArgumentException("Format string parsing error!", "format");
                }

                /// Try to parse the placeholder string
                string placeholderNumStr = format.Substring(idxOfOpeningBrace + 1, idxOfClosingBrace - idxOfOpeningBrace - 1);
                int placeholderNum;
                if (placeholderNumStr == string.Empty || !int.TryParse(placeholderNumStr, out placeholderNum))
                {
                    throw new ArgumentException("Format string parsing error!", "format");
                }

                /// Create the new placeholder
                PlaceHolder newPlaceholder = new PlaceHolder()
                {
                    StartIndex = idxOfOpeningBrace,
                    Length = idxOfClosingBrace - idxOfOpeningBrace + 1,
                    Number = placeholderNum
                };
                retList.Add(newPlaceholder);

                startSearchFrom = idxOfClosingBrace + 1;
            }
            return retList;
        }

        /// <summary>
        /// Chops the format string into fragments.
        /// </summary>
        /// <param name="format">The format string to chop.</param>
        /// <param name="placeHolders">The placeholders for the variable fragments</param>
        /// <returns>The list of all fragments. The variable fragments will be null in this list.</returns>
        private static List<string> ChopFormatString(string format, List<PlaceHolder> placeHolders)
        {
            List<string> retList = new List<string>();
            int constFragmentStart = 0;
            foreach (PlaceHolder ph in placeHolders)
            {
                /// Compute the position of the next constant fragment
                int constFragmentLength = ph.StartIndex - constFragmentStart;

                /// Create the next constant fragment
                string constFragment = format.Substring(constFragmentStart, constFragmentLength);
                constFragment = constFragment.Replace("{{", "{").Replace("}}", "}");
                if (constFragment.Length == 0)
                {
                    /// Zero-length constant fragment
                    retList.Add(null);
                    constFragmentStart = ph.StartIndex + ph.Length;
                    continue;
                }

                /// Add the constant fragment and the null for the variable fragment to the returned list
                retList.Add(constFragment);
                retList.Add(null);
                constFragmentStart = ph.StartIndex + ph.Length;
            }

            /// Create the last constant fragment
            string lastConstFragment = format.Substring(constFragmentStart);
            lastConstFragment = lastConstFragment.Replace("{{", "{").Replace("}}", "}");
            if (lastConstFragment.Length > 0)
            {
                retList.Add(lastConstFragment);
            }

            return retList;
        }

        /// <summary>
        /// Creates an empty string ribbon from the given fragments.
        /// </summary>
        /// <param name="fragmentStrings">The fragments of the string.</param>
        /// <param name="font">The font of the string.</param>
        /// <param name="fontSprite">The font sprite.</param>
        /// <returns>
        /// An empty string ribbon that can be filled with the constant fragments of the string or null if the string has no constant fragments.
        /// </returns>
        private static UISprite CreateEmptyStringRibbon(List<string> fragmentStrings, UIFont font, UISprite fontSprite)
        {
            /// Compute the width of the ribbon.
            int ribbonWidth = 0;
            for (int i = 0; i < fragmentStrings.Count; ++i)
            {
                string fragment = fragmentStrings[i];
                if (fragment != null)
                {
                    for (int j = 0; j < fragment.Length; ++j)
                    {
                        if (fragment[j] == ' ')
                        {
                            ribbonWidth += font.MinimumSpaceWidth;
                            continue;
                        }

                        RCIntRectangle fontSection;
                        int offset;
                        font.GetCharacter(fragment[j], out fontSection, out offset);
                        ribbonWidth += fontSection.Width;
                        if (j < fragment.Length - 1 && fragment[j + 1] != ' ')
                        {
                            ribbonWidth += font.SpaceBetweenChars;
                        }
                    }
                }
            }

            /// Creates the empty ribbon.
            UISprite stringRibbon = ribbonWidth > 0
                ? UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(
                    font.TransparentColor,
                    new RCIntVector(ribbonWidth, font.MinimumLineHeight),
                    fontSprite.PixelSize)
                : null;
            return stringRibbon;
        }

        /// <summary>
        /// Searches the given character sequence in the given format string starting from the beginning
        /// of the format string.
        /// </summary>
        /// <param name="format">The format string to search in.</param>
        /// <param name="sequence">The sequence to search.</param>
        /// <returns>
        /// The list that contains the starting index of every occurence of the searched sequence.
        /// </returns>
        private static RCSet<int> SearchForwardInFormatString(string format, string sequence)
        {
            if (sequence == null || sequence.Length == 0) { throw new ArgumentNullException("sequence"); }

            RCSet<int> retList = new RCSet<int>();
            int startSearchFrom = 0;
            while (startSearchFrom < format.Length)
            {
                int idxOfOccurence = format.IndexOf(sequence, startSearchFrom);
                if (idxOfOccurence == -1) { break; } /// No more occurence
                retList.Add(idxOfOccurence);
                startSearchFrom = idxOfOccurence + sequence.Length;
            }
            return retList;
        }

        /// <summary>
        /// Searches the given character sequence in the given format string starting from the end
        /// of the format string.
        /// </summary>
        /// <param name="format">The format string to search in.</param>
        /// <param name="sequence">The sequence to search.</param>
        /// <returns>
        /// The list that contains the starting index of every occurence of the searched sequence.
        /// </returns>
        private static RCSet<int> SearchBackwardInFormatString(string format, string sequence)
        {
            if (sequence == null || sequence.Length == 0) { throw new ArgumentNullException("sequence"); }

            RCSet<int> retList = new RCSet<int>();
            int startSearchFrom = format.Length - 1;
            while (startSearchFrom >= 0)
            {
                int idxOfOccurence = format.LastIndexOf(sequence, startSearchFrom);
                if (idxOfOccurence == -1) { break; } /// No more occurence
                retList.Add(idxOfOccurence);
                startSearchFrom = idxOfOccurence - 1;
            }
            return retList;
        }

        /// <summary>
        /// Represents a placeholder in the format string.
        /// </summary>
        private struct PlaceHolder
        {
            public int Number;
            public int StartIndex;
            public int Length;
        }

        #endregion Private static format string parsing methods

        /// <summary>
        /// Cached list of all fragments for rendering. This cache is invalidated every time a variable has been changed.
        /// </summary>
        private List<UIStringFragment> allFragmentCache;

        /// <summary>
        /// List of the constant fragments in this UIString. The places of variable fragments are indicated
        /// with UIStringFragment.Undefined in this list.
        /// </summary>
        private List<UIStringFragment> constantFragments;

        /// <summary>
        /// List of the constant parts in this UIString. The places of variable parts are indicated with nulls.
        /// </summary>
        private List<string> constantParts;

        /// <summary>
        /// The nth element in this list indicates the index of the nth variable fragment in this UIString.
        /// </summary>
        private List<int> variableIndices;

        /// <summary>
        /// List of the variable fragments in this UIString mapped by their indices.
        /// </summary>
        private Dictionary<int, List<UIStringFragment>> variableFragments;

        /// <summary>
        /// List of the variable parts of this UIString mapped by their indices.
        /// </summary>
        private Dictionary<int, string> variableParts;

        /// <summary>
        /// The font sprite with the correct color and pixel size.
        /// </summary>
        private UISprite fontSprite;

        /// <summary>
        /// The ribbon that contains the constant fragments of this UIString or null if this UIString has no constant fragments.
        /// </summary>
        private UISprite stringRibbon;

        /// <summary>
        /// The font of this UIString.
        /// </summary>
        private UIFont font;

        /// <summary>
        /// Cache of the width of this UIString.
        /// </summary>
        private CachedValue<int> widthCache;
    }
}
