using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.Scenarios;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// The common base class of entity path tracker implementations.
    /// </summary>
    public abstract class PathTrackerBase : HeapedObject, IMotionControlEnvironment
    {
        /// <summary>
        /// Constructs a PathTrackerBase instance.
        /// </summary>
        /// <param name="controlledEntity">The entity that this path tracker controls.</param>
        public PathTrackerBase(Entity controlledEntity)
        {
            if (controlledEntity == null) { throw new ArgumentNullException("controlledEntity"); }

            this.targetPosition = this.ConstructField<RCNumVector>("targetPosition");
            this.controlledEntity = this.ConstructField<Entity>("controlledEntity");

            this.targetPosition.Write(RCNumVector.Undefined);
            this.controlledEntity.Write(controlledEntity);
        }

        /// <summary>
        /// Gets or sets the target position that this path tracker has to move its target.
        /// </summary>
        public RCNumVector TargetPosition
        {
            get { return this.targetPosition.Read(); }
            set
            {
                // TODO: implement path tracking if a new target position is set.
                this.targetPosition.Write(value);
            }
        }

        #region IMotionControlEnvironment members

        /// <see cref="IMotionControlEnvironment.PreferredVelocity"/>
        public RCNumVector PreferredVelocity
        {
            get { throw new NotImplementedException(); }
        }

        /// <see cref="IMotionControlEnvironment.DynamicObstacles"/>
        public IEnumerable<DynamicObstacleInfo> DynamicObstacles
        {
            get { throw new NotImplementedException(); }
        }

        #endregion IMotionControlEnvironment members

        /// <summary>
        /// The target position of this path tracker.
        /// </summary>
        private HeapedValue<RCNumVector> targetPosition;

        /// <summary>
        /// The entity that this path tracker controls.
        /// </summary>
        private HeapedValue<Entity> controlledEntity;
    }
}
