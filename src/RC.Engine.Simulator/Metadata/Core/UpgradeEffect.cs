using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.Metadata.Core;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Metadata.Core
{
    /// <summary>
    /// Represents an effect of an upgrade.
    /// </summary>
    abstract class UpgradeEffectBase : IUpgradeEffect
    {
        #region IUpgradeEffect methods

        /// <see cref="IUpgradeEffect.Perform"/>
        public abstract void Perform(IScenarioMetadataUpgrade metadataUpgrade);

        #endregion IUpgradeEffect methods

        #region UpgradeEffect buildup methods

        /// <summary>
        /// Checks and finalizes this effect definition.
        /// </summary>
        public void CheckAndFinalize()
        {
            if (!this.metadata.IsFinalized)
            {
                if (!this.metadata.HasElementType(this.targetTypeName)) { throw new SimulatorException(string.Format("ScenarioElementType '{0}' doesn't exist!", this.targetTypeName)); }
            }
        }

        #endregion UpgradeEffect buildup methods

        /// <summary>
        /// Constructs an UpgradeEffectBase instance.
        /// </summary>
        /// <param name="targetTypeName">The name of the type on which the effect to be performed.</param>
        /// <param name="metadata">The metadata object that this effect belongs to.</param>
        protected UpgradeEffectBase(string targetTypeName, ScenarioMetadata metadata)
        {
            if (metadata == null) { throw new ArgumentNullException("metadata"); }
            if (targetTypeName == null) { throw new ArgumentNullException("targetTypeName"); }

            this.targetTypeName = targetTypeName;
            this.metadata = metadata;
        }

        /// <summary>
        /// Gets the name of the type on which the effect to be performed.
        /// </summary>
        protected string TargetTypeName { get { return this.targetTypeName; } }

        /// <summary>
        /// The name of the type on which the effect to be performed.
        /// </summary>
        private readonly string targetTypeName;

        /// <summary>
        /// Reference to the metadata object that this effect belongs to.
        /// </summary>
        private readonly ScenarioMetadata metadata;
    }

    /// <summary>
    /// Represents an upgrade effect that executes an action with a parameter.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    class UpgradeEffect<T> : UpgradeEffectBase
    {
        /// <summary>
        /// Constructs an upgrade effect that executes an action with a parameter.
        /// </summary>
        /// <param name="action">The action to be executed.</param>
        /// <param name="actionParam">The parameter of the action.</param>
        /// <param name="targetTypeName">The name of the type on which the effect to be performed.</param>
        /// <param name="metadata">The metadata object that this effect belongs to.</param>
        public UpgradeEffect(Action<IScenarioElementTypeUpgrade, T> action, T actionParam, string targetTypeName, ScenarioMetadata metadata)
            : base(targetTypeName, metadata)
        {
            if (action == null) { throw new ArgumentNullException("action"); }
            this.action = action;
            this.actionParam = actionParam;
        }

        /// <see cref="UpgradeEffectBase.Perform"/>
        public override void Perform(IScenarioMetadataUpgrade metadataUpgrade)
        {
            if (metadataUpgrade == null) { throw new ArgumentNullException("metadataUpgrade"); }

            IScenarioElementTypeUpgrade targetType = metadataUpgrade.GetElementTypeUpgrade(this.TargetTypeName);
            this.action(targetType, this.actionParam);
        }

        /// <summary>
        /// The action to be executed.
        /// </summary>
        private readonly Action<IScenarioElementTypeUpgrade, T> action;

        /// <summary>
        /// The parameter of the action (optional).
        /// </summary>
        private readonly T actionParam;
    }
}
