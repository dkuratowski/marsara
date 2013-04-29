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
    /// Implementation of views on the objects of the currently opened map.
    /// </summary>
    class MapObjectView : MapViewBase, IMapObjectView
    {
        /// <summary>
        /// Constructs a MapObjectView instance.
        /// </summary>
        /// <param name="map">The subject of this view.</param>
        public MapObjectView(IMapAccess map)
            : base(map)
        {
        }

        #region IMapObjectView methods

        /// <see cref="IMapObjectView.GetVisibleObjects"/>
        public List<MapSpriteInstance> GetVisibleObjects(RCIntRectangle displayedArea)
        {
            throw new NotImplementedException();
        }

        /// <see cref="IMapObjectView.GetVisibleSelectionIndicators"/>
        public List<RCIntRectangle> GetVisibleSelectionIndicators(RCIntRectangle displayedArea)
        {
            throw new NotImplementedException();
        }

        #endregion IMapObjectView methods
    }
}
