﻿using System;
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
    }
}
