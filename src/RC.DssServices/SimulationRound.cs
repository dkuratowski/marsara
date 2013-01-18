using System;
using System.Collections.Generic;
using RC.Common;
using RC.Common.Diagnostics;

namespace RC.DssServices
{
    /// <summary>
    /// Represents a simulation round during the simulation stage.
    /// </summary>
    public class SimulationRound
    {
        /// <summary>
        /// Constructs a SimulationRound object.
        /// </summary>
        /// <param name="opCount">Number of the operators including the host.</param>
        public SimulationRound(/*int opCount, */bool[] opFlags)
        {
            //if (opFlags == null || opFlags.Length != opCount) { throw new ArgumentException("opFlags"); }
            if (opFlags == null || opFlags.Length < 1) { throw new ArgumentException("opFlags"); }

            this.initialized = false;
            this.speedControlComputed = false;
            this.baseTimeSet = false;

            this.commitFlags = new bool[opFlags.Length];
            this.operatorFlags = opFlags;
            this.currentFrameIndex = -1;
            this.roundIndex = -1;
            this.highestAFT = -1;
            this.highestAPT = -1;
            this.numberOfFrames = -1;
            this.targetFrameTime = -1;
            this.baseTime = -1;
            this.stateHash = null;
            this.leavingGuests = new SortedSet<int>();

            this.simulatorCommands = new RCPackage[DssConstants.MAX_FRAME_NUM][][];
            for (int frm = 0; frm < this.simulatorCommands.Length; frm++)
            {
                this.simulatorCommands[frm] = new RCPackage[opFlags.Length][];
                for (int op = 0; op < this.simulatorCommands[frm].Length; op++)
                {
                    this.simulatorCommands[frm][op] = null;
                }
            }
        }

        /// <summary>
        /// Initializes or resets this SimulationRound object.
        /// </summary>
        /// <param name="committed">True if the round has to be committed.</param>
        /// <param name="roundIdx">The index of this round.</param>
        public void Reset(bool committed, int roundIdx)
        {
            if (roundIdx < 0) { throw new ArgumentOutOfRangeException("roundIdx"); }

            /// Reset the commit flag array
            for (int i = 0; i < this.commitFlags.Length; ++i)
            {
                this.commitFlags[i] = committed;
            }

            /// Reset the simulator command array
            for (int frm = 0; frm < this.simulatorCommands.Length; frm++)
            {
                for (int op = 0; op < this.simulatorCommands[frm].Length; op++)
                {
                    this.simulatorCommands[frm][op] = null;
                }
            }

            this.roundIndex = roundIdx;
            this.currentFrameIndex = 0;
            this.highestAFT = 0;
            this.highestAPT = 0;
            this.numberOfFrames = 0;
            this.targetFrameTime = 0;
            this.baseTime = -1;
            this.stateHash = null;
            this.initialized = true;
            this.speedControlComputed = false;
            this.baseTimeSet = false;
            this.leavingGuests.Clear();
        }

        /// <summary>
        /// Handles an incoming commit from an operator.
        /// </summary>
        /// <param name="senderIdx">The index of the sender.</param>
        /// <param name="senderADT">The measured local AFT of the sender.</param>
        /// <param name="senderAPT">The highest measured APT of the sender.</param>
        /// <param name="stateHash">The state hash value of the sender.</param>
        /// <returns>True in case of success, or false if there is any problem.</returns>
        public bool Commit(int senderIdx, int senderAFT, int senderAPT, byte[] stateHash)
        {
            if (senderIdx < 0 || senderIdx >= this.commitFlags.Length) { throw new ArgumentOutOfRangeException("senderIdx"); }
            if (senderAFT < 0) { throw new ArgumentOutOfRangeException("senderAFT"); }
            if (senderAPT < 0) { throw new ArgumentOutOfRangeException("senderAPT"); }
            if (stateHash == null) { throw new ArgumentNullException("stateHash"); }

            if (this.initialized)
            {
                if (!this.commitFlags[senderIdx])
                {
                    TraceManager.WriteAllTrace(string.Format("COMMIT_ARRIVED(roundIdx: {0}; senderID: {1}; AFT: {2}; APT: {3})", this.roundIndex, senderIdx, senderAFT, senderAPT), DssTraceFilters.SIMULATION_INFO);

                    /// Commit the round and save the APT and AFT values.
                    this.commitFlags[senderIdx] = true;
                    this.highestAFT = (senderAFT > this.highestAFT) ? (senderAFT) : (this.highestAFT);
                    this.highestAPT = (senderAPT > this.highestAPT) ? (senderAPT) : (this.highestAPT);

                    if (this.stateHash == null)
                    {
                        this.stateHash = stateHash;
                        return true;
                    }
                    else
                    {
                        return CompareStateHashes(stateHash, this.stateHash);
                    }
                }
                else
                {
                    /// Round has been already committed by this sender.
                    return false;
                }
            }
            else
            {
                throw new DssException("Uninitialized SimulationRound!");
            }
        }

