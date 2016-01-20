using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.Behaviors;
using RC.Engine.Simulator.Engine;

namespace RC.Engine.Simulator.Terran.Addons
{
    /// <summary>
    /// Represents a Terran Control Tower.
    /// </summary>
    class ControlTower : Addon
    {
        /// <summary>
        /// Constructs a Terran Control Tower.
        /// </summary>
        public ControlTower()
            : base(CONTROLTOWER_TYPE_NAME,
                   new BurndownBehavior("SmallBurn", "HeavyBurn", (RCNumber)78 / (RCNumber)1000),
                   new ConstructionBehavior("Construction", "Online"),
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
        /// The name of the Control Tower element type.
        /// </summary>
        public const string CONTROLTOWER_TYPE_NAME = "ControlTower";
    }
}
