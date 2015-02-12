using System;
using RC.Common;
using RC.Engine.Simulator.Scenarios;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// The abstract base class of IMapWindow implementations
    /// </summary>
    abstract class MapWindowBase : IMapWindow, IDisposable
    {
        /// <summary>
        /// Constructs a MapWindowBase instance.
        /// </summary>
        /// <param name="targetScenario">Reference to the target scenario.</param>
        protected MapWindowBase(Scenario targetScenario)
        {
            if (targetScenario == null) { throw new ArgumentNullException("targetScenario"); }

            this.isDisposed = false;
            this.targetScenario = targetScenario;

            this.windowMapCoordsCache = new CachedValue<RCNumRectangle>(this.CalculateWindowMapCoords);
            this.cellWindowCache = new CachedValue<RCIntRectangle>(this.CalculateCellWindow);
            this.quadTileWindowCache = new CachedValue<RCIntRectangle>(this.CalculateQuadTileWindow);
            this.windowOffsetCache = new CachedValue<RCIntVector>(this.CalculateWindowOffset);
            this.pixelWindowCache = new CachedValue<RCIntRectangle>(this.CalculatePixelWindow);

            RCNumVector nullVectorOfPixelGrid = new RCNumVector(
                (RCNumber)1 / (2 * MapWindowBase.PIXEL_PER_NAVCELL) - (RCNumber)1 / 2,
                (RCNumber)1 / (2 * MapWindowBase.PIXEL_PER_NAVCELL) - (RCNumber)1 / 2);
            RCNumVector baseVectorOfPixelGridX = new RCNumVector((RCNumber)1 / MapWindowBase.PIXEL_PER_NAVCELL, 0);
            RCNumVector baseVectorOfPixelGridY = new RCNumVector(0, (RCNumber)1 / MapWindowBase.PIXEL_PER_NAVCELL);
            this.mapToPixelGridTransformation = new RCCoordTransformation(nullVectorOfPixelGrid, baseVectorOfPixelGridX, baseVectorOfPixelGridY);

            this.fullPixelGrid = new RCIntRectangle(new RCIntVector(0, 0), this.targetScenario.Map.CellSize * MapWindowBase.PIXEL_PER_NAVCELL);
        }

        #region IDisposable members

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            this.isDisposed = true;
        }

        #endregion IDisposable members

        #region IMapWindow members

        /// <see cref="IMapWindow.WindowToMapCoords"/>
        public RCNumVector WindowToMapCoords(RCIntVector windowCoords)
        {
            if (windowCoords == RCIntVector.Undefined) { throw new ArgumentNullException("windowCoords"); }
            if (this.isDisposed) { throw new ObjectDisposedException("IMapWindow"); }

            return this.mapToPixelGridTransformation.TransformBA(this.pixelWindowCache.Value.Location + windowCoords);
        }

        /// <see cref="IMapWindow.WindowToMapRect"/>
        public RCNumRectangle WindowToMapRect(RCIntRectangle windowRect)
        {
            if (windowRect == RCIntRectangle.Undefined) { throw new ArgumentNullException("windowRect"); }
            if (this.isDisposed) { throw new ObjectDisposedException("IMapWindow"); }

            RCNumRectangle windowRectPixelCoords = new RCNumRectangle(this.pixelWindowCache.Value.Location + windowRect.Location - MapWindowBase.HALF_VECTOR, windowRect.Size);
            RCNumVector topLeftCornerMapCoords = this.mapToPixelGridTransformation.TransformBA(windowRectPixelCoords.Location);
            RCNumVector bottomRightCornerMapCoords = this.mapToPixelGridTransformation.TransformBA(windowRectPixelCoords.Location + windowRectPixelCoords.Size);

            return new RCNumRectangle(topLeftCornerMapCoords, bottomRightCornerMapCoords - topLeftCornerMapCoords);
        }

        /// <see cref="IMapWindow.MapToWindowCoords"/>
        public RCIntVector MapToWindowCoords(RCNumVector mapCoords)
        {
            if (mapCoords == RCNumVector.Undefined) { throw new ArgumentNullException("mapCoords"); }
            if (this.isDisposed) { throw new ObjectDisposedException("IMapWindow"); }

            return this.mapToPixelGridTransformation.TransformAB(mapCoords).Round() - this.PixelWindow.Location;
        }

        /// <see cref="IMapWindow.MapToWindowRect"/>
        public RCIntRectangle MapToWindowRect(RCNumRectangle mapRect)
        {
            if (mapRect == RCNumRectangle.Undefined) { throw new ArgumentNullException("mapRect"); }
            if (this.isDisposed) { throw new ObjectDisposedException("IMapWindow"); }

            RCIntVector topLeftCornerWindowCoords = this.mapToPixelGridTransformation.TransformAB(mapRect.Location).Round() - this.pixelWindowCache.Value.Location;
            RCIntVector bottomRightCornerWindowCoords = this.mapToPixelGridTransformation.TransformAB(mapRect.Location + mapRect.Size).Round() - this.pixelWindowCache.Value.Location;

            return new RCIntRectangle(topLeftCornerWindowCoords, bottomRightCornerWindowCoords - topLeftCornerWindowCoords + new RCIntVector(1, 1));
        }

        /// <see cref="IMapWindow.QuadToWindowRect"/>
        public RCIntRectangle QuadToWindowRect(RCIntRectangle quadRect)
        {
            if (quadRect == RCIntRectangle.Undefined) { throw new ArgumentNullException("quadRect"); }
            if (this.isDisposed) { throw new ObjectDisposedException("IMapWindow"); }

            return this.CellToWindowRectImpl(this.targetScenario.Map.QuadToCellRect(quadRect));
        }

        /// <see cref="IMapWindow.CellToWindowRect"/>
        public RCIntRectangle CellToWindowRect(RCIntRectangle cellRect)
        {
            if (cellRect == RCIntRectangle.Undefined) { throw new ArgumentNullException("cellRect"); }
            if (this.isDisposed) { throw new ObjectDisposedException("IMapWindow"); }

            return this.CellToWindowRectImpl(cellRect);
        }


        /// <see cref="IMapWindow.WindowMapCoords"/>
        public RCNumRectangle WindowMapCoords
        {
            get
            {
                if (this.isDisposed) { throw new ObjectDisposedException("IMapWindow"); }
                return this.windowMapCoordsCache.Value;
            }
        }

        /// <see cref="IMapWindow.CellWindow"/>
        public RCIntRectangle CellWindow
        {
            get
            {
                if (this.isDisposed) { throw new ObjectDisposedException("IMapWindow"); }
                return this.cellWindowCache.Value;
            }
        }

        /// <see cref="IMapWindow.QuadTileWindow"/>
        public RCIntRectangle QuadTileWindow
        {
            get
            {
                if (this.isDisposed) { throw new ObjectDisposedException("IMapWindow"); }
                return this.quadTileWindowCache.Value;
            }
        }

        /// <see cref="IMapWindow.PixelWindow"/>
        public RCIntRectangle PixelWindow
        {
            get
            {
                if (this.isDisposed) { throw new ObjectDisposedException("IMapWindow"); }
                return this.pixelWindowCache.Value;
            }
        }

        #endregion IMapWindow members

        #region Protected members

        /// <summary>
        /// Invalidates the cached values.
        /// </summary>
        protected void InvalidateCaches()
        {
            this.windowMapCoordsCache.Invalidate();
            this.cellWindowCache.Invalidate();
            this.quadTileWindowCache.Invalidate();
            this.windowOffsetCache.Invalidate();
            this.pixelWindowCache.Invalidate();
        }

        /// <summary>
        /// Gets the full pixel grid.
        /// </summary>
        protected RCIntRectangle FullPixelGrid { get { return this.fullPixelGrid; } }

        /// <summary>
        /// Gets the coordinate transformation between the map (A) and pixel grid (B) coordinate-systems.
        /// </summary>
        protected RCCoordTransformation MapToPixelGridTransformation { get { return this.mapToPixelGridTransformation; } }

        #endregion Protected members

        #region Overridables

        /// <summary>
        /// Calculates the pixel window.
        /// </summary>
        /// <returns>The calculated pixel window.</returns>
        protected abstract RCIntRectangle CalculatePixelWindow();

        #endregion Overridables

        #region Internal calculation methods

        /// <summary>
        /// Calculates the map coordinates of the window.
        /// </summary>
        /// <returns>The calculated map coordinates of the window.</returns>
        private RCNumRectangle CalculateWindowMapCoords()
        {
            RCNumRectangle pixelWindowPixelCoords = new RCNumRectangle(this.pixelWindowCache.Value.Location - MapWindowBase.HALF_VECTOR, this.pixelWindowCache.Value.Size);
            RCNumVector topLeftCornerMapCoords = this.mapToPixelGridTransformation.TransformBA(pixelWindowPixelCoords.Location);
            RCNumVector bottomRightCornerMapCoords = this.mapToPixelGridTransformation.TransformBA(pixelWindowPixelCoords.Location + pixelWindowPixelCoords.Size);

            return new RCNumRectangle(topLeftCornerMapCoords, bottomRightCornerMapCoords - topLeftCornerMapCoords);
        }

        /// <summary>
        /// Calculates the cell window.
        /// </summary>
        /// <returns>The calculated cell window.</returns>
        private RCIntRectangle CalculateCellWindow()
        {
            return new RCIntRectangle(this.pixelWindowCache.Value.X / MapWindowBase.PIXEL_PER_NAVCELL,
                                      this.pixelWindowCache.Value.Y / MapWindowBase.PIXEL_PER_NAVCELL,
                                      (this.pixelWindowCache.Value.Right - 1) / MapWindowBase.PIXEL_PER_NAVCELL - this.pixelWindowCache.Value.X / MapWindowBase.PIXEL_PER_NAVCELL + 1,
                                      (this.pixelWindowCache.Value.Bottom - 1) / MapWindowBase.PIXEL_PER_NAVCELL - this.pixelWindowCache.Value.Y / MapWindowBase.PIXEL_PER_NAVCELL + 1);
        }

        /// <summary>
        /// Calculates the quadratic tile window.
        /// </summary>
        /// <returns>The calculated quadratic tile window.</returns>
        private RCIntRectangle CalculateQuadTileWindow()
        {
            return this.targetScenario.Map.CellToQuadRect(this.cellWindowCache.Value);
        }

        /// <summary>
        /// Calculates the window offset vector.
        /// </summary>
        /// <returns>The calculated window offset vector.</returns>
        private RCIntVector CalculateWindowOffset()
        {
            return new RCIntVector(this.pixelWindowCache.Value.X % MapWindowBase.PIXEL_PER_NAVCELL, this.pixelWindowCache.Value.Y % MapWindowBase.PIXEL_PER_NAVCELL);
        }

        /// <summary>
        /// The internal implementation of MapWindowBase.CellToWindowRect
        /// </summary>
        private RCIntRectangle CellToWindowRectImpl(RCIntRectangle cellRect)
        {
            return (cellRect - this.cellWindowCache.Value.Location) *
                   new RCIntVector(MapWindowBase.PIXEL_PER_NAVCELL, MapWindowBase.PIXEL_PER_NAVCELL) - this.windowOffsetCache.Value;
        }

        #endregion Internal calculation methods

        /// <summary>
        /// This flag indicates whether this map window has already been disposed or not.
        /// </summary>
        private bool isDisposed;

        /// <summary>
        /// Reference to the target scenario.
        /// </summary>
        private readonly Scenario targetScenario;

        /// <summary>
        /// The full pixel grid.
        /// </summary>
        private readonly RCIntRectangle fullPixelGrid;

        /// <summary>
        /// Cached pixel window.
        /// </summary>
        private CachedValue<RCIntRectangle> pixelWindowCache;

        /// <summary>
        /// Cached cell window map coordinates.
        /// </summary>
        private CachedValue<RCNumRectangle> windowMapCoordsCache;

        /// <summary>
        /// Cached cell window.
        /// </summary>
        private CachedValue<RCIntRectangle> cellWindowCache;

        /// <summary>
        /// Cached quadratic tile window.
        /// </summary>
        private CachedValue<RCIntRectangle> quadTileWindowCache;

        /// <summary>
        /// Cached window offset vector.
        /// </summary>
        private CachedValue<RCIntVector> windowOffsetCache;

        /// <summary>
        /// Coordinate transformation between the map (A) and pixel grid (B) coordinate-systems.
        /// </summary>
        private readonly RCCoordTransformation mapToPixelGridTransformation;

        /// <summary>
        /// Constant fields.
        /// </summary>
        private static readonly RCNumVector HALF_VECTOR = new RCNumVector(1, 1) / 2;

        /// <summary>
        /// Number of pixels per cells in both horizontal and vertical direction.
        /// </summary>
        private const int PIXEL_PER_NAVCELL = 4;
    }

    /// <summary>
    /// Represents a full map window.
    /// </summary>
    class FullMapWindow : MapWindowBase
    {
        /// <summary>
        /// Constructs a FullMapWindow instance.
        /// </summary>
        /// <param name="targetScenario">Reference to the target scenario.</param>
        public FullMapWindow(Scenario targetScenario) : base(targetScenario)
        {
        }

        /// <see cref="MapWindowBase.CalculatePixelWindow"/>
        protected override RCIntRectangle CalculatePixelWindow()
        {
            return this.FullPixelGrid;
        }
    }

    /// <summary>
    /// Represents a partial map window.
    /// </summary>
    class PartialMapWindow : MapWindowBase
    {
        /// <summary>
        /// Constructs a PartialMapWindow instance.
        /// </summary>
        /// <param name="targetScenario">Reference to the target scenario.</param>
        /// <param name="desiredWindowCenterMapCoords">The desired coordinates of the center of this map window in map coordinates.</param>
        /// <param name="windowPixelSize">The size of this window in pixels.</param>
        public PartialMapWindow(Scenario targetScenario, RCNumVector desiredWindowCenterMapCoords, RCIntVector windowPixelSize)
            : base(targetScenario)
        {
            if (desiredWindowCenterMapCoords == RCNumVector.Undefined) { throw new ArgumentNullException("desiredWindowCenterMapCoords"); }
            if (windowPixelSize == RCIntVector.Undefined) { throw new ArgumentNullException("windowPixelSize"); }

            this.windowPixelSize = windowPixelSize;
            this.desiredWindowCenterMapCoords = desiredWindowCenterMapCoords;
        }

        /// <summary>
        /// Scrolls the center of this window to the given position on the map.
        /// </summary>
        /// <param name="targetPosition">The coordinates of the target position on the map.</param>
        public void ScrollTo(RCNumVector targetPosition)
        {
            if (targetPosition == RCNumVector.Undefined) { throw new ArgumentNullException("targetPosition"); }
            this.desiredWindowCenterMapCoords = targetPosition;
            this.InvalidateCaches();
        }

        /// <see cref="MapWindowBase.CalculatePixelWindow"/>
        protected override RCIntRectangle CalculatePixelWindow()
        {
            RCIntVector windowCenterPixelCoords = this.MapToPixelGridTransformation.TransformAB(this.desiredWindowCenterMapCoords).Round();
            RCIntVector windowTopLeftPixelCoords = new RCIntVector(
                Math.Min(Math.Max(0, windowCenterPixelCoords.X - this.windowPixelSize.X / 2), this.FullPixelGrid.Right - this.windowPixelSize.X),
                Math.Min(Math.Max(0, windowCenterPixelCoords.Y - this.windowPixelSize.Y / 2), this.FullPixelGrid.Bottom - this.windowPixelSize.Y));
            RCIntRectangle pixelWindow = new RCIntRectangle(windowTopLeftPixelCoords, this.windowPixelSize);

            if (pixelWindow.Width > this.FullPixelGrid.Width || pixelWindow.Height > this.FullPixelGrid.Height) { throw new InvalidOperationException("Pixel window is bigger than the pixel grid of the map!"); }
            return pixelWindow;
        }

        /// <summary>
        /// The size of this window in pixels.
        /// </summary>
        private RCIntVector windowPixelSize;

        /// <summary>
        /// The desired coordinates of the center of this map window in map coordinates.
        /// </summary>
        private RCNumVector desiredWindowCenterMapCoords;
    }
}
