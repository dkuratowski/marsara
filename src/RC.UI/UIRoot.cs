using System;
using System.Reflection;
using System.Collections.Generic;
using RC.Common.Diagnostics;

namespace RC.UI
{
    /// <summary>
    /// This is the main access point of the RC.UI component. UIRoot is a singleton class, and it's instance
    /// can be globally accessible with the UIRoot.Instance property.
    /// </summary>
    public sealed class UIRoot : IDisposable
    {
        /// <summary>
        /// Gets the singleton instance of the UIRoot class.
        /// </summary>
        public static UIRoot Instance
        {
            get
            {
                if (theInstance == null) { throw new UIException("No instance of UIRoot exists!"); }
                return theInstance;
            }
        }

        /// <summary>
        /// Constructs the singleton instance of the UIRoot class.
        /// </summary>
        public UIRoot()
        {
            if (theInstance != null) { throw new UIException("An instance of UIRoot already exists!"); }
            this.loadedPlugins = new Dictionary<string, IUIPlugin>();
            this.graphicsPlatform = null;
            this.mouseAccess = null;
            this.keyboardAccess = null;
            this.objectDisposed = false;

            theInstance = this;
            TraceManager.WriteAllTrace("UIRoot.Instance created", UITraceFilters.INFO);
        }

        #region Public methods and properties

        /// <summary>
        /// Gets the currently registered graphics platform or null if there is no registered platform.
        /// </summary>
        public IUIGraphicsPlatform GraphicsPlatform
        {
            get
            {
                if (this.objectDisposed) { throw new ObjectDisposedException("UIRoot"); }
                return this.graphicsPlatform;
            }
        }

        /// <summary>
        /// Gets an interface for accessing the current state of the mouse input device.
        /// </summary>
        public IUIMouseAccess MouseAccess
        {
            get
            {
                if (this.objectDisposed) { throw new ObjectDisposedException("UIRoot"); }
                return this.mouseAccess;
            }
        }

        /// <summary>
        /// Gets an interface for accessing the current state of the keyboard input device.
        /// </summary>
        public IUIKeyboardAccess KeyboardAccess
        {
            get
            {
                if (this.objectDisposed) { throw new ObjectDisposedException("UIRoot"); }
                return this.keyboardAccess;
            }
        }

        #endregion Public methods and properties

        #region Plugin management

