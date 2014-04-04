using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// The actuator implementation for ground units.
    /// </summary>
    class GroundUnitActuator : EntityActuatorBase
    {
        /// <summary>
        /// Constructs a GroundUnitActuator instance with the given maximum speed.
        /// </summary>
        /// <param name="velocityValue">Reference to the velocity value that this actuator has to manipulate.</param>
        /// <param name="maxSpeedValue">Reference to the value that stores the current value of the maximum speed.</param>
        public GroundUnitActuator(IValue<RCNumVector> velocityValue, IValueRead<RCNumber> maxSpeedValue)
            : base(velocityValue)
        {
            if (maxSpeedValue == null) { throw new ArgumentNullException("maxSpeedValue"); }
            if (maxSpeedValue.Read() < 0) { throw new ArgumentOutOfRangeException("maxSpeedValue"); }

            this.maxSpeedValue = maxSpeedValue;
            this.maxSpeedValue.ValueChanged += this.OnMaxSpeedValueChanged;
            this.UpdateVelocityGraph();
        }

        /// <see cref="IDisposable.Dispose"/>
        public override void Dispose() { this.maxSpeedValue.ValueChanged -= this.OnMaxSpeedValueChanged; }

        /// <summary>
        /// This method is called when the value of the maximum speed has been changed.
        /// </summary>
        private void OnMaxSpeedValueChanged(object sender, EventArgs e) { this.UpdateVelocityGraph(); }

        /// <summary>
        /// Updates the velocity graph based on the current value of the maximum speed.
        /// </summary>
        private void UpdateVelocityGraph()
        {
            /// First we calculate every possible velocity vectors and put them into a 2D array where the first coordinate
            /// is the index of the corresponding speed and the second coordinate is the index of the corresponding
            /// basis vector. In parallel we put the calculated vectors into the velocity graph with empty reachability
            /// lists. Those lists will be filled in a second step.
            Dictionary<RCNumVector, HashSet<RCNumVector>> velocityGraph = new Dictionary<RCNumVector, HashSet<RCNumVector>>();
            RCNumVector[,] velocityVectors = new RCNumVector[ACCELERATION_DURATION + 1, BASIS_VECTORS.Length];
            velocityVectors[0, 0] = new RCNumVector(0, 0);
            velocityGraph.Add(new RCNumVector(0, 0), new HashSet<RCNumVector>());
            RCNumber speedIncrement = this.maxSpeedValue.Read() / ACCELERATION_DURATION;
            for (int spdIdx = 1; spdIdx <= ACCELERATION_DURATION; spdIdx++)
            {
                RCNumber speed = speedIncrement * spdIdx;
                for (int basisVectIdx = 0; basisVectIdx < BASIS_VECTORS.Length; basisVectIdx++)
                {
                    RCNumVector velocityVector = BASIS_VECTORS[basisVectIdx] * speed;
                    velocityVectors[spdIdx, basisVectIdx] = velocityVector;
                    velocityGraph.Add(velocityVector, new HashSet<RCNumVector>());
                }
            }

            /// Collect the velocities reachable from the (0, 0) vector.
            velocityGraph[velocityVectors[0, 0]].Add(velocityVectors[0, 0]);
            for (int basisVectIdx = 0; basisVectIdx < BASIS_VECTORS.Length; basisVectIdx++)
            {
                velocityGraph[velocityVectors[0, 0]].Add(velocityVectors[1, basisVectIdx]);
            }

            /// Collect the velocities reachable from the longest velocities.
            for (int basisVectIdx = 0; basisVectIdx < BASIS_VECTORS.Length; basisVectIdx++)
            {
                HashSet<RCNumVector> reachableVelocityList = velocityGraph[velocityVectors[ACCELERATION_DURATION, basisVectIdx]];
                reachableVelocityList.Add(velocityVectors[ACCELERATION_DURATION, basisVectIdx]);
                reachableVelocityList.Add(velocityVectors[ACCELERATION_DURATION, (basisVectIdx + 1) % BASIS_VECTORS.Length]);
                reachableVelocityList.Add(velocityVectors[ACCELERATION_DURATION, (basisVectIdx - 1 + BASIS_VECTORS.Length) % BASIS_VECTORS.Length]);

                if (ACCELERATION_DURATION == 1) { reachableVelocityList.Add(velocityVectors[0, 0]); }
                else
                {
                    reachableVelocityList.Add(velocityVectors[ACCELERATION_DURATION - 1, basisVectIdx]);
                    reachableVelocityList.Add(velocityVectors[ACCELERATION_DURATION - 1, (basisVectIdx + 1) % BASIS_VECTORS.Length]);
                    reachableVelocityList.Add(velocityVectors[ACCELERATION_DURATION - 1, (basisVectIdx - 1 + BASIS_VECTORS.Length) % BASIS_VECTORS.Length]);
                }
            }

            if (ACCELERATION_DURATION > 1)
            {
                /// Collect the velocities reachable from the shortest velocities.
                for (int basisVectIdx = 0; basisVectIdx < BASIS_VECTORS.Length; basisVectIdx++)
                {
                    HashSet<RCNumVector> reachableVelocityList = velocityGraph[velocityVectors[1, basisVectIdx]];
                    reachableVelocityList.Add(velocityVectors[0, 0]);
                    reachableVelocityList.Add(velocityVectors[1, basisVectIdx]);
                    reachableVelocityList.Add(velocityVectors[1, (basisVectIdx + 1) % BASIS_VECTORS.Length]);
                    reachableVelocityList.Add(velocityVectors[1, (basisVectIdx - 1 + BASIS_VECTORS.Length) % BASIS_VECTORS.Length]);
                    reachableVelocityList.Add(velocityVectors[2, basisVectIdx]);
                    reachableVelocityList.Add(velocityVectors[2, (basisVectIdx + 1) % BASIS_VECTORS.Length]);
                    reachableVelocityList.Add(velocityVectors[2, (basisVectIdx - 1 + BASIS_VECTORS.Length) % BASIS_VECTORS.Length]);
                }

                /// Collect the velocities reachable from any other velocities.
                for (int spdIdx = 2; spdIdx < ACCELERATION_DURATION; spdIdx++)
                {
                    for (int basisVectIdx = 0; basisVectIdx < BASIS_VECTORS.Length; basisVectIdx++)
                    {
                        HashSet<RCNumVector> reachableVelocityList = velocityGraph[velocityVectors[spdIdx, basisVectIdx]];
                        reachableVelocityList.Add(velocityVectors[spdIdx - 1, basisVectIdx]);
                        reachableVelocityList.Add(velocityVectors[spdIdx - 1, (basisVectIdx + 1) % BASIS_VECTORS.Length]);
                        reachableVelocityList.Add(velocityVectors[spdIdx - 1, (basisVectIdx - 1 + BASIS_VECTORS.Length) % BASIS_VECTORS.Length]);
                        reachableVelocityList.Add(velocityVectors[spdIdx, basisVectIdx]);
                        reachableVelocityList.Add(velocityVectors[spdIdx, (basisVectIdx + 1) % BASIS_VECTORS.Length]);
                        reachableVelocityList.Add(velocityVectors[spdIdx, (basisVectIdx - 1 + BASIS_VECTORS.Length) % BASIS_VECTORS.Length]);
                        reachableVelocityList.Add(velocityVectors[spdIdx + 1, basisVectIdx]);
                        reachableVelocityList.Add(velocityVectors[spdIdx + 1, (basisVectIdx + 1) % BASIS_VECTORS.Length]);
                        reachableVelocityList.Add(velocityVectors[spdIdx + 1, (basisVectIdx - 1 + BASIS_VECTORS.Length) % BASIS_VECTORS.Length]);
                    }
                }
            }

            /// Upload the velocity graph to the base class.
            this.UpdateVelocityGraph(velocityGraph);
        }

        /// <summary>
        /// Reference to the value that stores the current value of the maximum speed.
        /// </summary>
        private IValueRead<RCNumber> maxSpeedValue;

        /// <summary>
        /// The list of the basis vectors starting from North in clockwise order.
        /// </summary>
        private static readonly RCNumVector[] BASIS_VECTORS = new RCNumVector[]
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
        private static readonly int ACCELERATION_DURATION = 5;
    }
}
