using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }

    /// <summary>
    /// Represents a standard weapon of an entity.
    /// </summary>
    public class StandardWeapon : Weapon
    {
        /// <summary>
        /// Constructs a StandardWeapon instance.
        /// </summary>
        /// <param name="weaponData">The definition of the weapon from the metadata.</param>
        public StandardWeapon(IWeaponData weaponData)
        {
            if (weaponData == null) { throw new ArgumentNullException("weaponData"); }
            this.weaponData = weaponData;

            this.dummyField = this.ConstructField<int>("dummyField");
        }

        /// <see cref="Weapon.CanTargetEntity"/>
        public override bool CanTargetEntity(Entity entityToCheck)
        {
            return (entityToCheck.IsFlying && this.weaponData.WeaponType.Read() == WeaponTypeEnum.Air) ||
                   (!entityToCheck.IsFlying && this.weaponData.WeaponType.Read() == WeaponTypeEnum.Ground);
        }

        /// <summary>
        /// Reference to the definition of this weapon from the metadata.
        /// </summary>
        private readonly IWeaponData weaponData;

        /// <summary>
        /// To avoid runtime error.
        /// </summary>
        private readonly HeapedValue<int> dummyField;
    }
}