        /// <summary>
        /// Handles an incoming leave event from a guest.
        /// </summary>
        /// <param name="senderIdx">The index of the guest.</param>
        /// <returns>
        /// True if the leave event has successfully registered to this simulation round or false if the sender
        /// has been already committed this round.
        /// </returns>
        public bool Leave(int senderIdx)
        {
            /// Check whether the sender is not the host.
            if (senderIdx < 1 || senderIdx >= this.commitFlags.Length) { throw new ArgumentOutOfRangeException("senderIdx"); }

            if (this.initialized)
            {
                if (!this.commitFlags[senderIdx])
                {
                    TraceManager.WriteAllTrace(string.Format("LEAVE_ARRIVED(roundIdx: {0}; senderID: {1})", this.roundIndex, senderIdx), DssTraceFilters.SIMULATION_INFO);

                    /// Commit the round and register the sender into the this.leavingGuests list.
                    /// NOTE: the operatorFlags array is managed by the DssSimulationMgr
                    this.commitFlags[senderIdx] = true;
                    if (!this.leavingGuests.Contains(senderIdx)) { this.leavingGuests.Add(senderIdx); }
                    return true;
                }
                else
                {
                    /// Round has been already committed by this sender.
                    return false;
                }
            }
            else
            {
                throw new DssException("Uninitialized SimulationRound!");
            }
        }

        /// <summary>
        /// This function computes the target frame time and number of frames in this simulation round.
        /// </summary>
        /// <remarks>
        /// You have to call this function only if the round is committed otherwise you get an exception.
        /// </remarks>
        public void ComputeSpeedControl()
        {
            if (this.initialized)
            {
                if (this.IsCommitted)
                {
                    if (!this.speedControlComputed)
                    {
                        /// Compute the target frame time and number of frames for this simulation round.
                        this.targetFrameTime = Math.Min(Math.Max(this.highestAFT,
                                                                 DssConstants.MIN_TARGET_FRAME_TIME),
                                                        DssConstants.MAX_TARGET_FRAME_TIME);
                        this.numberOfFrames = Math.Min(Math.Max((this.highestAPT / this.targetFrameTime) + 1,
                                                                DssConstants.MIN_FRAME_NUM),
                                                       DssConstants.MAX_FRAME_NUM);
                        TraceManager.WriteAllTrace(string.Format("ROUND-{0}(TFT:{1}; NOF:{2})", this.roundIndex, this.targetFrameTime, this.numberOfFrames), DssTraceFilters.SIMULATION_INFO);
                        this.speedControlComputed = true;
                    }
                }
                else
                {
                    throw new DssException("Uncommitted SimulationRound!");
                }
            }
            else
            {
                throw new DssException("Uninitialized SimulationRound!");
            }
        }

        /// <summary>
        /// Call this function if a list of commands has been arrived from the given operator in the given frame.
        /// </summary>
        /// <param name="theCommands">List of the arrived commands.</param>
        /// <param name="opIdx">The index of the sender operator.</param>
        /// <param name="frameIdx">The index of the frame of the commands.</param>
        /// <returns>True in case of success, false otherwise.</returns>
        public bool Command(RCPackage[] theCommands, int opIdx, int frameIdx)
        {
            if (theCommands == null || theCommands.Length == 0) { throw new ArgumentNullException("theCommands"); }
            if (opIdx < 0 || opIdx >= this.commitFlags.Length) { throw new ArgumentOutOfRangeException("opIdx"); }
            if (frameIdx < 0 || frameIdx >= this.simulatorCommands.Length) { throw new ArgumentOutOfRangeException("frameNum"); }

            for (int i = 0; i < theCommands.Length; i++)
            {
                if (theCommands[i] == null || !theCommands[i].IsCommitted ||
                    theCommands[i].PackageType != RCPackageType.CUSTOM_DATA_PACKAGE ||
                    DssRoot.IsInternalFormat(theCommands[i].PackageFormat))
                {
                    return false;
                }
            }

            if (this.initialized)
            {
                if (!this.commitFlags[opIdx])
                {
                    if (this.simulatorCommands[frameIdx][opIdx] == null)
                    {
                        this.simulatorCommands[frameIdx][opIdx] = theCommands;
                        return true;
                    }
                    else
                    {
                        /// Command list has been already sent by the given operator for the given frame.
                        return false;
                    }
                }
                else
                {
                    /// The round has been already committed by the sender operator --> error.
                    return false;
                }
            }
            else
            {
                throw new DssException("Uninitialized SimulationRound!");
            }
        }

