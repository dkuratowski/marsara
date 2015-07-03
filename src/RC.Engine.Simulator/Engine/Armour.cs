using System;
using System.Collections.Generic;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Engine
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
            this.standardWeapons = new List<Weapon>();

            this.owner.Write(owner);
            this.target.Write(null);
            foreach (IWeaponData weapon in this.owner.Read().ElementType.StandardWeapons)
            {
                this.standardWeapons.Add(new StandardWeapon(weapon));
            }
        }

        /// <summary>
        /// Selects an enemy that can be attacked by the owner.
        /// </summary>
        /// <returns>An enemy that can be attacked by the owner or null if no such enemy has been found.</returns>
        public Entity SelectEnemy()
        {
            if (this.standardWeapons.Count == 0)
            {
                /// The owner has no weapons -> no enemy can be attacked.
                return null;
            }

            /// Select the nearest enemy.
            RCNumber nearestEnemyDistance = 0;
            Entity nearestEnemy = null;
            foreach (Entity locatedEntity in this.owner.Read().Locator.LocateEntities())
            {
                /// Check if the located entity is an enemy or not.
                if (locatedEntity.Owner == null || locatedEntity.Owner == this.owner.Read().Owner) { continue; }

                /// Check if any of the standard weapons can target the located entity.
                bool hasWeaponCanTargetEntity = false;
                foreach (Weapon standardWeapon in this.standardWeapons)
                {
                    if (standardWeapon.CanTargetEntity(locatedEntity))
                    {
                        hasWeaponCanTargetEntity = true;
                        break;
                    }
                }
                if (!hasWeaponCanTargetEntity)
                {
                    /// The owner has no weapons that can attack the located enemy -> continue with the next located enemy.
                    continue;
                }

                if (nearestEnemy == null) { nearestEnemy = locatedEntity; }
                else
                {
                    RCNumber distance = MapUtils.ComputeDistance(this.owner.Read().Position, locatedEntity.Position);
                    if (distance < nearestEnemyDistance) { nearestEnemy = locatedEntity; }
                }
            }

            return nearestEnemy;
        }

        /// <summary>
        /// Starts attacking the given target entity with a standard weapon if its in attack range.
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

        /// <summary>
        /// The list of the standard weapons of the owner entity.
        /// </summary>
        /// TODO: make it heaped!
        private readonly List<Weapon> standardWeapons;
    }
}
