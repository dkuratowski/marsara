using System.Collections.Generic;
using RC.Common.Diagnostics;

namespace RC.Common.SMC
{
    /// <summary>
    /// This is a helper class that is used to find state objects by their interfaces.
    /// </summary>
    class SMStateObjectMap
    {
        /// <summary>
        /// Constructs a new SMStateObjectMap object.
        /// </summary>
        public SMStateObjectMap()
        {
            this.objectMap = new Dictionary<ISMState, SMState>();
            this.commissioned = false;
        }

        /// <summary>
        /// Registers the given state in the object map.
        /// </summary>
        /// <param name="state">The state you want to register.</param>
        public void RegisterState(SMState state)
        {
            if (this.commissioned) { throw new SMException("Unable to register states to a commissioned object map!"); }

            if (!this.objectMap.ContainsKey(state))
            {
                this.objectMap.Add(state, state);
            }
            else
            {
                TraceManager.WriteAllTrace(string.Format("Warning! State '{0}' has already been registered!", state.Name),
                                           StateMachineController.SMC_INFO);
            }
        }

        /// <summary>
        /// Call this function when all needed states has been registered.
        /// </summary>
        public void Commission()
        {
            if (this.commissioned)
            {
                TraceManager.WriteAllTrace("Object map has already been commissioned!", StateMachineController.SMC_INFO);
                return;
            }

            this.commissioned = true;
        }

        /// <summary>
        /// Gets the SMState object that implements the interface given in the parameter.
        /// </summary>
        /// <param name="iface">The interface of the object you want to get.</param>
        /// <returns>The implementor object or null if no such object exists in this map.</returns>
        public SMState GetStateObject(ISMState iface)
        {
            if (this.objectMap.ContainsKey(iface))
            {
                return this.objectMap[iface];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// This is the wrapped object map.
        /// </summary>
        private Dictionary<ISMState, SMState> objectMap;

        /// <summary>
        /// This flag is true if the object map is commissioned and no more SMState object can be registered.
        /// </summary>
        private bool commissioned;
    }
}
