using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace BmpToPng
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Arguments missing");
            }
            else
            {
                DirectoryInfo rootDir = new DirectoryInfo(args[0]);
                if (rootDir.Exists)
                {
                    ConvertImages(rootDir);
                }
                else
                {
                    Console.WriteLine(string.Format("Directory {0} missing!", rootDir.FullName));
                }
            }
            Console.ReadLine();
        }

        static void ConvertImages(DirectoryInfo dir)
        {
            FileInfo[] bmps = dir.GetFiles("*.bmp");
            foreach (FileInfo bmp in bmps)
            {
                if (bmp.Exists)
                {
                    Bitmap image = (Bitmap)Image.FromFile(bmp.FullName);
                    if (PixelFormat.Format24bppRgb == image.PixelFormat)
                    {
                        string targetFileName = bmp.FullName.Replace(".bmp", ".png");
                        image.Save(targetFileName, ImageFormat.Png);
                        image.Dispose();
                        bmp.Delete();
                        Console.WriteLine(string.Format("{0} converted successfully!", bmp.FullName));
                    }
                    else
                    {
                        Console.WriteLine(string.Format("Pixel format of {0} must be PixelFormat.Format24bppRgb!", bmp.FullName));
                    }
                }
            }

            DirectoryInfo[] subdirs = dir.GetDirectories();
            foreach (DirectoryInfo subdir in subdirs)
            {
                ConvertImages(subdir);
            }
        }
    }
}
