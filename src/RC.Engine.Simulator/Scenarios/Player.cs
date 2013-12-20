using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Scenarios
{
    /// <summary>
    /// Represents a player of a scenario.
    /// </summary>
    public class Player : IDisposable // TODO: derive from HeapedObject
    {
        /// <summary>
        /// Represents a method that creates some initial entitites for a player at its start location.
        /// </summary>
        /// <param name="player">The player that will own the initial entities.</param>
        public delegate void Initializer(Player player);

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
            if (!startLocation.Scenario.VisibleEntities.HasContent(startLocation)) { throw new SimulatorException("The given start location has already been initialized!"); }

            this.playerIndex = playerIndex;
            this.startLocation = startLocation;
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
        public int PlayerIndex { get { return this.playerIndex; } }

        /// <summary>
        /// Gets the start location of this player.
        /// </summary>
        public StartLocation StartLocation { get { return this.startLocation; } }

        #region IDisposable methods

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            foreach (Building building in this.buildings)
            {
                this.startLocation.Scenario.RemoveEntity(building);
            }
        }

        #endregion IDisposable methods

        /// <summary>
        /// The index of the player.
        /// </summary>
        private int playerIndex;

        /// <summary>
        /// The start location of the player.
        /// </summary>
        private StartLocation startLocation;

        /// <summary>
        /// The buildings of the player.
        /// </summary>
        private HashSet<Building> buildings;

        /// <summary>
        /// The units of the player.
        /// </summary>
        private HashSet<Unit> units;

        /// <summary>
        /// The maximum number of players.
        /// </summary>
        public const int MAX_PLAYERS = 8;
    }
}
