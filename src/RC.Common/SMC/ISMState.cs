using System.Collections.Generic;

namespace RC.Common.SMC
{
    /// <summary>
    /// The public interface of a state in a state machine.
    /// </summary>
    public interface ISMState
    {
        /// <summary>
        /// Adds an external trigger to this state. An external trigger can be fired from anywhere. When an external
        /// trigger is fired, a trasition happens from this state of a state machine to another state.
        /// </summary>
        /// <param name="targetState">The target state of the transition.</param>
        /// <param name="handler">This function will be called when the transition happens.</param>
        /// <returns>The public interface of the trigger.</returns>
        ISMTrigger AddExternalTrigger(ISMState targetState, StateMachineController.TransitionHandler handler);

        /// <summary>
        /// Adds an internal trigger to this state. This internal trigger will be fired automatically when the given states
        /// become active. When an internal trigger is fired, a trasition happens from this state of a state machine to
        /// another state.
        /// </summary>
        /// <param name="targetState">The target state of the transition.</param>
        /// <param name="handler">This function will be called when the transition happens.</param>
        /// <param name="neededStates">
        /// The list of the states needed to be active for firing the created internal trigger.
        /// </param>
        void AddInternalTrigger(ISMState targetState, StateMachineController.TransitionHandler handler, HashSet<ISMState> neededStates);

        /// <summary>
        /// Adds an internal trigger to this state. This internal trigger will be fired automatically when the given operator
        /// is satisfied. When an internal trigger is fired, a trasition happens from this state of a state machine to
        /// another state.
        /// </summary>
        /// <param name="targetState">The target state of the transition.</param>
        /// <param name="handler">This function will be called when the transition happens.</param>
        /// <param name="stateOperator">
        /// The operator that has to be satisfied for firing the created internal trigger.
        /// </param>
        void AddInternalTrigger(ISMState targetState, StateMachineController.TransitionHandler handler, SMOperator stateOperator);

        /// <summary>
        /// Gets the name of the state.
        /// </summary>
        string Name { get; }
    }
}
