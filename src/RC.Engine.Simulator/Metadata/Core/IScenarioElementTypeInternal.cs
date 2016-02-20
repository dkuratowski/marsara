using System;
using RC.Common;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.PublicInterfaces;
using System.Collections.Generic;
using RC.Common.Configuration;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Simulator.Metadata.Core
{
    /// <summary>
    /// Internal interface of the scenario element types defined in the metadata.
    /// </summary>
    interface IScenarioElementTypeInternal
    {
        /// <summary>
        /// Gets the name of this element type.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the displayed name of this element type.
        /// </summary>
        string DisplayedName { get; }

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
        /// The index of the shadow sprite of this element type in the shadow palette of the metadata or -1 if
        /// no shadow data has been defined for this element type.
        /// </summary>
        int ShadowSpriteIndex { get; }

        /// <summary>
        /// The coordinates of the center of the shadow relative to the upper-left corner of the area defined for this element type
        /// or RCNumVector.Undefined if no shadow data has been defined for this element type.
        /// </summary>
        RCNumVector ShadowOffset { get; }

        /// <summary>
        /// Gets the sprite palette of this element type or null if this element type has no sprite palette.
        /// </summary>
        ISpritePalette<MapDirection> SpritePalette { get; }

        /// <summary>
        /// Gets the HP indicator icon palette of this element type or null if this element type has no HP indicator icon palette.
        /// </summary>
        ISpritePalette HPIconPalette { get; }

        /// <summary>
        /// Gets the animation palette of this element type or null if this element type has no animation palette.
        /// </summary>
        IAnimationPalette AnimationPalette { get; }

        /// <summary>
        /// Gets the build time of this element type or null if this element type has no build time defined.
        /// </summary>
        IValueRead<int> BuildTime { get; }

        /// <summary>
        /// Gets the supply used by this element type or null if this element type has no such value defined.
        /// </summary>
        IValueRead<int> SupplyUsed { get; }

        /// <summary>
        /// Gets the supply provided by this element type or null if this element type has no such value defined.
        /// </summary>
        IValueRead<int> SupplyProvided { get; }

        /// <summary>
        /// Gets the mineral cost of this element type or null if this element type has no mineral cost defined.
        /// </summary>
        IValueRead<int> MineralCost { get; }

        /// <summary>
        /// Gets the gas cost of this element type or null if this element type has no gas cost defined.
        /// </summary>
        IValueRead<int> GasCost { get; }

        /// <summary>
        /// Gets the area of the corresponding element type in map coordinates or null if this element type has no area defined.
        /// </summary>
        IValueRead<RCNumVector> Area { get; }

        /// <summary>
        /// Gets the armor of the corresponding element type or null if this element type has no armor defined.
        /// </summary>
        IValueRead<int> Armor { get; }

        /// <summary>
        /// Gets the maximum energy of the corresponding element type or null if this element type has no maximum energy defined.
        /// </summary>
        IValueRead<int> MaxEnergy { get; }

        /// <summary>
        /// Gets the maximum HP of the corresponding element type or null if this element type has no maximum HP defined.
        /// </summary>
        IValueRead<int> MaxHP { get; }

        /// <summary>
        /// Gets the sight range of the corresponding element type or null if this element type has no sight range defined.
        /// </summary>
        IValueRead<int> SightRange { get; }

        /// <summary>
        /// Gets the size of the corresponding element type or null if this element type has no size defined.
        /// </summary>
        IValueRead<SizeEnum> Size { get; }

        /// <summary>
        /// Gets the speed of the corresponding element type or null if this element type has no speed defined.
        /// </summary>
        IValueRead<RCNumber> Speed { get; }

        /// <summary>
        /// Gets the list of the standard weapons of this element type.
        /// </summary>
        IEnumerable<IWeaponData> StandardWeapons { get; }

        /// <summary>
        /// Gets the list of the custom weapons of this element type.
        /// </summary>
        IEnumerable<IWeaponData> CustomWeapons { get; }
        
        /// <summary>
        /// Gets the requirements of this element type.
        /// </summary>
        IEnumerable<IRequirement> Requirements { get; }

        /// <summary>
        /// Gets the quadratic coordinates relative to the origin that are inside the sight range or null if this element type has no sight range defined.
        /// </summary>
        IEnumerable<RCIntVector> RelativeQuadCoordsInSight { get; }

        /// <summary>
        /// Checks whether the constraints of this element type allows placing an entity of this type to the given scenario at the given quadratic
        /// position and collects all the violating quadratic coordinates relative to the given position.
        /// </summary>
        /// <param name="scenario">Reference to the given scenario.</param>
        /// <param name="position">The position to be checked.</param>
        /// <param name="entitiesToIgnore">
        /// The list of entities to be ignored during the check. All entities in this list shall belong to the given scenario.
        /// </param>
        /// <returns>
        /// The list of the quadratic coordinates (relative to the given position) violating the placement constraints of this element type.
        /// </returns>
        RCSet<RCIntVector> CheckPlacementConstraints(Scenario scenario, RCIntVector position, RCSet<Entity> entitiesToIgnore);
    }
}
