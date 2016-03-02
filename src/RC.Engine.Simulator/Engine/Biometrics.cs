using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// Reponsible for handling all the biometric data of a given entity.
    /// </summary>
    public class Biometrics : HeapedObject
    {
        /// <summary>
        /// Constructs a biometrics instance for the given entity.
        /// </summary>
        /// <param name="owner">The owner of this biometrics.</param>
        public Biometrics(Entity owner)
        {
            if (owner == null) { throw new ArgumentNullException("owner"); }

            this.owner = this.ConstructField<Entity>("owner");
            this.hp = this.ConstructField<RCNumber>("hp");
            this.energy = this.ConstructField<RCNumber>("energy");
            this.constructionProgress = this.ConstructField<int>("constructionProgress");
            this.frameIndexOfLastEnemyDamage = this.ConstructField<int>("frameIndexOfLastEnemyDamage");

            this.owner.Write(owner);
            this.hp.Write(owner.ElementType.MaxHP != null ? owner.ElementType.MaxHP.Read() : -1);
            this.energy.Write(owner.ElementType.MaxEnergy != null ? owner.ElementType.MaxEnergy.Read() : -1);
            this.constructionProgress.Write(-1);
            this.frameIndexOfLastEnemyDamage.Write(-1);
        }

        /// <summary>
        /// Gets the HP of the owner entity or -1 if the owner entity is not attackable.
        /// </summary>
        public RCNumber HP { get { return this.hp.Read(); } }

        /// <summary>
        /// Gets the energy of the owner entity or -1 if the owner entity has no energy.
        /// </summary>
        public RCNumber Energy { get { return this.energy.Read(); } }

        /// <summary>
        /// Gets the current construction progress of the owner entity.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the owner entity is not under construction.</exception>
        public int ConstructionProgress
        {
            get
            {
                if (!this.IsUnderConstruction) { throw new InvalidOperationException("The owner entity is not under construction!"); }
                if (this.constructionProgress.Read() == -1) { return -1; }
                if (this.constructionProgress.Read() == this.owner.Read().ElementType.BuildTime.Read()) { return -1; }
                return this.constructionProgress.Read();
            }
        }

        /// <summary>
        /// Gets whether the owner entity is currently under construction.
        /// </summary>
        public bool IsUnderConstruction
        {
            get
            {
                return this.constructionProgress.Read() != -1 && this.constructionProgress.Read() != this.owner.Read().ElementType.BuildTime.Read();
            }
        }

        /// <summary>
        /// Gets the index of the frame in which the owner of this biometrics has been damaged by an enemy or -1 if the owner has
        /// not yet been attacked by an enemy.
        /// </summary>
        public int FrameIndexOfLastEnemyDamage { get { return this.frameIndexOfLastEnemyDamage.Read(); } }

        /// <summary>
        /// Makes a damage on the owner of this biometrics based on the given damage type and value.
        /// </summary>
        /// <param name="damageType">The type of the damage.</param>
        /// <param name="damageValue">The amount of the damage.</param>
        /// <param name="isFriendlyDamage">This flag indicates whether this is a friendly damage or not.</param>
        public void Damage(DamageTypeEnum damageType, int damageValue, bool isFriendlyDamage)
        {
            if (this.owner.Read().ElementType.Size == null) { throw new InvalidOperationException("Unable to make damage on non-attackable entities!"); }
            if (this.owner.Read().ElementType.Armor == null) { throw new InvalidOperationException("Unable to make damage on non-attackable entities!"); }
            if (this.hp.Read() == -1) { throw new InvalidOperationException("Unable to make damage on non-attackable entities!"); }

            SizeEnum size = this.owner.Read().ElementType.Size.Read();
            int armor = this.owner.Read().ElementType.Armor.Read();
            RCNumber finalDamage = Math.Max(0, damageValue - armor) * DAMAGE_EFFECTIVENESS_TABLE[Tuple.Create(damageType, size)];
            if (finalDamage < (RCNumber) 1/(RCNumber) 2)
            {
                finalDamage = (RCNumber) 1/(RCNumber) 2;
            }

            RCNumber newHP = this.hp.Read() - finalDamage;
            if (newHP < 0) { newHP = 0; }

            this.hp.Write(newHP);
            if (!isFriendlyDamage) { this.frameIndexOfLastEnemyDamage.Write(this.owner.Read().Scenario.CurrentFrameIndex); }
        }

        /// <summary>
        /// Makes an absolute damage on the owner of this biometrics.
        /// </summary>
        /// <param name="damageValue">The amount of the damage.</param>
        public void Damage(RCNumber damageValue)
        {
            if (this.owner.Read().ElementType.Size == null) { throw new InvalidOperationException("Unable to make damage on non-attackable entities!"); }
            if (this.owner.Read().ElementType.Armor == null) { throw new InvalidOperationException("Unable to make damage on non-attackable entities!"); }
            if (this.hp.Read() == -1) { throw new InvalidOperationException("Unable to make damage on non-attackable entities!"); }
            
            RCNumber newHP = this.hp.Read() - damageValue;
            if (newHP < 0) { newHP = 0; }
            this.hp.Write(newHP);
        }

        /// <summary>
        /// Perform a repair step on the owner entity.
        /// </summary>
        /// <returns>True if the repair step performed successfully; otherwise false.</returns>
        public bool Repair()
        {
            if (this.owner.Read().ElementType.BuildTime == null) { throw new InvalidOperationException("Unable to repair entities if build time is not defined in the metadata!"); }
            if (this.hp.Read() == -1) { throw new InvalidOperationException("Unable to repair non-attackable entities!"); }
            if (this.IsUnderConstruction) { throw new InvalidOperationException("Unable to repair entities under construction!"); }

            /// If we reached the maximum HP -> repair step failed to perform.
            if (this.hp.Read() == this.owner.Read().ElementType.MaxHP.Read()) { return false; }

            /// Calculate the HP to add and the resources to take in this repair step.
            RCNumber hpToAdd = (RCNumber)this.owner.Read().ElementType.MaxHP.Read() / (RCNumber)this.owner.Read().ElementType.BuildTime.Read();
            RCNumber totalOriginalMineralCost = this.owner.Read().ElementType.MineralCost != null
                ? this.owner.Read().ElementType.MineralCost.Read()
                : 0;
            RCNumber mineralsToTake = totalOriginalMineralCost * REPAIR_COST_RATE / this.owner.Read().ElementType.BuildTime.Read();
            RCNumber totalOriginalGasCost = this.owner.Read().ElementType.GasCost != null
                ? this.owner.Read().ElementType.GasCost.Read()
                : 0;
            RCNumber gasToTake = totalOriginalGasCost * REPAIR_COST_RATE / this.owner.Read().ElementType.BuildTime.Read();

            /// Try to take the necessary resources from the player of the owner entity (if it has a player).
            if (this.owner.Read().Owner != null && !this.owner.Read().Owner.TakeResources(mineralsToTake, gasToTake))
            {
                /// Not enough resources -> repair step failed to perform.
                return false;
            }

            /// Necessary resources taken successfully (or the entity is neutral) -> modify the HP value and return with success.
            RCNumber newHP = this.hp.Read() + hpToAdd;
            if (newHP > this.owner.Read().ElementType.MaxHP.Read()) { newHP = this.owner.Read().ElementType.MaxHP.Read(); }
            this.hp.Write(newHP);
            return true;
        }

        /// <summary>
        /// Begins or continues the construction progress of the owner entity.
        /// </summary>
        public void Construct()
        {
            if (this.owner.Read().ElementType.BuildTime == null) { throw new InvalidOperationException("Unable to construct entities if build time is not defined in the metadata!"); }
            if (this.hp.Read() == -1) { throw new InvalidOperationException("Unable to construct non-attackable entities!"); }

            /// Do nothing if the HP reached 0.
            if (this.hp.Read() == 0) { return; }

            if (this.constructionProgress.Read() == -1)
            {
                /// Begin the construction starting from 10% of MaxHP.
                this.constructionProgress.Write(0);
                this.hp.Write((RCNumber)this.owner.Read().ElementType.MaxHP.Read() / (RCNumber)10);
                if (this.energy.Read() != -1)
                {
                    /// Starting from 50 (or MaxEnergy) at the beginning of the construction.
                    this.energy.Write(Math.Min(this.owner.Read().ElementType.MaxEnergy.Read(), INITIAL_ENERGY));
                }
            }
            else
            {
                /// Do nothing if the construction has already completed.
                if (!this.IsUnderConstruction) { return; }

                /// Continue construction and increment HP accordingly.
                this.constructionProgress.Write(this.constructionProgress.Read() + 1);
                RCNumber hpIncrement = ((RCNumber)this.owner.Read().ElementType.MaxHP.Read() * (RCNumber)9)
                                     / ((RCNumber)this.owner.Read().ElementType.BuildTime.Read() * (RCNumber)10);
                this.hp.Write(this.hp.Read() + hpIncrement);
                if (this.hp.Read() > this.owner.Read().ElementType.MaxHP.Read())
                {
                    this.hp.Write(this.owner.Read().ElementType.MaxHP.Read());
                }
            }
        }

        /// <summary>
        /// Cancels the construction and destroys the owner entity.
        /// </summary>
        public void CancelConstruct()
        {
            if (!this.IsUnderConstruction) { throw new InvalidOperationException("The owner entity is not under construction!"); }

            this.hp.Write(0);
            this.constructionProgress.Write(-1);

            /// Give back the 75% of the locked resources to the player of the owner building (if the player exists).
            if (this.owner.Read().Owner != null)
            {
                int mineralCost = this.owner.Read().ElementType.MineralCost != null ? this.owner.Read().ElementType.MineralCost.Read() : 0;
                int vespeneGasCost = this.owner.Read().ElementType.GasCost != null ? this.owner.Read().ElementType.GasCost.Read() : 0;
                this.owner.Read().Owner.GiveResources((int)(mineralCost * (RCNumber)3 / (RCNumber)4), (int)(vespeneGasCost * (RCNumber)3 / (RCNumber)4));
            }
        }

        /// <summary>
        /// Reference to the owner of this biometrics.
        /// </summary>
        private readonly HeapedValue<Entity> owner;

        /// <summary>
        /// The HP of the owner entity.
        /// </summary>
        private readonly HeapedValue<RCNumber> hp;

        /// <summary>
        /// The energy of the owner entity.
        /// </summary>
        private readonly HeapedValue<RCNumber> energy;

        /// <summary>
        /// The current construction progress of the owner entity or -1 if the owner entity has no construction progress.
        /// </summary>
        private readonly HeapedValue<int> constructionProgress;

        /// <summary>
        /// The index of the frame in which the owner of this biometrics has been damaged by an enemy or -1 if the owner has
        /// not yet been attacked by an enemy.
        /// </summary>
        private readonly HeapedValue<int> frameIndexOfLastEnemyDamage; 

        /// <summary>
        /// The hardcoded damage effectiveness table (see: RC10SF_DAMAGECALCULATION).
        /// </summary>
        private static readonly Dictionary<Tuple<DamageTypeEnum, SizeEnum>, RCNumber> DAMAGE_EFFECTIVENESS_TABLE =
            new Dictionary<Tuple<DamageTypeEnum, SizeEnum>, RCNumber>
        {
            { Tuple.Create(DamageTypeEnum.Normal, SizeEnum.Small), 1 },
            { Tuple.Create(DamageTypeEnum.Normal, SizeEnum.Medium), 1 },
            { Tuple.Create(DamageTypeEnum.Normal, SizeEnum.Large), 1 },
            { Tuple.Create(DamageTypeEnum.Concussive, SizeEnum.Small), 1 },
            { Tuple.Create(DamageTypeEnum.Concussive, SizeEnum.Medium), (RCNumber)1/(RCNumber)2 },
            { Tuple.Create(DamageTypeEnum.Concussive, SizeEnum.Large), (RCNumber)1/(RCNumber)4 },
            { Tuple.Create(DamageTypeEnum.Explosive, SizeEnum.Small), (RCNumber)1/(RCNumber)2 },
            { Tuple.Create(DamageTypeEnum.Explosive, SizeEnum.Medium), (RCNumber)3/(RCNumber)4 },
            { Tuple.Create(DamageTypeEnum.Explosive, SizeEnum.Large), 1 },
        };

        /// <summary>
        /// The initial energy value of the owner entity when its construction begins.
        /// </summary>
        private const int INITIAL_ENERGY = 50;

        /// <summary>
        /// The rate of the cost of repairing an entity compared to its original cost.
        /// </summary>
        private static readonly RCNumber REPAIR_COST_RATE = (RCNumber)1 / (RCNumber)3;
    }
}
