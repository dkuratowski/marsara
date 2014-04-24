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
    /// The common base class of entity actuator implementations.
    /// </summary>
    /// TODO: call Dispose from the corresponding Entity instance when necessary!
    public abstract class EntityActuatorBase : IMotionControlActuator, IDisposable
    {
        /// <summary>
        /// Constructs an EntityActuatorBase instance.
        /// </summary>
        /// <param name="velocityValue">Reference to the velocity value that this actuator has to manipulate.</param>
        public EntityActuatorBase(IValue<RCNumVector> velocityValue)
        {
            if (velocityValue == null) { throw new ArgumentNullException("velocityValue"); }

            this.velocityGraph = new Dictionary<RCNumVector, List<RCNumVector>>()
            {
                { new RCNumVector(0, 0), new List<RCNumVector>() { new RCNumVector(0, 0) } }
            };
            this.velocityValue = velocityValue;
        }

        #region IMotionControlActuator methods

        /// <see cref="IMotionControlActuator.SelectNewVelocity"/>
        public void SelectNewVelocity(int selectedVelocityIndex)
        {
            if (!this.velocityGraph.ContainsKey(this.velocityValue.Read())) { throw new InvalidOperationException("Current velocity is non-admissible!"); }

            List<RCNumVector> reachableVelocities = this.velocityGraph[this.velocityValue.Read()];
            if (selectedVelocityIndex < 0 || selectedVelocityIndex >= reachableVelocities.Count) { throw new ArgumentOutOfRangeException("selectedVelocityIndex"); }
            this.velocityValue.Write(reachableVelocities[selectedVelocityIndex]);
        }

        /// <see cref="IMotionControlActuator.AdmissibleVelocities"/>
        public IEnumerable<RCNumVector> AdmissibleVelocities
        {
            get
            {
                if (!this.velocityGraph.ContainsKey(this.velocityValue.Read())) { throw new InvalidOperationException("Current velocity is non-admissible!"); }
                return new List<RCNumVector>(this.velocityGraph[this.velocityValue.Read()]);
            }
        }

        #endregion IMotionControlActuator methods

        #region IDisposable methods

        /// <see cref="IDisposable.Dispose"/>
        public virtual void Dispose() { }

        #endregion IDisposable methods

        /// <summary>
        /// Updates the velocity graph of this actuator.
        /// </summary>
        /// <param name="velocityGraph">
        /// The velocity graph that contains the admissible velocities and the reachability between them.
        /// The keys of this dictionary are the admissible velocities. The values of this dictionary are the lists
        /// the velocities reachable from the velocity stored by corresponding key. Each velocity in one of those
        /// lists must either be present as a key in this dictionary. The (0;0) vector must be present in this
        /// dictionary as a key.
        /// </param>
        protected void UpdateVelocityGraph(Dictionary<RCNumVector, HashSet<RCNumVector>> velocityGraph)
        {
            if (velocityGraph == null) { throw new ArgumentNullException("velocityGraph"); }
            if (!velocityGraph.ContainsKey(new RCNumVector(0, 0))) { throw new ArgumentException("Velocity graph must contain the (0;0) vector as a key!", "velocityGraph"); }

            /// Check and save the new velocity graph and select the velocity out of this graph whose difference to the
            /// current velocity is minimal.
            RCNumber minDiff = 0;
            RCNumVector minDiffVelocity = RCNumVector.Undefined;
            RCNumVector currentVelocity = this.velocityValue.Read();
            this.velocityGraph = new Dictionary<RCNumVector, List<RCNumVector>>();
            foreach (KeyValuePair<RCNumVector, HashSet<RCNumVector>> item in velocityGraph)
            {
                if (item.Value.Any(velocity => !velocityGraph.ContainsKey(velocity))) { throw new ArgumentException("Each velocity in the reachability lists must either be present as a key in the velocity graph dictionary!", "velocityGraph"); }
                this.velocityGraph.Add(item.Key, new List<RCNumVector>(item.Value));

                RCNumber diff = MapUtils.ComputeDistance(currentVelocity, item.Key);
                if (minDiffVelocity == RCNumVector.Undefined || diff < minDiff)
                {
                    minDiff = diff;
                    minDiffVelocity = item.Key;
                }
            }

            /// Change the current velocity to the selected velocity.
            this.velocityValue.Write(minDiffVelocity);
        }

        /// <summary>
        /// Reference to the velocity value that this actuator has to manipulate.
        /// </summary>
        private IValue<RCNumVector> velocityValue;

        /// <summary>
        /// The velocity graph that contains the admissible velocities and the reachability between them.
        /// See the comment of the constructor for more informations.
        /// </summary>
        private Dictionary<RCNumVector, List<RCNumVector>> velocityGraph;
    }
}
