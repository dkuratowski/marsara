using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.PublicInterfaces;
using RC.UI;
using RC.Common;

namespace RC.App.PresLogic.Controls
{
    /// <summary>
    /// Adds the following new functionalities to the map display control:
    ///     - displays the pathfinder graph
    /// </summary>
    public class RCMapDebugDisplay : RCMapDisplayExtension
    {
        /// <summary>
        /// Constructs an RCMapDebugDisplay extension for the given map display control.
        /// </summary>
        /// <param name="extendedControl">The map display control to extend.</param>
        /// <param name="mapTerrainView">Reference to a map terrain view.</param>
        public RCMapDebugDisplay(RCMapDisplay extendedControl, IMapTerrainView mapTerrainView)
            : base(extendedControl, mapTerrainView)
        {
            if (mapTerrainView == null) { throw new ArgumentNullException("mapTerrainView"); }

            this.mapTerrainView = mapTerrainView;
        }

        #region Overrides

        /// <see cref="RCMapDisplayExtension.RenderExtension_i"/>
        protected override void RenderExtension_i(IUIRenderContext renderContext)
        {
            /// TODO: implement!
        }

        #endregion Overrides

        /// <summary>
        /// Reference to the map terrain view.
        /// </summary>
        private IMapTerrainView mapTerrainView;
    }
}
