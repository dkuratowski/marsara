using System.Collections.Generic;
using RC.Common;
using RC.NetworkingSystem;
using System.Drawing;
using RC.RenderSystem;
using RC.Common.Diagnostics;

namespace RC.DssServices.Test
{
    class DssGuestThread : DssThread, IDssGuestSetup
    {
        public DssGuestThread(ControlStatusManager ctrlStatusMgr, IUiInvoke ui, TestSimulator simulator, LobbyInfo tgtLobby, INetwork network)
            : base(ctrlStatusMgr, ui, simulator, network)
        {
            this.firstSetupStep = true;
            this.newColor = PlayerColor.White;
            this.newColorSelected = false;
            this.applyNewColorSelection = false;
            //this.colorSelectRq = new ColorSelectRQ();
            this.targetLobby = tgtLobby;
            this.aw = new List<RCPackage>();
            this.previousChannelStates = null;
        }

        #region IDssGuestSetup members

        public void DroppedByHost()
        {
        }

        public void GuestConnectionLost(int guestIndex)
        {
            TraceManager.WriteAllTrace(string.Format("SETUP_CALL: Guest-{0} connection lost.", guestIndex), TestClientTraceFilters.TEST_INFO);
            this.simulator.GetPlayer(guestIndex + 1).Deactivate();
            Rectangle refreshedArea;
            Display.Instance.RenderOneFrame(out refreshedArea);
            this.uiCallMarshal.RefreshDisplay();
        }

        public void GuestLeftDss(int guestIndex)
        {
            TraceManager.WriteAllTrace(string.Format("SETUP_CALL: Guest-{0} left DSS.", guestIndex), TestClientTraceFilters.TEST_INFO);
            this.simulator.GetPlayer(guestIndex + 1).Deactivate();
            Rectangle refreshedArea;
            Display.Instance.RenderOneFrame(out refreshedArea);
            this.uiCallMarshal.RefreshDisplay();
        }

        public void HostConnectionLost()
        {
            TraceManager.WriteAllTrace("SETUP_CALL: Host connection lost", TestClientTraceFilters.TEST_INFO);
        }

        public void HostLeftDss()
        {
            TraceManager.WriteAllTrace("SETUP_CALL: Host left DSS", TestClientTraceFilters.TEST_INFO);
        }

        public void SimulationStarted()
        {
            TraceManager.WriteAllTrace("SETUP_CALL: Simulation started", TestClientTraceFilters.TEST_INFO);
        }

        public bool ExecuteNextStep(IDssHostChannel channelToHost)
        {
            TraceManager.WriteAllTrace("SETUP_CALL: Execute setup step", TestClientTraceFilters.TEST_INFO);
            this.aw.Clear();

            if (this.firstSetupStep)
            {
                bool isOK = FirstSetupStep(channelToHost);
                if (!isOK)
                {
                    /// Exit the DSS in case of error.
                    return false;
                }
                this.firstSetupStep = false;
            }

            bool result = ProcessUiActions(channelToHost);
            if (result)
            {
                bool isOK = ProcessHostChannel(channelToHost);
                if (!isOK)
                {
                    /// Exit the DSS in case of error.
                    return false;
                }
                channelToHost.AnswerToHost = aw.ToArray();
            }

            /// Refresh the display
            Rectangle refreshedArea;
            Display.Instance.RenderOneFrame(out refreshedArea);
            this.uiCallMarshal.RefreshDisplay();
            return result;
        }

        #endregion

        protected override void DssThreadProc()
        {
            DssServiceAccess.ConnectDSS(this.targetLobby, this.network, this, this);
            
            /// End of DSS --> deactivate the players
            for (int i = 0; i < this.simulator.MaxNumOfPlayers; i++)
            {
                Player currPlayer = this.simulator.GetPlayer(i);
                if (currPlayer.IsActive)
                {
                    currPlayer.Deactivate();
                }
            }

            /// Refresh the display
            Rectangle refreshedArea;
            Display.Instance.RenderOneFrame(out refreshedArea);
            this.uiCallMarshal.RefreshDisplay();

            this.uiCallMarshal.EndOfDss();
        }

