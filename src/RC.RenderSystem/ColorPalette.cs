using System;
using System.Collections.Generic;
using System.Drawing;
using RC.Common;

namespace RC.RenderSystem
{
    /// <summary>
    /// This class stores the color palette that can be used for drawing.
    /// </summary>
    public class ColorPalette
    {
        /// <summary>
        /// Constructs a ColorPalette object.
        /// </summary>
        /// <param name="palette">See ColorPalette.palette for more informations.</param>
        /// <param name="paletteMask">See ColorPalette.paletteMask for more informations.</param>
        /// <param name="specialColors">See ColorPalette.specialColors for more informations.</param>
        /// <remarks>
        /// Only the first parameter is mandatory. You can give null references for the others.
        /// </remarks>
        public ColorPalette(Color[] palette,
                            Color[] paletteMask,
                            Color[] specialColors)
        {
            this.palette = null;
            this.paletteMask = null;
            this.specialColors = null;

            /// Checking arguments.
            if (null == palette || 0 == palette.Length)
            {
                throw new ArgumentException("You must define at least 1 color in the palette.", "palette");
            }
            if (null != paletteMask && palette.Length != paletteMask.Length)
            {
                throw new ArgumentException("The arrays palette and paletteMask must have the same length.");
            }

            /// Saving the color palette.
            RCSet<Color> paletteSet = new RCSet<Color>();
            foreach (Color c in palette)
            {
                if (!paletteSet.Contains(c))
                {
                    paletteSet.Add(c);
                }
                else
                {
                    throw new ArgumentException("All colors in the array palette must be unique.", "palette");
                }
            }
            this.palette = palette;

            /// Saving the paletteMask.
            this.paletteMask = new Dictionary<Color, int>();
            if (null != paletteMask)
            {
                int idx = 0;
                foreach (Color c in paletteMask)
                {
                    if (!this.paletteMask.ContainsKey(c))
                    {
                        this.paletteMask.Add(c, idx);
                        idx++;
                    }
                    else
                    {
                        throw new ArgumentException("All colors in the array paletteMask must be unique.", "paletteMask");
                    }
                }
            }
            else
            {
                /// If no palette mask defined, we simply create the inverse of this.palette
                for (int idx = 0; idx < this.palette.Length; ++idx)
                {
                    this.paletteMask.Add(this.palette[idx], idx);
                }
            }

            /// Saving the special colors.
            if (null != specialColors)
            {
                RCSet<Color> specColorSet = new RCSet<Color>();
                foreach (Color c in specialColors)
                {
                    if (!specColorSet.Contains(c))
                    {
                        if (!paletteSet.Contains(c) && !this.paletteMask.ContainsKey(c))
                        {
                            specColorSet.Add(c);
                        }
                        else
                        {
                            throw new ArgumentException("A color defined in the array specialColors has already been defined in the array palette or paletteMask.", "specialColors");
                        }
                    }
                    else
                    {
                        throw new ArgumentException("All colors in the array specialColors must be unique.", "specialColors");
                    }
                }
                this.specialColors = specialColors;
            }
        }

        /// <summary>
        /// Gets the color from the palette specified by the given ID.
        /// </summary>
        /// <param name="colorID">The ID of the color you want to get from the palette.</param>
        /// <returns>The color with the given ID or Color.Empty if no color exists with the given ID.</returns>
        public Color GetColorByID(int colorID)
        {
            if (colorID >= 0 && colorID < this.palette.Length)
            {
                return this.palette[colorID];
            }
            else
            {
                return Color.Empty;
            }
        }

        /// <summary>
        /// Gets the special color from the list specified by the given ID.
        /// </summary>
        /// <param name="specColorID">The ID of the special color you want to get from the list.</param>
        /// <returns>
        /// The special color with the given ID or Color.Empty if no special color exists with the
        /// given ID.
        /// </returns>
        public Color GetSpecialColorByID(int specColorID)
        {
            if (specColorID >= 0 && specColorID < this.specialColors.Length)
            {
                return this.specialColors[specColorID];
            }
            else
            {
                return Color.Empty;
            }
        }

        /// <summary>
        /// Gets the ID of the palette color that is mapped to the given mask color.
        /// </summary>
        /// <param name="mask">The given mask color.</param>
        /// <returns>
        /// The ID of the palette color that is mapped to the given mask color or -1 if the given
        /// mask color doesn't exist.
        /// </returns>
        public int GetIDByMask(Color mask)
        {
            if (this.paletteMask.ContainsKey(mask))
            {
                return this.paletteMask[mask];
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Gets the palette color itself that is mapped to the given mask color.
        /// </summary>
        /// <param name="mask">The given mask color.</param>
        /// <returns>
        /// The palette color itself that is mapped to the given mask color or Color.Empty if the given
        /// mask color doesn't exist.
        /// </returns>
        public Color GetColorByMask(Color mask)
        {
            int colorID = GetIDByMask(mask);
            if (-1 != colorID)
            {
                return GetColorByID(colorID);
            }
            else
            {
                return Color.Empty;
            }
        }

        /// <summary>
        /// Defines the color palette that can be used for drawing. If a client wants to draw something with
        /// a color then he must define the ID of that color. This array maps these IDs to the real colors.
        /// </summary>
        private Color[] palette;

        /// <summary>
        /// Defines the color mask that is used when loading the sprites into the memory. If a sprite bitmap
        /// contains a color from this map, then all pixel with this color will be replaced by the color
        /// with the corresponding ID from the palette.
        /// </summary>
        private Dictionary<Color, int> paletteMask;

        /// <summary>
        /// Defines the colors that have special meaning for the render system. For example a color that is
        /// used to mark transparent pixels in a sprite bitmap.
        /// </summary>
        private Color[] specialColors;
    }
}
