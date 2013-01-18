using System;
using System.Collections.Generic;
using RC.Common;

namespace RC.DssServices
{
    /// <summary>
    /// Enumerates the possible states of a setup step.
    /// </summary>
    enum SetupStepState
    {
        NOT_FINISHED = 0,   /// Not every necessary package has arrived
        READY = 1,          /// Every necessary package of the setup step message arrived successfully
        ERROR = 2           /// An unexpected package arrived or any other error occured
    }

    /// <summary>
    /// This class represents a setup step.
    /// </summary>
    class SetupStep
    {
        /// <summary>
        /// Constructs a setup step object.
        /// </summary>
        public SetupStep(DssMode mode)
        {
            this.mode = mode;
            this.packageListTmp = new List<RCPackage>();
            this.leftList = null;
            this.lostList = null;
            this.channelStateList = null;
            this.beginArrived = false;

            if (this.mode == DssMode.HOST_SIDE)
            {
                this.state = SetupStepState.READY;
                this.stepID = RandomService.DefaultGenerator.Next();
                this.packageList = new RCPackage[0];
            }
            else
            {
                this.state = SetupStepState.NOT_FINISHED;
                this.stepID = 0;
                this.packageList = null;
            }
        }

        /// <summary>
        /// Starts a new setup step at host side.
        /// </summary>
        /// <param name="outgoingPackages">The outgoing packages sent by the client module.</param>
        /// <param name="leftList">List of the guests that left or were dropped from the DSS.</param>
        /// <param name="lostList">List of the guests that the host left connection with.</param>
        /// <param name="channelStates">The current state of the channels.</param>
        /// <returns>List of the packages of the outgoing setup step request.</returns>
        /// <remarks>This function must be called at host side.</remarks>
        public RCPackage[] CreateRequest(RCPackage[] outgoingPackages,
                                         int[] leftList,
                                         int[] lostList,
                                         DssChannelState[] channelStates)
        {
            if (this.mode != DssMode.HOST_SIDE) { throw new DssException("This call is only allowed at host side!"); }
            if (this.state != SetupStepState.NOT_FINISHED) { throw new DssException("Invalid setup step state!"); }
            if (channelStates == null || channelStates.Length < 1) { throw new ArgumentNullException("channelStates"); }
            if (outgoingPackages == null) { throw new ArgumentNullException("outgoingPackages"); }

            int numPackages = outgoingPackages.Length + 2; /// BEGIN, END

            RCPackage[] retList = new RCPackage[numPackages];
            for (int i = 0; i < numPackages; i++)
            {
                if (i == 0)         /// BEGIN
                {
                    retList[i] = RCPackage.CreateNetworkControlPackage(DssRoot.DSS_CTRL_SETUP_STEP_RQ_BEGIN);
                    retList[i].WriteInt(0, this.stepID);
                    retList[i].WriteIntArray(1, leftList != null ? leftList : new int[0] { });
                    retList[i].WriteIntArray(2, lostList != null ? lostList : new int[0] { });
                    byte[] stateBytes = new byte[channelStates.Length];
                    for (int j = 0; j < channelStates.Length; j++)
                    {
                        stateBytes[j] = (byte)channelStates[j];
                    }
                    retList[i].WriteByteArray(3, stateBytes);
                }
                else if (i == numPackages - 1)  /// END
                {
                    retList[i] = RCPackage.CreateNetworkControlPackage(DssRoot.DSS_CTRL_SETUP_STEP_MSG_END);
                    retList[i].WriteInt(0, this.stepID);
                }
                else    /// Package from the client module
                {
                    if (outgoingPackages[i - 1] != null && outgoingPackages[i - 1].IsCommitted &&
                        outgoingPackages[i - 1].PackageType == RCPackageType.NETWORK_CONTROL_PACKAGE &&
                        !DssRoot.IsInternalFormat(outgoingPackages[i - 1].PackageFormat))
                    {
                        retList[i] = outgoingPackages[i - 1];
                    }
                    else
                    {
                        throw new DssException("Outgoing setup package format error!");
                    }
                }
            } /// end-for

            return retList;
        }

        /// <summary>
        /// Creates an answer to the current setup step at guest side.
        /// </summary>
        /// <param name="outgoingPackages">The outgoing packages sent by the client module.</param>
        /// <returns>List of the packages of the outgoing setup step answer.</returns>
        /// <remarks>This function must be called at guest side.</remarks>
        public RCPackage[] CreateAnswer(RCPackage[] outgoingPackages)
        {
            if (this.mode != DssMode.GUEST_SIDE) { throw new DssException("This call is only allowed at guest side!"); }
            if (this.state != SetupStepState.READY) { throw new DssException("You can only create answer in READY state!"); }
            if (outgoingPackages == null) { throw new ArgumentNullException("outgoingPackages"); }

            int numPackages = outgoingPackages.Length + 2; /// BEGIN, END

            RCPackage[] retList = new RCPackage[numPackages];
            for (int i = 0; i < numPackages; i++)
            {
                if (i == 0)         /// BEGIN
                {
                    retList[i] = RCPackage.CreateNetworkControlPackage(DssRoot.DSS_CTRL_SETUP_STEP_AW_BEGIN);
                    retList[i].WriteInt(0, this.stepID);
                }
                else if (i == numPackages - 1)  /// END
                {
                    retList[i] = RCPackage.CreateNetworkControlPackage(DssRoot.DSS_CTRL_SETUP_STEP_MSG_END);
                    retList[i].WriteInt(0, this.stepID);
                }
                else    /// Package from the client module
                {
                    if (outgoingPackages[i - 1] != null && outgoingPackages[i - 1].IsCommitted &&
                        outgoingPackages[i - 1].PackageType == RCPackageType.NETWORK_CONTROL_PACKAGE &&
                        !DssRoot.IsInternalFormat(outgoingPackages[i - 1].PackageFormat))
                    {
                        retList[i] = outgoingPackages[i - 1];
                    }
                    else
                    {
                        throw new DssException("Outgoing setup package format error!");
                    }
                }
            } /// end-for

            return retList;
        }

