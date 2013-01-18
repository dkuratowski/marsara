using RC.Common;

namespace RC.DssServices
{
    /// <summary>
    /// Enumerates the possible states of a DSS-channel.
    /// </summary>
    public enum DssChannelState
    {
        CHANNEL_OPENED = 0x4F,     /// The channel is opened. A guest can connect to the channel (ASCII character 'O').
        CHANNEL_CLOSED = 0x43,     /// The channel is closed. No guest connection is possible (ASCII character 'C').
        GUEST_CONNECTED = 0x45     /// A guest is currently connected to the channel (ASCII character 'E').
    }

    /// <summary>
    /// The host can access the DSS-channel of a guest using this interface in the setup stage of the DSS.
    /// </summary>
    /// <remarks>
    /// Accessing the interface of the DSS-channels from the client module is only possible after the RC.DssServices
    /// has called the function IDssHostSetup.ExecuteNextStep and before this call returns and the access must
    /// happen from the context of the caller thread. Any other case is followed by an immediate exit from the DSS.
    /// </remarks>
    public interface IDssGuestChannel
    {
        /// <summary>
        /// Closes the DSS-channel. If the DSS-channel is already closed or there is a guest connected to the
        /// channel then calling this function has no effect.
        /// </summary>
        void CloseChannel();

        /// <summary>
        /// Drops the guest on the other side of the channel out of the DSS. If there is no guest on the other side
        /// of the channel then calling this function has no effect.
        /// </summary>
        /// <param name="keepOpened">
        /// Set this parameter true if you want to keep the DSS-channel opened for other connections or false
        /// if you want to close it.
        /// </param>
        void DropGuest(bool keepOpened);

        /// <summary>
        /// Opens the DSS-channel. If the DSS-channel is already opened or there is a guest connected to the
        /// channel then calling this function has no effect.
        /// </summary>
        void OpenChannel();

        /// <summary>
        /// Gets the packages that the guest on the other side has sent as an answer to the previous request of
        /// the host. This property returns null if there is no guest connected to this DSS-channel. This property
        /// returns an empty array if the guest only acknowledged the request but it had nothing to answer. It is
        /// the responsibility of the client module to check whether the answer packages are OK.
        /// </summary>
        /// <remarks>This property is readonly.</remarks>
        RCPackage[] AnswerFromGuest { get; }

        /// <summary>
        /// The host has to put the packages into this array that should be sent as a request in the next setup step
        /// to the guest at the other side of the channel. If there is no guest at the other side then writing this
        /// property has no effect. If the host wants to send an empty request it should set this property to null
        /// or to an empty array.
        /// </summary>
        /// <remarks>This property is writeonly.</remarks>
        RCPackage[] RequestToGuest { set; }

        /// <summary>
        /// Gets the actual state of the DSS-channel.
        /// </summary>
        DssChannelState ChannelState { get; }
    }

    /// <summary>
    /// The guests can access the DSS-channel to the host using this interface in the setup stage of the DSS.
    /// </summary>
    /// <remarks>
    /// Accessing the interface of the DSS-channel from the client module is only possible after the RC.DssServices
    /// has called the function IDssGuestSetup.ExecuteNextStep and before this call returns and the access must
    /// happen from the context of the caller thread. Any other case is followed by an immediate exit from the DSS.
    /// </remarks>
    public interface IDssHostChannel
    {
        /// <summary>
        /// The guest has to put the packages into this array that should be sent as an answer to the host in the
        /// current setup step. If the guest wants only to acknowledge the request it should set this property to null
        /// or to an empty array.
        /// </summary>
        /// <remarks>This property is writeonly.</remarks>
        RCPackage[] AnswerToHost { set; }

        /// <summary>
        /// Gets the packages that the host on the other side has sent as a request in the current setup step.
        /// This property returns an empty array if the host sent an empty request. It is the responsibility of
        /// the client module to check whether the request packages are OK.
        /// </summary>
        /// <remarks>This property is readonly.</remarks>
        RCPackage[] RequestFromHost { get; }

        /// <summary>
        /// Gets the current state of the DSS-channels.
        /// </summary>
        DssChannelState[] ChannelStates { get; }

        /// <summary>
        /// Gets the index of the DSS-channel that this guest is being connected to the host.
        /// </summary>
        int GuestIndex { get; }
    }
}
