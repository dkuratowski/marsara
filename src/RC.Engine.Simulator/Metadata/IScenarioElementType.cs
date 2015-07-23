using RC.Common;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.PublicInterfaces;
using System.Collections.Generic;
using RC.Common.Configuration;
using RC.Engine.Maps.PublicInterfaces;

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
    /// </summary>
    public interface IScenarioElementType
    {
        /// <summary>
        /// Gets the name of this element type.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Get the ID of this element type.
        /// </summary>
        int ID { get; }

        /// <summary>
        /// This flag indicates whether the instances of this type might belong to a player (true) or are
        /// always neutral.
        /// </summary>
        bool HasOwner { get; }

        /// <summary>
        /// Gets the sprite palette of this element type or null if this element type has no sprite palette.
        /// </summary>
        ISpritePalette<MapDirection> SpritePalette { get; }

        /// <summary>
        /// Gets the animation palette of this element type or null if this element type has no animation palette.
        /// </summary>
        IAnimationPalette AnimationPalette { get; }

        /// <summary>
        /// Gets the build time of this element type or null if this element type has no build time defined.
        /// </summary>
        ConstValue<int> BuildTime { get; }

        /// <summary>
        /// Gets the food cost of this element type or null if this element type has no food cost defined.
        /// </summary>
        ConstValue<int> FoodCost { get; }

        /// <summary>
        /// Gets the mineral cost of this element type or null if this element type has no mineral cost defined.
        /// </summary>
        ConstValue<int> MineralCost { get; }

        /// <summary>
        /// Gets the gas cost of this element type or null if this element type has no gas cost defined.
        /// </summary>
        ConstValue<int> GasCost { get; }

        /// <summary>
        /// Gets the area of the corresponding element type in map coordinates or null if this element type has no area defined.
        /// </summary>
        ConstValue<RCNumVector> Area { get; }

        /// <summary>
        /// Gets the armor of the corresponding element type or null if this element type has no armor defined.
        /// </summary>
        ConstValue<int> Armor { get; }

        /// <summary>
        /// Gets the maximum energy of the corresponding element type or null if this element type has no maximum energy defined.
        /// </summary>
        ConstValue<int> MaxEnergy { get; }

        /// <summary>
        /// Gets the maximum HP of the corresponding element type or null if this element type has no maximum HP defined.
        /// </summary>
        ConstValue<int> MaxHP { get; }

        /// <summary>
        /// Gets the sight range of the corresponding element type or null if this element type has no sight range defined.
        /// </summary>
        ConstValue<int> SightRange { get; }

        /// <summary>
        /// Gets the size of the corresponding element type or null if this element type has no size defined.
        /// </summary>
        ConstValue<SizeEnum> Size { get; }

        /// <summary>
        /// Gets the speed of the corresponding element type or null if this element type has no speed defined.
        /// </summary>
        ConstValue<RCNumber> Speed { get; }

        /// <summary>
        /// Gets the list of the standard weapons of this element type.
        /// </summary>
        IEnumerable<IWeaponData> StandardWeapons { get; }

        /// <summary>
        /// Gets the quadratic coordinates relative to the origin that are inside the sight range or null if this element type has no sight range defined.
        /// </summary>
        /// TODO: later the sight range will depend on the upgrades of the players!
        IEnumerable<RCIntVector> RelativeQuadCoordsInSight { get; }

        /// <summary>
        /// Checks whether the constraints of this entity type allows placing an entity of this type to the given scenario at the given
        /// quadratic position and collects all the violating quadratic coordinates relative to the top-left corner of the
        /// entity.
        /// </summary>
        /// <param name="scenario">Reference to the scenario.</param>
        /// <param name="position">The position to check.</param>
        /// <returns>
        /// The list of the quadratic coordinates (relative to the top-left corner) violating the constraints of this entity type.
        /// </returns>
        RCSet<RCIntVector> CheckConstraints(Scenario scenario, RCIntVector position);
    }
}
