using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Scenarios;

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
            : base(SCV_TYPE_NAME)
        {
            this.SetCurrentAnimation("Stopped", (MapDirection)RandomService.DefaultGenerator.Next(8));
        }

        /// <summary>
        /// The name of the SCV element type.
        /// </summary>
        private const string SCV_TYPE_NAME = "SCV";
    }
}
