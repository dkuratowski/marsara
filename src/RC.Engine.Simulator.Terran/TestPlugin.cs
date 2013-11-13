using RC.Common.ComponentModel;
using RC.Engine.Simulator.ComponentInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Engine.Simulator.Terran
{
    [Plugin(typeof(ISimulator))]
    class TestPlugin : IPlugin<ISimulatorPluginInstall>
    {
        public TestPlugin()
        {
        }

        public void Install(ISimulatorPluginInstall extendedComponent)
        {
        }

        public void Uninstall(ISimulatorPluginInstall extendedComponent)
        {
        }
    }
}
