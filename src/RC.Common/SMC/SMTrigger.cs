using System;
using RC.Common.Diagnostics;

namespace RC.Common.SMC
{
    /// <summary>
    /// Base class of trigger objects that can cause state transitions in a state machine.
    /// </summary>
    abstract class SMTrigger
    {
        /// <summary>
        /// Constructs a trigger object.
        /// </summary>
        public SMTrigger(SMState source, SMState target, StateMachineController.TransitionHandler handler)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            if (target == null) { throw new ArgumentNullException("target"); }
            if (source.SM != target.SM) { throw new SMException("Transition between states in different state machines not allowed!"); }

            this.sourceState = source;
            this.targetState = target;
            this.transitionHandler = handler;
            this.sourceState.SM.CurrentStateChangedEvt += this.CurrentStateChanged;
        }

        /// <summary>
        /// Calls the method assigned to this trigger if exists.
        /// </summary>
        public void CallTransitionMethod()
        {
            if (null != this.transitionHandler)
            {
                this.transitionHandler(this.targetState, this.sourceState);
            }

            TraceManager.WriteAllTrace(string.Format("{0}: {1} --> {2}", this.sourceState.SM.Name, this.sourceState.Name, this.targetState.Name),
                                       StateMachineController.SMC_INFO);
        }

        /// <summary>
        /// This function is only implemented in SMInternalTrigger.
        /// </summary>
        //public virtual void InternalFire() { }

        /// <summary>
        /// Gets the source state of this trigger.
        /// </summary>
        public SMState SourceState { get { return this.sourceState; } }

        /// <summary>
        /// Gets the target state of this trigger.
        /// </summary>
        public SMState TargetState { get { return this.targetState; } }

        /// <summary>
        /// Called by the state machine when the current state has been changed.
        /// </summary>
        /// <param name="currState">The new current state.</param>
        /// <param name="prevState">The state that was previously the current state.</param>
        protected abstract void CurrentStateChanged(SMState currState, SMState prevState);

        /// <summary>
        /// The source of the transition when this trigger is fired.
        /// </summary>
        protected SMState sourceState;

        /// <summary>
        /// The target of the transition when this trigger is fired.
        /// </summary>
        protected SMState targetState;

        /// <summary>
        /// The handler function that will be called when the trigger is being fired.
        /// </summary>
        private StateMachineController.TransitionHandler transitionHandler;
    }
}
