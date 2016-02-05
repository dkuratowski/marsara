using RC.Common;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using RC.Common.Configuration;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Simulator.Metadata.Core
{
    /// <summary>
    /// Common base class for type definitions of scenario elements.
    /// </summary>
    class ScenarioElementType : IScenarioElementTypeInternal
    {
        /// <summary>
        /// Constructs a new element type.
        /// </summary>
        /// <param name="name">The name of this element type.</param>
        /// <param name="metadata">Reference to the metadata object that this type belongs to.</param>
        public ScenarioElementType(string name, ScenarioMetadata metadata)
        {
            if (name == null) { throw new ArgumentNullException("name"); }
            if (metadata == null) { throw new ArgumentNullException("metadata"); }
            if (metadata.IsFinalized) { throw new InvalidOperationException("ScenarioMetadata already finalized!"); }

            this.id = -1;
            this.name = name;
            this.displayedName = null;
            this.hasOwner = false;
            this.shadowSpriteName = null;
            this.shadowSpriteIndex = -1;
            this.shadowOffset = RCNumVector.Undefined;
            this.metadata = metadata;
            this.spritePalette = null;
            this.hpIconPalette = null;
            this.animationPalette = null;
            this.relativeQuadCoordsInSight = null;
            this.placementConstraints = new List<EntityPlacementConstraint>();
            this.requirements = new List<Requirement>();
            this.standardWeapons = new List<WeaponData>();
        }

        #region IScenarioElementTypeInternal members

        /// <see cref="IScenarioElementTypeInternal.Name"/>
        public string Name { get { return this.name; } }

        /// <see cref="IScenarioElementTypeInternal.DisplayedName"/>
        public string DisplayedName { get { return this.displayedName; } }

        /// <see cref="IScenarioElementTypeInternal.ID"/>
        public int ID { get { return this.id; } }

        /// <see cref="IScenarioElementTypeInternal.HasOwner"/>
        public bool HasOwner { get { return this.hasOwner; } }

        /// <see cref="IScenarioElementTypeInternal.ShadowSpriteIndex"/>
        public int ShadowSpriteIndex { get { return this.shadowSpriteIndex; } }

        /// <see cref="IScenarioElementTypeInternal.ShadowOffset"/>
        public RCNumVector ShadowOffset { get { return this.shadowOffset; } }

        /// <see cref="IScenarioElementTypeInternal.SpritePalette"/>
        public ISpritePalette<MapDirection> SpritePalette { get { return this.spritePalette; } }

        /// <see cref="IScenarioElementTypeInternal.HPIconPalette"/>
        public ISpritePalette HPIconPalette { get { return this.hpIconPalette; } }

        /// <see cref="IScenarioElementTypeInternal.AnimationPalette"/>
        public IAnimationPalette AnimationPalette { get { return this.animationPalette; } }

        /// <see cref="IScenarioElementTypeInternal.RelativeQuadCoordsInSight"/>
        public IEnumerable<RCIntVector> RelativeQuadCoordsInSight { get { return this.relativeQuadCoordsInSight; } }

        /// <see cref="IScenarioElementTypeInternal.StandardWeapons"/>
        public IEnumerable<IWeaponData> StandardWeapons { get { return this.standardWeapons; } }

        /// <see cref="IScenarioElementTypeInternal.Requirements"/>
        public IEnumerable<IRequirement> Requirements { get { return this.requirements; } }

        #region Costs data properties

        /// <see cref="IScenarioElementTypeInternal.BuildTime"/>
        public IValueRead<int> BuildTime { get { return this.buildTime; } }

        /// <see cref="IScenarioElementTypeInternal.SupplyUsed"/>
        public IValueRead<int> SupplyUsed { get { return this.supplyUsed; } }

        /// <see cref="IScenarioElementTypeInternal.SupplyProvided"/>
        public IValueRead<int> SupplyProvided { get { return this.supplyProvided; } }

        /// <see cref="IScenarioElementTypeInternal.MineralCost"/>
        public IValueRead<int> MineralCost { get { return this.mineralCost; } }

        /// <see cref="IScenarioElementTypeInternal.GasCost"/>
        public IValueRead<int> GasCost { get { return this.gasCost; } }

        #endregion Costs data properties

        #region General data properties

        /// <see cref="IScenarioElementTypeInternal.Area"/>
        public IValueRead<RCNumVector> Area { get { return this.area; } }

        /// <see cref="IScenarioElementTypeInternal.Armor"/>
        public IValueRead<int> Armor { get { return this.armor; } }

        /// <see cref="IScenarioElementTypeInternal.MaxEnergy"/>
        public IValueRead<int> MaxEnergy { get { return this.maxEnergy; } }

        /// <see cref="IScenarioElementTypeInternal.MaxHP"/>
        public IValueRead<int> MaxHP { get { return this.maxHP; } }

        /// <see cref="IScenarioElementTypeInternal.SightRange"/>
        public IValueRead<int> SightRange { get { return this.sightRange; } }

        /// <see cref="IScenarioElementTypeInternal.Size"/>
        public IValueRead<SizeEnum> Size { get { return this.size; } }

        /// <see cref="IScenarioElementTypeInternal.Speed"/>
        public IValueRead<RCNumber> Speed { get { return this.speed; } }

        #endregion General data properties

        /// <see cref="IScenarioElementTypeInternal.CheckPlacementConstraints"/>
        public RCSet<RCIntVector> CheckPlacementConstraints(Scenario scenario, RCIntVector position, RCSet<Entity> entitiesToIgnore)
        {
            if (scenario == null) { throw new ArgumentNullException("scenario"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }

            /// Check against the constraints defined by this scenario element type.
            RCSet<RCIntVector> retList = new RCSet<RCIntVector>();
            foreach (EntityPlacementConstraint constraint in this.placementConstraints)
            {
                retList.UnionWith(constraint.Check(scenario, position, entitiesToIgnore));
            }

            /// Check against map boundaries.
            this.CheckMapBorderIntersections(scenario, position, ref retList);

            return retList;
        }

        #endregion IScenarioElementType members

        #region ScenarioElementType buildup methods

        /// <summary>
        /// Sets the ID of this element type.
        /// </summary>
        /// <param name="id">The ID of this element type.</param>
        public void SetID(int id)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (id < 0) { throw new ArgumentOutOfRangeException("id"); }
            this.id = id;
        }

        /// <summary>
        /// Sets the displayed name of this element type.
        /// </summary>
        /// <param name="displayedName">The displayed name of this element type.</param>
        public void SetDisplayedName(string displayedName)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (displayedName == null) { throw new ArgumentNullException("displayedName"); }
            this.displayedName = displayedName;
        }

        /// <summary>
        /// Sets the hasOwner flag of this element type.
        /// </summary>
        /// <param name="hasOwner">The new value of the hasOwner flag.</param>
        public void SetHasOwner(bool hasOwner)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            this.hasOwner = hasOwner;
        }

        /// <summary>
        /// Sets the sprite palette of this element type.
        /// </summary>
        /// <param name="spritePalette">The sprite palette of this element type.</param>
        public void SetSpritePalette(ISpritePalette<MapDirection> spritePalette)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (spritePalette == null) { throw new ArgumentNullException("spritePalette"); }
            this.spritePalette = spritePalette;
        }

        /// <summary>
        /// Sets the HP indicator icon palette of this element type.
        /// </summary>
        /// <param name="hpIconPalette">The HP indicator icon palette of this element type.</param>
        public void SetHPIconPalette(ISpritePalette hpIconPalette)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (hpIconPalette == null) { throw new ArgumentNullException("hpIconPalette"); }
            this.hpIconPalette = hpIconPalette;
        }

        /// <summary>
        /// Sets the animation palette of this element type.
        /// </summary>
        /// <param name="animationPalette">The animation palette of this element type.</param>
        public void SetAnimationPalette(AnimationPalette animationPalette)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (animationPalette == null) { throw new ArgumentNullException("animationPalette"); }
            this.animationPalette = animationPalette;
        }

        /// <summary>
        /// Sets the shadow data of this element type.
        /// </summary>
        /// <param name="shadowSpriteName">The name of the shadow sprite in the shadow palette of the metadata.</param>
        /// <param name="shadowOffset">The coordinates of the center of the shadow relative to the upper-left corner of the area defined for this element type.</param>
        public void SetShadowData(string shadowSpriteName, RCNumVector shadowOffset)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (shadowSpriteName == null) { throw new ArgumentNullException("shadowSpriteName"); }
            if (shadowOffset == RCNumVector.Undefined) { throw new ArgumentNullException("shadowOffset"); }

            this.shadowSpriteName = shadowSpriteName;
            this.shadowOffset = shadowOffset;
        }

        /// <summary>
        /// Adds a placement constraint to this element type.
        /// </summary>
        /// <param name="constraints">The placement constraint to add.</param>
        public void AddPlacementConstraint(EntityPlacementConstraint constraint)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (constraint == null) { throw new ArgumentNullException("constraint"); }

            constraint.SetEntityType(new IScenarioElementType(this));
            this.placementConstraints.Add(constraint);
        }

        /// <summary>
        /// Adds a requirement to this element type.
        /// </summary>
        /// <param name="requirement">The requirement to add.</param>
        public void AddRequirement(Requirement requirement)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (requirement == null) { throw new ArgumentNullException("requirement"); }
            this.requirements.Add(requirement);
        }

        /// <summary>
        /// Adds a standard weapon to this element type.
        /// </summary>
        /// <param name="weaponData">The definition of the weapon to be added.</param>
        public void AddStandardWeapon(WeaponData weaponData)
        {
            if (this.Metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (weaponData == null) { throw new ArgumentNullException("weaponData"); }
            this.standardWeapons.Add(weaponData);
        }

        #region Costs data setters

        /// <summary>
        /// Sets the build time.
        /// </summary>
        public void SetBuildTime(int buildTime)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            this.buildTime = new ConstValue<int>(buildTime);
        }

        /// <summary>
        /// Sets the supply used.
        /// </summary>
        public void SetSupplyUsed(int supplyUsed)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            this.supplyUsed = new ConstValue<int>(supplyUsed);
        }

        /// <summary>
        /// Sets the supply provided.
        /// </summary>
        public void SetSupplyProvided(int supplyProvided)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            this.supplyProvided = new ConstValue<int>(supplyProvided);
        }

        /// <summary>
        /// Sets the mineral cost.
        /// </summary>
        public void SetMineralCost(int mineralCost)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            this.mineralCost = new ConstValue<int>(mineralCost);
        }

        /// <summary>
        /// Sets the gas cost.
        /// </summary>
        public void SetGasCost(int gasCost)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            this.gasCost = new ConstValue<int>(gasCost);
        }

        #endregion Costs data setters

        #region General data setters

        /// <summary>
        /// Sets the area of the corresponding element type in map coordinates.
        /// </summary>
        /// <param name="area">The area vector.</param>
        public void SetArea(RCNumVector area)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (area == RCNumVector.Undefined) { throw new ArgumentNullException("area"); }
            this.area = new ConstValue<RCNumVector>(area);
        }

        /// <summary>
        /// Sets the armor of the corresponding element type.
        /// </summary>
        /// <param name="armor">The armor value.</param>
        public void SetArmor(int armor)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            this.armor = new ConstValue<int>(armor);
        }

        /// <summary>
        /// Sets the maximum energy of the corresponding element type.
        /// </summary>
        public void SetMaxEnergy(int maxEnergy)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            this.maxEnergy = new ConstValue<int>(maxEnergy);
        }

        /// <summary>
        /// Sets the maximum HP of the corresponding element type.
        /// </summary>
        public void SetMaxHP(int maxHP)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            this.maxHP = new ConstValue<int>(maxHP);
        }

        /// <summary>
        /// Sets the sight range of the corresponding element type.
        /// </summary>
        public void SetSightRange(int sightRange)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            this.sightRange = new ConstValue<int>(sightRange);

            RCIntVector nullVector = new RCIntVector(0, 0);
            this.relativeQuadCoordsInSight = new RCSet<RCIntVector>();
            for (int x = -this.sightRange.Read(); x <= this.sightRange.Read(); x++)
            {
                for (int y = -this.sightRange.Read(); y <= this.sightRange.Read(); y++)
                {
                    RCIntVector quadCoord = new RCIntVector(x, y);
                    if (MapUtils.ComputeDistance(nullVector, quadCoord) < this.sightRange.Read())
                    {
                        this.relativeQuadCoordsInSight.Add(quadCoord);
                    }
                }
            }
        }

        /// <summary>
        /// Sets the size of the corresponding element type.
        /// </summary>
        public void SetSize(SizeEnum size)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            this.size = new ConstValue<SizeEnum>(size);
        }

        /// <summary>
        /// Sets the speed of the corresponding element type.
        /// </summary>
        public void SetSpeed(RCNumber speed)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            this.speed = new ConstValue<RCNumber>(speed);
        }

        #endregion General data setters

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
            if (!this.metadata.IsFinalized)
            {
                if (this.animationPalette != null) { this.animationPalette.CheckAndFinalize(); }
                foreach (WeaponData weapon in this.standardWeapons) { weapon.CheckAndFinalize(); }

                if (this.buildTime != null && this.buildTime.Read() < 0) { throw new SimulatorException("BuildTime must be non-negative!"); }
                if (this.supplyUsed != null && this.supplyUsed.Read() < 0) { throw new SimulatorException("SupplyUsed must be non-negative!"); }
                if (this.supplyProvided != null && this.supplyProvided.Read() < 0) { throw new SimulatorException("SupplyProvided must be non-negative!"); }
                if (this.mineralCost != null && this.mineralCost.Read() < 0) { throw new SimulatorException("MineralCost must be non-negative!"); }
                if (this.gasCost != null && this.gasCost.Read() < 0) { throw new SimulatorException("GasCost must be non-negative!"); }

                if (this.area != null && (this.area.Read().X <= 0 || this.area.Read().Y <= 0)) { throw new SimulatorException("Area cannot be 0 or less in any directions!"); }
                if (this.armor != null && this.armor.Read() < 0) { throw new SimulatorException("Armor must be non-negative!"); }
                if (this.maxEnergy != null && this.maxEnergy.Read() < 0) { throw new SimulatorException("MaxEnergy must be non-negative!"); }
                if (this.maxHP != null && this.maxHP.Read() <= 0) { throw new SimulatorException("MaxHP cannot be 0 or less!"); }
                if (this.sightRange != null && this.sightRange.Read() < 0) { throw new SimulatorException("SightRange must be non-negative!"); }
                if (this.speed != null && this.speed.Read() < 0) { throw new SimulatorException("Speed must be non-negative!"); }

                if (this.shadowSpriteName == null && this.shadowOffset != RCNumVector.Undefined ||
                    this.shadowSpriteName != null && this.shadowOffset == RCNumVector.Undefined)
                {
                    throw new SimulatorException("Both shadow sprite name and shadow offset shall be defined in the metadata or none of them!");
                }

                if (this.shadowSpriteName != null)
                {
                    if (this.metadata.ShadowPalette == null) { throw new SimulatorException("Shadow palette not defined in the metadata!"); }
                    this.shadowSpriteIndex = this.metadata.ShadowPalette.GetSpriteIndex(this.shadowSpriteName);
                }

                foreach (Requirement requirement in this.requirements)
                {
                    requirement.CheckAndFinalize();
                }
                this.CheckAndFinalizeImpl();
            }
        }

        #endregion ScenarioElementType buildup methods

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
        protected ScenarioMetadata Metadata { get { return this.metadata; } }

        /// <summary>
        /// Checks whether placing an entity of this type to the given scenario at the given quadratic position remains inside the boundaries of
        /// the map and collects all the quadratic coordinates relative to the given position that violates this condition.
        /// </summary>
        /// <param name="scenario">Reference to the given scenario.</param>
        /// <param name="position">The position to be checked.</param>
        /// <param name="violatingQuadCoords">The target list in which to collect the violating quadratic coordinates.</param>
        private void CheckMapBorderIntersections(Scenario scenario, RCIntVector position, ref RCSet<RCIntVector> violatingQuadCoords)
        {
            RCIntVector quadSize = scenario.Map.CellToQuadSize(this.Area.Read());
            for (int quadX = 0; quadX < quadSize.X; quadX++)
            {
                for (int quadY = 0; quadY < quadSize.Y; quadY++)
                {
                    RCIntVector relQuadCoords = new RCIntVector(quadX, quadY);
                    RCIntVector absQuadCoords = position + relQuadCoords;
                    if (absQuadCoords.X < 0 || absQuadCoords.X >= scenario.Map.Size.X ||
                        absQuadCoords.Y < 0 || absQuadCoords.Y >= scenario.Map.Size.Y)
                    {
                        violatingQuadCoords.Add(relQuadCoords);
                    }
                }
            }
        }

        /// <summary>
        /// The name of this element type. Must be unique in the metadata.
        /// </summary>
        private readonly string name;

        /// <summary>
        /// The displayed name of this element type or null if this element type doesn't define displayed name.
        /// </summary>
        private string displayedName;

        /// <summary>
        /// The ID of this element type. Must be unique in the metadata.
        /// </summary>
        private int id;

        /// <summary>
        /// This flag indicates whether the instances of this type might belong to a player (true) or are
        /// always neutral.
        /// </summary>
        private bool hasOwner;

        /// <summary>
        /// The sprite palette of this element type.
        /// </summary>
        private ISpritePalette<MapDirection> spritePalette;

        /// <summary>
        /// The HP indicator icon palette of this element type.
        /// </summary>
        private ISpritePalette hpIconPalette;

        /// <summary>
        /// The animation palette of this element type.
        /// </summary>
        private AnimationPalette animationPalette;

        /// <summary>
        /// List of the placement constraints of this element type.
        /// </summary>
        private readonly List<EntityPlacementConstraint> placementConstraints;

        /// <summary>
        /// The costs data of this element type.
        /// </summary>
        private ConstValue<int> buildTime;
        private ConstValue<int> supplyUsed;
        private ConstValue<int> supplyProvided;
        private ConstValue<int> mineralCost;
        private ConstValue<int> gasCost;

        /// <summary>
        /// The general data of this element type.
        /// </summary>
        private ConstValue<RCNumVector> area;
        private ConstValue<int> armor;
        private ConstValue<int> maxEnergy;
        private ConstValue<int> maxHP;
        private ConstValue<int> sightRange;
        private ConstValue<SizeEnum> size;
        private ConstValue<RCNumber> speed;

        /// <summary>
        /// The shadow data of this element type.
        /// </summary>
        private string shadowSpriteName;
        private int shadowSpriteIndex;
        private RCNumVector shadowOffset;

        /// <summary>
        /// The standard weapons of this element type.
        /// </summary>
        private readonly List<WeaponData> standardWeapons;

        /// <summary>
        /// The list of the requirements of this element type.
        /// </summary>
        private readonly List<Requirement> requirements;

        /// <summary>
        /// Reference to the metadata object that this type belongs to.
        /// </summary>
        private readonly ScenarioMetadata metadata;

        /// <summary>
        /// The quadratic coordinates relative to the origin that are inside the sight range or null if this element type has no sight range defined.
        /// </summary>
        private RCSet<RCIntVector> relativeQuadCoordsInSight;
    }
}
