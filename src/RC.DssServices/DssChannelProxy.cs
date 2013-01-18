using System;
using RC.Common;

namespace RC.DssServices
{
    /// <summary>
    /// Enumerates the possible tasks to a DSS-channel that must be performed after returning from the
    /// setup interface of the client module in the current setup step.
    /// </summary>
    enum DssChannelTask
    {
        NOT_SPECIFIED = 0,
        CLOSE = 1,
        OPEN = 2,
        DROP_AND_CLOSE = 3,
        DROP_AND_OPEN = 4
    }

    /// <summary>
    /// This class implements the channel interfaces that is provided to the client module.
    /// </summary>
    class DssChannelProxy : IDssGuestChannel, IDssHostChannel
    {
        /// <summary>
        /// Constructs a channel proxy that will be provided to the client module during the setup step execution.
        /// </summary>
        public DssChannelProxy(DssMode mode)
        {
            this.proxyMode = mode;
            this.accessDenied = true;
            this.accessorThread = RCThread.CurrentThread;
            this.incomingPackages = null;
            this.outgoingPackages = null;
            this.currentStateOfChannel = DssChannelState.CHANNEL_CLOSED;
            this.currentStateOfChannels = null;
            this.taskToPerform = DssChannelTask.NOT_SPECIFIED;
            this.guestIndex = -1;   /// currently we don't know the index at guest side
        }

        #region IDssGuestChannel implementation

        /// <see cref="IDssGuestChannel.CloseChannel"/>
        public void CloseChannel()
        {
            if (IsInterfaceAccessGranted(DssMode.HOST_SIDE))
            {
                if (this.currentStateOfChannel == DssChannelState.CHANNEL_OPENED)
                {
                    if (this.taskToPerform == DssChannelTask.NOT_SPECIFIED)
                    {
                        this.taskToPerform = DssChannelTask.CLOSE;
                        this.currentStateOfChannel = DssChannelState.CHANNEL_CLOSED;
                    }
                    else if (this.taskToPerform == DssChannelTask.OPEN)
                    {
                        this.taskToPerform = DssChannelTask.NOT_SPECIFIED;
                        this.currentStateOfChannel = DssChannelState.CHANNEL_CLOSED;
                    }
                    else if (this.taskToPerform == DssChannelTask.DROP_AND_OPEN)
                    {
                        this.taskToPerform = DssChannelTask.DROP_AND_CLOSE;
                        this.currentStateOfChannel = DssChannelState.CHANNEL_CLOSED;
                    }
                }
            }
            else
            {
                throw new DssException("Access denied to interface 'IDssGuestChannel'!");
            }
        }

        /// <see cref="IDssGuestChannel.DropGuest"/>
        public void DropGuest(bool keepOpened)
        {
            if (IsInterfaceAccessGranted(DssMode.HOST_SIDE))
            {
                if (this.currentStateOfChannel == DssChannelState.GUEST_CONNECTED)
                {
                    if (this.taskToPerform == DssChannelTask.NOT_SPECIFIED)
                    {
                        this.taskToPerform = (keepOpened ? DssChannelTask.DROP_AND_OPEN : DssChannelTask.DROP_AND_CLOSE);
                        this.currentStateOfChannel = (keepOpened ? DssChannelState.CHANNEL_OPENED : DssChannelState.CHANNEL_CLOSED);
                    }
                }
            }
            else
            {
                throw new DssException("Access denied to interface 'IDssGuestChannel'!");
            }
        }

        /// <see cref="IDssGuestChannel.OpenChannel"/>
        public void OpenChannel()
        {
            if (IsInterfaceAccessGranted(DssMode.HOST_SIDE))
            {
                if (this.currentStateOfChannel == DssChannelState.CHANNEL_CLOSED)
                {
                    if (this.taskToPerform == DssChannelTask.NOT_SPECIFIED)
                    {
                        this.taskToPerform = DssChannelTask.OPEN;
                        this.currentStateOfChannel = DssChannelState.CHANNEL_OPENED;
                    }
                    else if (this.taskToPerform == DssChannelTask.CLOSE)
                    {
                        this.taskToPerform = DssChannelTask.NOT_SPECIFIED;
                        this.currentStateOfChannel = DssChannelState.CHANNEL_OPENED;
                    }
                    else if (this.taskToPerform == DssChannelTask.DROP_AND_CLOSE)
                    {
                        this.taskToPerform = DssChannelTask.DROP_AND_OPEN;
                        this.currentStateOfChannel = DssChannelState.CHANNEL_OPENED;
                    }
                }
            }
            else
            {
                throw new DssException("Access denied to interface 'IDssGuestChannel'!");
            }
        }

