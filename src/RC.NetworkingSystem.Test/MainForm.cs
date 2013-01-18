using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using RC.Common;
using System.Diagnostics;
using RC.Common.Configuration;

namespace RC.NetworkingSystem.Test
{
    public partial class MainForm : Form, ILobbyListener, ILobbyLocator
    {
        public MainForm()
        {
            InitializeComponent();

            /// Initialize the network
            List<int> wellKnownBroadcastPorts = new List<int>();
            wellKnownBroadcastPorts.Add(25000);
            wellKnownBroadcastPorts.Add(25001);
            wellKnownBroadcastPorts.Add(25002);
            wellKnownBroadcastPorts.Add(25003);
            this.network = Network.CreateLocalAreaNetwork(wellKnownBroadcastPorts);

            MY_FORMAT = RCPackageFormatMap.Get("RC.NetworkingSystem.Test.TestFormat");

            /// Open the images for the indicators
            this.engagedIndicator = (Bitmap)Bitmap.FromFile("engaged.png");
            this.openedIndicator = (Bitmap)Bitmap.FromFile("opened.png");
            this.closedIndicator = (Bitmap)Bitmap.FromFile("closed.png");
            this.unknownIndicator = (Bitmap)Bitmap.FromFile("unknown.png");

            this.lastKnownIdOfThisPeer = -1;
            this.lastKnownLineStates = null;

            /// Create and fill the control maps
            this.indicators = new PictureBox[8];
            this.indicatorMap = new Dictionary<PictureBox, int>();
            this.indicators[0] = this.picLineState0;
            this.indicators[1] = this.picLineState1;
            this.indicators[2] = this.picLineState2;
            this.indicators[3] = this.picLineState3;
            this.indicators[4] = this.picLineState4;
            this.indicators[5] = this.picLineState5;
            this.indicators[6] = this.picLineState6;
            this.indicators[7] = this.picLineState7;
            for (int i = 0; i < this.indicators.Length; i++) { this.indicatorMap.Add(this.indicators[i], i); }
            this.openButtons = new Button[8];
            this.openButtonMap = new Dictionary<Button, int>();
            this.openButtons[0] = this.btnOpen0;
            this.openButtons[1] = this.btnOpen1;
            this.openButtons[2] = this.btnOpen2;
            this.openButtons[3] = this.btnOpen3;
            this.openButtons[4] = this.btnOpen4;
            this.openButtons[5] = this.btnOpen5;
            this.openButtons[6] = this.btnOpen6;
            this.openButtons[7] = this.btnOpen7;
            for (int i = 0; i < this.openButtons.Length; i++) { this.openButtonMap.Add(this.openButtons[i], i); }
            this.closeButtons = new Button[8];
            this.closeButtonMap = new Dictionary<Button, int>();
            this.closeButtons[0] = this.btnClose0;
            this.closeButtons[1] = this.btnClose1;
            this.closeButtons[2] = this.btnClose2;
            this.closeButtons[3] = this.btnClose3;
            this.closeButtons[4] = this.btnClose4;
            this.closeButtons[5] = this.btnClose5;
            this.closeButtons[6] = this.btnClose6;
            this.closeButtons[7] = this.btnClose7;
            for (int i = 0; i < this.closeButtons.Length; i++) { this.closeButtonMap.Add(this.closeButtons[i], i); }
            this.textBoxes = new TextBox[8];
            this.textBoxMap = new Dictionary<TextBox, int>();
            this.textBoxes[0] = this.txtIO0;
            this.textBoxes[1] = this.txtIO1;
            this.textBoxes[2] = this.txtIO2;
            this.textBoxes[3] = this.txtIO3;
            this.textBoxes[4] = this.txtIO4;
            this.textBoxes[5] = this.txtIO5;
            this.textBoxes[6] = this.txtIO6;
            this.textBoxes[7] = this.txtIO7;
            for (int i = 0; i < this.textBoxes.Length; i++) { this.textBoxMap.Add(this.textBoxes[i], i); }
            this.targetSelectors = new CheckBox[8];
            this.targetSelectorMap = new Dictionary<CheckBox, int>();
            this.targetSelectors[0] = this.chkTargetSel0;
            this.targetSelectors[1] = this.chkTargetSel1;
            this.targetSelectors[2] = this.chkTargetSel2;
            this.targetSelectors[3] = this.chkTargetSel3;
            this.targetSelectors[4] = this.chkTargetSel4;
            this.targetSelectors[5] = this.chkTargetSel5;
            this.targetSelectors[6] = this.chkTargetSel6;
            this.targetSelectors[7] = this.chkTargetSel7;
            for (int i = 0; i < this.targetSelectors.Length; i++) { this.targetSelectorMap.Add(this.targetSelectors[i], i); }
            this.testButtons = new Button[8];
            this.testButtonMap = new Dictionary<Button, int>();
            this.testButtons[0] = this.btnTest0;
            this.testButtons[1] = this.btnTest1;
            this.testButtons[2] = this.btnTest2;
            this.testButtons[3] = this.btnTest3;
            this.testButtons[4] = this.btnTest4;
            this.testButtons[5] = this.btnTest5;
            this.testButtons[6] = this.btnTest6;
            this.testButtons[7] = this.btnTest7;
            for (int i = 0; i < this.testButtons.Length; i++) { this.testButtonMap.Add(this.testButtons[i], i); }
        }

