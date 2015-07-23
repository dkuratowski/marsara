using System;
using System.Collections.Generic;

namespace RC.Common.SMC
{
    /// <summary>
    /// Represents a state in a state machine.
    /// </summary>
    class SMState : ISMState
    {
        /// <summary>
        /// Constructs a state to a state machine.
        /// </summary>
        public SMState(string name, StateMachineController.StateHandler handler, StateMachine sm)
        {
            if (name == null || name.Length == 0) { throw new ArgumentNullException("name"); }
            if (sm == null) { throw new ArgumentNullException("sm"); }

            this.name = name;
            this.handler = handler;
            this.stateMachine = sm;
        }

        /// <see cref="ISMState.AddExternalTrigger"/>
        public ISMTrigger AddExternalTrigger(ISMState targetState, StateMachineController.TransitionHandler handler)
        {
            if (this.stateMachine.Commissioned) { throw new SMException("Unable to add trigger to a commissioned state machine"); }
            if (targetState == null) { throw new ArgumentNullException("targetState"); }

            SMState target = this.stateMachine.GetState(targetState.Name);
            if (target != null)
            {
                SMExternalTrigger extTrigger = new SMExternalTrigger(this, target, handler);
                this.stateMachine.RegisterTrigger(extTrigger);
                return extTrigger;
            }
            else
            {
                throw new SMException("State '" + targetState.Name + "' doesn't exist!");
            }
        }

        /// <see cref="ISMState.AddInternalTrigger"/>
        public void AddInternalTrigger(ISMState targetState, StateMachineController.TransitionHandler handler, RCSet<ISMState> neededStates)
        {
            if (this.stateMachine.Commissioned) { throw new SMException("Unable to add trigger to a commissioned state machine"); }
            if (targetState == null) { throw new ArgumentNullException("targetState"); }
            if (neededStates == null || neededStates.Count == 0) { throw new ArgumentNullException("neededStates"); }

            SMState target = this.stateMachine.GetState(targetState.Name);
            if (target != null)
            {
                ISMState[] neededStatesArray = new ISMState[neededStates.Count];
                int i = 0;
                foreach (ISMState st in neededStates)
                {
                    neededStatesArray[i] = st;
                    i++;
                }
                //RCSet<SMState> neededStateObjects = new RCSet<SMState>();
                //foreach (ISMState s in neededStates)
                //{
                //    SMState sObj = this.stateMachine.StateObjectMap.GetStateObject(s);
                //    if (null != sObj)
                //    {
                //        neededStateObjects.Add(sObj);
                //    }
                //    else { throw new SMException("State '" + s.Name + "' was not found in the object map!"); }
                //}
                SMInternalTrigger intTrigger =
                    new SMInternalTrigger(this, target, handler, new SMOperator(SMOperatorType.AND, neededStatesArray));
                this.stateMachine.RegisterTrigger(intTrigger);
            }
            else { throw new SMException("State '" + targetState.Name + "' doesn't exist!"); }
        }

        /// <see cref="ISMState.AddInternalTrigger"/>
        public void AddInternalTrigger(ISMState targetState, StateMachineController.TransitionHandler handler, SMOperator stateOperator)
        {
            if (this.stateMachine.Commissioned) { throw new SMException("Unable to add trigger to a commissioned state machine"); }
            if (targetState == null) { throw new ArgumentNullException("targetState"); }
            if (stateOperator == null) { throw new ArgumentNullException("stateOperator"); }

            SMState target = this.stateMachine.GetState(targetState.Name);
            if (target != null)
            {
                SMInternalTrigger intTrigger = new SMInternalTrigger(this, target, handler, stateOperator);
                this.stateMachine.RegisterTrigger(intTrigger);
            }
            else { throw new SMException("State '" + targetState.Name + "' doesn't exist!"); }
        }

        /// <see cref="ISMState.Name"/>
        public string Name { get { return this.name; } }

        /// <summary>
        /// Calls the method assigned to this SMState if exists.
        /// </summary>
        public void CallStateMethod()
        {
            if (this.handler != null)
            {
                this.handler(this);
            }
        }

        /// <summary>
        /// Gets the state machine that this state belongs to.
        /// </summary>
        public StateMachine SM { get { return this.stateMachine; } }

        /// <summary>
        /// The name of the state.
        /// </summary>
        private string name;

        /// <summary>
        /// The handler function that will be called when this state became active.
        /// </summary>
        private StateMachineController.StateHandler handler;

        /// <summary>
        /// The state machine that this state belongs to.
        /// </summary>
        private StateMachine stateMachine;
    }
}
