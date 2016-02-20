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
    /// Represents a stub of a custom weapon of an entity.
    /// </summary>
    sealed class CustomWeaponStub : Weapon
    {
        #region Internal methods

        /// <summary>
        /// Constructs a CustomWeaponStub instance.
        /// </summary>
        /// <param name="owner">Reference to the entity that this custom weapon stub belongs to.</param>
        /// <param name="weaponData">The definition of the weapon from the metadata.</param>
        internal CustomWeaponStub(Entity owner, IWeaponData weaponData)
            : base(owner, weaponData.Missiles)
        {
            if (weaponData == null) { throw new ArgumentNullException("weaponData"); }
            this.weaponData = weaponData;

            this.attachedWeapon = this.ConstructField<CustomWeapon>("attachedWeapon");
            this.attachedWeapon.Write(null);
        }

        /// <summary>
        /// Attaches the given custom weapon to this stub.
        /// </summary>
        /// <param name="weapon">The weapon to be attached.</param>
        internal void AttachWeapon(CustomWeapon weapon)
        {
            if (weapon == null) { throw new ArgumentNullException("weapon"); }
            if (this.attachedWeapon.Read() != null) { throw new InvalidOperationException("Another custom weapon has already been attached to this stub!"); }

            weapon.OnAttachingToStub(this);
            this.attachedWeapon.Write(weapon);
        }

        /// <summary>
        /// Gets the owner of this weapon stub.
        /// </summary>
        internal Entity StubOwner { get { return this.Owner; } }

        /// <summary>
        /// Gets the weapon data assigned to this weapon stub from the metadata.
        /// </summary>
        internal IWeaponData WeaponData { get { return this.weaponData; } }

        #endregion Internal methods

        #region Overrides

        /// <see cref="Weapon.CanTargetEntity"/>
        public override bool CanTargetEntity(Entity entityToCheck)
        {
            if (this.attachedWeapon.Read() == null) { throw new InvalidOperationException("Custom weapon is not attached to this stub!"); }
            return this.attachedWeapon.Read().CanTargetEntity(entityToCheck);
        }

        /// <see cref="Weapon.CanLaunchMissiles"/>
        protected override bool CanLaunchMissiles()
        {
            if (this.attachedWeapon.Read() == null) { throw new InvalidOperationException("Custom weapon is not attached to this stub!"); }
            return this.attachedWeapon.Read().CanLaunchMissiles();
        }

        /// <see cref="Weapon.IsInRange"/>
        protected override bool IsInRange(RCNumber distance)
        {
            if (this.attachedWeapon.Read() == null) { throw new InvalidOperationException("Custom weapon is not attached to this stub!"); }
            return this.attachedWeapon.Read().IsInRange(distance);
        }

        /// <see cref="Weapon.OnLaunch"/>
        protected override void OnLaunch()
        {
            if (this.attachedWeapon.Read() == null) { throw new InvalidOperationException("Custom weapon is not attached to this stub!"); }
            this.attachedWeapon.Read().OnLaunch();
        }

        /// <see cref="Weapon.OnImpact"/>
        protected override void OnImpact(Missile impactedMissile)
        {
            if (this.attachedWeapon.Read() == null) { throw new InvalidOperationException("Custom weapon is not attached to this stub!"); }
            this.attachedWeapon.Read().OnImpact(impactedMissile);
        }
        
        /// <see cref="HeapedObject.DisposeImpl"/>
        protected override void DisposeImpl()
        {
            if (this.attachedWeapon.Read() != null)
            {
                this.attachedWeapon.Read().Dispose();
                this.attachedWeapon.Write(null);
            }
        }

        #endregion Overrides

        /// <summary>
        /// Reference to the attached custom weapon.
        /// </summary>
        private readonly HeapedValue<CustomWeapon> attachedWeapon;

        /// <summary>
        /// Reference to the weapon data assigned to this weapon stub from the metadata.
        /// </summary>
        private readonly IWeaponData weaponData;
    }
}
