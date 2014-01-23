using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Core;
using System.Drawing.Imaging;
using RC.Common;
using System.Diagnostics;
using RC.Engine.Maps.ComponentInterfaces;
using System.IO;
using RC.Common.Configuration;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.PathFinder.Test
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            this.fromCoords = new List<RCIntVector>();
            this.toCoords = RCIntVector.Undefined;

            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            ComponentManager.RegisterComponents("RC.Engine.Maps, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
                                                new string[2] { "RC.Engine.Maps.MapLoader", "RC.Engine.Maps.TileSetLoader" });
            ComponentManager.StartComponents();
            this.pathfinder = new Simulator.Core.PathFinder();

            //this.pathfinder.Initialize(this.ReadTestMap("..\\..\\..\\..\\tilesets\\bandlands\\bandlands.xml", "..\\..\\..\\..\\maps\\testmap4.rcm"), 5000);
            this.pathfinder.Initialize(this.ReadTestMapFromImg("pathfinder_testmap2.png"), 5000);

            this.originalMapImg = new Bitmap(this.pathfinder.PathfinderTreeRoot.AreaOnMap.Width * CELL_SIZE, this.pathfinder.PathfinderTreeRoot.AreaOnMap.Height * CELL_SIZE);
            Graphics outputGC = Graphics.FromImage(this.originalMapImg);

            for (int i = 0; i < this.pathfinder.PathfinderTreeRoot.AreaOnMap.Width / 4; i++)
            {
                outputGC.DrawLine(Pens.Cyan, i * 4 * CELL_SIZE, 0, i * 4 * CELL_SIZE, this.pathfinder.PathfinderTreeRoot.AreaOnMap.Height * CELL_SIZE);
            }
            for (int i = 0; i < this.pathfinder.PathfinderTreeRoot.AreaOnMap.Height / 4; i++)
            {
                outputGC.DrawLine(Pens.Cyan, 0, i * 4 * CELL_SIZE, this.pathfinder.PathfinderTreeRoot.AreaOnMap.Width * CELL_SIZE, i * 4 * CELL_SIZE);
            }

            HashSet<PFTreeNode> leafNodes = this.pathfinder.PathfinderTreeRoot.GetAllLeafNodes();
            foreach (PFTreeNode leafNode in leafNodes)
            {
                RCIntRectangle nodeRect = leafNode.AreaOnMap * new RCIntVector(CELL_SIZE, CELL_SIZE);
                if (!leafNode.IsWalkable)
                {
                    outputGC.FillRectangle(Brushes.Black, nodeRect.X, nodeRect.Y, nodeRect.Width, nodeRect.Height);
                }
                outputGC.DrawRectangle(leafNode.IsWalkable ? Pens.Black : Pens.Yellow, nodeRect.X, nodeRect.Y, nodeRect.Width, nodeRect.Height);
            }

            outputGC.Dispose();

            this.ClientSize = new Size(this.originalMapImg.Width, this.originalMapImg.Height);
        }

        private PFTreeNode ReadTestMapFromImg(string fileName)
        {
            /// Load the test map.
            Bitmap testMapBmp = (Bitmap)Bitmap.FromFile(fileName);
            if (testMapBmp.PixelFormat != PixelFormat.Format24bppRgb)
            {
                throw new Exception("Pixel format of the test Bitmap must be PixelFormat.Format24bppRgb");
            }

            /// Find the number of subdivision levels.
            int boundingBoxSize = Math.Max(testMapBmp.Width, testMapBmp.Height);
            int subdivisionLevels = 1;
            while (boundingBoxSize > (int)Math.Pow(2, subdivisionLevels)) { subdivisionLevels++; }

            /// Create the root of the pathfinder tree.
            PFTreeNode pfTreeRoot = new PFTreeNode(subdivisionLevels);

            /// Add obstacles to the pathfinder tree
            for (int row = 0; row < pfTreeRoot.AreaOnMap.Height; row++)
            {
                for (int column = 0; column < pfTreeRoot.AreaOnMap.Width; column++)
                {
                    if (row >= testMapBmp.Height || column >= testMapBmp.Width)
                    {
                        /// Everything out of the map range is considered to be obstacle.
                        pfTreeRoot.AddObstacle(new RCIntVector(column, row));
                    }
                    else
                    {
                        /// Add obstacle depending on the color of the pixel in the test map image.
                        if (testMapBmp.GetPixel(column, row) == Color.FromArgb(0, 0, 0))
                        {
                            pfTreeRoot.AddObstacle(new RCIntVector(column, row));
                        }
                    }
                }
            }

            testMapBmp.Dispose();
            return pfTreeRoot;
        }

        private IMapAccess ReadTestMap(string tilesetFileName, string mapFileName)
        {
            FileInfo tilesetFile = new FileInfo(tilesetFileName);
            string xmlStr = File.ReadAllText(tilesetFile.FullName);
            string imageDir = tilesetFile.DirectoryName;

            RCPackage tilesetPackage = RCPackage.CreateCustomDataPackage(PackageFormats.TILESET_FORMAT);
            tilesetPackage.WriteString(0, xmlStr);
            tilesetPackage.WriteString(1, imageDir);

            byte[] buffer = new byte[tilesetPackage.PackageLength];
            tilesetPackage.WritePackageToBuffer(buffer, 0);
            ITileSet tileset = ComponentManager.GetInterface<ITileSetLoader>().LoadTileSet(buffer);
            return ComponentManager.GetInterface<IMapLoader>().LoadMap(tileset, File.ReadAllBytes(mapFileName));
        }

        private const int CELL_SIZE = 1;

        /// <summary>
        /// The image that contains the original map
        /// </summary>
        private Bitmap originalMapImg;

        /// <summary>
        /// The image that contains the currently computed path.
        /// </summary>
        private Bitmap searchResultImg;

        /// <summary>
        /// Reference to the pathfinder component.
        /// </summary>
        private Engine.Simulator.Core.PathFinder pathfinder;

        private List<RCIntVector> fromCoords;
        private RCIntVector toCoords;

        private int currentSearchTime;
        private int currentSearchIterations;
        private int currentSearchFrames;

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(this.originalMapImg, 0, 0);
            if (this.searchResultImg != null)
            {
                e.Graphics.DrawImage(this.searchResultImg, 0, 0);
                e.Graphics.DrawString(string.Format("Time: {0}", this.currentSearchTime), SystemFonts.CaptionFont, Brushes.Blue, 0.0f, 0.0f);
                e.Graphics.DrawString(string.Format("CheckedNodes: {0}", this.currentSearchIterations), SystemFonts.CaptionFont, Brushes.Blue, 0.0f, 10.0f);
                e.Graphics.DrawString(string.Format("Frames: {0}", this.currentSearchFrames), SystemFonts.CaptionFont, Brushes.Blue, 0.0f, 20.0f);
            }
        }

        private void MainForm_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (this.toCoords != RCIntVector.Undefined)
                {
                    this.fromCoords.Clear();
                    this.toCoords = RCIntVector.Undefined;
                }
                RCIntVector mapCoords = new RCIntVector(e.X, e.Y) / CELL_SIZE;
                this.fromCoords.Add(mapCoords);
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                if (this.fromCoords.Count == 0) { return; }

                RCIntVector mapCoords = new RCIntVector(e.X, e.Y) / CELL_SIZE;
                this.toCoords = mapCoords;
                if (this.searchResultImg != null) { this.searchResultImg.Dispose(); }

                Stopwatch watch = new Stopwatch();
                List<RC.Engine.Simulator.Core.Path> paths = new List<RC.Engine.Simulator.Core.Path>();
                foreach (RCIntVector fromCoord in this.fromCoords) { paths.Add((RC.Engine.Simulator.Core.Path)this.pathfinder.StartPathSearching(fromCoord, toCoords, 5000)); }
                this.currentSearchFrames = 0;
                watch.Start();
                while (!CheckPathCompleteness(paths))
                {
                    this.pathfinder.ContinueSearching();
                    this.currentSearchFrames++;
                }
                watch.Stop();

                this.currentSearchTime = (int)watch.ElapsedMilliseconds;
                this.currentSearchIterations = 0;
                foreach (RC.Engine.Simulator.Core.Path path in paths)
                {
                    this.currentSearchIterations += path.CompletedNodes.Count();
                }

                this.searchResultImg = new Bitmap(this.pathfinder.PathfinderTreeRoot.AreaOnMap.Width * CELL_SIZE, this.pathfinder.PathfinderTreeRoot.AreaOnMap.Height * CELL_SIZE, PixelFormat.Format24bppRgb);
                Graphics outputGC = Graphics.FromImage(this.searchResultImg);
                outputGC.Clear(Color.FromArgb(255, 0, 255));
                HashSet<RCIntRectangle> sectionsOnPath = new HashSet<RCIntRectangle>();
                foreach (RC.Engine.Simulator.Core.Path path in paths)
                {
                    for (int i = 0; i < path.Length; ++i)
                    {
                        RCIntRectangle sectionRect = path[i] * new RCIntVector(CELL_SIZE, CELL_SIZE);
                        outputGC.FillRectangle(path.IsTargetFound ? Brushes.LightGreen : Brushes.Orange, sectionRect.X, sectionRect.Y, sectionRect.Width, sectionRect.Height);
                        outputGC.DrawRectangle(Pens.Black, sectionRect.X, sectionRect.Y, sectionRect.Width, sectionRect.Height);
                        sectionsOnPath.Add(sectionRect);
                    }
                    foreach (PFTreeNode completedNode in path.CompletedNodes)
                    {
                        RCIntRectangle sectionRect = completedNode.AreaOnMap * new RCIntVector(CELL_SIZE, CELL_SIZE);
                        if (!sectionsOnPath.Contains(sectionRect))
                        {
                            outputGC.FillRectangle(Brushes.Red, sectionRect.X, sectionRect.Y, sectionRect.Width, sectionRect.Height);
                            outputGC.DrawRectangle(Pens.Black, sectionRect.X, sectionRect.Y, sectionRect.Width, sectionRect.Height);
                        }
                    }
                    for (int i = 1; i < path.Length; ++i)
                    {
                        RCIntRectangle prevSectionRect = path[i - 1] * new RCIntVector(CELL_SIZE, CELL_SIZE);
                        RCIntRectangle currSectionRect = path[i] * new RCIntVector(CELL_SIZE, CELL_SIZE);
                        outputGC.DrawLine(Pens.Blue, (prevSectionRect.Left + prevSectionRect.Right) / 2, (prevSectionRect.Top + prevSectionRect.Bottom) / 2, (currSectionRect.Left + currSectionRect.Right) / 2, (currSectionRect.Top + currSectionRect.Bottom) / 2);
                    }
                }
                outputGC.Dispose();
                this.searchResultImg.MakeTransparent(Color.FromArgb(255, 0, 255));

                this.Invalidate();
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Middle)
            {
                RCIntVector mapCoords = new RCIntVector(e.X, e.Y) / CELL_SIZE;
                if (this.searchResultImg != null) { this.searchResultImg.Dispose(); }

                this.currentSearchFrames = 1;
                this.currentSearchIterations = 0;
                Stopwatch watch = new Stopwatch();
                watch.Start();
                RC.Engine.Simulator.Core.Region region = new Simulator.Core.Region(this.pathfinder.PathfinderTreeRoot.GetLeafNode(mapCoords), 40);
                watch.Stop();
                this.currentSearchTime = (int)watch.ElapsedMilliseconds;

                this.searchResultImg = new Bitmap(this.pathfinder.PathfinderTreeRoot.AreaOnMap.Width * CELL_SIZE, this.pathfinder.PathfinderTreeRoot.AreaOnMap.Height * CELL_SIZE, PixelFormat.Format24bppRgb);
                Graphics outputGC = Graphics.FromImage(this.searchResultImg);
                outputGC.Clear(Color.FromArgb(255, 0, 255));
                foreach (PFTreeNode node in region.ContainedNodes)
                {
                    RCIntRectangle sectionRect = node.AreaOnMap * new RCIntVector(CELL_SIZE, CELL_SIZE);
                    outputGC.FillRectangle(Brushes.Cyan, sectionRect.X, sectionRect.Y, sectionRect.Width, sectionRect.Height);
                    outputGC.DrawRectangle(Pens.Black, sectionRect.X, sectionRect.Y, sectionRect.Width, sectionRect.Height);
                }
                outputGC.Dispose();
                this.searchResultImg.MakeTransparent(Color.FromArgb(255, 0, 255));

                this.Invalidate();
            }
        }

        private bool CheckPathCompleteness(List<RC.Engine.Simulator.Core.Path> pathsToCheck)
        {
            foreach (RC.Engine.Simulator.Core.Path path in pathsToCheck) { if (!path.IsReadyForUse) { return false; } }
            return true;
        }
    }
}
