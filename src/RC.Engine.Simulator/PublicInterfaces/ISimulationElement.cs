using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.PublicInterfaces
{
    /// <summary>
    /// Delegate declarations of simulation element functions.
    /// </summary>
    public delegate void SimElemVoidFunction<TParam0>(TParam0 param0);
    public delegate void SimElemVoidFunction<TParam0, TParam1>(TParam0 param0, TParam1 param1);
    public delegate void SimElemVoidFunction<TParam0, TParam1, TParam2>(TParam0 param0, TParam1 param1, TParam2 param2);
    public delegate void SimElemVoidFunction<TParam0, TParam1, TParam2, TParam3>(TParam0 param0, TParam1 param1, TParam2 param2, TParam3 param3);
    public delegate TReturn SimElemFunction<TReturn, TParam0>(TParam0 param0);
    public delegate TReturn SimElemFunction<TReturn, TParam0, TParam1>(TParam0 param0, TParam1 param1);
    public delegate TReturn SimElemFunction<TReturn, TParam0, TParam1, TParam2>(TParam0 param0, TParam1 param1, TParam2 param2);
    public delegate TReturn SimElemFunction<TReturn, TParam0, TParam1, TParam2, TParam3>(TParam0 param0, TParam1 param1, TParam2 param2, TParam3 param3);

    /// <summary>
    /// The common interface of any kind of simulation elements that should be updated at each simulation frame.
    /// </summary>
    public interface ISimulationElement
    {
        /// <summary>
        /// Simulates the next frame of this simulation element.
        /// </summary>
        void SimulateNextFrame();

        /// <summary>
        /// Gets the unique identifier of this simulation element.
        /// </summary>
        int UID { get; }

        /// <summary>
        /// Invokes the given function of this simulation element.
        /// </summary>
        void Invoke<TParam0>(string functionName, TParam0 param0);
        void Invoke<TParam0, TParam1>(string functionName, TParam0 param0, TParam1 param1);
        void Invoke<TParam0, TParam1, TParam2>(string functionName, TParam0 param0, TParam1 param1, TParam2 param2);
        void Invoke<TParam0, TParam1, TParam2, TParam3>(string functionName, TParam0 param0, TParam1 param1, TParam2 param2, TParam3 param3);
        TReturn Invoke<TReturn, TParam0>(string functionName, TParam0 param0);
        TReturn Invoke<TReturn, TParam0, TParam1>(string functionName, TParam0 param0, TParam1 param1);
        TReturn Invoke<TReturn, TParam0, TParam1, TParam2>(string functionName, TParam0 param0, TParam1 param1, TParam2 param2);
        TReturn Invoke<TReturn, TParam0, TParam1, TParam2, TParam3>(string functionName, TParam0 param0, TParam1 param1, TParam2 param2, TParam3 param3);
    }
}
