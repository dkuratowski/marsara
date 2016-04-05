using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.IO;
using RC.Common;
using RC.Engine.Pathfinder.PublicInterfaces;
using RC.Engine.Pathfinder.Core;

namespace RC.Engine.PathFinder.Test
{
    public partial class MainForm : Form, IAgentClient
    {
        public MainForm()
        {
            this.pathfinder = null;
            this.gridImg = null;
            this.agentsImg = null;

            InitializeComponent();
        }

        #region IAgentClient members

        /// <see cref="IAgentClient.MaxSpeed"/>
        public RCNumber MaxSpeed
        {
            get { return (RCNumber)4 / (RCNumber)4; }
        }

        /// <see cref="IAgentClient.IsOverlapEnabled"/>
        public bool IsOverlapEnabled(IAgentClient otherClient)
        {
            return false;
        }

        #endregion IAgentClient members

        private void MainForm_Load(object sender, EventArgs e)
        {
            /// Create and initialize the pathfinder.
            this.pathfinder = new Pathfinder.Core.Pathfinder();
            Bitmap testMapBmp = (Bitmap)Bitmap.FromFile("testmap.png");
            TestWalkabilityReader testMapGrid = new TestWalkabilityReader(testMapBmp);
            testMapBmp.Dispose();
            this.pathfinder.Initialize(testMapGrid, MAX_OBJECT_SIZE);

            /// Draw the grid.
            this.gridImg = this.DrawGrid(this.pathfinder.Grid);
            this.agentsImg = new Bitmap(testMapGrid.Width * CELL_SIZE, testMapGrid.Height * CELL_SIZE, PixelFormat.Format32bppArgb);
            this.agentsImgGC = Graphics.FromImage(this.agentsImg);
            this.agentsImgGC.Clear(Color.FromArgb(0, Color.White));
            this.ClientSize = new Size(this.gridImg.Width, this.gridImg.Height);

            this.testAgent = this.pathfinder.PlaceAgent(new RCIntRectangle(0, 0, 3, 3), this);
            this.DrawAgent(this.testAgent, this.pathfinder.Grid);

            this.timer = new Timer();
            this.timer.Interval = 40;
            this.timer.Tick += this.OnTimerTick;
            this.timer.Start();

            this.DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(this.gridImg, 0, 0);
            if (this.agentsImg != null) { e.Graphics.DrawImage(this.agentsImg, 0, 0); }
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            this.pathfinder.Update();
            this.DrawAgent(this.testAgent, this.pathfinder.Grid);
            this.Invalidate();
        }

        private void MainForm_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                /// Left-click event handling.
                this.testAgent.MoveTo(new RCIntVector(e.X, e.Y) / CELL_SIZE);
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                /// Right-click event handling.
                this.testAgent.StopMoving();
            }
        }

        /// <summary>
        /// Creates the image of the pathfinding grid.
        /// </summary>
        /// <param name="grid">The pathfinding grid.</param>
        /// <returns>The created image.</returns>
        private Bitmap DrawGrid(Grid grid)
        {
            Bitmap result = new Bitmap(grid.Width * CELL_SIZE, grid.Height * CELL_SIZE, PixelFormat.Format24bppRgb);
            Graphics outputGC = Graphics.FromImage(result);
            outputGC.Clear(Color.FromArgb(0, 0, 0));

            RCSet<Sector> sectorsToDraw = new RCSet<Sector>();
            for (int row = 0; row < grid.Height; row++)
            {
                for (int col = 0; col < grid.Width; col++)
                {
                    Rectangle cellRect = new Rectangle(col * CELL_SIZE, row * CELL_SIZE, CELL_SIZE, CELL_SIZE);
                    Cell cellToDraw = grid[col, row];
                    if (cellToDraw.WallCellDistance > 0)
                    {
                        outputGC.FillRectangle(Brushes.White, cellRect);
                        outputGC.DrawRectangle(Pens.Black, cellRect);
                    }
                    else
                    {
                        outputGC.FillRectangle(Brushes.Black, cellRect);
                    }
                    outputGC.DrawString(cellToDraw.WallCellDistance.ToString(), SystemFonts.CaptionFont, Brushes.Blue, cellRect.Left + 1, cellRect.Top + 1);

                    sectorsToDraw.Add(cellToDraw.Sector);
                }
            }

            foreach (Sector sectorToDraw in sectorsToDraw)
            {
                Rectangle sectorRect = new Rectangle(sectorToDraw.AreaOnGrid.X * CELL_SIZE, sectorToDraw.AreaOnGrid.Y * CELL_SIZE, sectorToDraw.AreaOnGrid.Width * CELL_SIZE, sectorToDraw.AreaOnGrid.Height * CELL_SIZE);
                outputGC.DrawRectangle(Pens.Red, sectorRect);
            }

            outputGC.Dispose();
            //result.Save("grid.png");
            return result;
        }

        /// <summary>
        /// Draws the given agent.
        /// </summary>
        private void DrawAgent(IAgent agent, Grid grid)
        {
            Rectangle agentRect = new Rectangle(agent.Area.X * CELL_SIZE, agent.Area.Y * CELL_SIZE, agent.Area.Width * CELL_SIZE, agent.Area.Height * CELL_SIZE);
            this.agentsImgGC.Clear(Color.FromArgb(0, Color.White));
            this.agentsImgGC.FillRectangle(agent.IsMoving ? Brushes.LightGreen : Brushes.Green, agentRect);

            //for (int row = agent.Area.Top - (grid.MaxMovingSize/* - 1*/); row < agent.Area.Bottom; row++)
            //{
            //    for (int column = agent.Area.Left - (grid.MaxMovingSize/* - 1*/); column < agent.Area.Right; column++)
            //    {
            //        Cell cell = grid[column, row];
            //        if (cell != null)
            //        {
            //            int size = 1;
            //            for (; size <= grid.MaxMovingSize && cell.GetAgents(size).Count == 0; size++) { }

            //            if (size <= grid.MaxMovingSize)
            //            {
            //                Rectangle cellRect = new Rectangle(column * CELL_SIZE, row * CELL_SIZE, CELL_SIZE, CELL_SIZE);
            //                this.agentsImgGC.FillRectangle(Brushes.Green, cellRect);
            //                this.agentsImgGC.DrawString((size - 1).ToString(), SystemFonts.CaptionFont, Brushes.Red, cellRect.Left + 1, cellRect.Top + 1);
            //            }
            //        }
            //    }
            //}

            //this.agentsImg.Save("agents.png");
        }

        /// <summary>
        /// The image that contains the grid.
        /// </summary>
        private Bitmap gridImg;

        /// <summary>
        /// The image that contains the agents.
        /// </summary>
        private Bitmap agentsImg;

        /// <summary>
        /// The render context of the agents image.
        /// </summary>
        private Graphics agentsImgGC;

        /// <summary>
        /// Reference to the pathfinder component.
        /// </summary>
        private Pathfinder.Core.Pathfinder pathfinder;

        /// <summary>
        /// Reference to the test agent.
        /// </summary>
        private IAgent testAgent;

        /// <summary>
        /// Reference to the timer instance.
        /// </summary>
        private Timer timer;

        /// <summary>
        /// The maximum size of objects.
        /// </summary>
        private const int MAX_OBJECT_SIZE = 4;

        /// <summary>
        /// The size of a cell on the result images.
        /// </summary>
        private const int CELL_SIZE = 15;
    }
}
