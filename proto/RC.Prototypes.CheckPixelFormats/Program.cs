using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace CheckPixelFormats
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
                foreach (string arg in args)
                {
                    DirectoryInfo rootDir = new DirectoryInfo(arg);
                    if (rootDir.Exists)
                    {
                        CheckImages(rootDir);
                    }
                    else
                    {
                        Console.WriteLine(string.Format("Directory {0} missing!", rootDir.FullName));
                    }
                }
            }
            Console.ReadLine();
        }

        static void CheckImages(DirectoryInfo dir)
        {
            FileInfo[] bmps = dir.GetFiles("*.png");
            foreach (FileInfo bmp in bmps)
            {
                if (bmp.Exists)
                {
                    Bitmap image = (Bitmap)Image.FromFile(bmp.FullName);
                    if (PixelFormat.Format24bppRgb != image.PixelFormat)
                    {
                        Console.WriteLine(string.Format("Pixel format of {0} must be PixelFormat.Format24bppRgb!", bmp.FullName));
                        Console.WriteLine("Do you want to change the pixel format of that file? (y/n)");
                        string input = Console.ReadLine();
                        if (input == "y")
                        {
                            Bitmap changedImage = new Bitmap(image.Width, image.Height, PixelFormat.Format24bppRgb);
                            using (Graphics gr = Graphics.FromImage(changedImage))
                            {
                                gr.DrawImage(image, new Rectangle(0, 0, changedImage.Width, changedImage.Height));
                            }
                            image.Dispose();
                            changedImage.Save(bmp.FullName);
                            changedImage.Dispose();
                        }
                    }
                    else
                    {
                        image.Dispose();
                    }
                }
            }

            DirectoryInfo[] subdirs = dir.GetDirectories();
            foreach (DirectoryInfo subdir in subdirs)
            {
                CheckImages(subdir);
            }
        }
    }
}
