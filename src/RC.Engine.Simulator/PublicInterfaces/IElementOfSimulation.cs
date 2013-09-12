using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.PublicInterfaces
{
    /// <summary>
    /// The common interface of any kind of simulation elements that should be updated at each simulation frame.
    /// </summary>
    public interface IElementOfSimulation
    {
        /// <summary>
        /// Updates the simulation element.
        /// </summary>
        void Update();
    }
}