        /// <summary>
        /// Initializes the controls on the UI.
        /// </summary>
        /// <returns>True in case of error, false otherwise.</returns>
        private bool FirstSetupStep(IDssHostChannel channelToHost)
        {
            RCPackage[] requests = channelToHost.RequestFromHost;
            if (requests.Length != 1 || requests[0].PackageFormat.ID != TestClientMessages.RESET) { return false; }
            byte[] colorBytes = requests[0].ReadByteArray(0);
            int[] xCoords = requests[0].ReadIntArray(1);
            int[] yCoords = requests[0].ReadIntArray(2);
            if (colorBytes.Length != this.simulator.MaxNumOfPlayers ||
                xCoords.Length != this.simulator.MaxNumOfPlayers ||
                yCoords.Length != this.simulator.MaxNumOfPlayers) { return false; }

            for (int i = 0; i < colorBytes.Length; i++)
            {
                this.simulator.GetPlayer(i).Color = (PlayerColor)colorBytes[i];
                this.simulator.GetPlayer(i).Position = new Rectangle(xCoords[i], yCoords[i], Player.DIAMETER, Player.DIAMETER);
            }

            this.simulator.GetPlayer(0).Activate();

            this.uiCallMarshal.SetMainControlStatus(FormStatus.GuestSide);

            this.uiCallMarshal.SelectNewHostColor(this.simulator.GetPlayer(0).Color);
            this.uiCallMarshal.SetHostControlStatus(HostControlStatus.AccessDenied);

            this.previousChannelStates = new DssChannelState[channelToHost.ChannelStates.Length];
            for (int i = 0; i < this.previousChannelStates.Length; i++)
            {
                this.previousChannelStates[i] = channelToHost.ChannelStates[i];
                if (channelToHost.ChannelStates[i] == DssChannelState.CHANNEL_OPENED)
                {
                    this.uiCallMarshal.SetGuestControlStatus(i, GuestControlStatus.GuestSideOpened);
                }
                else if (channelToHost.ChannelStates[i] == DssChannelState.CHANNEL_CLOSED)
                {
                    this.uiCallMarshal.SetGuestControlStatus(i, GuestControlStatus.GuestSideClosed);
                }
                else if (channelToHost.ChannelStates[i] == DssChannelState.GUEST_CONNECTED)
                {
                    if (channelToHost.GuestIndex == i)
                    {
                        this.uiCallMarshal.SetGuestControlStatus(i, GuestControlStatus.GuestSideAccessGranted);
                    }
                    else
                    {
                        this.uiCallMarshal.SetGuestControlStatus(i, GuestControlStatus.GuestSideAccessDenied);
                    }
                    this.uiCallMarshal.SelectNewGuestColor(i, this.simulator.GetPlayer(i + 1).Color);
                    this.simulator.GetPlayer(i + 1).Activate();
                }
            }
            return true;
        }

        /// <summary>
        /// Updates the channel state indicator controls on the UI.
        /// </summary>
        /// <returns>True in case of error, false otherwise.</returns>
        private bool ProcessHostChannel(IDssHostChannel channelToHost)
        {
            /// First refresh the guest and host controls.
            for (int i = 0; i < this.previousChannelStates.Length; i++)
            {
                if (this.previousChannelStates[i] != channelToHost.ChannelStates[i])
                {
                    /// The state of the channel has changed.
                    this.previousChannelStates[i] = channelToHost.ChannelStates[i];
                    if (channelToHost.ChannelStates[i] == DssChannelState.CHANNEL_OPENED)
                    {
                        this.uiCallMarshal.SetGuestControlStatus(i, GuestControlStatus.GuestSideOpened);
                    }
                    else if (channelToHost.ChannelStates[i] == DssChannelState.CHANNEL_CLOSED)
                    {
                        this.uiCallMarshal.SetGuestControlStatus(i, GuestControlStatus.GuestSideClosed);
                    }
                    else if (channelToHost.ChannelStates[i] == DssChannelState.GUEST_CONNECTED)
                    {
                        if (channelToHost.GuestIndex == i)
                        {
                            this.uiCallMarshal.SetGuestControlStatus(i, GuestControlStatus.GuestSideAccessGranted);
                        }
                        else
                        {
                            this.uiCallMarshal.SetGuestControlStatus(i, GuestControlStatus.GuestSideAccessDenied);
                        }
                        this.simulator.GetPlayer(i + 1).Activate();
                    }
                }
            }

            /// Then process the request of the host
            RCPackage[] requestFromHost = channelToHost.RequestFromHost;
            for (int j = 0; j < requestFromHost.Length; j++)
            {
                if (requestFromHost[j].PackageFormat.ID == TestClientMessages.COLOR_CHANGE_NOTIFICATION)
                {
                    /// Color change notification arrived from the host.
                    int opID = requestFromHost[j].ReadInt(0);
                    PlayerColor newColor = (PlayerColor)requestFromHost[j].ReadByte(1);

                    if (opID >= 0 && opID - 1 != channelToHost.GuestIndex)
                    {
                        this.simulator.GetPlayer(opID).Color = newColor;
                        if (opID == 0)
                        {
                            this.uiCallMarshal.SelectNewHostColor(newColor);
                        }
                        else if (opID > 0 && channelToHost.ChannelStates[opID - 1] == DssChannelState.GUEST_CONNECTED)
                        {
                            this.uiCallMarshal.SelectNewGuestColor(opID - 1, newColor);
                        }
                    }
                }
            }

            if (this.newColorSelected)
            {
                if (this.applyNewColorSelection)
                {
                    this.uiCallMarshal.SetGuestControlStatus(channelToHost.GuestIndex, GuestControlStatus.GuestSideAccessGranted);
                    this.simulator.GetPlayer(channelToHost.GuestIndex + 1).Color = this.newColor;
                    this.newColorSelected = false;
                    this.applyNewColorSelection = false;
                }
                else
                {
                    /// Color will be changed only in the next setup step
                    this.applyNewColorSelection = true;
                }
            }

            return true;
        }

