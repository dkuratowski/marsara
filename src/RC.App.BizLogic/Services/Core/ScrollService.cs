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

        /// <see cref="IScrollService.Scroll"/>
        public void Scroll(ScrollDirectionEnum direction)
        {
            /// TODO: implement!
        }

        #endregion IScrollService members

        /// <summary>
        /// Reference to the map window business component.
        /// </summary>
        private IMapWindowBC mapWindowBC;
    }
}