        /// <summary>
        /// Call this function if a control package has arrived from the network and you are waiting for a setup step
        /// answer or request. This class will parse it for you automatically.
        /// </summary>
        /// <param name="package">The incoming control package.</param>
        public void IncomingPackage(RCPackage package)
        {
            if (package == null) { throw new ArgumentNullException("package"); }
            if (!package.IsCommitted) { throw new ArgumentException("Uncommitted package!", "package"); }
            if (package.PackageType != RCPackageType.NETWORK_CONTROL_PACKAGE) { throw new ArgumentException("Unexpected package type!", "package"); }
            if (this.state != SetupStepState.NOT_FINISHED) { throw new DssException("This call is only allowed in NOT_FINISHED state!"); }

            if (this.mode == DssMode.HOST_SIDE)
            {
                /// Host side parsing
                if (this.beginArrived)
                {
                    if (!DssRoot.IsInternalFormat(package.PackageFormat))
                    {
                        this.packageListTmp.Add(package);
                    }
                    else
                    {
                        if (package.PackageFormat.ID == DssRoot.DSS_CTRL_SETUP_STEP_MSG_END &&
                            package.ReadInt(0) == this.stepID)
                        {
                            /// Finished
                            this.packageList = this.packageListTmp.ToArray();
                            this.state = SetupStepState.READY;
                        }
                        else
                        {
                            /// Error
                            this.state = SetupStepState.ERROR;
                        }
                    }
                }
                else /// end-if (this.beginArrived)
                {
                    if (package.PackageFormat.ID == DssRoot.DSS_CTRL_SETUP_STEP_AW_BEGIN &&
                        package.ReadInt(0) == this.stepID)
                    {
                        this.beginArrived = true;
                    }
                    else
                    {
                        /// Error
                        this.state = SetupStepState.ERROR;
                    }
                }
            }
            else /// end-if (this.mode == SetupStepMode.HOST_SIDE)
            {
                /// Guest side parsing
                if (this.beginArrived)
                {
                    if (!DssRoot.IsInternalFormat(package.PackageFormat))
                    {
                        this.packageListTmp.Add(package);
                    }
                    else
                    {
                        if (package.PackageFormat.ID == DssRoot.DSS_CTRL_SETUP_STEP_MSG_END &&
                            package.ReadInt(0) == this.stepID)
                        {
                            /// Finished
                            this.packageList = this.packageListTmp.ToArray();
                            this.state = SetupStepState.READY;
                        }
                        else
                        {
                            /// Error
                            this.state = SetupStepState.ERROR;
                        }
                    }
                }
                else /// end-if (this.beginArrived)
                {
                    if (package.PackageFormat.ID == DssRoot.DSS_CTRL_SETUP_STEP_RQ_BEGIN)
                    {
                        /// Read the step ID
                        this.stepID = package.ReadInt(0);

                        /// Read the left-list
                        int[] leftList = package.ReadIntArray(1);
                        bool err = false;
                        for (int i = 0; i < leftList.Length; ++i)
                        {
                            if (leftList[i] < 0)
                            {
                                err = true;
                                break;
                            }
                        }
                        if (!err)
                        {
                            /// OK, save the list
                            this.leftList = leftList;
                        }
                        else
                        {
                            /// Error
                            this.state = SetupStepState.ERROR;
                            return;
                        }

                        /// Read the lost-list
                        int[] lostList = package.ReadIntArray(2);
                        err = false;
                        for (int i = 0; i < lostList.Length; ++i)
                        {
                            if (lostList[i] < 0)
                            {
                                err = true;
                                break;
                            }
                        }
                        if (!err)
                        {
                            /// OK, save the list
                            this.lostList = lostList;
                        }
                        else
                        {
                            /// Error
                            this.state = SetupStepState.ERROR;
                            return;
                        }

                        /// Read the channel-state-list
                        byte[] chStateBytes = package.ReadByteArray(3);
                        if (chStateBytes.Length != 0)
                        {
                            this.channelStateList = new DssChannelState[chStateBytes.Length];
                            for (int i = 0; i < chStateBytes.Length; ++i)
                            {
                                if (chStateBytes[i] == (byte)DssChannelState.CHANNEL_CLOSED ||
                                    chStateBytes[i] == (byte)DssChannelState.CHANNEL_OPENED ||
                                    chStateBytes[i] == (byte)DssChannelState.GUEST_CONNECTED)
                                {
                                    this.channelStateList[i] = (DssChannelState)chStateBytes[i];
                                }
                                else
                                {
                                    /// Error
                                    this.state = SetupStepState.ERROR;
                                    return;
                                }
                            }
                        } /// end-if (chStateBytes != null && chStateBytes.Length != 0)
                        else
                        {
                            /// Error
                            this.state = SetupStepState.ERROR;
                            return;
                        }

                        /// Everything is OK, the begin message arrived successfully.
                        this.beginArrived = true;
                    }
                    else /// end-if (package.PackageFormat.ID == DssRoot.DSS_CTRL_SETUP_STEP_RQ_BEGIN)
                    {
                        /// Error
                        this.state = SetupStepState.ERROR;
                    }
                }
            }
        }

