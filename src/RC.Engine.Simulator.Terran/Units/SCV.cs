using System;
using System.Collections.Generic;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Behaviors;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.MotionControl;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.Terran.Buildings;
using RC.Engine.Simulator.Terran.Commands;

namespace RC.Engine.Simulator.Terran.Units
{
    /// <summary>
    /// Represents a Terran SCV.
    /// </summary>
    class SCV : Unit
    {
        /// <summary>
        /// Constructs a Terran SCV instance.
        /// </summary>
        public SCV()
            : base(SCV_TYPE_NAME, false, new BasicAnimationsBehavior("Moving", "Stopped", "Stopped"))
        {
            this.constructionJob = this.ConstructField<TerranBuildingConstructionJob>("constructionJob");
            this.constructionJob.Write(null);
        }

        /// <summary>
        /// Gets whether this SCV is currently performing a construction job.
        /// </summary>
        public bool IsConstructing { get { return this.constructionJob.Read() != null; } }

        /// <summary>
        /// Gets the construction job to which this SCV is attached to or null if this SCV is not attached to a construction job.
        /// </summary>
        public TerranBuildingConstructionJob ConstructionJob { get { return this.constructionJob.Read(); } }

        /// <summary>
        /// Continues the current construction job of this SCV.
        /// </summary>
        /// <returns>True if the construction job has finished; otherwise false.</returns>
        public bool ContinueConstruct()
        {
            if (this.constructionJob.Read() != null && this.constructionJob.Read().Continue())
            {
                this.constructionJob.Read().DetachSCV();
                this.constructionJob.Write(null);
            }

            return !this.IsConstructing;
        }

        /// <see cref="Entity.IsOverlapEnabled"/>
        public override bool IsOverlapEnabled(Entity otherEntity)
        {
            return this.constructionJob.Read() != null && this.constructionJob.Read().ConstructedBuilding == otherEntity;
        }

        /// <see cref="Entity.DestructionAnimationName"/>
        protected override string DestructionAnimationName { get { return "Dying"; } }

        /// <see cref="Entity.OnDestroyingImpl"/>
        protected override void OnDestroyingImpl()
        {
            if (this.constructionJob.Read() != null)
            {
                this.constructionJob.Read().DetachSCV();
            }
        }

        /// <summary>
        /// This method is called after this SCV is attached to a TerranBuildingConstructionJob.
        /// </summary>
        /// <param name="job">Reference to the job.</param>
        internal void OnAttachConstructionJob(TerranBuildingConstructionJob job)
        {
            if (job == null) { throw new ArgumentNullException("job"); }
            if (this.constructionJob.Read() != null) { throw new InvalidOperationException("This SCV has already a construction job attached to it!"); }

            this.constructionJob.Write(job);
        }

        /// <summary>
        /// This method is called when this SCV is being detached from a TerranBuildingConstructionJob.
        /// </summary>
        internal void OnDetachConstructionJob()
        {
            if (this.constructionJob.Read() == null) { throw new InvalidOperationException("This SCV has no construction job attached to it!"); }
            this.constructionJob.Write(null);
        }

        /// <summary>
        /// Reference to the construction job that is currently attached to this SCV.
        /// </summary>
        private readonly HeapedValue<TerranBuildingConstructionJob> constructionJob;

        /// <summary>
        /// The name of the SCV element type.
        /// </summary>
        public const string SCV_TYPE_NAME = "SCV";
    }
}
