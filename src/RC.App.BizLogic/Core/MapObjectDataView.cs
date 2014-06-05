using RC.App.BizLogic.PublicInterfaces;
using RC.Common;
using RC.Engine.Simulator.Scenarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.Core
{
    /// <summary>
    /// Implementation of views on data of a map object.
    /// </summary>
    class MapObjectDataView : MapViewBase, IMapObjectDataView
    {
        /// <summary>
        /// Constructs a map object data view.
        /// </summary>
        /// <param name="scenario">Reference to the scenario whose entities is being read by this view.</param>
        public MapObjectDataView(Entity entity)
            : base(entity.Scenario.Map)
        {
            if (entity == null) { throw new ArgumentNullException("entity"); }
            this.entity = entity;
        }

        #region IMapObjectDataView members

        /// <see cref="IMapObjectDataView.MapObjectID"/>
        public int ObjectID { get { return this.entity.ID.Read(); } }

        /// <see cref="IMapObjectDataView.VespeneGasAmount"/>
        public int VespeneGasAmount
        {
            get
            {
                MineralField entityAsMineralField = this.entity as MineralField;
                return entityAsMineralField != null ? entityAsMineralField.ResourceAmount.Read() : -1;
            }
        }

        /// <see cref="IMapObjectDataView.MineralsAmount"/>
        public int MineralsAmount
        {
            get
            {
                VespeneGeyser entityAsVespeneGeyser = this.entity as VespeneGeyser;
                return entityAsVespeneGeyser != null ? entityAsVespeneGeyser.ResourceAmount.Read() : -1;
            }
        }

        #endregion IMapObjectDataView members

        /// <summary>
        /// Reference to the entity being read by this view or null if there is no entity being read by this view.
        /// </summary>
        private Entity entity;
    }
}
