using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// The interface of entities providing resources.
    /// </summary>
    public interface IResourceProvider
    {
        /// <summary>
        /// Gets the amount of minerals provided by this resource provider or -1 if this resource provider doesn't provide minerals.
        /// </summary>
        int MineralsAmount { get; }

        /// <summary>
        /// Gets the amount of vespene gas provided by this resource provider or -1 if this resource provider doesn't provide vespene gas.
        /// </summary>
        int VespeneGasAmount { get; }
    }
}
