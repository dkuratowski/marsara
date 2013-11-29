using RC.Common;
using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Scenarios
{
    /// <summary>
    /// Common base class for type definitions of scenario elements.
    /// </summary>
    class ScenarioElementType : IScenarioElementType
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

            this.name = name;
            this.metadata = metadata;
            this.spritePalette = null;
            this.requirements = new List<Requirement>();
        }

        #region IScenarioElementType members

        /// <see cref="IScenarioElementType.Name"/>
        public string Name { get { return this.name; } }

        /// <see cref="IScenarioElementType.SpritePalette"/>
        public ISpritePalette SpritePalette { get { return this.GetSpritePaletteImpl(); } }

        #region Costs data properties

        /// <see cref="IScenarioElementType.BuildTime"/>
        public ConstValue<int> BuildTime { get { return this.buildTime; } }

        /// <see cref="IScenarioElementType.FoodCost"/>
        public ConstValue<int> FoodCost { get { return this.foodCost; } }

        /// <see cref="IScenarioElementType.MineralCost"/>
        public ConstValue<int> MineralCost { get { return this.mineralCost; } }

        /// <see cref="IScenarioElementType.GasCost"/>
        public ConstValue<int> GasCost { get { return this.gasCost; } }

        #endregion Costs data properties

        #region General data properties

        /// <see cref="IScenarioElementType.Area"/>
        public ConstValue<RCNumVector> Area { get { return this.area; } }

        /// <see cref="IScenarioElementType.Armor"/>
        public ConstValue<int> Armor { get { return this.armor; } }

        /// <see cref="IScenarioElementType.MaxEnergy"/>
        public ConstValue<int> MaxEnergy { get { return this.maxEnergy; } }

        /// <see cref="IScenarioElementType.MaxHP"/>
        public ConstValue<int> MaxHP { get { return this.maxHP; } }

        /// <see cref="IScenarioElementType.SightRange"/>
        public ConstValue<int> SightRange { get { return this.sightRange; } }

        /// <see cref="IScenarioElementType.Size"/>
        public ConstValue<SizeEnum> Size { get { return this.size; } }

        /// <see cref="IScenarioElementType.Speed"/>
        public ConstValue<RCNumber> Speed { get { return this.speed; } }

        #endregion General data properties

        #region Weapons

        /// <see cref="IScenarioElementType.GroundWeapon"/>
        public IWeaponData GroundWeapon { get { return this.groundWeapon; } }

        /// <see cref="IScenarioElementType.AirWeapon"/>
        public IWeaponData AirWeapon { get { return this.airWeapon; } }

        #endregion Weapons

        #endregion IScenarioElementType members

        #region Internal public methods

        /// <see cref="IScenarioElementType.SpritePalette"/>
        public SpritePalette GetSpritePaletteImpl() { return this.spritePalette; }

        #endregion Internal public methods

        #region ScenarioElementType buildup methods

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
        /// Sets the ground weapon of this element type.
        /// </summary>
        /// <param name="weaponData">The ground weapon information of this element type.</param>
        public void SetGroundWeapon(WeaponData weaponData)
        {
            if (this.Metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (weaponData == null) { throw new ArgumentNullException("weaponData"); }
            this.groundWeapon = weaponData;
        }

        /// <summary>
        /// Sets the air weapon of this element type.
        /// </summary>
        /// <param name="weaponData">The air weapon information of this element type.</param>
        public void SetAirWeapon(WeaponData weaponData)
        {
            if (this.Metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (weaponData == null) { throw new ArgumentNullException("weaponData"); }
            this.airWeapon = weaponData;
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
        /// Sets the food cost.
        /// </summary>
        public void SetFoodCost(int foodCost)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            this.foodCost = new ConstValue<int>(foodCost);
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
                if (this.spritePalette != null) { this.spritePalette.CheckAndFinalize(); }
                if (this.groundWeapon != null) { this.groundWeapon.CheckAndFinalize(); }
                if (this.airWeapon != null) { this.airWeapon.CheckAndFinalize(); }

                if (this.buildTime != null && this.buildTime.Read() < 0) { throw new SimulatorException("BuildTime must be non-negative!"); }
                if (this.foodCost != null && this.foodCost.Read() < 0) { throw new SimulatorException("FoodCost must be non-negative!"); }
                if (this.mineralCost != null && this.mineralCost.Read() < 0) { throw new SimulatorException("MineralCost must be non-negative!"); }
                if (this.gasCost != null && this.gasCost.Read() < 0) { throw new SimulatorException("GasCost must be non-negative!"); }

                if (this.area != null && (this.area.Read().X <= 0 || this.area.Read().Y <= 0)) { throw new SimulatorException("Area cannot be 0 or less in any directions!"); }
                if (this.armor != null && this.armor.Read() < 0) { throw new SimulatorException("Armor must be non-negative!"); }
                if (this.maxEnergy != null && this.maxEnergy.Read() < 0) { throw new SimulatorException("MaxEnergy must be non-negative!"); }
                if (this.maxHP != null && this.maxHP.Read() <= 0) { throw new SimulatorException("MaxHP cannot be 0 or less!"); }
                if (this.sightRange != null && this.sightRange.Read() < 0) { throw new SimulatorException("SightRange must be non-negative!"); }
                if (this.speed != null && this.speed.Read() < 0) { throw new SimulatorException("Speed must be non-negative!"); }

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
        /// The name of this element type. Must be unique in the metadata.
        /// </summary>
        private string name;

        /// <summary>
        /// The sprite palette of this element type.
        /// </summary>
        private SpritePalette spritePalette;

        /// <summary>
        /// The costs data of this element type.
        /// </summary>
        private ConstValue<int> buildTime;
        private ConstValue<int> foodCost;
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
        /// The weapons of this element type.
        /// </summary>
        private WeaponData groundWeapon;
        private WeaponData airWeapon;

        /// <summary>
        /// The list of the requirements of this element type.
        /// </summary>
        private List<Requirement> requirements;

        /// <summary>
        /// Reference to the metadata object that this type belongs to.
        /// </summary>
        private ScenarioMetadata metadata;
    }
}
