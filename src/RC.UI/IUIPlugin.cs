using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.UI
{
    /// <summary>
    /// The RC.UI component has several parts that can be customized by plugins. A plugin class must implement the
    /// IUIPlugin interface and must be a sealed class.
    /// </summary>
    public interface IUIPlugin
    {
        /// <summary>
        /// Installs this plugin. If the plugin has already been installed then this function has no effect.
        /// </summary>
        void Install();

        /// <summary>
        /// Uninstalls this plugin. If the plugin has not yet been installed then this function has no effect.
        /// </summary>
        void Uninstall();

        /// <summary>
        /// Gets the name of the plugin. This name must be unique among the loaded plugins.
        /// </summary>
        string Name { get; }
    }
}