        private INetwork network;
        private readonly int MY_FORMAT;

        private int internalCount = 0;

        private ILobbyClient joinedLobby = null;
        private ILobbyServer createdLobby = null;

        private LobbyLineState[] lastKnownLineStates;
        private int lastKnownIdOfThisPeer;

        private bool textChangingFromNetwork = false;

        private bool connectingToLobby = false;

        private Bitmap engagedIndicator;
        private Bitmap openedIndicator;
        private Bitmap closedIndicator;
        private Bitmap unknownIndicator;

        private PictureBox[] indicators;
        private Dictionary<PictureBox, int> indicatorMap;
        private Button[] openButtons;
        private Dictionary<Button, int> openButtonMap;
        private Button[] closeButtons;
        private Dictionary<Button, int> closeButtonMap;
        private TextBox[] textBoxes;
        private Dictionary<TextBox, int> textBoxMap;
        private CheckBox[] targetSelectors;
        private Dictionary<CheckBox, int> targetSelectorMap;
        private Button[] testButtons;
        private Dictionary<Button, int> testButtonMap;

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = string.Format("{0} -- PID: {1}", this.Text, Process.GetCurrentProcess().Id);
            UpdateControls();
            this.network.StartLocatingLobbies(this);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.network.ShutdownNetwork();
        }

