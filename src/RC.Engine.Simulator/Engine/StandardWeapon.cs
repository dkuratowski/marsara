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
    /// Represents a standard weapon of an entity.
    /// </summary>
    public class StandardWeapon : Weapon
    {
        /// <summary>
        /// Constructs a StandardWeapon instance.
        /// </summary>
        /// <param name="owner">Reference to the entity that this standard weapon belongs to.</param>
        /// <param name="weaponData">The definition of the weapon from the metadata.</param>
        public StandardWeapon(Entity owner, IWeaponData weaponData)
            : base(owner, weaponData.Missiles)
        {
            if (weaponData == null) { throw new ArgumentNullException("weaponData"); }
            this.weaponData = weaponData;

            this.frameIndexOfLastLaunch = this.ConstructField<int>("frameIndexOfLastLaunch");
            this.frameIndexOfLastLaunch.Write(-1);
        }

        /// <see cref="Weapon.CanTargetEntity"/>
        public override bool CanTargetEntity(Entity entityToCheck)
        {
            if (entityToCheck.ElementType.MaxHP == null) { return false; }  /// A standard weapon cannot attack entities with no HPs.
            if (entityToCheck == this.Owner) { return false; }              /// A standard weapon cannot attack its owner.

            /// Check the type of this weapon against the entity's current flying status.
            return (entityToCheck.MotionControl.IsFlying && this.weaponData.WeaponType.Read() == WeaponTypeEnum.Air) ||
                   (!entityToCheck.MotionControl.IsFlying && this.weaponData.WeaponType.Read() == WeaponTypeEnum.Ground);
        }

        /// <see cref="Weapon.CanLaunchMissiles"/>
        protected override bool CanLaunchMissiles()
        {
            int currentFrameIndex = this.Owner.Scenario.CurrentFrameIndex;
            if (this.frameIndexOfLastLaunch.Read() == -1 ||
                currentFrameIndex - this.frameIndexOfLastLaunch.Read() >= this.weaponData.Cooldown.Read())
            {
                this.frameIndexOfLastLaunch.Write(currentFrameIndex);
                return true;
            }
            return false;
        }

        /// <see cref="Weapon.IsInRange"/>
        protected override bool IsInRange(RCNumber quadDistance)
        {
            return quadDistance >= this.weaponData.RangeMin.Read() && quadDistance <= this.weaponData.RangeMax.Read();
        }

        /// <see cref="Weapon.OnImpact"/>
        protected override void OnImpact(Missile impactedMissile)
        {
            IQuadTile launchQuadTile = impactedMissile.Scenario.Map.GetCell(impactedMissile.LaunchPosition.Round()).ParentQuadTile;
            IQuadTile impactQuadTile = impactedMissile.Scenario.Map.GetCell(impactedMissile.TargetEntity.MotionControl.PositionVector.Read().Round()).ParentQuadTile;

            /// TODO: Don't use the default random generator here because scenario update needs to be deterministic!
            bool makeDamage = impactedMissile.LaunchedFromAir ||
                              impactedMissile.TargetEntity.MotionControl.IsFlying ||
                              impactQuadTile.GroundLevel <= launchQuadTile.GroundLevel ||
                              RandomService.DefaultGenerator.Next(100) < LOW_TO_HIGH_GROUNDLEVEL_DAMAGE_PROBABILITY;
            if (makeDamage)
            {
                impactedMissile.TargetEntity.Biometrics.Damage(this.weaponData.DamageType.Read(), this.weaponData.Damage.Read(), impactedMissile.Owner == impactedMissile.TargetEntity.Owner);
            }
        }

        /// <summary>
        /// Reference to the definition of this weapon from the metadata.
        /// </summary>
        private readonly IWeaponData weaponData;

        /// <summary>
        /// The index of the frame when this weapon launched a missile group last time.
        /// </summary>
        private readonly HeapedValue<int> frameIndexOfLastLaunch;

        /// <summary>
        /// The probability of damage of an impacted missile launched from a quadratic tile with lower ground level than the ground level
        /// of the target entity.
        /// </summary>
        private const int LOW_TO_HIGH_GROUNDLEVEL_DAMAGE_PROBABILITY = 70;
    }
}
