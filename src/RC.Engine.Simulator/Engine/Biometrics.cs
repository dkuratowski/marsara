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
            this.frameIndexOfLastEnemyDamage = this.ConstructField<int>("frameIndexOfLastEnemyDamage");

            this.owner.Write(owner);
            this.hp.Write(owner.ElementType.MaxHP != null ? owner.ElementType.MaxHP.Read() : -1);
            this.frameIndexOfLastEnemyDamage.Write(-1);
        }

        /// <summary>
        /// Gets the HP of the owner entity or -1 if the owner entity is not attackable.
        /// </summary>
        public RCNumber HP { get { return this.hp.Read(); } }

        /// <summary>
        /// Gets the index of the frame in which the owner of this biometrics has been damaged by an enemy or -1 if the owner has
        /// not yet been attacked by an enemy.
        /// </summary>
        public int FrameIndexOfLastEnemyDamage { get { return this.frameIndexOfLastEnemyDamage.Read(); } }

        /// <summary>
        /// Makes a damage on the owner of this biometrics.
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
        /// Reference to the owner of this biometrics.
        /// </summary>
        private readonly HeapedValue<Entity> owner;

        /// <summary>
        /// The HP of the owner entity.
        /// </summary>
        private readonly HeapedValue<RCNumber> hp;

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
    }
}
