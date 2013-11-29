using RC.Common;
using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Scenarios
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
        /// Gets the sprite palette of this element type or null if this element type has no sprite palette.
        /// </summary>
        ISpritePalette SpritePalette { get; }

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
        /// Gets the size of the corresponding element type or null if this element type has no gas cost defined.
        /// </summary>
        ConstValue<SizeEnum> Size { get; }

        /// <summary>
        /// Gets the speed of the corresponding element type or null if this element type has no gas cost defined.
        /// </summary>
        ConstValue<RCNumber> Speed { get; }

        /// <summary>
        /// Gets the ground weapon of this element type or null if this element type has no ground weapon defined.
        /// </summary>
        IWeaponData GroundWeapon { get; }

        /// <summary>
        /// Gets the air weapon of this element type or null if this element type has no air weapon defined.
        /// </summary>
        IWeaponData AirWeapon { get; }
    }
}
