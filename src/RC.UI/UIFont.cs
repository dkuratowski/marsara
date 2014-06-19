using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.Configuration;
using System.Xml.Linq;
using RC.Common;
using System.IO;

namespace RC.UI
{
    /// <summary>
    /// Represents a font definition.
    /// </summary>
    public class UIFont : IDisposable
    {
        /// <summary>
        /// Constructs a UIFont object.
        /// </summary>
        public UIFont(XElement mappingFileRoot, byte[] imageData)
        {
            this.fontSprites = new Dictionary<UIFontSpriteDefinition, UISprite>();
            this.charIds = new Dictionary<char, int>();
            this.charSections = new List<RCIntRectangle>();
            this.charOffsets = new List<int>();

            XElement charMappingsElem = mappingFileRoot.Element(CHAR_MAPPINGS_ELEM);
            XElement transparentColorElem = mappingFileRoot.Element(TRANSPARENT_COLOR_ELEM);
            XElement charMaskColorElem = mappingFileRoot.Element(CHAR_MASK_COLOR_ELEM);
            XAttribute defaultCharAttr = charMappingsElem.Attribute(FONT_DEFATULT_CHAR_ATTR);
            if (charMappingsElem != null && transparentColorElem != null &&
                charMaskColorElem != null && defaultCharAttr != null)
            {
                /// Load the default character
                this.defaultChar = XmlHelper.LoadChar(defaultCharAttr.Value);

                /// Load the space between characters and the minimum space width
                XAttribute spaceBetweenCharsAttr = charMappingsElem.Attribute(SPACE_BETWEEN_CHARS_ATTR);
                XAttribute minimumSpaceWidthAttr = charMappingsElem.Attribute(MINIMUM_SPACE_WIDTH_ATTR);
                if (spaceBetweenCharsAttr != null && minimumSpaceWidthAttr != null)
                {
                    this.spaceBetweenChars = XmlHelper.LoadInt(spaceBetweenCharsAttr.Value);
                    this.minimumSpaceWidth = XmlHelper.LoadInt(minimumSpaceWidthAttr.Value);
                    if (this.spaceBetweenChars < 0)
                    {
                        throw new ConfigurationException("Space between characters out of range!");
                    }
                    if (this.minimumSpaceWidth < 0)
                    {
                        throw new ConfigurationException("Minimum space width out of range!");
                    }
                }
                else
                {
                    throw new ConfigurationException("No space between characters and minimum space width has been defined!");
                }

                /// Load the character mappings
                IEnumerable<XElement> mappingElems = charMappingsElem.Elements(CHAR_MAPPING_ELEM);
                foreach (XElement elem in mappingElems)
                {
                    this.LoadMappingElem(elem);
                }

                /// Load the transparent and masking colors
                this.transparentColor = XmlHelper.LoadColor(transparentColorElem.Value);
                this.characterMaskColor = XmlHelper.LoadColor(charMaskColorElem.Value);
                if (this.transparentColor == this.characterMaskColor)
                {
                    throw new ConfigurationException("Transparent and character masking color cannot be equal!");
                }

                /// Load the font sprite and set it's transparent color as it is defined in the mapping file.
                /// Do not upload this sprite to the graphics device as it will only be used for creating other font sprites.
                this.originalFontSprite = UIRoot.Instance.GraphicsPlatform.SpriteManager.LoadSprite(imageData);
                this.originalFontSprite.TransparentColor = this.characterMaskColor;

                /// Check whether there is a mapping for the default character of the UIFont.
                if (!this.charIds.ContainsKey(this.defaultChar))
                {
                    throw new ConfigurationException("Mapping doesn't exist for default character!");
                }
            }
            else
            {
                /// Error: no namespace, name, sprite or default character defined
                throw new ConfigurationException("No namespace, name, sprite or default character defined for UIFont!");
            }
        }

        #region Public methods and properties

        /// <summary>
        /// Gets the font section and the offset of the given character.
        /// </summary>
        /// <param name="input">The character to search.</param>
        /// <param name="section">The section of the input character or of the default character if not found.</param>
        /// <param name="offset">The offset of the input character or of the default character if not found.</param>
        /// <returns>True if the input character has been found, false otherwise.</returns>
        public bool GetCharacter(char input, out RCIntRectangle section, out int offset)
        {
            if (this.charIds.ContainsKey(input))
            {
                int charId = this.charIds[input];
                section = this.charSections[charId];
                offset = this.charOffsets[charId];
                return true;
            }
            else
            {
                int defaultCharId = this.charIds[this.defaultChar];
                section = this.charSections[defaultCharId];
                offset = this.charOffsets[defaultCharId];
                return false;
            }
        }

