using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Common.Configuration
{
    /// <summary>
    /// Represents a multi-variant sprite palette.
    /// </summary>
    class SpritePalette<TVariant> : ISpritePalette<TVariant> where TVariant : struct
    {
        /// <summary>
        /// Constructs a multi-variant sprite palette.
        /// </summary>
        /// <param name="imageData">The byte sequence that contains the image data of this sprite palette.</param>
        /// <param name="transpColor">The the transparent color of the image data or RCColor.Undefined if no transparent color is defined.</param>
        /// <param name="ownerMaskColorStr">The mask color of the image data or RCColor.Undefined if no mask color is defined.</param>
        public SpritePalette(byte[] imageData, RCColor transpColor, RCColor ownerMaskColor)
        {
            if (imageData == null || imageData.Length == 0) { throw new ArgumentNullException("imageData"); }

            this.index = -1;
            this.imageData = imageData;
            this.transparentColor = transpColor;
            this.maskColor = ownerMaskColor;
            this.sourceRegions = new List<RCIntRectangle>();
            this.offsets = new List<RCIntVector>();
            this.indexTable = new Dictionary<string, Dictionary<TVariant, int>>();
        }

        #region ISpritePalette<TVariant> members

        /// <see cref="ISpritePalette<TVariant>.ImageData"/>
        public byte[] ImageData { get { return this.imageData; } }

        /// <see cref="ISpritePalette<TVariant>.TransparentColor"/>
        public RCColor TransparentColor { get { return this.transparentColor; } }

        /// <see cref="ISpritePalette<TVariant>.MaskColor"/>
        public RCColor MaskColor { get { return this.maskColor; } }

        /// <see cref="ISpritePalette<TVariant>.Index"/>
        public int Index
        {
            get
            {
                if (this.index == -1) { throw new InvalidOperationException("Sprite palette index has not yet been set!"); }
                return this.index;
            }
        }

        /// <see cref="ISpritePalette<TVariant>.SetIndex"/>
        public void SetIndex(int newIndex)
        {
            if (this.index != -1) { throw new InvalidOperationException("Sprite palette index already set!"); }
            if (newIndex < 0) { throw new ArgumentOutOfRangeException("newIndex", "The index of the sprite palettes must be non-negative!"); }
            this.index = newIndex;
        }

        /// <see cref="ISpritePalette<TVariant>.GetSpriteIndex"/>
        public int GetSpriteIndex(string spriteName, TVariant variant)
        {
            if (spriteName == null) { throw new ArgumentNullException("spriteName"); }
            if (!this.indexTable.ContainsKey(spriteName)) { throw new InvalidOperationException(string.Format("Sprite with name '{0}' doesn't exist!", spriteName)); }

            return this.indexTable[spriteName].ContainsKey(variant) ? this.indexTable[spriteName][variant] : -1;
        }

        /// <see cref="ISpritePalette<TVariant>.GetSection"/>
        public RCIntRectangle GetSection(int spriteIdx) { return this.sourceRegions[spriteIdx]; }

        /// <see cref="ISpritePalette<TVariant>.GetOffset"/>
        public RCIntVector GetOffset(int spriteIdx) { return this.offsets[spriteIdx]; }

        #endregion ISpritePalette<TVariant> members

        /// <summary>
        /// Adds a sprite to this sprite palette.
        /// </summary>
        /// <param name="name">The name of the sprite to add.</param>
        /// <param name="variant">The variant of the sprite to add.</param>
        /// <param name="sourceRegion">The source region of the sprite to add.</param>
        /// <param name="offset">The offset of the sprite to add.</param>
        internal void AddSprite(string name, TVariant variant, RCIntRectangle sourceRegion, RCIntVector offset)
        {
            if (name == null) { throw new ArgumentNullException("name"); }
            if (sourceRegion == RCIntRectangle.Undefined) { throw new ArgumentNullException("sourceRegion"); }
            if (offset == RCIntVector.Undefined) { throw new ArgumentNullException("offset"); }
            if (this.indexTable.ContainsKey(name) && this.indexTable[name].ContainsKey(variant)) { throw new InvalidOperationException(string.Format("Sprite with name '{0}' and variant '{1}' already exists!", name, variant)); }

            if (!this.indexTable.ContainsKey(name)) { this.indexTable.Add(name, new Dictionary<TVariant, int>()); }
            this.indexTable[name].Add(variant, this.sourceRegions.Count);
            this.sourceRegions.Add(sourceRegion);
            this.offsets.Add(offset);
        }

        /// <summary>
        /// The byte sequence that contains the image data of this sprite palette.
        /// </summary>
        private byte[] imageData;

        /// <summary>
        /// The transparent color of the image data.
        /// </summary>
        private RCColor transparentColor;

        /// <summary>
        /// The mask color of the image data.
        /// </summary>
        private RCColor maskColor;

        /// <summary>
        /// The index of this sprite palette.
        /// </summary>
        private int index;

        /// <summary>
        /// List of the source regions of the sprites.
        /// </summary>
        private List<RCIntRectangle> sourceRegions;

        /// <summary>
        /// List of the offsets of the sprites.
        /// </summary>
        private List<RCIntVector> offsets;

        /// <summary>
        /// The indices of the sprites mapped by their names and variants.
        /// </summary>
        private Dictionary<string, Dictionary<TVariant, int>> indexTable;
    }

    /// <summary>
    /// Represents a single-variant sprite palette.
    /// </summary>
    class SpritePalette : SpritePalette<SpritePalette.DummyEnum>, ISpritePalette
    {
        /// <summary>
        /// Constructs a single-variant sprite palette.
        /// </summary>
        /// <param name="imageData">The byte sequence that contains the image data of this sprite palette.</param>
        /// <param name="transpColor">The the transparent color of the image data or RCColor.Undefined if no transparent color is defined.</param>
        /// <param name="ownerMaskColorStr">The mask color of the image data or RCColor.Undefined if no mask color is defined.</param>
        public SpritePalette(byte[] imageData, RCColor transpColor, RCColor ownerMaskColor)
            : base(imageData, transpColor, ownerMaskColor)
        {
        }

        #region ISpritePalette members

        /// <see cref="ISpritePalette.GetSpriteIndex"/>
        public int GetSpriteIndex(string spriteName)
        {
            return base.GetSpriteIndex(spriteName, DummyEnum.DummyEnumItem);
        }

        #endregion ISpritePalette members

        /// <summary>
        /// Dummy enumeration for re-using the code of the generic base class.
        /// </summary>
        internal enum DummyEnum { DummyEnumItem = 0 }
    }
}
