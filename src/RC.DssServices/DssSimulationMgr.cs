using System;
using System.Collections.Generic;
using RC.Common;
using RC.Common.Diagnostics;

namespace RC.DssServices
{
    /// <summary>
    /// This class is responsible to manage every data that is needed for running the local simulation.
    /// </summary>
    class DssSimulationMgr : IDisposable
    {
        /// <summary>
        /// Constructs a DssSimulationMgr object.
        /// </summary>
        /// <param name="root">The root object.</param>
        public DssSimulationMgr(DssRoot root)
        {
            if (root == null) { throw new ArgumentNullException("root"); }

            this.initialized = false;
            this.hostLeft = false;

            this.root = root;
            this.aftCalculator = null;
            this.aptCalculators = null;

            this.currentRound = null;
            this.nextRound = null;
            this.nextNextRound = null;
            this.nextNextNextRound = null;
            this.waitingForCommit = false;

            this.commitTimeoutClock = null;
            this.simulationFrameClock = null;

            this.commitAwMonitors = null;
            this.commitAwTimeouts = null;

            this.operatorFlags = null;
        }

        /// <summary>
        /// Asks the manager to execute the next simulation frame at the given time.
        /// </summary>
        /// <param name="when">The time when you want to execute the next simulation frame.</param>
        public void SetNextFrameExecutionTime(int when)
        {
            if (!this.initialized) { throw new DssException("DssSimulationMgr is uninitialized!"); }
            if (when < 0) { throw new ArgumentOutOfRangeException("when"); }

            /// Cancel the current clock if necessary.
            if (this.simulationFrameClock != null)
            {
                this.simulationFrameClock.Cancel();
            }

            this.simulationFrameClock = this.root.AlarmClkMgr.SetAlarmClock(when, this.ExecuteNextFrame);
        }

        /// <summary>
        /// Call this function when an error occurs during the simulation stage.
        /// </summary>
        /// <param name="reason">The reason of the error.</param>
        /// <param name="customData">Custom data about the error.</param>
        public void SimulationStageError(string reason, byte[] customData)
        {
            if (!this.initialized) { throw new DssException("DssSimulationMgr is uninitialized!"); }

            /// If we are currently finishing the simulation --> do nothing
            if (this.hostLeft) { return; }

            /// Stop the alarm clocks
            StopCommitTimeoutClock();
            StopSimulationFrameClock();
            UnregisterAllCommitMonitors();

            /// Send the DSS_SIM_ERROR message to the others
            RCPackage errPackage = RCPackage.CreateNetworkCustomPackage(DssRoot.DSS_SIM_ERROR);
            errPackage.WriteString(0, reason);
            errPackage.WriteByteArray(1, customData);
            this.root.LobbyIface.SendPackage(errPackage);

            /// Quit from the DSS immediately
            this.root.SimulatorIface.SimulationError(reason, customData);
            this.root.EventQueue.ExitEventLoop();
        }

        /// <summary>
        /// Call this function when a DSS_SIM_ERROR message has been received from another operator.
        /// </summary>
        /// <param name="reason">The reason of the error.</param>
        /// <param name="customData">Custom data about the error.</param>
        public void SimulationStageErrorReceived(string reason, byte[] customData)
        {
            if (!this.initialized) { throw new DssException("DssSimulationMgr is uninitialized!"); }

            /// If we are currently finishing the simulation --> do nothing
            if (this.hostLeft) { return; }

            /// Stop the alarm clocks
            StopCommitTimeoutClock();
            StopSimulationFrameClock();
            UnregisterAllCommitMonitors();

            /// Quit from the DSS immediately
            this.root.SimulatorIface.SimulationError(reason, customData);
            this.root.EventQueue.ExitEventLoop();
        }

