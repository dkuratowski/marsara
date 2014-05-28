using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.MotionControl;

namespace RC.Engine.Simulator.Scenarios
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
        /// <param name="actuator">The actuator of the unit.</param>
        /// <param name="pathTracker">The path-tracker of the unit.</param>
        public Unit(string unitTypeName)
            : base(unitTypeName)
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
