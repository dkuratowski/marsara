using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Scenarios
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
            if (startLocation.Scenario.GetEntityOnMap<StartLocation>(startLocation.ID.Read()) == null) { throw new SimulatorException("The given start location has already been initialized!"); }

            this.playerIndex = this.ConstructField<int>("playerIndex");
            this.startLocation = this.ConstructField<StartLocation>("startLocation");

            this.playerIndex.Write(playerIndex);
            this.startLocation.Write(startLocation);

            this.buildings = new HashSet<Building>();
            this.units = new HashSet<Unit>();
        }

        /// <summary>
        /// Adds a building to this player.
        /// </summary>
        /// <param name="building">The building to add to this player.</param>
        public void AddBuilding(Building building)
        {
            if (building == null) { throw new ArgumentNullException("building"); }
            this.buildings.Add(building);
            building.OnAddedToPlayer(this);
        }

        /// <summary>
        /// Adds a unit to this player.
        /// </summary>
        /// <param name="unit">The unit to add to this player.</param>
        public void AddUnit(Unit unit)
        {
            if (unit == null) { throw new ArgumentNullException("unit"); }
            this.units.Add(unit);
            unit.OnAddedToPlayer(this);
        }

        /// <summary>
        /// Removes a building from this player.
        /// </summary>
        /// <param name="building">The building to be removed.</param>
        public void RemoveBuilding(Building building)
        {
            if (building == null) { throw new ArgumentNullException("building"); }
            this.buildings.Remove(building);
            building.OnRemovedFromPlayer();
        }

        /// <summary>
        /// Removes a unit from this player.
        /// </summary>
        /// <param name="unit">The unit to be removed.</param>
        public void RemoveUnit(Unit unit)
        {
            if (unit == null) { throw new ArgumentNullException("unit"); }
            this.units.Remove(unit);
            unit.OnRemovedFromPlayer();
        }

        /// <summary>
        /// Gets the index of this player.
        /// </summary>
        public int PlayerIndex { get { return this.playerIndex.Read(); } }

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
                foreach (Building building in this.buildings) { yield return building; }
                foreach (Unit unit in this.units) { yield return unit; }
            }
        }

        #region IDisposable methods

        /// <see cref="IDisposable.Dispose"/>
        protected override void DisposeImpl()
        {
            foreach (Unit unit in this.units)
            {
                if (unit.PositionValue.Read() != RCNumVector.Undefined) { this.Scenario.DetachEntityFromMap(unit); }
                this.startLocation.Read().Scenario.RemoveEntityFromScenario(unit);
                unit.Dispose();
            }
            foreach (Building building in this.buildings)
            {
                if (building.PositionValue.Read() != RCNumVector.Undefined) { this.Scenario.DetachEntityFromMap(building); }
                this.startLocation.Read().Scenario.RemoveEntityFromScenario(building);
                building.Dispose();
            }
            this.units.Clear();
            this.buildings.Clear();
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

        #endregion Heaped members

        /// <summary>
        /// The buildings of the player.
        /// </summary>
        /// TODO: store the buildings also in a HeapedArray!
        private HashSet<Building> buildings;

        /// <summary>
        /// The units of the player.
        /// </summary>
        /// TODO: store the units also in a HeapedArray!
        private HashSet<Unit> units;

        /// <summary>
        /// The maximum number of players.
        /// </summary>
        public const int MAX_PLAYERS = 8;
    }
}
