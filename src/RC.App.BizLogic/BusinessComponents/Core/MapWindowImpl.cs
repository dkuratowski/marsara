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
        public MapWindowBase(Scenario targetScenario)
        {
            this.isDisposed = false;
            this.targetScenario = targetScenario;

            this.windowCache = new CachedValue<RCNumRectangle>(this.CalculateWindow);
            this.cellWindowCache = new CachedValue<RCIntRectangle>(this.CalculateCellWindow);
            this.quadTileWindowCache = new CachedValue<RCIntRectangle>(this.CalculateQuadTileWindow);
            this.windowOffsetCache = new CachedValue<RCIntVector>(this.CalculateWindowOffset);
            this.pixelWindowCache = new CachedValue<RCIntRectangle>(this.CalculatePixelWindow);

            RCNumVector nullVectorOfPixelGrid = new RCNumVector(
                (RCNumber)1 / 2 - (RCNumber)1 / (2 * MapWindowBase.PIXEL_PER_NAVCELL),
                (RCNumber)1 / 2 - (RCNumber)1 / (2 * MapWindowBase.PIXEL_PER_NAVCELL));
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

            return this.mapToPixelGridTransformation.TransformBA(this.PixelWindow.Location + windowCoords);
        }

        /// <see cref="IMapWindow.WindowToMapRect"/>
        public RCNumRectangle WindowToMapRect(RCIntRectangle windowRect)
        {
            if (windowRect == RCIntRectangle.Undefined) { throw new ArgumentNullException("windowRect"); }
            if (this.isDisposed) { throw new ObjectDisposedException("IMapWindow"); }

            RCNumRectangle windowRectPixelCoords = new RCNumRectangle(this.PixelWindow.Location + windowRect.Location - MapWindowBase.HALF_VECTOR, windowRect.Size);
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

            RCIntVector topLeftCornerWindowCoords = this.mapToPixelGridTransformation.TransformAB(mapRect.Location).Round() - this.PixelWindow.Location;
            RCIntVector bottomRightCornerWindowCoords = this.mapToPixelGridTransformation.TransformAB(mapRect.Location + mapRect.Size).Round() - this.PixelWindow.Location;

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
                return this.windowCache.Value;
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

        /// <see cref="IMapWindow.WindowOffset"/>
        public RCIntVector WindowOffset
        {
            get
            {
                if (this.isDisposed) { throw new ObjectDisposedException("IMapWindow"); }
                return this.windowOffsetCache.Value;
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
        /// Calculates the window.
        /// </summary>
        /// <returns>The calculated window.</returns>
        private RCNumRectangle CalculateWindow()
        {
            return new RCNumRectangle(this.cellWindowCache.Value.Location - MapWindowBase.HALF_VECTOR, this.cellWindowCache.Value.Size);
        }

        /// <summary>
        /// Calculates the cell window.
        /// </summary>
        /// <returns>The calculated cell window.</returns>
        private RCIntRectangle CalculateCellWindow()
        {
            return new RCIntRectangle(this.PixelWindow.X / MapWindowBase.PIXEL_PER_NAVCELL,
                                      this.PixelWindow.Y / MapWindowBase.PIXEL_PER_NAVCELL,
                                      (this.PixelWindow.Right - 1) / MapWindowBase.PIXEL_PER_NAVCELL - this.PixelWindow.X / MapWindowBase.PIXEL_PER_NAVCELL + 1,
                                      (this.PixelWindow.Bottom - 1) / MapWindowBase.PIXEL_PER_NAVCELL - this.PixelWindow.Y / MapWindowBase.PIXEL_PER_NAVCELL + 1);
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
            return new RCIntVector(this.PixelWindow.X % MapWindowBase.PIXEL_PER_NAVCELL, this.PixelWindow.Y % MapWindowBase.PIXEL_PER_NAVCELL);
        }

        /// <summary>
        /// The internal implementation of MapWindowBase.CellToWindowRect
        /// </summary>
        private RCIntRectangle CellToWindowRectImpl(RCIntRectangle cellRect)
        {
            return (cellRect - this.CellWindow.Location) *
                   new RCIntVector(MapWindowBase.PIXEL_PER_NAVCELL, MapWindowBase.PIXEL_PER_NAVCELL) - this.WindowOffset;
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
        /// Cached window.
        /// </summary>
        private CachedValue<RCNumRectangle> windowCache;

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
        /// <param name="windowCenterMapCoords">The coordinates of the center of this map window in map coordinates.</param>
        /// <param name="windowPixelSize">The size of this window in pixels.</param>
        public PartialMapWindow(Scenario targetScenario, RCNumVector windowCenterMapCoords, RCIntVector windowPixelSize)
            : base(targetScenario)
        {
            if (windowCenterMapCoords == RCNumVector.Undefined) { throw new ArgumentNullException("windowCenterMapCoords"); }
            if (windowPixelSize == RCIntVector.Undefined) { throw new ArgumentNullException("windowPixelSize"); }

            this.windowCenterMapCoords = windowCenterMapCoords;
            this.windowPixelSize = windowPixelSize;
        }

        /// <see cref="MapWindowBase.CalculatePixelWindow"/>
        protected override RCIntRectangle CalculatePixelWindow()
        {
            RCIntVector windowCenterPixelCoords = this.MapToPixelGridTransformation.TransformAB(windowCenterMapCoords).Round();
            RCIntVector windowTopLeftPixelCoords = new RCIntVector(
                Math.Max(0, windowCenterPixelCoords.X - windowPixelSize.X / 2),
                Math.Max(0, windowCenterPixelCoords.Y - windowPixelSize.Y / 2));
            RCIntRectangle pixelWindow = new RCIntRectangle(windowTopLeftPixelCoords, windowPixelSize);
            pixelWindow.Intersect(this.FullPixelGrid);

            return pixelWindow;
        }

        /// <summary>
        /// The coordinates of the center of this map window in map coordinates.
        /// </summary>
        private RCNumVector windowCenterMapCoords;

        /// <summary>
        /// The size of this window in pixels.
        /// </summary>
        private RCIntVector windowPixelSize;
    }
}