        /// <summary>
        /// Resets the setup step. At host side this function must be called after the current setup step answer has arrived.
        /// At guest side this function must be called after the current setup step request arrived and the answer has
        /// been created and sent.
        /// </summary>
        public void Reset()
        {
            if (this.state == SetupStepState.READY || this.state == SetupStepState.ERROR)
            {
                this.state = SetupStepState.NOT_FINISHED;
                this.packageListTmp.Clear();
                if (this.mode == DssMode.HOST_SIDE) { this.stepID = RandomService.DefaultGenerator.Next(); }
                this.packageList = null;
                this.leftList = null;
                this.lostList = null;
                this.channelStateList = null;
                this.beginArrived = false;
            }
            else
            {
                throw new DssException("Invalid setup step state!");
            }
        }

        /// <summary>
        /// Gets the packages of the current setup step.
        /// </summary>
        public RCPackage[] StepPackageList
        {
            get
            {
                if (this.state != SetupStepState.READY) { throw new DssException("You can only read the contents in READY state!"); }
                return this.packageList;
            }
        }

        /// <summary>
        /// Gets the list of the guests that left the DSS.
        /// </summary>
        /// <remarks>You can only call this property at guest side.</remarks>
        public int[] GuestsLeftDss
        {
            get
            {
                if (this.mode != DssMode.GUEST_SIDE) { throw new DssException("This call is only allowed at guest side!"); }
                if (this.state != SetupStepState.READY) { throw new DssException("You can only read the contents in READY state!"); }
                return this.leftList;
            }
        }

        /// <summary>
        /// Gets the list of the guests that the host lost connection with.
        /// </summary>
        /// <remarks>You can only call this property at guest side.</remarks>
        public int[] GuestsLost
        {
            get
            {
                if (this.mode != DssMode.GUEST_SIDE) { throw new DssException("This call is only allowed at guest side!"); }
                if (this.state != SetupStepState.READY) { throw new DssException("You can only read the contents in READY state!"); }
                return this.lostList;
            }
        }

        /// <summary>
        /// Gets the list of the current state of the channels at host side.
        /// </summary>
        /// <remarks>You can only call this property at guest side.</remarks>
        public DssChannelState[] ChannelStateList
        {
            get
            {
                if (this.mode != DssMode.GUEST_SIDE) { throw new DssException("This call is only allowed at guest side!"); }
                if (this.state != SetupStepState.READY) { throw new DssException("You can only read the contents in READY state!"); }
                return this.channelStateList;
            }
        }

        /// <summary>
        /// Gets the current state of this setup step.
        /// </summary>
        public SetupStepState State { get { return this.state; } }

        /// <summary>
        /// List of the incoming packages.
        /// </summary>
        private RCPackage[] packageList;

        /// <summary>
        /// Temporary list of the incoming packages.
        /// </summary>
        private List<RCPackage> packageListTmp;

        /// <summary>
        /// List of the guests that left the DSS.
        /// </summary>
        /// <remarks>Only relevant at guest side.</remarks>
        private int[] leftList;

        /// <summary>
        /// List of the guests that the host lost connection with.
        /// </summary>
        /// <remarks>Only relevant at guest side.</remarks>
        private int[] lostList;

        /// <summary>
        /// True if the DSS_CTRL_SETUP_STEP_AW_BEGIN package has been arrived.
        /// </summary>
        private bool beginArrived;

        /// <summary>
        /// List of the current state of the channels at host side.
        /// </summary>
        /// <remarks>Only relevant at guest side.</remarks>
        private DssChannelState[] channelStateList;

        /// <summary>
        /// A randomly generated number that acts as the ID of the current setup step.
        /// </summary>
        /// <remarks>
        /// When the host sends a setup step request, it generates a random stepID and sends this ID with the request
        /// to the guest. The answer sent by the guest must contain the same number as this ID. At this way the host
        /// can make certain that the guest answered to the request.
        /// </remarks>
        private int stepID;

        /// <summary>
        /// The mode of this setup step.
        /// </summary>
        private DssMode mode;

        /// <summary>
        /// The current state of this setup step.
        /// </summary>
        private SetupStepState state;
    }
}
