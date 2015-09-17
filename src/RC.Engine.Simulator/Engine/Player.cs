using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Engine
{
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
            if (startLocation.Scenario.GetElementOnMap<StartLocation>(startLocation.ID.Read()) == null) { throw new SimulatorException("The given start location has already been initialized!"); }

            this.playerIndex = this.ConstructField<int>("playerIndex");
            this.startLocation = this.ConstructField<StartLocation>("startLocation");
            this.startPosition = this.ConstructField<RCNumVector>("startPosition");

            this.playerIndex.Write(playerIndex);
            this.startLocation.Write(startLocation);
            this.startPosition.Write(startLocation.MotionControl.PositionVector.Read());

            this.buildings = new Dictionary<string, RCSet<Building>>();
            this.addons = new Dictionary<string, RCSet<Addon>>();
            this.units = new Dictionary<string, RCSet<Unit>>();
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
        }

        /// <summary>
        /// Checks whether this player has at least 1 entity of the given type.
        /// </summary>
        /// <param name="scenarioElementType">The name of the type to check.</param>
        /// <returns>True if this player has at least 1 entity of the given type; otherwise false.</returns>
        public bool HasEntity(string scenarioElementType)
        {
            if (scenarioElementType == null) { throw new ArgumentNullException("scenarioElementType"); }

            return this.buildings.ContainsKey(scenarioElementType) ||
                   this.addons.ContainsKey(scenarioElementType) ||
                   this.units.ContainsKey(scenarioElementType);
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

        #region IDisposable methods

        /// <see cref="IDisposable.Dispose"/>
        protected override void DisposeImpl()
        {
            foreach (Entity entity in this.Entities) { entity.OnRemovedFromPlayer(); }
            this.units.Clear();
            this.buildings.Clear();
            this.addons.Clear();
        }

        #endregion IDisposable methods

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
        /// The maximum number of players.
        /// </summary>
        public const int MAX_PLAYERS = 8;
    }
}
