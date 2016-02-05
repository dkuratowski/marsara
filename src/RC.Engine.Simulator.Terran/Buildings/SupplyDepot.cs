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
    /// Represents a Terran Supply Depot.
    /// </summary>
    class SupplyDepot : TerranBuilding
    {
        /// <summary>
        /// Constructs a Terran Supply Depot instance.
        /// </summary>
        public SupplyDepot()
            : base(SUPPLYDEPOT_TYPE_NAME,
                   new BurndownBehavior("SmallBurn", "HeavyBurn", (RCNumber)78/(RCNumber)1000),
                   new ConstructionBehavior("Construction0", "Construction1", "Construction2"),
                   new BasicAnimationsBehavior("Normal", "Normal", "Normal"))
        {
        }

        /// <see cref="Entity.DestructionAnimationName"/>
        protected override string DestructionAnimationName { get { return "Destruction"; } }

        /// <summary>
        /// The name of the Supply Depot element type.
        /// </summary>
        public const string SUPPLYDEPOT_TYPE_NAME = "SupplyDepot";
    }
}
