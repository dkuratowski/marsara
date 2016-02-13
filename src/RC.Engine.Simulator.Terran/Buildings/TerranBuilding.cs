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

        /// <summary>
        /// Attaches this Terran building to the given quadratic tile on the map and starts its construction.
        /// </summary>
        /// <param name="topLeftTile">The quadratic tile at the top-left corner of this Terran building.</param>
        /// <returns>
        /// True if this Terran building was successfully attached to the map and its construction has been started; otherwise false.
        /// </returns>
        /// <remarks>
        /// Attaching a Terran building using this method is allowed only if there is exactly 1 SCV that is entirely contained within the
        /// area where the building is to be placed.
        /// </remarks>
        public bool AttachToMapAndStartConstruction(IQuadTile topLeftTile)
        {
            /// Calculate the area of this Terran building.
            ICell topLeftCell = topLeftTile.GetCell(new RCIntVector(0, 0));
            RCNumVector position = topLeftCell.MapCoords - new RCNumVector(1, 1) / 2 + this.ElementType.Area.Read() / 2;
            RCNumRectangle area = this.CalculateArea(position);

            /// Check if there is exactly 1 SCV at the calculated area.
            RCSet<SCV> scvsAtArea = this.Scenario.GetElementsOnMap<SCV>(area, MapObjectLayerEnum.GroundObjects);
            if (scvsAtArea.Count != 1) { return false; }

            /// Check if the SCV is entirely contained within the calculated area.
            SCV scvAtArea = scvsAtArea.First();
            if (!area.Contains(scvAtArea.Area)) { return false; }

            /// Attach this Terran building to the map.
            this.scvAllowedToOverlap.Write(scvAtArea);
            bool attachToMapSuccess = this.AttachToMap(topLeftTile);
            this.scvAllowedToOverlap.Write(null);

            /// Start construction if successfully attached.
            if (attachToMapSuccess) { this.Biometrics.Construct(); }
            return attachToMapSuccess;
        }

        /// <see cref="ScenarioElement.AttachToMap"/>
        public override bool AttachToMap(RCNumVector position)
        {
            bool attachToMapSuccess = base.AttachToMap(position);
            if (attachToMapSuccess)
            {
                this.MotionControl.Fix();
            }
            return attachToMapSuccess;
        }

        /// <see cref="Entity.IsOverlapEnabled"/>
        public override bool IsOverlapEnabled(Entity otherEntity)
        {
            return otherEntity == this.scvAllowedToOverlap.Read();
        }

        /// <summary>
        /// Constructs a TerranBuilding instance.
        /// </summary>
        /// <param name="buildingTypeName">The name of the type of this Terran building.</param>
        /// <param name="behaviors">The list of behaviors of this Terran building.</param>
        protected TerranBuilding(string buildingTypeName, params EntityBehavior[] behaviors)
            : base(buildingTypeName, behaviors)
        {
            this.scvAllowedToOverlap = this.ConstructField<SCV>("scvAllowedToOverlap");
            this.constructionJob = this.ConstructField<TerranBuildingConstructionJob>("constructionJob");
            this.scvAllowedToOverlap.Write(null);
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
        /// Reference to the SCV that is allowed to overlap the area of this Terran building.
        /// </summary>
        private readonly HeapedValue<SCV> scvAllowedToOverlap;

        /// <summary>
        /// Reference to the construction job attached to this Terran building or null if there is no construction job
        /// attached to this building.
        /// </summary>
        private readonly HeapedValue<TerranBuildingConstructionJob> constructionJob;
    }
}
