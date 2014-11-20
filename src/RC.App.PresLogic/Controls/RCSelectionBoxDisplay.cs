using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.Services;
using RC.App.BizLogic.Views;
using RC.Common;
using RC.Common.ComponentModel;
using RC.UI;

namespace RC.App.PresLogic.Controls
{
    /// <summary>
    /// Adds the following new functionalities to the map display control:
    ///     - displays the selection box if a selection is currently in progress
    /// </summary>
    class RCSelectionBoxDisplay : RCMapDisplayExtension
    {
        /// <summary>
        /// Constructs an RCSelectionBoxDisplay extension for the given map display control.
        /// </summary>
        /// <param name="extendedControl">The map display control to extend.</param>
        public RCSelectionBoxDisplay(RCMapDisplay extendedControl)
            : base(extendedControl)
        {
            this.mapView = null;
            this.selectionBoxPointer = UIResourceManager.GetResource<UIPointer>("RC.App.Pointers.SelectionBoxPointer");
            this.crosshairsPointer = UIResourceManager.GetResource<UIPointer>("RC.App.Pointers.CrosshairsPointer");
            this.selectionBoxBrush = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(RCColor.LightGreen, new RCIntVector(1, 1), UIWorkspace.Instance.PixelScaling);
            this.selectionBoxBrush.Upload();
        }
        
        #region Overrides

        /// <see cref="RCMapDisplayExtension.GetMousePointer_i"/>
        protected override UIPointer GetMousePointer_i(RCIntVector localPosition)
        {
            if (this.MouseHandler != null)
            {
                if (this.MouseHandler.SelectionBox != RCIntRectangle.Undefined)
                {
                    return this.selectionBoxPointer;
                }
                else if (this.MouseHandler.DisplayCrosshairs)
                {
                    return this.crosshairsPointer;
                }
                /// TODO: display scrolling pointers if scroll is in progress!
            }

            return null;
        }

        /// <see cref="RCMapDisplayExtension.MapView"/>
        protected override IMapView MapView { get { return this.mapView; } }

        /// <see cref="RCMapDisplayExtension.ConnectEx_i"/>
        protected override void ConnectEx_i()
        {
            IViewService viewService = ComponentManager.GetInterface<IViewService>();
            this.mapView = viewService.CreateView<IMapTerrainView>();
        }

        /// <see cref="RCMapDisplayExtension.DisconnectEx_i"/>
        protected override void DisconnectEx_i()
        {
            this.mapView = null;
        }

        /// <see cref="RCMapDisplayExtension.RenderEx_i"/>
        protected override void RenderEx_i(IUIRenderContext renderContext)
        {
            /// Render the selection box if necessary.
            if (this.MouseHandler != null && this.MouseHandler.SelectionBox != RCIntRectangle.Undefined)
            {
                renderContext.RenderRectangle(this.selectionBoxBrush, this.MouseHandler.SelectionBox);
            }
        }

        #endregion Overrides

        /// <summary>
        /// Reference to a map view.
        /// </summary>
        private IMapView mapView;

        /// <summary>
        /// Resources for rendering.
        /// </summary>
        private UIPointer selectionBoxPointer;
        private UIPointer crosshairsPointer;
        private UISprite selectionBoxBrush;
    }
}
