using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.MotionControl;

namespace RC.Engine.Simulator.Terran.Units
{
    /// <summary>
    /// Represents a Terran SCV.
    /// </summary>
    class SCV : Unit
    {
        /// <summary>
        /// Constructs a Terran SCV instance.
        /// </summary>
        public SCV()
            : base(SCV_TYPE_NAME, false)
        {
        }

        /// <see cref="ScenarioElement.AttachToMap"/>
        public override bool AttachToMap(RCNumVector position)
        {
            bool attachToMapSuccess = base.AttachToMap(position);
            if (attachToMapSuccess) { this.MapObject.StartAnimation("Stopped", this.MotionControl.VelocityVector, this.Armour.TargetVector); }
            return attachToMapSuccess;
        }

        /// <see cref="Entity.DestructionAnimationName"/>
        protected override string DestructionAnimationName { get { return "Dying"; } }

        /// <summary>
        /// The name of the SCV element type.
        /// </summary>
        public const string SCV_TYPE_NAME = "SCV";
    }
}
