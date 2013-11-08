using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Contains the definition of a building type.
    /// </summary>
    class BuildingType
    {
        /// <summary>
        /// Constructs a new building type instance.
        /// </summary>
        /// <param name="name">The name of this building type.</param>
        /// <param name="spritePalette">The sprite palette of this building type.</param>
        public BuildingType(string name, SpritePalette spritePalette)
        {
            if (name == null) { throw new ArgumentNullException("name"); }
            if (spritePalette == null) { throw new ArgumentNullException("spritePalette"); }

            this.name = name;
            this.spritePalette = spritePalette;
        }

        /// <summary>
        /// Gets the name of this building type.
        /// </summary>
        public string Name { get { return this.name; } }

        /// <summary>
        /// The name of this building type.
        /// </summary>
        private string name;

        /// <summary>
        /// The sprite palette of this building type.
        /// </summary>
        private SpritePalette spritePalette;
    }
}
