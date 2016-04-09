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
using MC = RC.Prototypes.NewPathfinding.MotionControl;
using RC.Prototypes.NewPathfinding.MotionControl;
using RC.Prototypes.JumpPointSearch.MotionControl;

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
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.mapImage = (Bitmap)Image.FromFile("maze_test.png");
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
                /// Source cell selection.
                this.sourceCell = this.grid[e.X / CELL_SIZE, e.Y / CELL_SIZE];
            }
            else if (e.Button == MouseButtons.Right)
            {
                /// Target cell selection.
                if (this.sourceCell == null) { return; }
                this.targetCell = this.grid[e.X / CELL_SIZE, e.Y / CELL_SIZE];

                /// Execute the high-level pathfinding.
                PathfindingAlgorithm<MC.Region> pathfinding =
                    new PathfindingAlgorithm<MC.Region>(this.sourceCell.GetRegion(OBJECT_SIZE), new GridTopologyGraph(this.targetCell, OBJECT_SIZE));
                this.lastHighLevelResult = pathfinding.Run();
                this.currentRegionIndex = 0;
                this.currentStartCell = this.sourceCell;

                /// Draw the result.
                this.resultImageGC.Clear(Color.FromArgb(0, Color.White));
                foreach (MC.Region currentRegion in this.lastHighLevelResult.ExploredNodes) { this.DrawRegion(currentRegion, Color.LightGray); }
                foreach (MC.Region currentRegion in this.lastHighLevelResult.Path) { this.DrawRegion(currentRegion, Color.Gray); }

                Console.WriteLine("Search time: {0} ms", this.lastHighLevelResult.ElapsedTime);
                Console.WriteLine("Explored nodes: {0}", this.lastHighLevelResult.ExploredNodes.Count);

                this.Invalidate();
            }
            else if (e.Button == MouseButtons.Middle)
            {
                if (this.currentRegionIndex == this.lastHighLevelResult.Path.Count) { return; }

                /// Execute the low-level pathfinding for the current region index.
                IGraph<MC.Cell> graph = null;
                if (this.currentRegionIndex < this.lastHighLevelResult.Path.Count - 1)
                {
                    graph = new TransitRegionGraph(this.lastHighLevelResult.Path[this.currentRegionIndex], this.lastHighLevelResult.Path[this.currentRegionIndex + 1]);
                }
                else
                {
                    graph = new TargetRegionGraph(this.lastHighLevelResult.Path[this.currentRegionIndex], this.targetCell);
                }
                PathfindingAlgorithm<MC.Cell> pathfinding =
                    new PathfindingAlgorithm<MC.Cell>(this.currentStartCell, graph);
                this.lastLowLevelResult = pathfinding.Run();
                this.currentRegionIndex++;
                this.currentStartCell = this.lastLowLevelResult.Path.Last();

                /// Draw the result.
                foreach (MC.Cell currentCell in this.lastLowLevelResult.ExploredNodes) { this.resultImage.SetPixel(currentCell.Coords.X, currentCell.Coords.Y, Color.Red); }
                foreach (MC.Cell currentCell in this.lastLowLevelResult.Path) { this.resultImage.SetPixel(currentCell.Coords.X, currentCell.Coords.Y, Color.Yellow); }

                Console.WriteLine("Search time: {0} ms", this.lastLowLevelResult.ElapsedTime);
                Console.WriteLine("Explored nodes: {0}", this.lastLowLevelResult.ExploredNodes.Count);

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

        private void DrawRegion(MC.Region region, Color color)
        {
            foreach (Cell cellOfRegion in region.AllCells)
            {
                this.resultImage.SetPixel(cellOfRegion.Coords.X, cellOfRegion.Coords.Y, color);
            }
        }

        private void DrawRegionsToOutput(int objectSize, Grid grid)
        {
            Random random = new Random();
            //Dictionary<MC.Region, Color> colorsPerRegion = new Dictionary<MC.Region, Color>();
            Dictionary<MC.Region, Brush> colorsPerRegion = new Dictionary<MC.Region, Brush>();
            Bitmap outputImg = new Bitmap(grid.Width * OUTPUT_CELLSIZE, grid.Height * OUTPUT_CELLSIZE);
            Graphics outputGC = Graphics.FromImage(outputImg);
            for (int row = 0; row < grid.Height; row++)
            {
                for (int col = 0; col < grid.Width; col++)
                {
                    Rectangle cellRect = new Rectangle(col * OUTPUT_CELLSIZE, row * OUTPUT_CELLSIZE, OUTPUT_CELLSIZE, OUTPUT_CELLSIZE);
                    Cell cell = grid[col, row];
                    MC.Region regionOfCell = cell.GetRegion(objectSize);
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
        /// Result of the last high-level pathfinding.
        /// </summary>
        private PathfindingResult<MC.Region> lastHighLevelResult;

        /// <summary>
        /// The result of the last low-level pathfinding.
        /// </summary>
        private PathfindingResult<MC.Cell> lastLowLevelResult;
        private int currentRegionIndex;
        private MC.Cell currentStartCell;

        private const int CELL_SIZE = 2;
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
