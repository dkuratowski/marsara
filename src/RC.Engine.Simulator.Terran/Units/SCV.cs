using System;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Behaviors;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.MotionControl;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.Terran.Buildings;

namespace RC.Engine.Simulator.Terran.Units
{
    /// <summary>
    /// Represents a Terran SCV.
    /// </summary>
    class SCV : Unit
    {
        /// <summary>
        /// Constructs a Terran SCV instance.
        /// </summary>
        public SCV()
            : base(SCV_TYPE_NAME, false, new BasicAnimationsBehavior("Moving", "Stopped", "Stopped"))
        {
            this.buildingUnderConstruction = this.ConstructField<TerranBuilding>("buildingUnderConstruction");
            this.buildingUnderConstruction.Write(null);
        }

        /// <summary>
        /// Gets whether this SCV is currently constructing a building.
        /// </summary>
        public bool IsConstructing { get { return this.buildingUnderConstruction.Read() != null; } }

        /// <summary>
        /// TODO: remove when TerranBuildingProductionJob implemented!
        /// </summary>
        public Building BuildingUnderConstruction { get { return this.buildingUnderConstruction.Read(); } }

        /// <summary>
        /// Places the given building to the given quadratic tile and starts its construction.
        /// </summary>
        /// <param name="building">The building to be placed.</param>
        /// <param name="topLeftQuadTile"></param>
        /// <returns>True if starting the construction of the building succeeded; otherwise false.</returns>
        public bool StartConstruct(TerranBuilding building, RCIntVector topLeftQuadTile)
        {
            if (topLeftQuadTile == RCIntVector.Undefined) { throw new ArgumentNullException("topLeftQuadTile"); }
            if (building == null) { throw new ArgumentNullException("building"); }
            if (building.Scenario != this.Scenario) { throw new ArgumentException("The given building shall belong to the same scenario as this SCV", "building"); }
            if (this.IsConstructing) { throw new InvalidOperationException("This SCV is already constructing a building!"); }
            if (building.Biometrics.IsUnderConstruction) { throw new InvalidOperationException("The given building is currently under construction!"); }

            /// Take the necessary amount of minerals and vespene gas from the owner player.
            int mineralsNeeded = building.BuildingType.MineralCost != null ? building.BuildingType.MineralCost.Read() : 0;
            int vespeneGasNeeded = building.BuildingType.GasCost != null ? building.BuildingType.GasCost.Read() : 0;
            bool resourcesLockedSuccessfully = this.Owner.TakeResources(mineralsNeeded, vespeneGasNeeded);
            if (!resourcesLockedSuccessfully)
            {
                /// TODO: send a message to the user about insufficient resources!
                return false;
            }

            /// Try to attach the given building onto the map.
            this.buildingUnderConstruction.Write(building);
            this.Owner.AddBuilding(this.buildingUnderConstruction.Read());
            bool buildingPlacedSuccessfully = this.buildingUnderConstruction.Read().AttachToMapAndStartConstruction(this.Scenario.Map.GetQuadTile(topLeftQuadTile));
            if (!buildingPlacedSuccessfully)
            {
                this.Owner.RemoveBuilding(this.buildingUnderConstruction.Read());
                this.buildingUnderConstruction.Write(null);
            }

            return buildingPlacedSuccessfully;
        }

        /// <summary>
        /// Continues the construction of the building that this SCV is currently constructing.
        /// </summary>
        /// <returns>True if the construction has finished; otherwise false.</returns>
        public bool ContinueConstruct()
        {
            if (!this.IsConstructing) { throw new InvalidOperationException("This SCV is not constructing any building!"); }

            if (this.buildingUnderConstruction.Read().Scenario == null)
            {
                /// The building has been destroyed -> do not continue the construction!
                this.buildingUnderConstruction.Write(null);
                return true;
            }

            this.buildingUnderConstruction.Read().Biometrics.Construct();
            if (!this.buildingUnderConstruction.Read().Biometrics.IsUnderConstruction)
            {
                /// Construction finished.
                this.buildingUnderConstruction.Write(null);
            }

            return !this.IsConstructing;
        }

        /// <see cref="Entity.DestructionAnimationName"/>
        protected override string DestructionAnimationName { get { return "Dying"; } }

        /// <summary>
        /// Reference to the building that is currently under construction.
        /// </summary>
        private readonly HeapedValue<TerranBuilding> buildingUnderConstruction;

        /// <summary>
        /// The name of the SCV element type.
        /// </summary>
        public const string SCV_TYPE_NAME = "SCV";
    }
}
