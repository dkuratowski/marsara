using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.MotionControl;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// Represents a unit.
    /// </summary>
    public abstract class Unit : Entity
    {
        /// <summary>
        /// Constructs a Unit instance.
        /// </summary>
        /// <param name="unitTypeName">The name of the type of this unit.</param>
        /// <param name="behaviors">The list of behaviors of this entity.</param>
        protected Unit(string unitTypeName, params EntityBehavior[] behaviors)
            : base(unitTypeName, behaviors)
        {
            this.unitType = ComponentManager.GetInterface<IScenarioLoader>().Metadata.GetUnitType(unitTypeName);
        }

        /// <summary>
        /// Gets the metadata type definition of the unit.
        /// </summary>
        public IUnitType UnitType { get { return this.unitType; } }

        /// <summary>
        /// The type of this unit.
        /// </summary>
        private IUnitType unitType;
    }
}
