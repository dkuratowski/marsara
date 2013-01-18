using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace MetricTest
{
    class Program
    {
        static void Main(string[] args)
        {
            int range = 5;

            int width = range * 2 + 20;
            int height = width;

            int pointX = width / 2;
            int pointY = height / 2;

            Bitmap target = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    target.SetPixel(x, y, Distance(pointX, pointY, x, y) <= range ? Color.Black : Color.White);
                }
            }
            target.Save("result.png", ImageFormat.Png);
            target.Dispose();
        }

        static float Distance(int x0, int y0, int x1, int y1)
        {
            int horz = Math.Abs(x0 - x1);
            int vert = Math.Abs(y0 - y1);
            int diff = Math.Abs(horz - vert);
            return Math.Min(horz, vert) * ROOT_OF_TWO + diff;
        }

        private const float ROOT_OF_TWO = 1.4142f;
    }
}
