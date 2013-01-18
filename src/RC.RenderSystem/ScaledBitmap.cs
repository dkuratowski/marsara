using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace RC.RenderSystem
{
    /// <summary>
    /// Represents a bitmap that is scaled by the amounts defined in Display.HorizontalScale and
    /// Display.VerticalScale. Only this kind of bimaps can be drawn to the Display.
    /// </summary>
    /// <remarks>
    /// If the singleton instance of Display doesn't exist when you use this class, then the behaviour equals
    /// if the scaling factors were 1.
    /// </remarks>
    public class ScaledBitmap : IDisposable
    {
        /// <summary>
        /// Creates a ScaledBitmap from the given original Bitmap.
        /// </summary>
        /// <param name="originalBmp">The original Bitmap.</param>
        /// <returns>The created ScaledBitmap.</returns>
        /// <remarks>Disposing originalBmp is the responsibility of the caller.</remarks>
        public static ScaledBitmap FromBitmap(Bitmap originalBmp)
        {
            if (null == originalBmp) { throw new ArgumentNullException("originalBmp"); }
            if (PixelFormat.Format24bppRgb != originalBmp.PixelFormat)
            {
                throw new ArgumentException("Pixel format of the given Bitmap must be PixelFormat.Format24bppRgb",
                                            "originalBmp");
                /// TODO: implement other pixel format support (?)
            }
            ScaledBitmap retBmp = new ScaledBitmap(originalBmp.Width, originalBmp.Height);
            CopyBitmapScaled(originalBmp, retBmp.rawBitmap);
            return retBmp;
        }

        /// <summary>
        /// Creates a ScaledBitmap from the given original bitmap and replace it's transparent pixels with the
        /// given new color.
        /// </summary>
        /// <param name="originalBmp">The original bitmap.</param>
        /// <param name="transparentColor">The selected transparent color on the original bitmap.</param>
        /// <param name="replaceTo">
        /// The transparent pixels on the original bitmap will be replaced with this color.
        /// </param>
        /// <returns>The created ScaledBitmap.</returns>
        /// <remarks>Disposing originalBmp is the responsibility of the caller.</remarks>
        public static ScaledBitmap FromBitmap(Bitmap originalBmp, Color transparentColor, Color replaceTo)
        {
            if (null == originalBmp) { throw new ArgumentNullException("originalBmp"); }
            if (PixelFormat.Format24bppRgb != originalBmp.PixelFormat)
            {
                throw new ArgumentException("Pixel format of the given Bitmap must be PixelFormat.Format24bppRgb",
                                            "originalBmp");
                /// TODO: implement other pixel format support (?)
            }

            ScaledBitmap original = ScaledBitmap.FromBitmap(originalBmp);
            ScaledBitmap retBmp = new ScaledBitmap(original.Width, original.Height);
            BitmapAccess retBmpAccess = BitmapAccess.FromBitmap(retBmp);
            retBmpAccess.Clear(replaceTo);
            original.MakeTransparent(transparentColor);
            retBmpAccess.DrawBitmap(original, 0, 0);
            retBmpAccess.Dispose();
            original.Dispose();

            return retBmp;
        }

        /// <summary>
        /// Creates a ScaledBitmap from the given original bitmap and replace it's transparent pixels with the
        /// pixels of another bitmap.
        /// </summary>
        /// <param name="originalBmp">The original bitmap.</param>
        /// <param name="transparentColor">The selected transparent color on the original bitmap.</param>
        /// <param name="replaceTo">
        /// The transparent pixels on the original bitmap will be replaced with the pixels of this bitmap.
        /// </param>
        /// <returns>The created ScaledBitmap.</returns>
        /// <remarks>Disposing originalBmp and replaceTo is the responsibility of the caller.</remarks>
        public static ScaledBitmap FromBitmap(Bitmap originalBmp, Color transparentColor, Bitmap replaceTo)
        {
            if (null == originalBmp) { throw new ArgumentNullException("originalBmp"); }
            if (null == replaceTo) { throw new ArgumentNullException("replaceTo"); }
            if (PixelFormat.Format24bppRgb != originalBmp.PixelFormat || PixelFormat.Format24bppRgb != replaceTo.PixelFormat)
            {
                throw new ArgumentException("Pixel format of the given Bitmaps must be PixelFormat.Format24bppRgb");
                /// TODO: implement other pixel format support (?)
            }

            ScaledBitmap original = ScaledBitmap.FromBitmap(originalBmp);
            ScaledBitmap replaceToBmp = ScaledBitmap.FromBitmap(replaceTo);

            ScaledBitmap retBmp = new ScaledBitmap(original.Width, original.Height);
            BitmapAccess retBmpAccess = BitmapAccess.FromBitmap(retBmp);
            for (int currX = 0; currX < retBmp.Width; currX = currX + replaceTo.Width)
            {
                for (int currY = 0; currY < retBmp.Height; currY = currY + replaceTo.Height)
                {
                    retBmpAccess.DrawBitmap(replaceToBmp, currX, currY);
                }
            }
            original.MakeTransparent(transparentColor);
            retBmpAccess.DrawBitmap(original, 0, 0);
            retBmpAccess.Dispose();
            original.Dispose();
            replaceToBmp.Dispose();

            return retBmp;
        }

        /// <summary>
        /// Copies the source bitmap to the target bitmap and scales it with the actual scaling factors.
        /// </summary>
        /// <param name="source">The source bitmap with the original size.</param>
        /// <param name="target">The target bitmap with the scaled size.</param>
        private static void CopyBitmapScaled(Bitmap source, Bitmap target)
        {
            if (PixelFormat.Format24bppRgb != source.PixelFormat ||
                PixelFormat.Format24bppRgb != target.PixelFormat)
            {
                throw new ArgumentException("Pixel format of the given Bitmaps must be PixelFormat.Format24bppRgb");
                /// TODO: implement other pixel format support (?)
            }

            BitmapData srcRawData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height),
                                                    ImageLockMode.ReadOnly,
                                                    PixelFormat.Format24bppRgb);
            BitmapData tgtRawData = target.LockBits(new Rectangle(0, 0, target.Width, target.Height),
                                                    ImageLockMode.ReadOnly,
                                                    PixelFormat.Format24bppRgb);

            for (int row = 0; row < source.Height; row++)
            {
                for (int col = 0; col < source.Width; col++)
                {
                    CopyPixelScaled(srcRawData, tgtRawData, col, row);
                }
            }

            source.UnlockBits(srcRawData);
            target.UnlockBits(tgtRawData);
        }

        /// <summary>
        /// Copies a pixel scaled by the scaling factors from the source bitmap to the target.
        /// </summary>
        /// <param name="source">The raw data of the source bitmap.</param>
        /// <param name="target">The raw data of the target bitmap.</param>
        /// <param name="x">The X coordinate of the pixel you want to copy and scale.</param>
        /// <param name="y">The Y coordinate of the pixel you want to copy and scale.</param>
        private static void CopyPixelScaled(BitmapData source, BitmapData target, int x, int y)
        {
            if (x < 0 || x >= source.Width || y < 0 || y >= source.Height)
            {
                throw new ArgumentException("Unexpected coordinates: X = " + x + ", Y = " + y);
            }

            byte b = Marshal.ReadByte(source.Scan0, (source.Stride * y) + (x * 3) + 0); // blue component
            byte g = Marshal.ReadByte(source.Scan0, (source.Stride * y) + (x * 3) + 1); // green component
            byte r = Marshal.ReadByte(source.Scan0, (source.Stride * y) + (x * 3) + 2); // red component

            for (int subPixelRow = 0; subPixelRow < Display.VerticalScale; subPixelRow++)
            {
                for (int subPixelCol = 0; subPixelCol < Display.HorizontalScale; subPixelCol++)
                {
                    int targetPixelX = x * Display.HorizontalScale + subPixelCol;
                    int targetPixelY = y * Display.VerticalScale + subPixelRow;

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

        /// <summary>
        /// Creates a new ScaledBitmap object with the given sizes
        /// </summary>
        /// <param name="width">The width of the ScaledBitmap in logical pixels.</param>
        /// <param name="height">The height of the ScaledBitmap in logical pixels.</param>
        public ScaledBitmap(int width, int height)
        {
            this.rawBitmap = new Bitmap(width * Display.HorizontalScale,
                                        height * Display.VerticalScale,
                                        PixelFormat.Format24bppRgb);
            this.width = width;
            this.height = height;
        }

        /// <summary>
        /// Saves this ScaledBitmap into the given file.
        /// </summary>
        /// <param name="fileName">The name of the file you want to save to.</param>
        public void Save(string fileName)
        {
            this.rawBitmap.Save(fileName);
        }

        /// <summary>
        /// Makes the specified color transparent for this ScaledBitmap.
        /// </summary>
        /// <param name="transparentColor">The color you want to be transparent for this ScaledBitmap.</param>
        public void MakeTransparent(Color transparentColor)
        {
            this.rawBitmap.MakeTransparent(transparentColor);
        }

        #region IDisposable members

        public void Dispose()
        {
            this.rawBitmap.Dispose();
        }

        #endregion

        /// <summary>
        /// Gets the width of the scaled bitmap in logical pixels.
        /// </summary>
        public int Width { get { return this.width; } }

        /// <summary>
        /// Gets the height of the scaled bitmap in logical pixels.
        /// </summary>
        public int Height { get { return this.height; } }

        /// <summary>
        /// Gets the underlying raw Bitmap object.
        /// </summary>
        public Bitmap RawBitmap { get { return this.rawBitmap; } }

        /// <summary>
        /// Contains the scaled bitmap.
        /// </summary>
        private Bitmap rawBitmap;

        /// <summary>
        /// The width of the scaled bitmap in logical pixels.
        /// </summary>
        private int width;

        /// <summary>
        /// The height of the scaled bitmap in logical pixels.
        /// </summary>
        private int height;
    }
}