        /// <summary>
        /// Processes the actions arrived from the UI.
        /// </summary>
        private bool ProcessUiActions(IDssHostChannel channelToHost)
        {
            UiActionType[] uiActions = null;
            int[] firstParams = null;
            int[] secondParams = null;
            this.ActionQueue.GetAllActions(out uiActions, out firstParams, out secondParams);

            for (int i = 0; i < uiActions.Length; i++)
            {
                if (uiActions[i] == UiActionType.NewColorSelected)
                {
                    /// Send the request to the host and register that a color selection request is in progress.
                    int opID = firstParams[i];
                    if (!this.newColorSelected && opID - 1 == channelToHost.GuestIndex)
                    {
                        /// Send the request to the host.
                        PlayerColor newColor = (PlayerColor)secondParams[i];
                        RCPackage colorChgRq = RCPackage.CreateNetworkControlPackage(TestClientMessages.COLOR_CHANGE_REQUEST);
                        colorChgRq.WriteByte(0, (byte)newColor);
                        this.aw.Add(colorChgRq);

                        /// Register that a color selection request is in progress.
                        this.newColor = newColor;
                        this.newColorSelected = true;
                    }
                }
                else if (uiActions[i] == UiActionType.LeaveBtnPressed)
                {
                    /// TODO: notify the UI
                    return false;
                }
                else
                {
                    /// Otherwise ignore the action
                }
            }

            return true;
        }

        private LobbyInfo targetLobby;

        private DssChannelState[] previousChannelStates;

        private List<RCPackage> aw;

        private bool firstSetupStep;

        private PlayerColor newColor;

        private bool newColorSelected;

        private bool applyNewColorSelection;

        //private ColorSelectRQ colorSelectRq;
    }

    /*
    /// <summary>
    /// Represents a color selection request that has been sent to the host.
    /// </summary>
    struct ColorSelectRQ
    {
        /// <summary>
        /// Constructs an invalid ColorSelectRQ structure.
        /// </summary>
        public ColorSelectRQ()
        {
            this.originalColor = PlayerColor.White;
            this.newColor = PlayerColor.White;
            this.isValid = false;
        }

        /// <summary>
        /// Constructs a ColorSelectRQ structure with the given colors.
        /// </summary>
        public ColorSelectRQ(PlayerColor origColor, PlayerColor newColor)
        {
            this.originalColor = origColor;
            this.newColor = newColor;
            this.isValid = true;
        }

        /// <summary>
        /// Sets this ColorSelectRQ to invalid.
        /// </summary>
        public void Invalidate()
        {
            this.isValid = false;
        }

        /// <summary>
        /// Returns true if this ColorSelectRQ is valid, or false if it has been deleted.
        /// </summary>
        public bool IsValid { get { return this.isValid; } }

        /// <summary>
        /// Gets the original color before the request.
        /// </summary>
        public PlayerColor OriginalColor
        {
            get
            {
                if (!this.isValid) { throw new Exception("Unable to access invalid ColorSelectRQ!"); }
                return this.originalColor;
            }
        }

        /// <summary>
        /// Gets the requested new color.
        /// </summary>
        public PlayerColor NewColor
        {
            get
            {
                if (!this.isValid) { throw new Exception("Unable to access invalid ColorSelectRQ!"); }
                return this.newColor;
            }
        }

        /// <summary>
        /// The original color before the request.
        /// </summary>
        private readonly PlayerColor originalColor;

        /// <summary>
        /// The requested new color.
        /// </summary>
        private readonly PlayerColor newColor;

        /// <summary>
        /// True if this ColorSelectRQ is valid, or false if it has been deleted.
        /// </summary>
        private bool isValid;
    }
    */
}
