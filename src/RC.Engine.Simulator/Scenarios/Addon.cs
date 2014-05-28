using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.ComponentInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Scenarios
{
    /// <summary>
    /// Represents an addon.
    /// </summary>
    public abstract class Addon : QuadEntity
    {
        /// <summary>
        /// Constructs an Addon instance.
        /// </summary>
        /// <param name="addonTypeName">The name of the type of this addon.</param>
        public Addon(string addonTypeName)
            : base(addonTypeName)
        {
            this.addonType = ComponentManager.GetInterface<IScenarioLoader>().Metadata.GetAddonType(addonTypeName);
        }

        /// <summary>
        /// The type of this addon.
        /// </summary>
        private IAddonType addonType;
    }
}
