using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Scenarios;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// Represents a pixel on the minimap image.
    /// </summary>
    class MinimapPixel : IMinimapPixel, IDisposable
    {
        /// <summary>
        /// Constructs a MinimapPixel instance.
        /// </summary>
        /// <param name="targetScenario">Reference to the target scenario.</param>
        /// <param name="pixelCoords">The coordinates of this pixel on the minimap image.</param>
        /// <param name="mapToMinimapTransformation">Coordinate transformation between the map (A) and minimap (B) coordinate-systems.</param>
        public MinimapPixel(Scenario targetScenario, RCIntVector pixelCoords, RCCoordTransformation mapToMinimapTransformation)
        {
            this.isDisposed = false;
            this.pixelCoords = pixelCoords;

            /// Calculate the area on the map covered by this pixel.
            RCNumVector minimapPixelTopLeft = pixelCoords - (new RCNumVector(1, 1) / 2);
            RCNumVector minimapPixelBottomRight = pixelCoords + (new RCNumVector(1, 1) / 2);
            RCNumVector mapRectTopLeft = mapToMinimapTransformation.TransformBA(minimapPixelTopLeft);
            RCNumVector mapRectBottomRight = mapToMinimapTransformation.TransformBA(minimapPixelBottomRight);
            this.coveredArea = new RCNumRectangle(mapRectTopLeft, mapRectBottomRight - mapRectTopLeft);
            this.coveredArea.Intersect(new RCNumRectangle(new RCNumVector(0, 0), targetScenario.Map.CellSize) - (new RCNumVector(1, 1) / 2));

            /// Find the quadratic tiles whose centers are inside the covered area.
            RCIntVector cellRectTopLeft = this.coveredArea.Location.Round();
            RCIntVector cellRectBottomRight = (this.coveredArea.Location + this.coveredArea.Size).Round();
            RCIntRectangle cellRect = new RCIntRectangle(cellRectTopLeft, cellRectBottomRight - cellRectTopLeft + new RCIntVector(1, 1));
            RCIntRectangle quadRect = targetScenario.Map.CellToQuadRect(cellRect);

            RCIntVector coveredQuadTilesTopLeft = RCIntVector.Undefined;
            RCIntVector coveredQuadTilesBottomRight = RCIntVector.Undefined;
            for (int quadCoordX = quadRect.Left; quadCoordX < quadRect.Right; quadCoordX++)
            {
                for (int quadCoordY = quadRect.Top; quadCoordY < quadRect.Bottom; quadCoordY++)
                {
                    RCNumRectangle quadTileRect = (RCNumRectangle)targetScenario.Map.QuadToCellRect(new RCIntRectangle(quadCoordX, quadCoordY, 1, 1))
                                                - (new RCNumVector(1, 1) / 2);
                    RCNumVector quadTileRectCenter = (quadTileRect.Location + quadTileRect.Location + quadTileRect.Size) / 2;
                    if (this.coveredArea.Contains(quadTileRectCenter))
                    {
                        RCIntVector quadCoords = new RCIntVector(quadCoordX, quadCoordY);
                        if (coveredQuadTilesTopLeft == RCIntVector.Undefined) { coveredQuadTilesTopLeft = quadCoords; }
                        if (coveredQuadTilesBottomRight == RCIntVector.Undefined ||
                            quadCoords.Y > coveredQuadTilesBottomRight.Y ||
                            quadCoords.X > coveredQuadTilesBottomRight.X)
                        {
                            coveredQuadTilesBottomRight = quadCoords;
                        }
                    }
                }
            }
            this.coveredQuadTiles = new RCIntRectangle(coveredQuadTilesTopLeft, coveredQuadTilesBottomRight - coveredQuadTilesTopLeft + new RCIntVector(1, 1));
        }

        #region IDisposable members

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            this.isDisposed = true;
        }

        #endregion IDisposable members

        #region IMinimapPixel members

        /// <see cref="IMinimapPixel.PixelCoords"/>
        public RCIntVector PixelCoords
        {
            get
            {
                if (this.isDisposed) { throw new ObjectDisposedException("IMinimapPixel"); }
                return this.pixelCoords;
            }
        }

        /// <see cref="IMinimapPixel.CoveredQuadTiles"/>
        public RCIntRectangle CoveredQuadTiles
        {
            get
            {
                if (this.isDisposed) { throw new ObjectDisposedException("IMinimapPixel"); }
                return this.coveredQuadTiles;
            }
        }

        /// <see cref="IMinimapPixel.CoveredArea"/>
        public RCNumRectangle CoveredArea
        {
            get
            {
                if (this.isDisposed) { throw new ObjectDisposedException("IMinimapPixel"); }
                return this.coveredArea;
            }
        }

        #endregion IMinimapPixel members

        /// <summary>
        /// This flag indicates whether this minimap pixel has already been disposed or not.
        /// </summary>
        private bool isDisposed;

        /// <summary>
        /// The coordinates of this minimap pixel.
        /// </summary>
        private readonly RCIntVector pixelCoords;

        /// <summary>
        /// The rectangle of the quadratic tiles covered by this pixel.
        /// </summary>
        private readonly RCIntRectangle coveredQuadTiles;

        /// <summary>
        /// The area on the map covered by this pixel.
        /// </summary>
        private RCNumRectangle coveredArea;
    }
}
