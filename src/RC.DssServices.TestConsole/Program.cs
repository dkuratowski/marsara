using System;
using System.Collections.Generic;
using RC.NetworkingSystem;
using RC.Common.Diagnostics;
using RC.Common.Configuration;

namespace RC.DssServices.TestConsole
{
    class Program
    {
        static Program()
        {
            ConfigurationManager.Initialize("../../../../config/RC.DssServices.TestConsole/RC.DssServices.TestConsole.root");
            MY_FORMAT = RCPackageFormatMap.Get("RC.DssServices.TestConsole.TestFormat");
        }

        static void TestFunc0(AdvancedTimer whichTimer)
        {
            Console.WriteLine("TESTFUNC0");
        }

        static void TestFunc1(AlarmClockThread whichTimer)
        {
            Console.WriteLine("TESTFUNC1");
        }

        static void Main(string[] args)
        {
            List<int> wellKnownPorts = new List<int>();
            wellKnownPorts.Add(25000);
            wellKnownPorts.Add(25001);
            wellKnownPorts.Add(25002);
            wellKnownPorts.Add(25003);
            wellKnownPorts.Add(25004);

            network = Network.CreateLocalAreaNetwork(wellKnownPorts);
            locator = new LobbyLocatorImpl();
            host = new HostImpl();
            guest = new GuestImpl();

            if (args != null && args.Length == 1)
            {
                if (args[0].CompareTo("client") == 0)
                {
                    Client();
                }
                else
                {
                    Server();
                }
            }
            else
            {
                Server();
            }
        }

        static void Server()
        {
            DssServiceAccess.CreateDSS(8, network, host, host);
        }

        static void Client()
        {
            /// Wait for the server to create a DSS-lobby
            network.StartLocatingLobbies(locator);
            LobbyInfo foundLobby = locator.WaitForFirstLobby();
            network.StopLocatingLobbies();

            DssServiceAccess.ConnectDSS(foundLobby, network, guest, guest);
        }

        static INetwork network;
        static LobbyLocatorImpl locator;
        static HostImpl host;
        static GuestImpl guest;

        public static readonly int MY_FORMAT;
    }

    static class TestConsoleTraceFilters
    {
        public static readonly int TEST_INFO = TraceManager.GetTraceFilterID("RC.DssServices.TestConsole.Info");
    }
}
