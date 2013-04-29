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
        /// Registers the component with the given name from the given assembly.
        /// </summary>
        /// <param name="assembly">The full name of the assembly where the component is implemented.</param>
        /// <param name="name">The name of the component to register.</param>
        public static void RegisterComponent(string assembly, string name)
        {
            if (assembly == null) { throw new ArgumentNullException("assembly"); }
            if (name == null) { throw new ArgumentNullException("name"); }
            if (componentsAreRunning) { throw new InvalidOperationException("The registered components are currently running!"); }
            if (registeredComponents.ContainsKey(name)) { throw new InvalidOperationException(string.Format("Component with name '{0}' has already been registered!", name)); }

            registeredComponents.Add(name, assembly);
            if (!componentAssemblies.ContainsKey(assembly)) { componentAssemblies.Add(assembly, new HashSet<string>()); }
            componentAssemblies[assembly].Add(name);
        }

        /// <summary>
        /// Registers the components with the given names from the given assembly.
        /// </summary>
        /// <param name="assembly">The full name of the assembly.</param>
        /// <param name="names">The names of the components to register.</param>
        public static void RegisterComponents(string assembly, string[] names)
        {
            foreach (string name in names) { RegisterComponent(assembly, name); }
        }

        /// <summary>
        /// Unregisters every components.
        /// </summary>
        public static void UnregisterComponents()
        {
            if (componentsAreRunning) { throw new InvalidOperationException("The registered components are currently running!"); }

            registeredComponents.Clear();
            componentAssemblies.Clear();
        }

        /// <summary>
        /// Starts every registered component.
        /// </summary>
        public static void StartComponents()
        {
            if (componentsAreRunning) { throw new InvalidOperationException("The registered components are currently running!"); }
            if (registeredComponents.Count == 0) { throw new InvalidOperationException("No components were registered!"); }

            /// Start every registered components.
            foreach (KeyValuePair<string, HashSet<string>> compAssembly in componentAssemblies)
            {
                StartComponentsFromAssembly(compAssembly.Key, compAssembly.Value);
            }

            /// Set the references between the components.
            foreach (object component in startedComponents.Values)
            {
                Type compType = component.GetType();
                FieldInfo[] fields = compType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (FieldInfo field in fields)
                {
                    ComponentReferenceAttribute[] refAttr = field.GetCustomAttributes(typeof(ComponentReferenceAttribute), false) as ComponentReferenceAttribute[];
                    if (refAttr == null || refAttr.Length != 1) { continue; }

                    if (componentIfaces.ContainsKey(field.FieldType))
                    {
                        field.SetValue(component, componentIfaces[field.FieldType]);
                    }
                }
            }

            /// Call Start on components implementing the IComponentStart interface.
            foreach (object component in startedComponents.Values)
            {
                IComponentStart compStart = component as IComponentStart;
                if (compStart != null) { compStart.Start(); }
            }

            componentsAreRunning = true;
        }

        /// <summary>
        /// Stops every registered component.
        /// </summary>
        public static void StopComponents()
        {
            if (!componentsAreRunning) { throw new InvalidOperationException("The registered components are currently stopped!"); }

            /// Remove the references between the components.
            foreach (object component in startedComponents.Values)
            {
                Type compType = component.GetType();
                FieldInfo[] fields = compType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (FieldInfo field in fields)
                {
                    ComponentReferenceAttribute[] compRefAttr = field.GetCustomAttributes(typeof(ComponentReferenceAttribute), false) as ComponentReferenceAttribute[];
                    CallbackReferenceAttribute[] callbackRefAttr = field.GetCustomAttributes(typeof(CallbackReferenceAttribute), false) as CallbackReferenceAttribute[];
                    if ((compRefAttr != null && compRefAttr.Length == 1) ||
                        (callbackRefAttr != null && callbackRefAttr.Length == 1))
                    {
                        field.SetValue(component, null);
                    }
                }
            }

            /// Call Dispose on components implementing the IDisposable interface.
            foreach (object component in startedComponents.Values)
            {
                IDisposable disposableComp = component as IDisposable;
                if (disposableComp != null) { disposableComp.Dispose(); }
            }

            /// Clear the lists.
            startedComponents.Clear();
            componentIfaces.Clear();

            componentsAreRunning = false;
        }

        /// <summary>
        /// Gets a reference to the component that implements the interface given in the type parameter.
        /// </summary>
        /// <typeparam name="T">The interface to get reference to.</typeparam>
        /// <returns>A reference to the implementing component or null if no such component is running.</returns>
        public static T GetInterface<T>() where T : class
        {
            if (!componentsAreRunning) { throw new InvalidOperationException("The registered components are currently stopped!"); }

            Type ifaceType = typeof(T);
            return componentIfaces.ContainsKey(ifaceType) ? (T)componentIfaces[ifaceType] : null;
        }

        /// <summary>
        /// Connects the given callback object to the component that implements the interface given
        /// in the type parameter.
        /// </summary>
        /// <typeparam name="T">The interface of the component.</typeparam>
        /// <param name="targetObj">The callback object to connect.</param>
        public static void ConnectToComponent<T>(object targetObj) where T : class
        {
            if (!componentsAreRunning) { throw new InvalidOperationException("The registered components are currently stopped!"); }
            if (targetObj == null) { throw new ArgumentNullException("targetObj"); }

            Type ifaceType = typeof(T);
            if (!componentIfaces.ContainsKey(ifaceType)) { throw new InvalidOperationException(string.Format("Component with interface '{0}' doesn't exist!", ifaceType.FullName)); }

            object component = componentIfaces[ifaceType];
            Type compType = component.GetType();
            FieldInfo[] fields = compType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (FieldInfo field in fields)
            {
                CallbackReferenceAttribute[] refAttr = field.GetCustomAttributes(typeof(CallbackReferenceAttribute), false) as CallbackReferenceAttribute[];
                if (refAttr == null || refAttr.Length != 1) { continue; }

                Type fieldType = field.FieldType;
                if (!fieldType.IsInterface) { continue; }
                CallbackInterfaceAttribute[] callbackIfaceAttr = fieldType.GetCustomAttributes(typeof(CallbackInterfaceAttribute), false) as CallbackInterfaceAttribute[];
                if (callbackIfaceAttr == null || callbackIfaceAttr.Length != 1) { continue; }

                if (fieldType.IsAssignableFrom(targetObj.GetType()) && field.GetValue(component) == null)
                {
                    field.SetValue(component, targetObj);
                }
            }
        }

        /// <summary>
        /// Disconnects the given callback object from the component that implements the interface given
        /// in the type parameter.
        /// </summary>
        /// <typeparam name="T">The interface of the component.</typeparam>
        /// <param name="targetObj">The callback object to disconnect.</param>
        public static void DisconnectFromComponent<T>(object targetObj) where T : class
        {
            if (!componentsAreRunning) { throw new InvalidOperationException("The registered components are currently stopped!"); }
            if (targetObj == null) { throw new ArgumentNullException("targetObj"); }

            Type ifaceType = typeof(T);
            if (!componentIfaces.ContainsKey(ifaceType)) { throw new InvalidOperationException(string.Format("Component with interface '{0}' doesn't exist!", ifaceType.FullName)); }

            object component = componentIfaces[ifaceType];
            Type compType = component.GetType();
            FieldInfo[] fields = compType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (FieldInfo field in fields)
            {
                CallbackReferenceAttribute[] refAttr = field.GetCustomAttributes(typeof(CallbackReferenceAttribute), false) as CallbackReferenceAttribute[];
                if (refAttr == null || refAttr.Length != 1) { continue; }

                if (field.GetValue(component) == targetObj) { field.SetValue(component, null); }
            }
        }

        /// <summary>
        /// Starts the given components from the given assembly.
        /// </summary>
        /// <param name="assembly">The assembly of the components.</param>
        /// <param name="components">The components to start.</param>
        private static void StartComponentsFromAssembly(string assembly, HashSet<string> components)
        {
            TraceManager.WriteAllTrace(string.Format("Starting components of assembly '{0}'.", assembly), ComponentManager.COMPONENT_MGR_INFO);

            Assembly asm = Assembly.Load(assembly);
            if (asm != null)
            {
                Type[] types = asm.GetTypes();
                foreach (Type type in types)
                {
                    ComponentAttribute compAttr = GetComponentAttribute(type);
                    if (compAttr == null) { continue; }
                    if (startedComponents.ContainsKey(compAttr.Name)) { continue; }

                    if (components.Contains(compAttr.Name)) { CreateComponentInstance(compAttr.Name, type); }
                }
            }
            else
            {
                throw new ComponentModelException(string.Format("Unable to load assembly '{0}'!", assembly));
            }
        }

        /// <summary>
        /// Creates and instance of the given component type if it implements at least one component interface.
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
                    if (componentIfaces.ContainsKey(iface)) { throw new InvalidOperationException(string.Format("Interface '{0}' is already implemented by another component!", iface.FullName)); }
                    compInterfaces.Add(iface);
                }
            }

            /// Instantiate the component and save it's reference.
            if (compInterfaces.Count == 0) { throw new InvalidOperationException(string.Format("Component '{0}' doesn't implement any component interfaces!", name)); }
            object componentInstance = Activator.CreateInstance(type);
            foreach (Type iface in compInterfaces)
            {
                componentIfaces.Add(iface, componentInstance);
            }
            startedComponents.Add(name, componentInstance);
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
        /// ID of the RC.Common.ComponentMgr.Info trace filter.
        /// </summary>
        public static readonly int COMPONENT_MGR_INFO = TraceManager.GetTraceFilterID("RC.Common.ComponentMgr.Info");

        /// <summary>
        /// This flag indicates whether the registered components are running or not.
        /// </summary>
        private static bool componentsAreRunning = false;

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
        /// List of the started components mapped by their name.
        /// </summary>
        private static Dictionary<string, object> startedComponents = new Dictionary<string, object>();

        /// <summary>
        /// List of the started components mapped by the component interfaces they implement.
        /// </summary>
        private static Dictionary<Type, object> componentIfaces = new Dictionary<Type, object>();
    }
}
