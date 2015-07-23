using System.Collections.Generic;
using RC.Common;
using RC.NetworkingSystem;
using RC.RenderSystem;
using System.Drawing;
using RC.Common.Diagnostics;

namespace RC.DssServices.Test
{
    class DssHostThread : DssThread, IDssHostSetup
    {
        public DssHostThread(ControlStatusManager ctrlStatusMgr, IUiInvoke ui, TestSimulator simulator, int opCount, INetwork network)
            : base(ctrlStatusMgr, ui, simulator, network)
        {
            this.firstSetupStep = true;
            this.opCount = opCount;
            this.previousChannelStates = null;
            this.rqs = new List<RCPackage>[opCount - 1];
            for (int i = 0; i < this.rqs.Length; i++)
            {
                this.rqs[i] = new List<RCPackage>();
            }
        }

        #region IDssHostSetup members

        /// <see cref="IDssHostSetup.GuestConnectionLost"/>
        public bool GuestConnectionLost(int guestIndex)
        {
            TraceManager.WriteAllTrace(string.Format("SETUP_CALL: Guest-{0} connection lost.", guestIndex), TestClientTraceFilters.TEST_INFO);
            this.simulator.GetPlayer(guestIndex + 1).Deactivate();
            Rectangle refreshedArea;
            Display.Instance.RenderOneFrame(out refreshedArea);
            this.uiCallMarshal.RefreshDisplay();
            return true;
        }

        /// <see cref="IDssHostSetup.GuestLeftDss"/>
        public bool GuestLeftDss(int guestIndex)
        {
            TraceManager.WriteAllTrace(string.Format("SETUP_CALL: Guest-{0} left DSS.", guestIndex), TestClientTraceFilters.TEST_INFO);
            this.simulator.GetPlayer(guestIndex + 1).Deactivate();
            Rectangle refreshedArea;
            Display.Instance.RenderOneFrame(out refreshedArea);
            this.uiCallMarshal.RefreshDisplay();

            this.previousChannelStates[guestIndex] = DssChannelState.CHANNEL_OPENED;
            this.uiCallMarshal.SetGuestControlStatus(guestIndex, GuestControlStatus.HostSideOpened);
            return true;
        }