        /// <summary>
        /// Resets the state of the DssSimulationMgr. Call this function every time you start a simulation stage.
        /// </summary>
        /// <param name="opFlags">
        /// The current operator flag array. See the comment at this.operatorFlags for more information.
        /// </param>
        public void Reset(bool[] opFlags)
        {
            if (opFlags == null) { throw new ArgumentNullException("opFlags"); }
            if (opFlags.Length != this.root.OpCount) { throw new ArgumentException("Array length mismatch.", "opFlags"); }

            this.aftCalculator = new AverageCalculator(DssConstants.AVG_CALC_VECTOR_LENGTH,
                                                       DssConstants.INITIAL_AFT);
            this.aptCalculators = new AverageCalculator[this.root.OpCount];
            for (int i = 0; i < this.aptCalculators.Length; i++)
            {
                this.aptCalculators[i] = new AverageCalculator(DssConstants.AVG_CALC_VECTOR_LENGTH,
                                                               DssConstants.INITIAL_APT);
            }

            if (this.commitAwTimeouts != null)
            {
                foreach (KeyValuePair<AlarmClock, CommitMonitor> item in this.commitAwTimeouts)
                {
                    item.Key.Cancel();
                }
            }
            this.commitAwMonitors = new Dictionary<int, CommitMonitor>();
            this.commitAwTimeouts = new Dictionary<AlarmClock, CommitMonitor>();

            StopCommitTimeoutClock();
            StopSimulationFrameClock();

            this.operatorFlags = opFlags;

            this.currentRound = new SimulationRound(this.operatorFlags);
            this.currentRound.Reset(true, 0);
            this.currentRound.ComputeSpeedControl();
            this.nextRound = new SimulationRound(this.operatorFlags);
            this.nextRound.Reset(false, 1);
            this.nextNextRound = new SimulationRound(this.operatorFlags);
            this.nextNextRound.Reset(false, 2);
            this.nextNextNextRound = new SimulationRound(this.operatorFlags);
            this.nextNextNextRound.Reset(false, 3);
            this.waitingForCommit = false;

            this.initialized = true;
            this.hostLeft = false;

            int ticket = RandomService.DefaultGenerator.Next();
            byte[] stateHash = this.root.SimulatorIface.StateHash;
            int highestAPT = this.HighestAPT;

            this.commitTimeoutClock = this.root.AlarmClkMgr.SetAlarmClock(DssRoot.Time + DssConstants.COMMIT_TIMEOUT,
                                                                          this.CommitTimeout);

            /// Create the first commit package.
            RCPackage commitPackage = RCPackage.CreateNetworkCustomPackage(DssRoot.DSS_COMMIT);
            commitPackage.WriteShort(0, (short)this.aftCalculator.Average);
            commitPackage.WriteShort(1, (short)highestAPT);    /// The highest measured APT
            commitPackage.WriteInt(2, 1); /// Round index of the commit is 1 (next round is committed)
            commitPackage.WriteInt(3, ticket);  /// Commit answer ticket
            commitPackage.WriteByteArray(4, stateHash != null ? stateHash : new byte[0] { }); /// State-hash value

            /// Send the commit package to the lobby.
            this.root.LobbyIface.SendPackage(commitPackage);

            RegisterCommitMonitor(ticket);

            TraceManager.WriteAllTrace(string.Format("Self commit round {0}", this.nextRound.RoundIndex), DssTraceFilters.SIMULATION_INFO);
            if (!this.nextRound.Commit(this.root.IdOfThisPeer, this.aftCalculator.Average, highestAPT, stateHash))
            {
                SimulationStageError(string.Format("Commit of round-{0} failed!", this.nextRound.RoundIndex),
                                     new byte[0] { });
            }
        }

