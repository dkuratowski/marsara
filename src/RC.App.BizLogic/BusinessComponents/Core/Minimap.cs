using System;
using RC.Common;
using RC.Engine.Simulator.Engine;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// This class is responsible for minimap calculations.
    /// </summary>
    class Minimap : IMinimap, IDisposable
    {
        /// <summary>
        /// Constructs a Minimap instance.
        /// </summary>
        /// <param name="targetScenario">Reference to the target scenario.</param>
        /// <param name="fullWindow">Reference to the full window.</param>
        /// <param name="attachedWindow">Reference to the currently attached window.</param>
        /// <param name="minimapControlPixelSize">The size of the minimap control in pixels.</param>
        public Minimap(Scenario targetScenario, IMapWindow fullWindow, IMapWindow attachedWindow, RCIntVector minimapControlPixelSize)
        {
            if (fullWindow == null) { throw new ArgumentNullException("fullWindow"); }
            if (attachedWindow == null) { throw new ArgumentNullException("attachedWindow"); }
            if (minimapControlPixelSize == RCIntVector.Undefined) { throw new ArgumentNullException("minimapControlPixelSize"); }

            this.isDisposed = false;
            this.fullWindow = fullWindow;
            this.attachedWindow = attachedWindow;

            this.windowIndicatorCache = new CachedValue<RCIntRectangle>(this.CalculateWindowIndicator);
            this.windowIndicatorSizeCache = new CachedValue<RCIntVector>(this.CalculateWindowIndicatorSize);

            if (!Minimap.TryAlignMinimapHorizontally(minimapControlPixelSize, this.fullWindow.CellWindow.Size, out this.minimapPosition, out this.mapToMinimapTransformation) &&
                !Minimap.TryAlignMinimapVertically(minimapControlPixelSize, this.fullWindow.CellWindow.Size, out this.minimapPosition, out this.mapToMinimapTransformation))
            {
                throw new InvalidOperationException("Unable to align the minimap inside the minimap control!");
            }

            this.pixelMatrix = new MinimapPixel[this.minimapPosition.Width, this.minimapPosition.Height];
            this.quadTileMatrix = new MinimapPixel[targetScenario.Map.Size.X, targetScenario.Map.Size.Y];
            for (int row = 0; row < this.minimapPosition.Width; row++)
            {
                for (int col = 0; col < this.minimapPosition.Height; col++)
                {
                    this.pixelMatrix[col, row] = new MinimapPixel(targetScenario, new RCIntVector(col, row), this.mapToMinimapTransformation);
                    this.AddPixelToQuadTileMatrix(this.pixelMatrix[col, row]);
                }
            }
        }

        #region IDisposable members

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            this.isDisposed = true;
            for (int row = 0; row < this.minimapPosition.Width; row++)
            {
                for (int col = 0; col < this.minimapPosition.Height; col++)
                {
                    this.pixelMatrix[col, row].Dispose();
                }
            }
        }

        #endregion IDisposable members

        #region IMinimap members

        /// <see cref="IMinimap.GetMinimapPixel"/>
        public IMinimapPixel GetMinimapPixel(RCIntVector minimapPixel)
        {
            if (this.isDisposed) { throw new ObjectDisposedException("IMinimap"); }
            if (minimapPixel == RCIntVector.Undefined) { throw new ArgumentNullException("minimapPixel"); }

            return this.pixelMatrix[minimapPixel.X, minimapPixel.Y];
        }

        /// <see cref="IMinimap.GetMinimapPixel"/>
        public IMinimapPixel GetMinimapPixelAtQuadTile(RCIntVector quadTile)
        {
            if (this.isDisposed) { throw new ObjectDisposedException("IMinimap"); }
            if (quadTile == RCIntVector.Undefined) { throw new ArgumentNullException("quadTile"); }

            return this.quadTileMatrix[quadTile.X, quadTile.Y];
        }

        /// <see cref="IMinimap.WindowIndicator"/>
        public RCIntRectangle WindowIndicator
        {
            get
            {
                if (this.isDisposed) { throw new ObjectDisposedException("IMinimap"); }
                return this.windowIndicatorCache.Value;
            }
        }

        /// <see cref="IMinimap.MinimapPosition"/>
        public RCIntRectangle MinimapPosition
        {
            get
            {
                if (this.isDisposed) { throw new ObjectDisposedException("IMinimap"); }
                return this.minimapPosition;
            }
        }

        #endregion IMinimap members

        #region Internal public methods

        /// <summary>
        /// Invalidates the cached data of this minimap.
        /// </summary>
        internal void Invalidate() { this.windowIndicatorCache.Invalidate(); }

        #endregion Internal public methods

        #region Private methods

        /// <summary>
        /// Calculates the window indicator.
        /// </summary>
        /// <returns>The calculated window indicator.</returns>
        private RCIntRectangle CalculateWindowIndicator()
        {
            bool alignToLeft = this.attachedWindow.WindowMapCoords.Left == this.fullWindow.WindowMapCoords.Left;
            bool alignToRight = this.attachedWindow.WindowMapCoords.Right == this.fullWindow.WindowMapCoords.Right;
            bool alignToTop = this.attachedWindow.WindowMapCoords.Top == this.fullWindow.WindowMapCoords.Top;
            bool alignToBottom = this.attachedWindow.WindowMapCoords.Bottom == this.fullWindow.WindowMapCoords.Bottom;

            RCIntVector size = this.windowIndicatorSizeCache.Value;
            RCIntVector topLeftCorner = this.mapToMinimapTransformation.TransformAB(this.attachedWindow.WindowMapCoords.Location).Round();
            int topLeftCornerX = alignToLeft ? 0 : (alignToRight ? this.minimapPosition.Size.X - size.X : topLeftCorner.X);
            int topLeftCornerY = alignToTop ? 0 : (alignToBottom ? this.minimapPosition.Size.Y - size.Y : topLeftCorner.Y);
            RCIntRectangle indicatorRect = new RCIntRectangle(topLeftCornerX, topLeftCornerY, size.X, size.Y) + this.minimapPosition.Location;

            return indicatorRect;
        }

        /// <summary>
        /// Calculates the size of the window indicator.
        /// </summary>
        /// <returns>The calculated window indicator size.</returns>
        private RCIntVector CalculateWindowIndicatorSize()
        {
            RCIntVector topLeftCorner = this.mapToMinimapTransformation.TransformAB(this.attachedWindow.WindowMapCoords.Location).Round();
            RCIntVector bottomRightCorner = this.mapToMinimapTransformation.TransformAB(this.attachedWindow.WindowMapCoords.Location + this.attachedWindow.WindowMapCoords.Size).Round();
            return bottomRightCorner - topLeftCorner;
        }

        /// <summary>
        /// Tries to align the minimap horizontally.
        /// </summary>
        /// <param name="minimapControlPixelSize">The size of the minimap control in pixels.</param>
        /// <param name="mapCellSize">The size of the map in cells.</param>
        /// <param name="minimapPosition">The position of the minimap on the minimap control in pixels.</param>
        /// <param name="transformation">The transformation between the map (A) and minimap (B) coordinate-systems.</param>
        /// <returns>True if the alignment was successfully; otherwise false.</returns>
        private static bool TryAlignMinimapHorizontally(
            RCIntVector minimapControlPixelSize,
            RCIntVector mapCellSize,
            out RCIntRectangle minimapPosition,
            out RCCoordTransformation transformation)
        {
            RCNumber horzAlignedMinimapHeight = (RCNumber)(minimapControlPixelSize.X * mapCellSize.Y) / (RCNumber)mapCellSize.X;
            if (horzAlignedMinimapHeight > minimapControlPixelSize.Y)
            {
                /// Cannot align horizontally.
                minimapPosition = RCIntRectangle.Undefined;
                transformation = null;
                return false;
            }

            /// Align horizontally
            int minimapPixelHeight = horzAlignedMinimapHeight > (int)horzAlignedMinimapHeight
                                   ? (int)horzAlignedMinimapHeight + 1
                                   : (int)horzAlignedMinimapHeight;
            minimapPosition = new RCIntRectangle(0, (minimapControlPixelSize.Y - minimapPixelHeight) / 2, minimapControlPixelSize.X, minimapPixelHeight);

            /// Create the coordinate transformation
            RCNumVector pixelSizeOnMap = new RCNumVector((RCNumber)mapCellSize.X / (RCNumber)minimapControlPixelSize.X,
                                                         (RCNumber)mapCellSize.X / (RCNumber)minimapControlPixelSize.X);
            RCNumVector nullVectorOfMinimap = (pixelSizeOnMap / 2) - (new RCNumVector(1, 1) / 2);
            RCNumVector baseVectorOfMinimapX = new RCNumVector(pixelSizeOnMap.X, 0);
            RCNumVector baseVectorOfMinimapY = new RCNumVector(0, pixelSizeOnMap.Y);
            transformation = new RCCoordTransformation(nullVectorOfMinimap, baseVectorOfMinimapX, baseVectorOfMinimapY);

            return true;
        }

        /// <summary>
        /// Tries to align the minimap vertically.
        /// </summary>
        /// <param name="minimapControlPixelSize">The size of the minimap control in pixels.</param>
        /// <param name="mapCellSize">The size of the map in cells.</param>
        /// <param name="minimapPosition">The position of the minimap on the minimap control in pixels.</param>
        /// <param name="transformation">The transformation between the map (A) and minimap (B) coordinate-systems.</param>
        /// <returns>True if the alignment was successfully; otherwise false.</returns>
        private static bool TryAlignMinimapVertically(
            RCIntVector minimapControlPixelSize,
            RCIntVector mapCellSize,
            out RCIntRectangle minimapPosition,
            out RCCoordTransformation transformation)
        {
            RCNumber vertAlignedMinimapWidth = (RCNumber)(minimapControlPixelSize.Y * mapCellSize.X) / (RCNumber)mapCellSize.Y;
            if (vertAlignedMinimapWidth > minimapControlPixelSize.X)
            {
                /// Cannot align vertically.
                minimapPosition = RCIntRectangle.Undefined;
                transformation = null;
                return false;
            }

            /// Align vertically
            int minimapPixelWidth = vertAlignedMinimapWidth > (int)vertAlignedMinimapWidth
                                  ? (int)vertAlignedMinimapWidth + 1
                                  : (int)vertAlignedMinimapWidth;
            minimapPosition = new RCIntRectangle((minimapControlPixelSize.X - minimapPixelWidth) / 2, 0, minimapPixelWidth, minimapControlPixelSize.Y);

            /// Create the coordinate transformation
            RCNumVector pixelSizeOnMap = new RCNumVector((RCNumber)mapCellSize.Y / (RCNumber)minimapControlPixelSize.Y,
                                                         (RCNumber)mapCellSize.Y / (RCNumber)minimapControlPixelSize.Y);
            RCNumVector nullVectorOfMinimap = (pixelSizeOnMap / 2) - (new RCNumVector(1, 1) / 2);
            RCNumVector baseVectorOfMinimapX = new RCNumVector(pixelSizeOnMap.X, 0);
            RCNumVector baseVectorOfMinimapY = new RCNumVector(0, pixelSizeOnMap.Y);
            transformation = new RCCoordTransformation(nullVectorOfMinimap, baseVectorOfMinimapX, baseVectorOfMinimapY);

            return true;
        }

        /// <summary>
        /// Adds the given minimap pixel to the quadratic tile matrix.
        /// </summary>
        /// <param name="pixel">The minimap pixel to add.</param>
        private void AddPixelToQuadTileMatrix(MinimapPixel pixel)
        {
            for (int quadY = pixel.CoveredQuadTiles.Top; quadY < pixel.CoveredQuadTiles.Bottom; quadY++)
            {
                for (int quadX = pixel.CoveredQuadTiles.Left; quadX < pixel.CoveredQuadTiles.Right; quadX++)
                {
                    if (this.quadTileMatrix[quadX, quadY] != null) { throw new InvalidOperationException("Quadratic tile covered by multiple minimap pixels!"); }
                    this.quadTileMatrix[quadX, quadY] = pixel;
                }
            }
        }

        #endregion Private methods

        /// <summary>
        /// This flag indicates whether this minimap has already been disposed or not.
        /// </summary>
        private bool isDisposed;

        /// <summary>
        /// Reference to the currently attached window.
        /// </summary>
        private readonly IMapWindow attachedWindow;

        /// <summary>
        /// Reference to the current full window.
        /// </summary>
        private readonly IMapWindow fullWindow;

        /// <summary>
        /// The position of the minimap inside the minimap control.
        /// </summary>
        private readonly RCIntRectangle minimapPosition;

        /// <summary>
        /// Cached window indicator.
        /// </summary>
        private CachedValue<RCIntRectangle> windowIndicatorCache;

        /// <summary>
        /// Cached window indicator size.
        /// </summary>
        private CachedValue<RCIntVector> windowIndicatorSizeCache;

        /// <summary>
        /// The 2D array of the minimap pixels.
        /// </summary>
        private readonly MinimapPixel[,] pixelMatrix;

        /// <summary>
        /// The 2D array that contains the covering minimap pixel for each quadratic tiles.
        /// </summary>
        private readonly MinimapPixel[,] quadTileMatrix;

        /// <summary>
        /// Coordinate transformation between the map (A) and minimap (B) coordinate-systems.
        /// </summary>
        private readonly RCCoordTransformation mapToMinimapTransformation;
    }
}
