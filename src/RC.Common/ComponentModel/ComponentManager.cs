using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.Diagnostics;
using System.Reflection;

namespace RC.Common.ComponentModel
{
    /// <summary>
    /// The component manager that is used to load or access components.
    /// </summary>
    public static class ComponentManager
    {
        /// <summary>
        /// Registers the components with the given names from the given assembly.
        /// </summary>
        /// <param name="assembly">The full name of the assembly.</param>
        /// <param name="names">The names of the components to register.</param>
        public static void RegisterComponents(string assembly, string[] names)
        {
            if (onlyInterfaceQueryIsAllowed) { throw new ComponentModelException("Only the ComponentManager.GetInterface method is allowed to be called at this time!"); }
            if (assembly == null) { throw new ArgumentNullException("assembly"); }
            if (names == null) { throw new ArgumentNullException("names"); }
            if (componentsAreRunning) { throw new ComponentModelException("The registered components are currently running!"); }

            foreach (string name in names)
            {
                if (registeredComponents.ContainsKey(name)) { throw new ComponentModelException(string.Format("Component with name '{0}' has already been registered!", name)); }

                registeredComponents.Add(name, assembly);
                if (!componentAssemblies.ContainsKey(assembly)) { componentAssemblies.Add(assembly, new HashSet<string>()); }
                componentAssemblies[assembly].Add(name);
            }
        }

        /// <summary>
        /// Registers the given assembly where plugins for the running components will be searched.
        /// </summary>
        /// <param name="assembly">The full name of the assembly where the plugins are implemented.</param>
        public static void RegisterPluginAssembly(string assembly)
        {
            if (onlyInterfaceQueryIsAllowed) { throw new ComponentModelException("Only the ComponentManager.GetInterface method is allowed to be called at this time!"); }
            if (assembly == null) { throw new ArgumentNullException("assembly"); }
            if (componentsAreRunning) { throw new ComponentModelException("The registered components are currently running!"); }
            if (pluginAssemblies.Contains(assembly)) { throw new ComponentModelException(string.Format("Plugin assembly with name '{0}' has already been registered!", assembly)); }

            pluginAssemblies.Add(assembly);
        }

        /// <summary>
        /// Unregisters every components and plugins.
        /// </summary>
        public static void UnregisterComponentsAndPlugins()
        {
            if (onlyInterfaceQueryIsAllowed) { throw new ComponentModelException("Only the ComponentManager.GetInterface method is allowed to be called at this time!"); }
            if (componentsAreRunning) { throw new ComponentModelException("The registered components are currently running!"); }

            registeredComponents.Clear();
            componentAssemblies.Clear();
            pluginAssemblies.Clear();
        }

        /// <summary>
        /// Starts every registered component and their corresponding plugins. The process is the following:
        ///     - Instantiate every registered components.
        ///     - Instantiate every plugins found in the registered assemblies for the started components.
        ///     - Call IPlugin.Install on plugins implementing one or more IPlugin generic interfaces.
        ///     - Call IComponent.Start method on components implementing the IComponent interface.
        /// </summary>
        public static void StartComponents()
        {
            if (onlyInterfaceQueryIsAllowed) { throw new ComponentModelException("Only the ComponentManager.GetInterface method is allowed to be called at this time!"); }
            if (componentsAreRunning) { throw new ComponentModelException("The registered components are currently running!"); }
            if (registeredComponents.Count == 0) { throw new ComponentModelException("No components were registered!"); }

            onlyInterfaceQueryIsAllowed = true;

            /// Instantiate every registered components.
            foreach (KeyValuePair<string, HashSet<string>> compAssembly in componentAssemblies)
            {
                try
                {
                    CreateComponentsFromAssembly(compAssembly.Key, compAssembly.Value);
                }
                catch (Exception ex)
                {
                    TraceManager.WriteAllTrace(string.Format("Creating components of assembly '{0}' failed! Exception: {1}", compAssembly.Key, ex), ComponentManager.COMPONENT_MGR_INFO);
                }
            }

            /// Instantiate every plugins found in the registered assemblies for the started components.
            foreach (string pluginAssembly in pluginAssemblies)
            {
                try
                {
                    CreatePluginsFromAssembly(pluginAssembly);
                }
                catch (Exception ex)
                {
                    TraceManager.WriteAllTrace(string.Format("Creating plugins of assembly '{0}' failed! Exception: {1}", pluginAssembly, ex), ComponentManager.COMPONENT_MGR_INFO);
                }
            }

            /// Call IPlugin.Install on plugins implementing the IPlugin interface.
            foreach (KeyValuePair<Type, HashSet<object>> pluginsOfComponent in createdPlugins)
            {
                foreach (object plugin in pluginsOfComponent.Value)
                {
                    try
                    {
                        InstallOrUninstallPlugin(plugin, componentIfaces[pluginsOfComponent.Key], true);
                    }
                    catch (Exception ex)
                    {
                        TraceManager.WriteAllTrace(string.Format("Installing a plugin failed! Exception: {0}", ex), ComponentManager.COMPONENT_MGR_INFO);
                    }
                }
            }

            /// Set the running flag.
            componentsAreRunning = true;

            /// Call IComponent.Start on components implementing the IComponent interface.
            foreach (object component in createdComponents.Values)
            {
                try
                {
                    IComponent compStart = component as IComponent;
                    if (compStart != null) { compStart.Start(); }
                }
                catch (Exception ex)
                {
                    TraceManager.WriteAllTrace(string.Format("Starting a component failed! Exception: {0}", ex), ComponentManager.COMPONENT_MGR_INFO);
                }
            }

            onlyInterfaceQueryIsAllowed = false;
        }

