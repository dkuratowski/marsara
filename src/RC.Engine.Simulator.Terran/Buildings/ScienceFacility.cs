using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.Behaviors;
using RC.Engine.Simulator.Engine;

namespace RC.Engine.Simulator.Terran.Buildings
{
    /// <summary>
    /// Represents a Terran Science Facility.
    /// </summary>
    class ScienceFacility : TerranBuilding
    {
        /// <summary>
        /// Constructs a Terran Science Facility instance.
        /// </summary>
        public ScienceFacility()
            : base(SCIENCEFACILITY_TYPE_NAME,
                   new BurndownBehavior("SmallBurn", "HeavyBurn", (RCNumber)78 / (RCNumber)1000),
                   new ConstructionBehavior("Construction0", "Construction1", "Construction2"),
                   new LiftoffBehavior("Normal", "TakingOff", "Flying", "Landing"),
                   new ProductionAnimationBehavior("Producing", "Normal"))
        {
        }

        /// <see cref="Entity.DestructionAnimationName"/>
        protected override string DestructionAnimationName
        {
            get
            {
                return this.MotionControl.IsFlying
                    ? "DestructionFlying"
                    : "DestructionNormal";
            }
        }

        /// <summary>
        /// The name of the Science Facility element type.
        /// </summary>
        public const string SCIENCEFACILITY_TYPE_NAME = "ScienceFacility";
    }
}
