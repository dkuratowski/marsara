using RC.App.BizLogic.PublicInterfaces;
using RC.Common;
using RC.Common.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.PresLogic.Controls
{
    /// <summary>
    /// Special map object display that sends the generated mouse events to the BE for further processing.
    /// </summary>
    public class RCMapObjectGameplayDisplay : RCMapObjectDisplay
    {
        /// <summary>
        /// Constructs an RCMapObjectGameplayDisplay extension for the given map display control.
        /// </summary>
        /// <param name="extendedControl">The map display control to extend.</param>
        /// <param name="mapObjectView">Reference to a map object view.</param>
        /// <param name="mapObjectControlView">Reference to a map object control view.</param>
        /// <param name="metadataView">Reference to a metadata view.</param>
        public RCMapObjectGameplayDisplay(RCMapDisplay extendedControl, IMapObjectView mapObjectView, IMapObjectControlView mapObjectControlView, IMetadataView metadataView)
            : base(extendedControl, mapObjectView, metadataView)
        {
            if (mapObjectControlView == null) { throw new ArgumentNullException("mapObjectControlView"); }
            this.mapObjectControlView = mapObjectControlView;
        }

        #region Overrides

        /// <see cref="RCMapObjectDisplay.OnRightClick"/>
        protected override void OnRightClick(RCIntVector position)
        {
            TraceManager.WriteAllTrace(string.Format("RIGHT_CLICK {0}", position), PresLogicTraceFilters.INFO);
            this.mapObjectControlView.RightClick(this.DisplayedArea, position);
        }

        /// <see cref="RCMapObjectDisplay.OnLeftClick"/>
        protected override void OnLeftClick(RCIntVector position)
        {
            TraceManager.WriteAllTrace(string.Format("LEFT_CLICK {0}", position), PresLogicTraceFilters.INFO);
            this.mapObjectControlView.LeftClick(this.DisplayedArea, position);
        }

        /// <see cref="RCMapObjectDisplay.OnDoubleClick"/>
        protected override void OnDoubleClick(RCIntVector position)
        {
            TraceManager.WriteAllTrace(string.Format("DOUBLE_CLICK {0}", position), PresLogicTraceFilters.INFO);
            this.mapObjectControlView.DoubleClick(this.DisplayedArea, position);
        }

        /// <see cref="RCMapObjectDisplay.OnSelectionBox"/>
        protected override void OnSelectionBox(RCIntRectangle selectionBox)
        {
            TraceManager.WriteAllTrace(string.Format("SELECTION {0}", selectionBox), PresLogicTraceFilters.INFO);
            this.mapObjectControlView.SelectionBox(this.DisplayedArea, selectionBox);
        }

        #endregion Overrides

        /// <summary>
        /// Reference to the view that handles the mouse events on the map display.
        /// </summary>
        private IMapObjectControlView mapObjectControlView;
    }
}
