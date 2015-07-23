using System;
using System.Collections.Generic;
using RC.Common.Diagnostics;

namespace RC.Common.SMC
{
    /// <summary>
    /// Represents a state machine in a StateMachineController.
    /// </summary>
    class StateMachine : IStateMachine
    {
        /// <summary>
        /// Prototype of functions that will be called when the current state of this state machine changes.
        /// </summary>
        /// <param name="currState">The new current state.</param>
        /// <param name="prevState">The state that was previously the current state.</param>
        public delegate void CurrentStateChangedHandler(SMState currState, SMState prevState);

        /// <summary>
        /// Prototype of functions that will be called when this state machine is being commissioned.
        /// </summary>
        /// <param name="initialState">The initial state of the state machine.</param>
        public delegate void CommissionedHandler(SMState initialState);

        /// <summary>
        /// This event is fired when the current state of this state machine changes.
        /// </summary>
        public event CurrentStateChangedHandler CurrentStateChangedEvt;

        /// <summary>
        /// This event is fired when this state machine is being commissioned.
        /// </summary>
        public event CommissionedHandler CommissionedEvt;

        /// <summary>
        /// Constructs a StateMachine object.
        /// </summary>
        /// <param name="name">Name of this state machine.</param>
        /// <param name="smc">The SM-controller that this state machine belongs to.</param>
        public StateMachine(string name, StateMachineController smc, SMStateObjectMap objectMap)
        {
            if (name == null || name.Length == 0) { throw new ArgumentNullException("name"); }
            if (smc == null) { throw new ArgumentNullException("smc"); }

            this.name = name;
            this.smController = smc;
            this.states = new Dictionary<string, SMState>();
            this.registeredTriggers = new RCSet<SMTrigger>();
            this.currentState = null;
            this.commissioned = false;
            this.stateMethodCalled = false;
            this.triggerHasBeenFired = null;
            this.stateObjectMap = objectMap;
            this.stateChanged = false;
            this.evtArgs = new CurrentStateChangedEvtArgs(null, null);
        }

        /// <see cref="IStateMachine.AddState"/>
        public ISMState AddState(string name, StateMachineController.StateHandler handler)
        {
            if (this.commissioned) { throw new SMException("Unable to add state to a commissioned state machine!"); }
            if (name == null || name.Length == 0) { throw new ArgumentNullException("name"); }

            if (!this.states.ContainsKey(name))
            {
                SMState newState = new SMState(name, handler, this);
                this.states.Add(name, newState);
                this.stateObjectMap.RegisterState(newState);
                return newState;
            }
            else
            {
                throw new SMException("State '" + name + "' already exists in state machine '" + this.name + "'!");
            }
        }

        /// <see cref="IStateMachine.AddState"/>
        public void SetInitialState(ISMState state)
        {
            if (this.commissioned) { throw new SMException("Unable to set the initial state to a commissioned state machine!"); }
            if (state == null) { throw new ArgumentNullException("state"); }

            SMState stateObj = GetState(state.Name);
            if (null == stateObj) { throw new ArgumentException("The state '" + state.Name + "' was not found in state machine '" + this.name + "'!"); }

            this.currentState = stateObj;
        }

        /// <see cref="IStateMachine.CurrentState"/>
        public ISMState CurrentState
        {
            get
            {
                if (!this.commissioned) { throw new SMException("Unable to access the current state of a state machine that was not commissioned!"); }
                return this.currentState;
            }
        }