        /// <summary>
        /// Gets the list of commands sent by the given operator in the current frame.
        /// </summary>
        /// <param name="opIdx">The operator whose commands you want to get.</param>
        /// <returns>The list of the commands or null if the operator has no command in the current frame.</returns>
        /// <remarks>You have to call SimulationRound.StepNextFrame() to step to the next frame.</remarks>
        public RCPackage[] GetCurrentFrameCommands(int opIdx)
        {
            if (opIdx < 0 || opIdx >= this.commitFlags.Length) { throw new ArgumentOutOfRangeException("opIdx"); }

            if (this.initialized && this.IsCommitted && this.speedControlComputed)
            {
                if (this.operatorFlags[opIdx])
                {
                    /// The given operator is currently in the simulation.
                    if (this.currentFrameIndex < this.numberOfFrames - 1)
                    {
                        /// If the current is not the last frame then we simply make a copy of the command list.
                        RCPackage[] retList = (this.simulatorCommands[this.currentFrameIndex][opIdx] != null)
                                            ? new RCPackage[this.simulatorCommands[this.currentFrameIndex][opIdx].Length]
                                            : null;
                        if (retList != null)
                        {
                            for (int i = 0; i < retList.Length; i++)
                            {
                                retList[i] = this.simulatorCommands[this.currentFrameIndex][opIdx][i];
                            }
                        }

                        return retList;
                    }
                    else
                    {
                        /// If the current is the last frame then we have to concatenate the remaining command lists.
                        /// First we compute the length of the result list.
                        int retListLength = 0;
                        for (int frm = this.currentFrameIndex; frm < this.simulatorCommands.Length; frm++)
                        {
                            retListLength += (this.simulatorCommands[frm][opIdx] != null)
                                           ? (this.simulatorCommands[frm][opIdx].Length)
                                           : (0);
                        }

                        /// Then we fill the result list.
                        RCPackage[] retList = (retListLength > 0) ? (new RCPackage[retListLength]) : (null);
                        if (retList != null)
                        {
                            int i = 0;
                            for (int frm = this.currentFrameIndex; frm < this.simulatorCommands.Length; frm++)
                            {
                                if (this.simulatorCommands[frm][opIdx] != null)
                                {
                                    for (int j = 0; j < this.simulatorCommands[frm][opIdx].Length; j++)
                                    {
                                        retList[i] = this.simulatorCommands[frm][opIdx][j];
                                        i++;
                                    }
                                }
                            }
                        }

                        return retList;
                    }
                }
                else
                {
                    /// The given operator is out of the simulation.
                    return null;
                }
            }
            else
            {
                throw new DssException("SimulationRound access denied!");
            }
        }

        /// <summary>
        /// Steps to the next frame.
        /// </summary>
        /// <returns>False if the current is the last frame, true otherwise.</returns>
        public bool StepNextFrame()
        {
            if (this.initialized && this.IsCommitted && this.speedControlComputed)
            {
                if (this.currentFrameIndex < this.numberOfFrames - 1)
                {
                    /// The current frame is not the last frame in this round.
                    this.currentFrameIndex++;
                    return true;
                }
                else
                {
                    /// The current frame is the last frame in this round.
                    return false;
                }
            }
            else
            {
                throw new DssException("SimulationRound access denied!");
            }
        }

