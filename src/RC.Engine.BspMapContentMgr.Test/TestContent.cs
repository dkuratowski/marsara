using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.BspMapContentMgr.Test
{
    class TestContent : ISearchTreeContent
    {
        public TestContent(RCNumRectangle initialPos)
        {
            this.currentPosition = initialPos;
        }

        #region ISearchTreeContent Members

        public RCNumRectangle BoundingBox
        {
            get { return this.currentPosition; }
            set
            {
                if (this.currentPosition != value)
                {
                    if (this.BoundingBoxChanging != null) { this.BoundingBoxChanging(this); }
                    this.currentPosition = value;
                    if (this.BoundingBoxChanged != null) { this.BoundingBoxChanged(this); }
                }
            }
        }

        public event ContentBoundingBoxChangeHdl BoundingBoxChanging;

        public event ContentBoundingBoxChangeHdl BoundingBoxChanged;

        #endregion

        private RCNumRectangle currentPosition;
    }
}
