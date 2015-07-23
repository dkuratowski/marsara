using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// Represents an octagonal velocity graph.
    /// </summary>
    public class OctagonalVelocityGraph : BasicVelocityGraph
    {
        /// <summary>
        /// Constructs an octogonal velocity graph.
        /// </summary>
        /// <param name="maxSpeed">The maximum speed.</param>
        public OctagonalVelocityGraph(RCNumber maxSpeed) : base(BASIS_VECTORS, maxSpeed, ACCELERATION_DURATION)
        {
        }

        /// <summary>
        /// The list of the basis vectors starting from North in clockwise order.
        /// </summary>
        private static readonly List<RCNumVector> BASIS_VECTORS = new List<RCNumVector>
        {
            new RCNumVector(0, -1),
            new RCNumVector(1, -1) / RCNumber.ROOT_OF_TWO,
            new RCNumVector(1, 0),
            new RCNumVector(1, 1) / RCNumber.ROOT_OF_TWO,
            new RCNumVector(0, 1),
            new RCNumVector(-1, 1) / RCNumber.ROOT_OF_TWO,
            new RCNumVector(-1, 0),
            new RCNumVector(-1, -1) / RCNumber.ROOT_OF_TWO
        };

        /// <summary>
        /// The number of frames needed to reach the maximum speed.
        /// </summary>
        private const int ACCELERATION_DURATION = 5;
    }
}