        private void UpdateControls()
        {
            if (this.connectingToLobby)
            {
                /// Currently connecting
                for (int i = 0; i < this.indicators.Length; i++)
                {
                    this.indicators[i].Image = this.unknownIndicator;
                    this.openButtons[i].Enabled = false;
                    this.closeButtons[i].Enabled = false;
                    this.textBoxes[i].Enabled = false;
                    this.targetSelectors[i].Checked = false;
                    this.targetSelectors[i].Enabled = false;
                    this.testButtons[i].Enabled = false;
                }
                this.btnCreateLobby.Enabled = false;
                this.btnJoinLobby.Enabled = false;
                this.btnShutdownDisconnectLobby.Enabled = false;
            }
            else
            {
                if (this.joinedLobby == null && this.createdLobby == null)
                {
                    /// No created and no joined lobby exists.
                    for (int i = 0; i < this.indicators.Length; i++)
                    {
                        this.indicators[i].Image = this.unknownIndicator;
                        this.openButtons[i].Enabled = false;
                        this.closeButtons[i].Enabled = false;
                        this.textBoxes[i].Enabled = false;
                        this.targetSelectors[i].Checked = false;
                        this.targetSelectors[i].Enabled = false;
                        this.testButtons[i].Enabled = false;
                    }
                    this.btnCreateLobby.Enabled = true;
                    this.btnJoinLobby.Enabled = (this.lstLobbies.SelectedItem != null);
                    this.btnShutdownDisconnectLobby.Enabled = false;
                }
                else if (this.joinedLobby != null && this.createdLobby == null)
                {
                    /// Joined lobby exists.
                    for (int i = 0; i < this.indicators.Length; i++)
                    {
                        if (i < this.lastKnownLineStates.Length)
                        {
                            switch (this.lastKnownLineStates[i])
                            {
                                case LobbyLineState.Opened:
                                    this.indicators[i].Image = this.openedIndicator;
                                    break;
                                case LobbyLineState.Closed:
                                    this.indicators[i].Image = this.closedIndicator;
                                    break;
                                case LobbyLineState.Engaged:
                                    this.indicators[i].Image = this.engagedIndicator;
                                    break;
                                default:
                                    throw new Exception("Unexpected line state!");
                            }
                        }
                        else
                        {
                            this.indicators[i].Image = this.unknownIndicator;
                        }

                        this.openButtons[i].Enabled = false;
                        this.closeButtons[i].Enabled = false;
                        this.textBoxes[i].Enabled = (i == this.lastKnownIdOfThisPeer && !this.connectingToLobby);
                        this.targetSelectors[i].Enabled = (i < this.lastKnownLineStates.Length && i != this.lastKnownIdOfThisPeer && !this.connectingToLobby);
                        this.testButtons[i].Enabled = (i == 0 && !this.connectingToLobby); /// Internal message only to the server
                    }
                    this.btnCreateLobby.Enabled = false;
                    this.btnJoinLobby.Enabled = false;
                    this.btnShutdownDisconnectLobby.Text = "Disconnect lobby";
                    this.btnShutdownDisconnectLobby.Enabled = !this.connectingToLobby;
                }
                else if (this.joinedLobby == null && this.createdLobby != null)
                {
                    /// Created lobby exists.
                    for (int i = 0; i < this.indicators.Length; i++)
                    {
                        if (i < this.lastKnownLineStates.Length)
                        {
                            switch (this.lastKnownLineStates[i])
                            {
                                case LobbyLineState.Opened:
                                    this.indicators[i].Image = this.openedIndicator;
                                    break;
                                case LobbyLineState.Closed:
                                    this.indicators[i].Image = this.closedIndicator;
                                    break;
                                case LobbyLineState.Engaged:
                                    this.indicators[i].Image = this.engagedIndicator;
                                    break;
                                default:
                                    throw new Exception("Unexpected line state!");
                            }
                        }
                        else
                        {
                            this.indicators[i].Image = this.unknownIndicator;
                        }

                        this.openButtons[i].Enabled = (i < this.lastKnownLineStates.Length && i != this.lastKnownIdOfThisPeer &&
                                                       this.lastKnownLineStates[i] == LobbyLineState.Closed && !this.connectingToLobby);
                        this.closeButtons[i].Enabled = (i < this.lastKnownLineStates.Length && i != this.lastKnownIdOfThisPeer && !this.connectingToLobby &&
                                                       (this.lastKnownLineStates[i] == LobbyLineState.Opened || this.lastKnownLineStates[i] == LobbyLineState.Engaged));
                        this.textBoxes[i].Enabled = (i == this.lastKnownIdOfThisPeer && !this.connectingToLobby);
                        this.targetSelectors[i].Enabled = (i < this.lastKnownLineStates.Length && i != this.lastKnownIdOfThisPeer && !this.connectingToLobby);
                        this.testButtons[i].Enabled = (i != 0 && !this.connectingToLobby && this.lastKnownLineStates[i] == LobbyLineState.Engaged); /// Internal message to the clients
                    }
                    this.btnCreateLobby.Enabled = false;
                    this.btnJoinLobby.Enabled = false;
                    this.btnShutdownDisconnectLobby.Text = "Shutdown lobby";
                    this.btnShutdownDisconnectLobby.Enabled = !this.connectingToLobby;
                }
                else
                {
                    throw new Exception("Inconsistent state!");
                }
            }
        }

