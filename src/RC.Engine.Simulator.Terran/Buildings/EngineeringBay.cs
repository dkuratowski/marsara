using RC.Common;
using RC.Engine.Simulator.Behaviors;
using RC.Engine.Simulator.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Terran.Buildings
{
    /// <summary>
    /// Represents a Terran Engineering Bay.
    /// </summary>
    class EngineeringBay : Building
    {
        /// <summary>
        /// Constructs a Terran Engineering Bay instance.
        /// </summary>
        public EngineeringBay()
            : base(ENGINEERINGBAY_TYPE_NAME,
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
        /// The name of the Engineering Bay element type.
        /// </summary>
        public const string ENGINEERINGBAY_TYPE_NAME = "EngineeringBay";
    }
}
