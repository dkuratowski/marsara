using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.PublicInterfaces;

namespace RC.Engine.BspMapContentMgr.Test
{
    class TestContent : IMapContent
    {
        public TestContent(RCNumRectangle initialPos)
        {
            this.currentPosition = initialPos;
        }

        #region IMapContent Members

        public RCNumRectangle Position
        {
            get { return this.currentPosition; }
            set
            {
                if (this.currentPosition != value)
                {
                    if (this.PositionChanging != null) { this.PositionChanging(this); }
                    this.currentPosition = value;
                    if (this.PositionChanged != null) { this.PositionChanged(this); }
                }
            }
        }

        public event MapContentPropertyChangeHdl PositionChanging;

        public event MapContentPropertyChangeHdl PositionChanged;

        #endregion

        private RCNumRectangle currentPosition;
    }
}
