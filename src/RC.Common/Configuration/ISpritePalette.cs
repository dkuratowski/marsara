using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Common.Configuration
{
    /// <summary>
    /// Base interface of a sprite palette.
    /// </summary>
    public interface ISpritePaletteBase
    {
        /// <summary>
        /// Gets the byte sequence that contains the image data of this sprite palette.
        /// </summary>
        byte[] ImageData { get; }

        /// <summary>
        /// Gets the transparent color of the image data or RCColor.Undefined if no transparent color is defined.
        /// </summary>
        RCColor TransparentColor { get; }

        /// <summary>
        /// Gets the mask color of the image data or RCColor.Undefined if no mask color is defined.
        /// </summary>
        RCColor MaskColor { get; }

        /// <summary>
        /// Gets the index of this sprite palette.
        /// </summary>
        /// <exception cref="InvalidOperationException">The index has not yet been set for this sprite palette.</exception>
        int Index { get; }

        /// <summary>
        /// Sets the index of this sprite palette.
        /// </summary>
        /// <param name="index">The index of this sprite palette.</param>
        /// <exception cref="InvalidOperationException">The the index has already been set for this sprite palette.</exception>
        void SetIndex(int index);

        /// <summary>
        /// Gets the section of the given sprite.
        /// </summary>
        /// <param name="spriteIdx">The index of the sprite inside the sprite palette.</param>
        /// <returns>The section of the given sprite.</returns>
        RCIntRectangle GetSection(int spriteIdx);

        /// <summary>
        /// Gets the offset of the given sprite.
        /// </summary>
        /// <param name="spriteIdx">The index of the sprite inside the sprite palette.</param>
        /// <returns>The offset of the given sprite.</returns>
        RCIntVector GetOffset(int spriteIdx);
    }

    /// <summary>
    /// Interface of a multi-variant sprite palette.
    /// </summary>
    public interface ISpritePalette<T> : ISpritePaletteBase where T : struct
    {
        /// <summary>
        /// Gets the index of the given sprite.
        /// </summary>
        /// <param name="spriteName">The name of the sprite.</param>
        /// <param name="variant">The variant of the sprite.</param>
        /// <returns>The index of the given sprite or -1 if the given sprite doesn't have the given variant.</returns>
        /// <exception cref="InvalidOperationException">If no sprite with the given name exists in the sprite palette.</exception>
        int GetSpriteIndex(string spriteName, T variant);
    }

    /// <summary>
    /// Interface of a single-variant sprite palette.
    /// </summary>
    public interface ISpritePalette : ISpritePaletteBase
    {
        /// <summary>
        /// Gets the index of the given sprite.
        /// </summary>
        /// <param name="spriteName">The name of the sprite.</param>
        /// <returns>The index of the given sprite.</returns>
        /// <exception cref="InvalidOperationException">If no sprite with the given name exists in the sprite palette.</exception>
        int GetSpriteIndex(string spriteName);
    }
}
