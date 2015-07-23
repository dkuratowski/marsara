using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// Represents a hexadecagonal velocity graph.
    /// </summary>
    public class HexadecagonalVelocityGraph : BasicVelocityGraph
    {
        /// <summary>
        /// Constructs a hexadecagonal velocity graph.
        /// </summary>
        /// <param name="maxSpeed">The maximum speed.</param>
        public HexadecagonalVelocityGraph(RCNumber maxSpeed)
            : base(BASIS_VECTORS, maxSpeed, ACCELERATION_DURATION)
        {
        }

        /// <summary>
        /// The list of the basis vectors starting from North in clockwise order.
        /// </summary>
        private static readonly List<RCNumVector> BASIS_VECTORS = new List<RCNumVector>
        {
            new RCNumVector(0, -1),
            new RCNumVector((RCNumber)382/(RCNumber)1000, -(RCNumber)923/(RCNumber)1000),
            new RCNumVector(1, -1) / RCNumber.ROOT_OF_TWO,
            new RCNumVector((RCNumber)923/(RCNumber)1000, -(RCNumber)382/(RCNumber)1000),
            new RCNumVector(1, 0),
            new RCNumVector((RCNumber)923/(RCNumber)1000, (RCNumber)382/(RCNumber)1000),
            new RCNumVector(1, 1) / RCNumber.ROOT_OF_TWO,
            new RCNumVector((RCNumber)382/(RCNumber)1000, (RCNumber)923/(RCNumber)1000),
            new RCNumVector(0, 1),
            new RCNumVector(-(RCNumber)382/(RCNumber)1000, (RCNumber)923/(RCNumber)1000),
            new RCNumVector(-1, 1) / RCNumber.ROOT_OF_TWO,
            new RCNumVector(-(RCNumber)923/(RCNumber)1000, (RCNumber)382/(RCNumber)1000),
            new RCNumVector(-1, 0),
            new RCNumVector(-(RCNumber)923/(RCNumber)1000, -(RCNumber)382/(RCNumber)1000),
            new RCNumVector(-1, -1) / RCNumber.ROOT_OF_TWO,
            new RCNumVector(-(RCNumber)382/(RCNumber)1000, -(RCNumber)923/(RCNumber)1000),
        };

        /// <summary>
        /// The number of frames needed to reach the maximum speed.
        /// </summary>
        private const int ACCELERATION_DURATION = 5;
    }
}