        /// <summary>
        /// Gets whether this round is committed by every operators or not.
        /// </summary>
        public bool IsCommitted
        {
            get
            {
                if (!this.initialized) { throw new DssException("Uninitialized SimulationRound!"); }
                if (this.operatorFlags == null) { throw new DssException("this.operatorFlags not set!"); }

                for (int i = 0; i < this.commitFlags.Length; ++i)
                {
                    if (this.operatorFlags[i] && !this.commitFlags[i])
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Gets the index of this round.
        /// </summary>
        public int RoundIndex
        {
            get
            {
                if (!this.initialized) { throw new DssException("Uninitialized SimulationRound!"); }
                return this.roundIndex;
            }
        }

        /// <summary>
        /// Gets the index of the current frame in this round.
        /// </summary>
        public int CurrentFrameIndex
        {
            get
            {
                if (!this.initialized && !this.IsCommitted) { throw new DssException("Uninitialized SimulationRound!"); }
                return this.currentFrameIndex;
            }
        }

        /// <summary>
        /// Gets the TFT value computed for this simulation round.
        /// </summary>
        public int TargetFrameTime
        {
            get
            {
                if (!this.initialized && !this.IsCommitted) { throw new DssException("Uninitialized SimulationRound!"); }
                if (!this.speedControlComputed) { throw new DssException("Speed control information not yet available!"); }
                return this.targetFrameTime;
            }
        }

        /// <summary>
        /// Gets the NOF value computed for this simulation round.
        /// </summary>
        public int NumberOfFrames
        {
            get
            {
                if (!this.initialized && !this.IsCommitted) { throw new DssException("Uninitialized SimulationRound!"); }
                if (!this.speedControlComputed) { throw new DssException("Speed control information not yet available!"); }
                return this.numberOfFrames;
            }
        }

        /// <summary>
        /// Gets the beginning time of the first frame in this simulation round.
        /// </summary>
        /// <remarks>Should be queried at the beginning of the current frame.</remarks>
        public int BaseTime
        {
            get
            {
                if (!this.initialized && !this.IsCommitted) { throw new DssException("Uninitialized SimulationRound!"); }

                if (!this.baseTimeSet)
                {
                    this.baseTime = DssRoot.Time;
                    this.baseTimeSet = true;
                }

                return this.baseTime;
            }
        }

        /// <summary>
        /// Gets the leaving guests of this round in increasing order.
        /// </summary>
        public int[] LeavingGuests
        {
            get
            {
                int[] retList = new int[this.leavingGuests.Count];
                int i = 0;
                foreach (int lg in this.leavingGuests)
                {
                    retList[i] = lg;
                }
                return retList;
            }
        }

        /// <summary>
        /// Checks whether the given state hashes are equal.
        /// </summary>
        /// <param name="stateHash0">The first state hash to compare.</param>
        /// <param name="stateHash1">The second state hash to compare.</param>
        /// <returns>True if the state hashes are equal, false otherwise.</returns>
        private bool CompareStateHashes(byte[] stateHash0, byte[] stateHash1)
        {
            if (stateHash0 == null) { throw new ArgumentNullException("stateHash0"); }
            if (stateHash1 == null) { throw new ArgumentNullException("stateHash1"); }

            if (stateHash0.Length == stateHash1.Length)
            {
                /// Matching length --> check the bytes.
                for (int i = 0; i < stateHash0.Length; i++)
                {
                    if (stateHash0[i] != stateHash1[i])
                    {
                        /// Byte mismatch at position i.
                        return false;
                    }
                }

                /// The hash values are equal.
                return true;
            }
            else
            {
                /// Length mismatch.
                return false;
            }
        }

        /// <summary>
        /// This 3D array contains the simulator commands of each operator for every frame in this round.
        /// </summary>
        /// <remarks>
        /// For example: simulatorCommands[k][i][] is the array of the commands arrived from operator-i that have
        ///              to be executed in frame-k of this round. If i == 0 then this operator is the host, otherwise
        ///              this operator is the guest with index (i-1).
        /// </remarks>
        private RCPackage[][][] simulatorCommands;

        /// <summary>
        /// The index of this round.
        /// </summary>
        private int roundIndex;

        /// <summary>
        /// The index of the currently executed frame in this round.
        /// </summary>
        private int currentFrameIndex;

        /// <summary>
        /// These flags show whether the commits from the corresponding operators have been arrived or not.
        /// </summary>
        private bool[] commitFlags;

        /// <summary>
        /// These flags indicate which operators are participating in the simulation and which aren't.
        /// See DssSimulationMgr.operatorFlags for more informations.
        /// </summary>
        private bool[] operatorFlags;

        /// <summary>
        /// The common state hash value of this round. This array is used to validate the synch between the operators.
        /// </summary>
        private byte[] stateHash;

        /// <summary>
        /// List of the guests that are leaving at the beginning of this round.
        /// </summary>
        private SortedSet<int> leavingGuests;

        /// <summary>
        /// The highest AFT value that we have get from the commits. 
        /// </summary>
        private int highestAFT;

        /// <summary>
        /// The highest APT value that we have get from the commits. 
        /// </summary>
        private int highestAPT;

        /// <summary>
        /// The computed target frame time in this simulation round.
        /// </summary>
        private int targetFrameTime;

        /// <summary>
        /// The number of frames in this simulation round.
        /// </summary>
        private int numberOfFrames;

        /// <summary>
        /// The beginning time of the first frame in this simulation round.
        /// </summary>
        private int baseTime;

        /// <summary>
        /// This flag indicates whether this simulation round has been initialized or not.
        /// </summary>
        private bool initialized;

        /// <summary>
        /// This flag is true if the target frame time and number of frames have been computed for this simulation round.
        /// </summary>
        private bool speedControlComputed;

        /// <summary>
        /// This flag is true if the beginning time of the first frame has been saved for this simulation round.
        /// </summary>
        private bool baseTimeSet;
    }
}
