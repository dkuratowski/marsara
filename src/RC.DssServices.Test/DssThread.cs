using System.Collections.Generic;
using RC.Common;
using RC.NetworkingSystem;
using System.Drawing;
using RC.RenderSystem;
using RC.Common.Diagnostics;

namespace RC.DssServices.Test
{
    /// <summary>
    /// This class is responsible to control the DSS-thread.
    /// </summary>
    abstract class DssThread: ISimulator
    {
        /// <summary>
        /// Constructs a DssThread object.
        /// </summary>
        public DssThread(ControlStatusManager ctrlStatusMgr, IUiInvoke ui, TestSimulator simulator, INetwork network)
        {
            this.ctrlStatusMgr = ctrlStatusMgr;
            this.ui = ui;
            this.uiCallMarshal = new UiCallMarshal(this.ctrlStatusMgr, this.ui);
            this.simulator = simulator;
            this.actionQueue = new UiActionQueue();
            this.currentChannelStates = null;
            this.network = network;
        }

        /// <summary>
        /// Starts the underlying DSS-thread. If the thread has been already started then this function has no effect.
        /// </summary>
        public void Start()
        {
            if (this.dssThread == null)
            {
                this.dssThread = new RCThread(this.DssThreadProc, "DssThread");
                this.dssThread.Start();
            }
        }
        
        #region ISimulator members

        public bool ExecuteNextFrame(out RCPackage[] outgoingCmds)
        {
            outgoingCmds = null;
            List<RCPackage> tmpOutgoingCmds = new List<RCPackage>();

            UiActionType[] uiActions = null;
            int[] firstParams = null;
            int[] secondParams = null;
            this.ActionQueue.GetAllActions(out uiActions, out firstParams, out secondParams);

            for (int i = 0; i < uiActions.Length; i++)
            {
                if (uiActions[i] == UiActionType.UpKeyPressed)
                {
                    RCPackage upCmd = RCPackage.CreateCustomDataPackage(TestClientMessages.COMMAND);
                    upCmd.WriteByte(0, (byte)PlayerDirection.Up);
                    tmpOutgoingCmds.Add(upCmd);
                }
                else if (uiActions[i] == UiActionType.DownKeyPressed)
                {
                    RCPackage downCmd = RCPackage.CreateCustomDataPackage(TestClientMessages.COMMAND);
                    downCmd.WriteByte(0, (byte)PlayerDirection.Down);
                    tmpOutgoingCmds.Add(downCmd);
                }
                else if (uiActions[i] == UiActionType.LeftKeyPressed)
                {
                    RCPackage leftCmd = RCPackage.CreateCustomDataPackage(TestClientMessages.COMMAND);
                    leftCmd.WriteByte(0, (byte)PlayerDirection.Left);
                    tmpOutgoingCmds.Add(leftCmd);
                }
                else if (uiActions[i] == UiActionType.RightKeyPressed)
                {
                    RCPackage rightCmd = RCPackage.CreateCustomDataPackage(TestClientMessages.COMMAND);
                    rightCmd.WriteByte(0, (byte)PlayerDirection.Right);
                    tmpOutgoingCmds.Add(rightCmd);
                }
                else if (uiActions[i] == UiActionType.LeaveBtnPressed)
                {
                    return false;
                }
            }

            if (tmpOutgoingCmds.Count > 0)
            {
                outgoingCmds = tmpOutgoingCmds.ToArray();
            }

            /// Step simulations and refresh the display
            this.simulator.MakeStep();
            Rectangle refreshedArea;
            Display.Instance.RenderOneFrame(out refreshedArea);
            this.uiCallMarshal.RefreshDisplay();

            return true;
        }

        public void GuestCommand(int guestIndex, RCPackage command)
        {
            if (command.PackageFormat.ID == TestClientMessages.COMMAND)
            {
                PlayerDirection dir = (PlayerDirection)command.ReadByte(0);
                this.simulator.GetPlayer(guestIndex + 1).Direction = dir;
            }
        }

        public void GuestLeftDssDuringSim(int guestIndex)
        {
            TraceManager.WriteAllTrace(string.Format("SIMULATOR_CALL: ISimulator.GuestLeftDssDuringSim(guestIndex = {0})", guestIndex), TestClientTraceFilters.TEST_INFO);
            this.simulator.GetPlayer(guestIndex + 1).Deactivate();
        }

        public void HostCommand(RCPackage command)
        {
            if (command.PackageFormat.ID == TestClientMessages.COMMAND)
            {
                PlayerDirection dir = (PlayerDirection)command.ReadByte(0);
                this.simulator.GetPlayer(0).Direction = dir;
            }
        }

        public void HostLeftDssDuringSim()
        {
            TraceManager.WriteAllTrace("SIMULATOR_CALL: ISimulator.HostLeftDssDuringSim()", TestClientTraceFilters.TEST_INFO);
            //this.simulator.GetPlayer(0).Deactivate();
        }

        public void SimulationError(string reason, byte[] customData)
        {
            TraceManager.WriteAllTrace(string.Format("SIMULATOR_CALL: ISimulator.SimulationError(reason = {0})", reason), TestClientTraceFilters.TEST_INFO);
        }

        public byte[] StateHash
        {
            get { return new byte[0] { }; }
        }

        #endregion

        /// <summary>
        /// Blocks the caller thread while the DSS-thread is not finished.
        /// </summary>
        public void Join()
        {
            if (this.dssThread != null)
            {
                this.dssThread.Join();
                this.dssThread = null;
            }
        }

        /// <summary>
        /// Gets the action-queue corresponding to this DSS-thread.
        /// </summary>
        public UiActionQueue ActionQueue { get { return this.actionQueue; } }

        /// <summary>
        /// The starting function of the DSS-thread. Must be implemented in the derived classes.
        /// </summary>
        protected abstract void DssThreadProc();

        /// <summary>
        /// Interface to the RC.NetworkingSystem.
        /// </summary>
        protected INetwork network;

        /// <summary>
        /// The current states of the channels.
        /// </summary>
        protected DssChannelState[] currentChannelStates;

        /// <summary>
        /// Reference to the ControlStatusManager object of the UI.
        /// </summary>
        protected ControlStatusManager ctrlStatusMgr;

        /// <summary>
        /// Reference to the invoke interface of the UI.
        /// </summary>
        protected IUiInvoke ui;

        /// <summary>
        /// Reference to the marshal interface of the UI.
        /// </summary>
        protected UiCallMarshal uiCallMarshal;

        /// <summary>
        /// Reference to the simulator.
        /// </summary>
        protected TestSimulator simulator;

        /// <summary>
        /// This action queue is used to post the UI actions to the DSS-thread for processing.
        /// </summary>
        private UiActionQueue actionQueue;

        /// <summary>
        /// The DSS-thread.
        /// </summary>
        private RCThread dssThread;

        /// <summary>
        /// Mutex object.
        /// </summary>
        //private object lockObject;

        /// <summary>
        /// This flag indicated to the DSS thread when to leave the DSS.
        /// </summary>
        //private bool dssLeave;
    }
}
