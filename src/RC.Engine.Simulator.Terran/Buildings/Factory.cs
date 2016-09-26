using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.Behaviors;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.Terran.Units;

namespace RC.Engine.Simulator.Terran.Buildings
{
    /// <summary>
    /// Represents a Terran Factory.
    /// </summary>
    class Factory : TerranBuilding
    {
        /// <summary>
        /// Constructs a Terran Factory instance.
        /// </summary>
        public Factory()
            : base(FACTORY_TYPE_NAME,
                   new BurndownBehavior("SmallBurn", "HeavyBurn", (RCNumber)78 / (RCNumber)1000),
                   new ConstructionBehavior("Construction0", "Construction1", "Construction2"),
                   new LiftoffBehavior("Normal", "TakingOff", "Flying", "Landing"),
                   new ProductionAnimationBehavior("Producing", "Normal", Goliath.GOLIATH_TYPE_NAME))
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
        /// The name of the Factory element type.
        /// </summary>
        public const string FACTORY_TYPE_NAME = "Factory";
    }
}
