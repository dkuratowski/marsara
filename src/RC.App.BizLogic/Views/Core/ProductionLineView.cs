using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.App.BizLogic.Views.Core
{
    /// <summary>
    /// Implementation of views for providing detailed informations about the production line of the currently selected map object.
    /// </summary>
    class ProductionLineView : MapViewBase, IProductionLineView
    {
        /// <summary>
        /// Constructs a production line view.
        /// </summary>
        public ProductionLineView()
        {
        }

        #region IProductionLineView members

        /// <see cref="IProductionLineView.Capacity"/>
        public int Capacity
        {
            get { return 5; }   // TODO: implement!
        }

        /// <see cref="IProductionLineView.ItemCount"/>
        public int ItemCount
        {
            get { return 3; }   // TODO: implement!
        }

        /// <see cref="IProductionLineView.ProgressNormalized"/>
        public RCNumber ProgressNormalized
        {
            get { return (RCNumber)1 / (RCNumber)3; }   // TODO: implement!
        }

        /// <see cref="IProductionLineView.this"/>
        public SpriteInst this[int itemIndex]
        {
            get
            {
                return new SpriteInst
                {
                    DisplayCoords = new RCIntVector(0, 0),
                    Index = 0,
                    Section = new RCIntRectangle(1, 1, 20, 20)
                };   // TODO: implement!
            }
        }

        #endregion IProductionLineView members
    }
}
