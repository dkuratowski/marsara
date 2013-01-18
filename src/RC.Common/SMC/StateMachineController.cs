using System;
using System.Collections.Generic;
using RC.Common.Diagnostics;

namespace RC.Common.SMC
{
    /// <summary>
    /// A StateMachineController (SM-controller) is an object that can control a complex process using the state
    /// machines defined inside it.
    /// </summary>
    public class StateMachineController
    {
        /// <summary>
        /// Prototype of functions that will be automatically called when a transition happens in a state machine.
        /// </summary>
        /// <param name="targetState">The target state of the transition.</param>
        /// <param name="sourceState">The source state of the transition.</param>
        public delegate void TransitionHandler(ISMState targetState, ISMState sourceState);

        /// <summary>
        /// A State handler function can be added to each SM-state, that will be called when the corresponding state
        /// has become active.
        /// </summary>
        /// <param name="state">The state that called the function.</param>
        public delegate void StateHandler(ISMState state);

        /// <summary>
        /// Constructs an SM-controller with the given name.
        /// </summary>
        public StateMachineController()
        {
            this.stateMachines = new Dictionary<string, StateMachine>();
            this.commissioned = false;
            this.firingForbidden = false;
            this.executingFirings = false;
            this.objectMap = new SMStateObjectMap();
        }

        /// <summary>
        /// Adds a state machine to this controller with the given name.
        /// </summary>
        /// <param name="name">The name of the new state machine.</param>
        /// <returns>The public interface of the state machine.</returns>
        public IStateMachine AddStateMachine(string name)
        {
            if (this.commissioned) { throw new SMException("Unable to add state machine to a commissioned SM-controller!"); }
            if (name == null || name.Length == 0) { throw new ArgumentNullException("name"); }

            if (!this.stateMachines.ContainsKey(name))
            {
                StateMachine newSM = new StateMachine(name, this, this.objectMap);
                this.stateMachines.Add(name, newSM);
                return newSM;
            }
            else
            {
                throw new SMException("StateMachine with the name '" + name + "' already exists in this SM-controller!");
            }
        }

        /// <summary>
        /// You have to call this function if the construction of the SM-controller is completed.
        /// </summary>
        public void CommissionController()
        {
            if (this.commissioned)
            {
                TraceManager.WriteAllTrace("SM-controller has already been commissioned!", StateMachineController.SMC_INFO);
                return;
            }
            if (this.stateMachines.Count == 0) { throw new SMException("Unable to commission an SM-controller with no state machines!"); }

            this.objectMap.Commission();

            foreach (KeyValuePair<string, StateMachine> smItem in this.stateMachines)
            {
                smItem.Value.Commission();
            }
            this.commissioned = true;

            foreach (KeyValuePair<string, StateMachine> smItem in this.stateMachines)
            {
                smItem.Value.CallStateMethod();
            }
        }

        /// <summary>
        /// Call this function after you have fired all external triggers if you want to execute those firings.
        /// </summary>
        public void ExecuteFirings()
        {
            if (!this.commissioned) { throw new SMException("Unable to execute firings in an SM-controller that was not commissioned!"); }
            if (this.executingFirings) { throw new SMException("Recursive call to StateMachineController.ExecuteFirings()!"); }

            this.executingFirings = true;

            while (true)
            {
                bool continueNeeded = false;

                this.firingForbidden = true;    /// Firing not possible
                foreach (KeyValuePair<string, StateMachine> smItem in this.stateMachines)
                {
                    bool fireHappened = smItem.Value.ExecuteFiring();
                    if (!continueNeeded) { continueNeeded = fireHappened; }
                }
                this.firingForbidden = false;   /// Firing is possible again

                if (continueNeeded)
                {
                    /// Search the activated internal triggers
                    foreach (KeyValuePair<string, StateMachine> smItem in this.stateMachines)
                    {
                        smItem.Value.SearchFiredInternalTriggers();
                    }

                    /// At least one fire happened, so we have to call the state methods
                    foreach (KeyValuePair<string, StateMachine> smItem in this.stateMachines)
                    {
                        smItem.Value.CallStateMethod();
                    }
                }
                else
                {
                    break;
                }
            }

            this.executingFirings = false;
        }

        /// <summary>
        /// Gets whether firing triggers is forbidden or not.
        /// </summary>
        public bool IsFiringForbidden { get { return this.firingForbidden; } }

        /// <summary>
        /// ID of the RC.Common.SMC.Info trace filter.
        /// </summary>
        public static readonly int SMC_INFO = TraceManager.GetTraceFilterID("RC.Common.SMC.Info");

        /// <summary>
        /// List of the state machines.
        /// </summary>
        private Dictionary<string, StateMachine> stateMachines;

        /// <summary>
        /// This flag becomes true when the SM-controller has been successfully commissioned.
        /// </summary>
        private bool commissioned;

        /// <summary>
        /// This flag is true if firing triggers is forbidden.
        /// </summary>
        private bool firingForbidden;

        /// <summary>
        /// This flag is used to avoid a recursive call to StateMachineController.ExecuteFirings().
        /// </summary>
        private bool executingFirings;

        /// <summary>
        /// This map is used to find an SMState object by its interface.
        /// </summary>
        private SMStateObjectMap objectMap;
    }
}
