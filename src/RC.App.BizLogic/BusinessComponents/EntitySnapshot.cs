using System;
using System.Collections.Generic;
using RC.App.BizLogic.BusinessComponents.Core;
using RC.App.BizLogic.Views;
using RC.Common;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.App.BizLogic.BusinessComponents
{
    /// <summary>
    /// Represents the snapshot of an Entity at a timepoint.
    /// </summary>
    class EntitySnapshot
    {
        /// <summary>
        /// Creates a snapshot of the given source Entity at the current simulation frame of the Scenario it belongs to.
        /// </summary>
        /// <param name="sourceEntity">The source Entity of this snapshot.</param>
        /// <exception cref="ArgumentException">If the source Entity doesn't belong to a Scenario.</exception>
        public EntitySnapshot(Entity sourceEntity)
        {
            if (sourceEntity == null) { throw new ArgumentNullException("sourceEntity"); }
            if (sourceEntity.Scenario == null) { throw new ArgumentException("The source Entity doesn't belong to a scenario!"); }
            if (!sourceEntity.HasMapObject) { throw new ArgumentException("The source Entity is not attached to the map!"); }

            /// Copy some properties of the source entity.
            this.id = sourceEntity.ID.Read();
            this.timeStamp = sourceEntity.Scenario.CurrentFrameIndex;
            this.position = sourceEntity.Position;
            this.entityType = sourceEntity.ElementType;
            this.quadraticPosition = sourceEntity.MapObject.QuadraticPosition;

            /// Save the current animation frame of the source entity.
            List<int> spriteIndices = new List<int>();
            foreach (AnimationPlayer animation in sourceEntity.MapObject.CurrentAnimations) { spriteIndices.AddRange(animation.CurrentFrame); }
            this.animationFrame = spriteIndices.ToArray();

            /// Save the owner player of the source entity.
            this.owner = BizLogicHelpers.GetMapObjectOwner(sourceEntity.MapObject);
        }

        #region Public members

        /// <summary>
        /// Gets the ID of the source Entity.
        /// </summary>
        public int ID { get { return this.id; } }

        /// <summary>
        /// Gets the index of the simulation frame when this snapshot has been created.
        /// </summary>
        public int TimeStamp { get { return this.timeStamp; } }

        /// <summary>
        /// Gets the position of the source Entity at the timepoint when this snapshot has been created.
        /// </summary>
        public RCNumRectangle Position { get { return this.position; } }

        /// <summary>
        /// Gets the quadratic position of the source Entity at the timepoint when this snapshot has been created.
        /// </summary>
        public RCIntRectangle QuadraticPosition { get { return this.quadraticPosition; } }

        /// <summary>
        /// Gets the sprite indices of the frame of the source Entity at the timepoint when this snapshot has been created.
        /// </summary>
        public int[] AnimationFrame { get { return this.animationFrame; } }

        /// <summary>
        /// Gets the owner of the source Entity at the timepoint when this snapshot has been created.
        /// </summary>
        public PlayerEnum Owner { get { return this.owner; } }

        /// <summary>
        /// Gets the type of the source Entity.
        /// </summary>
        public IScenarioElementType EntityType { get { return this.entityType; } }

        #endregion Public members

        #region Private members

        /// <summary>
        /// The ID of the source Entity.
        /// </summary>
        private int id;

        /// <summary>
        /// The index of the simulation frame when this snapshot has been created.
        /// </summary>
        private int timeStamp;

        /// <summary>
        /// The position of the source Entity at the timepoint when this snapshot has been created.
        /// </summary>
        private RCNumRectangle position;

        /// <summary>
        /// The quadratic position of the source Entity at the timepoint when this snapshot has been created.
        /// </summary>
        private RCIntRectangle quadraticPosition;

        /// <summary>
        /// The sprite indices of the frame of the source Entity at the timepoint when this snapshot has been created.
        /// </summary>
        private int[] animationFrame;

        /// <summary>
        /// The owner of the source Entity at the timepoint when this snapshot has been created.
        /// </summary>
        private PlayerEnum owner;

        /// <summary>
        /// The type of the source Entity.
        /// </summary>
        private IScenarioElementType entityType;

        #endregion Private members
    }
}
