using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.BusinessComponents;
using RC.Common;
using RC.Common.ComponentModel;

namespace RC.App.BizLogic.Services.Core
{
    /// <summary>
    /// The implementation of the sroll service.
    /// </summary>
    [Component("RC.App.BizLogic.ScrollService")]
    class ScrollService : IScrollService, IComponent
    {
        /// <summary>
        /// Constructs a ScrollService instance.
        /// </summary>
        public ScrollService()
        {
        }

        #region IComponent members

        /// <see cref="IComponent.Start"/>
        public void Start()
        {
            this.mapWindowBC = ComponentManager.GetInterface<IMapWindowBC>();
        }

        /// <see cref="IComponent.Stop"/>
        public void Stop()
        {
        }

        #endregion IComponent members

        #region IScrollService members

        /// <see cref="IScrollService.AttachWindow"/>
        public void AttachWindow(RCIntVector windowSize)
        {
            this.mapWindowBC.AttachWindow(windowSize);
        }

        /// <see cref="IScrollService.AttachMinimap"/>
        public void AttachMinimap(RCIntVector minimapSize)
        {
            this.mapWindowBC.AttachMinimap(minimapSize);
        }

        /// <see cref="IScrollService.Scroll"/>
        public void Scroll(ScrollDirectionEnum direction)
        {
            if (this.mapWindowBC.AttachedWindow == null) { throw new InvalidOperationException("Window has not yet been attached!"); }
            if (direction == ScrollDirectionEnum.NoScroll) { return; }

            RCNumVector windowCenterMapCoords = this.mapWindowBC.AttachedWindow.WindowMapCoords.Location
                                              + this.mapWindowBC.AttachedWindow.WindowMapCoords.Size / 2;
            if (direction == ScrollDirectionEnum.North) { this.mapWindowBC.ScrollTo(windowCenterMapCoords + new RCNumVector(0, -SCROLL_STEP)); }
            else if (direction == ScrollDirectionEnum.NorthEast) { this.mapWindowBC.ScrollTo(windowCenterMapCoords + new RCNumVector(SCROLL_STEP, -SCROLL_STEP)); }
            else if (direction == ScrollDirectionEnum.East) { this.mapWindowBC.ScrollTo(windowCenterMapCoords + new RCNumVector(SCROLL_STEP, 0)); }
            else if (direction == ScrollDirectionEnum.SouthEast) { this.mapWindowBC.ScrollTo(windowCenterMapCoords + new RCNumVector(SCROLL_STEP, SCROLL_STEP)); }
            else if (direction == ScrollDirectionEnum.South) { this.mapWindowBC.ScrollTo(windowCenterMapCoords + new RCNumVector(0, SCROLL_STEP)); }
            else if (direction == ScrollDirectionEnum.SouthWest) { this.mapWindowBC.ScrollTo(windowCenterMapCoords + new RCNumVector(-SCROLL_STEP, SCROLL_STEP)); }
            else if (direction == ScrollDirectionEnum.West) { this.mapWindowBC.ScrollTo(windowCenterMapCoords + new RCNumVector(-SCROLL_STEP, 0)); }
            else if (direction == ScrollDirectionEnum.NorthWest) { this.mapWindowBC.ScrollTo(windowCenterMapCoords + new RCNumVector(-SCROLL_STEP, -SCROLL_STEP)); }
        }

        #endregion IScrollService members

        /// <summary>
        /// Reference to the map window business component.
        /// </summary>
        private IMapWindowBC mapWindowBC;

        /// <summary>
        /// This constant defines the step of the window per scrolls.
        /// </summary>
        private static readonly RCNumber SCROLL_STEP = (RCNumber)5 / (RCNumber)2;
    }
}
