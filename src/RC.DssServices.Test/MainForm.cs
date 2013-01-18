using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using RC.NetworkingSystem;
using RC.Common;
using RC.RenderSystem;
using System.Diagnostics;
using RC.Common.Configuration;

namespace RC.DssServices.Test
{
    public partial class MainForm : Form, IUiInvoke
    {
        /// <summary>
        /// Gets the reference to the UI-thread.
        /// </summary>
        public static RCThread UiThread { get { return uiThread; } }
        
        /// <summary>
        /// Channel state indicator bitmaps
        /// </summary>
        public static Bitmap connectedIndicator;
        public static Bitmap openedIndicator;
        public static Bitmap closedIndicator;
        public static Bitmap unknownIndicator;

        /// <summary>
        /// Reference to the UI-thread.
        /// </summary>
        private static RCThread uiThread;

        public MainForm()
        {
            uiThread = RCThread.CurrentThread;

            InitializeComponent();

            /// Initialize the network
            List<int> wellKnownBroadcastPorts = new List<int>();
            wellKnownBroadcastPorts.Add(25000);
            wellKnownBroadcastPorts.Add(25001);
            wellKnownBroadcastPorts.Add(25002);
            wellKnownBroadcastPorts.Add(25003);
            this.network = Network.CreateLocalAreaNetwork(wellKnownBroadcastPorts);
            this.hostThread = null;
            this.guestThread = null;
            this.dssThread = null;

            /// Open the images for the indicators
            connectedIndicator = (Bitmap)Bitmap.FromFile("connected.png");
            openedIndicator = (Bitmap)Bitmap.FromFile("opened.png");
            closedIndicator = (Bitmap)Bitmap.FromFile("closed.png");
            unknownIndicator = (Bitmap)Bitmap.FromFile("unknown.png");

            this.extComboChMgr = new ExtComboChangeMgr();

            /// Create the control status managers
            this.hostCtrlStatusMgr = new HostControlStatusManager(this.picChannelStateHost, this.comboColorHost, this.extComboChMgr);
            //this.guestCtrlStatusMgrList = new GuestControlStatusManager[0];
            this.guestCtrlStatusMgrList = new GuestControlStatusManager[7];
            this.guestCtrlStatusMgrList[0] = new GuestControlStatusManager(this.picChannelState0, this.btnOpen0, this.btnClose0, this.comboColor0, 0, this.extComboChMgr);
            this.guestCtrlStatusMgrList[1] = new GuestControlStatusManager(this.picChannelState1, this.btnOpen1, this.btnClose1, this.comboColor1, 1, this.extComboChMgr);
            this.guestCtrlStatusMgrList[2] = new GuestControlStatusManager(this.picChannelState2, this.btnOpen2, this.btnClose2, this.comboColor2, 2, this.extComboChMgr);
            this.guestCtrlStatusMgrList[3] = new GuestControlStatusManager(this.picChannelState3, this.btnOpen3, this.btnClose3, this.comboColor3, 3, this.extComboChMgr);
            this.guestCtrlStatusMgrList[4] = new GuestControlStatusManager(this.picChannelState4, this.btnOpen4, this.btnClose4, this.comboColor4, 4, this.extComboChMgr);
            this.guestCtrlStatusMgrList[5] = new GuestControlStatusManager(this.picChannelState5, this.btnOpen5, this.btnClose5, this.comboColor5, 5, this.extComboChMgr);
            this.guestCtrlStatusMgrList[6] = new GuestControlStatusManager(this.picChannelState6, this.btnOpen6, this.btnClose6, this.comboColor6, 6, this.extComboChMgr);
            this.ctrlStatusMgr = new ControlStatusManager(this.hostCtrlStatusMgr,
                                                          this.guestCtrlStatusMgrList,
                                                          this.lstDssLobbies,
                                                          this.btnCreateDss,
                                                          this.btnJoinDss,
                                                          this.btnLeaveDss,
                                                          this.btnStartSim,
                                                          this.extComboChMgr);
            
            /// Create and fill the control maps
            this.indicatorMap = new Dictionary<PictureBox, GuestControlStatusManager>();
            for (int i = 0; i < this.guestCtrlStatusMgrList.Length; i++)
            {
                this.indicatorMap.Add(this.guestCtrlStatusMgrList[i].PicChannelState, this.guestCtrlStatusMgrList[i]);
            }

            this.openButtonMap = new Dictionary<Button, GuestControlStatusManager>();
            for (int i = 0; i < this.guestCtrlStatusMgrList.Length; i++)
            {
                this.openButtonMap.Add(this.guestCtrlStatusMgrList[i].BtnOpen, this.guestCtrlStatusMgrList[i]);
            }

            this.closeButtonMap = new Dictionary<Button, GuestControlStatusManager>();
            for (int i = 0; i < this.guestCtrlStatusMgrList.Length; i++)
            {
                this.closeButtonMap.Add(this.guestCtrlStatusMgrList[i].BtnClose, this.guestCtrlStatusMgrList[i]);
            }

            this.comboBoxMap = new Dictionary<ComboBox, GuestControlStatusManager>();
            for (int i = 0; i < this.guestCtrlStatusMgrList.Length; i++)
            {
                this.comboBoxMap.Add(this.guestCtrlStatusMgrList[i].CbColorSelector, this.guestCtrlStatusMgrList[i]);
            }

            this.lobbyLocatorMarshal = new LobbyLocatorMarshal(this.ctrlStatusMgr, this);
            //this.uiCallMarshal = new UiCallMarshal(this.ctrlStatusMgr, this);
            this.simulator = new TestSimulator(this.picDisplay.Width, this.picDisplay.Height, OP_COUNT);
            this.displayGc = this.picDisplay.CreateGraphics();
        }