        /// <summary>
        /// Gets the font sprite with the given pixel size and font color.
        /// </summary>
        /// <param name="pixelSize">The pixel size of the font sprite.</param>
        /// <param name="fontColor">The color of the font sprite.</param>
        /// <returns>The font sprite with the given pixel size and font color.</returns>
        public UISprite GetFontSprite(RCIntVector pixelSize, RCColor fontColor)
        {
            if (pixelSize == RCIntVector.Undefined) { throw new ArgumentNullException("pixelSize"); }
            if (fontColor == RCColor.Undefined) { throw new ArgumentNullException("fontColor"); }

            UIFontSpriteDefinition spriteDef = new UIFontSpriteDefinition(pixelSize, fontColor);
            if (!this.fontSprites.ContainsKey(spriteDef))
            {
                /// Load the font sprite with the given pixel size and color if necessary.
                UISprite newFontSprite =
                    UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(fontColor,
                                                                                this.originalFontSprite.Size,
                                                                                pixelSize);
                IUIRenderContext newFontSpriteCtx =
                    UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateRenderContext(newFontSprite);
                newFontSpriteCtx.RenderSprite(this.originalFontSprite, new RCIntVector(0, 0));
                UIRoot.Instance.GraphicsPlatform.SpriteManager.CloseRenderContext(newFontSprite);
                newFontSprite.TransparentColor = this.transparentColor;
                newFontSprite.Upload();
                this.fontSprites.Add(spriteDef, newFontSprite);
            }

            return this.fontSprites[spriteDef];
        }

        /// <summary>
        /// Gets the maximum of the characters' top from the baseline in this font (absolute value).
        /// </summary>
        public int CharTopMaximum { get { return this.charTopMax; } }

        /// <summary>
        /// Gets the maximum of the characters' bottom from the baseline in this font (absolute value).
        /// </summary>
        public int CharBottomMaximum { get { return this.charBottomMax; } }

        /// <summary>
        /// Gets the minimum space between lines of a text that is created with this font.
        /// </summary>
        public int MinimumLineHeight { get { return this.charTopMax + this.charBottomMax + 1; } }

        /// <summary>
        /// Gets the minimum width of the space character in logical pixels.
        /// </summary>
        public int MinimumSpaceWidth { get { return this.minimumSpaceWidth; } }

        /// <summary>
        /// Gets the space between the characters in logical pixels.
        /// </summary>
        public int SpaceBetweenChars { get { return this.spaceBetweenChars; } }

        /// <summary>
        /// Gets the transparent color on the font sprite.
        /// </summary>
        public RCColor TransparentColor { get { return this.transparentColor; } }

        /// <summary>
        /// Gets the masking color of the characters on the font sprite.
        /// </summary>
        public RCColor CharacterMaskColor { get { return this.characterMaskColor; } }

        #endregion Public methods and properties

        #region IDisposable Members

        public void Dispose()
        {
            /// Destroy the colored/resized font sprites created by this UIFont object.
            foreach (UISprite sprite in this.fontSprites.Values)
            {
                UIRoot.Instance.GraphicsPlatform.SpriteManager.DestroySprite(sprite);
            }

            UIRoot.Instance.GraphicsPlatform.SpriteManager.DestroySprite(this.originalFontSprite);
            this.defaultChar = (char)0;
            this.charIds.Clear();
            this.charSections.Clear();
            this.charOffsets.Clear();
            this.charTopMax = 0;
            this.charBottomMax = 0;
            this.spaceBetweenChars = 0;
            this.minimumSpaceWidth = 0;
            this.transparentColor = RCColor.Undefined;
            this.characterMaskColor = RCColor.Undefined;
            this.originalFontSprite = null;
        }

        #endregion IDisposable Members

        #region Internal methods