        /// <summary>
        /// Loads all the plugins from the given assembly. This method automatically searches the classes in the given assembly
        /// that implement the IUIPlugin interface, creates an instance each of them and registers them for installation. See
        /// the description of the IUIPlugin interface for more informations.
        /// </summary>
        /// <param name="fromAsm">The assembly to load the plugins from.</param>
        /// <returns>True in case of success, false otherwise.</returns>
        public bool LoadPlugins(Assembly fromAsm)
        {
            if (this.objectDisposed) { throw new ObjectDisposedException("UIRoot"); }
            if (fromAsm == null) { throw new ArgumentNullException("fromAsm"); }

            try
            {
                Type[] types = fromAsm.GetTypes();
                foreach (Type type in types)
                {
                    Type ifaceType = type.GetInterface("RC.UI.IUIPlugin");
                    if (ifaceType == typeof(IUIPlugin) && type.IsSealed)
                    {
                        IUIPlugin foundPlugin = fromAsm.CreateInstance(type.FullName) as IUIPlugin;
                        string name = foundPlugin.Name;
                        if (name != null && !this.loadedPlugins.ContainsKey(name))
                        {
                            this.loadedPlugins.Add(name, foundPlugin);
                            TraceManager.WriteAllTrace(string.Format("Plugin \"{0}\" has been loaded.", name), UITraceFilters.INFO);
                        }
                        else
                        {
                            TraceManager.WriteAllTrace(string.Format("Loading plugin \"{0}\" failed.", name != null ? name : "NULL"), UITraceFilters.ERROR);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                TraceManager.WriteExceptionAllTrace(ex, false);
                return false;
            }
        }

        /// <summary>
        /// Gets the loaded plugin with the given name.
        /// </summary>
        /// <param name="name">The name of the plugin to get.</param>
        /// <returns>The loaded plugin with the given name or a null reference if no plugin has been loaded with this name.</returns>
        public IUIPlugin GetPlugin(string name)
        {
            if (this.objectDisposed) { throw new ObjectDisposedException("UIRoot"); }
            if (name == null) { throw new ArgumentNullException("name"); }

            if (this.loadedPlugins.ContainsKey("name"))
            {
                return this.loadedPlugins[name];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Installs all the loaded plugins.
        /// </summary>
        public void InstallPlugins()
        {
            if (this.objectDisposed) { throw new ObjectDisposedException("UIRoot"); }
            foreach (KeyValuePair<string, IUIPlugin> item in this.loadedPlugins)
            {
                TraceManager.WriteAllTrace(string.Format("Installing plugin {0}", item.Value.Name), UITraceFilters.INFO);
                item.Value.Install();
            }
        }

        /// <summary>
        /// Uninstalls all the loaded plugins.
        /// </summary>
        public void UninstallPlugins()
        {
            if (this.objectDisposed) { throw new ObjectDisposedException("UIRoot"); }
            foreach (KeyValuePair<string, IUIPlugin> item in this.loadedPlugins)
            {
                TraceManager.WriteAllTrace(string.Format("Uninstalling plugin {0}", item.Value.Name), UITraceFilters.INFO);
                item.Value.Uninstall();
            }
        }

        /// <summary>
        /// Uninstalls and unloads every loaded plugins.
        /// </summary>
        public void UnloadAllPlugins()
        {
            if (this.objectDisposed) { throw new ObjectDisposedException("UIRoot"); }
            foreach (KeyValuePair<string, IUIPlugin> item in this.loadedPlugins)
            {
                TraceManager.WriteAllTrace(string.Format("Uninstalling plugin {0}", item.Value.Name), UITraceFilters.INFO);
                item.Value.Uninstall();
            }
            this.loadedPlugins.Clear();
        }

        #endregion Plugin management

        #region Methods for plugins

        /// <summary>
        /// Registers a graphics platform to the UIRoot object.
        /// </summary>
        /// <param name="platform">The graphics platform to register.</param>
        /// <exception cref="UIException">If a graphics platform has already been registered.</exception>
        /// <remarks>This method should be called by the plugin that implements a graphics platform.</remarks>
        public void RegisterGraphicsPlatform(UIGraphicsPlatformBase platform)
        {
            if (this.objectDisposed) { throw new ObjectDisposedException("UIRoot"); }
            if (platform == null) { throw new ArgumentNullException("platform"); }
            if (this.graphicsPlatform != null) { throw new UIException("Graphics platform already registered!"); }

            this.graphicsPlatform = platform;
            TraceManager.WriteAllTrace("Graphics platform registered", UITraceFilters.INFO);
        }

        /// <summary>
        /// Unregisters the currently active graphics platform from the UIRoot object.
        /// </summary>
        /// <exception cref="UIException">If there is no registered graphics platform.</exception>
        /// <remarks>This method should be called by the plugin that implements the graphics platform.</remarks>
        public void UnregisterGraphicsPlatform()
        {
            if (this.objectDisposed) { throw new ObjectDisposedException("UIRoot"); }
            if (this.graphicsPlatform == null) { throw new UIException("There is no registered graphics platform!"); }

            this.graphicsPlatform = null;
            TraceManager.WriteAllTrace("Graphics platform unregistered", UITraceFilters.INFO);
        }

        /// <summary>
        /// Registers a mouse input device to the UIRoot object.
        /// </summary>
        /// <param name="mouseAccess">The mouse input device to register.</param>
        /// <exception cref="UIException">If a mouse input device has already been registered.</exception>
        /// <remarks>This method should be called by the plugin that implements the mouse input device.</remarks>
        public void RegisterMouseInputDevice(IUIMouseAccess mouseAccess)
        {
            if (this.objectDisposed) { throw new ObjectDisposedException("UIRoot"); }
            if (mouseAccess == null) { throw new ArgumentNullException("mouseAccess"); }
            if (this.mouseAccess != null) { throw new UIException("Mouse input device already registered!"); }

            this.mouseAccess = mouseAccess;
            TraceManager.WriteAllTrace("Mouse input device registered", UITraceFilters.INFO);
        }

        /// <summary>
        /// Unregisters the currently active mouse input device from the UIRoot object.
        /// </summary>
        /// <exception cref="UIException">If there is no registered mouse input device.</exception>
        /// <remarks>This method should be called by the plugin that implements the mouse input device.</remarks>
        public void UnregisterMouseInputDevice()
        {
            if (this.objectDisposed) { throw new ObjectDisposedException("UIRoot"); }
            if (this.mouseAccess == null) { throw new UIException("There is no registered mouse input device!"); }

            this.mouseAccess = null;
            TraceManager.WriteAllTrace("Mouse input device unregistered", UITraceFilters.INFO);
        }

        /// <summary>
        /// Registers a keyboard input device to the UIRoot object.
        /// </summary>
        /// <param name="keyboardAccess">The keyboard input device to register.</param>
        /// <exception cref="UIException">If a keyboard input device has already been registered.</exception>
        /// <remarks>This method should be called by the plugin that implements the keyboard input device.</remarks>
        public void RegisterKeyboardInputDevice(IUIKeyboardAccess keyboardAccess)
        {
            if (this.objectDisposed) { throw new ObjectDisposedException("UIRoot"); }
            if (keyboardAccess == null) { throw new ArgumentNullException("keyboardAccess"); }
            if (this.keyboardAccess != null) { throw new UIException("Keyboard input device already registered!"); }

            this.keyboardAccess = keyboardAccess;
            TraceManager.WriteAllTrace("Keyboard input device registered", UITraceFilters.INFO);
        }

        /// <summary>
        /// Unregisters the currently active keyboard input device from the UIRoot object.
        /// </summary>
        /// <exception cref="UIException">If there is no registered keyboard input device.</exception>
        /// <remarks>This method should be called by the plugin that implements the keyboard input device.</remarks>
        public void UnregisterKeyboardInputDevice()
        {
            if (this.objectDisposed) { throw new ObjectDisposedException("UIRoot"); }
            if (this.keyboardAccess == null) { throw new UIException("There is no registered keyboard input device!"); }

            this.keyboardAccess = null;
            TraceManager.WriteAllTrace("Keyboard input device unregistered", UITraceFilters.INFO);
        }

        #endregion Methods for plugins

        #region IDisposable members

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            if (this.objectDisposed) { throw new ObjectDisposedException("UIRoot"); }

            /// Uninstall and unload all plugins
            UnloadAllPlugins();

            this.objectDisposed = true;
            theInstance = null;
            TraceManager.WriteAllTrace("UIRoot.Instance destroyed", UITraceFilters.INFO);
        }

        #endregion IDisposable members

        /// <summary>
        /// Reference to the currently active graphics platform.
        /// </summary>
        private UIGraphicsPlatformBase graphicsPlatform;

        /// <summary>
        /// List of the loaded plugins mapped by their name.
        /// </summary>
        private Dictionary<string, IUIPlugin> loadedPlugins;

        /// <summary>
        /// Reference to the interface for accessing the current state of the mouse input device.
        /// </summary>
        private IUIMouseAccess mouseAccess;

        /// <summary>
        /// Reference to the interface for accessing the current state of the keyboard input device.
        /// </summary>
        private IUIKeyboardAccess keyboardAccess;

        /// <summary>
        /// This flag indicates whether this UIRoot object is disposed or not.
        /// </summary>
        private bool objectDisposed;

        /// <summary>
        /// Reference to the singleton instance of the UIRoot class.
        /// </summary>
        private static UIRoot theInstance = null;
    }
}
