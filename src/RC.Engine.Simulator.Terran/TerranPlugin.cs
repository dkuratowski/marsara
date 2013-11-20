using RC.Common.ComponentModel;
using RC.Engine.Simulator.ComponentInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RC.Engine.Simulator.Terran
{
    /// <summary>
    /// This class represents the simulator plugin for the Terran race.
    /// </summary>
    [Plugin(typeof(ISimulator))]
    class TerranPlugin : IPlugin<ISimulatorPluginInstall>
    {
        /// <summary>
        /// Constructs a TerranPlugin instance.
        /// </summary>
        public TerranPlugin()
        {
        }

        /// <see cref="IPlugin<T>.Install"/>
        public void Install(ISimulatorPluginInstall extendedComponent)
        {
            extendedComponent.RegisterHeapTypeContainer(this.GetType().Assembly);
        }

        /// <see cref="IPlugin<T>.Uninstall"/>
        public void Uninstall(ISimulatorPluginInstall extendedComponent)
        {
            /// TODO: Write uninstallation code here!
        }
    }
}