        #region IUiInvoke members

        /// <see cref="IUiInvoke.InvokeUI"/>
        public void InvokeUI(SynchronUiCall uiCall)
        {
            if (RCThread.CurrentThread == MainForm.UiThread) { throw new Exception("This call is not allowed from the UI-thread!"); }
            this.BeginInvoke(new UiInvokedCallback(this.UiInvoked), new object[1] { uiCall });
            uiCall.Wait();
        }

        /// <see cref="IUiInvoke.InvalidateDisplay"/>
        public void InvalidateDisplay(/*Rectangle invalidArea*/)
        {
            this.Invalidate();
        }

        /// <see cref="IUiInvoke.EndOfDss"/>
        public void EndOfDss()
        {
            if (RCThread.CurrentThread != MainForm.UiThread) { throw new Exception("This function must be called from the UI-thread!"); }
            
            this.ctrlStatusMgr.HostStatus.Status = HostControlStatus.Inactive;
            for (int i = 0; i < this.ctrlStatusMgr.GuestStatuses.Length; ++i)
            {
                this.ctrlStatusMgr.GuestStatuses[i].Status = GuestControlStatus.Inactive;
            }

            this.ctrlStatusMgr.Status = FormStatus.Inactive;
            this.ctrlStatusMgr.UpdateControls();

            this.guestThread = null;
            this.hostThread = null;
            this.dssThread = null;
        }

        delegate void UiInvokedCallback(SynchronUiCall uiCall);

        /// <summary>
        /// This function is called in the context of the UI-thread when another thread calls the InvokeUI method.
        /// </summary>
        /// <param name="uiCall">This object represents the call.</param>
        private void UiInvoked(SynchronUiCall uiCall)
        {
            if (RCThread.CurrentThread != MainForm.UiThread) { throw new Exception("This call is only allowed from the UI-thread!"); }
            uiCall.Execute();
        }

        #endregion

        #region Event handlers

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (!e.ClipRectangle.IsEmpty)
            {
                Display.Instance.AccessCurrentFrame(this.displayGc, e.ClipRectangle);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.Text = string.Format("{0} -- PID: {1}", this.Text, Process.GetCurrentProcess().Id);
            this.network.StartLocatingLobbies(this.lobbyLocatorMarshal);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SynchronUiCall.TurnOffBlocking();

            if (this.hostThread != null || this.guestThread != null)
            {
                this.ctrlStatusMgr.Status = FormStatus.Waiting;
                this.ctrlStatusMgr.UpdateControls();
                this.dssThread.ActionQueue.PostAction(UiActionType.LeaveBtnPressed, -1, -1);

                this.dssThread.Join();
                this.hostThread = null;
                this.guestThread = null;
                this.dssThread = null;
            }

            this.network.ShutdownNetwork();
        }

        private void btnCreateDss_Click(object sender, EventArgs e)
        {
            if (this.hostThread == null && this.guestThread == null)
            {
                this.ctrlStatusMgr.Status = FormStatus.Waiting;
                this.ctrlStatusMgr.UpdateControls();
                this.hostThread = new DssHostThread(this.ctrlStatusMgr, this, this.simulator, OP_COUNT, this.network);
                this.dssThread = this.hostThread;
                this.dssThread.Start();
            }
        }

        private void btnJoinDss_Click(object sender, EventArgs e)
        {
            if (this.hostThread == null && this.guestThread == null && this.lstDssLobbies.SelectedItem != null)
            {
                this.ctrlStatusMgr.Status = FormStatus.Waiting;
                this.ctrlStatusMgr.UpdateControls();
                this.guestThread = new DssGuestThread(this.ctrlStatusMgr, this, this.simulator, (LobbyInfo)this.lstDssLobbies.SelectedItem, this.network);
                this.dssThread = this.guestThread;
                this.dssThread.Start();
            }
        }

        private void btnLeaveDss_Click(object sender, EventArgs e)
        {
            if (this.hostThread != null || this.guestThread != null)
            {
                //SynchronUiCall.TurnOffBlocking();

                /// Post the action to the DSS-thread.
                this.ctrlStatusMgr.Status = FormStatus.Waiting;
                this.ctrlStatusMgr.UpdateControls();
                this.dssThread.ActionQueue.PostAction(UiActionType.LeaveBtnPressed, -1, -1);

                //this.dssThread.Join();
                this.hostThread = null;
                this.guestThread = null;
                this.dssThread = null;

                //SynchronUiCall.TurnOnBlocking();
            }
        }

