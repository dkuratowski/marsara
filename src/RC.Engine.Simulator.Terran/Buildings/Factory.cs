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
    /// Represents a Terran Factory.
    /// </summary>
    class Factory : Building
    {
        /// <summary>
        /// Constructs a Terran Factory instance.
        /// </summary>
        public Factory()
            : base(FACTORY_TYPE_NAME,
                   new BurndownBehavior("SmallBurn", "HeavyBurn", (RCNumber)78 / (RCNumber)1000),
                   new LiftoffBehavior("Normal", "TakingOff", "Flying", "Landing"),
                   new ProductionAnimationBehavior("Producing", "Normal"))
        {
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
