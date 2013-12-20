using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.Scenarios;

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
        /// <param name="quadCoords">The quadratic coordinates of the Command Center.</param>
        public CommandCenter(RCIntVector quadCoords)
            : base(COMMANDCENTER_TYPE_NAME, quadCoords)
        {
            this.SetCurrentAnimation("Normal");
        }

        /// <summary>
        /// The name of the Command Center element type.
        /// </summary>
        private const string COMMANDCENTER_TYPE_NAME = "CommandCenter";
    }
}
