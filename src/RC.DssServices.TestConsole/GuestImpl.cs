using System;
using System.Collections.Generic;
using RC.Common;
using System.Diagnostics;
using RC.Common.Diagnostics;

namespace RC.DssServices.TestConsole
{
    class GuestImpl : IDssGuestSetup, ISimulator
    {
        public GuestImpl()
        {
            this.counter = 0;
            this.localTime = new Stopwatch();
            this.localTime.Start();
            this.timeOfLastFrame = 0;
            this.avg = new AverageCalculator(15, 35);
        }

        public void DroppedByHost()
        {
            TraceManager.WriteAllTrace("SETUP_CALL: Dropped out of the DSS by the host!", TestConsoleTraceFilters.TEST_INFO);
        }

        public void GuestConnectionLost(int guestIndex)
        {
            TraceManager.WriteAllTrace(string.Format("SETUP_CALL: Connection lost with guest-{0}.", guestIndex), TestConsoleTraceFilters.TEST_INFO);
        }

        public void GuestLeftDss(int guestIndex)
        {
            TraceManager.WriteAllTrace(string.Format("SETUP_CALL: Guest-{0} has left the DSS during setup.", guestIndex), TestConsoleTraceFilters.TEST_INFO);
        }

        public void HostConnectionLost()
        {
            TraceManager.WriteAllTrace("SETUP_CALL: Connection lost with the host!", TestConsoleTraceFilters.TEST_INFO);
        }

        public void HostLeftDss()
        {
            TraceManager.WriteAllTrace("SETUP_CALL: The host left the DSS!", TestConsoleTraceFilters.TEST_INFO);
        }

        public void SimulationStarted()
        {
            TraceManager.WriteAllTrace("SETUP_CALL: Simulation started by the host!", TestConsoleTraceFilters.TEST_INFO);
        }

        public bool ExecuteNextStep(IDssHostChannel channelToHost)
        {
            TraceManager.WriteAllTrace("SETUP_CALL:", TestConsoleTraceFilters.TEST_INFO);
            TraceManager.WriteAllTrace(string.Format("Guest index: {0}", channelToHost.GuestIndex), TestConsoleTraceFilters.TEST_INFO);
            TraceManager.WriteAllTrace("Request from host:", TestConsoleTraceFilters.TEST_INFO);
            RCPackage[] request = channelToHost.RequestFromHost;
            for (int i = 0; i < request.Length; i++)
            {
                TraceManager.WriteAllTrace(request[i], TestConsoleTraceFilters.TEST_INFO);
            }
            TraceManager.WriteAllTrace("Channel states:", TestConsoleTraceFilters.TEST_INFO);
            for (int i = 0; i < channelToHost.ChannelStates.Length; i++)
            {
                if (channelToHost.ChannelStates[i] == DssChannelState.GUEST_CONNECTED)
                {
                    TraceManager.WriteAllTrace(string.Format("Channel-{0}: ENGAGED", i), TestConsoleTraceFilters.TEST_INFO);
                }
                else if (channelToHost.ChannelStates[i] == DssChannelState.CHANNEL_OPENED)
                {
                    TraceManager.WriteAllTrace(string.Format("Channel-{0}: OPENED", i), TestConsoleTraceFilters.TEST_INFO);
                }
                else if (channelToHost.ChannelStates[i] == DssChannelState.CHANNEL_CLOSED)
                {
                    TraceManager.WriteAllTrace(string.Format("Channel-{0}: CLOSED", i), TestConsoleTraceFilters.TEST_INFO);
                }
            }

            List<RCPackage> answer = new List<RCPackage>();
            while (true)
            {
                string input = Console.ReadLine();
                if (input.CompareTo("Continue") == 0)
                {
                    channelToHost.AnswerToHost = answer.ToArray();
                    return true;
                }
                else if (input.CompareTo("Leave") == 0)
                {
                    return false;
                }
                else
                {
                    RCPackage package = RCPackage.CreateNetworkControlPackage(Program.MY_FORMAT);
                    package.WriteString(0, input);
                    answer.Add(package);
                }
            }
        }

        public bool ExecuteNextFrame(out RC.Common.RCPackage[] outgoingCmds)
        {
            TraceManager.WriteAllTrace("SIMULATOR_CALL:", TestConsoleTraceFilters.TEST_INFO);
            TraceManager.WriteAllTrace(string.Format("Counter = {0}", this.counter), TestConsoleTraceFilters.TEST_INFO);
            int currTime = (int)this.localTime.ElapsedMilliseconds;
            TraceManager.WriteAllTrace(string.Format("TimeSinceLastFrame = {0}", (currTime - this.timeOfLastFrame)), TestConsoleTraceFilters.TEST_INFO);
            this.avg.NewItem(currTime - this.timeOfLastFrame);
            TraceManager.WriteAllTrace(string.Format("AvgTimeBetweenFrames = {0}", this.avg.Average), TestConsoleTraceFilters.TEST_INFO);
            this.timeOfLastFrame = currTime;
            this.counter++;
            outgoingCmds = null;
            RCThread.Sleep(35);
            return true;
        }

        public void GuestCommand(int guestIndex, RC.Common.RCPackage command)
        {
            TraceManager.WriteAllTrace(string.Format("SIMULATOR_CALL: Guest-{0} Command: {1}", guestIndex, command.ToString()), TestConsoleTraceFilters.TEST_INFO);
        }

        public void GuestLeftDssDuringSim(int guestIndex)
        {
            TraceManager.WriteAllTrace(string.Format("SIMULATOR_CALL: Guest-{0} has left the DSS during simulation. Channel will be closed.", guestIndex), TestConsoleTraceFilters.TEST_INFO);
        }

        public void HostCommand(RC.Common.RCPackage command)
        {
            TraceManager.WriteAllTrace(string.Format("SIMULATOR_CALL: Host command: {0}", command.ToString()), TestConsoleTraceFilters.TEST_INFO);
        }

        public void HostLeftDssDuringSim()
        {
            TraceManager.WriteAllTrace("SIMULATOR_CALL: ISimulator.HostLeftDssDuringSim()", TestConsoleTraceFilters.TEST_INFO);
        }

        public void SimulationError(string reason, byte[] customData)
        {
            TraceManager.WriteAllTrace(string.Format("SIMULATOR_CALL: ISimulator.SimulationError(reason = {0})", reason), TestConsoleTraceFilters.TEST_INFO);
        }

        public byte[] StateHash
        {
            get
            {
                return BitConverter.GetBytes(this.counter);
            }
        }

        private int counter;
        private Stopwatch localTime;
        private int timeOfLastFrame;
        private AverageCalculator avg;
    }
}
