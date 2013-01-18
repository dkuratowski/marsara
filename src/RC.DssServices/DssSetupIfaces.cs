namespace RC.DssServices
{
    /// <summary>
    /// Enumerates the possible results of the client side execution of a DSS setup step.
    /// </summary>
    public enum DssSetupResult
    {
        CONTINUE_SETUP = 0,         /// Continue the setup stage with the next step.
        START_SIMULATION = 1,       /// Setup stage finished, close all opened channels and start the simulation.
        LEAVE_DSS = 2               /// Leave the DSS.
    }

    /// <summary>
    /// Host-side interface that must be implemented by the client module. The client module is notified about
    /// DSS-events and gets the control to execute the setup steps in the setup stage of a DSS through this interface.
    /// </summary>
    public interface IDssHostSetup
    {
        /// <summary>
        /// Connection with the given guest has been lost in the last setup step.
        /// </summary>
        /// <param name="guestIndex">The index of the guest.</param>
        /// <returns>True if the channel of the guest has to remain opened, and false if it has to be closed.</returns>
        /// <remarks>
        /// We go back to the setup stage if the simulation is currently in progress.
        /// </remarks>
        bool GuestConnectionLost(int guestIndex);

        /// <summary>
        /// The given guest has left the DSS during the setup stage in the last setup step.
        /// </summary>
        /// <param name="guestIndex">The index of the guest.</param>
        /// <returns>True if the channel of the guest has to remain opened, and false if it has to be closed.</returns>
        bool GuestLeftDss(int guestIndex);

        /// <summary>
        /// The client module gets the control to execute the next step of the DSS setup stage when RC.DssServices
        /// calles this function.
        /// </summary>
        /// <param name="channelsToGuests">The channel interfaces to the guests.</param>
        /// <returns>The value that tells the RC.DssServices how to continue the DSS.</returns>
        DssSetupResult ExecuteNextStep(IDssGuestChannel[] channelsToGuests);
    }

    /// <summary>
    /// Guest-side interface that must be implemented by the client module. The client module is notified about
    /// DSS-events and gets the control to execute the setup steps in the setup stage of a DSS through this interface.
    /// </summary>
    public interface IDssGuestSetup
    {
        /// <summary>
        /// Dropped out of the DSS by the host.
        /// </summary>
        void DroppedByHost();

        /// <summary>
        /// Connection lost between the given guest and the host in the last setup step.
        /// </summary>
        /// <param name="guestIndex">The index of the guest.</param>
        void GuestConnectionLost(int guestIndex);

        /// <summary>
        /// The given guest has left the DSS or has been dropped out of the DSS in the last setup step.
        /// </summary>
        /// <param name="guestIndex">The index of the guest.</param>
        void GuestLeftDss(int guestIndex);

        /// <summary>
        /// Connection lost with the host.
        /// </summary>
        /// <remarks>The DSS is stopped after returning from this function.</remarks>
        void HostConnectionLost();

        /// <summary>
        /// The host has left the DSS in the last setup stage.
        /// </summary>
        /// <remarks>The DSS is stopped after returning from this function.</remarks>
        void HostLeftDss();

        /// <summary>
        /// The host has started to execute the simulation so the setup stage has been finished.
        /// </summary>
        void SimulationStarted();

        /// <summary>
        /// The client module gets the control to execute the next step of the DSS setup stage when RC.DssServices
        /// calles this function.
        /// </summary>
        /// <param name="channelToHost">The channel interface to the host.</param>
        /// <returns>True if the client module wants to stay in the DSS and false if it wants to leave the DSS.</returns>
        bool ExecuteNextStep(IDssHostChannel channelToHost);
    }
}
