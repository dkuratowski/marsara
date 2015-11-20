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
using RC.UnitTests;

namespace RC.Engine.PathFinder.Test
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            this.fromCoords = new List<RCIntVector>();
            this.toCoord = RCIntVector.Undefined;
            this.computedPaths = new List<Simulator.MotionControl.Path>();
            this.pathfinder = null;

            this.originalOutputImg = null;
            this.searchResultImg = null;
            this.lastSearchTime = 0;
            this.lastSearchIterations = 0;
            this.lastSearchFrames = 0;

            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            /// Create and initialize the pathfinder.
            this.pathfinder = new Simulator.MotionControl.PathFinder();
            //this.pathfinder.Initialize(this.ReadTestMap("..\\..\\..\\..\\maps\\testmap4.rcm"), 5000);
            this.pathfinder.Initialize(this.ReadTestMapFromImg("pathfinder_testmap.png"), 5000);

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
            INavMesh loadedNavMesh = navmeshLoader.LoadNavMesh(File.ReadAllBytes(mapFileName));

            /// Draw the navmesh into an output file.
            using (NavmeshPainter navmeshPainter = new NavmeshPainter(loadedNavMesh.GridSize, 4, new RCIntVector(0, 0)))
            {
                foreach (NavMeshNode node in loadedNavMesh.Nodes) { navmeshPainter.DrawNode(node); }
                foreach (NavMeshNode node in loadedNavMesh.Nodes) { navmeshPainter.DrawNeighbourLines(node); }
                navmeshPainter.OutputImage.Save("navmesh.png");
            }

            return loadedNavMesh;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(this.originalOutputImg, 0, 0);
            if (this.searchResultImg != null) { e.Graphics.DrawImage(this.searchResultImg, 0, 0); }
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

                Stopwatch watch = new Stopwatch();
                this.computedPaths = new List<RC.Engine.Simulator.MotionControl.Path>();
                foreach (RCIntVector fromCoord in this.fromCoords) { this.computedPaths.Add((RC.Engine.Simulator.MotionControl.Path)this.pathfinder.StartPathSearching(fromCoord, toCoord, 5000)); }
                this.pathfinder.Flush();
                this.lastSearchFrames = 1;
                watch.Start();
                while (!CheckPathCompleteness(this.computedPaths))
                {
                    this.pathfinder.Flush();
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
                RCSet<RCPolygon> nodesOnPath = new RCSet<RCPolygon>();
                foreach (RC.Engine.Simulator.MotionControl.Path path in this.computedPaths)
                {
                    for (int i = 0; i < path.Length; ++i)
                    {
                        this.FillPolygon(path[i].Polygon, outputGC, path.IsTargetFound ? Brushes.LightGreen : Brushes.Orange, Pens.Black);
                        nodesOnPath.Add(path[i].Polygon);
                    }
                    foreach (INavMeshNode completedNode in path.CompletedNodes)
                    {
                        if (!nodesOnPath.Contains(completedNode.Polygon))
                        {
                            this.FillPolygon(completedNode.Polygon, outputGC, Brushes.Red, Pens.Black);
                        }
                    }
                    if (this.computedPaths.Count == 1)
                    {
                        this.DrawPreferredVelocities(path, outputGC);
                        //this.DrawDistancesToTarget(path, outputGC);
                    }
                    //for (int i = 1; i < path.Length; ++i)
                    //{
                    //    RCNumVector prevNodeCenter = path[i - 1].Center * new RCNumVector(CELL_SIZE, CELL_SIZE);
                    //    RCNumVector currNodeCenter = path[i].Center * new RCNumVector(CELL_SIZE, CELL_SIZE);
                    //    outputGC.DrawLine(Pens.Blue, prevNodeCenter.X.Round(), prevNodeCenter.Y.Round(), currNodeCenter.X.Round(), currNodeCenter.Y.Round());
                    //}
                }
                outputGC.Dispose();
                this.searchResultImg.MakeTransparent(Color.FromArgb(255, 0, 255));
                this.Invalidate();
            }
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
                INavMeshNode pathNode = path[nodeIdx];
                RCNumRectangle boundingBox = pathNode.BoundingBox;
                for (int y = boundingBox.Top.Round(); y < boundingBox.Bottom.Round(); y += 4)
                {
                    for (int x = boundingBox.Left.Round(); x < boundingBox.Right.Round(); x += 4)
                    {
                        RCNumVector position = new RCNumVector(x, y);
                        if (!pathNode.Polygon.Contains(position)) { continue; }

                        RCNumVector vector = RCNumVector.Undefined;
                        if (nodeIdx == path.Length - 1)
                        {
                            vector = path.ToCoords - position;
                        }
                        else
                        {
                            INavMeshNode nextPathNode = path[nodeIdx + 1];
                            INavMeshEdge edgeToNextNode = pathNode.GetEdge(nextPathNode);
                            vector = (edgeToNextNode.TransitionVector + edgeToNextNode.Midpoint - position) / 2;
                        }

                        if (vector == RCNumVector.Undefined) { continue; }
                        if (vector.Length != 0) { vector /= vector.Length; vector *= 3; }
                        RCNumVector lineBegin = position * new RCNumVector(CELL_SIZE, CELL_SIZE);
                        RCNumVector lineEnd = (position + vector) * new RCNumVector(CELL_SIZE, CELL_SIZE);
                        gc.DrawEllipse(Pens.Blue, lineBegin.X.Round() - 2, lineBegin.Y.Round() - 2, 4, 4);
                        gc.DrawLine(Pens.Blue, lineBegin.X.Round(), lineBegin.Y.Round(), lineEnd.X.Round(), lineEnd.Y.Round());
                    }
                }
            }
        }

        /// <summary>
        /// Draw the distances to the target position onto the path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="gc">The graphic context.</param>
        private void DrawDistancesToTarget(RC.Engine.Simulator.MotionControl.Path path, Graphics gc)
        {
            for (int nodeIdx = 0; nodeIdx < path.Length; nodeIdx++)
            {
                RCNumber pathMiddleLength = 0;
                for (int i = nodeIdx + 2; i < path.Length - 1; i++)
                {
                    pathMiddleLength += MapUtils.ComputeDistance(path[i - 1].Polygon.Center, path[i].Polygon.Center);
                }

                INavMeshNode pathNode = path[nodeIdx];
                RCNumRectangle boundingBox = pathNode.BoundingBox;
                for (int y = boundingBox.Top.Round(); y < boundingBox.Bottom.Round(); y += 1)
                {
                    for (int x = boundingBox.Left.Round(); x < boundingBox.Right.Round(); x += 1)
                    {
                        RCNumber distanceToTarget = pathMiddleLength;
                        RCNumVector position = new RCNumVector(x, y);
                        if (!pathNode.Polygon.Contains(position)) { continue; }

                        if (nodeIdx + 1 == path.Length - 1)
                        {
                            distanceToTarget += MapUtils.ComputeDistance(position, path.ToCoords);
                        }
                        else
                        {
                            if (nodeIdx + 1 < path.Length - 1)
                            {
                                distanceToTarget += MapUtils.ComputeDistance(position, path[nodeIdx + 1].Polygon.Center);
                            }
                            if (path.Length - 2 > nodeIdx)
                            {
                                distanceToTarget += MapUtils.ComputeDistance(path[path.Length - 2].Polygon.Center, path.ToCoords);
                            }
                        }

                        Color brushColor;
                        if (distanceToTarget >= 768) { continue; }
                        else if (distanceToTarget < 768 && distanceToTarget >= 512)
                        {
                            brushColor = Color.FromArgb(255 - ((int)distanceToTarget - 512), 0, 0);
                        }
                        else if (distanceToTarget < 512 && distanceToTarget >= 256)
                        {
                            brushColor = Color.FromArgb(255, 255 - ((int)distanceToTarget - 256), 0);
                        }
                        else
                        {
                            brushColor = Color.FromArgb(255, 255, 255 - (int)distanceToTarget);
                        }

                        RCNumVector circlePos = position * new RCNumVector(CELL_SIZE, CELL_SIZE);
                        Brush brush = new SolidBrush(brushColor);
                        gc.FillEllipse(brush, circlePos.X.Round() - 2, circlePos.Y.Round() - 2, 4, 4);
                        brush.Dispose();
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
        /// Reference to the pathfinder component.
        /// </summary>
        private Engine.Simulator.MotionControl.PathFinder pathfinder;
    }
}
