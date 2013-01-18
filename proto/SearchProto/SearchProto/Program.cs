using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace SearchProto
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1) { throw new Exception("Map file argument missing!"); }

            bool[][] mapInfoArray = null;
            Bitmap sourceBmp = (Bitmap)Image.FromFile(args[0]);
            mapInfoArray = CreateMapInfoArray(sourceBmp);

            navMesh = new NavMesh(mapInfoArray);

            Dictionary<int, Color> colors = new Dictionary<int, Color>();
            Random rnd = new Random();
            for (int row = 0; row < navMesh.Height; row++)
            {
                for (int column = 0; column < navMesh.Width; column++)
                {
                    NavMeshNode node = navMesh[column, row];
                    if (node != null)
                    {
                        if (!colors.ContainsKey(node.Region.ID))
                        {
                            colors.Add(node.Region.ID, Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256)));
                        }
                        sourceBmp.SetPixel(column, row, colors[node.Region.ID]);
                    }
                }
            }

            //Stopwatch watch = new Stopwatch();
            //watch.Start();
            //Tuple<List<Tuple<NavMeshNode, int>>, List<NavMeshNode>> route = navMesh.FindRoute(navMesh[30, 30], navMesh[900, 900]);
            //watch.Stop();

            //Color touchColor = Color.FromArgb(255, 0, 0);
            //foreach (NavMeshNode item in route.Item2)
            //{
            //    sourceBmp.SetPixel(item.X, item.Y, touchColor);
            //}

            //Color pathColor = Color.FromArgb(0, 255, 0);
            //foreach (Tuple<NavMeshNode, int> item in route.Item1)
            //{
            //    sourceBmp.SetPixel(item.Item1.X, item.Item1.Y, pathColor);
            //}

            sourceBmp.Save("result.png", ImageFormat.Png);
            sourceBmp.Dispose();
            //Console.WriteLine(string.Format("Pathfinding duration: {0} ms", watch.ElapsedMilliseconds));
            Console.ReadLine();
        }

        private static bool[][] CreateMapInfoArray(Bitmap sourceBitmap)
        {
            if (PixelFormat.Format24bppRgb != sourceBitmap.PixelFormat)
            {
                throw new ArgumentException("Pixel format of the given Bitmap must be PixelFormat.Format24bppRgb");
            }

            BitmapData srcRawData = sourceBitmap.LockBits(new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height),
                                                          ImageLockMode.ReadOnly,
                                                          PixelFormat.Format24bppRgb);

            bool[][] retArray = new bool[sourceBitmap.Height][];
            for (int row = 0; row < sourceBitmap.Height; row++)
            {
                retArray[row] = new bool[sourceBitmap.Width];
                for (int col = 0; col < sourceBitmap.Width; col++)
                {
                    byte b = Marshal.ReadByte(srcRawData.Scan0, (srcRawData.Stride * row) + (col * 3) + 0); // blue component
                    retArray[row][col] = (b != 0);
                }
            }

            sourceBitmap.UnlockBits(srcRawData);
            return retArray;
        }

        private static NavMesh navMesh;
    }
}
