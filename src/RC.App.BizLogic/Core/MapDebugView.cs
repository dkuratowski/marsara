using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.PublicInterfaces;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Maps.PublicInterfaces;
using RC.Common;

namespace RC.App.BizLogic.Core
{
    /// <summary>
    /// Implementation of views on informations for debugging on the currently opened map.
    /// </summary>
    class MapDebugView : MapViewBase, IMapDebugView
    {
        /// <summary>
        /// Constructs a MapDebugView instance.
        /// </summary>
        /// <param name="map">The subject of this view.</param>
        /// <param name="pathfinder">Reference to the initialized pathfinder component.</param>
        public MapDebugView(IMapAccess map, IPathFinder pathfinder)
            : base(map)
        {
            if (pathfinder == null) { throw new ArgumentNullException("pathfinder"); }
            this.pathfinder = pathfinder;
        }

        #region IMapDebugView methods

        /// <see cref="IMapDebugView.GetVisiblePathfinderTreeNodes"/>
        public List<RCIntRectangle> GetVisiblePathfinderTreeNodes(RCIntRectangle displayedArea)
        {
            if (displayedArea == RCIntRectangle.Undefined) { throw new ArgumentNullException("displayedArea"); }
            if (!new RCIntRectangle(0, 0, this.MapSize.X, this.MapSize.Y).Contains(displayedArea)) { throw new ArgumentOutOfRangeException("displayedArea"); }

            RCIntRectangle cellWindow;
            RCIntVector displayOffset;
            this.CalculateCellWindow(displayedArea, out cellWindow, out displayOffset);

            List<RCIntRectangle> retList = new List<RCIntRectangle>();
            foreach (RCIntRectangle treeNode in this.pathfinder.GetTreeNodes(cellWindow))
            {
                retList.Add((treeNode - cellWindow.Location) * new RCIntVector(BizLogicConstants.PIXEL_PER_NAVCELL, BizLogicConstants.PIXEL_PER_NAVCELL) - displayOffset);
            }
            return retList;
        }

        #endregion IMapDebugView methods

        /// <summary>
        /// Reference to the initialized pathfinder component.
        /// </summary>
        private IPathFinder pathfinder;
    }
}
