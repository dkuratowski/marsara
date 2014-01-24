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
            this.toCoord = RCIntVector.Undefined;
            this.computedPaths = new List<Simulator.Core.Path>();
            this.blockedNodeIndex = 0;
            this.pathfinder = null;

            this.originalMapImg = null;
            this.searchResultImg = null;
            this.detourSearchResultImg = null;
            this.blockedNodeSelectionImg = null;
            this.lastSearchTime = 0;
            this.lastSearchIterations = 0;
            this.lastSearchFrames = 0;

            InitializeComponent();
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseWheel);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            ComponentManager.RegisterComponents("RC.Engine.Maps, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
                                                new string[2] { "RC.Engine.Maps.MapLoader", "RC.Engine.Maps.TileSetLoader" });
            ComponentManager.StartComponents();
            this.pathfinder = new Simulator.Core.PathFinder();

            this.pathfinder.Initialize(this.ReadTestMap("..\\..\\..\\..\\tilesets\\bandlands\\bandlands.xml", "..\\..\\..\\..\\maps\\testmap4.rcm"), 5000);
            //this.pathfinder.Initialize(this.ReadTestMapFromImg("pathfinder_testmap2.png"), 5000);

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

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(this.originalMapImg, 0, 0);
            if (this.searchResultImg != null) { e.Graphics.DrawImage(this.searchResultImg, 0, 0); }
            if (this.detourSearchResultImg != null) { e.Graphics.DrawImage(this.detourSearchResultImg, 0, 0); }
            if (this.blockedNodeSelectionImg != null) { e.Graphics.DrawImage(this.blockedNodeSelectionImg, 0, 0); }
            e.Graphics.DrawString(string.Format("Time: {0}", this.lastSearchTime), SystemFonts.CaptionFont, Brushes.Blue, 0.0f, 0.0f);
            e.Graphics.DrawString(string.Format("Iterations: {0}", this.lastSearchIterations), SystemFonts.CaptionFont, Brushes.Blue, 0.0f, 10.0f);
            e.Graphics.DrawString(string.Format("Frames: {0}", this.lastSearchFrames), SystemFonts.CaptionFont, Brushes.Blue, 0.0f, 20.0f);
        }

        private void MainForm_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                /// Left-click event handling.
                if (this.toCoord != RCIntVector.Undefined)
                {
                    this.fromCoords.Clear();
                    this.toCoord = RCIntVector.Undefined;
                }
                this.fromCoords.Add(new RCIntVector(e.X, e.Y) / CELL_SIZE);
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                /// Right-click event handling.
                if (this.fromCoords.Count == 0) { return; }

                RCIntVector mapCoords = new RCIntVector(e.X, e.Y) / CELL_SIZE;
                this.toCoord = mapCoords;
                if (this.searchResultImg != null) { this.searchResultImg.Dispose(); this.searchResultImg = null; }
                if (this.detourSearchResultImg != null) { this.detourSearchResultImg.Dispose(); this.detourSearchResultImg = null; }
                if (this.blockedNodeSelectionImg != null) { this.blockedNodeSelectionImg.Dispose(); this.blockedNodeSelectionImg = null; }

                Stopwatch watch = new Stopwatch();
                this.computedPaths = new List<RC.Engine.Simulator.Core.Path>();
                foreach (RCIntVector fromCoord in this.fromCoords) { this.computedPaths.Add((RC.Engine.Simulator.Core.Path)this.pathfinder.StartPathSearching(fromCoord, toCoord, 5000)); }
                this.lastSearchFrames = 0;
                watch.Start();
                while (!CheckPathCompleteness(this.computedPaths))
                {
                    this.pathfinder.ContinueSearching();
                    this.lastSearchFrames++;
                }
                watch.Stop();

                this.lastSearchTime = (int)watch.ElapsedMilliseconds;
                this.lastSearchIterations = 0;
                foreach (RC.Engine.Simulator.Core.Path path in this.computedPaths)
                {
                    this.lastSearchIterations += path.CompletedNodes.Count();
                }

                this.searchResultImg = new Bitmap(this.pathfinder.PathfinderTreeRoot.AreaOnMap.Width * CELL_SIZE, this.pathfinder.PathfinderTreeRoot.AreaOnMap.Height * CELL_SIZE, PixelFormat.Format24bppRgb);
                Graphics outputGC = Graphics.FromImage(this.searchResultImg);
                outputGC.Clear(Color.FromArgb(255, 0, 255));
                HashSet<RCIntRectangle> sectionsOnPath = new HashSet<RCIntRectangle>();
                foreach (RC.Engine.Simulator.Core.Path path in this.computedPaths)
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

                if (this.computedPaths.Count == 1) { this.blockedNodeIndex = 0; }
                this.Invalidate();
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Middle)
            {
                if (this.computedPaths.Count != 1) { return; }
                if (this.blockedNodeIndex == this.computedPaths[0].Length - 1) { return; }

                if (this.detourSearchResultImg != null) { this.detourSearchResultImg.Dispose(); this.detourSearchResultImg = null; }
                if (this.blockedNodeSelectionImg != null) { this.blockedNodeSelectionImg.Dispose(); this.blockedNodeSelectionImg = null; }

                Stopwatch watch = new Stopwatch();
                RC.Engine.Simulator.Core.Path originalPath = this.computedPaths[0];
                this.computedPaths = new List<RC.Engine.Simulator.Core.Path>();
                this.computedPaths.Add((RC.Engine.Simulator.Core.Path)this.pathfinder.StartDetourSearching(originalPath, this.blockedNodeIndex, 5000));
                this.lastSearchFrames = 0;
                watch.Start();
                while (!CheckPathCompleteness(this.computedPaths))
                {
                    this.pathfinder.ContinueSearching();
                    this.lastSearchFrames++;
                }
                watch.Stop();

                this.lastSearchTime = (int)watch.ElapsedMilliseconds;
                this.lastSearchIterations = 0;
                foreach (RC.Engine.Simulator.Core.Path path in this.computedPaths)
                {
                    this.lastSearchIterations += path.CompletedNodes.Count();
                }

                this.detourSearchResultImg = new Bitmap(this.pathfinder.PathfinderTreeRoot.AreaOnMap.Width * CELL_SIZE, this.pathfinder.PathfinderTreeRoot.AreaOnMap.Height * CELL_SIZE, PixelFormat.Format24bppRgb);
                Graphics outputGC = Graphics.FromImage(this.detourSearchResultImg);
                outputGC.Clear(Color.FromArgb(255, 0, 255));

                /// Draw the blocked edges.
                HashSet<Tuple<int, int>> blockedEdgeIDs = new HashSet<Tuple<int, int>>();
                this.computedPaths[0].CopyBlockedEdges(ref blockedEdgeIDs);
                foreach (Tuple<int, int> blockedEdgeID in blockedEdgeIDs)
                {
                    RCIntRectangle edgeBeginSectionRect = this.pathfinder.GetLeafNodeByID(blockedEdgeID.Item1).AreaOnMap * new RCIntVector(CELL_SIZE, CELL_SIZE);
                    RCIntRectangle edgeEndSectionRect = this.pathfinder.GetLeafNodeByID(blockedEdgeID.Item2).AreaOnMap * new RCIntVector(CELL_SIZE, CELL_SIZE);
                    RCIntVector edgeBegin = new RCIntVector((edgeBeginSectionRect.Left + edgeBeginSectionRect.Right) / 2, (edgeBeginSectionRect.Top + edgeBeginSectionRect.Bottom) / 2);
                    RCIntVector edgeEnd = new RCIntVector((edgeEndSectionRect.Left + edgeEndSectionRect.Right) / 2, (edgeEndSectionRect.Top + edgeEndSectionRect.Bottom) / 2);
                    outputGC.DrawLine(Pens.Red, edgeBegin.X, edgeBegin.Y, edgeEnd.X, edgeEnd.Y);
                }

                /// Draw the detour.
                for (int i = 1; i < this.computedPaths[0].Length; ++i)
                {
                    RCIntRectangle prevSectionRect = this.computedPaths[0][i - 1] * new RCIntVector(CELL_SIZE, CELL_SIZE);
                    RCIntRectangle currSectionRect = this.computedPaths[0][i] * new RCIntVector(CELL_SIZE, CELL_SIZE);
                    outputGC.DrawLine(Pens.Green, (prevSectionRect.Left + prevSectionRect.Right) / 2, (prevSectionRect.Top + prevSectionRect.Bottom) / 2, (currSectionRect.Left + currSectionRect.Right) / 2, (currSectionRect.Top + currSectionRect.Bottom) / 2);
                }

                outputGC.Dispose();
                this.detourSearchResultImg.MakeTransparent(Color.FromArgb(255, 0, 255));

                if (this.computedPaths.Count == 1) { this.blockedNodeIndex = 0; }
                this.Invalidate();
            }
            //else if (e.Button == System.Windows.Forms.MouseButtons.Middle)
            //{
            //    RCIntVector mapCoords = new RCIntVector(e.X, e.Y) / CELL_SIZE;
            //    if (this.searchResultImg != null) { this.searchResultImg.Dispose(); }

            //    this.lastSearchFrames = 1;
            //    this.lastSearchIterations = 0;
            //    Stopwatch watch = new Stopwatch();
            //    watch.Start();
            //    RC.Engine.Simulator.Core.Region region = new Simulator.Core.Region(this.pathfinder.PathfinderTreeRoot.GetLeafNode(mapCoords), 40);
            //    watch.Stop();
            //    this.lastSearchTime = (int)watch.ElapsedMilliseconds;

            //    this.searchResultImg = new Bitmap(this.pathfinder.PathfinderTreeRoot.AreaOnMap.Width * CELL_SIZE, this.pathfinder.PathfinderTreeRoot.AreaOnMap.Height * CELL_SIZE, PixelFormat.Format24bppRgb);
            //    Graphics outputGC = Graphics.FromImage(this.searchResultImg);
            //    outputGC.Clear(Color.FromArgb(255, 0, 255));
            //    foreach (PFTreeNode node in region.ContainedNodes)
            //    {
            //        RCIntRectangle sectionRect = node.AreaOnMap * new RCIntVector(CELL_SIZE, CELL_SIZE);
            //        outputGC.FillRectangle(Brushes.Cyan, sectionRect.X, sectionRect.Y, sectionRect.Width, sectionRect.Height);
            //        outputGC.DrawRectangle(Pens.Black, sectionRect.X, sectionRect.Y, sectionRect.Width, sectionRect.Height);
            //    }
            //    outputGC.Dispose();
            //    this.searchResultImg.MakeTransparent(Color.FromArgb(255, 0, 255));

            //    this.Invalidate();
            //}
        }

        private void MainForm_MouseWheel(object sender, MouseEventArgs e)
        {
            if (this.computedPaths.Count != 1) { return; }

            if (e.Delta < 0) { this.blockedNodeIndex = Math.Max(0, this.blockedNodeIndex - 1); }
            else if (e.Delta > 0) { this.blockedNodeIndex = Math.Min(this.computedPaths[0].Length - 1, this.blockedNodeIndex + 1); }

            if (this.blockedNodeSelectionImg != null) { this.blockedNodeSelectionImg.Dispose(); this.blockedNodeSelectionImg = null; }

            this.blockedNodeSelectionImg = new Bitmap(this.pathfinder.PathfinderTreeRoot.AreaOnMap.Width * CELL_SIZE, this.pathfinder.PathfinderTreeRoot.AreaOnMap.Height * CELL_SIZE, PixelFormat.Format24bppRgb);
            Graphics outputGC = Graphics.FromImage(this.blockedNodeSelectionImg);
            outputGC.Clear(Color.FromArgb(255, 0, 255));

            RCIntRectangle blockedNodeRect = this.computedPaths[0].GetPathNode(this.blockedNodeIndex).AreaOnMap * new RCIntVector(CELL_SIZE, CELL_SIZE);
            RCIntVector blockedNodeCenter = new RCIntVector((blockedNodeRect.Left + blockedNodeRect.Right) / 2, (blockedNodeRect.Top + blockedNodeRect.Bottom) / 2);
            outputGC.DrawEllipse(Pens.Blue, blockedNodeCenter.X - 3, blockedNodeCenter.Y - 3, 6, 6);
            outputGC.Dispose();
            this.blockedNodeSelectionImg.MakeTransparent(Color.FromArgb(255, 0, 255));

            this.Invalidate();
        }

        private bool CheckPathCompleteness(List<RC.Engine.Simulator.Core.Path> pathsToCheck)
        {
            foreach (RC.Engine.Simulator.Core.Path path in pathsToCheck) { if (!path.IsReadyForUse) { return false; } }
            return true;
        }

        /// <summary>
        /// The size of a cell on the result images.
        /// </summary>
        private const int CELL_SIZE = 4;

        /// <summary>
        /// The image that contains the original map
        /// </summary>
        private Bitmap originalMapImg;

        /// <summary>
        /// The image that contains the currently computed path.
        /// </summary>
        private Bitmap searchResultImg;

        /// <summary>
        /// The image that contains the currently computed detour.
        /// </summary>
        private Bitmap detourSearchResultImg;

        /// <summary>
        /// The image that contains the currently selected blocked node.
        /// </summary>
        private Bitmap blockedNodeSelectionImg;

        /// <summary>
        /// The total time of the last search operation in milliseconds.
        /// </summary>
        private int lastSearchTime;

        /// <summary>
        /// The total number of iterations of the last search operation.
        /// </summary>
        private int lastSearchIterations;

        /// <summary>
        /// The total number of frames elapsed during the last search operation.
        /// </summary>
        private int lastSearchFrames;

        /// <summary>
        /// The coordinates of the selected source nodes.
        /// </summary>
        private List<RCIntVector> fromCoords;

        /// <summary>
        /// The coordinates of the target node.
        /// </summary>
        private RCIntVector toCoord;

        /// <summary>
        /// The list of the currently computed paths.
        /// </summary>
        private List<RC.Engine.Simulator.Core.Path> computedPaths;

        /// <summary>
        /// The index of the currently selected blocked node or -1 if multiple paths were computed.
        /// </summary>
        private int blockedNodeIndex;

        /// <summary>
        /// Reference to the pathfinder component.
        /// </summary>
        private Engine.Simulator.Core.PathFinder pathfinder;
    }
}