        private void lstDssLobbies_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.hostThread == null && this.guestThread == null)
            {
                this.ctrlStatusMgr.UpdateControls();
            }
        }

        private void comboColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox combo = (ComboBox)sender;
            if (this.extComboChMgr.IsComboBoxChangedByTheUser(combo))
            {
                /// Post the action to the DSS-thread.
                int selectedColor = combo.SelectedIndex;
                if (this.hostCtrlStatusMgr.CbColorSelector == combo)
                {
                    this.hostCtrlStatusMgr.Status = HostControlStatus.Waiting;
                    this.ctrlStatusMgr.UpdateControls();

                    this.dssThread.ActionQueue.PostAction(UiActionType.NewColorSelected, 0, selectedColor);
                }
                else if (this.comboBoxMap.ContainsKey(combo))
                {
                    this.comboBoxMap[combo].Status = GuestControlStatus.GuestSideWaitingColorChange;
                    this.ctrlStatusMgr.UpdateControls();

                    this.dssThread.ActionQueue.PostAction(UiActionType.NewColorSelected,
                                                          this.comboBoxMap[combo].GuestIndex + 1,
                                                          selectedColor);
                }
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            /// Post the action to the DSS-thread.
            if (this.openButtonMap.ContainsKey(btn))
            {
                this.openButtonMap[btn].Status = GuestControlStatus.HostSideWaitingOpen;
                this.ctrlStatusMgr.UpdateControls();
                this.dssThread.ActionQueue.PostAction(UiActionType.OpenBtnPressed,
                                                      this.openButtonMap[btn].GuestIndex + 1,
                                                      -1);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            /// Post the action to the DSS-thread.
            if (this.closeButtonMap.ContainsKey(btn))
            {
                this.closeButtonMap[btn].Status = GuestControlStatus.HostSideWaitingClose;
                this.ctrlStatusMgr.UpdateControls();
                this.dssThread.ActionQueue.PostAction(UiActionType.CloseBtnPressed,
                                                      this.closeButtonMap[btn].GuestIndex + 1,
                                                      -1);
            }
        }

        private void btnStartSim_Click(object sender, EventArgs e)
        {
            this.dssThread.ActionQueue.PostAction(UiActionType.StartSimBtnPressed, -1, -1);
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (this.dssThread != null)
            {
                if (e.KeyCode == Keys.W)
                {
                    /// Post the action to the DSS-thread.
                    this.dssThread.ActionQueue.PostAction(UiActionType.UpKeyPressed, -1, -1);
                }
                else if (e.KeyCode == Keys.S)
                {
                    /// Post the action to the DSS-thread.
                    this.dssThread.ActionQueue.PostAction(UiActionType.DownKeyPressed, -1, -1);
                }
                else if (e.KeyCode == Keys.A)
                {
                    /// Post the action to the DSS-thread.
                    this.dssThread.ActionQueue.PostAction(UiActionType.LeftKeyPressed, -1, -1);
                }
                else if (e.KeyCode == Keys.D)
                {
                    /// Post the action to the DSS-thread.
                    this.dssThread.ActionQueue.PostAction(UiActionType.RightKeyPressed, -1, -1);
                }
            }
        }

        #endregion

        /// <summary>
        /// Reference to the simulator data model.
        /// </summary>
        private TestSimulator simulator;

        /// <summary>
        /// Graphic context of the simulator display.
        /// </summary>
        private Graphics displayGc;

        /// <summary>
        /// Interface to the RC.NetworkingSystem
        /// </summary>
        private INetwork network;

        /// <summary>
        /// Reference to the host thread.
        /// </summary>
        private DssHostThread hostThread;

        /// <summary>
        /// Reference to the guest thread.
        /// </summary>
        private DssGuestThread guestThread;

        /// <summary>
        /// Reference to the guest/host thread.
        /// </summary>
        private DssThread dssThread;

        /// <summary>
        /// Reference to the LobbyLocatorMarshal object.
        /// </summary>
        private LobbyLocatorMarshal lobbyLocatorMarshal;

        /// <summary>
        /// Reference to the marshal interface of the UI.
        /// </summary>
        //private UiCallMarshal uiCallMarshal;

        /// <summary>
        /// Control maps.
        /// </summary>
        private Dictionary<PictureBox, GuestControlStatusManager> indicatorMap;
        private Dictionary<Button, GuestControlStatusManager> openButtonMap;
        private Dictionary<Button, GuestControlStatusManager> closeButtonMap;
        private Dictionary<ComboBox, GuestControlStatusManager> comboBoxMap;

        /// <summary>
        /// Control status managers.
        /// </summary>
        private ControlStatusManager ctrlStatusMgr;
        private HostControlStatusManager hostCtrlStatusMgr;
        private GuestControlStatusManager[] guestCtrlStatusMgrList;

        /// <summary>
        /// Reference to the ExtComboChangeMgr.
        /// </summary>
        private ExtComboChangeMgr extComboChMgr;

        /// <summary>
        /// The number of the operators in the test simulation.
        /// </summary>
        private static readonly int OP_COUNT = ConstantsTable.Get<int>("RC.DssServices.Test.OperatorCount");
    }
}
