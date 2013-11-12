using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Common base class for type definitions of entities. Objects that have activities on the map are called entities.
    /// </summary>
    abstract class EntityType : SimObjectType
    {
        /// <summary>
        /// Constructs a new entity type.
        /// </summary>
        /// <param name="name">The name of this entity type.</param>
        /// <param name="metadata">The metadata that this entity type belongs to.</param>
        public EntityType(string name, SimMetadata metadata)
            : base(name, metadata)
        {
            this.generalData = null;
            this.groundWeapon = null;
            this.airWeapon = null;
        }

        /// <summary>
        /// Gets the general data of this entity type.
        /// </summary>
        public GeneralData GeneralData { get { return this.generalData; } }

        /// <summary>
        /// Gets the ground weapon of entities of this type.
        /// </summary>
        public WeaponData GroundWeapon { get { return this.groundWeapon; } }

        /// <summary>
        /// Gets the air weapon of entities of this type.
        /// </summary>
        public WeaponData AirWeapon { get { return this.airWeapon; } }

        #region EntityType buildup methods

        /// <summary>
        /// Sets the general data of this entity type.
        /// </summary>
        /// <param name="genData">The general data of this entity type.</param>
        public void SetGeneralData(GeneralData genData)
        {
            if (this.Metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (genData == null) { throw new ArgumentNullException("genData"); }
            this.generalData = genData;
        }

        /// <summary>
        /// Sets the ground weapon of this entity type.
        /// </summary>
        /// <param name="weaponData">The ground weapon information of this entity type.</param>
        public void SetGroundWeapon(WeaponData weaponData)
        {
            if (this.Metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (weaponData == null) { throw new ArgumentNullException("weaponData"); }
            this.groundWeapon = weaponData;
        }

        /// <summary>
        /// Sets the air weapon of this entity type.
        /// </summary>
        /// <param name="weaponData">The air weapon information of this entity type.</param>
        public void SetAirWeapon(WeaponData weaponData)
        {
            if (this.Metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (weaponData == null) { throw new ArgumentNullException("weaponData"); }
            this.airWeapon = weaponData;
        }

        /// <see cref="SimObjectType.CheckAndFinalizeImpl"/>
        protected override void CheckAndFinalizeImpl()
        {
            if (this.generalData != null) { this.generalData.CheckAndFinalize(); }
            if (this.groundWeapon != null) { this.groundWeapon.CheckAndFinalize(); }
            if (this.airWeapon != null) { this.airWeapon.CheckAndFinalize(); }
        }

        #endregion EntityType buildup methods

        /// <summary>
        /// The general data of this entity type.
        /// </summary>
        private GeneralData generalData;

        /// <summary>
        /// Informations about the ground weapon of entities of this type or null if ground weapon is not defined
        /// for this entity type.
        /// </summary>
        private WeaponData groundWeapon;

        /// <summary>
        /// Informations about the air weapon of entities of this type or null if air weapon is not defined
        /// for this entity type.
        /// </summary>
        private WeaponData airWeapon;
    }
}
