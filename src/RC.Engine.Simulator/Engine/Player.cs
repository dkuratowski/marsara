using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.Metadata.Core;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.ComponentInterfaces;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// Enumerates the possible statuses of an upgrade at a given player.
    /// </summary>
    public enum UpgradeStatus
    {
        None = 0,           /// Indicates that the player doesn't have the given upgrade at all.
        Researching = 1,    /// Indicates that the player is currently researching the given upgrade.
        Researched = 2      /// Indicates that the player has been researched the given upgrade.
    }

    /// <summary>
    /// Represents a player of a scenario.
    /// </summary>
    public class Player : HeapedObject
    {
        /// <summary>
        /// Constructs a new player instance.
        /// </summary>
        /// <param name="playerIndex">The index of the player.</param>
        /// <param name="startLocation">The start location of the player.</param>
        internal Player(int playerIndex, StartLocation startLocation)
        {
            if (playerIndex < 0 || playerIndex >= Player.MAX_PLAYERS) { throw new ArgumentOutOfRangeException("playerIndex"); }
            if (startLocation == null) { throw new ArgumentNullException("startLocation"); }
            if (startLocation.Scenario == null) { throw new SimulatorException("The given start location doesn't belong to a scenario!"); }
            if (!startLocation.HasMapObject(MapObjectLayerEnum.GroundObjects)) { throw new SimulatorException("The given start location has already been initialized!"); }

            ScenarioMetadataUpgrade metadata = new ScenarioMetadataUpgrade();
            metadata.AttachMetadata(ComponentManager.GetInterface<IScenarioLoader>().Metadata);
            this.metadata = metadata;
            this.metadataUpgrade = metadata;

            this.playerIndex = this.ConstructField<int>("playerIndex");
            this.startLocation = this.ConstructField<StartLocation>("startLocation");
            this.startPosition = this.ConstructField<RCNumVector>("startPosition");
            this.quadraticStartPosition = this.ConstructField<RCIntRectangle>("quadraticStartPosition");

            this.minerals = this.ConstructField<int>("minerals");
            this.vespeneGas = this.ConstructField<int>("vespeneGas");
            this.lockedSupplies = this.ConstructField<int>("lockedSupplies");
            this.usedSupplyCache = new CachedValue<int>(this.CalculateUsedSupply);
            this.totalSupplyCache = new CachedValue<int>(this.CalculateTotalSupply);

            this.playerIndex.Write(playerIndex);
            this.startLocation.Write(startLocation);
            this.startPosition.Write(startLocation.MotionControl.PositionVector.Read());
            this.quadraticStartPosition.Write(startLocation.MapObject.QuadraticPosition);

            this.buildings = new Dictionary<string, RCSet<Building>>();
            this.addons = new Dictionary<string, RCSet<Addon>>();
            this.units = new Dictionary<string, RCSet<Unit>>();
            this.upgrades = new Dictionary<string, Upgrade>();

            this.minerals.Write(INITIAL_MINERALS);
            this.vespeneGas.Write(INITIAL_VESPENE_GAS);
            this.lockedSupplies.Write(0);
        }

        /// <summary>
        /// Adds a building to this player.
        /// </summary>
        /// <param name="building">The building to add to this player.</param>
        public void AddBuilding(Building building)
        {
            if (building == null) { throw new ArgumentNullException("building"); }

            if (!this.buildings.ContainsKey(building.ElementType.Name))
            {
                this.buildings.Add(building.ElementType.Name, new RCSet<Building>());
            }
            this.buildings[building.ElementType.Name].Add(building);
            building.OnAddedToPlayer(this);

            this.usedSupplyCache.Invalidate();
            this.totalSupplyCache.Invalidate();
        }

        /// <summary>
        /// Adds an addon to this player.
        /// </summary>
        /// <param name="addon">The addon to add to this player.</param>
        public void AddAddon(Addon addon)
        {
            if (addon == null) { throw new ArgumentNullException("addon"); }

            if (!this.addons.ContainsKey(addon.ElementType.Name))
            {
                this.addons.Add(addon.ElementType.Name, new RCSet<Addon>());
            }
            this.addons[addon.ElementType.Name].Add(addon);
            addon.OnAddedToPlayer(this);

            this.usedSupplyCache.Invalidate();
            this.totalSupplyCache.Invalidate();
        }

        /// <summary>
        /// Adds a unit to this player.
        /// </summary>
        /// <param name="unit">The unit to add to this player.</param>
        public void AddUnit(Unit unit)
        {
            if (unit == null) { throw new ArgumentNullException("unit"); }

            if (!this.units.ContainsKey(unit.ElementType.Name))
            {
                this.units.Add(unit.ElementType.Name, new RCSet<Unit>());
            }
            this.units[unit.ElementType.Name].Add(unit);
            unit.OnAddedToPlayer(this);

            this.usedSupplyCache.Invalidate();
            this.totalSupplyCache.Invalidate();
        }

        /// <summary>
        /// Adds an upgrade to this player.
        /// </summary>
        /// <param name="upgradeType">The type of the upgrade to add to this player.</param>
        public void AddUpgrade(string upgradeType)
        {
            if (upgradeType == null) { throw new ArgumentNullException("upgrade"); }
            if (this.upgrades.ContainsKey(upgradeType)) { throw new InvalidOperationException(string.Format("Upgrade of type '{0}' already exists for player {1}!", upgradeType, this.playerIndex.Read())); }

            Upgrade upgrade = new Upgrade(upgradeType);
            this.Scenario.AddElementToScenario(upgrade);
            this.upgrades[upgradeType] = upgrade;
            upgrade.OnAddedToPlayer(this);

            this.usedSupplyCache.Invalidate();
            this.totalSupplyCache.Invalidate();
        }

        /// <summary>
        /// Removes a building from this player.
        /// </summary>
        /// <param name="building">The building to be removed.</param>
        public void RemoveBuilding(Building building)
        {
            if (building == null) { throw new ArgumentNullException("building"); }

            if (!this.buildings.ContainsKey(building.ElementType.Name)) { throw new InvalidOperationException("The given building is not added to this player!"); }
            if (!this.buildings[building.ElementType.Name].Remove(building)) { throw new InvalidOperationException("The given building is not added to this player!"); }
            if (this.buildings[building.ElementType.Name].Count == 0)
            {
                this.buildings.Remove(building.ElementType.Name);
            }
            building.OnRemovedFromPlayer();

            this.usedSupplyCache.Invalidate();
            this.totalSupplyCache.Invalidate();
        }

        /// <summary>
        /// Removes an addon from this player.
        /// </summary>
        /// <param name="addon">The addon to be removed.</param>
        public void RemoveAddon(Addon addon)
        {
            if (addon == null) { throw new ArgumentNullException("addon"); }

            if (!this.addons.ContainsKey(addon.ElementType.Name)) { throw new InvalidOperationException("The given addon is not added to this player!"); }
            if (!this.addons[addon.ElementType.Name].Remove(addon)) { throw new InvalidOperationException("The given addon is not added to this player!"); }
            if (this.addons[addon.ElementType.Name].Count == 0)
            {
                this.addons.Remove(addon.ElementType.Name);
            }
            addon.OnRemovedFromPlayer();

            this.usedSupplyCache.Invalidate();
            this.totalSupplyCache.Invalidate();
        }

        /// <summary>
        /// Removes a unit from this player.
        /// </summary>
        /// <param name="unit">The unit to be removed.</param>
        public void RemoveUnit(Unit unit)
        {
            if (unit == null) { throw new ArgumentNullException("unit"); }

            if (!this.units.ContainsKey(unit.ElementType.Name)) { throw new InvalidOperationException("The given unit is not added to this player!"); }
            if (!this.units[unit.ElementType.Name].Remove(unit)) { throw new InvalidOperationException("The given unit is not added to this player!"); }
            if (this.units[unit.ElementType.Name].Count == 0)
            {
                this.units.Remove(unit.ElementType.Name);
            }
            unit.OnRemovedFromPlayer();

            this.usedSupplyCache.Invalidate();
            this.totalSupplyCache.Invalidate();
        }

        /// <summary>
        /// Removes an entity from this player.
        /// </summary>
        /// <param name="entity">The entity to be removed.</param>
        public void RemoveEntity(Entity entity)
        {
            if (entity == null) { throw new ArgumentNullException("entity"); }

            Unit entityAsUnit = entity as Unit;
            Building entityAsBuilding = entity as Building;
            Addon entityAsAddon = entity as Addon;

            if (entityAsUnit != null) { this.RemoveUnit(entityAsUnit); }
            else if (entityAsBuilding != null) { this.RemoveBuilding(entityAsBuilding); }
            else if (entityAsAddon != null) { this.RemoveAddon(entityAsAddon); }
            else
            {
                throw new InvalidOperationException("The given entity is neither a unit, an addon nor a building!");
            }

            this.usedSupplyCache.Invalidate();
            this.totalSupplyCache.Invalidate();
        }

        /// <summary>
        /// Removes the upgrade of the given type from this player.
        /// </summary>
        /// <param name="upgradeType">The type of the upgrade to be removed.</param>
        public void RemoveUpgrade(string upgradeType)
        {
            if (upgradeType == null) { throw new ArgumentNullException("upgradeType"); }
            if (!this.upgrades.ContainsKey(upgradeType)) { throw new InvalidOperationException("The given upgrade is not added to this player!"); }
            if (!this.upgrades[upgradeType].IsUnderResearch) { throw new InvalidOperationException("The research of the given upgrade is completed! Completed upgrades cannot be removed from a player!"); }

            this.upgrades[upgradeType].OnRemovedFromPlayer();
            this.Scenario.RemoveElementFromScenario(this.upgrades[upgradeType]);
            this.upgrades[upgradeType].Dispose();
            this.upgrades.Remove(upgradeType);

            this.usedSupplyCache.Invalidate();
            this.totalSupplyCache.Invalidate();
        }

        /// <summary>
        /// Checks whether this player has at least 1 building of the given type that is not under construction.
        /// </summary>
        /// <param name="buildingType">The name of the building type to check.</param>
        /// <returns>
        /// True if this player has at least 1 building of the given type that is not under construction; otherwise false.
        /// </returns>
        public bool HasBuilding(string buildingType)
        {
            if (buildingType == null) { throw new ArgumentNullException("buildingType"); }

            if (!this.buildings.ContainsKey(buildingType)) { return false; }
            return this.buildings[buildingType].Any(building => !building.Biometrics.IsUnderConstruction);
        }

        /// <summary>
        /// Checks whether this player has at least 1 addon of the given type that is not under construction.
        /// </summary>
        /// <param name="addonType">The name of the addon type to check.</param>
        /// <returns>
        /// True if this player has at least 1 addon of the given type that is not under construction; otherwise false.
        /// </returns>
        public bool HasAddon(string addonType)
        {
            if (addonType == null) { throw new ArgumentNullException("addonType"); }

            if (!this.addons.ContainsKey(addonType)) { return false; }
            return this.addons[addonType].Any(addon => !addon.Biometrics.IsUnderConstruction);
        }

        /// <summary>
        /// Checks whether this player has at least 1 unit of the given type that is not under construction.
        /// </summary>
        /// <param name="unitType">The name of the unit type to check.</param>
        /// <returns>
        /// True if this player has at least 1 unit of the given type that is not under construction; otherwise false.
        /// </returns>
        public bool HasUnit(string unitType)
        {
            if (unitType == null) { throw new ArgumentNullException("unitType"); }

            if (!this.units.ContainsKey(unitType)) { return false; }
            return this.units[unitType].Any(unit => !unit.Biometrics.IsUnderConstruction);
        }

        /// <summary>
        /// Gets the status of the given upgrade at this player.
        /// </summary>
        /// <param name="upgradeType">The name of the upgrade type to check.</param>
        /// <returns>
        /// The status of the given upgrade at this player.
        /// </returns>
        public UpgradeStatus GetUpgradeStatus(string upgradeType)
        {
            if (upgradeType == null) { throw new ArgumentNullException("upgradeType"); }

            if (!this.upgrades.ContainsKey(upgradeType)) { return UpgradeStatus.None; }
            return this.upgrades[upgradeType].IsUnderResearch ? UpgradeStatus.Researching : UpgradeStatus.Researched;
        }

        /// <summary>
        /// Takes the given amount of minerals and vespene gas from this player.
        /// </summary>
        /// <param name="minerals">The amount of minerals to be taken.</param>
        /// <param name="vespeneGas">The amount of vespene gas to be taken.</param>
        /// <returns>True if the given amount of resources has been taken successfully.</returns>
        public bool TakeResources(int minerals, int vespeneGas)
        {
            if (minerals < 0) { throw new ArgumentOutOfRangeException("minerals", "The amount of minerals to be taken cannot be negative!"); }
            if (vespeneGas < 0) { throw new ArgumentOutOfRangeException("vespeneGas", "The amount of vespene gas to be taken cannot be negative!"); }

            if (this.minerals.Read() < minerals || this.vespeneGas.Read() < vespeneGas) { return false; }

            this.minerals.Write(this.minerals.Read() - minerals);
            this.vespeneGas.Write(this.vespeneGas.Read() - vespeneGas);
            return true;
        }

        /// <summary>
        /// Gives the given amount of minerals and vespene gas to this player.
        /// </summary>
        /// <param name="minerals">The amount of minerals to be given.</param>
        /// <param name="vespeneGas">The amount of vespene gas to be given.</param>
        public void GiveResources(int minerals, int vespeneGas)
        {
            if (minerals < 0) { throw new ArgumentOutOfRangeException("minerals", "The amount of minerals to be given cannot be negative!"); }
            if (vespeneGas < 0) { throw new ArgumentOutOfRangeException("vespeneGas", "The amount of vespene gas to be given cannot be negative!"); }

            this.minerals.Write(this.minerals.Read() + minerals);
            this.vespeneGas.Write(this.vespeneGas.Read() + vespeneGas);
        }

        /// <summary>
        /// Locks the given amount of supplies of this player.
        /// </summary>
        /// <param name="supply">The amount of minerals to be locked.</param>
        /// <returns>True if the given amount of supplies has been locked successfully.</returns>
        public bool LockSupply(int supply)
        {
            if (supply < 0) { throw new ArgumentOutOfRangeException("supply", "The amount of supply to be locked cannot be negative!"); }

            if (supply > 0 && this.usedSupplyCache.Value + supply > this.totalSupplyCache.Value) { return false; }

            this.lockedSupplies.Write(this.lockedSupplies.Read() + supply);
            this.usedSupplyCache.Invalidate();
            return true;
        }

        /// <summary>
        /// Unlocks the given amount of supplies of this player.
        /// </summary>
        /// <param name="supply">The amount of minerals to be unlocked.</param>
        public void UnlockSupply(int supply)
        {
            if (supply < 0) { throw new ArgumentOutOfRangeException("supply", "The amount of supply to be unlocked cannot be negative!"); }
            if (supply > this.lockedSupplies.Read()) { throw new ArgumentOutOfRangeException("supply", "The amount of supply to be unlocked cannot be greater than the amount of actually locked supplies!"); }

            this.lockedSupplies.Write(this.lockedSupplies.Read() - supply);
            this.usedSupplyCache.Invalidate();
        }

        /// <summary>
        /// Gets the index of this player.
        /// </summary>
        public int PlayerIndex { get { return this.playerIndex.Read(); } }

        /// <summary>
        /// Gets the start position of this player.
        /// </summary>
        public RCNumVector StartPosition { get { return this.startPosition.Read(); } }

        /// <summary>
        /// Gets the quadratic start position of this player.
        /// </summary>
        public RCIntRectangle QuadraticStartPosition { get { return this.quadraticStartPosition.Read(); } }

        /// <summary>
        /// Gets the start location of this player.
        /// </summary>
        public StartLocation StartLocation { get { return this.startLocation.Read(); } }

        /// <summary>
        /// Gets the scenario that this player belongs to.
        /// </summary>
        public Scenario Scenario { get { return this.startLocation.Read().Scenario; } }

        /// <summary>
        /// Gets the entities of this player.
        /// </summary>
        public IEnumerable<Entity> Entities
        {
            get
            {
                foreach (RCSet<Building> buildings in this.buildings.Values)
                {
                    foreach (Building building in buildings) { yield return building; }
                }
                foreach (RCSet<Addon> addons in this.addons.Values)
                {
                    foreach (Addon addon in addons) { yield return addon; }
                }
                foreach (RCSet<Unit> units in this.units.Values)
                {
                    foreach (Unit unit in units) { yield return unit; }
                }
            }
        }

        /// <summary>
        /// Gets the current amount of minerals of this player.
        /// </summary>
        public int Minerals { get { return this.minerals.Read(); } }

        /// <summary>
        /// Gets the current amount of vespene gas of this player.
        /// </summary>
        public int VespeneGas { get { return this.vespeneGas.Read(); } }

        /// <summary>
        /// Gets the amount of the supplies currently used by this player.
        /// </summary>
        public int UsedSupply { get { return this.usedSupplyCache.Value; } }

        /// <summary>
        /// Gets the total amount of supplies owned by this player.
        /// </summary>
        public int TotalSupply { get { return this.totalSupplyCache.Value; } }

        /// <summary>
        /// Gets the maximum amount of supplies can be owned by this player.
        /// </summary>
        public int MaxSupply { get { return MAX_SUPPLY; } }

        /// <summary>
        /// Gets the player-level metadata.
        /// </summary>
        public IScenarioMetadata Metadata { get { return this.metadata; } }

        /// <summary>
        /// Gets the upgrade interface of the player-level metadata.
        /// </summary>
        public IScenarioMetadataUpgrade MetadataUpgrade { get { return this.metadataUpgrade; } }

        #region IDisposable methods

        /// <see cref="IDisposable.Dispose"/>
        protected override void DisposeImpl()
        {
            foreach (Upgrade upgrade in this.upgrades.Values)
            {
                upgrade.OnRemovedFromPlayer();
                this.Scenario.RemoveElementFromScenario(upgrade);
                upgrade.Dispose();
            }

            foreach (Entity entity in this.Entities) { entity.OnRemovedFromPlayer(); }
            this.units.Clear();
            this.buildings.Clear();
            this.addons.Clear();
            this.upgrades.Clear();
        }

        #endregion IDisposable methods

        /// <summary>
        /// Calculates the amount of supplies used by this player.
        /// </summary>
        /// <returns>The calculated value.</returns>
        private int CalculateUsedSupply()
        {
            int supplyUsed = this.lockedSupplies.Read();
            foreach (Entity entity in this.Entities)
            {
                if (!entity.Biometrics.IsUnderConstruction && entity.ElementType.SupplyUsed != null)
                {
                    supplyUsed += entity.ElementType.SupplyUsed.Read();
                }
            }
            foreach (Upgrade upgrade in this.upgrades.Values)
            {
                if (!upgrade.IsUnderResearch && upgrade.ElementType.SupplyUsed != null)
                {
                    supplyUsed += upgrade.ElementType.SupplyUsed.Read();
                }
            }
            return supplyUsed;
        }

        /// <summary>
        /// Calculates the total amount of supplies owned by this player.
        /// </summary>
        /// <returns></returns>
        private int CalculateTotalSupply()
        {
            int totalSupply = 0;
            foreach (Entity entity in this.Entities)
            {
                if (!entity.Biometrics.IsUnderConstruction && entity.ElementType.SupplyProvided != null)
                {
                    totalSupply += entity.ElementType.SupplyProvided.Read();
                    if (totalSupply >= MAX_SUPPLY) { return MAX_SUPPLY; }
                }
            }
            foreach (Upgrade upgrade in this.upgrades.Values)
            {
                if (!upgrade.IsUnderResearch && upgrade.ElementType.SupplyProvided != null)
                {
                    totalSupply += upgrade.ElementType.SupplyProvided.Read();
                }
            }
            return totalSupply;
        }

        #region Heaped members

        /// <summary>
        /// The index of the player.
        /// </summary>
        private readonly HeapedValue<int> playerIndex;

        /// <summary>
        /// The start location of the player.
        /// </summary>
        private readonly HeapedValue<StartLocation> startLocation;

        /// <summary>
        /// The start position of the player.
        /// </summary>
        private readonly HeapedValue<RCNumVector> startPosition;

        /// <summary>
        /// The quadratic start position of the player.
        /// </summary>
        private readonly HeapedValue<RCIntRectangle> quadraticStartPosition;

        /// <summary>
        /// The current amount of minerals of this player.
        /// </summary>
        private readonly HeapedValue<int> minerals;

        /// <summary>
        /// The current amount of vespene gas of this player.
        /// </summary>
        private readonly HeapedValue<int> vespeneGas;

        /// <summary>
        /// The amount of supplies locked temporarily (for example by production jobs).
        /// </summary>
        private readonly HeapedValue<int> lockedSupplies;

        #endregion Heaped members

        /// <summary>
        /// The buildings of the player grouped by their types.
        /// </summary>
        /// TODO: store the buildings also in a HeapedArray!
        private readonly Dictionary<string, RCSet<Building>> buildings;

        /// <summary>
        /// The addons of the player grouped by their types.
        /// </summary>
        /// TODO: store the buildings also in a HeapedArray!
        private readonly Dictionary<string, RCSet<Addon>> addons;

        /// <summary>
        /// The units of the player grouped by their types.
        /// </summary>
        /// TODO: store the units also in a HeapedArray!
        private readonly Dictionary<string, RCSet<Unit>> units;

        /// <summary>
        /// The upgrades of the player mapped by their types.
        /// </summary>
        /// TODO: store the upgrades also in a HeapedArray!
        private readonly Dictionary<string, Upgrade> upgrades; 

        /// <summary>
        /// The amount of the supplies currently used by this player.
        /// </summary>
        private readonly CachedValue<int> usedSupplyCache;

        /// <summary>
        /// The total amount of supplies owned by this player.
        /// </summary>
        private readonly CachedValue<int> totalSupplyCache;

        /// <summary>
        /// Reference to the player-level metadata.
        /// </summary>
        private readonly IScenarioMetadata metadata;

        /// <summary>
        /// Reference to the upgrade interface of the player-level metadata.
        /// </summary>
        private readonly IScenarioMetadataUpgrade metadataUpgrade;

        /// <summary>
        /// The maximum number of players.
        /// </summary>
        public const int MAX_PLAYERS = 8;

        /// <summary>
        /// Resources related constants.
        /// </summary>
        private const int INITIAL_MINERALS = 5000;
        private const int INITIAL_VESPENE_GAS = 5000;
        private const int MAX_SUPPLY = 200;
    }
}
