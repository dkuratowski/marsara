﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.Behaviors;
using RC.Engine.Simulator.Engine;

namespace RC.Engine.Simulator.Terran.Buildings
{
    /// <summary>
    /// Represents a Terran Academy.
    /// </summary>
    class Academy : Building
    {
        /// <summary>
        /// Constructs a Terran Academy instance.
        /// </summary>
        public Academy()
            : base(ACADEMY_TYPE_NAME,
                   new BurndownBehavior("SmallBurn", "HeavyBurn", (RCNumber)78 / (RCNumber)1000),
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
        protected override string DestructionAnimationName { get { return "Destruction"; } }

        /// <summary>
        /// The name of the Academy element type.
        /// </summary>
        public const string ACADEMY_TYPE_NAME = "Academy";
    }
}
