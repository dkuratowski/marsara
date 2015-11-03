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
    public abstract class EntityPlacementConstraint
    {
        /// <summary>
        /// Constructs an EntityPlacementConstraint instance.
        /// </summary>
        public EntityPlacementConstraint()
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
        /// Checks whether this placement constraint allows placing an entity of the corresponding type to the given scenario at the given
        /// quadratic position and collects all the violating quadratic coordinates relative to the given position.
        /// </summary>
        /// <param name="scenario">Reference to the given scenario.</param>
        /// <param name="position">The position to be checked.</param>
        /// <returns>
        /// The list of the quadratic coordinates (relative to the given position) violating this placement constraint.
        /// </returns>
        public RCSet<RCIntVector> Check(Scenario scenario, RCIntVector position)
        {
            if (this.entityType == null) { throw new SimulatorException("Entity type has not yet been set for constraint!"); }
            if (scenario == null) { throw new ArgumentNullException("scenario"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }

            return this.CheckImpl(scenario, position, null);
        }

        /// <summary>
        /// Checks whether this placement constraint allows placing the given entity to its scenario at the given
        /// quadratic position and collects all the violating quadratic coordinates relative to the given position.
        /// </summary>
        /// <param name="entity">Reference to the entity to be checked.</param>
        /// <param name="position">The position to be checked.</param>
        /// <returns>
        /// The list of the quadratic coordinates (relative to the given position) violating this placement constraint.
        /// </returns>
        public RCSet<RCIntVector> Check(Entity entity, RCIntVector position)
        {
            if (this.entityType == null) { throw new SimulatorException("Entity type has not yet been set for constraint!"); }
            if (entity == null) { throw new ArgumentNullException("entity"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }
            if (entity.Scenario == null) { throw new ArgumentException("The given entity is not added to a scenario!", "entity"); }

            return this.CheckImpl(entity.Scenario, position, entity);
        }

        /// <summary>
        /// Checks whether this placement constraint allows placing an entity of the corresponding type to the given scenario at the given
        /// quadratic position and collects all the violating quadratic coordinates relative to the given position.
        /// </summary>
        /// <param name="scenario">Reference to the given scenario.</param>
        /// <param name="position">The position to be checked.</param>
        /// <param name="entityNotToConsider">An optional entity considered not to be attached to the map during the check.</param>
        /// <returns>
        /// The list of the quadratic coordinates (relative to the given position) violating this placement constraint.
        /// </returns>
        /// <remarks>
        /// This method shall be implemented in the derived classes.
        /// Note for the implementors: it is guaranteed that if the given entity is not null then its scenario will always equal to the given scenario.
        /// </remarks>
        protected abstract RCSet<RCIntVector> CheckImpl(Scenario scenario, RCIntVector position, Entity entityNotToConsider);

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
