using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using RC.Common;
using System.Drawing;

namespace RC.UI.MonoGamePlugin
{
    /// <summary>
    /// Static class for System.Drawing.Bitmap operations.
    /// </summary>
    static class MonoGameBitmapUtils
    {
        /// <summary>
        /// Copies the pixels of the source bitmap to the pixels of the target bitmap.
        /// </summary>
        /// <param name="source">The source bitmap.</param>
        /// <param name="target">The target bitmap.</param>
        /// <param name="srcPixelSize">The pixel size of the source bitmap.</param>
        /// <param name="tgtPixelSize">The pixel size of the target bitmap.</param>
        public static void CopyBitmapScaled(Bitmap source, Bitmap target,
                                            RCIntVector srcPixelSize, RCIntVector tgtPixelSize)
        {
            CopyBitmapScaled(source, target, srcPixelSize, tgtPixelSize,
                new RCIntRectangle(0, 0, source.Width / srcPixelSize.X, source.Height / srcPixelSize.Y),
                new RCIntVector(0, 0));
        }

        /// <summary>
        /// Copies a section of scaled pixels from the source bitmap to the scaled bitmaps of the target bitmap
        /// to a given position.
        /// </summary>
        /// <param name="source">The source bitmap.</param>
        /// <param name="target">The target bitmap.</param>
        /// <param name="srcPixelSize">The pixel size of the source bitmap.</param>
        /// <param name="tgtPixelSize">The pixel size of the target bitmap.</param>
        /// <param name="srcSection">The section of the source bitmap to copy.</param>
        /// <param name="tgtPosition">The position in the target bitmap to copy to.</param>
        public static void CopyBitmapScaled(Bitmap source, Bitmap target,
                                            RCIntVector srcPixelSize, RCIntVector tgtPixelSize,
                                            RCIntRectangle srcSection, RCIntVector tgtPosition)
        {
            /// Check the parameters.
            if (source == null) { throw new ArgumentNullException("source"); }
            if (target == null) { throw new ArgumentNullException("target"); }
            if (srcPixelSize == RCIntVector.Undefined) { throw new ArgumentNullException("srcPixelSize"); }
            if (tgtPixelSize == RCIntVector.Undefined) { throw new ArgumentNullException("tgtPixelSize"); }
            if (srcSection == RCIntRectangle.Undefined) { throw new ArgumentNullException("srcSection"); }
            if (tgtPosition == RCIntVector.Undefined) { throw new ArgumentNullException("tgtPosition"); }

            if (PixelFormat.Format24bppRgb != source.PixelFormat ||
                PixelFormat.Format24bppRgb != target.PixelFormat)
            {
                throw new ArgumentException("Pixel format of the given Bitmaps must be PixelFormat.Format24bppRgb");
            }

            /// Compute the source and the target sections.
            srcSection.Intersect(new RCIntRectangle(0, 0, source.Width / srcPixelSize.X, source.Height / srcPixelSize.Y));
            if (srcSection == RCIntRectangle.Undefined) { return; }
            RCIntRectangle tgtSection = new RCIntRectangle(tgtPosition.X, tgtPosition.Y, srcSection.Width, srcSection.Height);
            tgtSection.Intersect(new RCIntRectangle(0, 0, target.Width / tgtPixelSize.X, target.Height / tgtPixelSize.Y));
            if (tgtSection == RCIntRectangle.Undefined) { return; }

            /// Lock the bytes of the bitmaps for copying.
            BitmapData srcRawData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height),
                                                    ImageLockMode.ReadOnly,
                                                    PixelFormat.Format24bppRgb);
            BitmapData tgtRawData = target.LockBits(new Rectangle(0, 0, target.Width, target.Height),
                                                    ImageLockMode.ReadOnly,
                                                    PixelFormat.Format24bppRgb);

            /// Copy the pixels.
            for (int row = 0; row < srcSection.Height; row++)
            {
                if (row < tgtSection.Height)
                {
                    for (int col = 0; col < srcSection.Width; col++)
                    {
                        if (col < tgtSection.Width)
                        {
                            CopyPixelScaled(srcRawData, tgtRawData,
                                            new RCIntVector(srcSection.X + col, srcSection.Y + row),
                                            new RCIntVector(tgtSection.X + col, tgtSection.Y + row),
                                            srcPixelSize, tgtPixelSize);
                        }
                    }
                }
            }

            source.UnlockBits(srcRawData);
            target.UnlockBits(tgtRawData);
        }

        /// <summary>
        /// Copies the given scaled pixel of the source bitmap to the given scaled pixel of the target bitmap.
        /// </summary>
        /// <param name="source">The source bitmap data.</param>
        /// <param name="target">The target bitmap data.</param>
        /// <param name="srcRowCol">The coordinates of the scaled pixel in the source bitmap to copy.</param>
        /// <param name="tgtRowCol">The coordinates of the scaled pixel in the target bitmap to copy to.</param>
        /// <param name="srcPixelSize">The pixel size of the source bitmap.</param>
        /// <param name="tgtPixelSize">The pixel size of the target bitmap.</param>
        private static void CopyPixelScaled(BitmapData source, BitmapData target,
                                            RCIntVector srcRowCol, RCIntVector tgtRowCol,
                                            RCIntVector srcPixelSize, RCIntVector tgtPixelSize)
        {
            byte b = Marshal.ReadByte(source.Scan0, (source.Stride * srcRowCol.Y * srcPixelSize.Y) +
                                                    (srcRowCol.X * 3 * srcPixelSize.X) + 0);
            byte g = Marshal.ReadByte(source.Scan0, (source.Stride * srcRowCol.Y * srcPixelSize.Y) +
                                                    (srcRowCol.X * 3 * srcPixelSize.X) + 1);
            byte r = Marshal.ReadByte(source.Scan0, (source.Stride * srcRowCol.Y * srcPixelSize.Y) +
                                                    (srcRowCol.X * 3 * srcPixelSize.X) + 2);

            for (int subPixelRow = 0; subPixelRow < tgtPixelSize.Y; subPixelRow++)
            {
                for (int subPixelCol = 0; subPixelCol < tgtPixelSize.X; subPixelCol++)
                {
                    int targetPixelX = tgtRowCol.X * tgtPixelSize.X + subPixelCol;
                    int targetPixelY = tgtRowCol.Y * tgtPixelSize.Y + subPixelRow;

                    int bOffset = (target.Stride * targetPixelY) + (targetPixelX * 3) + 0; // blue component
                    int gOffset = (target.Stride * targetPixelY) + (targetPixelX * 3) + 1; // green component
                    int rOffset = (target.Stride * targetPixelY) + (targetPixelX * 3) + 2; // red component

                    if (bOffset < target.Stride * target.Height &&
                        gOffset < target.Stride * target.Height &&
                        rOffset < target.Stride * target.Height)
                    {
                        Marshal.WriteByte(target.Scan0, bOffset, b);
                        Marshal.WriteByte(target.Scan0, gOffset, g);
                        Marshal.WriteByte(target.Scan0, rOffset, r);
                    }
                }
            }
        }
    }
}