        /// <see cref="IDssGuestChannel.AnswerFromGuest"/>
        public RCPackage[] AnswerFromGuest
        {
            get
            {
                if (IsInterfaceAccessGranted(DssMode.HOST_SIDE))
                {
                    return this.IncomingPackages;
                }
                else
                {
                    throw new DssException("Access denied to interface 'IDssGuestChannel'!");
                }
            }
        }

        /// <see cref="IDssGuestChannel.RequestToGuest"/>
        public RCPackage[] RequestToGuest
        {
            set
            {
                if (IsInterfaceAccessGranted(DssMode.HOST_SIDE))
                {
                    this.OutgoingPackages = value;
                }
                else
                {
                    throw new DssException("Access denied to interface 'IDssGuestChannel'!");
                }
            }
        }

        /// <see cref="IDssGuestChannel.ChannelState"/>
        public DssChannelState ChannelState
        {
            get
            {
                if (IsInterfaceAccessGranted(DssMode.HOST_SIDE))
                {
                    return this.currentStateOfChannel;
                }
                else
                {
                    throw new DssException("Access denied to interface 'IDssGuestChannel'!");
                }
            }
        }

        #endregion


        #region IDssHostChannel implementation

        /// <see cref="IDssHostChannel.AnswerToHost"/>
        public RCPackage[] AnswerToHost
        {
            set
            {
                if (IsInterfaceAccessGranted(DssMode.GUEST_SIDE))
                {
                    this.OutgoingPackages = value;
                }
                else
                {
                    throw new DssException("Access denied to interface 'IDssHostChannel'!");
                }
            }
        }

        /// <see cref="IDssHostChannel.RequestFromHost"/>
        public RCPackage[] RequestFromHost
        {
            get
            {
                if (IsInterfaceAccessGranted(DssMode.GUEST_SIDE))
                {
                    return this.IncomingPackages;
                }
                else
                {
                    throw new DssException("Access denied to interface 'IDssHostChannel'!");
                }
            }
        }

        /// <see cref="IDssHostChannel.ChannelStates"/>
        public DssChannelState[] ChannelStates
        {
            get
            {
                if (IsInterfaceAccessGranted(DssMode.GUEST_SIDE))
                {
                    if (this.currentStateOfChannels != null && this.currentStateOfChannels.Length != 0)
                    {
                        return this.currentStateOfChannels;
                    }
                    else
                    {
                        throw new NullReferenceException("ChannelStates");
                    }
                }
                else
                {
                    throw new DssException("Access denied to interface 'IDssHostChannel'!");
                }
            }
        }

        /// <see cref="IDssHostChannel.GuestIndex"/>
        public int GuestIndex
        {
            get
            {
                if (IsInterfaceAccessGranted(DssMode.GUEST_SIDE))
                {
                    return this.guestIndex;
                }
                else
                {
                    throw new DssException("Access denied to interface 'IDssHostChannel'!");
                }
            }
        }

        #endregion

        /// <summary>
        /// Gets or sets the outgoing setup packages of the proxy.
        /// </summary>
        /// <remarks>
        /// Getting the packages is performed by the RC.DssServices, setting the packages is performed by the client module.
        /// </remarks>
        public RCPackage[] OutgoingPackages
        {
            get
            {
                if (this.accessDenied)
                {
                    RCPackage[] retList = this.outgoingPackages;
                    //this.outgoingPackages = null;
                    return retList;
                }
                else
                {
                    throw new DssException("Proxy is currently unlocked for client module use!");
                }
            }

            set
            {
                RCPackage[] packages = value;
                if (packages != null)
                {
                    this.outgoingPackages = new RCPackage[packages.Length];
                    for (int i = 0; i < packages.Length; i++)
                    {
                        if (packages[i] != null && packages[i].IsCommitted) /// TODO: more conditions to check...
                        {
                            this.outgoingPackages[i] = packages[i];
                        }
                        else
                        {
                            throw new DssException("Outgoing package error!");
                        }
                    }
                }
                else
                {
                    this.outgoingPackages = null;
                }
            }
        }

