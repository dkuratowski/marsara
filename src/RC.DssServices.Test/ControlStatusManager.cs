using System;
using System.Windows.Forms;
using RC.NetworkingSystem;
using RC.Common;

namespace RC.DssServices.Test
{
    /// <summary>
    /// Enumerates the possible states of the main form.
    /// </summary>
    enum FormStatus
    {
        Inactive = 0,       /// Not connected to any DSS
        HostSide = 1,       /// Connected to a DSS as the host
        GuestSide = 2,      /// Connected to a DSS as a guest
        Simulating = 3,     /// Running the simulation stage
        Waiting = 4         /// When we are waiting for an event from another thread, every control has to be disabled
    }

    /// <summary>
    /// This class is responsible for switching the states of the controls on the UI.
    /// </summary>
    class ControlStatusManager : ILobbyLocator
    {
        /// <summary>
        /// Creates a ControlStatusManager object.
        /// </summary>
        public ControlStatusManager(HostControlStatusManager hostStatus,
                                    GuestControlStatusManager[] guestStatuses,
                                    ListBox lstDssLobbies,
                                    Button btnCreateDss,
                                    Button btnJoinDss,
                                    Button btnLeaveDss,
                                    Button btnStartSim,
                                    //Graphics displayGc,
                                    ExtComboChangeMgr extComboChMgr)
        {
            this.extComboChMgr = extComboChMgr;
            this.hostStatus = hostStatus;
            this.guestStatuses = guestStatuses;

            this.lstDssLobbies = lstDssLobbies;
            this.btnCreateDss = btnCreateDss;
            this.btnJoinDss = btnJoinDss;
            this.btnLeaveDss = btnLeaveDss;
            this.btnStartSim = btnStartSim;

            //this.displayGc = displayGc;

            this.status = FormStatus.Inactive;
            UpdateControls();
        }

        /// <summary>
        /// Updates the controls on the UI.
        /// </summary>
        public void UpdateControls()
        {
            if (MainForm.UiThread != RCThread.CurrentThread) { throw new Exception("This call is only allowed from the UI-thread!"); }

            this.hostStatus.UpdateControls();
            foreach (GuestControlStatusManager guestStatus in this.guestStatuses)
            {
                guestStatus.UpdateControls();
            }

            if (this.status == FormStatus.Inactive)
            {
                this.btnCreateDss.Enabled = true;
                this.btnJoinDss.Enabled = (this.lstDssLobbies.SelectedItem != null);
                this.btnLeaveDss.Enabled = false;
                this.btnStartSim.Enabled = false;
            }
            else if (this.status == FormStatus.GuestSide)
            {
                this.btnCreateDss.Enabled = false;
                this.btnJoinDss.Enabled = false;
                this.btnLeaveDss.Enabled = true;
                this.btnStartSim.Enabled = false;
            }
            else if (this.status == FormStatus.HostSide)
            {
                this.btnCreateDss.Enabled = false;
                this.btnJoinDss.Enabled = false;
                this.btnLeaveDss.Enabled = true;
                this.btnStartSim.Enabled = true;
            }
            else if (this.status == FormStatus.Simulating)
            {
                this.btnCreateDss.Enabled = false;
                this.btnJoinDss.Enabled = false;
                this.btnLeaveDss.Enabled = true;
                this.btnStartSim.Enabled = false;
            }
            else if (this.status == FormStatus.Waiting)
            {
                this.btnCreateDss.Enabled = false;
                this.btnJoinDss.Enabled = false;
                this.btnLeaveDss.Enabled = false;
                this.btnStartSim.Enabled = false;
            }
        }

        #region ILobbyLocator members

        /// <see cref="ILobbyLocator.LobbyFound"/>
        public void LobbyFound(LobbyInfo foundLobby)
        {
            if (RCThread.CurrentThread != MainForm.UiThread) { throw new Exception("This call is only allowed from the UI-thread!"); }

            foreach (object item in this.lstDssLobbies.Items)
            {
                LobbyInfo infoItem = (LobbyInfo)item;
                if (infoItem.ID == foundLobby.ID)
                {
                    /// Already in the list.
                    return;
                }
            }
            this.lstDssLobbies.Items.Add(foundLobby);
        }

        /// <see cref="ILobbyLocator.LobbyChanged"/>
        public void LobbyChanged(LobbyInfo changedLobby)
        {
            if (RCThread.CurrentThread != MainForm.UiThread) { throw new Exception("This call is only allowed from the UI-thread!"); }

            /// Normal invoke.
            foreach (object item in this.lstDssLobbies.Items)
            {
                LobbyInfo infoItem = (LobbyInfo)item;
                if (infoItem.ID == changedLobby.ID)
                {
                    this.lstDssLobbies.Items.Remove(item);
                    this.lstDssLobbies.Items.Add(changedLobby);
                    return;
                }
            }
        }

