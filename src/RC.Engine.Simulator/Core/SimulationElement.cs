using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Represents an element of a simulated scenario.
    /// </summary>
    class SimulationElement : ISimulationElement
    {
        /// <summary>
        /// Constructs a SimulationElement instance.
        /// </summary>
        /// <param name="uid">The unique identifier of this simulation element.</param>
        public SimulationElement(int uid)
        {
            this.dataRefs = new List<ISimDataAccess>();
            this.behaviors = new List<ISimElemBehavior>();
            this.functions = new Dictionary<string, Delegate>();
            this.uid = uid;
        }

        #region ISimulationElement methods

        /// <see cref="ISimulationElement.SimulateNextFrame"/>
        public void SimulateNextFrame()
        {
            foreach (ISimElemBehavior behavior in this.behaviors)
            {
                if (behavior.CheckIfActive())
                {
                    behavior.StepNextFrame();
                    break;
                }
            }
        }

        /// <see cref="ISimulationElement.UID"/>
        public int UID { get { return this.uid; } }

        /// <see cref="ISimulationElement.Invoke"/>
        public void Invoke<TParam0>(string functionName, TParam0 param0)
        {
            if (!this.functions.ContainsKey(functionName)) { throw new SimulatorException(string.Format("Simulation element function '{0}' doesn't exist!", functionName)); }
            ((SimElemVoidFunction<TParam0>)this.functions[functionName]).Invoke(param0);
        }

        /// <see cref="ISimulationElement.Invoke"/>
        public void Invoke<TParam0, TParam1>(string functionName, TParam0 param0, TParam1 param1)
        {
            if (!this.functions.ContainsKey(functionName)) { throw new SimulatorException(string.Format("Simulation element function '{0}' doesn't exist!", functionName)); }
            ((SimElemVoidFunction<TParam0, TParam1>)this.functions[functionName]).Invoke(param0, param1);
        }

        /// <see cref="ISimulationElement.Invoke"/>
        public void Invoke<TParam0, TParam1, TParam2>(string functionName, TParam0 param0, TParam1 param1, TParam2 param2)
        {
            if (!this.functions.ContainsKey(functionName)) { throw new SimulatorException(string.Format("Simulation element function '{0}' doesn't exist!", functionName)); }
            ((SimElemVoidFunction<TParam0, TParam1, TParam2>)this.functions[functionName]).Invoke(param0, param1, param2);
        }

        /// <see cref="ISimulationElement.Invoke"/>
        public void Invoke<TParam0, TParam1, TParam2, TParam3>(string functionName, TParam0 param0, TParam1 param1, TParam2 param2, TParam3 param3)
        {
            if (!this.functions.ContainsKey(functionName)) { throw new SimulatorException(string.Format("Simulation element function '{0}' doesn't exist!", functionName)); }
            ((SimElemVoidFunction<TParam0, TParam1, TParam2, TParam3>)this.functions[functionName]).Invoke(param0, param1, param2, param3);
        }

        /// <see cref="ISimulationElement.Invoke"/>
        public TReturn Invoke<TReturn, TParam0>(string functionName, TParam0 param0)
        {
            if (!this.functions.ContainsKey(functionName)) { throw new SimulatorException(string.Format("Simulation element function '{0}' doesn't exist!", functionName)); }
            return ((SimElemFunction<TReturn, TParam0>)this.functions[functionName]).Invoke(param0);
        }

        /// <see cref="ISimulationElement.Invoke"/>
        public TReturn Invoke<TReturn, TParam0, TParam1>(string functionName, TParam0 param0, TParam1 param1)
        {
            if (!this.functions.ContainsKey(functionName)) { throw new SimulatorException(string.Format("Simulation element function '{0}' doesn't exist!", functionName)); }
            return ((SimElemFunction<TReturn, TParam0, TParam1>)this.functions[functionName]).Invoke(param0, param1);
        }

        /// <see cref="ISimulationElement.Invoke"/>
        public TReturn Invoke<TReturn, TParam0, TParam1, TParam2>(string functionName, TParam0 param0, TParam1 param1, TParam2 param2)
        {
            if (!this.functions.ContainsKey(functionName)) { throw new SimulatorException(string.Format("Simulation element function '{0}' doesn't exist!", functionName)); }
            return ((SimElemFunction<TReturn, TParam0, TParam1, TParam2>)this.functions[functionName]).Invoke(param0, param1, param2);
        }

        /// <see cref="ISimulationElement.Invoke"/>
        public TReturn Invoke<TReturn, TParam0, TParam1, TParam2, TParam3>(string functionName, TParam0 param0, TParam1 param1, TParam2 param2, TParam3 param3)
        {
            if (!this.functions.ContainsKey(functionName)) { throw new SimulatorException(string.Format("Simulation element function '{0}' doesn't exist!", functionName)); }
            return ((SimElemFunction<TReturn, TParam0, TParam1, TParam2, TParam3>)this.functions[functionName]).Invoke(param0, param1, param2, param3);
        }

        #endregion ISimulationElement methods

        /// <summary>
        /// Data references to the simulation heap.
        /// </summary>
        private List<ISimDataAccess> dataRefs;

        /// <summary>
        /// Ordered list of the behaviors of this simulation element.
        /// </summary>
        private List<ISimElemBehavior> behaviors;

        /// <summary>
        /// List of the available functions of this simulation element.
        /// </summary>
        private Dictionary<string, Delegate> functions;

        /// <summary>
        /// The unique identifier of this simulation element.
        /// </summary>
        private int uid;
    }
}
