namespace RC.Common.SMC
{
    /// <summary>
    /// The public interface of a state machine in an SM-controller (StateMachineController).
    /// </summary>
    public interface IStateMachine
    {
        /// <summary>
        /// Adds a new state to this state machine.
        /// </summary>
        /// <param name="name">The name of the new state.</param>
        /// <param name="handler">The state handler function.</param>
        /// <returns>The public interface of the state.</returns>
        ISMState AddState(string name, StateMachineController.StateHandler handler);

        /// <summary>
        /// Sets the given state as the initial.
        /// </summary>
        /// <param name="state">The state you want to be the initial.</param>
        void SetInitialState(ISMState state);

        /// <summary>
        /// Gets the current state of this state machine.
        /// </summary>
        ISMState CurrentState { get; }
    }
}
