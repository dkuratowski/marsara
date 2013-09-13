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
        /// <param name="mapDebugView">Reference to a map debug view.</param>
        public RCMapDebugDisplay(RCMapDisplay extendedControl, IMapDebugView mapDebugView)
            : base(extendedControl, mapDebugView)
        {
            if (mapDebugView == null) { throw new ArgumentNullException("mapDebugView"); }

            this.mapDebugView = mapDebugView;
            this.brushPalette = new BrushPaletteSpriteGroup();
        }

        #region Overrides

        /// <see cref="RCMapDisplayExtension.StartExtensionProc_i"/>
        protected override void StartExtensionProc_i()
        {
            this.brushPalette.Load();
        }

        /// <see cref="RCMapDisplayExtension.StopExtensionProc_i"/>
        protected override void StopExtensionProc_i()
        {
            this.brushPalette.Unload();
        }

        /// <see cref="RCMapDisplayExtension.RenderExtension_i"/>
        protected override void RenderExtension_i(IUIRenderContext renderContext)
        {
            /// Retrieve the list of the visible pathfinder tree nodes.
            List<RCIntRectangle> pathfinderTreeNodes = this.mapDebugView.GetVisiblePathfinderTreeNodes(this.DisplayedArea);

            /// Render the tree nodes.
            foreach (RCIntRectangle treeNode in pathfinderTreeNodes)
            {
                renderContext.RenderRectangle(this.brushPalette[3], treeNode);
            }
        }

        #endregion Overrides

        /// <summary>
        /// Reference to the map debug view.
        /// </summary>
        private IMapDebugView mapDebugView;

        /// <summary>
        /// The brush palette for drawing the informations coming from the debug view.
        /// </summary>
        private SpriteGroup brushPalette;
    }
}
