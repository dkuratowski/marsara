using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// Represents a basic velocity graph.
    /// </summary>
    public abstract class BasicVelocityGraph : VelocityGraph
    {
        /// <summary>
        /// Constructs a basic velocity graph.
        /// </summary>
        /// <param name="basisVectors">The list of basis vectors.</param>
        /// <param name="maxSpeed">The maximum speed.</param>
        /// <param name="accelerationDuration">The number of frames needed to reach the maximum speed.</param>
        protected BasicVelocityGraph(List<RCNumVector> basisVectors, RCNumber maxSpeed, int accelerationDuration) : base(CalculateVelocityGraph(basisVectors, maxSpeed, accelerationDuration))
        {
        }

        /// <summary>
        /// Calculates the velocity graph.
        /// </summary>
        private static Dictionary<RCNumVector, RCSet<RCNumVector>> CalculateVelocityGraph(List<RCNumVector> basisVectors, RCNumber maxSpeed, int accelerationDuration)
        {
            if (basisVectors == null) { throw new ArgumentNullException("basisVectors"); }
            if (basisVectors.Count == 0) { throw new ArgumentException("Empty basis vector list!", "basisVectors"); }
            if (maxSpeed <= 0) { throw new ArgumentOutOfRangeException("maxSpeed", "Maximum speed must be greater than 0!"); }
            if (accelerationDuration < 1) { throw new ArgumentOutOfRangeException("accelerationDuration", "Acceleration duration must be at least 1!"); }

            /// First we calculate every possible velocity vectors and put them into a 2D array where the first coordinate
            /// is the index of the corresponding speed and the second coordinate is the index of the corresponding
            /// basis vector. In parallel we put the calculated vectors into the velocity graph with empty reachability
            /// lists. Those lists will be filled in a second step.
            Dictionary<RCNumVector, RCSet<RCNumVector>> velocityGraph = new Dictionary<RCNumVector, RCSet<RCNumVector>>();
            RCNumVector[,] velocityVectors = new RCNumVector[accelerationDuration + 1, basisVectors.Count];
            velocityVectors[0, 0] = new RCNumVector(0, 0);
            velocityGraph.Add(new RCNumVector(0, 0), new RCSet<RCNumVector>());
            RCNumber speedIncrement = maxSpeed / accelerationDuration;
            for (int spdIdx = 1; spdIdx <= accelerationDuration; spdIdx++)
            {
                RCNumber speed = speedIncrement * spdIdx;
                for (int basisVectIdx = 0; basisVectIdx < basisVectors.Count; basisVectIdx++)
                {
                    RCNumVector velocityVector = basisVectors[basisVectIdx] * speed;
                    velocityVectors[spdIdx, basisVectIdx] = velocityVector;
                    velocityGraph.Add(velocityVector, new RCSet<RCNumVector>());
                }
            }

            /// Collect the velocities reachable from the (0, 0) vector.
            velocityGraph[velocityVectors[0, 0]].Add(velocityVectors[0, 0]);
            for (int basisVectIdx = 0; basisVectIdx < basisVectors.Count; basisVectIdx++)
            {
                velocityGraph[velocityVectors[0, 0]].Add(velocityVectors[1, basisVectIdx]);
            }

            /// Collect the velocities reachable from the longest velocities.
            for (int basisVectIdx = 0; basisVectIdx < basisVectors.Count; basisVectIdx++)
            {
                RCSet<RCNumVector> reachableVelocityList = velocityGraph[velocityVectors[accelerationDuration, basisVectIdx]];
                reachableVelocityList.Add(velocityVectors[accelerationDuration, basisVectIdx]);
                reachableVelocityList.Add(velocityVectors[accelerationDuration, (basisVectIdx + 1) % basisVectors.Count]);
                reachableVelocityList.Add(velocityVectors[accelerationDuration, (basisVectIdx - 1 + basisVectors.Count) % basisVectors.Count]);

                if (accelerationDuration == 1) { reachableVelocityList.Add(velocityVectors[0, 0]); }
                else
                {
                    reachableVelocityList.Add(velocityVectors[accelerationDuration - 1, basisVectIdx]);
                    reachableVelocityList.Add(velocityVectors[accelerationDuration - 1, (basisVectIdx + 1) % basisVectors.Count]);
                    reachableVelocityList.Add(velocityVectors[accelerationDuration - 1, (basisVectIdx - 1 + basisVectors.Count) % basisVectors.Count]);
                }
            }

            if (accelerationDuration > 1)
            {
                /// Collect the velocities reachable from the shortest velocities.
                for (int basisVectIdx = 0; basisVectIdx < basisVectors.Count; basisVectIdx++)
                {
                    RCSet<RCNumVector> reachableVelocityList = velocityGraph[velocityVectors[1, basisVectIdx]];
                    reachableVelocityList.Add(velocityVectors[0, 0]);
                    reachableVelocityList.Add(velocityVectors[1, basisVectIdx]);
                    reachableVelocityList.Add(velocityVectors[1, (basisVectIdx + 1) % basisVectors.Count]);
                    reachableVelocityList.Add(velocityVectors[1, (basisVectIdx - 1 + basisVectors.Count) % basisVectors.Count]);
                    reachableVelocityList.Add(velocityVectors[2, basisVectIdx]);
                    reachableVelocityList.Add(velocityVectors[2, (basisVectIdx + 1) % basisVectors.Count]);
                    reachableVelocityList.Add(velocityVectors[2, (basisVectIdx - 1 + basisVectors.Count) % basisVectors.Count]);
                }

                /// Collect the velocities reachable from any other velocities.
                for (int spdIdx = 2; spdIdx < accelerationDuration; spdIdx++)
                {
                    for (int basisVectIdx = 0; basisVectIdx < basisVectors.Count; basisVectIdx++)
                    {
                        RCSet<RCNumVector> reachableVelocityList = velocityGraph[velocityVectors[spdIdx, basisVectIdx]];
                        reachableVelocityList.Add(velocityVectors[spdIdx - 1, basisVectIdx]);
                        reachableVelocityList.Add(velocityVectors[spdIdx - 1, (basisVectIdx + 1) % basisVectors.Count]);
                        reachableVelocityList.Add(velocityVectors[spdIdx - 1, (basisVectIdx - 1 + basisVectors.Count) % basisVectors.Count]);
                        reachableVelocityList.Add(velocityVectors[spdIdx, basisVectIdx]);
                        reachableVelocityList.Add(velocityVectors[spdIdx, (basisVectIdx + 1) % basisVectors.Count]);
                        reachableVelocityList.Add(velocityVectors[spdIdx, (basisVectIdx - 1 + basisVectors.Count) % basisVectors.Count]);
                        reachableVelocityList.Add(velocityVectors[spdIdx + 1, basisVectIdx]);
                        reachableVelocityList.Add(velocityVectors[spdIdx + 1, (basisVectIdx + 1) % basisVectors.Count]);
                        reachableVelocityList.Add(velocityVectors[spdIdx + 1, (basisVectIdx - 1 + basisVectors.Count) % basisVectors.Count]);
                    }
                }
            }

            return velocityGraph;
        }
    }
}
