using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.PublicInterfaces
{
    /// <summary>
    /// Read interface of simulation data.
    /// </summary>
    /// <typeparam name="T">The type of the simulation data.</typeparam>
    public interface IValueRead<T>
    {
        /// <summary>
        /// Reads the simulation data.
        /// </summary>
        /// <returns>The simulation data.</returns>
        /// <exception cref="SimulatorException">In case of type mismatch.</exception>
        T Read();
    }

    /// <summary>
    /// Write interface of simulation data.
    /// </summary>
    /// <typeparam name="T">The type of the simulation data.</typeparam>
    public interface IValueWrite<T>
    {
        /// <summary>
        /// Writes the simulation data.
        /// </summary>
        /// <param name="newVal">The new value of the simulation data.</param>
        /// <exception cref="SimulatorException">In case of type mismatch.</exception>
        void Write(T newVal);
    }

    /// <summary>
    /// Read/write interface of simulation data.
    /// </summary>
    /// <typeparam name="T">The type of the simulation data.</typeparam>
    public interface IValue<T> : IValueRead<T>, IValueWrite<T> { }
}