        /// <summary>
        /// Loads a character mapping from the given XML-element.
        /// </summary>
        /// <param name="mappingElem">The XML-element to load from.</param>
        private void LoadMappingElem(XElement mappingElem)
        {
            if (mappingElem.Attribute(CHAR_ATTR) == null ||
                mappingElem.Attribute(BASE_ATTR) == null ||
                mappingElem.Attribute(TOP_ATTR) == null ||
                mappingElem.Attribute(BOTTOM_ATTR) == null ||
                mappingElem.Attribute(WIDTH_ATTR) == null)
            {
                throw new ConfigurationException("Missing character mapping attribute!");
            }

            char mappedChar = XmlHelper.LoadChar(mappingElem.Attribute(CHAR_ATTR).Value);
            RCIntVector baseOfChar = XmlHelper.LoadIntVector(mappingElem.Attribute(BASE_ATTR).Value);
            int top = XmlHelper.LoadInt(mappingElem.Attribute(TOP_ATTR).Value);
            int bottom = XmlHelper.LoadInt(mappingElem.Attribute(BOTTOM_ATTR).Value);
            int width = XmlHelper.LoadInt(mappingElem.Attribute(WIDTH_ATTR).Value);

            if (top <= 0 && bottom >= 0 && width > 0)
            {
                this.charIds.Add(mappedChar, this.charOffsets.Count);
                this.charSections.Add(new RCIntRectangle(baseOfChar.X,
                                                      baseOfChar.Y + top,
                                                      width,
                                                      (baseOfChar.Y + bottom) - (baseOfChar.Y + top) + 1));
                this.charOffsets.Add(top);

                if (Math.Abs(bottom) > this.charBottomMax) { this.charBottomMax = Math.Abs(bottom); }
                if (Math.Abs(top) > this.charTopMax) { this.charTopMax = Math.Abs(top); }
            }
            else
            {
                throw new ConfigurationException("Mapping element error!");
            }
        }

        #endregion Internal methods

        /// <summary>
        /// The default character of this UIFont.
        /// </summary>
        private char defaultChar;

        /// <summary>
        /// Maps the characters to their IDs.
        /// </summary>
        private Dictionary<char, int> charIds;

        /// <summary>
        /// List of the character sections in order of IDs.
        /// </summary>
        private List<RCIntRectangle> charSections;

        /// <summary>
        /// List of the character offsets in order of IDs.
        /// </summary>
        private List<int> charOffsets;

        /// <summary>
        /// The maximum of the characters' top from the baseline in this font (absolute value).
        /// </summary>
        private int charTopMax;

        /// <summary>
        /// The maximum of the characters' bottom from the baseline in this font (absolute value).
        /// </summary>
        private int charBottomMax;

        /// <summary>
        /// The space between the characters in logical pixels.
        /// </summary>
        private int spaceBetweenChars;

        /// <summary>
        /// The minimum width of the space character in logical pixels.
        /// </summary>
        private int minimumSpaceWidth;

        /// <summary>
        /// The transparent color on the font sprite.
        /// </summary>
        private RCColor transparentColor;

        /// <summary>
        /// The masking color of the characters on the font sprite.
        /// </summary>
        private RCColor characterMaskColor;

        /// <summary>
        /// The original font sprite.
        /// </summary>
        private UISprite originalFontSprite;

        /// <summary>
        /// List of the loaded font sprites mapped by their pixel size and color definitions.
        /// </summary>
        private Dictionary<UIFontSpriteDefinition, UISprite> fontSprites;

        /// <summary>
        /// Supported XML elements and attributes in font definition files.
        /// </summary>
        private const string FONT_DEFATULT_CHAR_ATTR = "defaultChar";
        private const string TRANSPARENT_COLOR_ELEM = "transparentColor";
        private const string CHAR_MASK_COLOR_ELEM = "characterMaskColor";
        private const string CHAR_MAPPINGS_ELEM = "characterMappings";
        private const string SPACE_BETWEEN_CHARS_ATTR = "spaceBetweenChars";
        private const string MINIMUM_SPACE_WIDTH_ATTR = "minimumSpaceWidth";
        private const string CHAR_MAPPING_ELEM = "characterMapping";
        private const string CHAR_ATTR = "char";
        private const string BASE_ATTR = "base";
        private const string TOP_ATTR = "top";
        private const string BOTTOM_ATTR = "bottom";
        private const string WIDTH_ATTR = "width";
    }

    /// <summary>
    /// Defines the pixel size and the color of a font sprite.
    /// </summary>
    struct UIFontSpriteDefinition
    {
        #region Operator overloads

        /// <summary>
        /// Compares two UIFontSpriteDefinition objects. The result specifies whether the pixel size and font color
        /// of the two UIFontSpriteDefinition objects are equal.
        /// </summary>
        /// <param name="lDef">A UIFontSpriteDefinition to compare.</param>
        /// <param name="rDef">A UIFontSpriteDefinition to compare.</param>
        /// <returns>True if the pixel size and font color of lDef and rDef are equal, false otherwise.</returns>
        public static bool operator ==(UIFontSpriteDefinition lDef, UIFontSpriteDefinition rDef)
        {
            return lDef.Equals(rDef);
        }

