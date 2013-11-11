using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Enumerates the possible sizes of a building/unit/addon.
    /// </summary>
    enum SizeEnum
    {
        [EnumMapping("Small")]
        Small = 0,
        [EnumMapping("Medium")]
        Medium = 1,
        [EnumMapping("Large")]
        Large = 2
    }

    /// <summary>
    /// Represents the general data of a building/unit/addon type.
    /// </summary>
    class GeneralData
    {
        /// <summary>
        /// Constructs a general data struct for a building/unit/addon type.
        /// </summary>
        public GeneralData()
        {
            this.area = null;
            this.armor = new ConstValue<int>(0);
            this.maxEnergy = new ConstValue<int>(0);
            this.maxHP = null;
            this.sightRange = null;
            this.size = null;
            this.speed = new ConstValue<RCNumber>(0);

            this.isFinalized = false;
        }

        /// <summary>
        /// Gets the area of the corresponding building/unit/addon type in map coordinates. No default value, must be set.
        /// </summary>
        public ConstValue<RCNumVector> Area
        {
            get { return this.area; }
            set
            {
                if (this.isFinalized) { throw new InvalidOperationException("Already finalized!"); }
                if (value == null) { throw new ArgumentNullException("Area"); }
                this.area = value;
            }
        }

        /// <summary>
        /// Gets the armor of the corresponding building/unit/addon type. Default value is 0.
        /// </summary>
        public ConstValue<int> Armor
        {
            get { return this.armor; }
            set
            {
                if (this.isFinalized) { throw new InvalidOperationException("Already finalized!"); }
                if (value == null) { throw new ArgumentNullException("Armor"); }
                this.armor = value;
            }
        }

        /// <summary>
        /// Gets the maximum energy of the corresponding building/unit/addon type. Default value is 0.
        /// </summary>
        public ConstValue<int> MaxEnergy
        {
            get { return this.maxEnergy; }
            set
            {
                if (this.isFinalized) { throw new InvalidOperationException("Already finalized!"); }
                if (value == null) { throw new ArgumentNullException("MaxEnergy"); }
                this.maxEnergy = value;
            }
        }

        /// <summary>
        /// Gets the maximum HP of the corresponding building/unit/addon type. No default value, must be set.
        /// </summary>
        public ConstValue<int> MaxHP
        {
            get { return this.maxHP; }
            set
            {
                if (this.isFinalized) { throw new InvalidOperationException("Already finalized!"); }
                if (value == null) { throw new ArgumentNullException("MaxHP"); }
                this.maxHP = value;
            }
        }

        /// <summary>
        /// Gets the sight range of the corresponding building/unit/addon type. No default value, must be set.
        /// </summary>
        public ConstValue<int> SightRange
        {
            get { return this.sightRange; }
            set
            {
                if (this.isFinalized) { throw new InvalidOperationException("Already finalized!"); }
                if (value == null) { throw new ArgumentNullException("SightRange"); }
                this.sightRange = value;
            }
        }

        /// <summary>
        /// Gets the size of the corresponding building/unit/addon type. No default value, must be set.
        /// </summary>
        public ConstValue<SizeEnum> Size
        {
            get { return this.size; }
            set
            {
                if (this.isFinalized) { throw new InvalidOperationException("Already finalized!"); }
                if (value == null) { throw new ArgumentNullException("Size"); }
                this.size = value;
            }
        }

        /// <summary>
        /// Gets the speed of the corresponding building/unit/addon type. Default value is 0.
        /// </summary>
        public ConstValue<RCNumber> Speed
        {
            get { return this.speed; }
            set
            {
                if (this.isFinalized) { throw new InvalidOperationException("Already finalized!"); }
                if (value == null) { throw new ArgumentNullException("Speed"); }
                this.speed = value;
            }
        }

        /// <summary>
        /// Validates and finalizes this data structure.
        /// </summary>
        public void CheckAndFinalize()
        {
            if (!this.isFinalized)
            {
                if (this.area == null) { throw new SimulatorException("Area must be set!"); }
                if (this.armor == null) { throw new SimulatorException("Armor must be set!"); }
                if (this.maxEnergy == null) { throw new SimulatorException("MaxEnergy must be set!"); }
                if (this.maxHP == null) { throw new SimulatorException("MaxHP must be set!"); }
                if (this.sightRange == null) { throw new SimulatorException("SightRange must be set!"); }
                if (this.size == null) { throw new SimulatorException("Size must be set!"); }
                if (this.speed == null) { throw new SimulatorException("Speed must be set!"); }

                if (this.area.Read().X <= 0 || this.area.Read().Y <= 0) { throw new SimulatorException("Area cannot be 0 or less in any directions!"); }
                if (this.armor.Read() < 0) { throw new SimulatorException("Armor must be non-negative!"); }
                if (this.maxEnergy.Read() < 0) { throw new SimulatorException("MaxEnergy must be non-negative!"); }
                if (this.maxHP.Read() <= 0) { throw new SimulatorException("MaxHP cannot be 0 or less!"); }
                if (this.sightRange.Read() < 0) { throw new SimulatorException("SightRange must be non-negative!"); }
                if (this.speed.Read() < 0) { throw new SimulatorException("Speed must be non-negative!"); }

                this.isFinalized = true;
            }
        }

        /// <summary>
        /// The values of this general data struct.
        /// </summary>
        private ConstValue<RCNumVector> area;
        private ConstValue<int> armor;
        private ConstValue<int> maxEnergy;
        private ConstValue<int> maxHP;
        private ConstValue<int> sightRange;
        private ConstValue<SizeEnum> size;
        private ConstValue<RCNumber> speed;

        /// <summary>
        /// Indicates whether this general data struct has been finalized or not.
        /// </summary>
        private bool isFinalized;
    }
}
