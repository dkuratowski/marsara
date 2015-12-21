using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.Engine.Behaviors;

namespace RC.Engine.Simulator.Terran.Buildings
{
    /// <summary>
    /// Represents a Terran Barracks.
    /// </summary>
    class Barracks : Building
    {
        /// <summary>
        /// Constructs a Terran Barracks instance.
        /// </summary>
        public Barracks()
            : base(BARRACKS_TYPE_NAME,
                   new BurndownBehavior("SmallBurn", "HeavyBurn", (RCNumber)78/(RCNumber)1000),
                   new LiftoffBehavior("Normal", "TakingOff", "Flying", "Landing"))
        {
        }

        /// <see cref="ScenarioElement.AttachToMap"/>
        public override bool AttachToMap(RCNumVector position)
        {
            bool attachToMapSuccess = base.AttachToMap(position);
            if (attachToMapSuccess)
            {
                this.MotionControl.Fix();
                this.MapObject.StartAnimation("Normal");
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
        /// The name of the Barracks element type.
        /// </summary>
        public const string BARRACKS_TYPE_NAME = "Barracks";
    }
}
