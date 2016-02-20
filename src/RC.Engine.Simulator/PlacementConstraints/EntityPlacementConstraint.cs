using RC.Common;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.PlacementConstraints
{
    /// <summary>
    /// The abstract base class of entity placement constraints.
    /// </summary>
    public abstract class EntityPlacementConstraint
    {
        /// <summary>
        /// Sets the entity type that this constraint belongs to.
        /// </summary>
        /// <param name="entityType">The entity type that this constraint belongs to.</param>
        /// <exception cref="SimulatorException">If a corresponding entity type has already been set for this constraint.</exception>
        public void SetEntityType(IScenarioElementType entityType)
        {
            if (entityType == null) { throw new ArgumentNullException("entityType"); }
            if (this.entityType != null) { throw new SimulatorException("Entity type has already been set for this constraint!"); }
            this.entityType = entityType;
        }

        /// <summary>
        /// Checks whether this placement constraint allows placing an entity of the corresponding type to the given scenario at the given
        /// quadratic position and collects all the violating quadratic coordinates relative to the given position.
        /// </summary>
        /// <param name="scenario">Reference to the given scenario.</param>
        /// <param name="position">The position to be checked.</param>
        /// <param name="entitiesToIgnore">
        /// The list of entities to be ignored during the check. All entities in this list shall belong to the given scenario.
        /// </param>
        /// <returns>
        /// The list of the quadratic coordinates (relative to the given position) violating this placement constraint.
        /// </returns>
        public RCSet<RCIntVector> Check(Scenario scenario, RCIntVector position, RCSet<Entity> entitiesToIgnore)
        {
            if (this.entityType == null) { throw new SimulatorException("Entity type has not yet been set for this constraint!"); }
            if (scenario == null) { throw new ArgumentNullException("scenario"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }
            if (entitiesToIgnore == null) { throw new ArgumentNullException("entitiesToIgnore"); }
            if (entitiesToIgnore.Any(entityToIgnore => entityToIgnore.Scenario != scenario)) { throw new ArgumentException("All entities to be ignored shall belong to the given scenario!", "entitiesToIgnore"); }
            
            return this.CheckImpl(scenario, position, entitiesToIgnore);
        }

        /// <summary>
        /// Constructs an EntityPlacementConstraint instance.
        /// </summary>
        protected EntityPlacementConstraint()
        {
            this.entityType = null;
        }

        /// <summary>
        /// Checks whether this placement constraint allows placing an entity of the corresponding type to the given scenario at the given
        /// quadratic position and collects all the violating quadratic coordinates relative to the given position.
        /// </summary>
        /// <param name="scenario">Reference to the given scenario.</param>
        /// <param name="position">The position to be checked.</param>
        /// <param name="entitiesToIgnore">The list of entities to be ignored during the check.</param>
        /// <returns>
        /// The list of the quadratic coordinates (relative to the given position) violating this placement constraint.
        /// </returns>
        /// <remarks>
        /// This method shall be implemented in the derived classes.
        /// Note for the implementors: it is guaranteed that if entitiesToIgnore is not empty then their scenario will always equal
        ///                            to the given scenario.
        /// </remarks>
        protected abstract RCSet<RCIntVector> CheckImpl(Scenario scenario, RCIntVector position, RCSet<Entity> entitiesToIgnore);

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
