using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageSqueeze
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 6)
            {
                Console.WriteLine("Arguments missing");
            }
            else
            {
                string sourceFile = args[0];
                string targetFile = args[1];
                int topLeftX = int.Parse(args[2]);
                int topLeftY = int.Parse(args[3]);
                int horzScale = int.Parse(args[4]);
                int vertScale = int.Parse(args[5]);
                Bitmap sourceBmp = (Bitmap)Image.FromFile(sourceFile);
                Bitmap targetBmp = CreateBitmapSqueezed(sourceBmp, topLeftX, topLeftY, horzScale, vertScale);
                targetBmp.Save(targetFile, ImageFormat.Png);
            }
        }

        private static Bitmap CreateBitmapSqueezed(Bitmap source, int topLeftX, int topLeftY, int horzScale, int vertScale)
        {
            if (PixelFormat.Format24bppRgb != source.PixelFormat)
            {
                throw new ArgumentException("Pixel format of the given Bitmap must be PixelFormat.Format24bppRgb");
            }

            Bitmap target = new Bitmap((source.Width - topLeftX) / horzScale, (source.Height - topLeftY) / vertScale, PixelFormat.Format24bppRgb);

            BitmapData srcRawData = source.LockBits(new Rectangle(topLeftX, topLeftY, source.Width - topLeftX, source.Height - topLeftY),
                                                    ImageLockMode.ReadOnly,
                                                    PixelFormat.Format24bppRgb);
            BitmapData tgtRawData = target.LockBits(new Rectangle(0, 0, target.Width, target.Height),
                                                    ImageLockMode.ReadWrite,
                                                    PixelFormat.Format24bppRgb);

            for (int row = 0; row < target.Height; row++)
            {
                for (int col = 0; col < target.Width; col++)
                {
                    CopyPixel(srcRawData, tgtRawData, col, row, topLeftX, topLeftY, horzScale, vertScale);
                }
            }

            source.UnlockBits(srcRawData);
            target.UnlockBits(tgtRawData);

            return target;
        }

        private static void CopyPixel(BitmapData source, BitmapData target, int x, int y, int topLeftX, int topLeftY, int horzScale, int vertScale)
        {
            if (x < 0 || x >= target.Width || y < 0 || y >= target.Height)
            {
                throw new ArgumentException("Unexpected coordinates: X = " + x + ", Y = " + y);
            }

            byte b = Marshal.ReadByte(source.Scan0, (source.Stride * (topLeftY + y * vertScale)) + ((topLeftX + x * horzScale) * 3) + 0); // blue component
            byte g = Marshal.ReadByte(source.Scan0, (source.Stride * (topLeftY + y * vertScale)) + ((topLeftX + x * horzScale) * 3) + 1); // green component
            byte r = Marshal.ReadByte(source.Scan0, (source.Stride * (topLeftY + y * vertScale)) + ((topLeftX + x * horzScale) * 3) + 2); // red component

            int bOffset = (target.Stride * y) + (x * 3) + 0; // blue component
            int gOffset = (target.Stride * y) + (x * 3) + 1; // green component
            int rOffset = (target.Stride * y) + (x * 3) + 2; // red component

            Marshal.WriteByte(target.Scan0, bOffset, b);
            Marshal.WriteByte(target.Scan0, gOffset, g);
            Marshal.WriteByte(target.Scan0, rOffset, r);
        }
    }
}
