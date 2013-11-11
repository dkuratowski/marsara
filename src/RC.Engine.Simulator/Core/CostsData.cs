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
        /// Constructs a costs data struct for a building/unit/addon/upgrade type.
        /// </summary>
        public CostsData()
        {
            this.buildTime = null;
            this.foodCost = new ConstValue<int>(0);
            this.mineralCost = new ConstValue<int>(0);
            this.gasCost = new ConstValue<int>(0);

            this.isFinalized = false;
        }

        /// <summary>
        /// Gets the build time. No default value, must be set.
        /// </summary>
        public ConstValue<int> BuildTime
        {
            get { return this.buildTime; }
            set
            {
                if (this.isFinalized) { throw new InvalidOperationException("Already finalized!"); }
                if (value == null) { throw new ArgumentNullException("BuildTime"); }
                this.buildTime = value;
            }
        }

        /// <summary>
        /// Gets the food cost. Default value is 0.
        /// </summary>
        public ConstValue<int> FoodCost
        {
            get { return this.foodCost; }
            set
            {
                if (this.isFinalized) { throw new InvalidOperationException("Already finalized!"); }
                if (value == null) { throw new ArgumentNullException("FoodCost"); }
                this.foodCost = value;
            }
        }

        /// <summary>
        /// Gets the mineral cost. Default value is 0.
        /// </summary>
        public ConstValue<int> MineralCost
        {
            get { return this.mineralCost; }
            set
            {
                if (this.isFinalized) { throw new InvalidOperationException("Already finalized!"); }
                if (value == null) { throw new ArgumentNullException("MineralCost"); }
                this.mineralCost = value;
            }
        }

        /// <summary>
        /// Gets the gas cost. Default value is 0.
        /// </summary>
        public ConstValue<int> GasCost
        {
            get { return this.gasCost; }
            set
            {
                if (this.isFinalized) { throw new InvalidOperationException("Already finalized!"); }
                if (value == null) { throw new ArgumentNullException("GasCost"); }
                this.gasCost = value;
            }
        }

        /// <summary>
        /// Validates and finalizes this data structure.
        /// </summary>
        public void CheckAndFinalize()
        {
            if (!this.isFinalized)
            {
                if (this.buildTime == null) { throw new SimulatorException("BuildTime must be set!"); }
                if (this.foodCost == null) { throw new SimulatorException("FoodCost must be set!"); }
                if (this.mineralCost == null) { throw new SimulatorException("MineralCost must be set!"); }
                if (this.gasCost == null) { throw new SimulatorException("GasCost must be set!"); }

                if (this.buildTime.Read() < 0) { throw new SimulatorException("BuildTime must be non-negative!"); }
                if (this.foodCost.Read() < 0) { throw new SimulatorException("FoodCost must be non-negative!"); }
                if (this.mineralCost.Read() < 0) { throw new SimulatorException("MineralCost must be non-negative!"); }
                if (this.gasCost.Read() < 0) { throw new SimulatorException("GasCost must be non-negative!"); }

                this.isFinalized = true;
            }
        }

        /// <summary>
        /// The values of this costs data struct.
        /// </summary>
        private ConstValue<int> buildTime;
        private ConstValue<int> foodCost;
        private ConstValue<int> mineralCost;
        private ConstValue<int> gasCost;

        /// <summary>
        /// Indicates whether this costs data struct has been finalized or not.
        /// </summary>
        private bool isFinalized;
    }
}
