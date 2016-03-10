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
            throw new NotImplementedException();
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
            this.gridImg = this.DrawGrid(testMapGrid);
            this.agentsImg = new Bitmap(testMapGrid.Width * CELL_SIZE, testMapGrid.Height * CELL_SIZE, PixelFormat.Format32bppArgb);
            this.agentsImgGC = Graphics.FromImage(this.agentsImg);
            this.agentsImgGC.Clear(Color.FromArgb(0, Color.White));
            this.ClientSize = new Size(this.gridImg.Width, this.gridImg.Height);

            this.testAgent = this.pathfinder.PlaceAgent(new RCIntRectangle(0, 0, 3, 3), this);
            this.DrawAgents();

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
            this.DrawAgents();
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
        /// Creates the image of the walkability grid.
        /// </summary>
        /// <param name="walkabilityReader">The walkability grid.</param>
        /// <returns>The created image.</returns>
        private Bitmap DrawGrid(IWalkabilityReader walkabilityReader)
        {
            Bitmap result = new Bitmap(walkabilityReader.Width * CELL_SIZE, walkabilityReader.Height * CELL_SIZE, PixelFormat.Format24bppRgb);
            Graphics outputGC = Graphics.FromImage(result);
            outputGC.Clear(Color.FromArgb(0, 0, 0));

            for (int row = 0; row < walkabilityReader.Height; row++)
            {
                for (int col = 0; col < walkabilityReader.Width; col++)
                {
                    Rectangle cellRect = new Rectangle(col * CELL_SIZE, row * CELL_SIZE, CELL_SIZE, CELL_SIZE);
                    if (walkabilityReader[col, row])
                    {
                        outputGC.FillRectangle(Brushes.White, cellRect);
                        outputGC.DrawRectangle(Pens.Black, cellRect);
                    }
                    else
                    {
                        outputGC.FillRectangle(Brushes.Black, cellRect);
                    }
                }
            }

            outputGC.Dispose();
            //result.Save("grid.png");
            return result;
        }

        /// <summary>
        /// Draws the current state of the agents.
        /// </summary>
        private void DrawAgents()
        {
            Rectangle agentRect = new Rectangle(this.testAgent.Area.X * CELL_SIZE, this.testAgent.Area.Y * CELL_SIZE, this.testAgent.Area.Width * CELL_SIZE, this.testAgent.Area.Height * CELL_SIZE);
            this.agentsImgGC.Clear(Color.FromArgb(0, Color.White));
            this.agentsImgGC.FillRectangle(this.testAgent.IsMoving ? Brushes.LightGreen : Brushes.Green, agentRect);
            this.agentsImg.Save("agents.png");
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
        private const int CELL_SIZE = 10;
    }
}
