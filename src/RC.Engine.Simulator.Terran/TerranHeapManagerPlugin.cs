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
    /// This class represents the heap manager plugin for the Terran race.
    /// </summary>
    [Plugin(typeof(IHeapManager))]
    class TerranHeapManagerPlugin : IPlugin<IHeapManagerPluginInstall>
    {
        /// <summary>
        /// Constructs a TerranHeapManagerPlugin instance.
        /// </summary>
        public TerranHeapManagerPlugin()
        {
        }

        /// <see cref="IPlugin<T>.Install"/>
        public void Install(IHeapManagerPluginInstall extendedComponent)
        {
            extendedComponent.RegisterHeapTypeContainer(this.GetType().Assembly);
        }

        /// <see cref="IPlugin<T>.Uninstall"/>
        public void Uninstall(IHeapManagerPluginInstall extendedComponent)
        {
            /// TODO: Write uninstallation code here!
        }
    }
}
