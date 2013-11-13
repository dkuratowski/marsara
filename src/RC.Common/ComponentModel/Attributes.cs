using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Common.ComponentModel
{
    /// <summary>
    /// This attribute is be used to indicate component interfaces.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class ComponentInterfaceAttribute : Attribute { }

    /// <summary>
    /// This attribute is be used to indicate plugin install interfaces.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class PluginInstallInterfaceAttribute : Attribute { }

    /// <summary>
    /// This attribute is used to indicate component classes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ComponentAttribute : Attribute
    {
        /// <summary>
        /// Creates a ComponentAttribute instance.
        /// </summary>
        /// <param name="name">The name of the component.</param>
        public ComponentAttribute(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Gets the name of the component.
        /// </summary>
        public string Name { get { return this.name; } }

        /// <summary>
        /// The name of the component.
        /// </summary>
        private string name;
    }

    /// <summary>
    /// This attribute is used to indicate plugin classes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PluginAttribute : Attribute
    {
        /// <summary>
        /// Creates a PluginAttribute instance.
        /// </summary>
        /// <param name="componentInterface">The interface of the component that the plugin extends.</param>
        public PluginAttribute(Type componentInterface)
        {
            this.componentInterface = componentInterface;
        }

        /// <summary>
        /// Gets the interface of the component that the plugin extends.
        /// </summary>
        public Type ComponentInterface { get { return this.componentInterface; } }

        /// <summary>
        /// The interface of the component that the plugin extends.
        /// </summary>
        private Type componentInterface;
    }
}
