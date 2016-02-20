using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.Terran.Buildings;
using RC.Engine.Simulator.Terran.Units;

namespace RC.Engine.Simulator.Terran.Commands
{
    /// <summary>
    /// Represents the construction job of a Terran building.
    /// </summary>
    class TerranBuildingConstructionJob : ProductionJob
    {
        /// <summary>
        /// Constructs a TerranBuildingConstructionJob instance.
        /// </summary>
        /// <param name="starterSCV">The SCV to attach to this job automatically when its being started.</param>
        /// <param name="buildingProduct">The type of building to be created by this job.</param>
        /// <param name="topLeftQuadTile">The coordinates of the top-left quadratic tile of the building to be created.</param>
        public TerranBuildingConstructionJob(SCV starterSCV, IBuildingType buildingProduct, RCIntVector topLeftQuadTile)
            : base(starterSCV.Owner, buildingProduct, 0)
        {
            if (starterSCV == null) { throw new ArgumentNullException("starterSCV"); }
            if (topLeftQuadTile == RCIntVector.Undefined) { throw new ArgumentNullException("topLeftQuadTile"); }

            this.buildingProduct = buildingProduct;
            this.topLeftQuadTile = this.ConstructField<RCIntVector>("topLeftQuadTile");
            this.constructedBuilding = this.ConstructField<TerranBuilding>("constructedBuilding");
            this.attachedSCV = this.ConstructField<SCV>("attachedSCV");
            this.starterSCV = this.ConstructField<SCV>("starterSCV");
            this.topLeftQuadTile.Write(topLeftQuadTile);
            this.constructedBuilding.Write(null);
            this.starterSCV.Write(starterSCV);
        }

        /// <summary>
        /// Gets the building being constructed by this construction job.
        /// </summary>
        public TerranBuilding ConstructedBuilding { get { return this.constructedBuilding.Read(); } }

        /// <summary>
        /// Gets the SCV that is currently attached to this construction job or null of there is no SCV currently attached
        /// to this construction job.
        /// </summary>
        public SCV AttachedSCV { get { return this.attachedSCV.Read(); } }

        /// <summary>
        /// Attaches the given SCV to this construction job.
        /// </summary>
        /// <param name="scv">The SCV to be attached.</param>
        /// <exception cref="InvalidOperationException">
        /// If this construction job has already an SCV attached to it.
        /// If this construction job is not attached to a Terran building.
        /// </exception>
        public void AttachSCV(SCV scv)
        {
            if (scv == null) { throw new ArgumentNullException("scv"); }
            if (this.attachedSCV.Read() != null) { throw new InvalidOperationException("This construction job has already an SCV attached to it!"); }
            if (this.constructedBuilding.Read() == null) { throw new InvalidOperationException("This construction job is not attached to a Terran building!"); }

            this.attachedSCV.Write(scv);
            this.attachedSCV.Read().OnAttachConstructionJob(this);
        }

        /// <summary>
        /// Detaches the currently attached SCV from this construction job. If there is no attached SCV then this method has no effect.
        /// </summary>
        public void DetachSCV()
        {
            if (this.attachedSCV.Read() != null)
            {
                this.attachedSCV.Read().OnDetachConstructionJob();
                this.attachedSCV.Write(null);
            }
        }

        /// <see cref="ProductionJob.StartImpl"/>
        protected override bool StartImpl()
        {
            /// Create the building and begin its construction.
            bool success = this.ElementFactory.CreateElement(this.buildingProduct.Name, this.OwnerPlayer, this.topLeftQuadTile.Read(), this.starterSCV.Read());
            if (success)
            {
                TerranBuilding building = this.OwnerPlayer.Scenario.GetFixedEntity<TerranBuilding>(this.topLeftQuadTile.Read());
                if (building == null) { throw new InvalidOperationException("Impossible case happened!"); }
                this.constructedBuilding.Write(building);
                this.constructedBuilding.Read().OnAttachConstructionJob(this);
                this.AttachSCV(this.starterSCV.Read());
                this.starterSCV.Write(null);
            }
            return success;
        }

        /// <see cref="ProductionJob.ContinueImpl"/>
        protected override bool ContinueImpl()
        {
            if (this.constructedBuilding.Read().Scenario == null)
            {
                /// The building has been destroyed -> do not continue this production job!
                return false;
            }

            /// Continue the construction of the building.
            this.constructedBuilding.Read().Biometrics.Construct();
            return true;
        }

        /// <see cref="ProductionJob.AbortImpl"/>
        protected override void AbortImpl(int lockedMinerals, int lockedVespeneGas, int lockedSupplies)
        {
            /// Give back the locked supply to the player of the owner building (if the player still exists).
            this.OwnerPlayer.UnlockSupply(lockedSupplies);

            /// Destroy the building being under construction.
            if (this.constructedBuilding.Read().Scenario != null)
            {
                this.constructedBuilding.Read().Biometrics.CancelConstruct();
            }
        }

        /// <summary>
        /// The type of building created by this job.
        /// </summary>
        private readonly IBuildingType buildingProduct;

        /// <summary>
        /// The coordinates of the top-left quadratic tile of the building to be created.
        /// </summary>
        private readonly HeapedValue<RCIntVector> topLeftQuadTile;

        /// <summary>
        /// Reference to the building being constructed by this production job.
        /// </summary>
        private readonly HeapedValue<TerranBuilding> constructedBuilding;

        /// <summary>
        /// Reference to the SCV that is currently attached to this construction job or null of there is no SCV currently attached
        /// to this construction job.
        /// </summary>
        private readonly HeapedValue<SCV> attachedSCV;

        /// <summary>
        /// Reference to the SCV that to be attached automatically to this construction job when it is being started.
        /// </summary>
        private readonly HeapedValue<SCV> starterSCV;
    }
}
