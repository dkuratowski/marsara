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
    /// Represents a Terran Starport.
    /// </summary>
    class Starport : Building
    {
        /// <summary>
        /// Constructs a Terran Starport instance.
        /// </summary>
        public Starport()
            : base(STARPORT_TYPE_NAME,
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
        /// The name of the Starport element type.
        /// </summary>
        public const string STARPORT_TYPE_NAME = "Starport";
    }
}
