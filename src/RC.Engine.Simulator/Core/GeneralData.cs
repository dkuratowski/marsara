using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Enumerates the possible sizes of an entity.
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
    /// Represents the general data of an entity type.
    /// </summary>
    class GeneralData
    {
        /// <summary>
        /// Constructs a general data struct for an entity type.
        /// </summary>
        /// <param name="metadata">The metadata object that this GeneralData belongs to.</param>
        public GeneralData(SimMetadata metadata)
        {
            if (metadata == null) { throw new ArgumentNullException("metadata"); }

            this.area = null;
            this.armor = new ConstValue<int>(0);
            this.maxEnergy = new ConstValue<int>(0);
            this.maxHP = null;
            this.sightRange = null;
            this.size = null;
            this.speed = new ConstValue<RCNumber>(0);

            this.metadata = metadata;
        }

        /// <summary>
        /// Gets the area of the corresponding entity type in map coordinates.
        /// </summary>
        public ConstValue<RCNumVector> Area { get { return this.area; }  }

        /// <summary>
        /// Gets the armor of the corresponding entity type.
        /// </summary>
        public ConstValue<int> Armor { get { return this.armor; } }

        /// <summary>
        /// Gets the maximum energy of the corresponding entity type.
        /// </summary>
        public ConstValue<int> MaxEnergy { get { return this.maxEnergy; } }

        /// <summary>
        /// Gets the maximum HP of the corresponding entity type.
        /// </summary>
        public ConstValue<int> MaxHP { get { return this.maxHP; } }

        /// <summary>
        /// Gets the sight range of the corresponding entity type.
        /// </summary>
        public ConstValue<int> SightRange { get { return this.sightRange; } }

        /// <summary>
        /// Gets the size of the corresponding entity type.
        /// </summary>
        public ConstValue<SizeEnum> Size { get { return this.size; } }

        /// <summary>
        /// Gets the speed of the corresponding entity type.
        /// </summary>
        public ConstValue<RCNumber> Speed { get { return this.speed; } }

        #region GeneralData buildup methods

        /// <summary>
        /// Sets the area of the corresponding entity type in map coordinates. No default value, must be set.
        /// </summary>
        /// <param name="area">The area vector.</param>
        public void SetArea(RCNumVector area)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (area == RCNumVector.Undefined) { throw new ArgumentNullException("area"); }
            this.area = new ConstValue<RCNumVector>(area);
        }

        /// <summary>
        /// Sets the armor of the corresponding entity type. Default value is 0.
        /// </summary>
        /// <param name="armor">The armor value.</param>
        public void SetArmor(int armor)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            this.armor = new ConstValue<int>(armor);
        }

        /// <summary>
        /// Sets the maximum energy of the corresponding entity type. Default value is 0.
        /// </summary>
        public void SetMaxEnergy(int maxEnergy)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            this.maxEnergy = new ConstValue<int>(maxEnergy);
        }

        /// <summary>
        /// Sets the maximum HP of the corresponding entity type. No default value, must be set.
        /// </summary>
        public void SetMaxHP(int maxHP)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            this.maxHP = new ConstValue<int>(maxHP);
        }

        /// <summary>
        /// Sets the sight range of the corresponding entity type. No default value, must be set.
        /// </summary>
        public void SetSightRange(int sightRange)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            this.sightRange = new ConstValue<int>(sightRange);
        }

        /// <summary>
        /// Sets the size of the corresponding entity type. No default value, must be set.
        /// </summary>
        public void SetSize(SizeEnum size)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            this.size = new ConstValue<SizeEnum>(size);
        }

        /// <summary>
        /// Sets the speed of the corresponding entity type. Default value is 0.
        /// </summary>
        public void SetSpeed(RCNumber speed)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            this.speed = new ConstValue<RCNumber>(speed);
        }

        /// <summary>
        /// Validates and finalizes this data structure.
        /// </summary>
        public void CheckAndFinalize()
        {
            if (!this.metadata.IsFinalized)
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
            }
        }

        #endregion GeneralData buildup methods

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
        /// The metadata object that this GeneralData belongs to.
        /// </summary>
        private SimMetadata metadata;
    }
}
