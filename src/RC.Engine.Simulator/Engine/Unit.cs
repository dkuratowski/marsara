using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.Behaviors;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.MotionControl;
using RC.Engine.Simulator.Metadata.Core;

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
        /// <param name="isFlying">A flag indicating whether this entity is initially flying.</param>
        /// <param name="behaviors">The list of behaviors of this entity.</param>
        protected Unit(string unitTypeName, bool isFlying, params EntityBehavior[] behaviors)
            : base(unitTypeName, isFlying, behaviors)
        {
            this.unitType = new IUnitType(this.ElementType.ElementTypeImpl as IUnitTypeInternal);
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