        #region ILobbyListener interface members

        delegate void PackageArrivedCallback(RCPackage package, int senderID);
        delegate void InternalArrivedFromServerCallback(RCPackage package);
        delegate void InternalArrivedFromClientCallback(RCPackage package, int senderID);
        delegate void LineStateReportCallback(int idOfThisPeer, LobbyLineState[] lineStates);
        delegate void LobbyLostCallback();

        /// <see cref="ILobbyListener.PackageArrived"/>
        public void PackageArrived(RCPackage package, int senderID)
        {
            if (this.InvokeRequired)
            {
                /// Invoke this function from the UI thread.
                this.Invoke(new PackageArrivedCallback(this.PackageArrived), new object[2] { package, senderID });
            }
            else
            {
                /// Normal invoke.
                if (senderID >= 0 && senderID < this.textBoxes.Length && package.PackageFormat.ID == MY_FORMAT)
                {
                    this.textChangingFromNetwork = true;
                    this.textBoxes[senderID].Text = package.ReadString(0);
                    this.textChangingFromNetwork = false;
                    UpdateControls();
                }
            }
        }

        /// <see cref="ILobbyListener.ControlPackageArrived"/>
        public void ControlPackageArrived(RCPackage package)
        {
            if (this.InvokeRequired)
            {
                /// Invoke this function from the UI thread.
                this.Invoke(new InternalArrivedFromServerCallback(this.ControlPackageArrived), new object[1] { package });
            }
            else
            {
                /// Normal invoke.
                if (package.PackageFormat.ID == MY_FORMAT)
                {
                    this.textChangingFromNetwork = true;
                    this.textBoxes[0].Text = package.ReadString(0);
                    this.textChangingFromNetwork = false;
                    UpdateControls();
                }
            }
        }

        /// <see cref="ILobbyListener.ControlPackageArrived"/>
        public void ControlPackageArrived(RCPackage package, int senderID)
        {
            if (this.InvokeRequired)
            {
                /// Invoke this function from the UI thread.
                this.Invoke(new InternalArrivedFromClientCallback(this.ControlPackageArrived), new object[2] { package, senderID });
            }
            else
            {
                /// Normal invoke.
                if (senderID >= 0 && senderID < this.textBoxes.Length && package.PackageFormat.ID == MY_FORMAT)
                {
                    this.textChangingFromNetwork = true;
                    this.textBoxes[senderID].Text = package.ReadString(0);
                    this.textChangingFromNetwork = false;
                    UpdateControls();
                }
            }
        }

        /// <see cref="ILobbyListener.LineStateReport"/>
        public void LineStateReport(int idOfThisPeer, LobbyLineState[] lineStates)
        {
            if (this.InvokeRequired)
            {
                /// Invoke this function from the UI thread.
                this.Invoke(new LineStateReportCallback(this.LineStateReport), new object[2] { idOfThisPeer, lineStates });
            }
            else
            {
                /// Normal invoke.
                if (lineStates.Length > 0 && lineStates.Length <= this.indicators.Length &&
                    idOfThisPeer >= 0 && idOfThisPeer < lineStates.Length)
                {
                    this.lastKnownLineStates = lineStates;
                    this.lastKnownIdOfThisPeer = idOfThisPeer;
                    this.connectingToLobby = false;
                    UpdateControls();
                }
            }
        }

