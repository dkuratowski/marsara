using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Scenarios
{
    /// <summary>
    /// Interface of the sprite palette of scenario element types.
    /// </summary>
    public interface ISpritePalette
    {
        /// <summary>
        /// Gets the byte sequence that contains the image data of this sprite palette.
        /// </summary>
        byte[] ImageData { get; }

        /// <summary>
        /// Gets the string that contains the transparent color of the image data or null if no transparent color is defined.
        /// </summary>
        string TransparentColorStr { get; }

        /// <summary>
        /// Gets the string that contains the owner mask color of the image data or null if no owner mask color is defined.
        /// </summary>
        string OwnerMaskColorStr { get; }

        /// <summary>
        /// Gets the index of this sprite palette inside the metadata.
        /// </summary>
        int Index { get; }

        /// <summary>
        /// Gets the index of the given sprite.
        /// </summary>
        /// <param name="spriteName">The name of the sprite.</param>
        /// <param name="direction">The direction of the sprite or MapDirection.Undefined if the direction is undefined.</param>
        /// <returns>The index of the given sprite or -1 if the given sprite has no variant in the given direction.</returns>
        /// <exception cref="SimulatorException">If no sprite with the given name exists.</exception>
        int GetSpriteIndex(string spriteName, MapDirection direction);

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
}
