using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RC.Prototypes.NewPathfinding.MotionControl;
using RC.Prototypes.NewPathfinding.Pathfinding;
using JPS = RC.Prototypes.NewPathfinding.MotionControl;

namespace RC.Prototypes.NewPathfinding
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            this.sourceCell = null;
            this.targetCell = null;

            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            e.Graphics.DrawImage(this.mapImage, 0, 0, this.mapImage.Width * CELL_SIZE, this.mapImage.Height * CELL_SIZE);
            e.Graphics.DrawImage(this.resultImage, 0, 0, this.resultImage.Width * CELL_SIZE, this.resultImage.Height * CELL_SIZE);
            if (this.lastResult != null)
            {
                Console.WriteLine("Search time: {0} ms", this.lastResult.ElapsedTime);
                Console.WriteLine("Explored nodes: {0}", this.lastResult.ExploredNodes.Count);
                //e.Graphics.DrawString(string.Format("Search time: {0} ms", this.lastResult.ElapsedTime), SystemFonts.CaptionFont, Brushes.Blue, 0.0f, 0.0f);
                //e.Graphics.DrawString(string.Format("Explored nodes: {0}", this.lastResult.ExploredNodes.Count), SystemFonts.CaptionFont, Brushes.Blue, 0.0f, 10.0f);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.mapImage = (Bitmap)Image.FromFile("maze.png");
            this.grid = new Grid(this.mapImage);

            //this.DrawLabelsOutput(this.grid);

            this.DrawRegionsToOutput(1, this.grid);
            this.DrawRegionsToOutput(2, this.grid);
            this.DrawRegionsToOutput(3, this.grid);
            this.DrawRegionsToOutput(4, this.grid);

            this.ClientSize = new Size(this.mapImage.Width * CELL_SIZE, this.mapImage.Height * CELL_SIZE);

            this.resultImage = new Bitmap(this.mapImage.Width, this.mapImage.Height, PixelFormat.Format32bppArgb);
            this.resultImageGC = Graphics.FromImage(this.resultImage);
            this.resultImageGC.Clear(Color.FromArgb(0, Color.White));
        }

        private void MainForm_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                /// Left-click event handling.
                this.sourceCell = this.grid[e.X / CELL_SIZE, e.Y / CELL_SIZE];
            }
            else if (e.Button == MouseButtons.Right)
            {
                /// Right-click event handling.
                if (this.sourceCell == null) { return; }
                this.targetCell = this.grid[e.X / CELL_SIZE, e.Y / CELL_SIZE];

                /// Execute and measure the pathfinding.
                PathfindingAlgorithm<Cell> pathfinding = new PathfindingAlgorithm<Cell>(this.sourceCell, this.targetCell, OBJECT_SIZE);
                //PathfindingOperation pathfinding = new PathfindingOperation(this.walkabilityGrid, this.fromCoord, this.toCoord, OBJECT_SIZE);
                //PathfindingOperation pathfinding = new PathfindingOperation(this.walkabilityGrid, new Point(0, 4), new Point(7, 4), OBJECT_SIZE);
                this.lastResult = pathfinding.Run();

                /// Draw the result.
                this.resultImageGC.Clear(Color.FromArgb(0, Color.White));

                foreach (Cell exploredCell in this.lastResult.ExploredNodes)
                {
                    this.resultImage.SetPixel(exploredCell.Coords.X, exploredCell.Coords.Y, Color.FromArgb(255, Color.Red));
                }

                Cell previousCell = null;
                foreach (Cell currentCell in this.lastResult.Path)
                {
                    if (previousCell == null)
                    {
                        previousCell = currentCell;
                        continue;
                    }

                    this.resultImageGC.DrawLine(Pens.Yellow, previousCell.Coords, currentCell.Coords);
                    previousCell = currentCell;
                }

                this.Invalidate();
            }
        }

        //private void DrawLabelsOutput(Grid grid)
        //{
        //    const int SIZE = 20;
        //    Bitmap outputImg = new Bitmap(grid.Width * SIZE, grid.Height * SIZE);
        //    Graphics outputGC = Graphics.FromImage(outputImg);
        //    Sector sector = grid[0, 0].Sector;
        //    for (int row = 0; row < sector.AreaOnGrid.Height; row++)
        //    {
        //        for (int col = 0; col < sector.AreaOnGrid.Width; col++)
        //        {
        //            Rectangle cellRect = new Rectangle(col * SIZE, row * SIZE, SIZE, SIZE);
        //            if (sector.Labels[col, row] != 0)
        //            {
        //                outputGC.DrawRectangle(Pens.Black, cellRect);
        //                outputGC.DrawString(sector.Labels[col, row].ToString(), SystemFonts.CaptionFont, Brushes.Blue, cellRect.Left + 2, cellRect.Top + 2);
        //            }
        //            else
        //            {
        //                outputGC.FillRectangle(Brushes.Black, cellRect);
        //            }
        //        }
        //    }
        //    outputGC.Dispose();
        //    outputImg.Save("labels.png");
        //    outputImg.Dispose();
        //}

        private void DrawRegionsToOutput(int objectSize, Grid grid)
        {
            Random random = new Random();
            //Dictionary<JPS.Region, Color> colorsPerRegion = new Dictionary<JPS.Region, Color>();
            Dictionary<JPS.Region, Brush> colorsPerRegion = new Dictionary<JPS.Region, Brush>();
            Bitmap outputImg = new Bitmap(grid.Width * OUTPUT_CELLSIZE, grid.Height * OUTPUT_CELLSIZE);
            Graphics outputGC = Graphics.FromImage(outputImg);
            for (int row = 0; row < grid.Height; row++)
            {
                for (int col = 0; col < grid.Width; col++)
                {
                    Rectangle cellRect = new Rectangle(col * OUTPUT_CELLSIZE, row * OUTPUT_CELLSIZE, OUTPUT_CELLSIZE, OUTPUT_CELLSIZE);
                    Cell cell = grid[col, row];
                    JPS.Region regionOfCell = cell.GetRegion(objectSize);
                    if (regionOfCell == null)
                    {
                        outputGC.FillRectangle(Brushes.Black, cellRect);
                        //outputImg.SetPixel(col, row, Color.Black);
                    }
                    else
                    {
                        if (!colorsPerRegion.ContainsKey(regionOfCell))
                        {
                            colorsPerRegion[regionOfCell] = new SolidBrush(Color.FromArgb(random.Next(100, 256), random.Next(100, 256), random.Next(100, 256)));
                            //colorsPerRegion[regionOfCell] = Color.FromArgb(random.Next(100, 256), random.Next(100, 256), random.Next(100, 256));
                        }
                        List<int> exitDirections = new List<int>();
                        for (int direction = 0; direction < GridDirections.DIRECTION_COUNT; direction++)
                        {
                            if (regionOfCell.ExitCells[direction].Contains(cell))
                            {
                                exitDirections.Add(direction);
                            }
                        }
                        Brush colorOfRegion = colorsPerRegion[regionOfCell];
                        outputGC.FillRectangle(colorOfRegion, cellRect);
                        outputGC.DrawRectangle(Pens.Black, cellRect);
                        //outputImg.SetPixel(col, row, Color.FromArgb(colorOfRegion.R - 60, colorOfRegion.G - 60, colorOfRegion.B - 60));
                        foreach (int exitDirection in exitDirections)
                        {
                            Point lineBegin = new Point(cellRect.Left + EXITVECTOR_BEGIN[exitDirection].X, cellRect.Top + EXITVECTOR_BEGIN[exitDirection].Y);
                            //Point lineBegin = new Point((cellRect.Left + cellRect.Right) / 2, (cellRect.Top + cellRect.Bottom) / 2);
                            Size directionVector = GridDirections.DIRECTION_VECTOR[exitDirection];
                            Point lineEnd = new Point(lineBegin.X + directionVector.Width * OUTPUT_CELLSIZE / 2, lineBegin.Y + directionVector.Height * OUTPUT_CELLSIZE / 2);
                            outputGC.DrawLine(Pens.Yellow, lineBegin, lineEnd);
                            outputGC.DrawEllipse(Pens.Yellow, new Rectangle(lineBegin.X - 2, lineBegin.Y - 2, 5, 5));
                        }
                    }
                }
            }
            outputGC.Dispose();
            outputImg.Save("regions_" + objectSize + ".png");
            outputImg.Dispose();
        }

        private Bitmap mapImage;

        private Bitmap resultImage;

        private Graphics resultImageGC;

        /// <summary>
        /// The grid.
        /// </summary>
        private Grid grid;

        /// <summary>
        /// The selected source cell.
        /// </summary>
        private Cell sourceCell;

        /// <summary>
        /// The selected target cell.
        /// </summary>
        private Cell targetCell;

        /// <summary>
        /// Result of the last pathfinding.
        /// </summary>
        private PathfindingResult<Cell> lastResult;

        private const int CELL_SIZE = 3;
        private const int OBJECT_SIZE = 1;

        const int OUTPUT_CELLSIZE = 20;
        private static readonly Point[] EXITVECTOR_BEGIN = new Point[]
        {
            new Point(OUTPUT_CELLSIZE / 2, OUTPUT_CELLSIZE / 4),
            new Point(3 * OUTPUT_CELLSIZE / 4, OUTPUT_CELLSIZE / 4),
            new Point(3 * OUTPUT_CELLSIZE / 4, OUTPUT_CELLSIZE / 2),
            new Point(3 * OUTPUT_CELLSIZE / 4, 3 * OUTPUT_CELLSIZE / 4),
            new Point(OUTPUT_CELLSIZE / 2, 3 * OUTPUT_CELLSIZE / 4),
            new Point(OUTPUT_CELLSIZE / 4, 3 * OUTPUT_CELLSIZE / 4),
            new Point(OUTPUT_CELLSIZE / 4, OUTPUT_CELLSIZE / 2),
            new Point(OUTPUT_CELLSIZE / 4, OUTPUT_CELLSIZE / 4),
        };
    }
}
