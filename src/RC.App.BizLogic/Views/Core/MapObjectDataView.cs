using RC.Common;
using RC.Engine.Simulator.Scenarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.Views.Core
{
    /// <summary>
    /// Implementation of views on data of a map object.
    /// </summary>
    class MapObjectDataView : MapViewBase, IMapObjectDataView
    {
        /// <summary>
        /// Constructs a map object data view.
        /// </summary>
        /// <param name="entityID">The ID of the entity being read by this view.</param>
        public MapObjectDataView(int entityID)
        {
            if (entityID < 0) { throw new ArgumentNullException("entityID"); }
            this.entityID = entityID;
        }

        #region IMapObjectDataView members

        /// <see cref="IMapObjectDataView.ObjectID"/>
        public int ObjectID { get { return this.entityID; } }

        /// <see cref="IMapObjectDataView.VespeneGasAmount"/>
        public int VespeneGasAmount
        {
            get
            {
                MineralField entityAsMineralField = this.Scenario.GetEntity<MineralField>(this.entityID);
                return entityAsMineralField != null ? entityAsMineralField.ResourceAmount.Read() : -1;
            }
        }

        /// <see cref="IMapObjectDataView.MineralsAmount"/>
        public int MineralsAmount
        {
            get
            {
                VespeneGeyser entityAsVespeneGeyser = this.Scenario.GetEntity<VespeneGeyser>(this.entityID);
                return entityAsVespeneGeyser != null ? entityAsVespeneGeyser.ResourceAmount.Read() : -1;
            }
        }

        #endregion IMapObjectDataView members

        /// <summary>
        /// The ID of the entity being read by this view.
        /// </summary>
        private int entityID;
    }
}