        /// <see cref="ILobbyLocator.LobbyVanished"/>
        public void LobbyVanished(LobbyInfo vanishedLobby)
        {
            if (RCThread.CurrentThread != MainForm.UiThread) { throw new Exception("This call is only allowed from the UI-thread!"); }

            /// Normal invoke.
            foreach (object item in this.lstDssLobbies.Items)
            {
                LobbyInfo infoItem = (LobbyInfo)item;
                if (infoItem.ID == vanishedLobby.ID)
                {
                    this.lstDssLobbies.Items.Remove(item);
                    return;
                }
            }
        }

        #endregion

        private HostControlStatusManager hostStatus;
        public HostControlStatusManager HostStatus { get { return this.hostStatus; } }

        private GuestControlStatusManager[] guestStatuses;
        public GuestControlStatusManager[] GuestStatuses { get { return this.guestStatuses; } }

        private ListBox lstDssLobbies;
        private Button btnCreateDss;
        private Button btnJoinDss;
        private Button btnLeaveDss;
        private Button btnStartSim;

        public FormStatus Status
        {
            get
            {
                if (MainForm.UiThread != RCThread.CurrentThread) { throw new Exception("This call is only allowed from the UI-thread!"); }
                return this.status;
            }
            set
            {
                if (MainForm.UiThread != RCThread.CurrentThread) { throw new Exception("This call is only allowed from the UI-thread!"); }
                this.status = value;
            }
        }
        private FormStatus status;

        private ExtComboChangeMgr extComboChMgr;

        //private Graphics displayGc;
    }

    /// <summary>
    /// Enumerates the possible states of the host control on the UI.
    /// </summary>
    enum HostControlStatus
    {
        Inactive = 0,       /// Not connected to any DSS
        AccessGranted = 1,  /// Connected to a DSS as the host --> color selection enabled
        AccessDenied = 2,   /// Connected to a DSS as a guest --> color selection disabled
        Simulating = 3,     /// Simulation is currently running
        Waiting = 4         /// Waiting for an asynchron event
    }

    /// <summary>
    /// This class is responsible for switching the states of the host controls on the UI.
    /// </summary>
    class HostControlStatusManager
    {
        /// <summary>
        /// Creates a HostControlStatusManager object.
        /// </summary>
        public HostControlStatusManager(PictureBox picChannelState, ComboBox cbColorSelector, ExtComboChangeMgr extComboChMgr)
        {
            this.extComboChMgr = extComboChMgr;
            this.picChannelState = picChannelState;
            this.cbColorSelector = cbColorSelector;

            this.status = HostControlStatus.Inactive;
            UpdateControls();
        }

        /// <summary>
        /// Updates the controls on the host control.
        /// </summary>
        public void UpdateControls()
        {
            if (MainForm.UiThread != RCThread.CurrentThread) { throw new Exception("This call is only allowed from the UI-thread!"); }

            if (this.status == HostControlStatus.Inactive)
            {
                this.picChannelState.Image = MainForm.unknownIndicator;
                this.cbColorSelector.Enabled = false;
                this.extComboChMgr.ChangeSelectedIndex(this.cbColorSelector, 0);
            }
            else if (this.status == HostControlStatus.AccessGranted)
            {
                this.picChannelState.Image = MainForm.connectedIndicator;
                this.cbColorSelector.Enabled = true;
            }
            else if (this.status == HostControlStatus.AccessDenied)
            {
                this.picChannelState.Image = MainForm.connectedIndicator;
                this.cbColorSelector.Enabled = false;
            }
            else if (this.status == HostControlStatus.Simulating)
            {
                this.picChannelState.Image = MainForm.connectedIndicator;
                this.cbColorSelector.Enabled = false;
            }
            else if (this.status == HostControlStatus.Waiting)
            {
                this.picChannelState.Image = MainForm.connectedIndicator;
                this.cbColorSelector.Enabled = false;
            }
        }

        /// <summary>
        /// Selects a new color in the combobox of the host.
        /// </summary>
        public void SelectNewColor(PlayerColor newColor)
        {
            this.extComboChMgr.ChangeSelectedIndex(this.cbColorSelector, (int)newColor);
        }

        private PictureBox picChannelState;
        public PictureBox PicChannelState
        {
            get
            {
                if (MainForm.UiThread != RCThread.CurrentThread) { throw new Exception("This call is only allowed from the UI-thread!"); }
                return this.picChannelState;
            }
        }