        /// <summary>
        /// Gets the incoming setup packages of the proxy.
        /// </summary>
        public RCPackage[] IncomingPackages
        {
            get
            {
                if (this.incomingPackages != null)
                {
                    RCPackage[] retList = new RCPackage[this.incomingPackages.Length];
                    for (int i = 0; i < this.incomingPackages.Length; i++)
                    {
                        retList[i] = this.incomingPackages[i];
                    }
                    return retList;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Call this function if you want to unlock this proxy for client use.
        /// </summary>
        /// <param name="incomingPackages">The incoming packages in this step.</param>
        /// <param name="state">The current state of the channel.</param>
        /// <remarks>Calling this function is only allowed on host side proxies.</remarks>
        public void UnlockForClient(RCPackage[] incomingPackages, DssChannelState state)
        {
            if (this.proxyMode == DssMode.HOST_SIDE)
            {
                if (this.accessDenied)
                {
                    this.incomingPackages = incomingPackages;
                    this.outgoingPackages = null;
                    this.currentStateOfChannel = state;
                    this.currentStateOfChannels = null;
                    this.taskToPerform = DssChannelTask.NOT_SPECIFIED;
                    this.accessDenied = false;
                }
                else
                {
                    throw new DssException("DssChannelProxy already unlocked!");
                }
            }
            else
            {
                throw new DssException("Proxy must be host side to call this function!");
            }
        }

        /// <summary>
        /// Call this function if you want to unlock this proxy for client use.
        /// </summary>
        /// <param name="incomingPackages">The incoming packages in this step.</param>
        /// <param name="states">The current states of the channels.</param>
        /// <param name="guestIdx">The index of the channel at the host that this guest is using for communication.</param>
        /// <remarks>Calling this function is only allowed on guest side proxies.</remarks>
        public void UnlockForClient(RCPackage[] incomingPackages, DssChannelState[] states, int guestIdx)
        {
            if (guestIdx < 0) { throw new ArgumentOutOfRangeException("guestIdx"); }

            if (this.proxyMode == DssMode.GUEST_SIDE)
            {
                if (this.accessDenied)
                {
                    this.incomingPackages = incomingPackages;
                    this.outgoingPackages = null;
                    this.currentStateOfChannels = states;
                    this.guestIndex = guestIdx;
                    this.taskToPerform = DssChannelTask.NOT_SPECIFIED;
                    this.accessDenied = false;
                }
                else
                {
                    throw new DssException("DssChannelProxy already unlocked!");
                }
            }
            else
            {
                throw new DssException("Proxy must be guest side to call this function!");
            }
        }

        /// <summary>
        /// Call this function to lock this proxy after the client module setup step execution has been finished.
        /// </summary>
        public void Lock()
        {
            if (!this.accessDenied)
            {
                this.accessDenied = true;
            }
            else
            {
                throw new DssException("DssChannelProxy already locked!");
            }
        }

        /// <summary>
        /// Gets the task to perform in the current setup step on the real channel.
        /// </summary>
        public DssChannelTask TaskToPerform
        {
            get
            {
                if (this.accessDenied)
                {
                    return this.taskToPerform;
                }
                else
                {
                    throw new DssException("Proxy is currently unlocked for client module use!");
                }
            }
        }

        /// <summary>
        /// Checks whether the caller thread can access an interface method.
        /// </summary>
        /// <returns>True if the access is granted, false if the access is denied.</returns>
        private bool IsInterfaceAccessGranted(DssMode neededMode)
        {
            return !this.accessDenied &&
                   (this.proxyMode == neededMode) &&
                   (RCThread.CurrentThread == this.accessorThread);
        }

        /// <summary>
        /// List of the incoming packages that arrived from the other side of the channel corresponding to this proxy.
        /// </summary>
        private RCPackage[] incomingPackages;

        /// <summary>
        /// List of the outgoing packages that have to be sent in the current setup step to the other side of the
        /// channel corresponding to this proxy.
        /// </summary>
        private RCPackage[] outgoingPackages;

        /// <summary>
        /// The current state of the channel corresponding to this proxy.
        /// </summary>
        /// <remarks>This member is only relevant if this is a host side proxy.</remarks>
        private DssChannelState currentStateOfChannel;

        /// <summary>
        /// The current state of the channels at the host.
        /// </summary>
        /// <remarks>This member is only relevant if this is a guest side proxy.</remarks>
        private DssChannelState[] currentStateOfChannels;

        /// <summary>
        /// The index of the guest that this channel proxy belongs to.
        /// </summary>
        /// <remarks>This member is only relevant if this is a guest side proxy.</remarks>
        private int guestIndex;

        /// <summary>
        /// The task that must be performed on this channel after returning from the call to the setup interface
        /// of the client module.
        /// </summary>
        /// <remarks>This member is only relevant if this is a host side proxy.</remarks>
        private DssChannelTask taskToPerform;

        /// <summary>
        /// The mode of this proxy object.
        /// </summary>
        private readonly DssMode proxyMode;

        /// <summary>
        /// The proxy cannot be accessed through the IDssGuestChannel or IDssHostChannel interfaces while this
        /// flag is true.
        /// </summary>
        private bool accessDenied;

        /// <summary>
        /// Reference the thread that can access this proxy through the IDssGuestChannel or IDssHostChannel interfaces.
        /// </summary>
        private readonly RCThread accessorThread;
    }
}
