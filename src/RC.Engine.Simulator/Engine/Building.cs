using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Metadata;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// Represents a building.
    /// </summary>
    public abstract class Building : QuadEntity
    {
        /// <summary>
        /// Constructs a Building instance.
        /// </summary>
        /// <param name="buildingTypeName">The name of the type of this building.</param>
        /// <param name="behaviors">The list of behaviors of this entity.</param>
        protected Building(string buildingTypeName, params EntityBehavior[] behaviors)
            : base(buildingTypeName, behaviors)
        {
            this.buildingType = ComponentManager.GetInterface<IScenarioLoader>().Metadata.GetBuildingType(buildingTypeName);
        }

        /// <summary>
        /// The type of this building.
        /// </summary>
        private IBuildingType buildingType;
    }
}
