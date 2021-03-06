﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// Represents a wreck of an entity that is responsible to play the destruction animation of the corresponding entity and
    /// to handle the weapons of the corresponding entity until they have active missiles.
    /// </summary>
    public class EntityWreck : ScenarioElement
    {
        /// <summary>
        /// Constructs an entity wreck instance for the given entity.
        /// </summary>
        /// <param name="entity">The corresponding entity.</param>
        /// <param name="destructionAnimationName">The name of the destruction animation to be played.</param>
        public EntityWreck(Entity entity, string destructionAnimationName) : base(entity.ElementType.Name)
        {
            if (destructionAnimationName == null) { throw new ArgumentNullException("destructionAnimationName"); }

            this.playerIndex = this.ConstructField<int>("playerIndex");
            this.motionControlStatus = this.ConstructField<byte>("motionControlStatus");
            this.position = this.ConstructField<RCNumVector>("position");
            this.playerIndex.Write(entity.LastOwnerIndex);
            this.weapons = entity.Armour.DetachWeapons();

            this.destructionAnimationName = destructionAnimationName;
            this.destructionMapObject = null;
            this.motionControlStatus.Write((byte)entity.MotionControl.Status);
            this.position.Write(RCNumVector.Undefined);
        }

        /// <summary>
        /// Gets the index of the player that this entity wreck belongs to or -1 if it is a wreck of a neutral entity.
        /// </summary>
        public int PlayerIndex { get { return this.playerIndex.Read(); } }

        /// <see cref="ScenarioElement.AttachToMap"/>
        public override bool AttachToMap(RCNumVector position, params ScenarioElement[] elementsToIgnore)
        {
            if (position == RCNumVector.Undefined) { throw new ArgumentNullException("position"); }

            this.position.Write(position);
            this.destructionMapObject = this.CreateMapObject(this.CalculateArea(position),
                                                             (MotionControlStatusEnum)this.motionControlStatus.Read() == MotionControlStatusEnum.OnGround ||
                                                             (MotionControlStatusEnum)this.motionControlStatus.Read() == MotionControlStatusEnum.Fixed ?
                                                                 MapObjectLayerEnum.GroundObjects :
                                                                 MapObjectLayerEnum.AirObjects);
            this.destructionMapObject.StartAnimation(this.destructionAnimationName);
            return true;
        }

        /// <see cref="ScenarioElement.DetachFromMap"/>
        public override RCNumVector DetachFromMap()
        {
            RCNumVector currentPosition = this.position.Read();
            if (this.destructionMapObject != null)
            {
                this.DestroyMapObject(this.destructionMapObject);
                this.destructionMapObject = null;
                this.position.Write(RCNumVector.Undefined);
            }
            return currentPosition;
        }

        /// <see cref="ScenarioElement.UpdateStateImpl"/>
        protected override void UpdateStateImpl()
        {
            /// Destroy the destruction map object if it finished playing its animation.
            if (!this.destructionMapObject.HasAnyAnimations)
            {
                this.DestroyMapObject(this.destructionMapObject);
            }

            /// Destroy the weapons if none of them has active missiles.
            if (this.weapons.All(weapon => !weapon.HasActiveMissiles))
            {
                foreach (Weapon weapon in this.weapons) { weapon.Dispose(); }
                this.weapons.Clear();
            }

            /// If the destruction map object has been destroyed and we have no weapons with active missile then we have finished.
            if (this.destructionMapObject.IsDestroyed && this.weapons.Count == 0)
            {
                this.Scenario.RemoveElementFromScenario(this);
            }
        }

        /// <see cref="HeapedObject.DisposeImpl"/>
        protected override void DisposeImpl()
        {
            foreach (Weapon weapon in this.weapons) { weapon.Dispose(); }
            this.weapons.Clear();
        }

        /// <summary>
        /// The list of the weapons to handle.
        /// </summary>
        /// TODO: make it heaped!
        private readonly List<Weapon> weapons;

        /// <summary>
        /// The index of the player that this entity wreck belongs to or -1 if it is a wreck of a neutral entity.
        /// </summary>
        private readonly HeapedValue<int> playerIndex;

        /// <summary>
        /// The motion control status of this entity wreck.
        /// </summary>
        private readonly HeapedValue<byte> motionControlStatus;

        /// <summary>
        /// The position of this entity wreck.
        /// </summary>
        private readonly HeapedValue<RCNumVector> position;

        /// <summary>
        /// The name of the destruction animation to be played.
        /// </summary>
        private readonly string destructionAnimationName;

        /// <summary>
        /// Reference to the map object that represents the destruction of the corresponding entity.
        /// </summary>
        private MapObject destructionMapObject;
    }
}
