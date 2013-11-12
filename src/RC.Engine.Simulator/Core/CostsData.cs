using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Stores the costs data of a building/unit/addon/upgrade type.
    /// </summary>
    class CostsData
    {
        /// <summary>
        /// Constructs a costs data struct for an object type.
        /// </summary>
        /// <param name="metadata">The metadata object that this cost information belongs to.</param>
        public CostsData(SimMetadata metadata)
        {
            if (metadata == null) { throw new ArgumentNullException("metadata"); }

            this.buildTime = null;
            this.foodCost = new ConstValue<int>(0);
            this.mineralCost = new ConstValue<int>(0);
            this.gasCost = new ConstValue<int>(0);

            this.metadata = metadata;
        }

        /// <summary>
        /// Gets the build time.
        /// </summary>
        public ConstValue<int> BuildTime { get { return this.buildTime; } }

        /// <summary>
        /// Gets the food cost.
        /// </summary>
        public ConstValue<int> FoodCost { get { return this.foodCost; } }

        /// <summary>
        /// Gets the mineral cost.
        /// </summary>
        public ConstValue<int> MineralCost { get { return this.mineralCost; } }

        /// <summary>
        /// Gets the gas cost.
        /// </summary>
        public ConstValue<int> GasCost { get { return this.gasCost; } }

        #region CostsData buildup methods

        /// <summary>
        /// Sets the build time. No default value, must be set.
        /// </summary>
        public void SetBuildTime(int buildTime)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            this.buildTime = new ConstValue<int>(buildTime);
        }

        /// <summary>
        /// Sets the food cost. Default value is 0.
        /// </summary>
        public void SetFoodCost(int foodCost)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            this.foodCost = new ConstValue<int>(foodCost);
        }

        /// <summary>
        /// Sets the mineral cost. Default value is 0.
        /// </summary>
        public void SetMineralCost(int mineralCost)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            this.mineralCost = new ConstValue<int>(mineralCost);
        }

        /// <summary>
        /// Sets the gas cost. Default value is 0.
        /// </summary>
        public void SetGasCost(int gasCost)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            this.gasCost = new ConstValue<int>(gasCost);
        }

        /// <summary>
        /// Validates and finalizes this data structure.
        /// </summary>
        public void CheckAndFinalize()
        {
            if (!this.metadata.IsFinalized)
            {
                if (this.buildTime == null) { throw new SimulatorException("BuildTime must be set!"); }
                if (this.foodCost == null) { throw new SimulatorException("FoodCost must be set!"); }
                if (this.mineralCost == null) { throw new SimulatorException("MineralCost must be set!"); }
                if (this.gasCost == null) { throw new SimulatorException("GasCost must be set!"); }

                if (this.buildTime.Read() < 0) { throw new SimulatorException("BuildTime must be non-negative!"); }
                if (this.foodCost.Read() < 0) { throw new SimulatorException("FoodCost must be non-negative!"); }
                if (this.mineralCost.Read() < 0) { throw new SimulatorException("MineralCost must be non-negative!"); }
                if (this.gasCost.Read() < 0) { throw new SimulatorException("GasCost must be non-negative!"); }
            }
        }

        #endregion CostsData uuildup methods

        /// <summary>
        /// The values of this costs data struct.
        /// </summary>
        private ConstValue<int> buildTime;
        private ConstValue<int> foodCost;
        private ConstValue<int> mineralCost;
        private ConstValue<int> gasCost;

        /// <summary>
        /// Reference to the metadata object that this cost information belongs to.
        /// </summary>
        private SimMetadata metadata;
    }
}