        /// <summary>
        /// Stops every registered component. The process is the following:
        ///     - Call IComponent.Stop method on components implementing the IComponent interface.
        ///     - Call IPlugin.Uninstall on plugins implementing one or more IPlugin generic interfaces.
        ///     - Call IDisposable.Dispose on components implementing the IDisposable interface.
        /// </summary>
        public static void StopComponents()
        {
            if (onlyInterfaceQueryIsAllowed) { throw new ComponentModelException("Only the ComponentManager.GetInterface method is allowed to be called at this time!"); }
            if (!componentsAreRunning) { throw new ComponentModelException("The registered components are currently stopped!"); }

            onlyInterfaceQueryIsAllowed = true;

            /// Call IComponent.Stop on components implementing the IComponent interface.
            foreach (object component in createdComponents.Values)
            {
                try
                {
                    IComponent compStart = component as IComponent;
                    if (compStart != null) { compStart.Stop(); }
                }
                catch (Exception ex)
                {
                    TraceManager.WriteAllTrace(string.Format("Stopping a component failed! Exception: {0}", ex), ComponentManager.COMPONENT_MGR_INFO);
                }
            }

            /// Reset the running flag.
            componentsAreRunning = false;

            /// Call IPlugin.Uninstall on plugins implementing the IPlugin interface.
            foreach (KeyValuePair<Type, HashSet<object>> pluginsOfComponent in createdPlugins)
            {
                foreach (object plugin in pluginsOfComponent.Value)
                {
                    try
                    {
                        InstallOrUninstallPlugin(plugin, componentIfaces[pluginsOfComponent.Key], false);
                    }
                    catch (Exception ex)
                    {
                        TraceManager.WriteAllTrace(string.Format("Uninstalling a plugin failed! Exception: {0}", ex), ComponentManager.COMPONENT_MGR_INFO);
                    }
                }
            }

            /// Dispose every registered components implementing the IDisposable interface.
            foreach (object component in createdComponents.Values)
            {
                try
                {
                    IDisposable disposableComp = component as IDisposable;
                    if (disposableComp != null) { disposableComp.Dispose(); }
                }
                catch (Exception ex)
                {
                    TraceManager.WriteAllTrace(string.Format("Disposing a component failed! Exception: {0}", ex), ComponentManager.COMPONENT_MGR_INFO);
                }
            }

            /// Clear the lists.
            createdComponents.Clear();
            componentIfaces.Clear();
            createdPlugins.Clear();

            onlyInterfaceQueryIsAllowed = false;
        }

        /// <summary>
        /// Gets a reference to the component that implements the interface given in the type parameter.
        /// </summary>
        /// <typeparam name="T">The interface to get reference to.</typeparam>
        /// <returns>A reference to the implementing component or null if no such component is running.</returns>
        public static T GetInterface<T>() where T : class
        {
            if (!componentsAreRunning) { throw new ComponentModelException("The registered components are currently stopped!"); }

            Type ifaceType = typeof(T);
            return componentIfaces.ContainsKey(ifaceType) ? (T)componentIfaces[ifaceType] : null;
        }

