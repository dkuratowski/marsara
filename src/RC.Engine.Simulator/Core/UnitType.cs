using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Contains the definition of a unit type.
    /// </summary>
    class UnitType : EntityType
    {
        /// <summary>
        /// Constructs a new unit type.
        /// </summary>
        /// <param name="name">The name of this unit type.</param>
        /// <param name="spritePalette">The sprite palette of this unit type.</param>
        public UnitType(string name, SpritePalette spritePalette)
            : base(name, spritePalette)
        {
            this.necessaryAddon = null;
            this.createdIn = null;
        }

        /// <summary>
        /// Gets the addon type that is necessary to be attached to the building to be able to create
        /// this type of units.
        /// </summary>
        public AddonType NecessaryAddon
        {
            get { return this.necessaryAddon; }
            set
            {
                if (this.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
                this.necessaryAddon = value;
            }
        }

        /// <summary>
        /// Gets the name of the building type that creates this type of units or null if no such a building type exists.
        /// </summary>
        public string CreatedIn
        {
            get { return this.createdIn; }
            set
            {
                if (this.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
                this.createdIn = value;
            }
        }

        /// <summary>
        /// Gets the name of the addon type that is necessary to be attached to the building to be able to create
        /// this type of units.
        /// </summary>
        public string NecessaryAddonName
        {
            get { return this.necessaryAddonName; }
            set
            {
                if (this.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
                this.necessaryAddonName = value;
            }
        }

        /// <summary>
        /// Reference to the addon type that is necessary to be attached to the building to be able to create
        /// this type of units.
        /// </summary>
        private AddonType necessaryAddon;

        /// <summary>
        /// Name of the addon type that is necessary to be attached to the building to be able to create
        /// this type of units.
        /// </summary>
        private string necessaryAddonName;

        /// <summary>
        /// The name of the building type that creates this type of units or null if no such a building type exists.
        /// </summary>
        private string createdIn;
    }
}