        /// <see cref="ILobbyListener.LobbyLost"/>
        public void LobbyLost()
        {
            if (this.InvokeRequired)
            {
                /// Invoke this function from the UI thread.
                this.Invoke(new LobbyLostCallback(this.LobbyLost));
            }
            else
            {
                /// Normal invoke.
                this.joinedLobby = null;
                this.createdLobby = null;
                this.lastKnownLineStates = null;
                this.lastKnownIdOfThisPeer = -1;
                this.connectingToLobby = false;
                UpdateControls();
            }
        }

        #endregion

        #region ILobbyLocator interface members

        delegate void LobbyFoundCallback(LobbyInfo foundLobby);
        delegate void LobbyChangedCallback(LobbyInfo changedLobby);
        delegate void LobbyVanishedCallback(LobbyInfo vanishedLobby);

        /// <see cref="ILobbyLocator.LobbyFound"/>
        public void LobbyFound(LobbyInfo foundLobby)
        {
            if (this.InvokeRequired)
            {
                /// Invoke this function from the UI thread.
                this.Invoke(new LobbyFoundCallback(this.LobbyFound), new object[1] { foundLobby });
            }
            else
            {
                /// Normal invoke.
                foreach (object item in this.lstLobbies.Items)
                {
                    LobbyInfo infoItem = (LobbyInfo)item;
                    if (infoItem.ID == foundLobby.ID)
                    {
                        /// Already in the list.
                        return;
                    }
                }
                this.lstLobbies.Items.Add(foundLobby);
                UpdateControls();
            }
        }

        /// <see cref="ILobbyLocator.LobbyChanged"/>
        public void LobbyChanged(LobbyInfo changedLobby)
        {
            if (this.InvokeRequired)
            {
                /// Invoke this function from the UI thread.
                this.Invoke(new LobbyChangedCallback(this.LobbyChanged), new object[1] { changedLobby });
            }
            else
            {
                /// Normal invoke.
                foreach (object item in this.lstLobbies.Items)
                {
                    LobbyInfo infoItem = (LobbyInfo)item;
                    if (infoItem.ID == changedLobby.ID)
                    {
                        this.lstLobbies.Items.Remove(item);
                        this.lstLobbies.Items.Add(changedLobby);
                        UpdateControls();
                        return;
                    }
                }
            }
        }

        /// <see cref="ILobbyLocator.LobbyVanished"/>
        public void LobbyVanished(LobbyInfo vanishedLobby)
        {
            if (this.InvokeRequired)
            {
                /// Invoke this function from the UI thread.
                this.Invoke(new LobbyVanishedCallback(this.LobbyVanished), new object[1] { vanishedLobby });
            }
            else
            {
                /// Normal invoke.
                foreach (object item in this.lstLobbies.Items)
                {
                    LobbyInfo infoItem = (LobbyInfo)item;
                    if (infoItem.ID == vanishedLobby.ID)
                    {
                        this.lstLobbies.Items.Remove(item);
                        UpdateControls();
                        return;
                    }
                }
            }
        }

        #endregion

        #region Event handlers

        private void txtIO_TextChanged(object sender, EventArgs e)
        {
            if (!this.textChangingFromNetwork && this.textBoxMap[(TextBox)sender] == this.lastKnownIdOfThisPeer)
            {
                /// Get an interface for sending the message
                ILobby sendIface = null;
                if (this.joinedLobby != null && this.createdLobby == null) { sendIface = this.joinedLobby; }
                else if (this.joinedLobby == null && this.createdLobby != null) { sendIface = this.createdLobby; }

                if (sendIface != null)
                {
                    /// Create the package
                    RCPackage package = RCPackage.CreateNetworkCustomPackage(MY_FORMAT);
                    package.WriteString(0, ((TextBox)sender).Text);

                    /// Get the targets of the package
                    List<int> targets = new List<int>();
                    for (int i = 0; i < this.lastKnownLineStates.Length; i++)
                    {
                        if (i != this.lastKnownIdOfThisPeer && this.targetSelectors[i].Checked)
                        {
                            targets.Add(i);
                        }
                    }
                    if (targets.Count != 0)
                    {
                        /// Send a dedicated message.
                        sendIface.SendPackage(package, targets.ToArray());
                    }
                    else
                    {
                        /// Send the message to the whole lobby.
                        sendIface.SendPackage(package);
                    }
                }
            }
        }