        /// <summary>
        /// Creates the given components from the given assembly.
        /// </summary>
        /// <param name="assembly">The assembly of the components.</param>
        /// <param name="components">The components to create.</param>
        private static void CreateComponentsFromAssembly(string assembly, HashSet<string> components)
        {
            TraceManager.WriteAllTrace(string.Format("Creating components of assembly '{0}'.", assembly), ComponentManager.COMPONENT_MGR_INFO);

            Assembly asm = Assembly.Load(assembly);
            if (asm != null)
            {
                Type[] types = asm.GetTypes();
                foreach (Type type in types)
                {
                    ComponentAttribute compAttr = GetComponentAttribute(type);
                    if (compAttr == null) { continue; }
                    if (createdComponents.ContainsKey(compAttr.Name)) { continue; }

                    if (components.Contains(compAttr.Name)) { CreateComponentInstance(compAttr.Name, type); }
                }
            }
            else
            {
                throw new ComponentModelException(string.Format("Unable to load assembly '{0}'!", assembly));
            }
        }

        /// <summary>
        /// Creates every plugins of the existing components from the given assembly.
        /// </summary>
        /// <param name="assembly">The assembly of the plugins.</param>
        private static void CreatePluginsFromAssembly(string assembly)
        {
            TraceManager.WriteAllTrace(string.Format("Creating plugins of assembly '{0}'.", assembly), ComponentManager.COMPONENT_MGR_INFO);

            Assembly asm = Assembly.Load(assembly);
            if (asm != null)
            {
                Type[] types = asm.GetTypes();
                foreach (Type type in types)
                {
                    PluginAttribute pluginAttr = GetPluginAttribute(type);
                    if (pluginAttr == null) { continue; }
                    if (!componentIfaces.ContainsKey(pluginAttr.ComponentInterface)) { continue; }

                    object pluginInstance = Activator.CreateInstance(type);
                    if (!createdPlugins.ContainsKey(pluginAttr.ComponentInterface)) { createdPlugins.Add(pluginAttr.ComponentInterface, new HashSet<object>()); }
                    createdPlugins[pluginAttr.ComponentInterface].Add(pluginInstance);
                }
            }
            else
            {
                throw new ComponentModelException(string.Format("Unable to load assembly '{0}'!", assembly));
            }

        }

        /// <summary>
        /// Creates an instance of the given component type if it implements at least one component interface.
        /// </summary>
        /// <param name="name">The name of the component.</param>
        /// <param name="type">The type to instantiate.</param>
        private static void CreateComponentInstance(string name, Type type)
        {
            Type[] interfaces = type.GetInterfaces();
            List<Type> compInterfaces = new List<Type>();

            /// Collect the implemented component interfaces.
            foreach (Type iface in interfaces)
            {
                ComponentInterfaceAttribute[] compIfaceAttr = iface.GetCustomAttributes(typeof(ComponentInterfaceAttribute), false) as ComponentInterfaceAttribute[];
                if (compIfaceAttr != null && compIfaceAttr.Length == 1)
                {
                    if (componentIfaces.ContainsKey(iface)) { throw new ComponentModelException(string.Format("Interface '{0}' is already implemented by another component!", iface.FullName)); }
                    compInterfaces.Add(iface);
                }
            }

            /// Instantiate the component and save it's reference.
            if (compInterfaces.Count == 0) { throw new ComponentModelException(string.Format("Component '{0}' doesn't implement any component interfaces!", name)); }
            object componentInstance = Activator.CreateInstance(type);
            foreach (Type iface in compInterfaces)
            {
                componentIfaces.Add(iface, componentInstance);
            }
            createdComponents.Add(name, componentInstance);
        }

        /// <summary>
        /// Call the appropriate Install methods on the given plugin.
        /// </summary>
        /// <param name="plugin">The plugin to install.</param>
        /// <param name="extendedComponent">The component that is extended by the plugin.</param>
        /// <param name="install">True in case of installation, false in case of uninstallation.</param>
        private static void InstallOrUninstallPlugin(object plugin, object extendedComponent, bool install)
        {
            Type[] pluginIfaces = plugin.GetType().GetInterfaces();
            foreach (Type pluginIface in pluginIfaces)
            {
                if (!pluginIface.IsGenericType || pluginIface.IsGenericTypeDefinition || pluginIface.ContainsGenericParameters) { continue; }
                if (pluginIface.GetGenericTypeDefinition() != typeof(IPlugin<>)) { continue; }

                Type[] installInterface = pluginIface.GetGenericArguments();
                if (installInterface.Length != 1) { throw new ComponentModelException("IPlugin<T> interface must have exactly 1 type parameter!"); }
                if (!installInterface[0].IsInterface) { throw new ComponentModelException("IPlugin<T> interface must have an interface type parameter!"); }
                
                PluginInstallInterfaceAttribute pluginInstallIfaceAttr = GetPluginInstallInterfaceAttribute(installInterface[0]);
                if (pluginInstallIfaceAttr == null) { throw new ComponentModelException(string.Format("Interface '{0}' must have a PluginInstallInterface attribute!", installInterface[0].FullName)); }

                /// Check if the extended component implements the plugin install interface.
                bool ifaceFoundOnComponent = false;
                foreach (Type compIface in extendedComponent.GetType().GetInterfaces())
                {
                    if (installInterface[0] == compIface)
                    {
                        ifaceFoundOnComponent = true;
                        break;
                    }
                }
                if (!ifaceFoundOnComponent) { throw new ComponentModelException(string.Format("The component doesn't implement plugin install interface '{0}'!", installInterface[0].FullName)); }

                /// Call the appropriate method on the plugin.
                MethodInfo methodToCall = pluginIface.GetMethod(install ? "Install" : "Uninstall");
                methodToCall.Invoke(plugin, new object[1] { extendedComponent });
            }
        }

