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

namespace RC.Engine.PathFinder.Test
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            this.fromCoords = RCIntVector.Undefined;
            this.toCoords = RCIntVector.Undefined;

            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            ComponentManager.RegisterComponents("RC.Engine.Simulator, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
                                                new string[1] { "RC.Engine.Simulator.PathFinder" });
            ComponentManager.StartComponents();

            this.pathFinder = ComponentManager.GetInterface<IPathFinder>();
            this.pfTreeRoot = this.ReadTestMap("pathfinder_testmap.png");

            this.originalMapImg = new Bitmap(this.pfTreeRoot.AreaOnMap.Width * CELL_SIZE, this.pfTreeRoot.AreaOnMap.Height * CELL_SIZE);
            Graphics outputGC = Graphics.FromImage(this.originalMapImg);
            HashSet<PFTreeNode> leafNodes = this.pfTreeRoot.GetAllLeafNodes();
            foreach (PFTreeNode leafNode in leafNodes)
            {
                RCIntRectangle nodeRect = leafNode.AreaOnMap * new RCIntVector(CELL_SIZE, CELL_SIZE);
                if (!leafNode.IsWalkable)
                {
                    outputGC.FillRectangle(Brushes.Black, nodeRect.X, nodeRect.Y, nodeRect.Width, nodeRect.Height);
                }
                outputGC.DrawRectangle(Pens.Black, nodeRect.X, nodeRect.Y, nodeRect.Width, nodeRect.Height);
            }

            outputGC.Dispose();

            this.ClientSize = new Size(this.originalMapImg.Width, this.originalMapImg.Height);
        }

        private PFTreeNode ReadTestMap(string fileName)
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

        private const int CELL_SIZE = 1;

        /// <summary>
        /// Reference to the pathfinder component.
        /// </summary>
        private IPathFinder pathFinder;

        /// <summary>
        /// The root of the pathfinder tree.
        /// </summary>
        private PFTreeNode pfTreeRoot;

        /// <summary>
        /// The image that contains the original map
        /// </summary>
        private Bitmap originalMapImg;

        /// <summary>
        /// The image that contains the currently computed path.
        /// </summary>
        private Bitmap currentPathImg;

        private int currentPathTime;

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(this.originalMapImg, 0, 0);
            if (this.currentPathImg != null)
            {
                e.Graphics.DrawImage(this.currentPathImg, 0, 0);
                e.Graphics.DrawString(this.currentPathTime.ToString(), SystemFonts.CaptionFont, Brushes.Cyan, 0.0f, 0.0f);
            }
        }

        private void MainForm_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (this.fromCoords != RCIntVector.Undefined && this.toCoords != RCIntVector.Undefined)
                {
                    this.fromCoords = RCIntVector.Undefined;
                    this.toCoords = RCIntVector.Undefined;
                }

                RCIntVector mapCoords = new RCIntVector(e.X, e.Y) / CELL_SIZE;
                if (this.fromCoords == RCIntVector.Undefined)
                {
                    this.fromCoords = mapCoords;
                }
                else if (this.toCoords == RCIntVector.Undefined)
                {
                    this.toCoords = mapCoords;
                    if (this.currentPathImg != null) { this.currentPathImg.Dispose(); }

                    Stopwatch watch = new Stopwatch();
                    watch.Start();
                    Path testPath = new Path(this.pfTreeRoot.GetLeafNode(this.fromCoords), this.toCoords, new RCNumVector(2, 2));
                    watch.Stop();
                    this.currentPathTime = (int)watch.ElapsedMilliseconds;

                    this.currentPathImg = new Bitmap(this.pfTreeRoot.AreaOnMap.Width * CELL_SIZE, this.pfTreeRoot.AreaOnMap.Height * CELL_SIZE, PixelFormat.Format24bppRgb);
                    Graphics outputGC = Graphics.FromImage(this.currentPathImg);
                    outputGC.Clear(Color.FromArgb(255, 0, 255));
                    HashSet<RCIntRectangle> sectionsOnPath = new HashSet<RCIntRectangle>();
                    for (int i = 0; i < testPath.Length; ++i)
                    {
                        RCIntRectangle sectionRect = testPath[i] * new RCIntVector(CELL_SIZE, CELL_SIZE);
                        outputGC.FillRectangle(Brushes.Blue, sectionRect.X, sectionRect.Y, sectionRect.Width, sectionRect.Height);
                        outputGC.DrawRectangle(Pens.Black, sectionRect.X, sectionRect.Y, sectionRect.Width, sectionRect.Height);
                        sectionsOnPath.Add(sectionRect);
                    }
                    foreach (PFTreeNode completedNode in testPath.CompletedNodes)
                    {
                        if (!sectionsOnPath.Contains(completedNode.AreaOnMap))
                        {
                            RCIntRectangle nodeRect = completedNode.AreaOnMap * new RCIntVector(CELL_SIZE, CELL_SIZE);
                            outputGC.FillRectangle(Brushes.Red, nodeRect.X, nodeRect.Y, nodeRect.Width, nodeRect.Height);
                            outputGC.DrawRectangle(Pens.Black, nodeRect.X, nodeRect.Y, nodeRect.Width, nodeRect.Height);
                        }
                    }
                    outputGC.Dispose();
                    this.currentPathImg.MakeTransparent(Color.FromArgb(255, 0, 255));

                    this.Invalidate();
                }
            }
        }

        private RCIntVector fromCoords;
        private RCIntVector toCoords;
    }
}
