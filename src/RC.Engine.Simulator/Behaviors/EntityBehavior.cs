using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
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

        /// <summary>
        /// Helper method for stop a set of animations and then start another set of animations of the given entity in one step.
        /// Animations in the intersection of the given sets won't be touched.
        /// </summary>
        /// <param name="animationsToStop">The set of animations to stop.</param>
        /// <param name="animationsToStart">The set of animations to start.</param>
        /// <param name="headingVectors">The heading vectors in priority order.</param>
        protected void StopStartAnimations(Entity entity, RCSet<string> animationsToStop, RCSet<string> animationsToStart, params IValueRead<RCNumVector>[] headingVectors)
        {
            foreach (string animationToStop in animationsToStop)
            {
                if (!animationsToStart.Contains(animationToStop)) { entity.MapObject.StopAnimation(animationToStop); }
            }
            foreach (string animationToStart in animationsToStart)
            {
                entity.MapObject.StartAnimation(animationToStart, headingVectors);
            }
        }
    }
}