        private ComboBox cbColorSelector;
        public ComboBox CbColorSelector
        {
            get
            {
                if (MainForm.UiThread != RCThread.CurrentThread) { throw new Exception("This call is only allowed from the UI-thread!"); }
                return this.cbColorSelector;
            }
        }

        public HostControlStatus Status
        {
            get
            {
                if (MainForm.UiThread != RCThread.CurrentThread) { throw new Exception("This call is only allowed from the UI-thread!"); }
                return this.status;
            }
            set
            {
                if (MainForm.UiThread != RCThread.CurrentThread) { throw new Exception("This call is only allowed from the UI-thread!"); }
                this.status = value;
            }
        }
        private HostControlStatus status;
        private ExtComboChangeMgr extComboChMgr;
    }

    /// <summary>
    /// Enumerates the possible states of a guest control on the UI.
    /// </summary>
    enum GuestControlStatus
    {
        Inactive = 0,                       /// Not connected to any DSS
        HostSideOpened = 1,                 /// Guest channel opened at host side
        HostSideClosed = 2,                 /// Guest channel closed at host side
        HostSideEngaged = 3,                /// Guest channel engaged at host side
        HostSideWaitingOpen = 4,            /// Guest channel waiting for open at host side
        HostSideWaitingClose = 5,           /// Guest channel waiting for close at host side
        HostSideWaitingDrop = 6,            /// Guest channel waiting for drop at host side
        GuestSideAccessGranted = 7,         /// Guest channel color selection enabled at guest side
        GuestSideAccessDenied = 8,          /// Guest channel color selection disabled at guest side
        GuestSideWaitingColorChange = 9,    /// Guest channel waiting for color selection at guest side
        GuestSideOpened = 10,               /// Guest channel opened at guest side
        GuestSideClosed = 11,               /// Guest channel closed at guest side
        SimulatingEngaged = 12,             /// Guest channel engaged during simulation stage
        SimulatingClosed = 13               /// Guest channel closed during simulation stage
    }

    /// <summary>
    /// This class is responsible for switching the states of a guest control on the UI.
    /// </summary>
    class GuestControlStatusManager
    {
        /// <summary>
        /// Creates a GuestControlStatusManager object.
        /// </summary>
        public GuestControlStatusManager(PictureBox picChannelState,
                                         Button btnOpen,
                                         Button btnClose,
                                         ComboBox cbColorSelector,
                                         int guestIndex,
                                         ExtComboChangeMgr extComboChMgr)
        {
            this.extComboChMgr = extComboChMgr;
            this.picChannelState = picChannelState;
            this.cbColorSelector = cbColorSelector;
            this.btnClose = btnClose;
            this.btnOpen = btnOpen;
            this.guestIndex = guestIndex;

            this.status = GuestControlStatus.Inactive;
            UpdateControls();
        }

