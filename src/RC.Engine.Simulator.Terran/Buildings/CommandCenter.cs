using RC.Common;
using RC.Engine.Simulator.Behaviors;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.Terran.Units;

namespace RC.Engine.Simulator.Terran.Buildings
{
    /// <summary>
    /// Represents a Terran Command Center.
    /// </summary>
    class CommandCenter : TerranBuilding
    {
        /// <summary>
        /// Constructs a Terran Command Center instance.
        /// </summary>
        public CommandCenter()
            : base(COMMANDCENTER_TYPE_NAME,
                   new BurndownBehavior("SmallBurn", "HeavyBurn", (RCNumber)78/(RCNumber)1000),
                   new ConstructionBehavior("Construction0", "Construction1", "Construction2"),
                   new LiftoffBehavior("Normal", "TakingOff", "Flying", "Landing"),
                   new ProductionAnimationBehavior("Producing", "Normal", SCV.SCV_TYPE_NAME))
        {
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
        /// The name of the Command Center element type.
        /// </summary>
        public const string COMMANDCENTER_TYPE_NAME = "CommandCenter";
    }
}