        /// <summary>
        /// Compares two UIFontSpriteDefinition objects. The result specifies whether the pixel size and font color
        /// of the two UIFontSpriteDefinition objects are unequal.
        /// </summary>
        /// <param name="lDef">A UIFontSpriteDefinition to compare.</param>
        /// <param name="rDef">A UIFontSpriteDefinition to compare.</param>
        /// <returns>True if the pixel size and font color of lDef and rDef differ, false otherwise.</returns>
        public static bool operator !=(UIFontSpriteDefinition lDef, UIFontSpriteDefinition rDef)
        {
            return !lDef.Equals(rDef);
        }

        #endregion Operator overloads

        #region Public fields
        
        /// <summary>
        /// Initializes a new UIFontSpriteDefinition with the specified pixel size and font color.
        /// </summary>
        /// <param name="pixelSize">The pixel size of the font sprite.</param>
        /// <param name="fontColor">The color of the font sprite.</param>
        public UIFontSpriteDefinition(RCIntVector pixelSize, RCColor fontColor)
        {
            if (pixelSize == RCIntVector.Undefined) { throw new ArgumentNullException("pixelSize"); }
            if (fontColor == RCColor.Undefined) { throw new ArgumentNullException("fontColor"); }

            this.isDefined = true;
            this.pixelSize = pixelSize;
            this.fontColor = fontColor;
        }

        /// <summary>
        /// Initializes a new UIFontSpriteDefinition with the specified UIFontSpriteDefinition.
        /// </summary>
        /// <param name="other">The UIFontSpriteDefinition to initialize with.</param>
        public UIFontSpriteDefinition(UIFontSpriteDefinition other)
        {
            if (!other.isDefined) { throw new ArgumentNullException("other"); }

            this.isDefined = true;
            this.pixelSize = other.pixelSize;
            this.fontColor = other.fontColor;
        }

        /// <summary>
        /// Checks whether the specified object is a UIFontSpriteDefinition and contains the same components as this
        /// UIFontSpriteDefinition.
        /// </summary>
        /// <param name="obj">The object to test.</param>
        /// <returns>
        /// True if obj is a UIFontSpriteDefinition and has the same components as this UIFontSpriteDefinition.
        /// </returns>
        public override bool Equals(object obj)
        {
            return (obj is UIFontSpriteDefinition) && Equals((UIFontSpriteDefinition)obj);
        }

        /// <summary>
        /// Checks whether this UIFontSpriteDefinition contains the same components as the specified UIFontSpriteDefinition.
        /// </summary>
        /// <param name="other">The UIFontSpriteDefinition to test.</param>
        /// <returns>
        /// True if other UIFontSpriteDefinition has the same components as this UIFontSpriteDefinition.
        /// </returns>
        public bool Equals(UIFontSpriteDefinition other)
        {
            return (!this.isDefined && !other.isDefined) ||
                   (this.isDefined && other.isDefined &&
                    this.pixelSize == other.pixelSize &&
                    this.fontColor == other.fontColor);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined UIFontSpriteDefinition!"); }
            return this.pixelSize.GetHashCode() ^ this.fontColor.GetHashCode();
        }

        /// <summary>
        /// Gets the pixel size of the font sprite.
        /// </summary>
        public RCIntVector PixelSize
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined UIFontSpriteDefinition"); }
                return this.pixelSize;
            }
        }

        /// <summary>
        /// Gets the color of the font sprite.
        /// </summary>
        public RCColor FontColor
        {
            get
            {
                if (!this.isDefined) { throw new InvalidOperationException("Illegal use of undefined UIFontSpriteDefinition"); }
                return this.fontColor;
            }
        }

        /// <summary>
        /// You can use this undefined UIFontSpriteDefinition as 'null' in reference types.
        /// </summary>
        public static readonly UIFontSpriteDefinition Undefined = new UIFontSpriteDefinition();

        #endregion Public fields

        #region Private fields

        /// <summary>
        /// The pixel size of the font sprite.
        /// </summary>
        private RCIntVector pixelSize;

        /// <summary>
        /// The color of the font sprite.
        /// </summary>
        private RCColor fontColor;

        /// <summary>
        /// This flag is true if this is a defined UIFontSpriteDefinition.
        /// </summary>
        private bool isDefined;

        #endregion Private fields
    }
}
