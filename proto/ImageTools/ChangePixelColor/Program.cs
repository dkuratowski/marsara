using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace ChangePixelColor
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 8)
            {
                Console.WriteLine("Arguments missing");
            }
            else
            {
                Bitmap source = (Bitmap)Image.FromFile(args[0]);
                //if (source.PixelFormat != PixelFormat.Format24bppRgb)
                //{
                //    throw new ArgumentException("Pixel format of the given Bitmap must be PixelFormat.Format24bppRgb");
                //}

                Color oldColor = Color.FromArgb(int.Parse(args[2]), int.Parse(args[3]), int.Parse(args[4]));
                Color newColor = Color.FromArgb(int.Parse(args[5]), int.Parse(args[6]), int.Parse(args[7]));

                Bitmap target = new Bitmap(source.Width, source.Height, PixelFormat.Format24bppRgb);

                for (int y = 0; y < source.Height; y++)
                {
                    for (int x = 0; x < source.Width; x++)
                    {
                        Color pixel = source.GetPixel(x, y);
                        if (pixel == oldColor)
                        {
                            target.SetPixel(x, y, newColor);
                        }
                        else
                        {
                            target.SetPixel(x, y, pixel);
                        }
                    }
                }
                target.Save(args[1], ImageFormat.Png);
                source.Dispose();
                target.Dispose();
            }
        }
    }
}
