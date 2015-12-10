using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.Metadata.Core;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// Represents an upgrade.
    /// </summary>
    public sealed class Upgrade : ScenarioElement
    {
        /// <summary>
        /// Gets whether this upgrade is currently being researched or not.
        /// </summary>
        public bool IsUnderResearch { get { return this.researchProgress.Read() != this.ElementType.BuildTime.Read(); } }

        /// <see cref="ScenarioElement.UpdateStateImpl"/>
        protected sealed override void UpdateStateImpl()
        {
            if (this.Owner == null) { throw new InvalidOperationException("Every upgrade must have an owner!"); }

            if (this.IsUnderResearch)
            {
                this.researchProgress.Write(this.researchProgress.Read() + 1);
                if (!this.IsUnderResearch)
                {
                    /// Research complete -> perform the effects of this upgrade.
                    foreach (IUpgradeEffect upgradeEffect in this.upgradeType.Effects)
                    {
                        upgradeEffect.Perform(this.Owner.MetadataUpgrade);
                    }
                }
            }
        }

        /// <summary>
        /// Constructs an Upgrade instance.
        /// </summary>
        /// <param name="upgradeTypeName">The name of the type of this upgrade.</param>
        internal Upgrade(string upgradeTypeName)
            : base(upgradeTypeName)
        {
            this.upgradeType = new IUpgradeType(this.ElementType.ElementTypeImpl as IUpgradeTypeInternal);
            this.researchProgress = this.ConstructField<int>("researchProgress");
            this.researchProgress.Write(0);
        }

        /// <summary>
        /// The type of this upgrade.
        /// </summary>
        private readonly IUpgradeType upgradeType;

        /// <summary>
        /// The current research progress of this upgrade.
        /// </summary>
        private readonly HeapedValue<int> researchProgress;
    }
}
