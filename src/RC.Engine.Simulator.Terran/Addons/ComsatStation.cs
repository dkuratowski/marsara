using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.Behaviors;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Terran.Addons
{
    /// <summary>
    /// Represents a Terran Comsat Station.
    /// </summary>
    class ComsatStation : Addon
    {
        /// <summary>
        /// Constructs a Terran Comsat Station.
        /// </summary>
        public ComsatStation()
            : base(COMSATSTATION_TYPE_NAME,
                   new BurndownBehavior("SmallBurn", "HeavyBurn", (RCNumber)78/(RCNumber)1000),
                   new ConstructionBehavior("Construction0", "Construction1", "Construction2"),
                   new AddonBehavior("Online", "Offline"))
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
                return "DestructionOnline";
            }
        }

        /// <summary>
        /// The name of the Comsat Station element type.
        /// </summary>
        public const string COMSATSTATION_TYPE_NAME = "ComsatStation";
    }
}
