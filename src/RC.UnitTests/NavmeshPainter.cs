using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.Core;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.UnitTests
{
    /// <summary>
    /// Internal class used for painting navmeshes.
    /// </summary>
    public class NavmeshPainter : IDisposable
    {
        /// <summary>
        /// Constructs a NavmeshPainter instance without walkability grid informations.
        /// </summary>
        /// <param name="cellSize"></param>
        /// <param name="offset"></param>
        public NavmeshPainter(RCIntVector gridSize, int cellSize, RCIntVector offset)
        {
            if (cellSize <= 0) { throw new ArgumentOutOfRangeException("cellSize"); }
            if (offset == RCNumVector.Undefined) { throw new ArgumentNullException("offset"); }

            this.cellSize = cellSize;
            this.offset = offset;
            this.outputImage = new Bitmap((gridSize.X + 2 * offset.X) * cellSize, (gridSize.Y + 2 * offset.Y) * cellSize, PixelFormat.Format24bppRgb);
            this.graphicContext = Graphics.FromImage(this.outputImage);
            this.graphicContext.Clear(Color.White);
        }

        /// <summary>
        /// Constructs a NavmeshPainter instance.
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="cellSize"></param>
        /// <param name="offset"></param>
        public NavmeshPainter(IWalkabilityGrid grid, int cellSize, RCIntVector offset)
            : this(new RCIntVector(grid.Width, grid.Height), cellSize, offset)
        {
            if (grid == null) { throw new ArgumentNullException("grid"); }

            /// Draw the original grid enlarged.
            for (int row = 0; row < grid.Height; row++)
            {
                for (int col = 0; col < grid.Width; col++)
                {
                    if (!grid[new RCIntVector(col, row)])
                    {
                        this.graphicContext.FillRectangle(Brushes.Black, (col + offset.X) * cellSize, (row + offset.Y) * cellSize, cellSize, cellSize);
                    }
                }
            }
        }

        #region IDisposable methods

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            this.graphicContext.Dispose();
            this.outputImage.Dispose();
        }

        #endregion IDisposable methods

        /// <summary>
        /// Gets the output image of this painter.
        /// </summary>
        public Bitmap OutputImage { get { return this.outputImage; } }

        /// <summary>
        /// Draws the given navmesh node.
        /// </summary>
        /// <param name="node">The navmesh node to be drawn.</param>
        public void DrawNode(INavMeshNode node)
        {
            this.DrawPolygon(node.Polygon);

            RCNumVector nodeCenter = (node.Polygon.Center + this.offset) * new RCNumVector(this.cellSize, this.cellSize);
            this.graphicContext.DrawString(node.ID.ToString(), SystemFonts.SmallCaptionFont, Brushes.Green, nodeCenter.Round().X, nodeCenter.Round().Y);
        }

        /// <summary>
        /// Draws the neigbour relationships of the given navmesh node.
        /// </summary>
        /// <param name="node">The navmesh node whose neighbour relationships have to be drawn.</param>
        /// <remarks>
        /// Bi-directional neighbour relationships will be drawn with blue, one-way neighbour relationships will be
        /// drawn with yellow.
        /// </remarks>
        public void DrawNeighbourLines(INavMeshNode node)
        {
            RCNumVector nodeCenter = (node.Polygon.Center + this.offset) * new RCNumVector(this.cellSize, this.cellSize);
            this.graphicContext.DrawEllipse(Pens.Blue, nodeCenter.Round().X - 3, nodeCenter.Round().Y - 3, 6, 6);
            foreach (NavMeshNode neighbour in node.Neighbours)
            {
                bool isBidirectional = false;
                foreach (NavMeshNode neighbourOfNeighbour in neighbour.Neighbours) { if (neighbourOfNeighbour == node) { isBidirectional = true; break; } }

                RCNumVector neighbourCenter = (neighbour.Polygon.Center + this.offset) * new RCNumVector(this.cellSize, this.cellSize);
                this.graphicContext.DrawLine(isBidirectional ? Pens.Blue : Pens.Yellow, nodeCenter.Round().X, nodeCenter.Round().Y, neighbourCenter.Round().X, neighbourCenter.Round().Y);
            }
        }

        /// <summary>
        /// Draws the given polygon.
        /// </summary>
        /// <param name="polygon">The polygon to be drawn.</param>
        private void DrawPolygon(RCPolygon polygon)
        {
            RCNumVector prevPoint = RCNumVector.Undefined;
            for (int i = 0; i < polygon.VertexCount; i++)
            {
                RCNumVector currPoint = (polygon[i] + new RCNumVector((RCNumber)1 / (RCNumber)2, (RCNumber)1 / (RCNumber)2) + this.offset) * new RCNumVector(this.cellSize, this.cellSize);
                if (prevPoint != RCNumVector.Undefined)
                {
                    this.graphicContext.DrawLine(Pens.Red, prevPoint.Round().X, prevPoint.Round().Y, currPoint.Round().X, currPoint.Round().Y);
                }
                prevPoint = currPoint;
            }

            RCNumVector lastPoint = (polygon[polygon.VertexCount - 1] + new RCNumVector((RCNumber)1 / (RCNumber)2, (RCNumber)1 / (RCNumber)2) + this.offset) * new RCNumVector(this.cellSize, this.cellSize);
            RCNumVector firstPoint = (polygon[0] + new RCNumVector((RCNumber)1 / (RCNumber)2, (RCNumber)1 / (RCNumber)2) + this.offset) * new RCNumVector(this.cellSize, this.cellSize);
            this.graphicContext.DrawLine(Pens.Red, lastPoint.Round().X, lastPoint.Round().Y, firstPoint.Round().X, firstPoint.Round().Y);
        }

        /// <summary>
        /// Reference to the output image.
        /// </summary>
        private Bitmap outputImage;

        /// <summary>
        /// Reference to the graphic context.
        /// </summary>
        private Graphics graphicContext;

        /// <summary>
        /// The size of 1 cell on the result image.
        /// </summary>
        private readonly int cellSize;

        /// <summary>
        /// The offset of the top-left cell on the result image.
        /// </summary>
        private readonly RCNumVector offset;
    }
}
