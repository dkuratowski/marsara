using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.PublicInterfaces;
using RC.Engine.Maps.PublicInterfaces;
using RC.Common;

namespace RC.App.BizLogic.Core
{
    /// <summary>
    /// Base class of views on game maps.
    /// </summary>
    abstract class MapViewBase : IMapView
    {
        /// <summary>
        /// Constructs a MapViewBase instance.
        /// </summary>
        /// <param name="map">Reference to the map.</param>
        public MapViewBase(IMapAccess map)
        {
            if (map == null) { throw new ArgumentNullException("map"); }
            this.map = map;
        }

        #region IMapView methods

        /// <see cref="IMapView.MapSize"/>
        public RCIntVector MapSize { get { return this.map.CellSize * new RCIntVector(BizLogicConstants.PIXEL_PER_NAVCELL, BizLogicConstants.PIXEL_PER_NAVCELL); } }

        #endregion IMapView methods

        /// <summary>
        /// Calculates the rectangle of visible cells on the map.
        /// </summary>
        /// <param name="displayedArea">The display area in pixels.</param>
        /// <param name="cellWindow">The calculated cell rectangle.</param>
        /// <param name="displayOffset">
        /// The difference between the top-left corner of the displayed area and the top-left corner of the
        /// top-left visible cell.
        /// </param>
        protected void CalculateCellWindow(RCIntRectangle displayedArea, out RCIntRectangle cellWindow, out RCIntVector displayOffset)
        {
            cellWindow = new RCIntRectangle(displayedArea.X / BizLogicConstants.PIXEL_PER_NAVCELL,
                                            displayedArea.Y / BizLogicConstants.PIXEL_PER_NAVCELL,
                                            (displayedArea.Right - 1) / BizLogicConstants.PIXEL_PER_NAVCELL - displayedArea.X / BizLogicConstants.PIXEL_PER_NAVCELL + 1,
                                            (displayedArea.Bottom - 1) / BizLogicConstants.PIXEL_PER_NAVCELL - displayedArea.Y / BizLogicConstants.PIXEL_PER_NAVCELL + 1);
            displayOffset = new RCIntVector(displayedArea.X % BizLogicConstants.PIXEL_PER_NAVCELL, displayedArea.Y % BizLogicConstants.PIXEL_PER_NAVCELL);
        }

        /// <summary>
        /// Gets the map of this view.
        /// </summary>
        protected IMapAccess Map { get { return this.map; } }

        /// <summary>
        /// Reference to the map.
        /// </summary>
        private IMapAccess map;
    }
}