        /// <summary>
        /// Gets the component attribute of the given type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>The component attribute of the given type or null if the given type is not a component.</returns>
        private static ComponentAttribute GetComponentAttribute(Type type)
        {
            if (!type.IsClass) { return null; }
            if (type.IsAbstract) { return null; }

            ComponentAttribute[] compAttr = type.GetCustomAttributes(typeof(ComponentAttribute), false) as ComponentAttribute[];
            if (compAttr == null || compAttr.Length != 1) { return null; }

            if (type.GetConstructor(new Type[0] { }) == null) { return null; }
            return compAttr[0];
        }

        /// <summary>
        /// Gets the plugin attribute of the given type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>The plugin attribute of the given type or null if the given type is not a plugin.</returns>
        private static PluginAttribute GetPluginAttribute(Type type)
        {
            if (!type.IsClass) { return null; }
            if (type.IsAbstract) { return null; }

            PluginAttribute[] pluginAttr = type.GetCustomAttributes(typeof(PluginAttribute), false) as PluginAttribute[];
            if (pluginAttr == null || pluginAttr.Length != 1) { return null; }

            if (type.GetConstructor(new Type[0] { }) == null) { return null; }
            return pluginAttr[0];
        }

        /// <summary>
        /// Gets the plugin install interface attribute of the given type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>
        /// The plugin install interface attribute of the given type or null if the given type is not a plugin
        /// install interface.
        /// </returns>
        private static PluginInstallInterfaceAttribute GetPluginInstallInterfaceAttribute(Type type)
        {
            PluginInstallInterfaceAttribute[] pluginInstallIfaceAttr = type.GetCustomAttributes(typeof(PluginInstallInterfaceAttribute), false) as PluginInstallInterfaceAttribute[];
            if (pluginInstallIfaceAttr == null || pluginInstallIfaceAttr.Length != 1) { return null; }
            return pluginInstallIfaceAttr[0];
        }

        /// <summary>
        /// ID of the RC.Common.ComponentMgr.Info trace filter.
        /// </summary>
        private static readonly int COMPONENT_MGR_INFO = TraceManager.GetTraceFilterID("RC.Common.ComponentMgr.Info");

        /// <summary>
        /// This flag indicates whether the registered components are running or not.
        /// </summary>
        private static bool componentsAreRunning = false;

        /// <summary>
        /// While this flag is true only the ComponentManager.GetInterface method is allowed to be called.
        /// This prevents recursive calls on the other methods.
        /// </summary>
        private static bool onlyInterfaceQueryIsAllowed = false;

        /// <summary>
        /// List of the registered components. The keys of this dictionary are the names of the components, the corresponding values are
        /// the full name of the assembly where the given component is implemented.
        /// </summary>
        private static Dictionary<string, string> registeredComponents = new Dictionary<string, string>();

        /// <summary>
        /// List of the registered components groupped by the assemblies where they are implemented.
        /// </summary>
        private static Dictionary<string, HashSet<string>> componentAssemblies = new Dictionary<string, HashSet<string>>();

        /// <summary>
        /// List of the registered plugin assemblies.
        /// </summary>
        private static HashSet<string> pluginAssemblies = new HashSet<string>();

        /// <summary>
        /// List of the created components mapped by their name.
        /// </summary>
        private static Dictionary<string, object> createdComponents = new Dictionary<string, object>();

        /// <summary>
        /// List of the created plugins mapped by the component interfaces they extend.
        /// </summary>
        private static Dictionary<Type, HashSet<object>> createdPlugins = new Dictionary<Type, HashSet<object>>();

        /// <summary>
        /// List of the created components mapped by the component interfaces they implement.
        /// </summary>
        private static Dictionary<Type, object> componentIfaces = new Dictionary<Type, object>();
    }
}
