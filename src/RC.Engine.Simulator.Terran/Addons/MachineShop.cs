using RC.Common;
using RC.Engine.Simulator.Behaviors;
using RC.Engine.Simulator.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Terran.Addons
{
    /// <summary>
    /// Represents a Terran Machine Shop.
    /// </summary>
    class MachineShop : Addon
    {
        /// <summary>
        /// Constructs a Terran Machine Shop.
        /// </summary>
        public MachineShop()
            : base(MACHINESHOP_TYPE_NAME,
                   new BurndownBehavior("SmallBurn", "HeavyBurn", (RCNumber)78 / (RCNumber)1000),
                   new ConstructionBehavior("Construction0", "Construction1", "Construction2"),
                   new AddonBehavior("Online", "Producing", "Normal", "Offline"))
        {
        }

        /// <see cref="ScenarioElement.AttachToMap"/>
        public override bool AttachToMap(RCNumVector position, params ScenarioElement[] elementsToIgnore)
        {
            bool attachToMapSuccess = base.AttachToMap(position, elementsToIgnore);
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
        /// The name of the Machine Shop element type.
        /// </summary>
        public const string MACHINESHOP_TYPE_NAME = "MachineShop";
    }
}
