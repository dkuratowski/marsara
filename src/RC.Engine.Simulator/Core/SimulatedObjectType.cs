using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Common base class for type definitions of simulated objects.
    /// </summary>
    abstract class SimulatedObjectType
    {
        /// <summary>
        /// Constructs a new object type.
        /// </summary>
        /// <param name="name">The name of this object type.</param>
        public SimulatedObjectType(string name)
        {
            if (name == null) { throw new ArgumentNullException("name"); }

            this.name = name;
            this.spritePalette = null;
            this.costs = null;
            this.requirements = new List<Requirement>();

            this.isFinalized = false;
        }

        /// <summary>
        /// Gets the name of this object type.
        /// </summary>
        public string Name { get { return this.name; } }

        /// <summary>
        /// Gets the sprite palette of this object type.
        /// </summary>
        public SpritePalette SpritePalette
        {
            get { return this.spritePalette; }
            set
            {
                if (this.isFinalized) { throw new InvalidOperationException("Already finalized!"); }
                this.spritePalette = value;
            }
        }

        /// <summary>
        /// Gets the costs of this object type or null if this object type has no costs.
        /// </summary>
        public CostsData Costs
        {
            get { return this.costs; }
            set
            {
                if (this.isFinalized) { throw new InvalidOperationException("Already finalized!"); }
                this.costs = value;
            }
        }

        /// <summary>
        /// Adds a requirement to this object type.
        /// </summary>
        /// <param name="requirement">The requirement to add.</param>
        public void AddRequirement(Requirement requirement)
        {
            if (requirement == null) { throw new ArgumentNullException("requirement"); }
            this.requirements.Add(requirement);
        }

        /// <summary>
        /// Validates and finalizes this type definition
        /// </summary>
        public void CheckAndFinalize()
        {
            if (!this.isFinalized)
            {
                if (this.spritePalette != null) { this.spritePalette.CheckAndFinalize(); }
                if (this.costs != null) { this.costs.CheckAndFinalize(); }
                foreach (Requirement requirement in this.requirements)
                {
                    requirement.CheckAndFinalize();
                }
                this.CheckAndFinalizeImpl();
                this.isFinalized = true;
            }
        }

        /// <summary>
        /// Further finalization process can be implemented by the derived classes by overriding this method.
        /// </summary>
        protected virtual void CheckAndFinalizeImpl() { }

        /// <summary>
        /// Gets whether this object type has been finalized or not.
        /// </summary>
        protected bool IsFinalized { get { return this.isFinalized; } }

        /// <summary>
        /// The name of this object type. Must be unique in the object metadata.
        /// </summary>
        private string name;

        /// <summary>
        /// The sprite palette of this object type.
        /// </summary>
        private SpritePalette spritePalette;

        /// <summary>
        /// The costs data of this object type.
        /// </summary>
        private CostsData costs;

        /// <summary>
        /// The list of the requirements of this object type.
        /// </summary>
        private List<Requirement> requirements;

        /// <summary>
        /// Indicates whether this object type has been finalized or not.
        /// </summary>
        private bool isFinalized;
    }
}
