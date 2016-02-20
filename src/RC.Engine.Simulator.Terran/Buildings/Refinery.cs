using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.Behaviors;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Terran.Buildings
{
    /// <summary>
    /// Represents a Terran Refinery.
    /// </summary>
    class Refinery : TerranBuilding, IResourceProvider
    {
        /// <summary>
        /// Constructs a Terran Refinery instance.
        /// </summary>
        public Refinery()
            : base(REFINERY_TYPE_NAME,
                   new BurndownBehavior("SmallBurn", "HeavyBurn", (RCNumber)78/(RCNumber)1000),
                   new VespeneGasProviderConstructionBehavior(
                       new string[] { "Construction0_Depleted", "Construction1_Depleted", "Construction2_Depleted" },
                       new string[] { "Construction0_Normal", "Construction1_Normal", "Construction2_Normal" }),
                   new VespeneGasProviderAnimationsBehavior("Depleted", "Normal"))
        {
            this.underlyingVespeneGeyser = this.ConstructField<VespeneGeyser>("underlyingVespeneGeyser");
            this.underlyingVespeneGeyser.Write(null);
        }

        #region IResourceProvider methods

        /// <see cref="IResourceProvider.MineralsAmount"/>
        int IResourceProvider.MineralsAmount { get { return -1; } }

        /// <see cref="IResourceProvider.VespeneGasAmount"/>
        int IResourceProvider.VespeneGasAmount
        {
            get
            {
                if (this.underlyingVespeneGeyser.Read() == null) { throw new InvalidOperationException("Refinery doesn't have underlying vespene geyser!"); }
                return this.underlyingVespeneGeyser.Read().ResourceAmount.Read();
            }
        }

        #endregion IResourceProvider methods

        /// <see cref="ScenarioElement.AttachToMap"/>
        public override bool AttachToMap(RCNumVector position, params ScenarioElement[] elementsToIgnore)
        {
            /// Check if the position of this Refinery would align with a VespeneGeyser.
            RCSet<VespeneGeyser> vespeneGeysersAtPos = this.Scenario.GetElementsOnMap<VespeneGeyser>(position, MapObjectLayerEnum.GroundObjects);
            if (vespeneGeysersAtPos.Count != 1) { return false; }
            VespeneGeyser vespeneGeyserAtPos = vespeneGeysersAtPos.First();
            if (this.CalculateArea(position) != vespeneGeyserAtPos.Area) { return false; }
            
            /// Save the VespeneGeyser and detach it from the map.
            this.underlyingVespeneGeyser.Write(vespeneGeyserAtPos);
            this.underlyingVespeneGeyser.Read().DetachFromMap();
            
            /// Try to attach the Refinery.
            bool refineryAttached = base.AttachToMap(position, elementsToIgnore);
            if (!refineryAttached)
            {
                /// If the Refinery could not be attached -> reattach the underlying VespeneGeyser.
                this.underlyingVespeneGeyser.Read().AttachToMap(position);
                this.underlyingVespeneGeyser.Write(null);
            }

            return refineryAttached;
        }

        /// <see cref="ScenarioElement.DetachFromMap"/>
        public override RCNumVector DetachFromMap()
        {
            this.ReattachUnderlyingVespeneGeyser();
            return base.DetachFromMap();
        }

        /// <see cref="Entity.OnDestroyingImpl"/>
        protected override void OnDestroyingImpl()
        {
            this.ReattachUnderlyingVespeneGeyser();
            base.OnDestroyingImpl();
        }

        /// <see cref="Entity.DestructionAnimationName"/>
        protected override string DestructionAnimationName { get { return "Destruction"; } }

        /// <summary>
        /// Reattaches the underlying VespeneGeyser if it has not yet been reattached.
        /// </summary>
        private void ReattachUnderlyingVespeneGeyser()
        {
            if (this.underlyingVespeneGeyser.Read() != null)
            {
                if (this.MotionControl.Status == MotionControlStatusEnum.Fixed) { this.MotionControl.Unfix(); }
                RCSet<Entity> entitiesToIgnore = this.Scenario.GetElementsOnMap<Entity>(this.Area, MapObjectLayerEnum.GroundObjects);
                entitiesToIgnore.Add(this);
                if (!this.underlyingVespeneGeyser.Read().AttachToMap(this.MotionControl.PositionVector.Read(), entitiesToIgnore.ToArray()))
                {
                    throw new InvalidOperationException("Underlying VespeneGeyser could not be reattached!");
                }
                this.underlyingVespeneGeyser.Write(null);
            }
        }

        /// <summary>
        /// Reference to the underlying vespene geyser.
        /// </summary>
        private readonly HeapedValue<VespeneGeyser> underlyingVespeneGeyser;

        /// <summary>
        /// The name of the Refinery element type.
        /// </summary>
        public const string REFINERY_TYPE_NAME = "Refinery";
    }
}
