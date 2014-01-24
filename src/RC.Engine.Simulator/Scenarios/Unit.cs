﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.ComponentInterfaces;

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
        public Unit(string unitTypeName)
            : base(unitTypeName)
        {
            this.unitType = ComponentManager.GetInterface<IScenarioLoader>().Metadata.GetUnitType(unitTypeName);
        }

        /// <summary>
        /// Adds this unit to the map.
        /// </summary>
        /// <param name="position">The position of this unit on the map.</param>
        public void AddToMap(RCNumVector position)
        {
            this.SetPosition(position);
            this.Scenario.VisibleEntities.AttachContent(this);
        }

        /// <summary>
        /// Removes this unit from the map.
        /// </summary>
        public void RemoveFromMap()
        {
            this.Scenario.VisibleEntities.DetachContent(this);
        }

        /// <summary>
        /// The type of this unit.
        /// </summary>
        private IUnitType unitType;
    }
}