        /// <summary>
        /// Registers the given commands to the given frame of the given round from the given sender.
        /// </summary>
        /// <param name="cmds">List of the arrived commands.</param>
        /// <param name="roundIdx">The index of the round.</param>
        /// <param name="frameIdx">The index of the frame.</param>
        /// <param name="senderID">The ID of the sender.</param>
        /// <returns>True in case of success, false otherwise.</returns>
        public bool RegisterCommands(RCPackage[] cmds, int roundIdx, int frameIdx, int senderID)
        {
            if (!this.initialized) { throw new DssException("DssSimulationMgr is uninitialized!"); }

            /// If we are currently finishing the simulation --> do nothing
            if (this.hostLeft) { return true; }

            if (cmds == null || cmds.Length == 0) { throw new ArgumentNullException("cmds"); }
            if (senderID < 0 || senderID >= this.root.OpCount) { throw new ArgumentException("senderID"); }

            if (frameIdx < 0 || frameIdx >= DssConstants.MAX_FRAME_NUM) { return false; }

            TraceManager.WriteAllTrace(string.Format("COMMAND_ARRIVED(roundIdx: {0}; senderID: {1})", roundIdx, senderID), DssTraceFilters.SIMULATION_INFO);
            if (this.nextRound.RoundIndex == roundIdx)
            {
                return this.nextRound.Command(cmds, senderID, frameIdx);
            }
            else if (this.nextNextRound.RoundIndex == roundIdx)
            {
                return this.nextNextRound.Command(cmds, senderID, frameIdx);
            }
            else if (this.nextNextNextRound.RoundIndex == roundIdx)
            {
                return this.nextNextNextRound.Command(cmds, senderID, frameIdx);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Registers the given commit arrived from the given operator.
        /// </summary>
        /// <param name="commit">The arrived commit package.</param>
        /// <param name="senderID">The sender operator.</param>
        /// <returns>True in case of success, false otherwise.</returns>
        public bool RegisterCommit(RCPackage commit, int senderID)
        {
            if (!this.initialized) { throw new DssException("DssSimulationMgr is uninitialized!"); }

            /// If we are currently finishing the simulation --> do nothing
            if (this.hostLeft) { return true; }

            if (commit == null || !commit.IsCommitted) { throw new ArgumentException("commit"); }
            if (senderID == this.root.IdOfThisPeer) { throw new DssException("Unexpected commit sender!"); }
            if (senderID < 0 || senderID >= this.root.OpCount) { throw new ArgumentException("senderID"); }

            if (commit.PackageFormat.ID != DssRoot.DSS_COMMIT)
            {
                /// Package format error.
                return false;
            }

            short senderAFT = commit.ReadShort(0);
            short senderAPT = commit.ReadShort(1);
            int roundIdx = commit.ReadInt(2);
            int ticket = commit.ReadInt(3);
            byte[] stateHash = commit.ReadByteArray(4);

            if (senderAFT < 0) { return false; }
            if (senderAPT < 0) { return false; }
            if (stateHash == null) { return false; }

            bool success = false;
            if (roundIdx == this.nextRound.RoundIndex)
            {
                success = this.nextRound.Commit(senderID, senderAFT, senderAPT, stateHash);
                if (this.waitingForCommit && this.nextRound.IsCommitted)
                {
                    TraceManager.WriteAllTrace("WAITING FOR COMMIT END", DssTraceFilters.SIMULATION_INFO);
                    /// The missing commit has arrived, so we can continue the simulation
                    this.waitingForCommit = false;
                    /// Compute the scheduled time for the next frame
                    int nextFrameStartTime = this.currentRound.BaseTime
                                           + (this.currentRound.CurrentFrameIndex + 1) * this.currentRound.TargetFrameTime;
                    SendCommit();
                    SwapRounds();
                    /// Schedule the next frame.
                    SetNextFrameExecutionTime(Math.Max(nextFrameStartTime, DssRoot.Time));
                }
            }
            else if (roundIdx == this.nextNextRound.RoundIndex)
            {
                success = this.nextNextRound.Commit(senderID, senderAFT, senderAPT, stateHash);
            }
            else
            {
                TraceManager.WriteAllTrace(string.Format("RoundIdx mismatch! RoundIdx: {0}", roundIdx), DssTraceFilters.SIMULATION_ERROR);
            }

            if (success)
            {
                /// Create the answer for the commit and send it back to the sender.
                RCPackage commitAw = RCPackage.CreateNetworkCustomPackage(DssRoot.DSS_COMMIT_ANSWER);
                commitAw.WriteInt(0, ticket);
                this.root.LobbyIface.SendPackage(commitAw, new int[1] { senderID });
            }
            return success;
        }

        /// <summary>
        /// Registers the given DSS_LEAVE message arrived from the given operator.
        /// </summary>
        /// <param name="leaveMsg">The arrived DSS_LEAVE message.</param>
        /// <param name="senderID">The sender operator.</param>
        /// <returns>True in case of success, false otherwise.</returns>
        public bool RegisterLeaveMessage(RCPackage leaveMsg, int senderID)
        {
            if (!this.initialized) { throw new DssException("DssSimulationMgr is uninitialized!"); }

            /// If we are currently finishing the simulation --> do nothing
            if (this.hostLeft) { return true; }

            if (leaveMsg == null || !leaveMsg.IsCommitted) { throw new ArgumentException("leaveMsg"); }
            if (senderID == this.root.IdOfThisPeer) { throw new DssException("Unexpected DSS_LEAVE sender!"); }
            if (senderID < 0 || senderID >= this.root.OpCount) { throw new ArgumentException("senderID"); }

            if (leaveMsg.PackageFormat.ID != DssRoot.DSS_LEAVE)
            {
                /// Package format error.
                return false;
            }

            string reason = leaveMsg.ReadString(0);
            byte[] customData = leaveMsg.ReadByteArray(1);
            TraceManager.WriteAllTrace(string.Format("DSS_LEAVE: sender={0}, reason= {1}", senderID, reason), DssTraceFilters.SIMULATION_INFO);

            /// Set the corresponding operator flag to false.
            this.operatorFlags[senderID] = false;

            if (senderID > 0)
            {
                /// GUEST-leave
                /// Unregister all CommitMonitors that became unnecessary.
                List<int> commitAwTicketsToUnreg = new List<int>();
                foreach (KeyValuePair<int, CommitMonitor> monitor in this.commitAwMonitors)
                {
                    monitor.Value.Refresh();
                    if (monitor.Value.IsCommitAnswered)
                    {
                        commitAwTicketsToUnreg.Add(monitor.Value.Ticket);
                    }
                }
                foreach (int ticket in commitAwTicketsToUnreg)
                {
                    UnregisterCommitMonitor(ticket);
                }

                /// Try to register the leave of operator to the nextRound.
                if (!this.nextRound.Leave(senderID))
                {
                    /// If failed, try to register to the nextNextRound.
                    if (!this.nextNextRound.Leave(senderID))
                    {
                        /// If failed, try to register to the nextNextNextRound.
                        if (!this.nextNextNextRound.Leave(senderID))
                        {
                            /// Error: unable to register
                            TraceManager.WriteAllTrace(string.Format("Unable to register the leave of operator-{0}.", senderID), DssTraceFilters.SIMULATION_ERROR);
                            return false;
                        }
                    }
                }

                /// Now check whether the next round has become committed with this leave message.
                if (this.waitingForCommit && this.nextRound.IsCommitted)
                {
                    TraceManager.WriteAllTrace("WAITING FOR COMMIT END (guest left)", DssTraceFilters.SIMULATION_INFO);
                    /// The missing commit has arrived, so we can continue the simulation
                    this.waitingForCommit = false;
                    /// Compute the scheduled time for the next frame
                    int nextFrameStartTime = this.currentRound.BaseTime
                                           + (this.currentRound.CurrentFrameIndex + 1) * this.currentRound.TargetFrameTime;
                    SendCommit();
                    SwapRounds();
                    /// Schedule the next frame.
                    SetNextFrameExecutionTime(Math.Max(nextFrameStartTime, DssRoot.Time));
                }
            }
            else
            {
                /// HOST-leave
                /// Stop the timers
                UnregisterAllCommitMonitors();
                StopCommitTimeoutClock();

                if (this.waitingForCommit && this.nextRound.IsCommitted)
                {
                    /// Can continue with the next round

                    TraceManager.WriteAllTrace("WAITING FOR COMMIT END (host left)", DssTraceFilters.SIMULATION_INFO);
                    /// The missing commit has arrived, so we can continue the simulation
                    this.waitingForCommit = false;
                    /// Compute the scheduled time for the next frame
                    int nextFrameStartTime = this.currentRound.BaseTime
                                           + (this.currentRound.CurrentFrameIndex + 1) * this.currentRound.TargetFrameTime;
                    SendCommit();
                    SwapRounds();
                    /// Schedule the next frame.
                    SetNextFrameExecutionTime(Math.Max(nextFrameStartTime, DssRoot.Time));
                }
                else if (this.waitingForCommit && !this.nextRound.IsCommitted)
                {
                    /// Cannot continue with the next round --> notify the client module and exit the event loop
                    NotifyClientAboutLeavingGuests(this.nextRound);
                    NotifyClientAboutLeavingGuests(this.nextNextRound);
                    NotifyClientAboutLeavingGuests(this.nextNextNextRound);
                    this.root.SimulatorIface.HostLeftDssDuringSim();
                    this.root.EventQueue.ExitEventLoop();
                }
                this.hostLeft = true;
            }

            return true;
        }

        /// <summary>
        /// Registers the given commit answer arrived from the given operator.
        /// </summary>
        /// <param name="commit">The arrived commit answer package.</param>
        /// <param name="senderID">The sender operator.</param>
        /// <returns>True in case of success, false otherwise.</returns>
        public bool RegisterCommitAnswer(RCPackage commitAw, int senderID)
        {
            if (!this.initialized) { throw new DssException("DssSimulationMgr is uninitialized!"); }

            /// If we are currently finishing the simulation --> do nothing
            if (this.hostLeft) { return true; }

            if (commitAw == null || !commitAw.IsCommitted) { throw new ArgumentException("commitAw"); }
            if (senderID == this.root.IdOfThisPeer) { throw new DssException("Unexpected commit answer sender!"); }
            if (senderID < 0 || senderID >= this.root.OpCount) { throw new ArgumentException("senderID"); }

            if (commitAw.PackageFormat.ID != DssRoot.DSS_COMMIT_ANSWER)
            {
                /// Package format error.
                return false;
            }

            int ticket = commitAw.ReadInt(0);
            if (!this.commitAwMonitors.ContainsKey(ticket))
            {
                /// Unexpected ticket.
                return false;
            }

            CommitMonitor monitor = this.commitAwMonitors[ticket];
            int pingTime = -1;
            if (monitor.AnswerArrived(senderID, out pingTime))
            {
                this.aptCalculators[senderID].NewItem(pingTime);
                if (monitor.IsCommitAnswered)
                {
                    UnregisterCommitMonitor(ticket);
                }
                return true;
            }
            else
            {
                /// Ping already answered by the sender.
                return false;
            }
        }

        /// <summary>
        /// Called when a lobby line has gone to the Opened state during simulation stage.
        /// </summary>
        /// <param name="channelIdx">The index of that line.</param>
        public void SimulationStageLineOpened(int lineIdx)
        {
            if (this.operatorFlags[lineIdx])
            {
                SimulationStageError(string.Format("Channel-{0} disconnected!", lineIdx - 1), new byte[0] { });
            }
        }

        #region IDisposable members

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            if (this.initialized)
            {
                StopCommitTimeoutClock();
                StopSimulationFrameClock();
                UnregisterAllCommitMonitors();
            }
        }

        #endregion

        /// <summary>
        /// This function will be called by the AlarmClockManager when the next simulation frame has to be executed.
        /// </summary>
        private void ExecuteNextFrame(AlarmClock whichClock, object param)
        {
            if (!this.initialized) { throw new DssException("DssSimulationMgr is uninitialized!"); }
            if (this.simulationFrameClock == whichClock)
            {
                int frameStartTime = DssRoot.Time;
                TraceManager.WriteAllTrace(string.Format("Simulation frame execution: frameStartTime={0}", frameStartTime), DssTraceFilters.SIMULATION_INFO);

                /// Compute the scheduled time for the next frame
                int nextFrameStartTime = this.currentRound.BaseTime
                                       + (this.currentRound.CurrentFrameIndex + 1) * this.currentRound.TargetFrameTime;

                SendCommandsToSimulator();

                /// Call the ISimulator interface of the client module.
                RCPackage[] outgoingCmds = null;
                bool continueSim = this.root.SimulatorIface.ExecuteNextFrame(out outgoingCmds);

                if (continueSim)
                {
                    /// The client wants to continue the simulation.
                    if (!this.hostLeft && outgoingCmds != null && !RegisterCommands(outgoingCmds,
                                                                                    this.nextNextRound.RoundIndex,
                                                                                    this.currentRound.CurrentFrameIndex,
                                                                                    this.root.IdOfThisPeer))
                    {
                        throw new DssException("Unexpected command package format from the simulation interface!");
                    }

                    if (!this.hostLeft) { SendCommandsToLobby(outgoingCmds); }

                    if (this.currentRound.StepNextFrame())
                    {
                        /// Schedule the next frame.
                        SetNextFrameExecutionTime(Math.Max(nextFrameStartTime, DssRoot.Time));
                    }
                    else
                    {
                        /// No more frames in the current round
                        if (this.nextRound.IsCommitted)
                        {
                            /// Next round is committed --> continue
                            SendCommit();
                            SwapRounds();
                            /// Schedule the next frame.
                            SetNextFrameExecutionTime(Math.Max(nextFrameStartTime, DssRoot.Time));
                        }
                        else
                        {
                            /// Next round is not committed
                            if (!this.hostLeft)
                            {
                                /// We cannot continue the simulation because of a missing commit --> wait
                                this.waitingForCommit = true;
                                TraceManager.WriteAllTrace("WAITING FOR COMMIT", DssTraceFilters.SIMULATION_INFO);
                            }
                            else
                            {
                                /// Host left the DSS --> notify the client module and exit the event loop
                                NotifyClientAboutLeavingGuests(this.nextRound);
                                NotifyClientAboutLeavingGuests(this.nextNextRound);
                                NotifyClientAboutLeavingGuests(this.nextNextNextRound);
                                this.root.SimulatorIface.HostLeftDssDuringSim();
                                this.root.EventQueue.ExitEventLoop();
                            }
                        }
                    }
                }
                else
                {
                    SendLeaveMessage();
                    this.root.EventQueue.ExitEventLoop();
                }

                /// Store the duration of the current frame in the AFT calculator.
                int frameEndTime = DssRoot.Time;
                this.aftCalculator.NewItem(frameEndTime - frameStartTime);
            }
            else
            {
                throw new DssException("Illegal call to DssRoot.ExecuteNextFrame!");
            }
        }

        /// <summary>
        /// Internal function to timestamp and send the given commands to the lobby.
        /// </summary>
        /// <param name="cmds">The list of the commands to send.</param>
        private void SendCommandsToLobby(RCPackage[] cmds)
        {
            if (!this.initialized) { throw new DssException("DssSimulationMgr is uninitialized!"); }
            if (cmds != null && cmds.Length != 0)
            {
                /// Compute the length of the buffer needed for sending the commands as a byte sequence
                int bufferLength = 0;
                for (int i = 0; i < cmds.Length; i++)
                {
                    if (cmds[i] != null && cmds[i].IsCommitted && cmds[i].PackageType == RCPackageType.CUSTOM_DATA_PACKAGE &&
                        !DssRoot.IsInternalFormat(cmds[i].PackageFormat))
                    {
                        bufferLength += cmds[i].PackageLength;
                    }
                    else
                    {
                        throw new DssException("Unexpected command from the client module!");
                    }
                }

                /// Create the buffer and write the commands into this buffer
                byte[] cmdBuffer = new byte[bufferLength];
                int offset = 0;
                for (int i = 0; i < cmds.Length; i++)
                {
                    offset += cmds[i].WritePackageToBuffer(cmdBuffer, offset);
                }

                /// Create the DSS_COMMAND package and send it to the lobby
                RCPackage cmdPackage = RCPackage.CreateNetworkCustomPackage(DssRoot.DSS_COMMAND);
                cmdPackage.WriteInt(0, this.nextNextRound.RoundIndex); /// Round index of the command
                cmdPackage.WriteInt(1, this.currentRound.CurrentFrameIndex); /// Frame index of the command
                cmdPackage.WriteByteArray(2, cmdBuffer); /// The command list

                this.root.LobbyIface.SendPackage(cmdPackage); /// Send the command package to the lobby
            }
        }

        /// <summary>
        /// Internal function for send the commands of a frame to the simulator interface.
        /// </summary>
        private void SendCommandsToSimulator()
        {
            if (!this.initialized) { throw new DssException("DssSimulationMgr is uninitialized!"); }

            /// Send the commands to the simulator interface
            for (int op = 0; op < this.root.OpCount; op++)
            {
                RCPackage[] cmds = this.currentRound.GetCurrentFrameCommands(op);
                if (cmds != null)
                {
                    if (op == 0)
                    {
                        /// Commands from the host
                        for (int i = 0; i < cmds.Length; i++)
                        {
                            this.root.SimulatorIface.HostCommand(cmds[i]);
                        }
                    }
                    else
                    {
                        /// Commands from a guest
                        for (int i = 0; i < cmds.Length; i++)
                        {
                            this.root.SimulatorIface.GuestCommand(op - 1, cmds[i]);
                        }
                    }
                }
            } /// end-for (int op = 0; op < this.root.OpCount; op++)
        }

        /// <summary>
        /// Jumps to the first frame of the next round.
        /// </summary>
        /// <remarks>
        /// Call this function only if the next round has been committed, and the current round has been finished.
        /// </remarks>
        private void SwapRounds()
        {
            if (!this.initialized) { throw new DssException("DssSimulationMgr is uninitialized!"); }
            if (this.currentRound.StepNextFrame()) { throw new DssException("DssSimulationMgr.SwapRounds() denied!"); }
            if (!this.nextRound.IsCommitted) { throw new DssException("DssSimulationMgr.SwapRounds() denied!"); }

            StopCommitTimeoutClock();
            this.commitTimeoutClock = this.root.AlarmClkMgr.SetAlarmClock(DssRoot.Time + DssConstants.COMMIT_TIMEOUT,
                                                                          this.CommitTimeout);

            SimulationRound tmpRound = this.currentRound;
            this.currentRound = this.nextRound;
            this.currentRound.ComputeSpeedControl();
            this.nextRound = this.nextNextRound;
            this.nextNextRound = this.nextNextNextRound;
            this.nextNextNextRound = tmpRound;
            this.nextNextNextRound.Reset(false, this.nextNextRound.RoundIndex + 1);

            TraceManager.WriteAllTrace(string.Format("DssSimulationMgr.SwapRounds: {0}-{1}-{2}-{3}",
                                                     this.currentRound.RoundIndex,
                                                     this.nextRound.RoundIndex,
                                                     this.nextNextRound.RoundIndex,
                                                     this.nextNextNextRound.RoundIndex),
                                       DssTraceFilters.SIMULATION_INFO);

            /// Indicate the registered operator leave events to the ISimulator interface of the client module
            NotifyClientAboutLeavingGuests(this.currentRound);
        }

        /// <summary>
        /// Sends a commit for this.nextNextRound to the lobby.
        /// </summary>
        /// <remarks>
        /// Call this function only if this.currentRound has been finished.
        /// </remarks>
        private void SendCommit()
        {
            if (!this.initialized) { throw new DssException("DssSimulationMgr is uninitialized!"); }
            if (this.currentRound.StepNextFrame()) { throw new DssException("DssSimulationMgr.SendCommit() denied!"); }

            if (!this.hostLeft)
            {
                byte[] stateHash = this.root.SimulatorIface.StateHash;
                int highestAPT = this.HighestAPT;

                /// Generate a ticket for the commit.
                int ticket = -1;
                do
                {
                    ticket = RandomService.DefaultGenerator.Next();
                } while (this.commitAwMonitors.ContainsKey(ticket));

                /// Create the commit package.
                RCPackage commitPackage = RCPackage.CreateNetworkCustomPackage(DssRoot.DSS_COMMIT);
                commitPackage.WriteShort(0, (short)this.aftCalculator.Average);
                commitPackage.WriteShort(1, (short)highestAPT);    /// The highest measured APT
                commitPackage.WriteInt(2, this.nextNextRound.RoundIndex); /// Round index of the commit
                commitPackage.WriteInt(3, ticket);  /// Commit answer ticket
                commitPackage.WriteByteArray(4, stateHash != null ? stateHash : new byte[0] { }); /// State-hash value

                /// Send the commit package to the lobby.
                this.root.LobbyIface.SendPackage(commitPackage);

                RegisterCommitMonitor(ticket);

                TraceManager.WriteAllTrace(string.Format("Self commit round {0}", this.nextNextRound.RoundIndex), DssTraceFilters.SIMULATION_INFO);
                if (!this.nextNextRound.Commit(this.root.IdOfThisPeer, this.aftCalculator.Average, highestAPT, stateHash))
                {
                    SimulationStageError(string.Format("Commit of round-{0} failed!", this.nextNextRound.RoundIndex),
                                         new byte[0] { });
                }
            }
        }

        /// <summary>
        /// Sends a DSS_LEAVE message to the lobby and leaves the DSS.
        /// </summary>
        private void SendLeaveMessage()
        {
            if (!this.initialized) { throw new DssException("DssSimulationMgr is uninitialized!"); }
            StopCommitTimeoutClock();
            StopSimulationFrameClock();
            UnregisterAllCommitMonitors();

            if (!this.hostLeft)
            {
                RCPackage leaveMessage = RCPackage.CreateNetworkCustomPackage(DssRoot.DSS_LEAVE);
                leaveMessage.WriteString(0, "Leave DSS during simulation stage!");
                leaveMessage.WriteByteArray(1, new byte[0] { });
                this.root.LobbyIface.SendPackage(leaveMessage);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ofWhichRound"></param>
        private void NotifyClientAboutLeavingGuests(SimulationRound ofWhichRound)
        {
            if (!this.initialized) { throw new DssException("DssSimulationMgr is uninitialized!"); }

            int[] leavingGuests = ofWhichRound.LeavingGuests;
            for (int i = 0; i < leavingGuests.Length; i++)
            {
                this.root.SimulatorIface.GuestLeftDssDuringSim(leavingGuests[i] - 1);
            }
        }

        /// <summary>
        /// This function is called when the answers to a commit don't arrive in a given amount of time.
        /// </summary>
        private void CommitAnswerTimeout(AlarmClock whichClock, object param)
        {
            if (!this.initialized) { throw new DssException("DssSimulationMgr is uninitialized!"); }

            if (DssConstants.TIMEOUTS_NOT_IGNORED)
            {
                TraceManager.WriteAllTrace("COMMIT_ANSWER_TIMEOUT", DssTraceFilters.SIMULATION_ERROR);
                SimulationStageError("COMMIT_ANSWER_TIMEOUT", new byte[0] { });
            }
        }

        /// <summary>
        /// This function is called when the simulation manager waits too much time for a missing commit.
        /// </summary>
        private void CommitTimeout(AlarmClock whichClock, object param)
        {
            if (!this.initialized) { throw new DssException("DssSimulationMgr is uninitialized!"); }

            if (DssConstants.TIMEOUTS_NOT_IGNORED)
            {
                TraceManager.WriteAllTrace("COMMIT_TIMEOUT", DssTraceFilters.SIMULATION_ERROR);
                SimulationStageError("COMMIT_TIMEOUT", new byte[0] { });
            }
        }

        /// <summary>
        /// Registers a CommitMonitor object with the given ticket.
        /// </summary>
        /// <param name="ticket">The ticket of the commit you want to register.</param>
        private void RegisterCommitMonitor(int ticket)
        {
            if (!this.initialized) { throw new DssException("DssSimulationMgr is uninitialized!"); }
            if (this.commitAwMonitors.ContainsKey(ticket)) { throw new DssException("Ticket " + ticket + " already registered!"); }

            int idOfThisPeer = this.root.IdOfThisPeer;
            bool otherOpExists = false;
            for (int i = 0; i < this.operatorFlags.Length; i++)
            {
                if (this.operatorFlags[i] && i != idOfThisPeer)
                {
                    otherOpExists = true;
                    break;
                }
            }

            if (otherOpExists)
            {
                AlarmClock awTimeoutClock =
                    this.root.AlarmClkMgr.SetAlarmClock(DssRoot.Time + DssConstants.COMMIT_ANSWER_TIMEOUT,
                                                        this.CommitAnswerTimeout);
                CommitMonitor monitor = new CommitMonitor(ticket, awTimeoutClock, this.root.IdOfThisPeer, this.root.OpCount, this.operatorFlags);
                if (monitor.IsCommitAnswered)
                {
                    awTimeoutClock.Cancel();
                }
                else
                {
                    this.commitAwMonitors.Add(ticket, monitor);
                    this.commitAwTimeouts.Add(awTimeoutClock, monitor);
                }
            }
        }

        /// <summary>
        /// Unregisters the CommitMonitor object with the given ticket.
        /// </summary>
        /// <param name="ticket">The ticket of the commit you want to unregister.</param>
        private void UnregisterCommitMonitor(int ticket)
        {
            if (!this.initialized) { throw new DssException("DssSimulationMgr is uninitialized!"); }
            if (!this.commitAwMonitors.ContainsKey(ticket)) { throw new DssException("Ticket " + ticket + " is not registered!"); }

            CommitMonitor monitorToUnreg = this.commitAwMonitors[ticket];
            if (!this.commitAwTimeouts.ContainsKey(monitorToUnreg.CommitAwTimeoutClock))
            {
                throw new DssException("Timeout clock not found for monitor with ticket " + ticket + "!");
            }

            monitorToUnreg.CommitAwTimeoutClock.Cancel();

            this.commitAwMonitors.Remove(ticket);
            this.commitAwTimeouts.Remove(monitorToUnreg.CommitAwTimeoutClock);
        }

        /// <summary>
        /// Unregisters every registered CommitMonitor objects.
        /// </summary>
        private void UnregisterAllCommitMonitors()
        {
            if (!this.initialized) { throw new DssException("DssSimulationMgr is uninitialized!"); }

            /// Collect the registered tickets.
            int[] registeredTickets = new int[this.commitAwMonitors.Count];
            int i = 0;
            foreach (KeyValuePair<int, CommitMonitor> item in this.commitAwMonitors)
            {
                registeredTickets[i] = item.Key;
                i++;
            }

            /// Unregister the collected tickets.
            for (int j = 0; j < registeredTickets.Length; j++)
            {
                UnregisterCommitMonitor(registeredTickets[j]);
            }
        }

        /// <summary>
        /// Stops the commit timeout clock.
        /// </summary>
        private void StopCommitTimeoutClock()
        {
            if (this.commitTimeoutClock != null)
            {
                this.commitTimeoutClock.Cancel();
                this.commitTimeoutClock = null;
            }
        }

        /// <summary>
        /// Stops the simulaton frame clock.
        /// </summary>
        private void StopSimulationFrameClock()
        {
            if (this.simulationFrameClock != null)
            {
                this.simulationFrameClock.Cancel();
                this.simulationFrameClock = null;
            }
        }

        /// <summary>
        /// Searches the highest APT value from the local APT calculators.
        /// </summary>
        private int HighestAPT
        {
            get
            {
                int highestAPT = 0;
                for (int i = 0; i < this.aptCalculators.Length; i++)
                {
                    if (this.operatorFlags[i])
                    {
                        int averageAPT = (i != this.root.IdOfThisPeer) ? this.aptCalculators[i].Average : 0;
                        highestAPT = (averageAPT > highestAPT) ? averageAPT : highestAPT;
                    }
                }
                return highestAPT;
            }
        }

        /// <summary>
        /// Reference to the root object.
        /// </summary>
        private DssRoot root;

        /// <summary>
        /// This object calculates the local AFT values.
        /// </summary>
        private AverageCalculator aftCalculator;

        /// <summary>
        /// List of the objects that calculate the APT values of each operators.
        /// </summary>
        private AverageCalculator[] aptCalculators;

        /// <summary>
        /// Reference to the round that is currently under execution.
        /// </summary>
        private SimulationRound currentRound;

        /// <summary>
        /// Reference to the next round.
        /// </summary>
        private SimulationRound nextRound;

        /// <summary>
        /// Reference to the round after the next round.
        /// </summary>
        private SimulationRound nextNextRound;

        /// <summary>
        /// Reference to the round after the nextNext round.
        /// </summary>
        private SimulationRound nextNextNextRound;

        /// <summary>
        /// This flag indicates that we could not start this.nextRound because of a missing commit.
        /// </summary>
        private bool waitingForCommit;

        /// <summary>
        /// This clock measures the timeout between two commits during the simulation stage.
        /// </summary>
        private AlarmClock commitTimeoutClock;

        /// <summary>
        /// This clock is used to wait the necessary amount of time between simulation frames during the
        /// simulation stage.
        /// </summary>
        private AlarmClock simulationFrameClock;

        /// <summary>
        /// True if the simulation manager has been initialized, false otherwise.
        /// </summary>
        private bool initialized;

        /// <summary>
        /// Maps the commit tickets to the corresponding answer monitor object.
        /// </summary>
        private Dictionary<int, CommitMonitor> commitAwMonitors;

        /// <summary>
        /// Maps the commit answer timeout clocks to the corresponding answer monitor object.
        /// </summary>
        private Dictionary<AlarmClock, CommitMonitor> commitAwTimeouts;

        /// <summary>
        /// The Nth item in this array is true if operator-N is currently participating in the DSS, false if not.
        /// This array is needed to decide whether a simulation round has been committed by every participating
        /// operator or not.
        /// </summary>
        private bool[] operatorFlags;

        /// <summary>
        /// This flag is true if a DSS_LEAVE message has arrived from the host and the simulation has to
        /// be stopped after the last committed round. In this case any other network event will be ignored.
        /// </summary>
        private bool hostLeft;
    }
}
