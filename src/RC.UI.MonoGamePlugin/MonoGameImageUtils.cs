using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using RC.Common;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace RC.UI.MonoGamePlugin
{
    /// <summary>
    /// Static class for operations on SixLabors.ImageSharp.Image instances.
    /// </summary>
    static class MonoGameImageUtils
    {
        /// <summary>
        /// Copies the scaled pixels of the source RGB image to the scaled pixels of the target RGB image.
        /// </summary>
        /// <param name="source">The source image.</param>
        /// <param name="target">The target image.</param>
        /// <param name="srcPixelSize">The pixel size of the source image.</param>
        /// <param name="tgtPixelSize">The pixel size of the target image.</param>
        public static void CopyImageScaled(Image<Rgb24> source, Image<Rgb24> target,
                                           RCIntVector srcPixelSize, RCIntVector tgtPixelSize)
        {
            CopyImageScaled(source, target, srcPixelSize, tgtPixelSize,
                new RCIntRectangle(0, 0, source.Size().Width / srcPixelSize.X, source.Size().Height / srcPixelSize.Y),
                new RCIntVector(0, 0),
                RCColor.Undefined);
        }

        /// <summary>
        /// Copies the scaled pixels of the source RGB image to the scaled pixels of the target RGB image with an optional
        /// transparent color defined for the source image.
        /// </summary>
        /// <param name="source">The source image.</param>
        /// <param name="target">The target image.</param>
        /// <param name="srcPixelSize">The pixel size of the source image.</param>
        /// <param name="tgtPixelSize">The pixel size of the target image.</param>
        /// <param name="transparentColor">
        /// The optional transparent color defined for the source image or RCColor.Undefined if such color is not defined.
        /// Pixels of the source image with this color won't overwrite the corresponding pixel of the target image.
        /// </param>
        public static void CopyImageScaled(Image<Rgb24> source, Image<Rgb24> target,
                                           RCIntVector srcPixelSize, RCIntVector tgtPixelSize,
                                           RCColor transparentColor)
        {
            CopyImageScaled(source, target, srcPixelSize, tgtPixelSize,
                new RCIntRectangle(0, 0, source.Size().Width / srcPixelSize.X, source.Size().Height / srcPixelSize.Y),
                new RCIntVector(0, 0),
                transparentColor);
        }

        /// <summary>
        /// Copies a section of scaled pixels from the source RGB image to the scaled pixels of the target RGB image
        /// to a given position.
        /// </summary>
        /// <param name="source">The source image.</param>
        /// <param name="target">The target image.</param>
        /// <param name="srcPixelSize">The pixel size of the source image.</param>
        /// <param name="tgtPixelSize">The pixel size of the target image.</param>
        /// <param name="srcSection">The section of the source image to copy.</param>
        /// <param name="tgtPosition">The position in the target image to copy to.</param>
        public static void CopyImageScaled(Image<Rgb24> source, Image<Rgb24> target,
                                           RCIntVector srcPixelSize, RCIntVector tgtPixelSize,
                                           RCIntRectangle srcSection, RCIntVector tgtPosition)
        {
            CopyImageScaled(source, target, srcPixelSize, tgtPixelSize,
                srcSection,
                tgtPosition,
                RCColor.Undefined);
        }

        /// <summary>
        /// Copies a section of scaled pixels from the source RGB image to the scaled pixels of the target RGB image
        /// to a given position.
        /// </summary>
        /// <param name="source">The source image.</param>
        /// <param name="target">The target image.</param>
        /// <param name="srcPixelSize">The pixel size of the source image.</param>
        /// <param name="tgtPixelSize">The pixel size of the target image.</param>
        /// <param name="srcSection">The section of the source image to copy.</param>
        /// <param name="tgtPosition">The position in the target image to copy to.</param>
        /// <param name="transparentColor">
        /// The optional transparent color defined for the source image or RCColor.Undefined if such color is not defined.
        /// Pixels of the source image with this color won't overwrite the corresponding pixel of the target image.
        /// </param>
        public static void CopyImageScaled(Image<Rgb24> source, Image<Rgb24> target,
                                           RCIntVector srcPixelSize, RCIntVector tgtPixelSize,
                                           RCIntRectangle srcSection, RCIntVector tgtPosition,
                                           RCColor transparentColor)
        {
            /// Check the parameters.
            if (source == null) { throw new ArgumentNullException("source"); }
            if (target == null) { throw new ArgumentNullException("target"); }
            if (srcPixelSize == RCIntVector.Undefined) { throw new ArgumentNullException("srcPixelSize"); }
            if (tgtPixelSize == RCIntVector.Undefined) { throw new ArgumentNullException("tgtPixelSize"); }
            if (srcSection == RCIntRectangle.Undefined) { throw new ArgumentNullException("srcSection"); }
            if (tgtPosition == RCIntVector.Undefined) { throw new ArgumentNullException("tgtPosition"); }

            /// Compute the source and the target sections.
            srcSection.Intersect(new RCIntRectangle(0, 0, source.Size().Width / srcPixelSize.X, source.Size().Height / srcPixelSize.Y));
            if (srcSection == RCIntRectangle.Undefined) { return; }
            RCIntRectangle tgtSection = new RCIntRectangle(tgtPosition.X, tgtPosition.Y, srcSection.Width, srcSection.Height);
            tgtSection.Intersect(new RCIntRectangle(0, 0, target.Size().Width / tgtPixelSize.X, target.Size().Height / tgtPixelSize.Y));
            if (tgtSection == RCIntRectangle.Undefined) { return; }

            source.ProcessPixelRows(srcAccessor =>
            {
                target.ProcessPixelRows(tgtAccessor =>
                {
                    // Iterate through the logical pixel rows of the source section.
                    for (int row = 0; row < srcSection.Height; row++)
                    {
                        Span<Rgb24> srcPixelRow = srcAccessor.GetRowSpan((srcSection.Top + row) * srcPixelSize.Y);
                        for (int subPixelRow = 0; subPixelRow < tgtPixelSize.Y; subPixelRow++)
                        {
                            Span<Rgb24> tgtPixelRow = tgtAccessor.GetRowSpan((tgtPosition.Y + row) * tgtPixelSize.Y + subPixelRow);
                            for (int col = 0; col < srcSection.Width; col++)
                            {
                                // Copy the source pixel if its not transparent.
                                ref Rgb24 srcPixel = ref srcPixelRow[(srcSection.Left + col) * srcPixelSize.X];
                                if (!PixelColorEquals(srcPixel, transparentColor))
                                {
                                    for (int subPixelCol = 0; subPixelCol < tgtPixelSize.X; subPixelCol++)
                                    {
                                        ref Rgb24 tgtPixel = ref tgtPixelRow[(tgtPosition.X + col) * tgtPixelSize.X + subPixelCol];
                                        tgtPixel = srcPixel;
                                    }
                                }
                            }
                        }
                    }
                });
            });
        }

        /// <summary>
        /// Copies the pixels from the source RGB image to the pixels of the target RGBA image.
        /// </summary>
        /// <param name="source">The source image.</param>
        /// <param name="target">The target image.</param>
        /// <param name="transparentColor">
        /// The optional transparent color defined for the source image or RCColor.Undefined if such color is not defined.
        /// Pixels of the source image with this color will be copied to the corresponding pixel of the target image with Alpha component 0.
        /// Pixels of the source image with any other color will be copied to the corresponding pixel of the target image with Alpha component 255.
        /// </param>
        public static void MakeImageTransparent(Image<Rgb24> source, Image<Rgba32> target, RCColor transparentColor)
        {
            /// Check the parameters.
            if (source == null) { throw new ArgumentNullException("source"); }
            if (target == null) { throw new ArgumentNullException("target"); }

            RCIntRectangle srcSection = new RCIntRectangle(0, 0, source.Size().Width, source.Size().Height);
            srcSection.Intersect(new RCIntRectangle(0, 0, target.Size().Width, target.Size().Height));
            if (srcSection == RCIntRectangle.Undefined) { return; }

            source.ProcessPixelRows(srcAccessor =>
            {
                target.ProcessPixelRows(tgtAccessor =>
                {
                    for (int row = 0; row < srcSection.Height; row++)
                    {
                        Span<Rgb24> srcPixelRow = srcAccessor.GetRowSpan(row);
                        Span<Rgba32> tgtPixelRow = tgtAccessor.GetRowSpan(row);
                        for (int col = 0; col < srcSection.Width; col++)
                        {
                            // Copy the source pixel to the target pixel.
                            ref Rgb24 srcPixel = ref srcPixelRow[col];
                            ref Rgba32 tgtPixel = ref tgtPixelRow[col];
                            tgtPixel.R = srcPixel.R;
                            tgtPixel.G = srcPixel.G;
                            tgtPixel.B = srcPixel.B;

                            // Set the Alpha component of the target pixel based on its color.
                            tgtPixel.A = PixelColorEquals(srcPixel, transparentColor) ? (byte)0 : (byte)255;
                        }
                    }
                });
            });
        }

        /// <summary>
        /// Checks whether the given pixel has the given color.
        /// </summary>
        /// <param name="pixel">The given pixel.</param>
        /// <param name="color">The given color.</param>
        /// <returns>True if the given pixel has the given color; otherwise false.</returns>
        private static bool PixelColorEquals(Rgb24 pixel, RCColor color)
        {
            return color != RCColor.Undefined && pixel.R == color.R && pixel.G == color.G && pixel.B == color.B;
        }
    }
}
