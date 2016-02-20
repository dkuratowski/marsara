using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Behaviors;
using RC.Engine.Simulator.Engine;
using RC.Common;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.Terran.Commands;
using RC.Engine.Simulator.Terran.Units;

namespace RC.Engine.Simulator.Terran.Buildings
{
    /// <summary>
    /// The abstract base class of a Terran building.
    /// </summary>
    abstract class TerranBuilding : Building
    {
        /// <summary>
        /// Gets the construction job attached to this Terran building or null if there is no construction job
        /// attached to this building.
        /// </summary>
        public TerranBuildingConstructionJob ConstructionJob { get { return this.constructionJob.Read(); } }

        /// <see cref="ScenarioElement.AttachToMap"/>
        public override bool AttachToMap(RCNumVector position, params ScenarioElement[] elementsToIgnore)
        {
            bool attachToMapSuccess = base.AttachToMap(position, elementsToIgnore);
            if (attachToMapSuccess)
            {
                this.MotionControl.Fix();
            }
            return attachToMapSuccess;
        }

        /// <summary>
        /// Constructs a TerranBuilding instance.
        /// </summary>
        /// <param name="buildingTypeName">The name of the type of this Terran building.</param>
        /// <param name="behaviors">The list of behaviors of this Terran building.</param>
        protected TerranBuilding(string buildingTypeName, params EntityBehavior[] behaviors)
            : base(buildingTypeName, behaviors)
        {
            this.constructionJob = this.ConstructField<TerranBuildingConstructionJob>("constructionJob");
            this.constructionJob.Write(null);
        }

        /// <see cref="Entity.OnDestroyingImpl"/>
        protected override void OnDestroyingImpl()
        {
            if (this.constructionJob.Read() != null)
            {
                this.constructionJob.Read().DetachSCV();
            }
        }

        /// <see cref="HeapedObject.DisposeImpl"/>
        protected override void DisposeImpl()
        {
            if (this.constructionJob.Read() != null)
            {
                this.constructionJob.Read().DetachSCV();
                this.constructionJob.Read().Dispose();
                this.constructionJob.Write(null);
            }

            base.DisposeImpl();
        }

        /// <summary>
        /// This method is called after this building is created by a TerranBuildingConstructionJob.
        /// </summary>
        /// <param name="job">Reference to the job.</param>
        internal void OnAttachConstructionJob(TerranBuildingConstructionJob job)
        {
            if (job == null) { throw new ArgumentNullException("job"); }
            this.constructionJob.Write(job);
        }

        /// <summary>
        /// Reference to the construction job attached to this Terran building or null if there is no construction job
        /// attached to this building.
        /// </summary>
        private readonly HeapedValue<TerranBuildingConstructionJob> constructionJob;
    }
}