        /// <see cref="IDssHostSetup.ExecuteNextStep"/>
        public DssSetupResult ExecuteNextStep(IDssGuestChannel[] channelsToGuests)
        {
            TraceManager.WriteAllTrace("SETUP_CALL: Execute setup step", TestClientTraceFilters.TEST_INFO);
            foreach (List<RCPackage> rq in this.rqs)
            {
                rq.Clear();
            }

            if (this.firstSetupStep)
            {
                FirstSetupStep(channelsToGuests);
                this.firstSetupStep = false;
            }

            DssSetupResult result = ProcessUiActions(channelsToGuests);
            if (result == DssSetupResult.CONTINUE_SETUP)
            {
                ProcessGuestChannels(channelsToGuests);
                int i = 0;
                foreach (List<RCPackage> rq in this.rqs)
                {
                    channelsToGuests[i].RequestToGuest = rq.ToArray();
                    i++;
                }
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
            DssServiceAccess.CreateDSS(this.opCount, this.network, this, this);

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
        /// Call this function to initialize the UI for the setup stage and send reset messages to the connected guests.
        /// </summary>
        private void FirstSetupStep(IDssGuestChannel[] channelsToGuests)
        {
            /// Reset the UI
            this.simulator.GetPlayer(0).Reset();
            this.simulator.GetPlayer(0).Activate();

            this.uiCallMarshal.SetMainControlStatus(FormStatus.HostSide);

            this.uiCallMarshal.SelectNewHostColor(this.simulator.GetPlayer(0).Color);
            this.uiCallMarshal.SetHostControlStatus(HostControlStatus.AccessGranted);

            this.previousChannelStates = new DssChannelState[channelsToGuests.Length];
            for (int i = 0; i < this.previousChannelStates.Length; i++)
            {
                this.simulator.GetPlayer(i + 1).Reset();
                this.previousChannelStates[i] = channelsToGuests[i].ChannelState;
                if (channelsToGuests[i].ChannelState == DssChannelState.CHANNEL_OPENED)
                {
                    this.uiCallMarshal.SetGuestControlStatus(i, GuestControlStatus.HostSideOpened);
                }
                else if (channelsToGuests[i].ChannelState == DssChannelState.CHANNEL_CLOSED)
                {
                    this.uiCallMarshal.SetGuestControlStatus(i, GuestControlStatus.HostSideClosed);
                }
                else if (channelsToGuests[i].ChannelState == DssChannelState.GUEST_CONNECTED)
                {
                    this.simulator.GetPlayer(i + 1).Activate();
                    this.uiCallMarshal.SelectNewGuestColor(i, this.simulator.GetPlayer(i + 1).Color);
                    this.uiCallMarshal.SetGuestControlStatus(i, GuestControlStatus.HostSideEngaged);
                }
            }

            /// Send reset messages
            for (int i = 0; i < channelsToGuests.Length; i++)
            {
                if (channelsToGuests[i].ChannelState == DssChannelState.GUEST_CONNECTED)
                {
                    byte[] colors = new byte[this.simulator.MaxNumOfPlayers];
                    int[] xCoords = new int[this.simulator.MaxNumOfPlayers];
                    int[] yCoords = new int[this.simulator.MaxNumOfPlayers];
                    for (int j = 0; j < colors.Length; j++)
                    {
                        colors[j] = (byte)this.simulator.GetPlayer(j).Color;
                        xCoords[j] = this.simulator.GetPlayer(j).Position.X;
                        yCoords[j] = this.simulator.GetPlayer(j).Position.Y;
                    }
                    RCPackage reset = RCPackage.CreateNetworkControlPackage(TestClientMessages.RESET);
                    reset.WriteByteArray(0, colors);
                    reset.WriteIntArray(1, xCoords);
                    reset.WriteIntArray(2, yCoords);
                    this.rqs[i].Add(reset);
                }
            }
        }

        /// <summary>
        /// Processes the channel events and incoming answers arrived from the guests.
        /// </summary>
        private void ProcessGuestChannels(IDssGuestChannel[] channelsToGuests)
        {
            RCSet<int> newGuests = new RCSet<int>();

            /// First collect the new guests
            for (int i = 0; i < this.previousChannelStates.Length; i++)
            {
                if (this.previousChannelStates[i] != channelsToGuests[i].ChannelState)
                {
                    /// The state of a channel has changed
                    this.previousChannelStates[i] = channelsToGuests[i].ChannelState;
                    if (channelsToGuests[i].ChannelState == DssChannelState.CHANNEL_OPENED)
                    {
                        this.uiCallMarshal.SetGuestControlStatus(i, GuestControlStatus.HostSideOpened);
                    }
                    else if (channelsToGuests[i].ChannelState == DssChannelState.CHANNEL_CLOSED)
                    {
                        this.uiCallMarshal.SetGuestControlStatus(i, GuestControlStatus.HostSideClosed);
                    }
                    else if (channelsToGuests[i].ChannelState == DssChannelState.GUEST_CONNECTED)
                    {
                        this.uiCallMarshal.SetGuestControlStatus(i, GuestControlStatus.HostSideEngaged);
                        this.simulator.GetPlayer(i + 1).Activate();
                        newGuests.Add(i);
                    }
                }
            }

            /// Then process the answers of any other guests.
            for (int i = 0; i < this.previousChannelStates.Length; i++)
            {
                if (!newGuests.Contains(i) && channelsToGuests[i].ChannelState == DssChannelState.GUEST_CONNECTED)
                {
                    /// If a guest is connected to this channel, process it's answer.
                    RCPackage[] answerFromGuest = channelsToGuests[i].AnswerFromGuest;
                    for (int j = 0; j < answerFromGuest.Length; j++)
                    {
                        if (answerFromGuest[j].PackageFormat.ID == TestClientMessages.COLOR_CHANGE_REQUEST)
                        {
                            /// Color change request arrived from the guest.
                            PlayerColor newColor = (PlayerColor)answerFromGuest[j].ReadByte(0);
                            this.simulator.GetPlayer(i + 1).Color = newColor;

                            /// Notify the other connected guests.
                            for (int k = 0; k < channelsToGuests.Length; k++)
                            {
                                if (!newGuests.Contains(k) && i != k && channelsToGuests[k].ChannelState == DssChannelState.GUEST_CONNECTED)
                                {
                                    RCPackage colorChgNotif =
                                        RCPackage.CreateNetworkControlPackage(TestClientMessages.COLOR_CHANGE_NOTIFICATION);
                                    colorChgNotif.WriteInt(0, i + 1);
                                    colorChgNotif.WriteByte(1, (byte)newColor);
                                    this.rqs[k].Add(colorChgNotif);
                                }
                            }

                            /// Notify the UI
                            this.uiCallMarshal.SelectNewGuestColor(i, newColor);
                            break;  /// Ignore the remaining messages
                        }
                    }
                }
            }

            /// Send a reset message to the new guests
            foreach (int newGuestIdx in newGuests)
            {
                byte[] colors = new byte[this.simulator.MaxNumOfPlayers];
                int[] xCoords = new int[this.simulator.MaxNumOfPlayers];
                int[] yCoords = new int[this.simulator.MaxNumOfPlayers];
                for (int j = 0; j < colors.Length; j++)
                {
                    colors[j] = (byte)this.simulator.GetPlayer(j).Color;
                    xCoords[j] = this.simulator.GetPlayer(j).Position.X;
                    yCoords[j] = this.simulator.GetPlayer(j).Position.Y;
                }
                RCPackage reset = RCPackage.CreateNetworkControlPackage(TestClientMessages.RESET);
                reset.WriteByteArray(0, colors);
                reset.WriteIntArray(1, xCoords);
                reset.WriteIntArray(2, yCoords);
                this.rqs[newGuestIdx].Add(reset);
            }
        }

        /// <summary>
        /// Processes the actions arrived from the UI.
        /// </summary>
        private DssSetupResult ProcessUiActions(IDssGuestChannel[] channelsToGuests)
        {
            PlayerColor currentColor = this.simulator.GetPlayer(0).Color;
            PlayerColor newColor = PlayerColor.White;
            bool newColorSelected = false;

            UiActionType[] uiActions = null;
            int[] firstParams = null;
            int[] secondParams = null;
            this.ActionQueue.GetAllActions(out uiActions, out firstParams, out secondParams);

            for (int i = 0; i < uiActions.Length; i++)
            {
                if (uiActions[i] == UiActionType.CloseBtnPressed)
                {
                    int opID = firstParams[i];
                    if (channelsToGuests[opID - 1].ChannelState == DssChannelState.GUEST_CONNECTED)
                    {
                        if (this.simulator.GetPlayer(opID).IsActive)
                        {
                            this.simulator.GetPlayer(opID).Deactivate();
                        }
                        /// Drop the guest if it is connected to the channel...
                        channelsToGuests[opID - 1].DropGuest(false);
                    }
                    else
                    {
                        /// otherwise close the channel.
                        channelsToGuests[opID - 1].CloseChannel();
                    }

                    /// and notify the UI.
                    this.uiCallMarshal.SetGuestControlStatus(opID - 1, GuestControlStatus.HostSideClosed);
                }
                else if (uiActions[i] == UiActionType.OpenBtnPressed)
                {
                    int opID = firstParams[i];
                    if (channelsToGuests[opID - 1].ChannelState == DssChannelState.GUEST_CONNECTED)
                    {
                        if (this.simulator.GetPlayer(opID).IsActive)
                        {
                            this.simulator.GetPlayer(opID).Deactivate();
                        }
                        /// Drop the guest if it is connected to the channel...
                        channelsToGuests[opID - 1].DropGuest(true);
                    }
                    else
                    {
                        /// otherwise open the channel.
                        channelsToGuests[opID - 1].OpenChannel();
                    }

                    /// and notify the UI.
                    this.uiCallMarshal.SetGuestControlStatus(opID - 1, GuestControlStatus.HostSideOpened);
                }
                else if (uiActions[i] == UiActionType.NewColorSelected)
                {
                    int opID = firstParams[i];
                    if (opID == 0)
                    {
                        newColor = (PlayerColor)secondParams[i];
                        newColorSelected = true;

                        for (int j = 0; j < channelsToGuests.Length; j++)
                        {
                            if (channelsToGuests[j].ChannelState == DssChannelState.GUEST_CONNECTED)
                            {
                                RCPackage colorChgNotif = RCPackage.CreateNetworkControlPackage(TestClientMessages.COLOR_CHANGE_NOTIFICATION);
                                colorChgNotif.WriteInt(0, opID);
                                colorChgNotif.WriteByte(1, (byte)newColor);
                                this.rqs[j].Add(colorChgNotif);
                            }
                        }
                    }

                    /// Notify the UI
                    //this.uiCallMarshal.SetHostControlStatus(HostControlStatus.AccessGranted);
                }
                else if (uiActions[i] == UiActionType.StartSimBtnPressed)
                {
                    if (newColorSelected)
                    {
                        /// Set back the color in the combobox
                        this.uiCallMarshal.SelectNewHostColor(currentColor);
                    }
                    return DssSetupResult.START_SIMULATION;
                }
                else if (uiActions[i] == UiActionType.LeaveBtnPressed)
                {
                    /// TODO: notify the UI
                    return DssSetupResult.LEAVE_DSS;
                }
                else
                {
                    /// Otherwise ignore the action
                }
            }

            if (newColorSelected)
            {
                /// Save the selected new color
                this.simulator.GetPlayer(0).Color = newColor;
            }

            this.uiCallMarshal.SetHostControlStatus(HostControlStatus.AccessGranted);
            return DssSetupResult.CONTINUE_SETUP;
        }

        /// <summary>
        /// The maximum number of the operators.
        /// </summary>
        private int opCount;

        private DssChannelState[] previousChannelStates;

        private List<RCPackage>[] rqs;

        private bool firstSetupStep;
    }
}
