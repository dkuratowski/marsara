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

        /// <see cref="IMapObjectView.GetVisibleMapObjects"/>
        public List<MapObjectInstance> GetVisibleMapObjects(RCIntRectangle displayedArea)
        {
            /// TODO: this is a dummy implementation
            List<MapObjectInstance> retList = new List<MapObjectInstance>();
            retList.Add(new MapObjectInstance()
            {
                SelectionIndicator = new RCIntRectangle(30, 42, 7, 6),
                SelectionIndicatorColorIdx = 0,
                Sprite = new MapSpriteInstance()
                {
                    DisplayCoords = new RCIntVector(30, 34),
                    Index = 0,
                    Section = new RCIntRectangle(5, 2, 7, 13)
                },
                Values = new List<Tuple<int,RCNumber>>()
                {
                    new Tuple<int, RCNumber>(1, (RCNumber)30/(RCNumber)100),
                    new Tuple<int, RCNumber>(3, (RCNumber)70/(RCNumber)100)
                }
            });
            retList.Add(new MapObjectInstance()
            {
                SelectionIndicator = new RCIntRectangle(32, 47, 7, 6),
                SelectionIndicatorColorIdx = 1,
                Sprite = new MapSpriteInstance()
                {
                    DisplayCoords = new RCIntVector(32, 39),
                    Index = 0,
                    Section = new RCIntRectangle(5, 2, 7, 13)
                },
                Values = new List<Tuple<int, RCNumber>>()
                {
                    new Tuple<int, RCNumber>(1, (RCNumber)60/(RCNumber)100),
                    new Tuple<int, RCNumber>(3, (RCNumber)20/(RCNumber)100)
                }
            });
            return retList;
        }

        #endregion IMapObjectView methods
    }
}
