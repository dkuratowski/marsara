using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using RC.Common.Configuration;

namespace RC.Engine.DrawTerrain.Test
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //FileInfo tilesetFile = new FileInfo("../../../../tilesets_raw/test/test.xml");
            //TileSet tileset = XmlTileSetReader.Read(tilesetFile.FullName);
            //IsoDraw draw = new IsoDraw();
            //foreach (TileType type in tileset.TileTypes)
            //{
            //    Bitmap img = draw.CreateTileImage(type);
            //    if (type.Combination == TerrainCombination.Simple)
            //    {
            //        img.Save(string.Format("{0}.png", type.TerrainA.Name), ImageFormat.Png);
            //    }
            //    else
            //    {
            //        img.Save(string.Format("{0}_{1}_{2}.png", type.TerrainA.Name, type.TerrainB.Name, type.Combination), ImageFormat.Png);
            //    }
            //}

            ConfigurationManager.Initialize("../../../../config/RC.Engine.Test/RC.Engine.Test.root");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
