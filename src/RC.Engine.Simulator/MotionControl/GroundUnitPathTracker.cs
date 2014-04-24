using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.Scenarios;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// The actuator implementation for ground units.
    /// </summary>
    public class GroundUnitPathTracker : PathTrackerBase
    {
        /// <summary>
        /// Constructs a GroundUnitPathTracker instance.
        /// </summary>
        /// <param name="controlledEntity">The entity that this path tracker controls.</param>
        public GroundUnitPathTracker(Entity controlledEntity) : base(controlledEntity) { }
    }
}
