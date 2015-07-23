using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// The common base class of velocity graphs. The nodes of a velocity graph are velocities and there is an edge from velocity A
    /// to velocity B if and only if B is admissible from A.
    /// </summary>
    public abstract class VelocityGraph
    {
        /// <summary>
        /// Gets the list of all velocities that are admissible from the given velocity.
        /// </summary>
        /// <param name="currentVelocity">The given velocity.</param>
        /// <returns>All velocities that are admissible from the given velocity.</returns>
        public IEnumerable<RCNumVector> GetAdmissibleVelocities(RCNumVector currentVelocity)
        {
            if (!this.velocityGraph.ContainsKey(currentVelocity))
            {
                /// Search the velocity that has a minimum difference to the current velocity.
                RCNumber minDiff = 0;
                foreach (RCNumVector velocity in this.velocityGraph.Keys)
                {
                    RCNumber diff = MapUtils.ComputeDistance(currentVelocity, velocity);
                    if (minDiff == 0 || diff < minDiff)
                    {
                        minDiff = diff;
                        currentVelocity = velocity;
                    }
                }
            }

            return new List<RCNumVector>(this.velocityGraph[currentVelocity]);
        }

        /// <summary>
        /// Constructs a VelocityGraph instance.
        /// </summary>
        /// <param name="velocityGraph">
        /// The dictionary that represents the velocity graph.
        /// The keys of this dictionary contains all the possible velocities. The values of this dictionary are the sets of
        /// the velocities admissible from the velocity stored by corresponding key. Each velocity in one of those
        /// lists must either be present as a key in this dictionary. The (0;0) vector must be present in this
        /// dictionary as a key.
        /// </param>
        protected VelocityGraph(Dictionary<RCNumVector, RCSet<RCNumVector>> velocityGraph)
        {
            if (velocityGraph == null) { throw new ArgumentNullException("velocityGraph"); }
            if (!velocityGraph.ContainsKey(new RCNumVector(0, 0))) { throw new ArgumentException("Velocity graph must contain the (0;0) vector as a key!", "velocityGraph"); }

            /// Check and save the new velocity graph.
            this.velocityGraph = new Dictionary<RCNumVector, RCSet<RCNumVector>>();
            foreach (KeyValuePair<RCNumVector, RCSet<RCNumVector>> item in velocityGraph)
            {
                if (item.Value.Any(velocity => !velocityGraph.ContainsKey(velocity))) { throw new ArgumentException("Each velocity in the admissibility lists must either be present as a key in the velocity graph dictionary!", "velocityGraph"); }
                this.velocityGraph.Add(item.Key, new RCSet<RCNumVector>(item.Value));
            }
        }

        /// <summary>
        /// The dictionary that represents the velocity graph. See the comment of the constructor for more informations.
        /// </summary>
        private readonly Dictionary<RCNumVector, RCSet<RCNumVector>> velocityGraph;
    }
}
