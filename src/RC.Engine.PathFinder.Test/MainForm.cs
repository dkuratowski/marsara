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
using RC.Engine.Simulator.MotionControl;
using RC.Engine.Maps.Core;

namespace RC.Engine.PathFinder.Test
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            this.fromCoords = new List<RCIntVector>();
            this.toCoord = RCIntVector.Undefined;
            this.computedPaths = new List<Simulator.MotionControl.Path>();
            this.blockedNodeIndex = 0;
            this.pathfinder = null;

            this.originalOutputImg = null;
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
            /// Create and initialize the pathfinder.
            this.pathfinder = new Simulator.MotionControl.PathFinder();
            this.pathfinder.Initialize(this.ReadTestMap("..\\..\\..\\..\\maps\\testmap4.rcm"), 5000);
            //this.pathfinder.Initialize(this.ReadTestMapFromImg("pathfinder_testmap2.png"), 5000);

            /// Draw the navmesh nodes.
            this.originalOutputImg = new Bitmap(this.pathfinder.Navmesh.GridSize.X * CELL_SIZE, this.pathfinder.Navmesh.GridSize.Y * CELL_SIZE, PixelFormat.Format24bppRgb);
            Graphics outputGC = Graphics.FromImage(this.originalOutputImg);
            outputGC.Clear(Color.FromArgb(0, 0, 0));
            foreach (NavMeshNode node in this.pathfinder.Navmesh.Nodes)
            {
                this.FillPolygon(node.Polygon, outputGC, Brushes.White, Pens.Black);
            }
            outputGC.Dispose();

            this.ClientSize = new Size(this.originalOutputImg.Width, this.originalOutputImg.Height);
        }

        private INavMesh ReadTestMapFromImg(string fileName)
        {
            /// Load the test map and create its navmesh.
            Bitmap testMapBmp = (Bitmap)Bitmap.FromFile(fileName);
            TestWalkabilityGrid testMapGrid = new TestWalkabilityGrid(testMapBmp);
            testMapBmp.Dispose();
            return new NavMesh(testMapGrid, 2);
        }

        private INavMesh ReadTestMap(string mapFileName)
        {
            /// Load the navmesh from the mapfile.
            NavMeshLoader navmeshLoader = new NavMeshLoader();
            return navmeshLoader.LoadNavMesh(File.ReadAllBytes(mapFileName));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(this.originalOutputImg, 0, 0);
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
                this.computedPaths = new List<RC.Engine.Simulator.MotionControl.Path>();
                foreach (RCIntVector fromCoord in this.fromCoords) { this.computedPaths.Add((RC.Engine.Simulator.MotionControl.Path)this.pathfinder.StartPathSearching(fromCoord, toCoord, 5000)); }
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
                foreach (RC.Engine.Simulator.MotionControl.Path path in this.computedPaths)
                {
                    this.lastSearchIterations += path.CompletedNodes.Count();
                }

                this.searchResultImg = new Bitmap(this.pathfinder.Navmesh.GridSize.X * CELL_SIZE, this.pathfinder.Navmesh.GridSize.Y * CELL_SIZE, PixelFormat.Format24bppRgb);
                Graphics outputGC = Graphics.FromImage(this.searchResultImg);
                outputGC.Clear(Color.FromArgb(255, 0, 255));
                HashSet<RCPolygon> nodesOnPath = new HashSet<RCPolygon>();
                foreach (RC.Engine.Simulator.MotionControl.Path path in this.computedPaths)
                {
                    for (int i = 0; i < path.Length; ++i)
                    {
                        this.FillPolygon(path[i], outputGC, path.IsTargetFound ? Brushes.LightGreen : Brushes.Orange, Pens.Black);
                        nodesOnPath.Add(path[i]);
                    }
                    foreach (INavMeshNode completedNode in path.CompletedNodes)
                    {
                        if (!nodesOnPath.Contains(completedNode.Polygon))
                        {
                            this.FillPolygon(completedNode.Polygon, outputGC, Brushes.Red, Pens.Black);
                        }
                    }
                    //if (this.computedPaths.Count == 1)
                    //{
                    //    this.DrawPreferredVelocities(path, outputGC);
                    //}
                    for (int i = 1; i < path.Length; ++i)
                    {
                        RCNumVector prevNodeCenter = path[i - 1].Center * new RCNumVector(CELL_SIZE, CELL_SIZE);
                        RCNumVector currNodeCenter = path[i].Center * new RCNumVector(CELL_SIZE, CELL_SIZE);
                        outputGC.DrawLine(Pens.Blue, prevNodeCenter.X.Round(), prevNodeCenter.Y.Round(), currNodeCenter.X.Round(), currNodeCenter.Y.Round());
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
                RC.Engine.Simulator.MotionControl.Path originalPath = this.computedPaths[0];
                this.computedPaths = new List<RC.Engine.Simulator.MotionControl.Path>();
                this.computedPaths.Add((RC.Engine.Simulator.MotionControl.Path)this.pathfinder.StartDetourSearching(originalPath, this.blockedNodeIndex, 5000));
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
                foreach (RC.Engine.Simulator.MotionControl.Path path in this.computedPaths)
                {
                    this.lastSearchIterations += path.CompletedNodes.Count();
                }

                this.detourSearchResultImg = new Bitmap(this.pathfinder.Navmesh.GridSize.X * CELL_SIZE, this.pathfinder.Navmesh.GridSize.Y * CELL_SIZE, PixelFormat.Format24bppRgb);
                Graphics outputGC = Graphics.FromImage(this.detourSearchResultImg);
                outputGC.Clear(Color.FromArgb(255, 0, 255));

                /// Draw the blocked edges.
                HashSet<Tuple<INavMeshNode, INavMeshNode>> blockedEdges = new HashSet<Tuple<INavMeshNode, INavMeshNode>>();
                this.computedPaths[0].CopyBlockedEdges(ref blockedEdges);
                foreach (Tuple<INavMeshNode, INavMeshNode> blockedEdge in blockedEdges)
                {
                    RCNumVector edgeBegin = blockedEdge.Item1.Polygon.Center * new RCNumVector(CELL_SIZE, CELL_SIZE);
                    RCNumVector edgeEnd = blockedEdge.Item2.Polygon.Center * new RCNumVector(CELL_SIZE, CELL_SIZE);
                    outputGC.DrawLine(Pens.Red, edgeBegin.X.Round(), edgeBegin.Y.Round(), edgeEnd.X.Round(), edgeEnd.Y.Round());
                }

                /// Draw the detour.
                for (int i = 1; i < this.computedPaths[0].Length; ++i)
                {
                    RCNumVector prevNodeCenter = this.computedPaths[0][i - 1].Center * new RCNumVector(CELL_SIZE, CELL_SIZE);
                    RCNumVector currNodeCenter = this.computedPaths[0][i].Center * new RCNumVector(CELL_SIZE, CELL_SIZE);
                    outputGC.DrawLine(Pens.Green, prevNodeCenter.X.Round(), prevNodeCenter.Y.Round(), currNodeCenter.X.Round(), currNodeCenter.Y.Round());
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

            this.blockedNodeSelectionImg = new Bitmap(this.pathfinder.Navmesh.GridSize.X * CELL_SIZE, this.pathfinder.Navmesh.GridSize.Y * CELL_SIZE, PixelFormat.Format24bppRgb);
            Graphics outputGC = Graphics.FromImage(this.blockedNodeSelectionImg);
            outputGC.Clear(Color.FromArgb(255, 0, 255));

            RCNumVector blockedNodeCenter = this.computedPaths[0][this.blockedNodeIndex].Center * new RCNumVector(CELL_SIZE, CELL_SIZE);
            outputGC.DrawEllipse(Pens.Blue, blockedNodeCenter.X.Round() - 3, blockedNodeCenter.Y.Round() - 3, 6, 6);
            outputGC.Dispose();
            this.blockedNodeSelectionImg.MakeTransparent(Color.FromArgb(255, 0, 255));

            this.Invalidate();
        }

        private bool CheckPathCompleteness(List<RC.Engine.Simulator.MotionControl.Path> pathsToCheck)
        {
            foreach (RC.Engine.Simulator.MotionControl.Path path in pathsToCheck) { if (!path.IsReadyForUse) { return false; } }
            return true;
        }

        /// <summary>
        /// Draws and fills the given polygon to the given graphic context.
        /// </summary>
        /// <param name="polygon">The polygon to be drawn.</param>
        private void FillPolygon(RCPolygon polygon, Graphics gc, Brush brush, Pen pen)
        {
            Point[] polygonVertices = new Point[polygon.VertexCount];
            for (int i = 0; i < polygon.VertexCount; i++)
            {
                RCNumVector currPoint = (polygon[i] + new RCNumVector((RCNumber)1 / (RCNumber)2, (RCNumber)1 / (RCNumber)2)) * new RCNumVector(CELL_SIZE, CELL_SIZE);
                polygonVertices[i] = new Point(currPoint.X.Round(), currPoint.Y.Round());
            }
            gc.FillPolygon(brush, polygonVertices);
            gc.DrawPolygon(pen, polygonVertices);
        }

        /// <summary>
        /// Draw the preferred velocities onto the path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="gc">The graphic context.</param>
        private void DrawPreferredVelocities(RC.Engine.Simulator.MotionControl.Path path, Graphics gc)
        {
            for (int nodeIdx = 0; nodeIdx < path.Length; nodeIdx++)
            {
                RCPolygon pathNode = path[nodeIdx];
                RCNumRectangle boundingBox = pathNode.BoundingBox;
                for (int y = boundingBox.Top.Round(); y < boundingBox.Bottom.Round(); y += 4)
                {
                    for (int x = boundingBox.Left.Round(); x < boundingBox.Right.Round(); x += 4)
                    {
                        RCNumVector position = new RCNumVector(x, y);
                        RCNumVector vector = RCNumVector.Undefined;
                        if (!pathNode.Contains(position)) { continue; }
                        if (nodeIdx < path.Length - 2)
                        {
                            vector = path[nodeIdx + 2].Center - position;
                        }
                        else
                        {
                            vector = path.ToCoords - position;
                        }

                        if (vector == RCNumVector.Undefined) { continue; }
                        if (vector.Length != 0) { vector /= vector.Length; }
                        RCNumVector lineBegin = position * new RCNumVector(CELL_SIZE, CELL_SIZE);
                        RCNumVector lineEnd = (position + vector) * new RCNumVector(CELL_SIZE, CELL_SIZE);
                        gc.DrawLine(Pens.Blue, lineBegin.X.Round(), lineBegin.Y.Round(), lineEnd.X.Round(), lineEnd.Y.Round());
                    }
                }
            }
        }

        /// <summary>
        /// The size of a cell on the result images.
        /// </summary>
        private const int CELL_SIZE = 4;

        /// <summary>
        /// The object that creates the image of the navmesh.
        /// </summary>
        //private NavmeshPainter navmeshPainter;
        /// <summary>
        /// The original image of the navmesh.
        /// </summary>
        private Bitmap originalOutputImg;

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
        private List<RC.Engine.Simulator.MotionControl.Path> computedPaths;

        /// <summary>
        /// The index of the currently selected blocked node or -1 if multiple paths were computed.
        /// </summary>
        private int blockedNodeIndex;

        /// <summary>
        /// Reference to the pathfinder component.
        /// </summary>
        private Engine.Simulator.MotionControl.PathFinder pathfinder;
    }
}
