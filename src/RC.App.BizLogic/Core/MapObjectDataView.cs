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
        public MapObjectDataView(Scenario scenario)
            : base(scenario.Map)
        {
            if (scenario == null) { throw new ArgumentNullException("scenario"); }
            this.scenario = scenario;
        }

        #region IMapObjectDataView members

        /// <see cref="IMapObjectDataView.StartReadingMapObject"/>
        public void StartReadingMapObject(int objectID)
        {
            if (objectID < 0) { throw new ArgumentOutOfRangeException("objectID"); }

            this.entityBeingRead = this.scenario.GetEntity<Entity>(objectID);
        }

        /// <see cref="IMapObjectDataView.StopReadingMapObject"/>
        public void StopReadingMapObject() { this.entityBeingRead = null; }

        /// <see cref="IMapObjectDataView.ObjectID"/>
        public int ObjectID { get { return this.entityBeingRead != null ? this.entityBeingRead.ID.Read() : -1; } }

        /// <see cref="IMapObjectDataView.VespeneGasAmount"/>
        public int VespeneGasAmount
        {
            get
            {
                if (this.entityBeingRead == null) { throw new InvalidOperationException("There is no map object currently being read by the view!"); }
                MineralField entityAsMineralField = this.entityBeingRead as MineralField;
                return entityAsMineralField != null ? entityAsMineralField.ResourceAmount.Read() : -1;
            }
        }

        /// <see cref="IMapObjectDataView.MineralsAmount"/>
        public int MineralsAmount
        {
            get
            {
                if (this.entityBeingRead == null) { throw new InvalidOperationException("There is no map object currently being read by the view!"); }
                VespeneGeyser entityAsVespeneGeyser = this.entityBeingRead as VespeneGeyser;
                return entityAsVespeneGeyser != null ? entityAsVespeneGeyser.ResourceAmount.Read() : -1;
            }
        }

        #endregion IMapObjectDataView members

        /// <summary>
        /// Reference to the scenario whose entities is being read by this view.
        /// </summary>
        private Scenario scenario;

        /// <summary>
        /// Reference to the entity being read by this view or null if there is no entity being read by this view.
        /// </summary>
        private Entity entityBeingRead;
    }
}
