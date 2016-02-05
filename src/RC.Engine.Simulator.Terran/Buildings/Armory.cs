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
    /// Represents a Terran Armory.
    /// </summary>
    class Armory : TerranBuilding
    {
        /// <summary>
        /// Constructs a Terran Armory instance.
        /// </summary>
        public Armory()
            : base(ARMORY_TYPE_NAME,
                   new BurndownBehavior("SmallBurn", "HeavyBurn", (RCNumber)78 / (RCNumber)1000),
                   new ConstructionBehavior("Construction0", "Construction1", "Construction2"),
                   new ProductionAnimationBehavior("Producing", "Normal"))
        {
        }

        /// <see cref="Entity.DestructionAnimationName"/>
        protected override string DestructionAnimationName { get { return "Destruction"; } }

        /// <summary>
        /// The name of the Armory element type.
        /// </summary>
        public const string ARMORY_TYPE_NAME = "Armory";
    }
}
