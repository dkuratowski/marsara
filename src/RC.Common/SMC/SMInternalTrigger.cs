using System;
using System.Collections.Generic;

namespace RC.Common.SMC
{
    class SMInternalTrigger : SMTrigger
    {
        public SMInternalTrigger(SMState source, SMState target, StateMachineController.TransitionHandler handler, SMOperator op)
            : base(source, target, handler)
        {
            //if (neededStates == null || neededStates.Count == 0) { throw new ArgumentNullException("neededStates"); }
            if (op == null) { throw new ArgumentNullException("op"); }

            //this.sourceStateIsCurrent = false;
            RCSet<ISMState> neededStatesSet = new RCSet<ISMState>();
            op.CollectAllStates(ref neededStatesSet);
            this.operatorToCheck = op;

            this.neededStates = new Dictionary<ISMState, bool>();

            //RCSet<StateMachine> sms = new RCSet<StateMachine>();
            foreach (ISMState s in neededStatesSet)
            {
                SMState state = this.sourceState.SM.StateObjectMap.GetStateObject(s);
                if (state == null) { throw new SMException("The state '" + s.Name + "' was not found in the object map!"); }

                if (this.sourceState.SM != state.SM && this.sourceState.SM.SmController == state.SM.SmController)// && !sms.Contains(state.SM))
                {
                    //sms.Add(state.SM);
                    this.neededStates.Add(state, false);
                    state.SM.CurrentStateChangedEvt += this.CurrentStateChanged;
                    state.SM.CommissionedEvt += this.StateMachineCommissioned;
                }
                else
                {
                    throw new SMException("Internal trigger operator error!");
                }
            }
        }

        /// <see cref="SMTrigger.CurrentStateChanged"/>
        protected override void CurrentStateChanged(SMState currState, SMState prevState)
        {
            if (currState == null) { throw new ArgumentNullException("currState"); }
            if (prevState == null) { throw new ArgumentNullException("prevState"); }

            //if (currState.SM == this.sourceState.SM)
            //{
            //    this.sourceStateIsCurrent = (currState == this.sourceState);
            //}
            if (this.neededStates.ContainsKey(prevState)) { this.neededStates[prevState] = false; }
            if (this.neededStates.ContainsKey(currState)) { this.neededStates[currState] = true; }

            if (this.sourceState == currState) { return; }

            if (!this.SourceStateIsCurrent) { /*this.triggerFired = false;*/ return; }

            if (!this.operatorToCheck.Evaluate(this.neededStates)) { return; }

            this.sourceState.SM.TriggerHasBeenFired(this);
        }

        /// <see cref="StateMachine.CommissionedHandler"/>
        private void StateMachineCommissioned(SMState initialState)
        {
            if (this.neededStates.ContainsKey(initialState))
            {
                this.neededStates[initialState] = true;
            }
        }

        /// <see cref="SMTrigger.InternalFire"/>
        //public override void InternalFire()
        //{
        //    if (this.triggerFired)
        //    {
        //        this.sourceState.SM.TriggerHasBeenFired(this);
        //        this.triggerFired = false;
        //    }
        //}

        /// <summary>
        /// List of the states needed to be monitored to decide when the trigger has to be fired. The decision is made
        /// by this.operatorToCheck.
        /// </summary>
        private Dictionary<ISMState, bool> neededStates;

        /// <summary>
        /// This operator describes the conditions of firing this trigger.
        /// </summary>
        private SMOperator operatorToCheck;

        /// <summary>
        /// This flag is true if the source state of this trigger is the current state of its corresponding state machine.
        /// </summary>
        private bool SourceStateIsCurrent { get { return this.sourceState.SM.CurrentStateObj == this.sourceState; } }

        /// <summary>
        /// This flag is true if this internal trigger has been fired.
        /// </summary>
        //private bool triggerFired;
    }
}
