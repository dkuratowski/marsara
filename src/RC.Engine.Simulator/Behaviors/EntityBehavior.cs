using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Behaviors
{
    /// <summary>
    /// The abstract base class of the entity behaviors. An entity behavior implements additional functionalities to an entity.
    /// </summary>
    public abstract class EntityBehavior : HeapedObject
    {
        /// <summary>
        /// Perform additional update operation on the given entity.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        public virtual void UpdateState(Entity entity) { }

        /// <summary>
        /// Updates the animations of the map object of the given entity.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        public virtual void UpdateMapObject(Entity entity) { }
    }
}