        private void btnCreateLobby_Click(object sender, EventArgs e)
        {
            if (this.joinedLobby == null && this.createdLobby == null && !this.connectingToLobby)
            {
                this.createdLobby = this.network.CreateLobby(8, this);
                if (this.createdLobby != null)
                {
                    this.connectingToLobby = true;
                    this.createdLobby.StartAnnouncing();
                    UpdateControls();
                }
            }
        }

        private void btnJoinLobby_Click(object sender, EventArgs e)
        {
            if (this.joinedLobby == null && this.createdLobby == null && !this.connectingToLobby &&
                this.lstLobbies.SelectedItem != null)
            {
                this.joinedLobby = this.network.JoinLobby((LobbyInfo)this.lstLobbies.SelectedItem, this);
                if (this.joinedLobby != null)
                {
                    this.connectingToLobby = true;
                    UpdateControls();
                }
            }
        }

        private void btnShutdownDisconnectLobby_Click(object sender, EventArgs e)
        {
            if (this.joinedLobby != null && this.createdLobby == null && !this.connectingToLobby)
            {
                this.joinedLobby.Disconnect();
                this.connectingToLobby = false;
                this.joinedLobby = null;
                this.createdLobby = null;
                this.lastKnownLineStates = null;
                this.lastKnownIdOfThisPeer = -1;
                this.connectingToLobby = false;
                UpdateControls();
            }
            else if (this.joinedLobby == null && this.createdLobby != null && !this.connectingToLobby)
            {
                this.createdLobby.Shutdown();
                this.joinedLobby = null;
                this.createdLobby = null;
                this.lastKnownLineStates = null;
                this.lastKnownIdOfThisPeer = -1;
                this.connectingToLobby = false;
                UpdateControls();
            }
        }

        private void lstLobbies_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            int index = this.openButtonMap[(Button)sender];
            if (this.joinedLobby == null && this.createdLobby != null &&
                index < this.lastKnownLineStates.Length && index != this.lastKnownIdOfThisPeer && !this.connectingToLobby &&
                this.lastKnownLineStates[index] == LobbyLineState.Closed)
            {
                this.createdLobby.OpenLine(index);
                UpdateControls();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            int index = this.closeButtonMap[(Button)sender];
            if (this.joinedLobby == null && this.createdLobby != null &&
                index < this.lastKnownLineStates.Length && index != this.lastKnownIdOfThisPeer && !this.connectingToLobby &&
                (this.lastKnownLineStates[index] == LobbyLineState.Opened || this.lastKnownLineStates[index] == LobbyLineState.Engaged))
            {
                this.createdLobby.CloseLine(index);
                UpdateControls();
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            int index = this.testButtonMap[(Button)sender];
            if (this.joinedLobby == null && this.createdLobby != null &&
                index < this.lastKnownLineStates.Length && index != this.lastKnownIdOfThisPeer && !this.connectingToLobby &&
                this.lastKnownLineStates[index] == LobbyLineState.Engaged)
            {
                /// Create the package
                RCPackage package = RCPackage.CreateNetworkControlPackage(MY_FORMAT);
                package.WriteString(0, "INTERNAL MESSAGE: " + this.internalCount);
                this.createdLobby.SendControlPackage(package, index);
            }
            else if (this.joinedLobby != null && this.createdLobby == null &&
                     index == 0 && !this.connectingToLobby)
            {
                /// Create the package
                RCPackage package = RCPackage.CreateNetworkControlPackage(MY_FORMAT);
                package.WriteString(0, "INTERNAL MESSAGE: " + this.internalCount);
                this.joinedLobby.SendControlPackage(package);
            }
            this.internalCount++;
        }

        #endregion
    }
}
