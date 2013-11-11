using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Common base class for type definitions of entities. Objects that have activities on the map are called entities.
    /// </summary>
    abstract class EntityType : SimulatedObjectType
    {
        /// <summary>
        /// Constructs a new entity type.
        /// </summary>
        /// <param name="name">The name of this entity type.</param>
        /// <param name="spritePalette">The sprite palette of this entity type.</param>
        public EntityType(string name, SpritePalette spritePalette)
            : base(name)
        {
            if (spritePalette == null) { throw new ArgumentNullException("spritePalette"); }
            this.SpritePalette = spritePalette;

            this.generalData = null;
            this.groundWeapon = null;
            this.airWeapon = null;
        }

        /// <summary>
        /// Gets the general data of this entity type.
        /// </summary>
        public GeneralData GeneralData
        {
            get { return this.generalData; }
            set
            {
                if (this.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
                this.generalData = value;
            }
        }

        /// <summary>
        /// Gets the ground weapon of entities of this type.
        /// </summary>
        public WeaponData GroundWeapon
        {
            get { return this.groundWeapon; }
            set
            {
                if (this.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
                this.groundWeapon = value;
            }
        }

        /// <summary>
        /// Gets the air weapon of entities of this type.
        /// </summary>
        public WeaponData AirWeapon
        {
            get { return this.airWeapon; }
            set
            {
                if (this.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
                this.airWeapon = value;
            }
        }

        /// <see cref="SimulatedObjectType.CheckAndFinalizeImpl"/>
        protected override void CheckAndFinalizeImpl()
        {
            if (this.generalData != null) { this.generalData.CheckAndFinalize(); }
            if (this.groundWeapon != null) { this.groundWeapon.CheckAndFinalize(); }
            if (this.airWeapon != null) { this.airWeapon.CheckAndFinalize(); }
        }

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
