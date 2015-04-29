using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Scenarios
{
    /// <summary>
    /// Responsible for handling the weapons and any battle related data of a given entity.
    /// </summary>
    public class Armour : HeapedObject
    {
        /// <summary>
        /// Constructs an armour instance for the given entity.
        /// </summary>
        /// <param name="owner">The owner of this armour.</param>
        public Armour(Entity owner)
        {
            if (owner == null) { throw new ArgumentNullException("owner"); }

            this.owner = this.ConstructField<Entity>("owner");
            this.target = this.ConstructField<Entity>("target");
            this.owner.Write(owner);
            this.target.Write(null);
        }

        /// <summary>
        /// Selects an enemy that can be attacked by the owner.
        /// </summary>
        /// <returns>An enemy that can be attacked by the owner or null if no such enemy has been found.</returns>
        public Entity SelectEnemy()
        {
            /// TODO: check the real weapons of the owner in the future instead of weapon metadata.
            IWeaponData airWeapon = this.owner.Read().ElementType.AirWeapon;
            IWeaponData groundWeapon = this.owner.Read().ElementType.GroundWeapon;
            if (airWeapon == null && groundWeapon == null)
            {
                /// The owner has no weapons -> no enemy can be attacked.
                return null;
            }

            /// Select the nearest enemy.
            RCNumber nearestEnemyDistance = 0;
            Entity nearestEnemy = null;
            foreach (Entity locatedEntity in this.owner.Read().Locator.LocateEntities())
            {
                if (locatedEntity.Owner == null || locatedEntity.Owner == this.owner.Read().Owner) { continue; }
                if (locatedEntity.IsFlying && airWeapon == null) { continue; }
                if (!locatedEntity.IsFlying && groundWeapon == null) { continue; }

                if (nearestEnemy == null) { nearestEnemy = locatedEntity; }
                else
                {
                    RCNumber distance = MapUtils.ComputeDistance(this.owner.Read().BoundingBox, locatedEntity.BoundingBox);
                    if (distance < nearestEnemyDistance) { nearestEnemy = locatedEntity; }
                }
            }

            return nearestEnemy;
        }

        /// <summary>
        /// Starts attacking the given target entity if its in attack range.
        /// </summary>
        /// <param name="targetID">The ID of the target entity.</param>
        public void StartAttack(int targetID)
        {
        }

        /// <summary>
        /// Stops attacking the target entity currently being attacked.
        /// </summary>
        public void StopAttack()
        {
            
        }

        /// <summary>
        /// Gets the current target or null if there is no target currently.
        /// </summary>
        public Entity Target { get { return this.target.Read(); } }

        /// <summary>
        /// Reference to the owner of this armour.
        /// </summary>
        private readonly HeapedValue<Entity> owner;

        /// <summary>
        /// Reference to the current target or null if there is no target currently.
        /// </summary>
        private readonly HeapedValue<Entity> target;
    }
}
