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
    /// This attribute is be used to indicate callback interfaces.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class CallbackInterfaceAttribute : Attribute { }

    /// <summary>
    /// This attribute is be used to indicate a reference from a component to another.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class ComponentReferenceAttribute : Attribute { }

    /// <summary>
    /// This attribute is be used to indicate a reference from a component to a callback reference.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class CallbackReferenceAttribute : Attribute { }

    /// <summary>
    /// This attribute is used to indicate component classes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ComponentAttribute : Attribute
    {
        /// <summary>
        /// Creates a ComponentAttribute instance.
        /// </summary>
        /// <param name="name">The name of the component class.</param>
        public ComponentAttribute(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Gets the name of the component classes.
        /// </summary>
        public string Name { get { return this.name; } }

        /// <summary>
        /// The name of the component class.
        /// </summary>
        private string name;
    }
}
