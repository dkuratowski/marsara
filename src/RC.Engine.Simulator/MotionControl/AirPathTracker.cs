using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Core;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// The path-tracker implementation for entities in the air.
    /// </summary>
    public class AirPathTracker : PathTrackerBase
    {
        /// <summary>
        /// Constructs a AirPathTracker instance.
        /// </summary>
        /// <param name="controlledEntity">The entity that this path tracker controls.</param>
        public AirPathTracker(Entity controlledEntity) : base(controlledEntity)
        {
        }

        #region PathTrackerBase overrides

        /// <see cref="PathTrackerBase.CurrentWaypoint"/>
        protected override RCNumVector CurrentWaypoint { get { return this.TargetPosition; } }

        /// <see cref="PathTrackerBase.IsLastWaypoint"/>
        protected override bool IsLastWaypoint { get { return true; } }

        #endregion PathTrackerBase overrides
    }
}
