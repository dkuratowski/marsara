using RC.Common;
using RC.Engine.Simulator.Engine;

namespace RC.Engine.Simulator.Terran.Buildings
{
    /// <summary>
    /// Represents a Terran Command Center
    /// </summary>
    class CommandCenter : Building
    {
        /// <summary>
        /// Constructs a Terran Command Center instance.
        /// </summary>
        public CommandCenter()
            : base(COMMANDCENTER_TYPE_NAME)
        {
        }

        /// <see cref="ScenarioElement.AttachToMap"/>
        public override bool AttachToMap(RCNumVector position)
        {
            bool attachToMapSuccess = base.AttachToMap(position);
            if (attachToMapSuccess) { this.MapObject.SetCurrentAnimation("Normal"); }
            return attachToMapSuccess;
        }

        /// <summary>
        /// The name of the Command Center element type.
        /// </summary>
        public const string COMMANDCENTER_TYPE_NAME = "CommandCenter";
    }
}
