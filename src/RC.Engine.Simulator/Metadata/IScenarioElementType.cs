using RC.Common;
using RC.Common.Configuration;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.Metadata.Core;
using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Metadata
{
    /// <summary>
    /// Enumerates the possible sizes of an entity.
    /// </summary>
    public enum SizeEnum
    {
        [EnumMapping("Small")]
        Small = 0,
        [EnumMapping("Medium")]
        Medium = 1,
        [EnumMapping("Large")]
        Large = 2
    }

    /// <summary>
    /// Interface of the scenario element types defined in the metadata.
    /// This interface is defined as a class because we want to overload the equality/inequality operators.
    /// </summary>
    public class IScenarioElementType
    {
        #region Interface methods

        /// <summary>
        /// Gets the name of this element type.
        /// </summary>
        public string Name { get { return this.implementation.Name;} }

        /// <summary>
        /// Gets the displayed name of this element type.
        /// </summary>
        public string DisplayedName { get { return this.implementation.DisplayedName; } }

        /// <summary>
        /// Get the ID of this element type.
        /// </summary>
        public int ID { get { return this.implementation.ID; } }

        /// <summary>
        /// This flag indicates whether the instances of this type might belong to a player (true) or are
        /// always neutral.
        /// </summary>
        public bool HasOwner { get { return this.implementation.HasOwner; } }

        /// <summary>
        /// The index of the shadow sprite of this element type in the shadow palette of the metadata or -1 if
        /// no shadow data has been defined for this element type.
        /// </summary>
        public int ShadowSpriteIndex { get { return this.implementation.ShadowSpriteIndex; } }

        /// <summary>
        /// The coordinates of the center of the shadow relative to the upper-left corner of the area defined for this element type
        /// or RCNumVector.Undefined if no shadow data has been defined for this element type.
        /// </summary>
        public RCNumVector ShadowOffset { get { return this.implementation.ShadowOffset; } }

        /// <summary>
        /// Gets the sprite palette of this element type or null if this element type has no sprite palette.
        /// </summary>
        public ISpritePalette<MapDirection> SpritePalette { get { return this.implementation.SpritePalette; } }

        /// <summary>
        /// Gets the HP indicator icon palette of this element type or null if this element type has no HP indicator icon palette.
        /// </summary>
        public ISpritePalette HPIconPalette { get { return this.implementation.HPIconPalette; } }

        /// <summary>
        /// Gets the animation palette of this element type or null if this element type has no animation palette.
        /// </summary>
        public IAnimationPalette AnimationPalette { get { return this.implementation.AnimationPalette; } }

        /// <summary>
        /// Gets the build time of this element type or null if this element type has no build time defined.
        /// </summary>
        public IValueRead<int> BuildTime { get { return this.implementation.BuildTime; } }

        /// <summary>
        /// Gets the supply used by this element type or null if this element type has no such value defined.
        /// </summary>
        public IValueRead<int> SupplyUsed { get { return this.implementation.SupplyUsed; } }

        /// <summary>
        /// Gets the supply provided by this element type or null if this element type has no such value defined.
        /// </summary>
        public IValueRead<int> SupplyProvided { get { return this.implementation.SupplyProvided; } }

        /// <summary>
        /// Gets the mineral cost of this element type or null if this element type has no mineral cost defined.
        /// </summary>
        public IValueRead<int> MineralCost { get { return this.implementation.MineralCost; } }

        /// <summary>
        /// Gets the gas cost of this element type or null if this element type has no gas cost defined.
        /// </summary>
        public IValueRead<int> GasCost { get { return this.implementation.GasCost; } }

        /// <summary>
        /// Gets the area of the corresponding element type in map coordinates or null if this element type has no area defined.
        /// </summary>
        public IValueRead<RCNumVector> Area { get { return this.implementation.Area; } }

        /// <summary>
        /// Gets the armor of the corresponding element type or null if this element type has no armor defined.
        /// </summary>
        public IValueRead<int> Armor { get { return this.implementation.Armor; } }

        /// <summary>
        /// Gets the maximum energy of the corresponding element type or null if this element type has no maximum energy defined.
        /// </summary>
        public IValueRead<int> MaxEnergy { get { return this.implementation.MaxEnergy; } }

        /// <summary>
        /// Gets the maximum HP of the corresponding element type or null if this element type has no maximum HP defined.
        /// </summary>
        public IValueRead<int> MaxHP { get { return this.implementation.MaxHP; } }

        /// <summary>
        /// Gets the sight range of the corresponding element type or null if this element type has no sight range defined.
        /// </summary>
        public IValueRead<int> SightRange { get { return this.implementation.SightRange; } }

        /// <summary>
        /// Gets the size of the corresponding element type or null if this element type has no size defined.
        /// </summary>
        public IValueRead<SizeEnum> Size { get { return this.implementation.Size; } }

        /// <summary>
        /// Gets the speed of the corresponding element type or null if this element type has no speed defined.
        /// </summary>
        public IValueRead<RCNumber> Speed { get { return this.implementation.Speed; } }

        /// <summary>
        /// Gets the list of the standard weapons of this element type.
        /// </summary>
        public IEnumerable<IWeaponData> StandardWeapons { get { return this.implementation.StandardWeapons; } }

        /// <summary>
        /// Gets the requirements of this element type.
        /// </summary>
        public IEnumerable<IRequirement> Requirements { get { return this.implementation.Requirements; } }

        /// <summary>
        /// Gets the quadratic coordinates relative to the origin that are inside the sight range or null if this element type has no sight range defined.
        /// </summary>
        public IEnumerable<RCIntVector> RelativeQuadCoordsInSight { get { return this.implementation.RelativeQuadCoordsInSight; } }

        /// <summary>
        /// Checks whether the constraints of this element type allows placing an entity of this type to the given scenario at the given quadratic
        /// position and collects all the violating quadratic coordinates relative to the given position.
        /// </summary>
        /// <param name="scenario">Reference to the given scenario.</param>
        /// <param name="position">The position to be checked.</param>
        /// <returns>
        /// The list of the quadratic coordinates (relative to the given position) violating the placement constraints of this element type.
        /// </returns>
        public RCSet<RCIntVector> CheckPlacementConstraints(Scenario scenario, RCIntVector position)
        {
            return this.implementation.CheckPlacementConstraints(scenario, position);
        }

        /// <summary>
        /// Checks whether the constraints of this element type allows placing the given entity to its scenario at the given quadratic position and
        /// collects all the violating quadratic coordinates relative to the given position.
        /// </summary>
        /// <param name="entity">Reference to the entity to be checked.</param>
        /// <param name="position">The position to be checked.</param>
        /// <returns>
        /// The list of the quadratic coordinates (relative to the given position) violating the constraints of this element type.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the type of the given entity is not the same as this type or if the entity is not added to a scenario.
        /// </exception>
        public RCSet<RCIntVector> CheckPlacementConstraints(Entity entity, RCIntVector position)
        {
            return this.implementation.CheckPlacementConstraints(entity, position);
        }

        #endregion Interface methods

        #region Equality operator overload

        /// <summary>
        /// Checks whether this element type is the same as the other element type.
        /// </summary>
        /// <param name="otherType">The other element type.</param>
        /// <returns>True if this element type is the same as the other element type.</returns>
        public bool Equals(IScenarioElementType otherType)
        {
            if (object.ReferenceEquals(otherType, null)) { return false; }
            return this.Name == otherType.Name;
        }

        /// <see cref="Object.Equals"/>
        public override bool Equals(object obj)
        {
            /// If parameter is null -> return false.
            if (object.ReferenceEquals(obj, null)) { return false; }

            /// If parameter cannot be cast to IScenarioElementType -> return false.
            IScenarioElementType type = obj as IScenarioElementType;
            if (object.ReferenceEquals(type, null)) { return false; }

            /// Return true if the names are equal.
            return this.Name == type.Name;
        }

        /// <see cref="Object.GetHashCode"/>
        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        /// <summary>
        /// Overload of operator ==.
        /// </summary>
        public static bool operator ==(IScenarioElementType typeA, IScenarioElementType typeB)
        {
            if (object.ReferenceEquals(typeA, typeB)) { return true; }
            if (object.ReferenceEquals(typeA, null) || object.ReferenceEquals(typeB, null)) { return false; }
            return typeA.Name == typeB.Name;
        }

        /// <summary>
        /// Overload of operator !=.
        /// </summary>
        public static bool operator !=(IScenarioElementType typeA, IScenarioElementType typeB)
        {
            return !(typeA == typeB);
        }

        #endregion Equality operator overload

        /// <summary>
        /// Constructs an instance of this interface with the given implementation.
        /// </summary>
        /// <param name="implementation">The implementation of this interface.</param>
        internal IScenarioElementType(IScenarioElementTypeInternal implementation)
        {
            if (implementation == null) { throw new ArgumentNullException("implementation"); }
            this.implementation = implementation;
        }

        /// <summary>
        /// Gets the implementation of this interface.
        /// </summary>
        internal IScenarioElementTypeInternal ElementTypeImpl { get { return this.implementation; } }

        /// <summary>
        /// Reference to the implementation of this interface.
        /// </summary>
        private IScenarioElementTypeInternal implementation;
    }
}
