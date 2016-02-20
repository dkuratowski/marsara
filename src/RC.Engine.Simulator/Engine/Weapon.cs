using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// The abstract base class of weapons of an entity.
    /// </summary>
    public abstract class Weapon : HeapedObject
    {
        /// <summary>
        /// Checks whether the given entity can be targeted by this weapon.
        /// </summary>
        /// <param name="entityToCheck">The entity to be checked.</param>
        /// <returns>True if the given entity can be targeted by this weapon; otherwise false.</returns>
        /// <remarks>Must be overriden in the derived classes.</remarks>
        public abstract bool CanTargetEntity(Entity entityToCheck);

        /// <summary>
        /// Checks whether the given target entity is in the attack range of this weapon.
        /// </summary>
        /// <param name="targetEntity">The target entity.</param>
        /// <returns>True if the given target entity is in the attack range of this weapon; otherwise false.</returns>
        public bool IsEntityInRange(Entity targetEntity)
        {
            if (!this.CanTargetEntity(targetEntity)) { return false; }

            RCNumRectangle ownerArea = this.owner.Read().Area;
            RCNumRectangle targetArea = targetEntity.Area;

            return this.IsInRange(MapUtils.ComputeDistance(ownerArea, targetArea));
        }

        /// <summary>
        /// Launches the next group of missiles to the given target entity if it's possible in the current simulation frame.
        /// </summary>
        /// <param name="targetEntity">The target entity.</param>
        /// <returns>
        /// True if the target entity is still in attack range; otherwise false.
        /// </returns>
        public bool LaunchMissiles(Entity targetEntity)
        {
            if (!this.IsEntityInRange(targetEntity)) { return false; }
            if (!this.CanLaunchMissiles()) { return true; }

            /// Launch the missiles.
            RCSet<Missile> launchedMissiles = new RCSet<Missile>();
            foreach (IMissileData missileData in this.missilesPerShot)
            {
                Missile missile = new Missile(missileData, this.owner.Read(), targetEntity);
                this.owner.Read().Scenario.AddElementToScenario(missile);
                launchedMissiles.Add(missile);
            }

            /// Subscribe to missile events and register the group of the launched missiles.
            foreach (Missile missile in launchedMissiles)
            {
                missile.LaunchCancel += this.OnMissileLaunchCancel;
                missile.Launch += this.OnMissileLaunch;
                missile.Impact += this.OnMissileImpact;
                this.missileGroups.Add(missile, launchedMissiles);
            }
            return true;
        }

        /// <summary>
        /// Gets whether this weapon has still active missiles.
        /// </summary>
        public bool HasActiveMissiles { get { return this.missileGroups.Count != 0; } }

        /// <summary>
        /// Constructs a Weapon instance.
        /// </summary>
        /// <param name="owner">Reference to the entity that this weapon belongs to.</param>
        /// <param name="missilesPerShot">The list of missiles to be launched per shot.</param>
        protected Weapon(Entity owner, IEnumerable<IMissileData> missilesPerShot)
        {
            if (owner == null) { throw new ArgumentNullException("owner"); }
            if (missilesPerShot == null) { throw new ArgumentNullException("missilesPerShot"); }

            this.owner = this.ConstructField<Entity>("owner");
            this.missilesPerShot = new List<IMissileData>(missilesPerShot);
            this.owner.Write(owner);

            this.missileGroups = new Dictionary<Missile, RCSet<Missile>>();
        }

        /// <summary>
        /// Gets the owner of this weapon.
        /// </summary>
        protected Entity Owner { get { return this.owner.Read(); } }

        /// <summary>
        /// Checks whether this weapon is currently be able to launch the next missiles.
        /// </summary>
        /// <returns>True if this weapon is currently be able to launch the next missiles; otherwise false.</returns>
        /// <remarks>Must be overriden in the derived classes.</remarks>
        protected abstract bool CanLaunchMissiles();

        /// <summary>
        /// Check whether the given distance is in the range of this weapon.
        /// </summary>
        /// <param name="distance">The distance to be checked in cells.</param>
        /// <returns>True if the given distance is in the range of this weapon.</returns>
        protected abstract bool IsInRange(RCNumber distance);

        /// <summary>
        /// This method is called when at least 1 missile of a missile group has been successfully launched.
        /// </summary>
        /// <remarks>Can be overriden in the derived classes. The default implementation does nothing.</remarks>
        protected virtual void OnLaunch()
        {
        }

        /// <summary>
        /// This method is called when at least 1 missile of a missile group has been successfully impacted.
        /// </summary>
        /// <param name="impactedMissile">Reference to the impacted missile.</param>
        /// <remarks>Can be overriden in the derived classes. The default implementation does nothing.</remarks>
        protected virtual void OnImpact(Missile impactedMissile)
        {
        }

        /// <summary>
        /// This method is called when the launch of a missile has been cancelled.
        /// </summary>
        /// <param name="missile">The missile whose launch has been cancelled.</param>
        private void OnMissileLaunchCancel(Missile missile)
        {
            /// Unsubscribe from all events of the cancelled missile.
            missile.LaunchCancel -= this.OnMissileLaunchCancel;
            missile.Launch -= this.OnMissileLaunch;
            missile.Impact -= this.OnMissileImpact;
            this.missileGroups[missile].Remove(missile);
            this.missileGroups.Remove(missile);
        }

        /// <summary>
        /// This method is called when a missile has been launched.
        /// </summary>
        /// <param name="missile">The launched missile.</param>
        private void OnMissileLaunch(Missile missile)
        {
            /// Get the group of the launched missile.
            RCSet<Missile> missileGroup = this.missileGroups[missile];
            foreach (Missile missileOfGroup in missileGroup)
            {
                /// Unsubscribe from the launch event of all missiles in that group.
                missileOfGroup.Launch -= this.OnMissileLaunch;
            }

            this.OnLaunch();
        }

        /// <summary>
        /// This method is called when a missile has impacted its target.
        /// </summary>
        /// <param name="missile">The impacted missile.</param>
        private void OnMissileImpact(Missile missile)
        {
            /// Get the group of the impacted missile.
            RCSet<Missile> missileGroup = this.missileGroups[missile];
            foreach (Missile missileOfGroup in missileGroup)
            {
                /// Unsubscribe from all events of all missiles in that group.
                missileOfGroup.LaunchCancel -= this.OnMissileLaunchCancel;
                missileOfGroup.Launch -= this.OnMissileLaunch;
                missileOfGroup.Impact -= this.OnMissileImpact;
                this.missileGroups.Remove(missileOfGroup);
            }

            /// Impact the target entity if it is still on the map.
            if (missile.TargetEntity != null && missile.TargetEntity.HasMapObject(MapObjectLayerEnum.GroundObjects, MapObjectLayerEnum.AirObjects))
            {
                this.OnImpact(missile);
            }
        }

        /// <summary>
        /// The list of missiles to be launched per shot.
        /// </summary>
        private readonly List<IMissileData> missilesPerShot;

        /// <summary>
        /// Reference to the entity that this weapon belongs to.
        /// </summary>
        private readonly HeapedValue<Entity> owner;

        /// <summary>
        /// The list of missile groups. This is used to ensure that the effect of this weapon is taken into account only once
        /// for each missile group.
        /// </summary>
        /// TODO: make this member heaped!
        private readonly Dictionary<Missile, RCSet<Missile>> missileGroups;

        /// <summary>
        /// The distance to be reached in case of nearby weapons.
        /// </summary>
        public static readonly RCNumber NEARBY_DISTANCE = 1;
    }
}
