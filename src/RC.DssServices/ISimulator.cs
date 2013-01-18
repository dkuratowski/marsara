using RC.Common;

namespace RC.DssServices
{
    /// <summary>
    /// The RC.DssServices controls the local simulation of an operator through this interface. It has to be
    /// implemented by the client module.
    /// </summary>
    public interface ISimulator
    {
        /// <summary>
        /// The client module get control to execute the next frame of the local simulation.
        /// </summary>
        /// <param name="outgoingCmds">The commands given by the local operator during the executed frame.</param>
        /// <returns>True if the client module wants to stay in the DSS and false if it wants to leave the DSS.</returns>
        bool ExecuteNextFrame(out RCPackage[] outgoingCmds);

        /// <summary>
        /// A command has been arrived from the given guest and has to be executed in the next frame.
        /// </summary>
        /// <param name="guestIndex">The guest that sent the command.</param>
        /// <param name="command">The package that contains the command.</param>
        void GuestCommand(int guestIndex, RCPackage command);

        /// <summary>
        /// The given guest has left the DSS during the simulation stage.
        /// </summary>
        /// <param name="guestIndex">The index of the guest.</param>
        /// <remarks>The channel of that guest will be closed automatically at host-side.</remarks>
        void GuestLeftDssDuringSim(int guestIndex);

        /// <summary>
        /// A command has been arrived from the host and has to be executed in the next frame.
        /// </summary>
        /// <param name="command">The package that contains the command.</param>
        void HostCommand(RCPackage command);

        /// <summary>
        /// The host has left the DSS during the simulation stage.
        /// </summary>
        /// <remarks>The DSS is stopped after returning from this function.</remarks>
        void HostLeftDssDuringSim();

        /// <summary>
        /// An unexpected error occured during simulation stage.
        /// </summary>
        /// <remarks>The DSS is stopped after returning from this function.</remarks>
        void SimulationError(string reason, byte[] customData);

        /// <summary>
        /// Returns the hash value of the current simulation state.
        /// </summary>
        byte[] StateHash { get; }
    }
}
