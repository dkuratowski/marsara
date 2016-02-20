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
            this.attackingCustomWeapon = this.ConstructField<Weapon>("attackingCustomWeapon");
            this.targetVector = this.ConstructField<RCNumVector>("targetVector");
            this.standardWeapons = new List<Weapon>();
            this.customWeapons = new Dictionary<string, CustomWeaponStub>();

            this.owner.Write(owner);
            this.target.Write(null);
            this.attackingCustomWeapon.Write(null);
            this.targetVector.Write(new RCNumVector(0, 0));

            /// Create the standard weapons.
            foreach (IWeaponData standardWeapon in this.owner.Read().ElementType.StandardWeapons)
            {
                this.standardWeapons.Add(new StandardWeapon(owner, standardWeapon));
            }

            /// Create the stubs for the custom weapons.
            foreach (IWeaponData customWeapon in this.owner.Read().ElementType.CustomWeapons)
            {
                this.customWeapons.Add(customWeapon.Name, new CustomWeaponStub(owner, customWeapon));
            }
        }

        /// <summary>
        /// Attaches the given custom weapon to this armour.
        /// </summary>
        /// <param name="name">The name of the custom weapon to be attached.</param>
        /// <param name="customWeapon">The curtom weapon to be attached.</param>
        public void AttachCustomWeapon(string name, CustomWeapon customWeapon)
        {
            if (name == null) { throw new ArgumentNullException("name"); }
            if (customWeapon == null) { throw new ArgumentNullException("customWeapon"); }
            if (!this.customWeapons.ContainsKey(name)) { throw new InvalidOperationException(string.Format("Custom weapon with name '{0}' is not defined for scenario element type '{1}'!", name, this.owner.Read().ElementType.Name)); }

            this.customWeapons[name].AttachWeapon(customWeapon);
        }

        /// <summary>
        /// Selects an enemy that can be attacked by the owner.
        /// </summary>
        /// <returns>An enemy that can be attacked by the owner or null if no such enemy has been found.</returns>
        public Entity SelectEnemyForStandardWeapons()
        {
            if (this.standardWeapons.Count == 0)
            {
                /// The owner has no standard weapons -> no enemy can be attacked.
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
                    RCNumber distance = MapUtils.ComputeDistance(this.owner.Read().Area, locatedEntity.Area);
                    if (distance < nearestEnemyDistance) { nearestEnemy = locatedEntity; }
                }
            }

            return nearestEnemy;
        }

        /// <summary>
        /// Starts attacking the given target entity with the standard weapons if its in attack range. Otherwise this function has no effect.
        /// </summary>
        /// <param name="targetID">The ID of the target entity.</param>
        public void StartAttack(int targetID)
        {
            /// Check if target entity is still on the map.
            Entity targetEntity = this.owner.Read().Scenario.GetElementOnMap<Entity>(targetID, MapObjectLayerEnum.GroundObjects, MapObjectLayerEnum.AirObjects);
            if (targetEntity == null) { return; }

            /// Check if target entity is in attack range.
            foreach (Weapon weapon in this.standardWeapons)
            {
                if (weapon.IsEntityInRange(targetEntity))
                {
                    this.target.Write(targetEntity);
                    this.attackingCustomWeapon.Write(null);
                    this.targetVector.Write(targetEntity.MotionControl.PositionVector.Read() - this.owner.Read().MotionControl.PositionVector.Read());
                    return;
                }
            }

            /// Target entity is out of attack range.
            this.target.Write(null);
            this.attackingCustomWeapon.Write(null);
        }

        /// <summary>
        /// Starts attacking the given target entity with the given custom weapon if its in attack range. Otherwise this function has no effect.
        /// </summary>
        /// <param name="targetID">The ID of the target entity.</param>
        /// <param name="customWeaponName">The name of the custom weapon to be used for the attack.</param>
        public void StartAttack(int targetID, string customWeaponName)
        {
            /// Check if the given custom weapon exists.
            if (!this.customWeapons.ContainsKey(customWeaponName)) { throw new ArgumentException(string.Format("Custom weapon '{0}' doesn't exists!", customWeaponName), "customWeaponName"); }
            
            /// Check if target entity is still on the map.
            Entity targetEntity = this.owner.Read().Scenario.GetElementOnMap<Entity>(targetID, MapObjectLayerEnum.GroundObjects, MapObjectLayerEnum.AirObjects);
            if (targetEntity == null) { return; }

            /// Check if the target entity is in attack range for the given weapon.
            Weapon customWeapon = this.customWeapons[customWeaponName];
            if (customWeapon.IsEntityInRange(targetEntity))
            {
                this.target.Write(targetEntity);
                this.attackingCustomWeapon.Write(customWeapon);
                this.targetVector.Write(targetEntity.MotionControl.PositionVector.Read() - this.owner.Read().MotionControl.PositionVector.Read());
                return;
            }

            /// Target entity is out of attack range for the given weapon.
            this.target.Write(null);
            this.attackingCustomWeapon.Write(null);
        }

        /// <summary>
        /// Continues to attack the given target entity with the standard weapons if its still in attack range.
        /// </summary>
        public void ContinueAttack()
        {
            if (this.target.Read() == null) { return; }

            bool isTargetStillInRange = false;
            if (this.target.Read().HasMapObject(MapObjectLayerEnum.GroundObjects, MapObjectLayerEnum.AirObjects))
            {
                this.targetVector.Write(this.target.Read().MotionControl.PositionVector.Read() -
                                        this.owner.Read().MotionControl.PositionVector.Read());

                if (this.attackingCustomWeapon.Read() != null)
                {
                    /// Attacking with a custom weapon.
                    if (this.attackingCustomWeapon.Read().LaunchMissiles(this.target.Read())) { isTargetStillInRange = true; }
                }
                else
                {
                    /// Attacking with standard weapons.
                    foreach (Weapon weapon in this.standardWeapons)
                    {
                        if (weapon.LaunchMissiles(this.target.Read())) { isTargetStillInRange = true; }
                    }
                }
            }

            if (!isTargetStillInRange)
            {
                this.target.Write(null);
                this.attackingCustomWeapon.Write(null);
            }
        }

        /// <summary>
        /// Stops attacking the target entity currently being attacked. If there is no entity currently being attacked then this function
        /// has no effect.
        /// </summary>
        public void StopAttack()
        {
            this.target.Write(null);
            this.attackingCustomWeapon.Write(null);
        }

        /// <summary>
        /// Detaches all the weapons from this armour.
        /// </summary>
        /// <returns>The list of the detached weapons.</returns>
        public List<Weapon> DetachWeapons()
        {
            List<Weapon> retList = new List<Weapon>(this.standardWeapons);
            retList.AddRange(this.customWeapons.Values);
            this.standardWeapons.Clear();
            this.customWeapons.Clear();
            return retList;
        }

        /// <summary>
        /// Gets the current target or null if there is no target currently.
        /// </summary>
        public Entity Target { get { return this.target.Read(); } }

        /// <summary>
        /// Gets the current target vector or RCNumVector.Undefined of there is no target currently.
        /// </summary>
        public IValueRead<RCNumVector> TargetVector { get { return this.targetVector; } }

        /// <see cref="HeapedObject.DisposeImpl"/>
        protected override void DisposeImpl()
        {
            foreach (Weapon standardWeapon in this.standardWeapons) { standardWeapon.Dispose(); }
            foreach (Weapon customWeapon in this.customWeapons.Values) { customWeapon.Dispose(); }
            this.standardWeapons.Clear();
            this.customWeapons.Clear();
        }

        /// <summary>
        /// Reference to the owner of this armour.
        /// </summary>
        private readonly HeapedValue<Entity> owner;

        /// <summary>
        /// Reference to the current target or null if there is no target currently.
        /// </summary>
        private readonly HeapedValue<Entity> target;

        /// <summary>
        /// Reference to the custom weapon that is currently used to attack the current target or null if there is no target currently.
        /// </summary>
        private readonly HeapedValue<Weapon> attackingCustomWeapon;

        /// <summary>
        /// The current target vector or RCNumVector.Undefined if there is no target currently.
        /// </summary>
        private readonly HeapedValue<RCNumVector> targetVector;

        /// <summary>
        /// The list of the standard weapons of the owner entity.
        /// </summary>
        /// TODO: make it heaped!
        private readonly List<Weapon> standardWeapons;

        /// <summary>
        /// The list of the custom weapons of the owner entity mapped by their name.
        /// </summary>
        /// TODO: make it heaped!
        private readonly Dictionary<string, CustomWeaponStub> customWeapons;
    }
}