        /// <summary>
        /// Gets the state with the given name.
        /// </summary>
        /// <param name="name">The name of the state you want to get.</param>
        /// <returns>The state with the given name or null if no state exists with that name.</returns>
        public SMState GetState(string name)
        {
            if (name == null || name.Length == 0) { throw new ArgumentNullException("name"); }

            if (this.states.ContainsKey(name))
            {
                return this.states[name];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Registers a trigger to this state machine.
        /// </summary>
        /// <param name="trigger">The registered trigger.</param>
        public void RegisterTrigger(SMTrigger trigger)
        {
            if (this.commissioned) { throw new SMException("Unable to register a trigger to a commissioned state machine"); }
            if (null == trigger) { throw new ArgumentNullException("trigger"); }
            if (trigger.SourceState.SM != this) { throw new SMException("Unable to register a trigger with a source state that belongs to a different state machine!"); }
            if (trigger.TargetState.SM != this) { throw new SMException("Unable to register a trigger with a target state that belongs to a different state machine!"); }

            this.registeredTriggers.Add(trigger);
        }

        /// <summary>
        /// You have to call this function when the construction of this state machine has been finished.
        /// </summary>
        public void Commission()
        {
            if (this.commissioned)
            {
                TraceManager.WriteAllTrace(string.Format("State machine '{0}' has already been commissioned!", this.name),
                                           StateMachineController.SMC_INFO);
                return;
            }
            if (this.states.Count == 0) { throw new SMException("Unable to commission a state machine with no states!"); }
            if (null == this.currentState) { throw new SMException("Unable to commission a state machine with no initial state!"); }

            if (this.CommissionedEvt != null) { this.CommissionedEvt(this.currentState); }
            this.stateMethodCalled = false;
            this.commissioned = true;
        }

        /// <summary>
        /// Calls the method that has been assigned to the current state if exists.
        /// </summary>
        public void CallStateMethod()
        {
            if (!this.stateMethodCalled)
            {
                this.currentState.CallStateMethod();
                this.stateMethodCalled = true;
            }
        }

        /// <summary>
        /// This function executes the firing of this state machine.
        /// </summary>
        /// <returns>
        /// True if a state change happened, false otherwise.
        /// </returns>
        public bool ExecuteFiring()
        {
            if (null != this.triggerHasBeenFired)
            {
                SMState prevState = this.currentState;
                this.currentState = this.triggerHasBeenFired.TargetState;
                this.triggerHasBeenFired.CallTransitionMethod();
                this.stateChanged = true;
                this.evtArgs.currentState = currentState;
                this.evtArgs.previousState = prevState;
                //this.CurrentStateChangedEvt(currentState, prevState);
                this.triggerHasBeenFired = null;
                this.stateMethodCalled = false;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// This function searches for internal transitions that wants to fire.
        /// </summary>
        public void SearchFiredInternalTriggers()
        {
            if (this.stateChanged)
            {
                if (this.CurrentStateChangedEvt != null)
                {
                    this.CurrentStateChangedEvt(this.evtArgs.currentState, this.evtArgs.previousState);
                }
                this.stateChanged = false;
            }
            //foreach (SMTrigger trigger in this.registeredTriggers)
            //{
            //    trigger.InternalFire();
            //}
        }

        /// <summary>
        /// Gets the SM-controller that this state machine belongs to.
        /// </summary>
        public StateMachineController SmController { get { return this.smController; } }

        /// <summary>
        /// Gets whether this state machine is commissioned or not.
        /// </summary>
        public bool Commissioned { get { return this.commissioned; } }

        /// <summary>
        /// This function is called by a trigger of this state machine when it is fired.
        /// </summary>
        public void TriggerHasBeenFired(SMTrigger trigger)
        {
            if (this.SmController.IsFiringForbidden) { throw new SMException("Firing triggers is currently forbidden by the SMC!"); }
            if (null == trigger) { throw new ArgumentNullException("trigger"); }
            if (!this.registeredTriggers.Contains(trigger)) { throw new SMException("Unable to fire unregistered trigger!"); }

            if (this.triggerHasBeenFired != null && this.triggerHasBeenFired != trigger)
            {
                throw new SMException("Another trigger has already been fired in state machine '" + this.name + "'!");
            }

            this.triggerHasBeenFired = trigger;
        }

        /// <summary>
        /// Gets the name of this state machine.
        /// </summary>
        public string Name { get { return this.name; } }

        /// <summary>
        /// Gets the object map.
        /// </summary>
        public SMStateObjectMap StateObjectMap { get { return this.stateObjectMap; } }

        /// <summary>
        /// Gets the current state of this state machine.
        /// </summary>
        public SMState CurrentStateObj { get { return this.currentState; } }

        /// <summary>
        /// Arguments of the CurrentStateChangeEvt event.
        /// </summary>
        private struct CurrentStateChangedEvtArgs
        {
            public CurrentStateChangedEvtArgs(SMState currState, SMState prevState)
            {
                currentState = currState;
                previousState = prevState;
            }
            public SMState currentState;
            public SMState previousState;
        }

        /// <summary>
        /// Name of this state machine.
        /// </summary>
        private string name;

        /// <summary>
        /// The SM-controller that this state machine belongs to.
        /// </summary>
        private StateMachineController smController;

        /// <summary>
        /// List of the states in this state machine.
        /// </summary>
        private Dictionary<string, SMState> states;

        /// <summary>
        /// List of the triggers registered to this state machine.
        /// </summary>
        private RCSet<SMTrigger> registeredTriggers;

        /// <summary>
        /// Reference to the initial state.
        /// </summary>
        private SMState currentState;

        /// <summary>
        /// This flag becomes true when the SM-controller has been successfully commissioned.
        /// </summary>
        private bool commissioned;

        /// <summary>
        /// This flag indicates whether the state method have been called since the last transition.
        /// </summary>
        private bool stateMethodCalled;

        /// <summary>
        /// Reference to the trigger that has been fired.
        /// </summary>
        private SMTrigger triggerHasBeenFired;

        /// <summary>
        /// The object map that is used to find SMState objects by their interface.
        /// </summary>
        private SMStateObjectMap stateObjectMap;

        /// <summary>
        /// Arguments for the next call of CurrentStateChangeEvt event.
        /// </summary>
        private CurrentStateChangedEvtArgs evtArgs;

        /// <summary>
        /// If this flag is true, the CurrentStateChangeEvt event has to be called.
        /// </summary>
        private bool stateChanged;
    }
}