        /// <summary>
        /// Updates the controls on the guest control.
        /// </summary>
        public void UpdateControls()
        {
            if (MainForm.UiThread != RCThread.CurrentThread) { throw new Exception("This call is only allowed from the UI-thread!"); }

            if (this.status == GuestControlStatus.Inactive)
            {
                this.picChannelState.Image = MainForm.unknownIndicator;
                this.cbColorSelector.Enabled = false;
                this.extComboChMgr.ChangeSelectedIndex(this.cbColorSelector, this.guestIndex + 1);
                this.btnClose.Enabled = false;
                this.btnOpen.Enabled = false;
            }
            else if (this.status == GuestControlStatus.HostSideOpened)
            {
                this.picChannelState.Image = MainForm.openedIndicator;
                this.cbColorSelector.Enabled = false;
                this.btnClose.Enabled = true;
                this.btnOpen.Enabled = false;
            }
            else if (this.status == GuestControlStatus.HostSideClosed)
            {
                this.picChannelState.Image = MainForm.closedIndicator;
                this.cbColorSelector.Enabled = false;
                //this.extComboChMgr.ChangeSelectedIndex(this.cbColorSelector, this.guestIndex + 1);
                this.btnClose.Enabled = false;
                this.btnOpen.Enabled = true;
            }
            else if (this.status == GuestControlStatus.HostSideEngaged)
            {
                this.picChannelState.Image = MainForm.connectedIndicator;
                this.cbColorSelector.Enabled = false;
                this.btnClose.Enabled = true;
                this.btnOpen.Enabled = true;
            }
            else if (this.status == GuestControlStatus.HostSideWaitingOpen)
            {
                this.picChannelState.Image = MainForm.closedIndicator;
                this.cbColorSelector.Enabled = false;
                this.btnClose.Enabled = false;
                this.btnOpen.Enabled = false;
            }
            else if (this.status == GuestControlStatus.HostSideWaitingClose)
            {
                this.picChannelState.Image = MainForm.openedIndicator;
                this.cbColorSelector.Enabled = false;
                this.btnClose.Enabled = false;
                this.btnOpen.Enabled = false;
            }
            else if (this.status == GuestControlStatus.HostSideWaitingDrop)
            {
                this.picChannelState.Image = MainForm.connectedIndicator;
                this.cbColorSelector.Enabled = false;
                this.btnClose.Enabled = false;
                this.btnOpen.Enabled = false;
            }
            else if (this.status == GuestControlStatus.GuestSideAccessGranted)
            {
                this.picChannelState.Image = MainForm.connectedIndicator;
                this.cbColorSelector.Enabled = true;
                this.btnClose.Enabled = false;
                this.btnOpen.Enabled = false;
            }
            else if (this.status == GuestControlStatus.GuestSideAccessDenied)
            {
                this.picChannelState.Image = MainForm.connectedIndicator;
                this.cbColorSelector.Enabled = false;
                this.btnClose.Enabled = false;
                this.btnOpen.Enabled = false;
            }
            else if (this.status == GuestControlStatus.GuestSideWaitingColorChange)
            {
                this.picChannelState.Image = MainForm.connectedIndicator;
                this.cbColorSelector.Enabled = false;
                this.btnClose.Enabled = false;
                this.btnOpen.Enabled = false;
            }
            else if (this.status == GuestControlStatus.GuestSideOpened)
            {
                this.picChannelState.Image = MainForm.openedIndicator;
                this.cbColorSelector.Enabled = false;
                this.btnClose.Enabled = false;
                this.btnOpen.Enabled = false;
            }
            else if (this.status == GuestControlStatus.GuestSideClosed)
            {
                this.picChannelState.Image = MainForm.closedIndicator;
                this.cbColorSelector.Enabled = false;
                //this.extComboChMgr.ChangeSelectedIndex(this.cbColorSelector, this.guestIndex + 1);
                this.btnClose.Enabled = false;
                this.btnOpen.Enabled = false;
            }
            else if (this.status == GuestControlStatus.SimulatingEngaged)
            {
                this.picChannelState.Image = MainForm.connectedIndicator;
                this.cbColorSelector.Enabled = false;
                this.btnClose.Enabled = false;
                this.btnOpen.Enabled = false;
            }
            else if (this.status == GuestControlStatus.SimulatingClosed)
            {
                this.picChannelState.Image = MainForm.closedIndicator;
                this.cbColorSelector.Enabled = false;
                //this.extComboChMgr.ChangeSelectedIndex(this.cbColorSelector, this.guestIndex + 1);
                this.btnClose.Enabled = false;
                this.btnOpen.Enabled = false;
            }
        }

        /// <summary>
        /// Selects a new color in the combobox of the host.
        /// </summary>
        public void SelectNewColor(PlayerColor newColor)
        {
            this.extComboChMgr.ChangeSelectedIndex(this.cbColorSelector, (int)newColor);
        }

        private PictureBox picChannelState;
        public PictureBox PicChannelState
        {
            get
            {
                if (MainForm.UiThread != RCThread.CurrentThread) { throw new Exception("This call is only allowed from the UI-thread!"); }
                return this.picChannelState;
            }
        }

        private ComboBox cbColorSelector;
        public ComboBox CbColorSelector
        {
            get
            {
                if (MainForm.UiThread != RCThread.CurrentThread) { throw new Exception("This call is only allowed from the UI-thread!"); }
                return this.cbColorSelector;
            }
        }

        private Button btnOpen;
        public Button BtnOpen
        {
            get
            {
                if (MainForm.UiThread != RCThread.CurrentThread) { throw new Exception("This call is only allowed from the UI-thread!"); }
                return this.btnOpen;
            }
        }

        private Button btnClose;
        public Button BtnClose
        {
            get
            {
                if (MainForm.UiThread != RCThread.CurrentThread) { throw new Exception("This call is only allowed from the UI-thread!"); }
                return this.btnClose;
            }
        }

        public int GuestIndex { get { return this.guestIndex; } }
        private int guestIndex;

        public GuestControlStatus Status
        {
            get
            {
                if (MainForm.UiThread != RCThread.CurrentThread) { throw new Exception("This call is only allowed from the UI-thread!"); }
                return this.status;
            }
            set
            {
                if (MainForm.UiThread != RCThread.CurrentThread) { throw new Exception("This call is only allowed from the UI-thread!"); }
                this.status = value;
            }
        }
        private GuestControlStatus status;
        private ExtComboChangeMgr extComboChMgr;
    }
}
