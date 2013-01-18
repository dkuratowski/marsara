using System;
using System.Collections.Generic;
using RC.Common;
using System.Diagnostics;
using RC.Common.Diagnostics;

namespace RC.DssServices.TestConsole
{
    class HostImpl : IDssHostSetup, ISimulator
    {
        public HostImpl()
        {
            this.counter = 0;
            this.localTime = new Stopwatch();
            this.localTime.Start();
            this.timeOfLastFrame = 0;
            this.avg = new AverageCalculator(15, 20);
        }

        public bool GuestConnectionLost(int guestIndex)
        {
            TraceManager.WriteAllTrace(string.Format("SETUP_CALL: Connection lost with guest-{0}. Keep channel opened? (y/n)", guestIndex), TestConsoleTraceFilters.TEST_INFO);
            string input = Console.ReadLine();
            return input.CompareTo("y") == 0;
        }

        public bool GuestLeftDss(int guestIndex)
        {
            TraceManager.WriteAllTrace(string.Format("SETUP_CALL: Guest-{0} has left the DSS during setup. Keep channel opened? (y/n)", guestIndex), TestConsoleTraceFilters.TEST_INFO);
            string input = Console.ReadLine();
            return input.CompareTo("y") == 0;
        }

        public DssSetupResult ExecuteNextStep(IDssGuestChannel[] channelsToGuests)
        {
            TraceManager.WriteAllTrace("SETUP_CALL:", TestConsoleTraceFilters.TEST_INFO);
            List<RCPackage>[] requests = new List<RCPackage>[channelsToGuests.Length];
            for (int i = 0; i < requests.Length; i++)
            {
                requests[i] = new List<RCPackage>();
            }

            while (true)
            {
                for (int i = 0; i < channelsToGuests.Length; i++)
                {
                    if (channelsToGuests[i].ChannelState == DssChannelState.GUEST_CONNECTED)
                    {
                        TraceManager.WriteAllTrace(string.Format("SETUP_CALL: Channel-{0}: ENGAGED", i), TestConsoleTraceFilters.TEST_INFO);
                        RCPackage[] answer = channelsToGuests[i].AnswerFromGuest;
                        for (int j = 0; j < answer.Length; j++)
                        {
                            Console.WriteLine("   " + answer[j]);
                        }
                    }
                    else if (channelsToGuests[i].ChannelState == DssChannelState.CHANNEL_OPENED)
                    {
                        TraceManager.WriteAllTrace(string.Format("SETUP_CALL: Channel-{0}: OPENED", i), TestConsoleTraceFilters.TEST_INFO);
                    }
                    else if (channelsToGuests[i].ChannelState == DssChannelState.CHANNEL_CLOSED)
                    {
                        TraceManager.WriteAllTrace(string.Format("SETUP_CALL: Channel-{0}: CLOSED", i), TestConsoleTraceFilters.TEST_INFO);
                    }
                }

                string input = Console.ReadLine();
                string[] inputTokens = input.Split(new char[1] { ' ' });
                if (inputTokens.Length == 2)
                {
                    if (inputTokens[0].CompareTo("CloseChannel") == 0)
                    {
                        channelsToGuests[int.Parse(inputTokens[1])].CloseChannel();
                    }
                    else if (inputTokens[0].CompareTo("OpenChannel") == 0)
                    {
                        channelsToGuests[int.Parse(inputTokens[1])].OpenChannel();
                    }
                    else if (inputTokens[0].CompareTo("DropAndClose") == 0)
                    {
                        channelsToGuests[int.Parse(inputTokens[1])].DropGuest(false);
                    }
                    else if (inputTokens[0].CompareTo("DropAndOpen") == 0)
                    {
                        channelsToGuests[int.Parse(inputTokens[1])].DropGuest(true);
                    }
                    else
                    {
                        int idx = int.Parse(inputTokens[1]);
                        RCPackage package = RCPackage.CreateNetworkControlPackage(Program.MY_FORMAT);
                        package.WriteString(0, input);
                        requests[idx].Add(package);
                    }
                }
                else if (inputTokens.Length == 1)
                {
                    if (inputTokens[0].CompareTo("Continue") == 0)
                    {
                        for (int i = 0; i < channelsToGuests.Length; i++)
                        {
                            channelsToGuests[i].RequestToGuest = requests[i].ToArray();
                        }
                        return DssSetupResult.CONTINUE_SETUP;
                    }
                    else if (inputTokens[0].CompareTo("StartSim") == 0)
                    {
                        return DssSetupResult.START_SIMULATION;
                    }
                    else if (inputTokens[0].CompareTo("Leave") == 0)
                    {
                        return DssSetupResult.LEAVE_DSS;
                    }
                }
            }
        }

        public bool ExecuteNextFrame(out RCPackage[] outgoingCmds)
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
            RCThread.Sleep(20);
            return true;
        }

        public void GuestCommand(int guestIndex, RCPackage command)
        {
            TraceManager.WriteAllTrace(string.Format("SIMULATOR_CALL: Guest-{0} Command: {1}", guestIndex, command.ToString()), TestConsoleTraceFilters.TEST_INFO);
        }

        public void GuestLeftDssDuringSim(int guestIndex)
        {
            TraceManager.WriteAllTrace(string.Format("SIMULATOR_CALL: Guest-{0} has left the DSS during simulation. Channel will be closed.", guestIndex), TestConsoleTraceFilters.TEST_INFO);
        }

        public void HostCommand(RCPackage command)
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
