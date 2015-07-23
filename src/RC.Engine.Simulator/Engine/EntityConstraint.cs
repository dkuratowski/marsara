using RC.Common;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// The abstract base class of entity placement constraints.
    /// </summary>
    public abstract class EntityConstraint
    {
        /// <summary>
        /// Constructs an EntityConstraint instance.
        /// </summary>
        public EntityConstraint()
        {
            this.entityType = null;
        }

        /// <summary>
        /// Sets the entity type that this constraint belongs to.
        /// </summary>
        /// <param name="entityType">The entity type that this constraint belongs to.</param>
        /// <exception cref="SimulatorException">If a corresponding entity type has already been set for this constraint.</exception>
        public void SetEntityType(IScenarioElementType entityType)
        {
            if (entityType == null) { throw new ArgumentNullException("entityType"); }
            if (this.entityType != null) { throw new SimulatorException("Entity type has already been set for constraint!"); }
            this.entityType = entityType;
        }

        /// <summary>
        /// Checks whether this constraint allows placing an entity of the corresponding type to the given scenario at the given
        /// quadratic position and collects all the violating quadratic coordinates relative to the top-left corner of the
        /// entity.
        /// </summary>
        /// <param name="scenario">Reference to the scenario.</param>
        /// <param name="position">The position to check.</param>
        /// <returns>
        /// The list of the quadratic coordinates (relative to the top-left corner) violating the constraint.
        /// </returns>
        public RCSet<RCIntVector> Check(Scenario scenario, RCIntVector position)
        {
            if (this.entityType == null) { throw new SimulatorException("Entity type has not yet been set for constraint!"); }
            if (scenario == null) { throw new ArgumentNullException("scenario"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }

            return this.CheckImpl(scenario, position);
        }

        /// <summary>
        /// The internal implementation of the Check method that must be overriden in the derived classes.
        /// </summary>
        protected abstract RCSet<RCIntVector> CheckImpl(Scenario scenario, RCIntVector position);

        /// <summary>
        /// Gets the entity type that this constraint belongs to.
        /// </summary>
        protected IScenarioElementType EntityType { get { return this.entityType; } }

        /// <summary>
        /// Reference to the entity type that this constraint belongs to.
        /// </summary>
        private IScenarioElementType entityType;
    }
}
