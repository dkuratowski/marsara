using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Common.Configuration;

namespace RC.Engine.Simulator.Metadata.Core
{
    /// <summary>
    /// Static factory class for creating UpgradeEffectBase instances.
    /// </summary>
    static class UpgradeEffectFactory
    {
        /// <summary>
        /// Creates an upgrade effect.
        /// </summary>
        /// <param name="actionName">The name of the action to be executed by the effect.</param>
        /// <param name="parameterStr">The parameter of the effect or null if the effect has no parameter.</param>
        /// <param name="targetType">The target type of the effect.</param>
        /// <param name="metadata">The metadata object.</param>
        /// <returns>The created effect.</returns>
        public static UpgradeEffectBase CreateUpgradeEffect(string actionName, string parameterStr, string targetType, ScenarioMetadata metadata)
        {
            if (!upgradeEffectFactoryMethods.ContainsKey(actionName)) { throw new ArgumentException(string.Format("Factory method for upgrade effect '{0}' not defined!", actionName)); }

            return upgradeEffectFactoryMethods[actionName](parameterStr, targetType, metadata);
        }

        #region Effect actions

        /// <summary>
        /// This action sets the weapon level of a given type.
        /// </summary>
        /// <param name="upgradeInterface">The upgrade interface of the type.</param>
        /// <param name="weaponLevel">The new weapon level.</param>
        private static void SetWeaponLevel(IScenarioElementTypeUpgrade upgradeInterface, int weaponLevel)
        {
            foreach (IWeaponDataUpgrade weaponUpgradeInterface in upgradeInterface.StandardWeaponUpgrades)
            {
                weaponUpgradeInterface.DamageLevel = weaponLevel;
            }
        }

        /// <summary>
        /// This action sets the armor level of a given type.
        /// </summary>
        /// <param name="upgradeInterface">The upgrade interface of the type.</param>
        /// <param name="armorLevel">The new armor level.</param>
        private static void SetArmorLevel(IScenarioElementTypeUpgrade upgradeInterface, int armorLevel)
        {
            upgradeInterface.ArmorLevel = armorLevel;
        }

        /// <summary>
        /// This action upgrades the attack range of a given type.
        /// </summary>
        /// <param name="upgradeInterface">The upgrade interface of the type.</param>
        /// <param name="attackRangeUpgrade">The amount of the upgrade on the attack range.</param>
        private static void IncreaseAttackRange(IScenarioElementTypeUpgrade upgradeInterface, int attackRangeUpgrade)
        {
            foreach (IWeaponDataUpgrade weaponUpgradeInterface in upgradeInterface.StandardWeaponUpgrades)
            {
                weaponUpgradeInterface.RangeMaxUpgrade = attackRangeUpgrade;
            }
        }

        /// <summary>
        /// This action upgrades the speed of a given type.
        /// </summary>
        /// <param name="upgradeInterface">The upgrade interface of the type.</param>
        /// <param name="speedUpgrade">The amount of the upgrade on the speed.</param>
        private static void IncreaseSpeed(IScenarioElementTypeUpgrade upgradeInterface, RCNumber speedUpgrade)
        {
            upgradeInterface.SpeedUpgrade = speedUpgrade;
        }

        /// <summary>
        /// This action upgrades the energy of a given type.
        /// </summary>
        /// <param name="upgradeInterface">The upgrade interface of the type.</param>
        /// <param name="energyUpgrade">The amount of the upgrade on the energy.</param>
        private static void IncreaseEnergy(IScenarioElementTypeUpgrade upgradeInterface, int energyUpgrade)
        {
            upgradeInterface.MaxEnergyUpgrade = energyUpgrade;
        }

        /// <summary>
        /// This action upgrades the sight range of a given type.
        /// </summary>
        /// <param name="upgradeInterface">The upgrade interface of the type.</param>
        /// <param name="sightRangeUpgrade">The amount of the upgrade on the sight range.</param>
        private static void IncreaseSightRange(IScenarioElementTypeUpgrade upgradeInterface, int sightRangeUpgrade)
        {
            upgradeInterface.SightRangeUpgrade = sightRangeUpgrade;
        }

        #endregion Effect actions

        /// <summary>
        /// List of the supported upgrade effect factory method.
        /// </summary>
        private static readonly Dictionary<string, Func<string, string, ScenarioMetadata, UpgradeEffectBase>> upgradeEffectFactoryMethods =
            new Dictionary<string, Func<string, string, ScenarioMetadata, UpgradeEffectBase>>
        {
            { "setWeaponLevel", (parameterStr, targetType, metadata) => new UpgradeEffect<int>(UpgradeEffectFactory.SetWeaponLevel, XmlHelper.LoadInt(parameterStr), targetType, metadata) },
            { "setArmorLevel", (parameterStr, targetType, metadata) => new UpgradeEffect<int>(UpgradeEffectFactory.SetArmorLevel, XmlHelper.LoadInt(parameterStr), targetType, metadata) },
            { "increaseAttackRange", (parameterStr, targetType, metadata) => new UpgradeEffect<int>(UpgradeEffectFactory.IncreaseAttackRange, XmlHelper.LoadInt(parameterStr), targetType, metadata) },
            { "increaseSpeed", (parameterStr, targetType, metadata) => new UpgradeEffect<RCNumber>(UpgradeEffectFactory.IncreaseSpeed, XmlHelper.LoadNum(parameterStr), targetType, metadata) },
            { "increaseEnergy", (parameterStr, targetType, metadata) => new UpgradeEffect<int>(UpgradeEffectFactory.IncreaseEnergy, XmlHelper.LoadInt(parameterStr), targetType, metadata) },
            { "increaseSightRange", (parameterStr, targetType, metadata) => new UpgradeEffect<int>(UpgradeEffectFactory.IncreaseSightRange, XmlHelper.LoadInt(parameterStr), targetType, metadata) },
        };
    }
}
