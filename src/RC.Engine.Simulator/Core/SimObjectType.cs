using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Common base class for type definitions of simulated objects.
    /// </summary>
    abstract class SimObjectType
    {
        /// <summary>
        /// Constructs a new object type.
        /// </summary>
        /// <param name="name">The name of this object type.</param>
        /// <param name="metadata">Reference to the metadata object that this type belongs to.</param>
        public SimObjectType(string name, SimMetadata metadata)
        {
            if (name == null) { throw new ArgumentNullException("name"); }
            if (metadata == null) { throw new ArgumentNullException("metadata"); }
            if (metadata.IsFinalized) { throw new InvalidOperationException("Metadata already finalized!"); }

            this.name = name;
            this.metadata = metadata;
            this.spritePalette = null;
            this.costs = null;
            this.requirements = new List<Requirement>();
        }

        /// <summary>
        /// Gets the name of this object type.
        /// </summary>
        public string Name { get { return this.name; } }

        /// <summary>
        /// Gets the sprite palette of this object type or null if this object type has no sprite palette.
        /// </summary>
        public SpritePalette SpritePalette { get { return this.spritePalette; } }

        /// <summary>
        /// Gets the costs of this object type or null if this object type has no costs.
        /// </summary>
        public CostsData Costs { get { return this.costs; } }

        #region SimObjectType buildup methods

        /// <summary>
        /// Sets the sprite palette of this object type.
        /// </summary>
        /// <param name="spritePalette">The sprite palette of this object type.</param>
        public void SetSpritePalette(SpritePalette spritePalette)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (spritePalette == null) { throw new ArgumentNullException("spritePalette"); }
            this.spritePalette = spritePalette;
        }

        /// <summary>
        /// Sets the costs information of this object type.
        /// </summary>
        /// <param name="costs">The costs information of this object type.</param>
        public void SetCostsData(CostsData costs)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (costs == null) { throw new ArgumentNullException("costs"); }
            this.costs = costs;
        }

        /// <summary>
        /// Adds a requirement to this object type.
        /// </summary>
        /// <param name="requirement">The requirement to add.</param>
        public void AddRequirement(Requirement requirement)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (requirement == null) { throw new ArgumentNullException("requirement"); }
            this.requirements.Add(requirement);
        }

        /// <summary>
        /// Builds up the references of this type definition.
        /// </summary>
        public void BuildupReferences()
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }

            this.BuildupReferencesImpl();
        }

        /// <summary>
        /// Checks and finalizes this type definition.
        /// </summary>
        public void CheckAndFinalize()
        {
            if (this.spritePalette != null) { this.spritePalette.CheckAndFinalize(); }
            if (this.costs != null) { this.costs.CheckAndFinalize(); }
            foreach (Requirement requirement in this.requirements)
            {
                requirement.CheckAndFinalize();
            }
            this.CheckAndFinalizeImpl();
        }

        #endregion SimObjectType buildup methods

        /// <summary>
        /// Further reference buildup process can be implemented by the derived classes by overriding this method.
        /// </summary>
        protected virtual void BuildupReferencesImpl() { }

        /// <summary>
        /// Further finalization process can be implemented by the derived classes by overriding this method.
        /// </summary>
        protected virtual void CheckAndFinalizeImpl() { }

        /// <summary>
        /// Gets a reference to the metadata object that this type belongs to.
        /// </summary>
        protected SimMetadata Metadata { get { return this.metadata; } }

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
        /// Reference to the metadata object that this type belongs to.
        /// </summary>
        private SimMetadata metadata;
    }
}
